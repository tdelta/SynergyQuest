import sys
import pygame
import time

from generator import Generator
from state_space import StateSpace
from utils import Color

IMPASSABLE_OBJECTS = ['#', 'B_d', 'B', 'BW', 'RW', 'BW_d', 'RW_d', 'RB', 'BB', 'RB_d', 'BB_d']


class Entity:

    def __init__(self, x, y):
        self.x = x
        self.y = y

    def render(self, grid):
        """
        @param grid the map onto which to render the entity
        @return map with entity
        """
        pass

    def can_move(self, x, y, level_map):
        return level_map.width() > self.x + x >= 0 and \
               level_map.height() > self.y + y >= 0 and \
               level_map.render()[self.y + y][self.x + x] not in IMPASSABLE_OBJECTS


class Worker(Entity):
    def __init__(self, x, y, color):
        super().__init__(x, y)
        self.color = color

    def render(self, grid):
        if grid[self.y][self.x] == 'd':
            # Worker is on dock
            grid[self.y][self.x] = 'BW_d' if self.color == Color.BLUE else 'RW_d'
        else:
            grid[self.y][self.x] = 'BW' if self.color == Color.BLUE else 'RW'
        return grid

    def can_push(self, x, y, level_map):
        box = level_map.get_box(self.x + x, self.y + y)
        if box is None:
            return False, None
        if not box.color == self.color:
            return False, box
        else:
            return box.can_move(x, y, level_map), box

    def can_pull(self, x, y, map):
        # Get box on opposite site of moving direction
        box = map.get_box(self.x - x, self.y - y)
        if box is None:
            return False, None
        elif not box.color == self.color:
            return False, None
        else:
            return self.can_move(x, y, map), box

    def move(self, x, y, level_map):
        if self.can_move(x, y, level_map):
            self.x += x
            self.y += y
            level_map.last_render_valid = False
            return True
        return False

    def push(self, x, y, level_map):
        can_push, box = self.can_push(x, y, level_map)
        if can_push:
            box.x += x
            box.y += y
            level_map.last_render_valid = False
            self.move(x, y, level_map)
            level_map.last_render_valid = False

    def pull(self, x, y, level_map):
        can_pull, box = self.can_pull(x, y, level_map)
        if can_pull:
            box.x += x
            box.y += y
            level_map.last_render_valid = False
            self.move(x, y, level_map)
            level_map.last_render_valid = False


class Box(Entity):
    def __init__(self, x, y, color):
        super().__init__(x, y)
        self.color = color

    def render(self, grid):
        if grid[self.y][self.x] == 'd':
            # Box is on dock
            grid[self.y][self.x] = 'B_d'
        else:
            grid[self.y][self.x] = 'BB' if self.color == Color.BLUE else 'RB'
        return grid


class Map:
    def __init__(self):
        self.grid = []
        self.current_x = 0
        self.current_y = -1
        self.entities = []
        self.workers = {}

        self.last_render = None
        self.last_render_valid = False

    def __eq__(self, other):
        return other is not None and isinstance(other, Map) and other.__hash__() == self.__hash__()

    def __hash__(self):
        return hash("".join("".join(r) for r in self.render()))

    def __deepcopy__(self, memodict=None):
        copy = Map()
        copy.grid = [x.copy() for x in self.grid]
        copy.current_x = self.current_x
        copy.current_y = self.current_y
        for entity in self.entities:
            if isinstance(entity, Box):
                copy.entities.append(Box(entity.x, entity.y, entity.color))
            elif isinstance(entity, Worker):
                entity_copy = Worker(entity.x, entity.y, entity.color)
                copy.entities.append(entity_copy)
                copy.workers[entity_copy.color] = entity_copy
        return copy

    def add_floor(self):
        self.grid[-1].append(' ')
        self.current_x += 1
        self.last_render_valid = False

    def add_wall(self):
        self.grid[-1].append('#')
        self.current_x += 1
        self.last_render_valid = False

    def add_dock(self):
        self.grid[-1].append('d')
        self.current_x += 1
        self.last_render_valid = False

    def next_row(self):
        self.grid.append([])
        self.current_x = 0
        self.current_y += 1
        self.last_render_valid = False

    def add_worker(self, color):
        self.entities.append(Worker(self.current_x, self.current_y, color))
        self.workers[color] = self.entities[-1]
        self.last_render_valid = False

    def add_box(self, color):
        self.entities.append(Box(self.current_x, self.current_y, color))
        self.last_render_valid = False

    def render(self):
        if not self.last_render_valid:
            self.last_render = [x.copy() for x in self.grid]
            for entity in self.entities:
                self.last_render = entity.render(self.last_render)

            self.last_render_valid = True

        return self.last_render

    def is_completed(self):
        for entity in self.entities:
            if isinstance(entity, Box):
                if not self.grid[entity.y][entity.x] == 'd':
                    # A box is not yet on a dock
                    return False
        # All boxes are on docks
        return True

    def get_worker(self, color):
        return self.workers[color]

    def get_box(self, x, y):
        for entity in self.entities:
            if isinstance(entity, Box):
                if entity.x == x and entity.y == y:
                    # Box found at y,x
                    return entity
        # No box found
        return None

    def height(self):
        # TODO: This implementation assumes that x and y are not changed
        # after the initial iteration through the map
        return self.current_y + 1

    def width(self):
        return self.current_x


def is_valid_value(char):
    if (char == ' ' or  # floor
            char == '#' or  # wall
            char == 'BW' or  # blue worker on floor
            char == 'RW' or  # red worker on floor
            char == 'd' or  # dock
            char == 'B_d' or  # box on dock
            char == 'BB_d' or  # blue box on dock
            char == 'RB_d' or  # red box on dock
            char == 'B' or  # box on floor
            char == 'RB' or  # blue box on floor
            char == 'BB' or  # red box on floor
            char == 'BW_d' or  # blue worker on dock
            char == 'RW_d'):  # red worker on dock
        return True
    else:
        return False


class Game:

    # TODO: Remove legacy entities without color
    def parse_char(self, char):
        if char == ' ':
            self.map.add_floor()
        elif char == '#':
            self.map.add_wall()
        elif char == 'BW':
            self.map.add_worker(Color.BLUE)
            self.map.add_floor()
        elif char == 'RW':
            self.map.add_worker(Color.RED)
            self.map.add_floor()
        elif char == 'd':
            self.map.add_dock()
        elif char == 'B_d':
            self.map.add_box(Color.BLUE)
            self.map.add_dock()
        elif char == 'BB_d':
            self.map.add_box(Color.BLUE)
            self.map.add_dock()
        elif char == 'RB_d':
            self.map.add_box(Color.RED)
            self.map.add_dock()
        elif char == 'B':
            self.map.add_box(Color.BLUE)
            self.map.add_floor()
        elif char == 'RB':
            self.map.add_box(Color.RED)
            self.map.add_floor()
        elif char == 'BB':
            self.map.add_box(Color.BLUE)
            self.map.add_floor()
        elif char == 'BW_d':
            self.map.add_worker(Color.BLUE)
            self.map.add_dock()
        elif char == 'RW_d':
            self.map.add_worker(Color.RED)
            self.map.add_dock()

    def __init__(self, filename, level):
        if filename == 'random':
            self.map = Map()
            for row in level:
                self.map.next_row()
                for c in row:
                    self.parse_char(c)
        else:
            self.load_file(filename, level)

    def load_file(self, filename, level):
        self.map = Map()
        #        if level < 1 or level > 50:
        if level < 1:
            print("ERROR: Level " + str(level) + " is out of range")
            sys.exit(1)
        else:
            file = open(filename, 'r')
            level_found = False
            for line in file:
                if not level_found:
                    if "Level " + str(level) == line.strip():
                        level_found = True
                else:
                    if line.strip() != "":
                        self.map.next_row()
                        for c in line:
                            if c != '\n' and is_valid_value(c):
                                self.parse_char(c)
                            elif c == '\n':  # jump to next row when newline
                                continue
                            else:
                                print("ERROR: Level " + str(level) + " has invalid value " + c)
                                sys.exit(1)
                    else:
                        break

    def load_size(self):
        x = self.map.width()
        y = self.map.height()
        return x * 32, y * 32

    def get_matrix(self):
        return self.map.render()

    def print_matrix(self):
        for row in self.map.render():
            for char in row:
                sys.stdout.write(char)
                sys.stdout.flush()
            sys.stdout.write('\n')

    def get_content(self, x, y):
        return self.map.render()[y][x]

    def is_completed(self):
        return self.map.is_completed()

    def is_legal_move(self, color, x, y):
        worker = self.map.get_worker(color)
        if worker.can_move(x, y, self.map):
            return True
        else:
            return worker.can_push(x, y, self.map)[0]

    def move(self, color, x, y, pull):
        worker = self.map.get_worker(color)
        if pull:
            worker.pull(x, y, self.map)
        elif not worker.move(x, y, self.map):
            # Cant move, try to push
            worker.push(x, y, self.map)


def print_game(matrix, screen):
    screen.fill(background)
    x = 0
    y = 0
    for row in matrix:
        for char in row:
            if char == ' ':  # floor
                screen.blit(floor, (x, y))
            elif char == '#':  # wall
                screen.blit(wall, (x, y))
            elif char == 'BW':  # worker on floor
                screen.blit(worker_blue, (x, y))
            elif char == 'RW':  # worker on floor
                screen.blit(worker_red, (x, y))
            elif char == 'd':  # dock
                screen.blit(docker, (x, y))
            elif char == 'B_d':  # box on dock
                screen.blit(box_docked, (x, y))
            elif char == 'B':  # box
                screen.blit(box, (x, y))
            elif char == 'RB':
                screen.blit(box_red, (x, y))
            elif char == 'BB':
                screen.blit(box_blue, (x, y))
            elif char == 'BW_d':  # worker on dock
                screen.blit(worker_docked_blue, (x, y))
            elif char == 'RW_d':  # worker on dock
                screen.blit(worker_docked_red, (x, y))
            x = x + 32
        x = 0
        y = y + 32


def get_key():
    while 1:
        event = pygame.event.poll()
        if event.type == pygame.KEYDOWN:
            return event.key
        else:
            pass


def display_box(screen, message):
    """
    Print a message in a box in the middle of the screen
    """
    font_object = pygame.font.Font(None, 18)
    pygame.draw.rect(screen, (0, 0, 0),
                     ((screen.get_width() / 2) - 100,
                      (screen.get_height() / 2) - 10,
                      200, 20), 0)
    pygame.draw.rect(screen, (255, 255, 255),
                     ((screen.get_width() / 2) - 102,
                      (screen.get_height() / 2) - 12,
                      204, 24), 1)
    if len(message) != 0:
        screen.blit(font_object.render(message, True, (255, 255, 255)),
                    ((screen.get_width() / 2) - 100, (screen.get_height() / 2) - 10))
    pygame.display.flip()


def display_end(screen):
    message = "Level Completed"
    display_box(screen, message)


def ask(screen, question):
    """
    Ask question on screen

    @return answer
    """
    pygame.font.init()
    current_string = []
    display_box(screen, question + ": " + "".join(current_string))
    while 1:
        in_key = get_key()
        if in_key == pygame.K_BACKSPACE:
            current_string = current_string[0:-1]
        elif in_key == pygame.K_RETURN:
            break
        elif in_key == pygame.K_MINUS:
            current_string.append("_")
        elif in_key <= 127:
            current_string.append(chr(in_key))
        display_box(screen, question + ": " + "".join(current_string))
    return "".join(current_string)


def start_game():
    start = pygame.display.set_mode((320, 240))
    level = ask(start, "Select Level (r for random)")
    return level


wall = pygame.image.load('images/wall.png')
floor = pygame.image.load('images/floor.png')
box = pygame.image.load('images/box.png')
box_blue = pygame.image.load('images/box_blue.png')
box_red = pygame.image.load('images/box_red.png')
box_docked = pygame.image.load('images/box_docked.png')
worker_blue = pygame.image.load('images/worker_blue.png')
worker_red = pygame.image.load('images/worker_red.png')
worker_docked_blue = pygame.image.load('images/worker_dock_blue.png')
worker_docked_red = pygame.image.load('images/worker_dock_red.png')
docker = pygame.image.load('images/dock.png')
background = 255, 226, 191
pygame.init()

level = start_game()
if level == 'r':
    generator = Generator()
    level_solvable = False
    shortest_path = 0
    while not level_solvable or shortest_path < 15:
        print("Generating new map")
        level_map = generator.generate(2, 1, 3)

        game_instance = Game('random', level_map)
        state_space = StateSpace(game_instance.map)
        level_solvable = state_space.solvable()
        if level_solvable:
            shortest_path = state_space.shortest_path_length()
            print("Shortest path: %d" % shortest_path)
        else:
            print("Level not solvable")

else:
    game_instance = Game('levels', int(level))

size = game_instance.load_size()
screen = pygame.display.set_mode(size)

for state in state_space.shortest_path():
    print_game(state.render(), screen)
    pygame.display.update()
    time.sleep(1)

while 1:
    time.sleep(0.05)
    if game_instance.is_completed():
        display_end(screen)

    print_game(game_instance.get_matrix(), screen)
    for event in pygame.event.get():
        red_pull = pygame.key.get_pressed()[pygame.K_e]
        blue_pull = pygame.key.get_pressed()[pygame.K_RSHIFT]
        if event.type == pygame.QUIT:
            sys.exit(0)
        elif event.type == pygame.KEYDOWN:
            if event.key == pygame.K_UP:
                game_instance.move(Color.BLUE, 0, -1, blue_pull)
            elif event.key == pygame.K_DOWN:
                game_instance.move(Color.BLUE, 0, 1, blue_pull)
            elif event.key == pygame.K_LEFT:
                game_instance.move(Color.BLUE, -1, 0, blue_pull)
            elif event.key == pygame.K_RIGHT:
                game_instance.move(Color.BLUE, 1, 0, blue_pull)
            elif event.key == pygame.K_w:
                game_instance.move(Color.RED, 0, -1, red_pull)
            elif event.key == pygame.K_s:
                game_instance.move(Color.RED, 0, 1, red_pull)
            elif event.key == pygame.K_a:
                game_instance.move(Color.RED, -1, 0, red_pull)
            elif event.key == pygame.K_d:
                game_instance.move(Color.RED, 1, 0, red_pull)
            elif event.key == pygame.K_q:
                sys.exit(0)
    pygame.display.update()
