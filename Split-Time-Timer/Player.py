from posixpath import split
import time
from threading import Thread
import requests
from PlayerDB import PlayerDB
import random
from Tokens import webhook, steam_api_key


class Player():
    def __init__(self, steam_name, steam_id, world_name, is_competitor):
        print("Player created with steam id", steam_id)
        self.steam_name = steam_name
        self.steam_id = steam_id
        avatar_src_req = requests.get(
            f"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={steam_api_key}&steamids={steam_id}"
        )
        try:
            avatar_src = avatar_src_req.json()["response"]["players"][0]["avatarfull"]
            self.avatar_src = avatar_src
        except:
            self.avatar_src = ""
        self.current_world = world_name
        self.current_trail = "none"
        self.online = False
        self.is_competitor = is_competitor
        self.trail_start_time = 0
        self.split_times = []
        self.amount_of_boundaries_inside = 0
        self.being_monitored = False
        self.time_started = None
        self.time_ended = None
        self.has_entered_checkpoint = False
        self.current_bike = "unknown"
        PlayerDB.add_player(steam_id, steam_name, is_competitor, self.avatar_src)

    def get_ban_status(self):
        return PlayerDB.get_ban_status(self.steam_id)

    def loaded(self, world_name):
        self.time_started = time.time()
        self.online = True
        self.current_world = world_name

    def unloaded(self):
        self.time_ended = time.time()
        PlayerDB.end_session(self.steam_id, self.time_started, self.time_ended, self.current_world)
        self.time_started = None
        self.time_ended = None
        self.online = False
        self.current_world = "None"

    def set_competitor(self, is_competitor):
        self.is_competitor = is_competitor
        PlayerDB.become_competitor(self.steam_id, False)

    async def entered_checkpoint(self, checkpoint_num : int, total_checkpoints : int, checkpoint_time : float, trail_name : str):
        self.has_entered_checkpoint = True
        self.current_trail = trail_name
        self.online = True
        if checkpoint_num == 0:
            print("entered checkpoint at start - setting trail start time...")
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
        # webhook
        try:
            print(PlayerDB.get_fastest_split_times(trail_name))
            fastest_times = PlayerDB.get_fastest_split_times(trail_name)
            fastest_time = fastest_times[len(fastest_times)-1]
            if split_times[len(split_times)-1] < fastest_time:
                print("New Fastest Time!")
                faster_amount = round(fastest_time - split_times[len(split_times)-1], 4)
                emojis = ["🎉"]
                content = ""
                content += random.choice(emojis) * 3
                content += "\n"
                congrats = ["Congratulations to", "Congrats to", "GG", "Well raced", "Good job", "Well done"]
                content += random.choice(congrats)
                content += f" **{self.steam_name}** "
                quickestest = ["fastest"]
                content += f"for the new " + random.choice(quickestest) + f" time on {trail_name} in {self.current_world}!"
                content += f"\nIt's {round(faster_amount, 5)} seconds faster than the previous best 🔥"
                data = {
                    "content": content,
                    "username": "Descenders Competitive"
                }
                result = requests.post(webhook, json = data)
                
        except Exception as e:
            print(e)
        PlayerDB.submit_time(self.steam_id, split_times, trail_name, self.being_monitored)

    def seconds_to_string(self, millis):
        seconds=(millis/1000)%60
        minutes=(millis/(1000*60))%60
        hours=(millis/(1000*60*60))%24
        return str(round(minutes)) + ":" + str(round(seconds)) + ":" + str(int((millis)%1000))

    def cancel_time(self):
        self.has_entered_checkpoint = False
        self.split_times = []
        self.trail_start_time = 0

    def disable_entered_checkpoint(self, delay):
        time.sleep(delay)
        self.has_entered_checkpoint = False
