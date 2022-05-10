from NetPlayer import NetPlayer
import socket
import threading
import logging


class SocketServer():
    def __init__(self, host: str, port: int):
        self.host = host
        self.port = port
        self.players = []

    def get_player_by_id(self, id: str) -> NetPlayer:
        for player in self.players:
            if player.steam_id == id:
                return player

    def create_client(self, conn, addr):
        logging.info("Creating client")
        player = NetPlayer(conn, addr)
        self.players.append(player)
        try:
            with conn:
                player.recieve()
        except OSError:
            logging.info("Client has disconnected")
        self.players.remove(player)

    def start(self):
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            s.bind((self.host, self.port))
            s.listen()
            while True:
                conn, addr = s.accept()
                logging.info(f"Connected by {addr}")
                threading.Thread(
                    target=self.create_client,
                    args=(conn, addr)
                ).start()
