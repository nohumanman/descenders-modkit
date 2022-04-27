from NetPlayer import NetPlayer
import socket
import threading

class SocketServer():
    def __init__(self, host : str, port : int):
        self.host = host
        self.port = port
        self.players = []

    def get_player_by_id(self, id : str) -> NetPlayer:
        for player in self.players:
            if player.steam_id == id:
                return player

    def create_client(self, conn):
        print("Watching Client")
        player = NetPlayer(conn)
        self.players.append(player)
        with conn:
            player.recieve()
        self.players.remove(player)

    def start(self):
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            s.bind((self.host, self.port))
            s.listen()
            while True:
                conn, addr = s.accept()
                print(f"Connected by {addr}")
                threading.Thread(
                    target=self.create_client,
                    args=(conn,)
                ).start()
