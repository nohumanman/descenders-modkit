""" Used to track the time on a map """
from typing import TYPE_CHECKING
import dataclasses
import time
import logging
import nest_asyncio # Used to fix RuntimeError in using async from thread
nest_asyncio.apply()

if TYPE_CHECKING: # for imports with intellisense
    from unity_socket import UnitySocket

@dataclasses.dataclass
class TimerInfo:
    """ Information for the functionality of a timer """
    started: bool # Whether the timer has started
    auto_verify: bool # whether the time should be verified when finished
    times: list[float] # List of times
    starting_speed: float # The starting speed of the player
    total_checkpoints: int # The total number of checkpoints in the trail
    time_started: float # The time the timer was started

class TrailTimer():
    """ Used to track the time on a trail """
    def __init__(self, trail_name, network_player):
        self.trail_name = trail_name
        self.network_player : UnitySocket = network_player
        self.timer_info = TimerInfo(
            started=False, auto_verify=True, times=[], starting_speed=0,
            total_checkpoints=0, time_started=0
        )
        self.__boundaries = []

    async def add_boundary(self, boundary_guid):
        """
        Add a boundary to the list of boundaries encountered during a run.

        If the list of boundaries is empty and a run has started, it sets the 'auto_verify' flag to
        False and sends a message to the network player indicating that their time will be reviewed
        due to cuts.
        """
        # if we were out of bounds and are in a run
        if len(self.__boundaries) == 0 and self.timer_info.started:
            # note we cannot verify this user instantly
            self.timer_info.auto_verify = False
            await self.network_player.send("SPLIT_TIME|Time will be reviewed")
        if boundary_guid not in self.__boundaries:
            self.__boundaries.append(boundary_guid)

    async def remove_boundary(self, boundary_guid):
        """
        Remove a boundary from the list of boundaries encountered during a run.
        """
        if boundary_guid in self.__boundaries:
            self.__boundaries.remove(boundary_guid)
        if len(self.__boundaries) == 0:
            if self.timer_info.started:
                # note we cannot verify this user instantly
                self.timer_info.auto_verify = False
                await self.network_player.send("SPLIT_TIME|Time will be reviewed")

    def reset_boundaries(self):
        """
        Reset the list of boundaries encountered during a run.
        """
        self.__boundaries = []

    async def start_timer(self, total_checkpoints: int):
        """ Start the timer. """
        logging.info(
            "%s '%s'\t- started timer with checkpoints %s", self.network_player.info.steam_id,
            self.network_player.info.steam_name, total_checkpoints
        )
        self.timer_info.auto_verify = True
        if len(self.__boundaries) == 0:
            await self.invalidate_timer("OUT OF BOUNDS!", always=True)
        else:
            self.timer_info.started = True
            self.timer_info.total_checkpoints = total_checkpoints
            self.timer_info.time_started = time.time()
            self.timer_info.times = []

    async def checkpoint(self, client_time: float):
        """ Log a checkpoint. """
        logging.info(
            "%s '%s'\t- entered checkpoint with client time %s", self.network_player.info.steam_id,
            self.network_player.info.steam_name, client_time
        )
        if self.timer_info.started:
            self.timer_info.times.append(float(client_time))
            fastest = await self.network_player.dbms.get_fastest_split_times(self.trail_name)
            try:
                time_diff = (
                    fastest[len(self.timer_info.times)-1]
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

            fastest = await self.network_player.dbms.get_personal_fastest_split_times(
                self.trail_name,
                self.network_player.info.steam_id
            )
            try:
                time_diff_local = (
                    fastest[len(self.timer_info.times)-1]
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
            await self.network_player.send(f"SPLIT_TIME|{mess}")

    async def invalidate_timer(self, reason: str, always=False):
        """ Invalidate the timer. """
        logging.info(
            "%s '%s'\t- invalidated due to %s, always=%s",
            self.network_player.info.steam_id, self.network_player.info.steam_name, reason, always
        )
        if (not self.timer_info.started) and not always:
            return
        await self.network_player.send(f"INVALIDATE_TIME|{reason}\\n")
        self.timer_info.started = False
        self.timer_info.times = []

    async def update_medals(self):
        """ Update the medals for the player. """
        await self.network_player.get_medals(self.trail_name)

    async def can_end(self) -> bool:
        """ Check if the data is valid to be able to end the timer """
        errors = {
            "ERR001: Out of Bounds":
                lambda: len(self.__boundaries) == 0,
            "ERR002: Too many checkpoints":
                lambda: len(self.timer_info.times) > self.timer_info.total_checkpoints-1,
            "ERR003: Not enough checkpoints":
                lambda: len(self.timer_info.times) < self.timer_info.total_checkpoints-1,
            "ERR004: No times logged":
                lambda: len(self.timer_info.times) == 0,
            "ERR005: Time is negative":
                lambda: self.timer_info.times[len(self.timer_info.times)-1] < 0,
            "ERR006: Didn't start timer":
                lambda: not self.timer_info.started,
            "ERR007: Client time did not match server time":
                lambda: not(
                    (
                        (
                            (time.time() - self.timer_info.time_started)
                            - self.timer_info.times[len(self.timer_info.times)-1]
                        ) < 1
                    and (
                        (time.time() - self.timer_info.time_started)
                        - self.timer_info.times[len(self.timer_info.times)-1]
                        ) > -1
                    )
                )
        }
        for error_message, error in errors.items():
            if error():
                await self.invalidate_timer(error_message)
                return False
        return True

    async def end_timer(self, client_time: float):
        """ End the timer. """
        self.timer_info.times.append(float(client_time)) # add the final time
        # check if we can end the timer
        if not await self.can_end():
            return
        # submit the time to the database
        time_id = await self.network_player.dbms.submit_time(
            self.network_player.info.steam_id,
            self.timer_info.times,
            self.trail_name,
            False,
            self.network_player.info.world_name,
            self.network_player.info.bike_type,
            self.timer_info.starting_speed,
            str(self.network_player.info.version),
            0,
            "1" if self.timer_info.auto_verify else "0"
        )
        # ask client to upload replay
        await self.network_player.send(f"UPLOAD_REPLAY|{time_id}")
        # send the submitted time to the client
        comment = "verified" if self.timer_info.auto_verify else "awaiting review"
        secs_str = TrailTimer.secs_to_str(client_time)
        await self.network_player.send(
            f"TIMER_FINISH|{secs_str}\\n{comment}"
        )
        # send the time to the discord server if it is a new fastest time
        global_fastest = await self.network_player.dbms.get_fastest_split_times(self.trail_name)
        if client_time < global_fastest[len(global_fastest)-1]:
            await self.__new_fastest_time(secs_str)
        # send the time to the discord server if it is a new fastest time
        our_fastest = await self.network_player.dbms.get_personal_fastest_split_times(
            self.trail_name,
            self.network_player.info.steam_id
        )
        if our_fastest is None or len(our_fastest) == 0:
            fastest_pb = -1
        else:
            fastest_pb = float(our_fastest[len(our_fastest)-1])

        # send the time to the discord server if it is a new personal best
        time_url = f"https://modkit.nohumanman.com/time/{time_id}"
        # note: we use <= here because if the time is the same as the fastest pb, we still want to
        #       send a message to discord
        if ((client_time <= fastest_pb or fastest_pb == -1) and self.timer_info.auto_verify):
            await self.network_player.parent.get_discord_bot().new_time(
                f"Automatically verified [a new pb]({time_url}) on '{self.trail_name}' by "
                f"'{self.network_player.info.steam_name}' of {secs_str}"
            )
        elif client_time <= fastest_pb or fastest_pb == -1:
            await self.network_player.parent.get_discord_bot().new_time(
                f"<@&1166081385732259941> Please verify [the new pb time]({time_url}) on "
                f"'{self.trail_name}' by '{self.network_player.info.steam_name}' of {secs_str}"
            )
        # update the leaderboards and medals on connected clients
        await self.update_leaderboards()
        await self.update_medals()
        # reset the timer
        self.timer_info.times = []
        self.timer_info.auto_verify = True

    async def potential_cheat(self, client_time: float):
        """ Called when the client time does not match the server time. """
        logging.info(
            "%s '%s'\t- potentially cheated! client time %s", self.network_player.info.steam_id,
            self.network_player.info.steam_name, client_time
        )
        await self.invalidate_timer("Client time did not match server time!")
        discord_bot = self.network_player.parent.discord_bot
        if discord_bot is not None:
            discord_bot.loop.run_until_complete(
                discord_bot.ban_note(
                    "**Potential cheat** from "
                    f"{self.network_player.info.steam_name}"
                    " - client time did not match server time!"
                    f"\n\nTime submitted was '{client_time}' and "
                    f"server time was '{(time.time() - self.timer_info.time_started)}'"
                )
            )

    async def update_leaderboards(self):
        """ Update the leaderboards for the trail. """
        for net_player in self.network_player.parent.players:
            await net_player.send(
                "LEADERBOARD|"
                + self.trail_name + "|"
                + str(
                    await self.network_player.get_leaderboard(self.trail_name)
                )
            )

    async def __new_fastest_time(self, our_time: str):
        """ Called when a new fastest time is set. """
        logging.info(
            "%s '%s'\t- new fastest time!", self.network_player.info.steam_id,
            self.network_player.info.steam_name
        )
        discord_bot = self.network_player.parent.discord_bot
        if discord_bot is not None:
            await discord_bot.new_fastest_time(
                    "🎉 New fastest time on **"
                    + self.trail_name
                    + "** by **"
                    + self.network_player.info.steam_name
                    + "**! 🎉\nTime to beat is: "
                    + our_time
                )

    @staticmethod
    def secs_to_str(secs):
        """ Convert seconds to a string. """
        secs = float(secs)
        d_mins = int(secs // 60)
        d_secs = int(secs % 60)
        fraction = float(secs * 1000)
        fraction = round(fraction % 1000)
        if fraction == 1000:
            fraction = 0
            d_secs += 1
        if len(str(d_mins)) == 1:
            d_mins = "0" + str(d_mins)
        if len(str(d_secs)) == 1:
            d_secs = "0" + str(d_secs)
        while len(str(fraction)) < 3:
            fraction = "0" + str(fraction)
        return f"{d_mins}:{d_secs}.{fraction}"

    @staticmethod
    def ord(n):
        """ Convert a number to an ordinal. """
        return (
            str(n)
            + ("th" if 4 <= (n % 100) <= 20 else {
                1: "st",
                2: "nd",
                3: "rd"
            }.get(n % 10, "th")
            )
        )
