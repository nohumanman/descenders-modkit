from NetPlayer import NetPlayer
import socket
import threading
import logging


class SocketServer():
    def __init__(self, host: str, port: int):
        self.host = host
        self.port = port
        self.discord_bot = None
        self.players = []

    def get_player_by_id(self, id: str) -> NetPlayer:
        for player in self.players:
            if player.steam_id == id:
                return player

    def create_client(self, conn, addr):
        logging.info("Creating client")
        player = NetPlayer(conn, addr, self)
        self.players.append(player)
        try:
            with conn:
                player.recieve()
        except Exception as error:
            logging.info(f"Client has disconnected with error code {error}")
        self.players.remove(player)

    def start(self):
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            logging.info("Binding with self")
            s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
            s.bind((self.host, self.port))
            logging.info("Bound, listening...")
            s.listen()
            while True:
                print("Waiting for client...")
                conn, addr = s.accept()
                logging.info(f"Connected by {addr}")
                threading.Thread(
                    target=self.create_client,
                    args=(conn, addr)
                ).start()
