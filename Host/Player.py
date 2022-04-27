import socket

operations = {
    "STEAM_ID" : lambda (netPlayer, data) : netPlayer.steam_id = data[1],
    "STEAM_NAME" : lambda (netPlayer, data) : netPlayer.steam_name = data[1],
    "WORLD_NAME" : lambda (netPlayer, data) : netPlayer.world_name = data[1],
    "BOUNDRY_ENTER" : lambda (netPlayer, data) : netPlayer.on_boundry_enter(data[1], data[2])
    "BOUNDRY_EXIT" : lambda (netPlayer, data) : netPlayer.on_boundry_exit(data[1], data[2]),
    "CHECKPOINT_ENTER" : lambda (netPlayer, data) : netPlayer.on_checkpoint_enter(data[1], data[2], data[3]),
    "RESPAWN" : lambda (netPlayer, data) : netPlayer.on_respawn(),
    "MAP_ENTER" : lambda (netPlayer, data) : netPlayer.on_map_enter(data[1], data[2]),
    "MAP_EXIT" : lambda (netPlayer, data) : netPlayer.on_map_exit(),
    "BIKE_SWITCH" : lambda (netPlayer, data) : netPlayer.on_bike_switch(data[1], data[2]),
    "GATE_NAME" : lambda (netPlayer, data) : if (data_list[1] not in netPlayer.gates): netPlayer.gates.append(data_list[1])
}

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
        for operator in operations:
            if data.startswith(operator):
                operations[operator](self, data_list)

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
