"""
This file randomly generates levels from level fragments.
Note that the resulting maps are simply 2d string arrays.
The meaning of the strings is defined in sokoban.py
"""

from random import random


class Fragment:
    def __init__(self):
        self.above = []
        self.below = []
        self.left = []
        self.right = []
        self.content = []

    def has_constraint(self, constraint):
        """
        If all characters of constraint are ?, it means that there is no constraint.

        @param constraint: above, below, left or right constraint
        @return: True if there is a constraint on the walls of the neighboring fragment
        """
        return constraint.count('?') < len(constraint)

    # TODO: Currently does not check the edges of constraints

    def check_above(self, fragment):
        for i in range(1, len(self.above) - 1):
            if not self.above[i] == '?' and not fragment.content[0][i - 1] == self.above[i]:
                return False
        return True

    def check_below(self, fragment):
        for i in range(1, len(self.below) - 1):
            if not self.below[i] == '?' and not fragment.content[len(fragment.content) - 1][i - 1] == self.below[i]:
                return False
        return True

    def check_left(self, fragment):
        for i in range(len(self.left)):
            if not self.left[i] == '?' and not fragment.content[i][len(fragment.content[i]) - 1] == self.left[i]:
                return False
        return True

    def check_right(self, fragment):
        for i in range(len(self.right)):
            if not self.right[i] == '?' and not fragment.content[i][0] == self.right[i]:
                return False
        return True


class Generator:
    def __init__(self):
        self.fragments = []
        self.load_fragments()

    @staticmethod
    def find_floor(level_map):
        """
        @param level_map: Floor map of the generated level
        @return: Random empty position
        """
        while True:
            y = int(random() * len(level_map))
            x = int(random() * len(level_map[y]))
            if level_map[y][x] == ' ':
                # The position is a floor :)
                break
        return x, y

    @staticmethod
    def count_floors(map):
        """
        @param map: Floor map of the generated level
        @return: Number of remaining empty floor positions
        """
        return sum(sum(int(x == ' ') for x in row) for row in map)

    def generate(self, x, y, n_boxes):
        """
        Generate a random level map

        @param x: Number of horizontal level fragments
        @param y: Number of vertical level fragments
        @param n_boxes: Number of boxes to place in the level
        @return: Level map
        """
        result = self.generate_walls(x, y)
        # TODO: less floors could be possible if box is already on dock
        while self.count_floors(result) < n_boxes * 2 + 2:
            result = self.generate_walls(x, y)

        n_docks = 0

        for _ in range(n_boxes):
            x, y = self.find_floor(result)
            place_box_on_dock = False
            if random() < 0.1:
                place_box_on_dock = True
            else:
                # Box was not placed on a dock, need to add dock later
                n_docks += 1
            if int(random() * 2) == 0:
                # Blue Box
                result[y][x] = 'BB_d' if place_box_on_dock else 'BB'
            else:
                # Red Box
                result[y][x] = 'RB_d' if place_box_on_dock else 'RB'

        for _ in range(n_docks):
            x, y = self.find_floor(result)
            result[y][x] = 'd'

        x, y = self.find_floor(result)
        # Place Blue Worker
        result[y][x] = 'BW'

        x, y = self.find_floor(result)
        # Place Red Worker
        result[y][x] = 'RW'
        print(result)
        return result

    def generate_walls(self, x, y):
        """
        Generate wall positions for a level map by placing level fragments next to each other

        @param x: Number of horizontal level fragments
        @param y: Number of vertical level fragments
        @return: Level map
        """
        result = [[] for _ in range(y)]
        for j in range(y):
            for i in range(x):
                while True:
                    new_fragment_idx = int(random() * len(self.fragments))  # idx between 0 and len(s.f) - 1
                    new_fragment = self.fragments[new_fragment_idx]

                    ##############################################
                    # Check if fragment fits in current position #
                    ##############################################

                    # TODO: Currently does not check the edges of constraints

                    constraint_violation = False

                    if i == 0:
                        if new_fragment.has_constraint(new_fragment.left):
                            # Cant put a fragment with left constraints to the left of the map
                            constraint_violation = True
                    elif not new_fragment.check_left(result[j][i - 1]):
                        constraint_violation = True
                    elif not result[j][i - 1].check_right(new_fragment):
                        constraint_violation = True

                    if i == x - 1:
                        if new_fragment.has_constraint(new_fragment.right):
                            # Cant put a fragment with right constraints to the right of the map
                            constraint_violation = True

                    if j == 0:
                        if new_fragment.has_constraint(new_fragment.above):
                            # Cant put a fragment with above constraints to the top of the map
                            constraint_violation = True
                    elif not new_fragment.check_above(result[j - 1][i]):
                        constraint_violation = True
                    elif not result[j - 1][i].check_right(new_fragment):
                        constraint_violation = True

                    if j == y - 1 and new_fragment.has_constraint(new_fragment.below):
                        # Cant put a fragment with below constraints to the bottom of the map
                        constraint_violation = True

                    if not constraint_violation:
                        break

                # If the loop terminated, it means that no check failed 
                result[j].append(new_fragment)
        parsed_result = [sum([f.content[i] for f in row], []) for row in result for i in range(3)]
        return parsed_result

    def load_fragments(self):
        """
        Load level fragments from file
        """
        with open("level_fragments", 'r') as f:
            i = 0  # current line in level fragment
            for line in f:
                if i == 5:
                    # Reached end of fragment
                    i = 0
                    continue

                elif i == 0:
                    self.fragments.append(Fragment())
                    self.fragments[-1].above = [c for c in line.strip()]

                elif i == 4:
                    self.fragments[-1].below = [c for c in line.strip()]

                else:
                    self.fragments[-1].left += line[0]
                    self.fragments[-1].right += line[4]
                    self.fragments[-1].content.append([c for c in line[1:4]])
                i += 1
