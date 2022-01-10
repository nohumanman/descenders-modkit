from posixpath import split
import time
from threading import Thread
import sqlite3


class Player():
    def __init__(self, steam_name, steam_id, world_name):
        self.steam_name = steam_name
        self.steam_id = steam_id
        self.current_world = world_name
        self.trail_start_time = 0
        self.split_times = []
        con = sqlite3.connect("TimeStats.db")
        con.execute(
            f'''
            REPLACE INTO Players (steam_id, steam_name, times_logged_on)
            VALUES ("{steam_id}", "{steam_name}", 404)
            '''
        )
        con.commit()

    def entered_checkpoint(self, checkpoint_num, total_checkpoints, checkpoint_time, trail_name):
        print(checkpoint_num)
        print(type(checkpoint_num))
        print(total_checkpoints)
        print(type(total_checkpoints))
        if checkpoint_num == 0:
            self.split_times = []
            self.trail_start_time = time.time()
        elif checkpoint_num == total_checkpoints-1:
            self.split_times.append(checkpoint_time - self.trail_start_time)
            self.submit_time(self.split_times, trail_name)
        else:
            self.split_times.append(checkpoint_time - self.trail_start_time)
        Thread(target=self.disable_entered_checkpoint, args=(5,)).start()

    def submit_time(self, split_times, trail_name):
        print("SUBMITTING TIME - TRAIL COMPLETE!!")
        con = sqlite3.connect("TimeStats.db")
        time_hash = hash(str(split_times[len(split_times)-1])+str(self.steam_id)+str(time.time()))
        con.execute(
            f'''
            INSERT INTO Times (steam_id, time_id, total_time, timestamp, trail_name)
            VALUES ("{self.steam_id}", "{time_hash}", "{split_times[len(split_times)-1]}", {time.time()}, "{trail_name}")
            ''')
        for n, split_time in enumerate(self.split_times):
            con.execute(
            f'''
            INSERT INTO "Split Times" (
                time_id,
                checkpoint_num,
                checkpoint_time
                )
            VALUES (
                "{time_hash}",
                "{n}",
                {split_time}
                )
            ''')
        con.commit()
        self.split_times = []
        self.trail_start_time = time.time()

    def disable_entered_checkpoint(self, delay):
        time.sleep(delay)
        self.has_entered_checkpoint = False
