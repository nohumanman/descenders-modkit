import asyncio
import websockets
import json
from unity_socket_server import UnitySocketServer

class WebSocketServer:
    def __init__(self, host, port, unity_socket_server : UnitySocketServer):
        self.host = host
        self.port = port
        self.unity_socket_server = unity_socket_server
        self.websockets = []

    async def send_players_all(self):
        for websocket in self.websockets:
            await self.send_players(websocket)

    async def send_players(self, websocket):
        await websocket.send(json.dumps([{
            "steam_id": player.info.steam_id,
            "steam_name": player.info.steam_name,
            "steam_avatar_src": player.info.avatar_src,
            "reputation": player.info.reputation,
            "world_name": player.info.world_name,
            "last_trick": player.info.last_trick,
            "version": player.info.version,
            "bike_type": player.info.bike_type,
            "trail_info": [
                {
                    "trail_name": player.trails[trail_name].trail_name,
                    "started": player.trails[trail_name].timer_info.started,
                    "auto_verify": player.trails[trail_name].timer_info.auto_verify,
                    "times": player.trails[trail_name].timer_info.times,
                    "starting_speed": player.trails[trail_name].timer_info.starting_speed,
                    "total_checkpoints": player.trails[trail_name].timer_info.total_checkpoints,
                    "time_started": player.trails[trail_name].timer_info.time_started
                }
                for trail_name in player.trails
            ]
        } for player in self.unity_socket_server.players]))

    async def handle(self, websocket: websockets.WebSocketClientProtocol, path):
        self.websockets.append(websocket)
        try:
            async for message in websocket:
                if message == "GET":
                    await self.send_players(websocket)
        except websockets.exceptions.ConnectionClosedOK:
            self.websockets.remove(websocket)

    async def start_server(self):
        start_server = websockets.serve(self.handle, self.host, self.port)
        async with start_server:
            await asyncio.Future()  # run forever

    def run_server(self):
        asyncio.run(self.start_server())

if __name__ == "__main__":
    server = WebSocketServer("localhost", 65430, None)
    asyncio.run(server.start_server())
