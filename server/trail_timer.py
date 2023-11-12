""" Used to track the time on a map """
from typing import TYPE_CHECKING
import time
import logging
import nest_asyncio # Used to fix RuntimeError in using async from thread
nest_asyncio.apply()

if TYPE_CHECKING: # for imports with intellisense
    from unity_socket import UnitySocket


logging = logging.getLogger('DescendersSplitTimer')


class TrailTimer():
    """ Used to track the time on a trail """
    def __init__(self, trail_name, network_player):
        self.trail_name = trail_name
        self.network_player : UnitySocket = network_player
        self.started = False
        self.times = []
        self.total_checkpoints: int = -1
        self.__boundaries = []
        self.time_started = 0
        self.time_ended = 0
        self.starting_speed = None
        self.exit_time = 0
        self.auto_verify = True

    async def add_boundary(self, boundary_guid):
        """
        Add a boundary to the list of boundaries encountered during a run.

        If the list of boundaries is empty and a run has started, it sets the 'auto_verify' flag to
        False and sends a message to the network player indicating that their time will be reviewed
        due to cuts.
        """
        # if we were out of bounds and are in a run
        if len(self.__boundaries) == 0 and self.started:
            # note we cannot verify this user instantly
            self.auto_verify = False
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
            if self.started:
                # note we cannot verify this user instantly
                self.auto_verify = False
                await self.network_player.send("SPLIT_TIME|Time will be reviewed")

    async def start_timer(self, total_checkpoints: int):
        """ Start the timer. """
        logging.info(
            "%s '%s'\t- started timer with checkpoints %s", self.network_player.info.steam_id,
            self.network_player.info.steam_name, total_checkpoints
        )
        self.auto_verify = True
        if len(self.__boundaries) == 0:
            await self.invalidate_timer("OUT OF BOUNDS!", always=True)
        else:
            self.started = True
            self.total_checkpoints = total_checkpoints
            self.time_started = time.time()
            self.times = []

    async def checkpoint(self, client_time: float):
        """ Log a checkpoint. """
        logging.info(
            "%s '%s'\t- entered checkpoint with client time %s", self.network_player.info.steam_id,
            self.network_player.info.steam_name, client_time
        )
        if self.started:
            self.times.append(float(client_time))
            fastest = self.network_player.dbms.get_fastest_split_times(self.trail_name)
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

            fastest = self.network_player.dbms.get_personal_fastest_split_times(
                self.trail_name,
                self.network_player.info.steam_id
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
            await self.network_player.send(f"SPLIT_TIME|{mess}")

    async def invalidate_timer(self, reason: str, always=False):
        """ Invalidate the timer. """
        logging.info(
            "%s '%s'\t- invalidated due to %s, always=%s",
            self.network_player.info.steam_id, self.network_player.info.steam_name, reason, always
        )
        if (not self.started) and not always:
            return
        await self.network_player.send(f"INVALIDATE_TIME|{reason}\\n")
        self.started = False
        self.times = []

    def update_medals(self):
        """ Update the medals for the player. """
        #self.network_player.get_medals(self.trail_name)

    async def end_timer(self, client_time: float):
        """ End the timer. """
        logging.info(
            "%s '%s'\t- ending timer at client time %s",
            self.network_player.info.steam_id, self.network_player.info.steam_name, client_time
        )
        self.started = False
        if len(self.__boundaries) == 0:
            await self.invalidate_timer("OUT OF BOUNDS ON FINISH!!!")
            return
        if self.total_checkpoints is None:
            await self.invalidate_timer("Didn't go through all checkpoints.")
        if len(self.times) == 0:
            # No times logged, can't finish
            return
        if self.times[len(self.times)-1] < 0:
            await self.invalidate_timer("Time was negative")
        # Check if the client time matches the server time
        server_end_time = time.time() - self.time_started
        client_end_time = float(client_time)
        if not(server_end_time - client_end_time < 1 and server_end_time - client_end_time > -1):
            await self.potential_cheat(client_time)
            return
        self.times.append(float(client_time))
        self.time_ended = client_time
        if len(self.times) == self.total_checkpoints-1:
            logging.info(
                "%s '%s'\t- submitting times '%s'", self.network_player.info.steam_id,
                self.network_player.info.steam_name, self.times
            )
            time_id = self.network_player.dbms.submit_time(
                self.network_player.info.steam_id,
                self.times,
                self.trail_name,
                False,
                self.network_player.info.world_name,
                self.network_player.info.bike_type,
                str(self.starting_speed),
                str(self.network_player.info.version),
                0,
                "1" if self.auto_verify else "0"
            )
            #self.network_player.send(f"UPLOAD_REPLAY|{time_id}")
            comment = "\\n"
            if self.auto_verify:
                comment += "verified"
            else:
                comment += "awaiting review"
            await self.network_player.send(
                "TIMER_FINISH|"
                + str(
                    TrailTimer.secs_to_str(
                            self.times[len(self.times)-1]
                    )
                ) + comment
            )
            fastest = self.network_player.dbms.get_fastest_split_times(self.trail_name)
            try:
                our_time = TrailTimer.secs_to_str(
                    self.times[len(self.times)-1]
                )
                if self.times[len(self.times)-1] < fastest[len(fastest)-1]:
                    self.__new_fastest_time(our_time)
                try:
                    fastest = self.network_player.dbms.get_personal_fastest_split_times(
                        self.trail_name,
                        self.network_player.info.steam_id
                    )
                    try:
                        fastest_pb = fastest[len(self.times)-1]
                    except IndexError:
                        fastest_pb = -1
                    time_url = f"https://split-timer.nohumanman.com/time/{time_id}"
                    # if the time is faster than the fastest pb or there is no fastest pb
                    # and the time is auto verified
                    if (
                        self.times[len(self.times)-1] <= fastest_pb
                        or fastest_pb == -1 and self.auto_verify
                    ):
                        discord_bot = self.network_player.parent.discord_bot
                        if discord_bot is not None:
                            discord_bot.loop.run_until_complete(
                                discord_bot.new_time(
                                    f"Server has auto verified [a new pb]({time_url}) on '"
                                    + self.trail_name
                                    + "' by '"
                                    + self.network_player.info.steam_name
                                    + "' of "
                                    + our_time
                                )
                            )
                    elif self.times[len(self.times)-1] <= fastest_pb or fastest_pb == -1:
                        discord_bot = self.network_player.parent.discord_bot
                        if discord_bot is not None:
                            discord_bot.loop.run_until_complete(
                                discord_bot.new_time(
                                    "<@&1166081385732259941> Please verify "
                                    + f"[the new **fastest** time]({time_url}) on '"
                                    + self.trail_name
                                    + "' by '"
                                    + self.network_player.info.steam_name
                                    + "' of "
                                    + our_time
                                )
                            )
                    else:
                        discord_bot = self.network_player.parent.discord_bot
                        if discord_bot is not None:
                            discord_bot.loop.run_until_complete(
                                discord_bot.new_time(
                                    f"||(debug message: [new **non-fastest** time]({time_url}) on '"
                                    + self.trail_name
                                    + "' by '"
                                    + self.network_player.info.steam_name
                                    + "' of "
                                    + our_time + "||"
                                )
                            )
                except RuntimeError as e:
                    logging.warning("Failed to submit time to discord server %s", e)
            except (IndexError, KeyError) as e:
                logging.error("Fastest not found: %s", e)
        else:
            await self.invalidate_timer("Didn't enter all checkpoints.", always=True)
        self.update_leaderboards()
        self.update_medals()
        self.times = []
        self.auto_verify = True

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
                    f"server time was '{(time.time() - self.time_started)}'"
                )
            )

    def update_leaderboards(self):
        """ Update the leaderboards for the trail. """
        pass
        #for net_player in self.network_player.parent.players:
            #net_player.send(
            #    "LEADERBOARD|"
           #     + self.trail_name + "|"
            #    + str(
            #        self.network_player.get_leaderboard(self.trail_name)
            #    )
            #)

    def __new_fastest_time(self, our_time: str):
        """ Called when a new fastest time is set. """
        logging.info(
            "%s '%s'\t- new fastest time!", self.network_player.info.steam_id,
            self.network_player.info.steam_name
        )
        discord_bot = self.network_player.parent.discord_bot
        if discord_bot is not None:
            discord_bot.loop.run_until_complete(
                discord_bot.new_fastest_time(
                    "🎉 New fastest time on **"
                    + self.trail_name
                    + "** by **"
                    + self.network_player.info.steam_name
                    + "**! 🎉\nTime to beat is: "
                    + our_time
                )
            )

    @staticmethod
    def secs_to_str(secs):
        """ Convert seconds to a string. """
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
