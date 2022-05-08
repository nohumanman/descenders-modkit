import socket
import time
from DBMS import DBMS
from TrailTimer import TrailTimer
import requests
from Tokens import steam_api_key

operations = {
    "STEAM_ID" : lambda netPlayer, data : netPlayer.set_steam_id(data[1]),
    "STEAM_NAME" : lambda netPlayer, data : netPlayer.set_steam_name(data[1]),
    "WORLD_NAME" : lambda netPlayer, data : netPlayer.set_world_name(data[1]),
    "BOUNDRY_ENTER" : lambda netPlayer, data : netPlayer.on_boundry_enter(data[1], data[2]),
    "BOUNDRY_EXIT" : lambda netPlayer, data : netPlayer.on_boundry_exit(data[1], data[2]),
    "CHECKPOINT_ENTER" : lambda netPlayer, data : netPlayer.on_checkpoint_enter(data[1], data[2], data[3]),
    "RESPAWN" : lambda netPlayer, data : netPlayer.on_respawn(),
    "MAP_ENTER" : lambda netPlayer, data : netPlayer.on_map_enter(data[1], data[2]),
    "MAP_EXIT" : lambda netPlayer, data : netPlayer.on_map_exit(),
    "BIKE_SWITCH" : lambda netPlayer, data : netPlayer.on_bike_switch(data[1], data[2])
}

class NetPlayer():
    def __init__(self, conn : socket):
        self.conn = conn
        self.trails = {}
        self.__avatar_src = None
        self.steam_id = None
        self.steam_name = None
        self.bike_type = "enduro"
        self.world_name = None
        self.time_started = time.time()
        self.send("SUCCESS")
        #while (self.steam_id == None
        #        or self.steam_name == None
        #        or self.world_name == None):
        #    time.sleep(0.5)

    def get_avatar_src(self):
        if self.__avatar_src is not None:
            return self.__avatar_src
        avatar_src_req = requests.get(f"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={steam_api_key}&steamids={self.steam_id}")
        try:
            self.__avatar_src = avatar_src_req.json()["response"]["players"][0]["avatarfull"]
        except KeyError:
            self.__avatar_src = ""
        return self.__avatar_src

    def set_steam_name(self, steam_name):
        self.steam_name = steam_name
        data = {
            "content" : f"{self.steam_name} has joined the server!",
            "username" : "Split Timer"
        }
        requests.post("https://discord.com/api/webhooks/972656466748141628/uejju8_qlDscdimgiGP3wnf9s1Igwazyrlxm6Hb9Aary3OhKYU_sbwuMp3sLWt8cUxjL", json=data)
    
    def set_steam_id(self, steam_id):
        self.steam_id = steam_id

    def set_world_name(self, world_name):
        self.world_name = world_name
        DBMS().update_player(self.steam_id, self.steam_name, self.get_avatar_src())

    def send(self, data : str):
        self.conn.sendall((data + "\n").encode())

    def handle_data(self, data : str):
        if data == "":
            return
        print(f"From {self.steam_name} Handling data '{data}'")
        data_list = data.split("|")
        for operator in operations:
            if data.startswith(operator):
                operations[operator](self, data_list)

    def recieve(self):
        while True:
            try:
                data = self.conn.recv(1024)
            except ConnectionResetError:
                print("User has disconnected, breaking loop")
                break
            if not data:
                print("No data!!!!")
                break
            for piece in data.decode().split("\n"):
                self.handle_data(piece)
        del(self)

    def ban(self, reason, method):
        self.send("BAN|" + reason + "|" + method)

    def invalidate_all_trails(self, reason : str):
        for trail in self.trails:
            self.trails[trail].invalidate_timer(reason)

    def on_respawn(self):
        self.invalidate_all_trails("You respawned!")

    def get_trail(self, trail_name) -> TrailTimer:
        if trail_name not in self.trails:
            self.trails[trail_name] = TrailTimer(trail_name, self)
        return self.trails[trail_name]

    def on_bike_switch(self, old_bike : str, new_bike : str):
        self.bike_type = new_bike
        self.invalidate_all_trails("You switched bikes!")

    def on_boundry_enter(self, trail_name : str, boundry_guid : str):
        trail = self.get_trail(trail_name)
        trail.add_boundary(boundry_guid)

    def on_boundry_exit(self, trail_name : str, boundry_guid : str):
        trail = self.get_trail(trail_name)
        trail.remove_boundary(boundry_guid)

    def on_checkpoint_enter(self, trail_name : str, type : str, total_checkpoints : str):
        self.get_trail(trail_name).total_checkpoints = int(total_checkpoints)
        if type == "Start":
            self.get_trail(trail_name).start_timer(total_checkpoints)
        if type == "Intermediate":
            self.get_trail(trail_name).checkpoint()
        if type == "Finish":
            self.get_trail(trail_name).end_timer()

    def on_map_enter(self, map_id, map_name):
        self.time_started = time.time()

    def on_map_exit(self):
        self.trails = {}
        DBMS.end_session(self.steam_id, self.time_started, time.time(), self.world_name)
        self.conn.close()
