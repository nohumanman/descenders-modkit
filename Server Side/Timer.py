from Player import Player
import sqlite3
import os
from PlayerDB import PlayerDB

script_path = os.path.dirname(os.path.realpath(__file__))

class Timer():
    def __init__(self):
        self.players = {}  # in form {123123123: Player()}
        self.monitored_players = []
        self.competitors_only = False
        self.timestamp = 0
        self.monitored_only = False
        self.get_players()

    def get_player(self, steam_id, steam_name : str, world_name) -> Player:
        if self.players[steam_id] is None:
            self.players[steam_id] = Player(
                steam_name,
                steam_id,
                world_name,
                False,
                "unbanned"
            )
        return self.players[steam_id]

    def get_players(self):
        players = PlayerDB.get_all_players()
        for player in players:
            steam_id = player[0]
            steam_name = player[1]
            is_competitor = player[2]
            ban_status = player[3]
            avatar_src = player[4]
            if is_competitor == "true":
                player = Player(steam_name, steam_id, "none", True, ban_status)
            else:
                player = Player(steam_name, steam_id, "none", False, ban_status)
            self.add_player(player, steam_id)
    
    def add_player(self, player : Player, steam_id: str):
        self.players[steam_id] = player
