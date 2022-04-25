from Player import Player
import socket

class SocketServer():
    def __init__(self, host : str, port : int):
        self.host = host
        self.port = port
        self.players = []

    def start(self):
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            s.bind((self.host, self.port))
            s.listen()
            conn, addr = s.accept()
            with conn:
                print(f"Connected by {addr}")
                player = Player(conn)
                self.players.append(player)
                player.recieve()
