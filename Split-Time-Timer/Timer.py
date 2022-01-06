from Player import Player


class Timer():
    def __init__(self):
        self.players = {}  # in form {123123123: Player()}
        self.monitored_player = None
