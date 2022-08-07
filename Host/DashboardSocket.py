import socket
from UnitySocketServer import UnitySocketServer
import logging
from DBMS import DBMS


class DashboardSocket():
    def __init__(self, conn: socket, addr, parent, unity_socket_server: UnitySocketServer):
        self.addr = addr
        self.conn = conn
        self.parent = parent
        self.unity_socket_server = unity_socket_server
        self.send("SUCCESS")
    
    def send(self, data: str):
        logging.debug(f"id{self.steam_id} alias {self.steam_name} - Sending data '{data}'")
        self.conn.sendall((data + "\n").encode())

    def get_spectated_info(self):
        for player in self.unity_socket_server.players:
            if player.spectating != "":
                spectated_player = self.socket_server.get_player_by_name(player.spectating)
                return {
                    "trails": [
                        {
                            "trail_name": trail,
                            "time_started" : spectated_player.get_trail(trail).time_started,
                            "starting_speed": spectated_player.get_trail(trail).starting_speed,
                            "started": spectated_player.get_trail(trail).started,
                            "last_time": spectated_player.get_trail(trail).time_ended
                        }
                        for trail in spectated_player.trails
                    ],
                    "bike_type": spectated_player.bike_type,
                    "rep": spectated_player.reputation
                }
        return "None Found"

    def spectating(self, steam_id: str, player_name: str, target_id: str):
        try:
            for player in self.unity_socket_server.players:
                player.being_monitored = False
            self.unity_socket_server.get_player_by_id(steam_id).spectating = player_name
            self.unity_socket_server.get_player_by_id(target_id).being_monitored = True
            return "Gotcha"
        except Exception as e:
            return str(e)
    
    def toggle_ignore_time(self, time_id, val):
        if self.permission() == "AUTHORISED":
            DBMS().set_ignore_time(time_id, val)
            return "Done"
        else:
            return "NOT VALID"

    def toggle_monitored(self, time_id, val):
        if self.permission() == "AUTHORISED":
            DBMS().set_monitored(time_id, val)
            return "Done"
        else:
            return "NOT VALID"

    def get_spectating(self, steam_id):
        return self.unity_socket_server.get_player_by_id(steam_id).spectating

    def eval(self, id):
        cmd = "SUCCESS"
        try:
            if self.permission() == "AUTHORISED":
                try:
                    self.unity_socket_server.get_player_by_id(
                        id
                    ).send(cmd)
                    if cmd.startswith("SET_BIKE"):
                        if cmd[9:10] == "1":
                            self.unity_socket_server.get_player_by_id(id).bike_type = "downhill"
                        elif cmd[9:10] == "0":
                            self.unity_socket_server.get_player_by_id(id).bike_type = "enduro"
                        elif cmd[9:10] == "2":
                            self.unity_socket_server.get_player_by_id(id).bike_type = "hardtail"
                except Exception as e:
                    logging.error(e)
                    return e
                return "Hello World!"
            else:
                return "FAILED - NOT VALID PERMISSIONS!"
        except Exception as e:
            return str(e)

    def get(self):
        return ""
        '''return jsonify(
            {
                'ids':
                [
                    {
                        "id": player.steam_id,
                        "name": player.steam_name,
                        "steam_avatar_src": player.get_avatar_src(),
                        "total_time": player.get_total_time(),
                        "time_on_world": player.get_total_time(onWorld=True),
                        "world_name": player.world_name,
                        "reputation": player.reputation,
                        "last_trick": player.last_trick,
                        "version": player.version,
                        "trails": [
                            player.trails[trail].get_boundaries()
                            for trail in player.trails
                        ],
                        "bike_type": player.bike_type,
                        "time_loaded": player.time_started,
                    } for player in self.unity_socket_server.players
                ]
            }
        )'''

    def randomise(self):
        if self.permission() == "AUTHORISED":
            global shouldRandomise
            shouldRandomise = not shouldRandomise
            return "123"
        return "FAILED - NOT VALID PERMISSIONS!"

    def permission(self):
        pass



