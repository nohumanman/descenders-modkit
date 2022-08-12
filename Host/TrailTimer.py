import time
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
        self.time_ended = 0
        self.starting_speed = None

    def get_boundaries(self):
        return self.__boundaries

    def add_boundary(self, boundary_guid):
        if (
            len(self.__boundaries) == 0
            and not self.network_player.being_monitored
        ):
            self.invalidate_timer("No boundaries entered")
        if boundary_guid not in self.__boundaries:
            self.__boundaries.append(boundary_guid)

    def remove_boundary(self, boundary_guid):
        if boundary_guid in self.__boundaries:
            self.__boundaries.remove(boundary_guid)
        if (
            len(self.__boundaries) == 0
            and not self.network_player.being_monitored
        ):
            self.invalidate_timer("out of bounds!")

    def start_timer(self, total_checkpoints: int):
        if (
            len(self.__boundaries) == 0
            and not self.network_player.being_monitored
        ):
            self.invalidate_timer("out of bounds!", always=True)
        else:
            self.started = True
            self.total_checkpoints = total_checkpoints
            self.time_started = time.time()
            self.times = []

    def checkpoint(self, client_time: str):
        logging.info(
            "TrailTimer.py - "
            f"id{self.network_player.steam_id} "
            f"alias {self.network_player.steam_name} "
            f"- checkpoint() with client time {client_time}"
        )
        if self.started:
            # self.times.append(time.time() - self.time_started)
            self.times.append(float(client_time))
            fastest = DBMS.get_fastest_split_times(self.trail_name)
            try:
                time_diff = (
                    fastest[len(self.times)-1]
                    - (time.time() - self.time_started)
                )
            except Exception:
                time_diff = 0
            if time_diff > 0:
                mess = (
                    "<color=lime>-"
                    + str(round(abs(time_diff), 3))
                    + "</color>"
                )
            elif time_diff < 0:
                mess = (
                    "<color=red>+"
                    + str(round(abs(time_diff), 3))
                    + "</color>"
                )
            if time_diff != 0:
                mess += " TOP"

            fastest = DBMS.get_personal_fastest_split_times(
                self.trail_name,
                self.network_player.steam_id
            )
            try:
                time_diff_local = (
                    fastest[len(self.times)-1]
                    - (time.time() - self.time_started)
                )
            except Exception:
                time_diff_local = 0
            if time_diff_local > 0:
                mess += (
                    "               <color=lime>-"
                    + str(round(abs(time_diff_local), 3))
                    + "</color>"
                )
            elif time_diff_local < 0:
                mess += (
                    "               <color=red>+"
                    + str(round(abs(time_diff_local), 3))
                    + "</color>"
                )
            if time_diff_local != 0:
                mess += " PB "
            self.network_player.send(f"SPLIT_TIME|{mess}")

    def invalidate_timer(self, reason: str, always=False):
        logging.info(
            "TrailTimer.py - "
            f"id{self.network_player.steam_id} "
            f"alias {self.network_player.steam_name} "
            "- invalidate_timer() with reason"
            f"{reason}"
        )
        if (not self.started) and not always:
            return
        logging.info(f"invalidating time of {self.network_player.steam_name}")
        self.network_player.send(f"INVALIDATE_TIME|{reason}")
        self.started = False
        self.times = []

    def end_timer(self, client_time: str):
        logging.info(
            "TrailTimer.py - "
            f"id{self.network_player.steam_id} "
            f"alias {self.network_player.steam_name} "
            f"- end_timer() with client time {client_time}"
        )
        from DBMS import DBMS
        if self.total_checkpoints is None:
            self.invalidate_timer("Didn't go through all checkpoints.")
            if not self.network_player.being_monitored:
                return
        if (self.times[len(self.times)-1] < 0):
            self.invalidate_timer("Time was negative")
            if not self.network_player.being_monitored:
                return
        if (
            not(
                (
                    (time.time() - self.time_started) - float(client_time) < 1
                )
                and
                (
                    (time.time() - self.time_started) - float(client_time) > -1
                )
            )
        ):
            logging.info("Potential Cheat")
            self.invalidate_timer("Client time did not match server time!")
            discord_bot = self.network_player.parent.discord_bot
            discord_bot.loop.run_until_complete(
                discord_bot.ban_note(
                    "**Potential cheat** from "
                    f"{self.network_player.steam_name}"
                    " - client time did not match server time!"
                    f"\n\nTime submitted was '{client_time}' and "
                    f"server time was '{(time.time() - self.time_started)}'"
                )
            )
            return
        self.times.append(float(client_time))
        self.time_ended = client_time
        if (len(self.times) == self.total_checkpoints-1):
            if self.trail_name != "4x Dobrany":
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
                    shouldNotifyAlways = True
                    if shouldNotifyAlways:
                        our_time = TrailTimer.secs_to_str(
                                self.times[len(self.times)-1]
                            )
                        discord_bot = self.network_player.parent.discord_bot
                        discord_bot.loop.run_until_complete(
                            discord_bot.ban_note(
                                "Time on '"
                                + self.trail_name
                                + "' by '"
                                + self.network_player.steam_name
                                + "' of "
                                + our_time
                            )
                        )
                except Exception as e:
                    logging.error(f"Fastest not found: {e}")
            DBMS().submit_time(
                self.network_player.steam_id,
                self.times,
                self.trail_name,
                self.network_player.being_monitored,
                self.network_player.world_name,
                self.network_player.bike_type,
                str(self.starting_speed),
                str(self.network_player.version)
            )
            self.network_player.send(
                "TIMER_FINISH|Time - "
                + str(TrailTimer.secs_to_str(
                            self.times[len(self.times)-1]
                        ))
            )
        else:
            self.invalidate_timer("Didn't enter all checkpoints.", always=True)
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
        d_mins = int(secs // 60)
        d_secs = int(secs % 60)
        fraction = float(secs * 1000)
        fraction = round(fraction % 1000)
        if len(str(d_mins)) == 1:
            d_mins = "0" + str(d_mins)
        if len(str(d_secs)) == 1:
            d_secs = "0" + str(d_secs)
        while len(str(fraction)) < 3:
            fraction = "0" + str(fraction)
        return f"{d_mins}:{d_secs}.{fraction}"

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
