""" Used to host the socket server. """
import socket
import threading
import logging
from unity_socket import UnitySocket

class PlayerNotFound(Exception):
    """ Exception called when the descenders unity client could not be found """


class UnitySocketServer():
    """ Used to communicate quickly with the Descenders Unity client. """
    def __init__(self, ip: str, port: int):
        self.host = ip
        self.port = port
        self.discord_bot = None
        self.players = []

    def get_player_by_id(self, _id: str) -> UnitySocket:
        """ Finds the player connected to the socket server from their id """
        for player in self.players:
            if player.steam_id == _id:
                return player
        raise PlayerNotFound("Cannot find player")

    def get_player_by_name(self, name: str) -> UnitySocket:
        """Used to find the player connected to the socket server from their name
          Not to be used reliably, some names may be identical. """
        for player in self.players:
            if player.steam_name == name:
                return player
        raise PlayerNotFound("Cannot find player")

    def create_client(self, conn : socket, addr):
        """ Creates a client from their socket and address """
        logging.info("UnitySocketServer.py - Creating client from addr %s", addr)
        player = UnitySocket(conn, addr, self)
        self.players.append(player)
        with conn:
            player.recieve()
        self.players.remove(player)

    def start(self):
        """ Starts listening on the socket server port and establishes incoming connections """
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            logging.info("Socket server open on %s:%s", self.host, self.port)
            s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
            s.bind((self.host, self.port))
            s.listen()
            while True:
                conn, addr = s.accept()
                logging.info("Establishing client from %s", addr)
                threading.Thread(
                    target=self.create_client,
                    args=(conn, addr)
                ).start()