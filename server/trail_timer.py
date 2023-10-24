import time
import asyncio
import logging
from dbms import DBMS

# Used to fix RuntimeError in using async from thread
import nest_asyncio
nest_asyncio.apply()

split_timer_logger = logging.getLogger('DescendersSplitTimer')


class Vector3():
    def __init__(self):
        self.x = 0
        self.y = 0
        self.z = 0

    def get_as_str(self):
        return f"X:{self.x} Y: {self.y} Z:{self.z}"

    @staticmethod
    def get_distance(vector1, vector2):
        return ((
            (vector1.x - vector2.x)**2
            + (vector1.y - vector2.y)**2
            + (vector1.z - vector2.z)**2
        )**(0.5))


class TrailTimer():
    def __init__(self, trail_name, network_player):
        self.trail_name = trail_name
        # to prevent circular import
        from unity_socket import UnitySocket
        self.network_player : UnitySocket = network_player
        self.started = False
        self.times = []
        self.total_checkpoints = None
        self.__boundaries = []
        self.time_started = 0
        self.time_ended = 0
        self.starting_speed = None
        self.total_running_penalty = 0.0
        self.current_penalty = 0.0
        self.player_positions = []
        self.exit_time = 0

    def get_boundaries(self):
        return self.__boundaries

    def add_boundary(self, boundary_guid):
        if (
            len(self.__boundaries) == 0
            and not self.network_player.being_monitored
        ):
            if (self.started):
                pass
                #self.current_penalty = (time.time()-self.exit_time)*100
                #if self.current_penalty < 2:
                #    self.current_penalty = 2
                # if self.current_penalty < 0.5:
                #     self.current_penalty = 0
                #self.total_running_penalty += self.current_penalty
                #self.network_player.send(
                #    f"SPLIT_TIME|penalty of "
                #    f"~{round(self.current_penalty * 100) / 100}"
                #)
                self.network_player.set_text_default()
        if boundary_guid not in self.__boundaries:
            self.__boundaries.append(boundary_guid)

    def remove_boundary(self, boundary_guid, boundry_obj_name):
        if boundary_guid in self.__boundaries:
            self.__boundaries.remove(boundary_guid)
        if (
            len(self.__boundaries) == 0
            and not self.network_player.being_monitored
        ):
            if (self.started):
                pass
                #self.exit_time = time.time()
                #self.network_player.send(f"INVALIDATE_TIME|Exited Boundaries of Trail.\nIf this is incorrect, please report it to nohumanman on Discord\nBOUNDARY ISSUE: {boundry_obj_name}")
                #self.network_player.set_text_colour(255, 0, 0)
                #self.started = False

    def start_timer(self, total_checkpoints: int):
        split_timer_logger.info("id%s '%s' started timer with checkpoints %s", self.network_player.steam_id, self.network_player.steam_name, total_checkpoints)
        self.total_running_penalty = 0
        self.player_positions = []
        self.current_penalty = 0
        if (
            len(self.__boundaries) == 0
            and not self.network_player.being_monitored
        ):
            self.invalidate_timer("OUT OF BOUNDS!", always=True)
        else:
            self.started = True
            self.total_checkpoints = total_checkpoints
            self.time_started = time.time()
            self.times = []

    def checkpoint(self, client_time: str):
        split_timer_logger.info("id%s '%s' entered checkpoint with client time %s", self.network_player.steam_id, self.network_player.steam_name, client_time)
        if self.started:
            self.times.append(float(client_time) + self.total_running_penalty)
            self.current_penalty = 0
            fastest = DBMS.get_fastest_split_times(self.trail_name)
            try:
                time_diff = (
                    fastest[len(self.times)-1]
                    - float(client_time)
                )
            except IndexError:
                time_diff = 0
            mess = ""
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
                mess += " WR"

            fastest = DBMS.get_personal_fastest_split_times(
                self.trail_name,
                self.network_player.steam_id
            )
            try:
                time_diff_local = (
                    fastest[len(self.times)-1]
                    - float(client_time)
                )
            except IndexError:
                time_diff_local = 0
            if time_diff_local > 0:
                mess += (
                    "  <color=lime>-"
                    + str(round(abs(time_diff_local), 3))
                    + "</color>"
                )
            elif time_diff_local < 0:
                mess += (
                    "  <color=red>+"
                    + str(round(abs(time_diff_local), 3))
                    + "</color>"
                )
            if time_diff_local != 0:
                mess += " PB"
            if mess == "":
                mess = self.secs_to_str(float(client_time))
            self.network_player.send(f"SPLIT_TIME|{mess}")

    def invalidate_timer(self, reason: str, always=False):
        split_timer_logger.info("id%s '%s'  invalidated due to %s, always=%s", self.network_player.steam_id, self.network_player.steam_name, reason, always)
        if (not self.started) and not always:
            return
        self.total_running_penalty = 0
        self.current_penalty = 0
        self.network_player.send(f"INVALIDATE_TIME|{reason}")
        self.started = False
        self.times = []
        self.player_positions = []

    def update_medals(self):
        self.network_player.get_medals(self.trail_name)

    def end_timer(self, client_time: str):
        split_timer_logger.info("id%s '%s' ending timer at client time %s", self.network_player.steam_id, self.network_player.steam_name, client_time)
        self.started = False
        if len(self.__boundaries) == 0:
            self.invalidate_timer("OUT OF BOUNDS ON FINISH!!!")
            return
        if self.total_checkpoints is None:
            self.invalidate_timer("Didn't go through all checkpoints.")
            if not self.network_player.being_monitored:
                return
        if len(self.times) == 0:
            # No times logged, can't finish
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
            self.potential_cheat(client_time)
            return
        self.times.append(float(client_time) + self.total_running_penalty)
        self.time_ended = client_time
        if (len(self.times) == self.total_checkpoints-1):
            split_timer_logger.info("id%s '%s' submitting times '%s'", self.network_player.steam_id, self.network_player.steam_name, self.times)
            time_id = DBMS().submit_time(
                self.network_player.steam_id,
                self.times,
                self.trail_name,
                self.network_player.being_monitored,
                self.network_player.world_name,
                self.network_player.bike_type,
                str(self.starting_speed),
                str(self.network_player.version),
                self.total_running_penalty
            )
            fastest = DBMS.get_fastest_split_times(self.trail_name)
            try:
                our_time = TrailTimer.secs_to_str(
                    self.times[len(self.times)-1]
                )
                if self.times[len(self.times)-1] < fastest[len(fastest)-1]:
                    self.__new_fastest_time(our_time)
                try:
                    discord_bot = self.network_player.parent.discord_bot
                    discord_bot.loop.run_until_complete(
                        discord_bot.new_time(
                            f"<@&1166081385732259941> Please verify [the new time](https://split-timer.nohumanman.com/time/{time_id}) on '"
                            + self.trail_name
                            + "' by '"
                            + self.network_player.steam_name
                            + "' of "
                            + our_time
                        )
                    )
                except RuntimeError as e:
                    split_timer_logger.warning("Failed to submit time to discord server %s", e)
            except (IndexError, KeyError) as e:
                logging.error("Fastest not found: %s", e)
            DBMS.submit_locations(time_id, self.player_positions)
            penalty_message = ""
            if self.total_running_penalty == 0:
                penalty_message = "\\nNo penalties :)"
            else:
                penalty_message = (
                    "\\nPenalty of "
                    + str(round(self.total_running_penalty * 100) / 100)
                )
            self.network_player.send(f"UPLOAD_REPLAY|{time_id}")
            self.network_player.send(
                "TIMER_FINISH|"
                + str(
                    TrailTimer.secs_to_str(
                            self.times[len(self.times)-1]
                    )
                )
                + penalty_message
            )
        else:
            self.invalidate_timer("Didn't enter all checkpoints.", always=True)
        self.update_leaderboards()
        self.update_medals()
        self.times = []
        self.total_running_penalty = 0
        self.current_penalty = 0

    def potential_cheat(self, client_time: float):
        split_timer_logger.info("id%s '%s' has potentially cheated! client time %s", self.network_player.steam_id, self.network_player.steam_name, client_time)
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

    def update_leaderboards(self):
        for net_player in self.network_player.parent.players:
            net_player.send(
                "LEADERBOARD|"
                + self.trail_name + "|"
                + str(
                    self.network_player.get_leaderboard(self.trail_name)
                )
            )

    def __new_fastest_time(self, our_time: str):
        split_timer_logger.info("id%s '%s' new fastest time!", self.network_player.steam_id, self.network_player.steam_name)
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