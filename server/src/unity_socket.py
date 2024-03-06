""" Used to manipulate socket connection """
from typing import TYPE_CHECKING
import dataclasses
import time
import logging
import os
import asyncio
import aiosqlite
import requests
import srcomapi
import srcomapi.datatypes as dt
from trail_timer import TrailTimer
from tokens import STEAM_API_KEY

if TYPE_CHECKING: # for imports with intellisense
    from unity_socket_server import UnitySocketServer

script_path = os.path.dirname(os.path.realpath(__file__))

operations = {
    "STEAM_ID":
        lambda netPlayer, data: netPlayer.set_steam_id(str(data[1])),
    "STEAM_NAME":
        lambda netPlayer, data: netPlayer.set_steam_name(data[1]),
    "WORLD_NAME":
        lambda netPlayer, data: netPlayer.set_world_name(data[1]),
    "BOUNDRY_ENTER":
        lambda netPlayer, data: netPlayer.on_boundry_enter(data[1], data[2]),
    "BOUNDRY_EXIT":
        lambda netPlayer, data: netPlayer.on_boundry_exit(data[1], data[2]),
    "CHECKPOINT_ENTER":
        lambda netPlayer, data: netPlayer.on_checkpoint_enter(
            data[1],
            data[2],
            int(data[3]),
            float(data[4]),
            data[5]
        ),
    "RESPAWN":
        lambda netPlayer, data: netPlayer.on_respawn(),
    "MAP_ENTER":
        lambda netPlayer, data: netPlayer.on_map_enter(data[2]), # WARNING: data[2] FOR A REASON
    "MAP_EXIT":
        lambda netPlayer, data: netPlayer.on_map_exit(),
    "BIKE_SWITCH":
        lambda netPlayer, data: netPlayer.on_bike_switch(data[2]), # WARNING: data[2] FOR A REASON
    "REP":
        lambda netPlayer, data: netPlayer.set_reputation(data[1]),
    "SPEEDRUN_DOT_COM_LEADERBOARD":
        lambda netPlayer, data: netPlayer.send_leaderboard(data[1]),
    "LEADERBOARD":
        lambda netPlayer, data: netPlayer.send_speedrun_leaderboard(data[1]),
    "CHAT_MESSAGE":
        lambda netPlayer, data: netPlayer.send_chat_message(data[1]),
    "START_SPEED":
        lambda netPlayer, data: netPlayer.start_speed(float(data[1])),
    "TRICK":
        lambda netPlayer, data: netPlayer.set_last_trick(str(data[1])),
    "VERSION":
        lambda netPlayer, data: netPlayer.set_version(str(data[1])),
    "GET_MEDALS":
        lambda netPlayer, data: netPlayer.get_medals(str(data[1])),
    "LOG_LINE":
        lambda netPlayer, data: netPlayer.log_line(str(data[0:])),
}


@dataclasses.dataclass
class Player:
    """ Class to hold instance data of a descenders player """
    steam_name: str
    avatar_src: str
    steam_id: str
    bike_type: str
    world_name: str
    last_trick: str
    reputation: int
    version: str
    time_started: float
    spectating: str
    spectating_id: str

class UnitySocket():
    """ Used to handle the connection to the descenders unity client """
    def __init__(self,
                 addr,
                 parent: 'UnitySocketServer',
                 reader:  asyncio.StreamReader,
                 writer: asyncio.StreamWriter):
        logging.info(
            "%s- New Instance created", addr
        )
        self.addr = addr
        self.parent = parent
        self.dbms = parent.dbms
        self.reader = reader
        self.writer = writer
        self.trails = {}
        self.last_contact = time.time()
        self.sent_non_modkit_notif = False
        self.info: Player = Player(
            steam_name="", steam_id="",
            avatar_src="",
            bike_type="", world_name="",
            last_trick="", reputation=0,
            version="OUTDATED", time_started=time.time(),
            spectating="", spectating_id = ""
        )

    async def log_line(self, line: str):
        """ Log a line of text to the server log """
        if self.info.steam_id == "":
            return
        # remove existing output_log if too large (over 1MB)
        if os.path.exists(f"{script_path}/output_log/{self.info.steam_id}.txt"):
            if os.path.getsize(f"{script_path}/output_log/{self.info.steam_id}.txt") > 1000000:
                os.remove(f"{script_path}/output_log/{self.info.steam_id}.txt")
        # save to output_log/{steam_id}.txt
        with open(f"{script_path}/output_log/{self.info.steam_id}.txt", "a") as file:
            file.write(line + "\n")

    async def send_leaderboard(self, trail_name: str):
        """ Send the leaderboard data for a specific trail to the descenders unity client """
        await self.send(
            "SPEEDRUN_DOT_COM_LEADERBOARD|"
            + trail_name + "|"
            + str(self.convert_to_unity(
                self.get_speedrun_dot_com_leaderboard(trail_name)
            ))
        )

    async def send_speedrun_leaderboard(self, trail_name: str):
        """ Send the leaderboard data for a specific trail to the descenders unity client """
        await self.send(
            "LEADERBOARD|"
            + trail_name + "|"
            + str(
                await self.get_leaderboard(trail_name)
            )
        )

    async def send_chat_message(self, mess: str):
        """ Send a chat message to all players in the same session """
        logging.info(
            "%s '%s'\t- sending chat message '%s'",
            self.info.steam_id, self.info.steam_name, mess
        )
        for player in self.parent.players:
            await player.send(f"CHAT_MESSAGE|{self.info.steam_name}|{self.info.world_name}|{mess}")

    async def set_last_trick(self, trick: str):
        """ Set the last performed trick for a player """
        logging.info(
            "%s '%s'\t- last_trick is %s",
            self.info.steam_id,self.info.steam_name,trick
        )
        self.info.last_trick = trick

    async def set_version(self, version: str):
        """ Set the version of a player's software or application. """
        logging.info(
            "%s '%s'\t- on version %s", self.info.steam_id, self.info.steam_name, version
        )
        self.info.version = version

    async def set_text_colour(self, r: int, g: int, b: int):
        """ Set the text color for a chat message. """
        await self.send(f"SET_TEXT_COLOUR|{r}|{g}|{b}")

    async def set_text_default(self):
        """ Reset the text color to the async default for chat messages """
        await self.send("SET_TEXT_COL_async defAULT")

    async def set_reputation(self, reputation):
        """ Set the reputation value for a player """
        logging.info(
            "%s '%s'\t- reputation is %s", self.info.steam_id,
            self.info.steam_name, reputation
        )
        try:
            self.info.reputation = int(reputation)
        except ValueError:
            pass

    async def start_speed(self, starting_speed: float):
        """ Set the starting speed for a player and invalidate timers if necessary """
        logging.info(
            "%s '%s'\t- start speed is %s",
            self.info.steam_id, self.info.steam_name, starting_speed
        )
        for trail_name, trail in self.trails.items():
            trail.timer_info.starting_speed = starting_speed
            if starting_speed > await self.dbms.get_average_start_time(trail_name)+2:
                await trail.invalidate_timer(
                    "You went through the start too fast!"
                )

    def convert_to_unity(self, leaderboard):
        """ Convert a leaderboard data structure to a Unity-friendly format. """
        if len(leaderboard) == 0:
            return {}
        keys = [key for key in leaderboard[0]]
        unity_leaderboard = {}
        for key in keys:
            unity_leaderboard[key] = []
        for leaderboard_time in leaderboard:
            for key in leaderboard_time:
                unity_leaderboard[key].append(leaderboard_time[key])
        return unity_leaderboard

    async def get_leaderboard(self, trail_name):
        """
        Get and convert the leaderboard data for a specific trail to a Unity-friendly format.
        """
        return self.convert_to_unity(
            [
                {
                    "place": leaderboard["place"],
                    "time": leaderboard["time"],
                    "name": leaderboard["name"],
                    "bike": leaderboard["bike"],
                    "verified": leaderboard["verified"],
                }
                for leaderboard in await self.dbms.get_leaderboard(
                    trail_name
                )
            ]
        )

    def get_speedrun_dot_com_leaderboard(self, trail_name):
        """ Retrieve the leaderboard data for a specific trail from Speedrun.com """
        api = srcomapi.SpeedrunCom()
        game = api.get_game("Descenders")
        for level in game.levels:
            if level.data["name"] == trail_name:
                leaderboard = dt.Leaderboard(
                    api,
                    data=api.get(
                        f"leaderboards/{game.id}/level/{level.id}"
                        f"/7dg4yg4d?embed=variables"
                    )
                )
                leaderboard_json = ([
                    {
                        "place": leaderboard["place"],
                        "time": leaderboard["run"].times["realtime_t"],
                        "name": leaderboard["run"].players[0].name
                    } for leaderboard in leaderboard.runs if (
                        leaderboard["place"] != 0
                    )
                    ])
                return leaderboard_json
        return [{"place": 1, "time": 0, "name": "No times", "verified": "1", "pen": 0}]

    async def get_total_time(self, on_world=False):
        """
        Get the total time spent by a player in the game, optionally within a specific world.
        """
        if on_world:
            return await self.dbms.get_time_on_world(self.info.steam_id, self.info.world_name)
        return await self.dbms.get_time_on_world(self.info.steam_id)

    async def get_avatar_src(self):
        """ Get the URL of the player's avatar image. """
        if self.info.avatar_src != "":
            return self.info.avatar_src
        avatar_src_req = requests.get(
            "https://api.steampowered.com/"
            "ISteamUser/GetPlayerSummaries"
            f"/v0002/?key={STEAM_API_KEY}"
            f"&steamids={self.info.steam_id}", timeout=5
        )
        try:
            self.info.avatar_src = avatar_src_req.json()[
                "response"]["players"][0]["avatarfull"]
        except (IndexError, KeyError):
            try:
                self.info.avatar_src = await self.dbms.get_avatar(self.info.steam_id)
            except aiosqlite.OperationalError:
                self.info.avatar_src = ""
        return self.info.avatar_src

    async def set_steam_name(self, steam_name):
        """ Set the steam name of a player and invalidate timers if necessary """
        logging.info(
            "%s '%s'\t- steam name setting to %s", self.info.steam_id,
            self.info.steam_name, steam_name
        )
        self.info.steam_name = steam_name
        if self.info.steam_id is not None:
            await self.has_both_steam_name_and_id()

    async def ban(self, _type: str):
        """ Ban a player from the game. """
        logging.info(
            "%s '%s'\t- banned with type %s",
            self.info.steam_id, self.info.steam_name, _type
        )
        await self.send("BANNED|" + _type)

    async def has_both_steam_name_and_id(self):
        """ Called when a player has both a steam name and id. """
        await self.dbms.update_player(
            self.info.steam_id,
            self.info.steam_name,
            await self.get_avatar_src()
        )
        for player in self.parent.players:
            if (
                player.info.steam_id == self.info.steam_id
                and self is not player
            ):
                self.parent.players.remove(player)
        if self.info.steam_id in ["OFFLINE", ""]:
            await self.ban("CRASH")
        banned_names = ["descender", "goldberg", "skidrow", "player"]
        for banned_name in banned_names:
            if self.info.steam_name.lower() == banned_name:
                await self.ban("CRASH")

    async def set_steam_id(self, steam_id : str):
        """ Set the steam id of a player and invalidate timers if necessary """
        logging.info(
            "%s '%s'\t- steam id set to %s", self.info.steam_id,
            self.info.steam_name, steam_id
        )
        self.info.steam_id = steam_id
        if self.info.steam_name is not None:
            await self.has_both_steam_name_and_id()
        # if id is not the correct length, ban the player
        if len(steam_id) != len("76561198805366422"):
            await self.send("SUCCESS")

    async def sanity_check(self):
        """ Perform a sanity check on a player's data. """
        # if no steam name, request it
        pass

    async def get_default_bike(self):
        """ Get the async default bike for a player. """
        if self.info.world_name is not None:
            start_bike = "downhill" #await self.dbms.get_start_bike(self.info.world_name)
            if start_bike is None:
                return "enduro"
            return start_bike
        return "enduro"

    async def set_world_name(self, world_name):
        """ Set the world name of a player and invalidate timers if necessary """
        logging.info(
            "%s '%s'\t- set world name to '%s'", self.info.steam_id,
            self.info.steam_name, world_name
        )
        self.info.world_name = world_name
        #await self.dbms.submit_ip(self.info.steam_id, self.addr[0], self.addr[1])

    async def send(self, data: str):
        """ Send data to the descenders unity client """
        logging.info(
            "%s '%s'\t- sending data '%s'", self.info.steam_id, self.info.steam_name, data
        )
        try:
            self.writer.write((data + "\n").encode("utf-8"))
            await self.writer.drain()
        except (BrokenPipeError, ConnectionResetError):
            logging.info(
                "%s '%s'\t- connection closed '%s'", self.info.steam_id, self.info.steam_name, data
            )
            self.parent.delete_player(self)
        except Exception as e:
            logging.info(
                "%s '%s'\t- exception '%s'", self.info.steam_id, self.info.steam_name, e
            )

    async def send_all(self, data: str):
        """ Send data to all players in the same session """
        logging.info(
            "%s '%s'\t- sending to all the data '%s''", self.info.steam_id,
            self.info.steam_name, data
        )
        for player in self.parent.players:
            await player.send(data)

    async def handle_data(self, data: str):
        """ Handle data sent from the descenders unity client """
        self.last_contact = time.time()
        if data == "":
            return
        data_list = data.split("|")
        for operator, function in operations.items():
            self.last_contact = time.time()
            if data.startswith(operator):
                await function(self, data_list)

    async def invalidate_all_trails(self, reason: str, exception = ""):
        """ Invalidate all trails for a player. """
        logging.info(
            "%s '%s'\t- all trails invalidated due to '%s'",
            self.info.steam_id, self.info.steam_name, reason
        )
        for trail_name, trail in self.trails.items():
            if trail_name in self.trails:
                if (trail_name != exception):
                    await trail.invalidate_timer(reason)

    async def on_respawn(self):
        """ Called when a player respawns """
        logging.info("%s '%s'\t- respawned", self.info.steam_id, self.info.steam_name)
        for trail_name, trail in self.trails.items():
            if trail_name in self.trails:
                trail.timer_info.auto_verify = False
                await self.send("SPLIT_TIME|Time requires review")

    async def get_trail(self, trail_name) -> TrailTimer:
        """ Get a trail timer for a player. """
        if trail_name not in self.trails:
            self.trails[trail_name] = TrailTimer(trail_name, self)
        return self.trails[trail_name]

    async def on_bike_switch(self, new_bike: str):
        """ Called when a player switches bikes."""
        self.info.bike_type = new_bike
        #self.send_all(f"SET_BIKE|{self.bike_type}|{self.info.steam_id}")
        await self.invalidate_all_trails("You switched bikes!")

    async def on_boundry_enter(self, trail_name: str, boundry_guid: str):
        """ Called when a player enters a boundry. """
        trail = await self.get_trail(trail_name)
        
        await trail.add_boundary(boundry_guid)

    async def on_boundry_exit(self, trail_name: str, boundry_guid: str):
        """ Called when a player exits a boundry. """
        trail = await self.get_trail(trail_name)
        await trail.remove_boundary(boundry_guid)

    async def on_checkpoint_enter(
        self,
        trail_name: str,
        checkpoint_type: str,
        total_checkpoints: int,
        client_time: float,
        checkpoint_hash: str
    ): 
        """ Called when a player enters a checkpoint. """
        logging.info(
            "%s '%s'\t- entered checkpoint on trail '%s' of type '%s'",
            self.info.steam_id, self.info.steam_name,
            trail_name, checkpoint_type
        )
        trail = await self.get_trail(trail_name)
        if trail is None:
            logging.error(
                "%s '%s' - trail %s not found!",
                self.info.steam_id, self.info.steam_name, trail_name
            )
            return
        trail.timer_info.total_checkpoints = int(total_checkpoints)
        if checkpoint_type == "Start":
            # stop all other timers
            for tr in self.trails:
                if trail_name != tr:
                    self.trails[tr].timer_info.started = False
            await trail.start_timer(total_checkpoints)
        if checkpoint_type == "Intermediate":
            await trail.checkpoint(client_time, checkpoint_hash)
        if checkpoint_type == "Finish":
            await trail.end_timer(client_time)

    async def on_map_enter(self, map_name: str):
        """ Called when a player enters a map. """
        self.info.world_name = map_name
        self.info.time_started = time.time()
        await self.update_concurrent_users()
        # invalidate all trails
        await self.send("INVALIDATE_TIME|\\n")
        # remove all trails
        self.trails = {} # FIXES reentry cheat
        if (self.info.bike_type == "" or self.info.bike_type is None):
            self.info.bike_type = await self.get_default_bike()
        if self.info.steam_id is not None:
            await self.send_all("SET_BIKE|" + self.info.bike_type + "|" + self.info.steam_id)
        # check static/trails to see if there's a csv file with the same name as world_name
        for csv_map_name in os.listdir(f"{script_path}/static/trails"):
            if csv_map_name[0:-4] == map_name:
                await self.send("NON_MODKIT_TRAIL|" + csv_map_name)
                if self.sent_non_modkit_notif:
                    await self.send(
                            "POPUP|Non modkit maps|Heyyy! You've got the Descenders modkit"
                            " loaded right now, and this map has a non-modkit timer. This means"
                            " you can make runs down trails with timers and see others splits"
                            ". It also means there are no boundaries - so your run has to be"
                            " verified by us. - nohumanman"
                    )
                    self.sent_non_modkit_notif = True

    async def on_map_exit(self):
        """ Called when a player exits a map. """
        await self.update_concurrent_users()
        for trail_name, trail in self.trails.items():
            if trail_name in self.trails:
                await trail.invalidate_timer("")
        self.trails = {}

    async def update_concurrent_users(self):
        """ Update the discord bot's presence with the number of concurrent users. """
        discord_bot = self.parent.discord_bot
        if discord_bot is None:
            logging.info("tried to update users before discord bot was created")
        try:
            if discord_bot is not None:
                asyncio.run(discord_bot.set_presence(
                        str(len(self.parent.players))
                        + " racers!"
                ))
        except RuntimeError:
            logging.info("update_concurrent_users() called, but it's already being attempted")

    async def get_medals(self, trail_name: str):
        """ Get the medals for a player on a specific trail. """
        logging.info(
            "%s '%s'\t- fetched medals on trail '%s'",
            self.info.steam_id, self.info.steam_name, trail_name
        )
        (rainbow, gold, silver, bronze) = await self.dbms.get_medals(
            self.info.steam_id,
            trail_name
        )
        await self.send(f"SET_MEDAL|{trail_name}|{rainbow}|{gold}|{silver}|{bronze}")
