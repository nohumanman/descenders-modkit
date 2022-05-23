import time
import math
from DBMS import DBMS
import logging


class TrailTimer():
    def __init__(self, trail_name, network_player):
        self.trail_name = trail_name
        self.network_player = network_player
        self.started = False
        self.times = []
        self.total_checkpoints = None
        self.__boundaries = []
        self.time_started = 0

    def add_boundary(self, boundary_guid):
        if len(self.__boundaries) == 0:
            self.invalidate_timer("No boundaries entered")
        if boundary_guid not in self.__boundaries:
            self.__boundaries.append(boundary_guid)

    def remove_boundary(self, boundary_guid):
        if boundary_guid in self.__boundaries:
            self.__boundaries.remove(boundary_guid)
        if len(self.__boundaries) == 0:
            self.invalidate_timer("Exited boundry without entering")

    def start_timer(self, total_checkpoints: int):
        self.started = True
        self.total_checkpoints = total_checkpoints
        self.time_started = time.time()
        self.times = []

    def checkpoint(self):
        if self.started:
            self.times.append(time.time() - self.time_started)
            fastest = DBMS.get_fastest_split_times(self.trail_name)
            try:
                time_diff = (
                    fastest[len(self.times)-1]
                    - (time.time() - self.time_started)
                )
            except Exception:
                time_diff = "unknown"
            if time_diff > 0:
                mess = str(round(abs(time_diff), 4)) + " seconds faster"
            elif time_diff < 0:
                mess = str(round(abs(time_diff), 4)) + " seconds slower"
            self.network_player.send(f"SPLIT_TIME|{mess}")

    def invalidate_timer(self, reason: str):
        if not self.started:
            return
        logging.info(f"invalidating time of {self.network_player.steam_name}")
        self.network_player.send(f"INVALIDATE_TIME|{reason}")
        self.started = False
        self.times = []

    def end_timer(self):
        from DBMS import DBMS
        if self.total_checkpoints is None:
            self.invalidate_timer("Didn't go through all checkpoints.")
        if (self.times[len(self.times)-1] < 0):
            self.invalidate_timer("Time was negative")
        self.times.append(time.time() - self.time_started)
        if (len(self.times) == self.total_checkpoints-1):
            logging.info(f"Times submitted: {self.times}")
            fastest = DBMS.get_fastest_split_times(self.trail_name)
            try:
                if self.times[len(self.times)-1] < fastest[len(fastest)-1]:
                    our_time = TrailTimer.secs_to_str(
                        self.times[len(self.times)-1]
                    )
                    logging.info("New fastest time.")
                    discord_bot = self.network_player.parent.discord_bot
                    discord_bot.loop.run_until_complete(
                        discord_bot.new_fastest_time(
                            "🎉 New fastest time on **"
                            + self.trail_name
                            + "** by **"
                            + self.network_player.steam_name
                            + "**! 🎉\nTime to beat is: "
                            + our_time
                        )
                    )
            except Exception as e:
                logging.error(f"Fastest not found: {e}")
            DBMS().submit_time(
                self.network_player.steam_id,
                self.times,
                self.trail_name,
                False,
                self.network_player.world_name,
                self.network_player.bike_type
            )
            self.network_player.send(
                "TIMER_FINISH|Time - "
                + str(TrailTimer.secs_to_str(
                            self.times[len(self.times)-1]
                        ))
            )
        self.started = False
        self.times = []
        for net_player in self.network_player.parent.players:
            net_player.send(
                "LEADERBOARD|"
                + self.trail_name + "|"
                + str(
                    self.network_player.get_leaderboard(self.trail_name)
                )
            )

    @staticmethod
    def secs_to_str(secs):
        secs = float(secs)
        d_mins = int(round(secs // 60))
        d_secs = int(round(secs - (d_mins * 60)))
        d_millis = int(round(secs-math.trunc(secs), 3) * 1000)
        if len(str(d_mins)) == 1:
            d_mins = "0" + str(d_mins)
        if len(str(d_secs)) == 1:
            d_secs = "0" + str(d_secs)
        while len(str(d_millis)) < 3:
            d_millis = str(d_millis) + "0"
        return f"{d_mins}:{d_secs}.{d_millis}"

    @staticmethod
    def ord(n):
        return (
            str(n)
            + ("th" if 4 <= (n % 100) <= 20 else {
                1: "st",
                2: "nd",
                3: "rd"
            }.get(n % 10, "th")
            )
        )
