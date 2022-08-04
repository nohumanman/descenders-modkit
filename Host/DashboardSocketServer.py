from DashboardSocket import DashboardSocket
import socket
import threading
import logging


class DashboardSocketServer():
    def __init__(self, host: str, port: int):
        self.host = host
        self.port = port
        self.discord_bot = None
        self.clients = []

    def create_client(self, conn, addr):
        client = DashboardSocket(conn, addr, self)
        self.clients.append(client)
        try:
            with conn:
                client.recieve()
        except Exception as error:
            logging.info(f"Client has disconnected with error code {error}")
        self.clients.remove(client)

    def start(self):
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            logging.info(f"Socket server open on {self.host} {self.port}")
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
