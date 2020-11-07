from random import random

class Fragment():
    def __init__(self):
        self.above = []
        self.below = []
        self.left = []
        self.right = []
        self.content = []

    def has_constraint(self, constraint):
        return constraint.count('?') < len(constraint)

    # TODO: Currently does not check the edges of constraints
    
    def check_above(self, fragment):
        for i in range(1,len(self.above)-1):
            if not self.above[i] == '?' and not fragment.content[len(fragment.content)-1][i-1] == self.above[i]:
                return False
        return True
    
    def check_below(self, fragment):
        for i in range(1,len(self.below)-1):
            if not self.below[i] == '?' and not fragment.content[0][i-1] == self.below[i]:
                return False
        return True
    
    def check_left(self, fragment):
        for i in range(len(self.left)):
            if not self.left[i] == '?' and not fragment.content[i][0] == self.left[i]:
                return False
        return True
    
    def check_right(self, fragment):
        for i in range(len(self.right)):
            if not self.right[i] == '?' and not fragment.content[i][len(fragment.content[i])-1] == self.right[i]:
                return False
        return True


class Generator():
    def __init__(self):
        self.fragments = []
        self.load_fragments()

    def generate(self, x, y, n_boxes):
        result = self.generate_walls(x, y)
        x,y = 0,0
        print(result)


        for _ in range(n_boxes):
            while True:
                y = int(random() * len(result))
                x = int(random() * len(result[y]))
                if result[y][x] == ' ':
                    # The position is a floor :)
                    break
            if int(random() * 2) == 0:
                # Blue Box
                result[y][x] = 'S'
            else:
                # Red Box
                result[y][x] = '|'
 
        while True:
            y = int(random() * len(result))
            x = int(random() * len(result[y]))
            if result[y][x] == ' ':
                # The position is on the floor :)
                break
        # Place Blue Worker
        result[y][x] = '@' 

        while True:
            y = int(random() * len(result))
            x = int(random() * len(result[y]))
            if result[y][x] == ' ':
                # The position is on the floor :)
                break
        # Place Red Worker
        result[y][x] = 'a'
        return result

    def generate_walls(self, x, y):
        result = [[] for _ in range(y)]
        for j in range(y):
            for i in range(x):
                while True:
                    new_fragment_idx = int(random() * len(self.fragments)) # idx between 0 and len(s.f) - 1
                    new_fragment = self.fragments[new_fragment_idx]

                    ##############################################
                    # Check if fragment fits in current position #
                    ##############################################

                    # TODO: Currently does not check the edges of constraints
                    
                    print(j,i)
                    print("Selected fragment:")
                    print(new_fragment.content)
                    
                    constraint_violation = False

                    if i == 0:
                        if new_fragment.has_constraint(new_fragment.left):
                            # Cant put a fragment with left contraints to the left of the map
                            print("fail 1")
                            constraint_violation = True
                    elif not new_fragment.check_left(result[j][i-1]):
                        print("fail 2")
                        constraint_violation = True
                    elif not result[j][i-1].check_right(new_fragment):
                        print("fail 3")
                        constraint_violation = True
                    
                    if i == x-1: 
                        if new_fragment.has_constraint(new_fragment.right):
                            # Cant put a fragment with right contraints to the right of the map
                            print("fail 4")
                            constraint_violation = True
                    
                    if j == 0:
                        if new_fragment.has_constraint(new_fragment.above):
                            # Cant put a fragment with above contraints to the top of the map
                            print("fail 5")
                            constraint_violation = True
                    elif not new_fragment.check_above(result[j-1][i]):
                        print("fail 6")
                        constraint_violation = True
                    elif not result[j-1][i].check_right(new_fragment):
                        print("fail 7")
                        constraint_violation = True
                    
                    if j == y-1 and new_fragment.has_constraint(new_fragment.below):
                        # Cant put a fragment with below contraints to the bottom of the map
                        print("fail 8")
                        constraint_violation = True

                    if not constraint_violation:
                        break

                # If the loop terminated, it means that no check failed 
                result[j].append(new_fragment)
        parsed_result = [sum([f.content[i] for f in row],[]) for row in result for i in range(3)]
        return parsed_result

    def load_fragments(self):
        with open("level_fragments", 'r') as f:
            i = 0 # current line in level fragment
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
                i+=1
