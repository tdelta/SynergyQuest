from utils import Color
import networkx as nx
import matplotlib.pyplot as plt
from copy import deepcopy
import multiprocessing
from functools import reduce
from tqdm import tqdm

from generator import Generator

direction_names = {
    (0, 1): "down",
    (0, -1): "up",
    (1, 0): "right",
    (-1, 0): "left"
}

possible_directions = [(0, 1), (0, -1), (1, 0), (-1, 0)]

possible_moves = [
    {
        "color": color,
        "x": direction[0],
        "y": direction[1],
        "pull": pull
    }
    for color in [Color.RED, Color.BLUE]
    for direction in possible_directions
    for pull in [True, False]
]


def is_legal_move(state, color, x, y, pull):
    """
    Evaluate whether a specific move can be executed on the current map

    @param state: Current state in explored graph
    @param color: Color of worker that makes the move
    @param x: Square that the worker moves to
    @param y: Square that the worker moves to
    @param pull: True if the worker should pull a box
    @return: True if the move can be executed
    """
    can_pull_before = state['map'].get_worker(color).can_pull(x, y, state['map'])[0]
    state['map'].last_render_valid = False
    can_pull_after = state['map'].get_worker(color).can_pull(x, y, state['map'])[0]
    assert can_pull_after == can_pull_before

    if pull:
        return state['map'].get_worker(color).can_pull(x, y, state['map'])[0]
    else:
        if state['map'].get_worker(color).can_move(x, y, state['map']):
            return True
        else:
            return state['map'].get_worker(color).can_push(x, y, state['map'])[0]


def make_move(state, color, x, y, pull):
    """
    Execute a move

    @param state: Current state in explored graph
    @param color: Color of worker that makes the move
    @param x: Square that the worker moves to
    @param y: Square that the worker moves to
    @param pull: True if the worker should pull a box
    @return: New state that appears after executing the move
    """

    new_map = deepcopy(state['map'])
    if pull:
        new_map.get_worker(color).pull(x, y, new_map)
    else:
        if state['map'].get_worker(color).can_move(x, y, state['map']):
            new_map.get_worker(color).move(x, y, new_map)
        else:
            new_map.get_worker(color).push(x, y, new_map)

    new_state = {'map': new_map}
    return new_state


class StateSpace:
    def __init__(self, level_map):
        """
        @param level_map: Map: initial map of the puzzle
        """
        self.state_graph = nx.DiGraph()
        self.end = None
        level_map = deepcopy(level_map)
        self.beginning = level_map

        n_states = self.total_number_of_states(level_map.render())
        self.statusbar = tqdm(total=n_states)

        self.state_graph.add_node(level_map, initial=True, goal=level_map.is_completed())
        print("Traversing state graph (Statusbar uses an approximation of the number of possible states. "
              "Sometimes not all states are reachable)")
        self.traverse_from_state({'map': level_map})
        self.statusbar.close()

    def shortest_path_length(self):
        assert self.solvable()
        return nx.shortest_path_length(
            self.state_graph.to_undirected(),
            source=self.beginning,
            target=self.end
        )

    def shortest_path(self):
        assert self.solvable()
        return nx.shortest_path(
            self.state_graph.to_undirected(),
            source=self.beginning,
            target=self.end
        )

    def solvable(self):
        return self.end is not None

    @staticmethod
    def total_number_of_states(level_map):
        n_floors = sum(len(list(x for x in row
                                if x in [" ", "BW", "RW", "BB", "RB", "BW_d", "RW_d", "BB_d", "RB_d", "d"]))
                       for row in level_map)
        n_boxes = sum(len(list(x for x in row
                               if x in ["BB", "RB", "BB_d", "RB_d"]))
                      for row in level_map)
        n_entities = n_boxes + 2  # 2 workers
        # All possible positions of all entities on empty floors
        return reduce(lambda x, y: x*y, range(n_floors - n_entities+1, n_floors+1), 1)

    def draw_graph(self):
        p = multiprocessing.Process(target=self.draw_graph_blocking)
        p.start()

    def draw_graph_blocking(self):
        pos = nx.nx_agraph.graphviz_layout(self.state_graph)
        edge_labels = nx.get_edge_attributes(self.state_graph, 'label')
        edge_labels = {(e[0], e[1]): edge_labels[e] for e in edge_labels}  # Map from tuple (node A, node B) to label
        nx.draw_networkx_edge_labels(self.state_graph, pos, edge_labels=edge_labels)
        # Create a list of node colors
        color_map = ['red' if self.state_graph.nodes(data=True)[node]['initial'] else (
            'green' if self.state_graph.nodes(data=True)[node]['goal'] else 'blue') for node in self.state_graph]
        nx.draw(self.state_graph, pos=pos, node_color=color_map)
        plt.show(block=True)

    def traverse_from_state(self, state):
        # Add all states and moves using breadth-first search
        q = [state]
        while len(q) > 0:
            current_state = q.pop(0)
            for move in possible_moves:
                if is_legal_move(current_state, **move):
                    next_state = make_move(current_state, **move)

                    # Found new game state, add to graph and traverse from here later
                    if not next_state['map'] in self.state_graph:
                        self.statusbar.update()
                        self.state_graph.add_node(next_state['map'], initial=False,
                                                  goal=next_state['map'].is_completed())
                        q.append(next_state)

                    if next_state['map'].is_completed():
                        # Stop exploring after the first goal state, since this is also the one with the shortest path
                        q = []
                        if self.end is None:
                            self.end = next_state['map']

                    self.add_move(current_state, next_state, **move)

    def add_move(self, current_state, next_state, x, y, color, pull):
        assert (x, y) in possible_directions

        # Generate text to display at the edges of the graph
        move_label = ("R " if color == Color.RED else "B ") + \
            direction_names[(x, y)] + (", pull" if pull else "")

        # Add move to graph if the reverse move was not added yet
        if not self.state_graph.has_edge(next_state['map'], current_state['map']):
            self.state_graph.add_edge(current_state['map'], next_state['map'], label=move_label)
