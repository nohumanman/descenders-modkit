from posixpath import split
import time
from threading import Thread
import logging
import requests
from PlayerDB import PlayerDB
import threading
import random
from Tokens import webhook, steam_api_key
from TrailTimer import TrailTimer, AntiCheatMeasure, TimerNotStarted


class Player():
    def __init__(self, steam_name, steam_id, world_name, is_competitor, ban_status):
        print("Player created with steam id", steam_id)
        self.steam_name = steam_name
        self.steam_id = steam_id
        self.ban_status = ban_status
        self.world = world_name
        self.is_competitor = is_competitor
        self.monitored = False
        self.online = False
        self.time_started = 0
        self.trail_timer = TrailTimer(self)
        avatar_src_req = requests.get(
            f"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={steam_api_key}&steamids={steam_id}"
        )
        try:
            avatar_src = avatar_src_req.json()["response"]["players"][0]["avatarfull"]
            self.avatar_src = avatar_src
        except:
            self.avatar_src = ""
        self.entered_boundaries = {}
        self.trail = "unknown"
        self.bike = "unknown"
        self.update_player_in_db()

    def on_competitor_status_change(self, is_competitor):
        self.is_competitor = is_competitor
        self.update_player_in_db()

    def update_player_in_db(self):
        PlayerDB.update_player(self.steam_id, self.steam_name, self.is_competitor, self.avatar_src)

    def on_avatar_change(self, new_src):
        self.avatar_src = new_src
        self.update_player_in_db()

    def on_bike_switch(self, new_bike):
        self.bike = new_bike
        if self.bike == "roadbike":
            self.online = False
        return "valid"

    def on_boundry_enter(self, boundry_guid, boundry):
        self.entered_boundaries[boundry_guid] = boundry

    def on_boundry_exit(self, boundry_guid, boundry):
        try:
            self.entered_boundaries.pop(boundry_guid)
        except:
            logging.warning("Boundry already popped!?")
        logging.info("Waiting 0.3 secs...")
        time.sleep(0.3)
        logging.info("Waited!")
        if len(self.entered_boundaries) == 0:
            logging.info("Outside of boundry!")
            return "Ouside of boundry for more than 300 milliseconds!"
        else:
            return "valid"

    def on_checkpoint_enter(self, checkpoint, client_time):
        if checkpoint.type == "start":
            self.trail_timer.start_timer(checkpoint)
        elif checkpoint.type == "intermediate":
            try:
                logging.info("Taking Split Time")
                self.trail_timer.split(client_time)
            except AntiCheatMeasure:
                print("Anticheat activated!")
                logging.info("Anticheat activated!")
                return "ERROR: AntiCheatMeasure has been called."
            except TimerNotStarted:
                logging.info("Timer not started!")
                print("Timer not started!")
                return "ERROR: Timer Not Started!"
        elif checkpoint.type == "stop" and checkpoint.total_checkpoints == checkpoint.num:
            self.trail_timer.end_timer()
        return "valid"

    def send_error(self, error):
        print(f"Sending error '{error}' to client!")

    def on_respawn(self):
        if self.trail_timer.started:
            self.trail_timer.cancel_timer()
            self.send_error("Time invalid: You Respawned")

    def on_map_enter(self, world_name):
        self.time_started = time.time()
        self.online = True
        self.world = world_name

    def on_map_exit(self):
        if (self.time_started != 0):
            self.online = False
            self.world = "unknown"
            self.trail_timer.cancel_timer()
            PlayerDB.end_session(
                self.steam_id,
                self.time_started,
                time.time(),
                self.world
            )
