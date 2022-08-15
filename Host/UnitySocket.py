import socket
import time
import srcomapi
import srcomapi.datatypes as dt
from DBMS import DBMS
from TrailTimer import TrailTimer
import requests
from Tokens import steam_api_key
import logging
import json
import os

script_path = os.path.dirname(os.path.realpath(__file__))

split_timer_logger = logging.getLogger('DescendersSplitTimer')

operations = {
    "STEAM_ID":
        lambda netPlayer, data: netPlayer.set_steam_id(data[1]),
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
            data[3],
            data[4]
        ),
    "RESPAWN":
        lambda netPlayer, data: netPlayer.on_respawn(),
    "MAP_ENTER":
        lambda netPlayer, data: netPlayer.on_map_enter(data[1], data[2]),
    "MAP_EXIT":
        lambda netPlayer, data: netPlayer.on_map_exit(),
    "BIKE_SWITCH":
        lambda netPlayer, data: netPlayer.on_bike_switch(data[1], data[2]),
    "REP":
        lambda netPlayer, data: netPlayer.set_reputation(data[1]),
    "SPEEDRUN_DOT_COM_LEADERBOARD":
        lambda netPlayer, data: (
            netPlayer.send(
                "SPEEDRUN_DOT_COM_LEADERBOARD|"
                + data[1] + "|"
                + str(netPlayer.convert_to_unity(
                    netPlayer.get_speedrun_dot_com_leaderboard(data[1])
                ))
            )
        ),
    "LEADERBOARD":
        lambda netPlayer, data: (
            netPlayer.send(
                "LEADERBOARD|"
                + data[1] + "|"
                + str(
                    netPlayer.get_leaderboard(data[1])
                )
            )
        ),
    "START_SPEED":
        lambda netPlayer, data: netPlayer.start_speed(float(data[1])),
    "TRICK":
        lambda netPlayer, data: netPlayer.set_last_trick(str(data[1])),
    "VERSION":
        lambda netPlayer, data: netPlayer.set_version(str(data[1])),
    "BIKE_TYPE":
        lambda netPlayer, data: netPlayer.set_bike(str(data[1])),
    "GET_MEDALS":
        lambda netPlayer, data: netPlayer.get_medals(str(data[1]))
}


class UnitySocket():
    def __init__(self, conn: socket, addr, parent):
        split_timer_logger.info(
            "UnitySocket.py - New UnitySocket instance created"
        )
        self.addr = addr
        self.conn = conn
        self.parent = parent
        self.trails = {}
        self.__avatar_src = None
        self.steam_id = None
        self.steam_name = None
        self.bike_type = "enduro"
        self.world_name = None
        self.spectating = ""
        self.being_monitored = False
        self.last_trick = ""
        self.reputation = 6969
        self.version = "OUTDATED"
        self.time_started = time.time()
        self.send("SUCCESS")
        self.send("INVALIDATE_TIME|scripts by nohumanman")

    def set_last_trick(self, trick: str):
        split_timer_logger.info(
            f"UnitySocket.py - {self.steam_id} '{self.steam_name}'"
            f" called set_last_trick('{trick}')"
        )
        self.last_trick = trick

    def set_bike(self, bike: str):
        split_timer_logger.info(
            f"UnitySocket.py - {self.steam_id} '{self.steam_name}'"
            f" called set_bike('{bike}')"
        )
        self.bike_type = bike

    def set_version(self, version: str):
        split_timer_logger.info(
            f"UnitySocket.py - {self.steam_id} '{self.steam_name}'"
            f" called set_version('{version}')"
        )
        self.version = version
        with open(script_path + "/current_version.json") as json_file:
            data = json.load(json_file)
            if version != data["latest_version"]:
                latest_version = data["latest_version"]
                self.send(
                    f"INVALIDATE_TIME|You are on version {version}, "
                    f"latest is {latest_version} - Please restart your game."
                )

    def set_reputation(self, reputation):
        split_timer_logger.info(
            f"UnitySocket.py - {self.steam_id} '{self.steam_name}'"
            f" called set_reputation({reputation})"
        )
        self.reputation = int(reputation)
        DBMS.log_rep(self.steam_id, self.reputation)

    def start_speed(self, starting_speed: float):
        split_timer_logger.info(
            f"UnitySocket.py - {self.steam_id} '{self.steam_name}'"
            f" called start_speed({starting_speed})"
        )
        if starting_speed > 50:
            self.send("INVALIDATE_TIME|You went through the start too fast!")
        for trail in self.trails:
            self.trails[trail].starting_speed = starting_speed

    def convert_to_unity(self, leaderboard):
        if len(leaderboard) == 0:
            return {}
        keys = [key for key in leaderboard[0]]
        unityLeaderboard = {}
        for key in keys:
            unityLeaderboard[key] = []
        for leaderboardTime in leaderboard:
            for key in leaderboardTime:
                unityLeaderboard[key].append(leaderboardTime[key])
        return unityLeaderboard

    def get_leaderboard(self, trail_name):
        split_timer_logger.info(
            f"UnitySocket.py - {self.steam_id} '{self.steam_name}'"
            f" called get_leaderboard('{trail_name}')"
        )
        return self.convert_to_unity(
            [
                {
                    "place": leaderboard["place"],
                    "time": leaderboard["time"],
                    "name": leaderboard["name"],
                    "bike": leaderboard["bike"]
                    } for leaderboard in DBMS.get_leaderboard(
                    trail_name
                )
                ]
            )

    def get_speedrun_dot_com_leaderboard(self, trail_name):
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
        return {}

    def get_total_time(self, onWorld=False):
        if onWorld:
            return DBMS.get_time_on_world(self.steam_id, self.world_name)
        return DBMS.get_time_on_world(self.steam_id)

    def get_avatar_src(self):
        if self.__avatar_src is not None:
            return self.__avatar_src
        avatar_src_req = requests.get(
            "https://api.steampowered.com/"
            "ISteamUser/GetPlayerSummaries"
            f"/v0002/?key={steam_api_key}"
            f"&steamids={self.steam_id}"
        )
        try:
            self.__avatar_src = avatar_src_req.json()[
                "response"]["players"][0]["avatarfull"]
        except Exception:
            try:
                self.__avatar_src = DBMS().get_avatar(self.steam_id)
            except Exception:
                self.__avatar_src = ""
        return self.__avatar_src

    def set_steam_name(self, steam_name):
        split_timer_logger.info(
            f"UnitySocket.py - {self.steam_id} '{self.steam_name}'"
            f" - set_steam_name('{steam_name}')"
        )
        self.steam_name = steam_name
        if self.steam_id is not None:
            self.has_both_steam_name_and_id()

    def ban(self, type: str):
        split_timer_logger.info(
            f"UnitySocket.py - {self.steam_id} '{self.steam_name}'"
            " called ban()"
        )
        # type - CLOSE, CRASH, ILLEGAL
        if type == "ILLEGAL":
            self.send("TOGGLE_GOD")
        self.send("BANNED|" + type)
        logging.info(
            f"id{self.steam_id}) - alias '{self.steam_name}'"
            f" banned with '{type}'."
        )
        discord_bot = self.parent.discord_bot
        discord_bot.loop.run_until_complete(
            discord_bot.ban_note(
                f"Player {self.steam_name} (id{self.steam_id}) tried"
                f" to join but was banned with '{type}'."
            )
        )

    def has_both_steam_name_and_id(self):
        DBMS.submit_alias(self.steam_id, self.steam_name)
        for player in self.parent.players:
            if (
                player.steam_id == self.steam_id
                and self is not player
            ):
                logging.warning(
                    f"id{self.steam_id} alias {self.steam_name}"
                    " - duplicate steam id!"
                )
                self.parent.players.remove(player)
                del(player)
        if self.steam_id == "OFFLINE" or self.steam_id == "":
            self.send("TOGGLE_GOD")
        banned_names = ["descender", "goldberg", "skidrow", "player"]
        for banned_name in banned_names:
            if (self.steam_name.lower() == banned_name):
                self.ban("ILLEGAL")
        ban_type = DBMS().get_ban_status(self.steam_id)
        if ban_type == "CLOSE":
            self.ban("CLOSE")
        elif ban_type == "CRASH":
            self.ban("CRASH")
        elif ban_type == "ILLEGAL":
            self.ban("ILLEGAL")

    def set_steam_id(self, steam_id):
        split_timer_logger.info(
            f"UnitySocket.py - {self.steam_id} '{self.steam_name}'"
            f" - set_steam_id({steam_id})"
        )
        self.steam_id = steam_id
        if self.steam_name is not None:
            self.has_both_steam_name_and_id()

    def set_world_name(self, world_name):
        split_timer_logger.info(
            f"UnitySocket.py - {self.steam_id} '{self.steam_name}'"
            f" - set_world_name('{world_name}')"
        )
        self.world_name = world_name
        DBMS().update_player(
            self.steam_id,
            self.steam_name,
            self.get_avatar_src()
        )
        DBMS.submit_ip(self.steam_id, self.addr[0], self.addr[1])

    def send(self, data: str):
        split_timer_logger.info(
            f"UnitySocket.py - {self.steam_id} '{self.steam_name}'"
            f" called send('{data}')"
        )
        self.conn.sendall((data + "\n").encode())

    def handle_data(self, data: str):
        if data == "":
            return
        # split_timer_logger.info(
        #     f"UnitySocket.py - {self.steam_id} '{self.steam_name}'"
        #     f" called handle_data('{data}')"
        # )
        data_list = data.split("|")
        for operator in operations:
            if data.startswith(operator):
                try:
                    operations[operator](self, data_list)
                except Exception as e:
                    logging.error(e)

    def recieve(self):
        while True:
            try:
                data = self.conn.recv(1024)
            except ConnectionResetError:
                split_timer_logger.warn(
                    f"UnitySocket.py - {self.steam_id} '{self.steam_name}'"
                    f" user has disconnected due to ConnectionResetError"
                )
                break
            if not data:
                split_timer_logger.warn(
                    f"UnitySocket.py - {self.steam_id} '{self.steam_name}'"
                    f" user has disconnected because not data"
                )
                break
            for piece in data.decode().split("\n"):
                self.handle_data(piece)
        del(self)

    def invalidate_all_trails(self, reason: str):
        split_timer_logger.info(
            f"UnitySocket.py - {self.steam_id} '{self.steam_name}'"
            f" called invalidate_all_trails('{reason}')"
        )
        for trail in self.trails:
            self.trails[trail].invalidate_timer(reason)

    def on_respawn(self):
        split_timer_logger.info(
            f"UnitySocket.py - {self.steam_id} '{self.steam_name}'"
            f" called on_respawn()"
        )
        if not self.being_monitored:
            if str(self.steam_id) == "76561198314526424":
                self.invalidate_all_trails("THOU HAST EATEN SHIT")
                return
            if str(self.steam_id) == "76561198154432619":
                self.invalidate_all_trails("R.I.P. Bozo")
                return
            if str(self.steam_id) == "76561198113876228":
                self.invalidate_all_trails("Skill issue.")
                return
            self.invalidate_all_trails("You respawned!")

    def get_trail(self, trail_name) -> TrailTimer:
        if trail_name not in self.trails:
            self.trails[trail_name] = TrailTimer(trail_name, self)
        return self.trails[trail_name]

    def on_bike_switch(self, old_bike: str, new_bike: str):
        self.bike_type = new_bike
        self.invalidate_all_trails("You switched bikes!")

    def on_boundry_enter(self, trail_name: str, boundry_guid: str):
        trail = self.get_trail(trail_name)
        trail.add_boundary(boundry_guid)

    def on_boundry_exit(self, trail_name: str, boundry_guid: str):
        trail = self.get_trail(trail_name)
        trail.remove_boundary(boundry_guid)

    def on_checkpoint_enter(
        self,
        trail_name: str,
        type: str,
        total_checkpoints: str,
        client_time: str
    ):
        split_timer_logger.info(
            f"UnitySocket.py - {self.steam_id} '{self.steam_name}'"
            f" called on_checkpoint_enter('{trail_name}', '{type}', "
            f"'{total_checkpoints}', '{client_time}')"
        )
        self.get_trail(trail_name).total_checkpoints = int(total_checkpoints)
        if type == "Start":
            self.get_trail(trail_name).start_timer(total_checkpoints)
        if type == "Intermediate":
            self.get_trail(trail_name).checkpoint(client_time)
        if type == "Finish":
            self.get_trail(trail_name).end_timer(client_time)

    def on_map_enter(self, map_id, map_name):
        self.time_started = time.time()
        self.update_concurrent_users()

    def on_map_exit(self):
        self.update_concurrent_users()
        self.trails = {}
        DBMS.end_session(
            self.steam_id,
            self.time_started,
            time.time(),
            self.world_name
        )
        self.conn.close()

    def update_concurrent_users(self):
        discord_bot = self.parent.discord_bot
        try:
            discord_bot.loop.run_until_complete(
                discord_bot.watch_user(
                    str(len(self.parent.players))
                    + " concurrent users!"
                )
            )
        except RuntimeError:
            logging.error("Event already running")

    def get_medals(self, trail_name: str):
        split_timer_logger.info(
            f"UnitySocket.py - {self.steam_id} '{self.steam_name}'"
            f" called get_medals('{trail_name}')"
        )
        (rainbow, gold, silver, bronze) = DBMS.get_medals(
            self.steam_id,
            trail_name
        )
        split_timer_logger.info(
            f"UnitySocket.py - {self.steam_id} '{self.steam_name}'"
            f" medals found - ({rainbow}, {gold}, {silver}, {bronze})"
        )
        self.send(f"SET_MEDAL|{trail_name}|{rainbow}|{gold}|{silver}|{bronze}")
