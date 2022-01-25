from Player import Player
import sqlite3
import os

script_path = os.path.dirname(os.path.realpath(__file__))

class Timer():
    def __init__(self):
        self.players = {}  # in form {123123123: Player()}
        self.monitored_player = None
        self.competitors_only = False
        self.timestamp = 0
        self.monitored_only = False
        self.get_players()

    def get_players(self):
        con = sqlite3.connect(script_path + "/TimeStats.db")
        cur = con.cursor()
        players = cur.execute("SELECT * FROM Players").fetchall()
        cur.close()
        con.close()
        for player in players:
            steam_id = player[0]
            steam_name = player[1]
            is_competitor = player[2]
            avatar_src = player[4]
            if is_competitor == "true":
                player = Player(steam_name, steam_id, "none", True)
            else:
                player = Player(steam_name, steam_id, "none", False)
            self.add_player(player, steam_id)
    
    def add_player(self, player : Player, steam_id: str):
        self.players[steam_id] = player
