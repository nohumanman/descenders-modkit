from posixpath import split
import time
from threading import Thread
from PlayerDB import PlayerDB


class Player():
    def __init__(self, steam_name, steam_id, world_name, is_competitor):
        print("Player created with steam id", steam_id)
        self.steam_name = steam_name
        self.steam_id = steam_id
        self.current_world = world_name
        self.current_trail = "none"
        self.online = False
        self.is_competitor = is_competitor
        self.trail_start_time = 0
        self.split_times = []
        self.time_started = None
        self.time_ended = None
        self.has_entered_checkpoint = False
        PlayerDB.add_player(steam_id, steam_name, is_competitor)

    def loaded(self):
        self.time_started = time.time()

    def unloaded(self):
        self.time_started = time.time()
        PlayerDB.end_session(self.steam_id, self.time_started)
        self.time_started = None
        self.time_ended = None

    def set_competitor(self, is_competitor):
        self.is_competitor = is_competitor
        PlayerDB.become_competitor(self.steam_id, False)

    def entered_checkpoint(self, checkpoint_num, total_checkpoints, checkpoint_time, trail_name):
        self.has_entered_checkpoint = True
        self.current_trail = trail_name
        self.online = True
        if checkpoint_num == 0:
            self.split_times = []
            self.current_trail = "none" 
            self.trail_start_time = time.time()
        elif checkpoint_num == total_checkpoints-1:
            self.split_times.append(checkpoint_time - self.trail_start_time)
            self.submit_time(self.split_times, trail_name)
        else:
            self.split_times.append(checkpoint_time - self.trail_start_time)
        Thread(target=self.disable_entered_checkpoint, args=(5,)).start()

    def submit_time(self, split_times, trail_name):
        self.online = True
        print("SUBMITTING TIME - TRAIL COMPLETE!!")
        PlayerDB.submit_time(self.steam_id, split_times, trail_name)

    def disable_entered_checkpoint(self, delay):
        time.sleep(delay)
        self.has_entered_checkpoint = False
