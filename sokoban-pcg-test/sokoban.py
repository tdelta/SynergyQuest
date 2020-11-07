#!../bin/python
import sys
import pygame
import string
import Queue
from enum import Enum
from copy import deepcopy
from generator import Generator

IMPASSABLE_OBJECTS = ['#','*','$','@','a','+','-','S','|','"',':']

class Color(Enum):
    RED = 1
    BLUE = 2

class Entity():
    """
    param grid: the map onto which to render the entity
    returns: map with entity
    """
    def render(self, grid):
        pass
    
    def can_move(self, x, y, map):
        return map.width() > self.x + x and \
                map.height() > self.y + y and \
                self.x + x >= 0 and \
                self.y + y >= 0 and \
                map.render()[self.y + y][self.x + x] not in IMPASSABLE_OBJECTS

class Worker(Entity):
    def __init__(self, x, y, color):
        self.x = x
        self.y = y
        self.color = color

    def render(self, grid): 
        if grid[self.y][self.x] == '.':
            # Worker is on dock
            grid[self.y][self.x] = '+' if self.color == Color.BLUE else '-'
        else:
            grid[self.y][self.x] = '@' if self.color == Color.BLUE else 'a'
        return grid

    def can_push(self, x, y, map):
        box = map.get_box(self.x + x,self.y + y)
        if box is None:
            return (False, None)
        if not box.color == self.color:
            return (False, box)
        else:
            return (box.can_move(x, y, map), box)

    def can_pull(self, x, y, map):
        # Get box on opposite site of moving direction
        box = map.get_box(self.x - x,self.y - y)
        if box is None:
            return (False, None)
        elif not box.color == self.color:
            return (False, None)
        else:
            return (self.can_move(x, y, map), box)

    def move(self, x, y, map):
        if self.can_move(x, y, map):
            self.x += x
            self.y += y
            return True
        return False

    def push(self, x, y, map):
        can_push, box = self.can_push(x, y, map)
        if can_push:
            box.x += x
            box.y += y
            self.move(x, y, map)

    def pull(self, x, y, map):
        can_pull, box = self.can_pull(x, y, map)
        if can_pull:
            box.x += x
            box.y += y
            self.move(x, y, map)

class Box(Entity):
    def __init__(self, x, y, color):
        self.x = x
        self.y = y
        self.color = color

    def render(self, grid): 
        if grid[self.y][self.x] == '.':
            # Box is on dock
            grid[self.y][self.x] = '*'
        else:
            grid[self.y][self.x] = '|' if self.color == Color.BLUE else 'S'
        return grid

class Map():
    def __init__(self):
        self.grid = []
        self.current_x = 0
        self.current_y = -1
        self.entities = []
        self.workers = {}

    def add_floor(self):
        self.grid[-1].append(' ')
        self.current_x += 1

    def add_wall(self):
        self.grid[-1].append('#')
        self.current_x += 1
    
    def add_dock(self):
        self.grid[-1].append('.')
        self.current_x += 1

    def next_row(self):
        self.grid.append([])
        self.current_x = 0
        self.current_y += 1

    def add_worker(self, color):
        self.entities.append(Worker(self.current_x,self.current_y,color))
        self.workers[color] = self.entities[-1]

    def add_box(self, color):
        self.entities.append(Box(self.current_x,self.current_y,color))

    def render(self):
        grid = deepcopy(self.grid)
        for entity in self.entities:
            grid = entity.render(grid)
        return grid

    def is_completed(self):
        for entity in self.entities:
            if isinstance(entity, Box):
                if not self.grid[entity.y][entity.x] == '.':
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

class game:

    def is_valid_value(self,char):
        if ( char == ' ' or #floor
            char == '#' or #wall
            char == '@' or #blue worker on floor
            char == 'a' or #red worker on floor
            char == '.' or #dock
            char == '*' or #box on dock
            char == ':' or
            char == '"' or
            char == '$' or #box
            char == 'S' or
            char == '|' or
            char == '+' or #blue worker on dock
            char == '-' ): #red worker on dock
            return True
        else:
            return False

    # TODO: Remove legacy entities without color
    def parse_char(self,char):
        if char == ' ':
            self.map.add_floor()
        elif char == '#':
            self.map.add_wall()
        elif char == '@':
            self.map.add_worker(Color.BLUE)
            self.map.add_floor()
        elif char == 'a':
            self.map.add_worker(Color.RED)
            self.map.add_floor()
        elif char == '.':
            self.map.add_dock()
        elif char == '*':
            self.map.add_box(Color.BLUE)
            self.map.add_dock()
        elif char == ':':
            self.map.add_box(Color.BLUE)
            self.map.add_dock()
        elif char == '"':
            self.map.add_box(Color.RED)
            self.map.add_dock()
        elif char == '$':
            self.map.add_box(Color.BLUE)
            self.map.add_floor()
        elif char == 'S':
            self.map.add_box(Color.BLUE)
            self.map.add_floor()
        elif char == '|':
            self.map.add_box(Color.RED)
            self.map.add_floor()
        elif char == '+':
            self.map.add_worker(Color.BLUE)
            self.map.add_dock()
        elif char == '-':
            self.map.add_worker(Color.RED)
            self.map.add_dock()

    def __init__(self,filename,level):
        if filename == 'random':
            self.map = Map()
            for row in level:
                self.map.next_row()
                for c in row:
                    self.parse_char(c)
        else:
            self.load_file(filename, level)

    def load_file(self, filename, level):
        self.queue = Queue.LifoQueue()
        self.map = Map()
#        if level < 1 or level > 50:
        if level < 1:
            print "ERROR: Level "+str(level)+" is out of range"
            sys.exit(1)
        else:
            file = open(filename,'r')
            level_found = False
            for line in file:
                if not level_found:
                    if  "Level "+str(level) == line.strip():
                        level_found = True
                else:
                    if line.strip() != "":
                        self.map.next_row()
                        for c in line:
                            if c != '\n' and self.is_valid_value(c):
                                self.parse_char(c)
                            elif c == '\n': #jump to next row when newline
                                continue
                            else:
                                print "ERROR: Level "+str(level)+" has invalid value "+c
                                sys.exit(1)
                    else:
                        break

    def load_size(self):
        x = self.map.width()
        y = self.map.height()
        return (x * 32, y * 32)

    def get_matrix(self):
        return self.map.render()

    def print_matrix(self):
        for row in self.map.render():
            for char in row:
                sys.stdout.write(char)
                sys.stdout.flush()
            sys.stdout.write('\n')

    def get_content(self,x,y):
        return self.map.render()[y][x]

    def is_completed(self):
        return self.map.is_completed()

    def is_legal_move(self, color, x, y, save):
        worker = self.map.get_worker(color)
        if worker.can_move(x, y, self.map):
            return True
        else:
            return worker.can_push(x, y, self.map)[0]

    def move(self,color, x,y,save, pull):
        worker = self.map.get_worker(color)
        if pull:
            worker.pull(x, y, self.map)
        elif not worker.move(x,y, self.map):
            # Cant move, try to push
            worker.push(x, y, self.map)

def print_game(matrix,screen):
    screen.fill(background)
    x = 0
    y = 0
    for row in matrix:
        for char in row:
            if char == ' ': #floor
                screen.blit(floor,(x,y))
            elif char == '#': #wall
                screen.blit(wall,(x,y))
            elif char == '@': #worker on floor
                screen.blit(worker_blue,(x,y))
            elif char == 'a': #worker on floor
                screen.blit(worker_red,(x,y))
            elif char == '.': #dock
                screen.blit(docker,(x,y))
            elif char == '*': #box on dock
                screen.blit(box_docked,(x,y))
            elif char == '$': #box
                screen.blit(box,(x,y))
            elif char == 'S':
                screen.blit(box_red,(x,y))
            elif char == '|':
                screen.blit(box_blue,(x,y))
            elif char == '+': #worker on dock
                screen.blit(worker_docked_blue,(x,y))
            elif char == '-': #worker on dock
                screen.blit(worker_docked_red,(x,y))
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
  "Print a message in a box in the middle of the screen"
  fontobject = pygame.font.Font(None,18)
  pygame.draw.rect(screen, (0,0,0),
                   ((screen.get_width() / 2) - 100,
                    (screen.get_height() / 2) - 10,
                    200,20), 0)
  pygame.draw.rect(screen, (255,255,255),
                   ((screen.get_width() / 2) - 102,
                    (screen.get_height() / 2) - 12,
                    204,24), 1)
  if len(message) != 0:
    screen.blit(fontobject.render(message, 1, (255,255,255)),
                ((screen.get_width() / 2) - 100, (screen.get_height() / 2) - 10))
  pygame.display.flip()

def display_end(screen):
    message = "Level Completed"
    fontobject = pygame.font.Font(None,18)
    pygame.draw.rect(screen, (0,0,0),
                   ((screen.get_width() / 2) - 100,
                    (screen.get_height() / 2) - 10,
                    200,20), 0)
    pygame.draw.rect(screen, (255,255,255),
                   ((screen.get_width() / 2) - 102,
                    (screen.get_height() / 2) - 12,
                    204,24), 1)
    screen.blit(fontobject.render(message, 1, (255,255,255)),
                ((screen.get_width() / 2) - 100, (screen.get_height() / 2) - 10))
    pygame.display.flip()


def ask(screen, question):
  "ask(screen, question) -> answer"
  pygame.font.init()
  current_string = []
  display_box(screen, question + ": " + string.join(current_string,""))
  while 1:
    inkey = get_key()
    if inkey == pygame.K_BACKSPACE:
      current_string = current_string[0:-1]
    elif inkey == pygame.K_RETURN:
      break
    elif inkey == pygame.K_MINUS:
      current_string.append("_")
    elif inkey <= 127:
      current_string.append(chr(inkey))
    display_box(screen, question + ": " + string.join(current_string,""))
  return string.join(current_string,"")

def start_game():
    start = pygame.display.set_mode((320,240))
    level = ask(start,"Select Level")
    if level > 0:
        return level
    else:
        print "ERROR: Invalid Level: "+str(level)
        sys.exit(2)

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

generator = Generator()
level = start_game()

if level == 'r':
    level_map = generator.generate(3,5,5)
    game = game('random', level_map)
else:
    game = game('levels',level)

size = game.load_size()
screen = pygame.display.set_mode(size)
while 1:
    if game.is_completed(): display_end(screen)
    print_game(game.get_matrix(),screen)
    for event in pygame.event.get():
        red_pull=pygame.key.get_pressed()[pygame.K_e]
        blue_pull=pygame.key.get_pressed()[pygame.K_RSHIFT]
        if event.type == pygame.QUIT: sys.exit(0)
        elif event.type == pygame.KEYDOWN:
            if event.key == pygame.K_UP: game.move(Color.BLUE, 0,-1, True, blue_pull)
            elif event.key == pygame.K_DOWN: game.move(Color.BLUE, 0,1, True, blue_pull)
            elif event.key == pygame.K_LEFT: game.move(Color.BLUE, -1,0, True, blue_pull)
            elif event.key == pygame.K_RIGHT: game.move(Color.BLUE, 1,0, True, blue_pull)
            elif event.key == pygame.K_w: game.move(Color.RED, 0,-1, True, red_pull)
            elif event.key == pygame.K_s: game.move(Color.RED, 0,1, True, red_pull)
            elif event.key == pygame.K_a: game.move(Color.RED, -1,0, True, red_pull)
            elif event.key == pygame.K_d: game.move(Color.RED, 1,0, True, red_pull)
            elif event.key == pygame.K_q: sys.exit(0)
    pygame.display.update()
