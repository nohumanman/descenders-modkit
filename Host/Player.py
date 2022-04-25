import socket

class NetPlayer():
    def __init__(self, conn : socket):
        self.conn = conn
        self.steam_id = None
        self.steam_name = None
        self.world_name = None
        self.gates = []
        self.send("SUCCESS")

    def send(self, data : str):
        self.conn.sendall((data + "\n").encode())

    def handle_data(self, data : str):
        if data == "":
            return
        print(f"Handling data '{data}'")
        data_list = data.split("|")
        if data.startswith("STEAM_ID"):
            self.steam_id = data_list[1]
        elif data.startswith("STEAM_NAME"):
            self.steam_name = data_list[1]
        elif data.startswith("WORLD_NAME"):
            self.world_name = data_list[1]
        elif data.startswith("BOUNDRY_ENTER"):
            self.on_boundry_enter(data_list[1], data_list[2])
        elif data.startswith("BOUNDRY_EXIT"):
            self.on_boundry_exit(data_list[1], data_list[2])
        elif data.startswith("CHECKPOINT_ENTER"):
            self.on_checkpoint_enter(data_list[1], data_list[2], data_list[3])
        elif data.startswith("RESPAWN"):
            self.on_respawn()
        elif data.startswith("MAP_ENTER"):
            self.on_map_enter(data_list[1], data_list[2])
        elif data.startswith("MAP_EXIT"):
            self.on_map_exit()
        elif data.startswith("BIKE_SWITCH"):
            self.on_bike_switch(data_list[1], data_list[2])
        elif data.startswith("GATE_NAME"):
            if (data_list[1] not in self.gates):
                self.gates.append(data_list[1])

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

    def on_bike_switch(self, old_bike : str, new_bike : str):
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
