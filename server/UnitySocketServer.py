from UnitySocket import UnitySocket
import socket
import threading
import logging


class UnitySocketServer():
    def __init__(self, ip: str, port: int):
        self.host = ip
        self.port = port
        self.discord_bot = None
        self.players = []

    def get_player_by_id(self, id: str) -> UnitySocket:
        for player in self.players:
            if player.steam_id == id:
                return player

    def get_player_by_name(self, name: str) -> UnitySocket:
        for player in self.players:
            if player.steam_name == name:
                return player

    def create_client(self, conn, addr):
        logging.info("UnitySocketServer.py - Creating client")
        player = UnitySocket(conn, addr, self)
        self.players.append(player)
        try:
            with conn:
                player.recieve()
        except Exception as error:
            logging.info(
                "UnitySocketServer.py - Client has "
                f"disconnected with error code {error}"
            )
        self.players.remove(player)

    def start(self):
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            logging.info(
                "UnitySocketServer.py - Socket server"
                f" open on {self.host} {self.port}"
            )
            s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
            s.bind((self.host, self.port))
            s.listen()
            while True:
                conn, addr = s.accept()
                logging.info(f"UnitySocketServer.py - Connected by {addr}")
                threading.Thread(
                    target=self.create_client,
                    args=(conn, addr)
                ).start()