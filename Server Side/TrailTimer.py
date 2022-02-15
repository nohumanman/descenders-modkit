import time
from PlayerDB import PlayerDB
import requests
import random
from Tokens import webhook


class TimerNotStarted(Exception):
    pass

class AntiCheatMeasure(Exception):
    pass

class TrailTimer():
    def __init__(self, player):
        self.started = False
        self.player = player
        self.times = []
        self.give = 0.5
        self.time_started = 0

    def start_timer(self, checkpoint):
        self.started = True
        self.total_checkpoints = checkpoint.total_checkpoints
        self.time_started = time.time()
        self.times = []

    def cancel_timer(self):
        self.started = False
        self.times = []

    def end_timer(self):
        self.started = False
        try:
            print(PlayerDB.get_fastest_split_times(self.player.trail))
            fastest_times = PlayerDB.get_fastest_split_times(self.player.trail)
            fastest_time = fastest_times[len(fastest_times)-1]
            if self.times[len(self.times)-1] < fastest_time:
                print("New Fastest Time!")
                faster_amount = round(fastest_time - self.times[len(self.times)-1], 4)
                emojis = ["🎉"]
                data = {
                    "content" : f"There's a new Time on {self.current_trail}!",
                    "username" : "Descenders Gear Hub"
                }
                data["embeds"] = [
                    {
                        "description" : f"🎉🎉 00:00:000 on {self.current_trail}!",
                        "title" : f"[Descenders Split Timer](https://gear-hub.nohumanman.com)",
                        "author" : {
                            "name": f"{self.steam_name}",
                            "url": "",
                            "icon_url": f"{self.avatar_src}"
                        },
                    }
                ]
                        
                content = ""
                content += random.choice(emojis) * 3
                content += "\n"
                congrats = ["Congratulations to", "Congrats to", "GG", "Well raced", "Good job", "Well done"]
                content += random.choice(congrats)
                content += f" **{self.steam_name}** "
                quickestest = ["fastest"]
                content += f"for the new " + random.choice(quickestest) + f" time on {self.player.trail} in {self.current_world}!"
                content += f"\nIt's {round(faster_amount, 5)} seconds faster than the previous best 🔥"
                data = {
                    "content": content,
                    "username": "Descenders Competitive"
                }
                result = requests.post(webhook, json = data)
                
        except Exception as e:
            print(e)
        PlayerDB.submit_time(
            self.player.steam_id,
            self.times,
            self.player.trail,
            self.player.monitored,
            self.player.world
        )

    def split(self, client_time : float, anti_cheat=True):
        if not self.started:
            raise TimerNotStarted("Timer has not started! Unable to split!")
        else:
            server_time = time.time()-self.time_started
            # If the client's submitted time is within 500 milliseconds
            # of the server's time, then accept the client's time,
            # otherwise, throw an error.
            import logging
            logging.info(f"server time: {server_time}, client time: {client_time}")
            if self.__within(
                float(server_time),
                float(client_time),
                float(self.give)
            ) and anti_cheat:
                self.times.append(client_time)
            else:
                raise AntiCheatMeasure("Client Time and Server Time are too far apart - cheating suspected!")

    def __within(self, first_val : float, second_val : float, seconds_give : float):
        max_second_val = second_val + seconds_give
        min_second_val = second_val - seconds_give
        if (first_val >= min_second_val and first_val <= max_second_val):
            return True
        else:
            return False

