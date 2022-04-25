import socket

class NetPlayer():
    def __init__(self, conn : socket):
        self.conn = conn
        self.steam_id = None
        self.steam_name = None
        self.world_name = None
        self.send("PROBE")

    def send(self, data : str):
        self.conn.sendall((data + "\n").encode())

    def handle_data(self, data : str):
        if data == "":
            return
        print(f"Handling data '{data}'")
        if data.startswith("STEAM_ID"):
            self.steam_id = data.split("|")[1]
        elif data.startswith("STEAM_NAME"):
            self.steam_name = data.split("|")[1]
        elif data.startswith("WORLD_NAME"):
            self.world_name = data.split("|")[1]
        elif data.startswith("BOUNDRY_ENTER"):
            self.on_boundry_enter()

    def recieve(self):
        while True:
            data = self.conn.recv(1024)
            if not data:
                break
            for piece in data.decode().split("\n"):
                self.handle_data(piece)

    def ban(self, reason, method):
        self.send("BAN|" + reason + "|" + method)

    def on_respawn(self):
        pass

    def on_bike_switch(self, old_bike, new_bike):
        pass

    def on_boundry_enter(self, boundry_guid, client_time):
        pass

    def on_boundry_exit(self, boundry_guid, client_time):
        pass

    def on_checkpoint_enter(self, checkpoint_num, total_checkpoints, client_time):
        pass

    def on_map_enter(self, map_id, map_name):
        pass

    def on_map_exit(self):
        pass
