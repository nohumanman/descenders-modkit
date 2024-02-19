""" Used to host the socket server. """
import asyncio
import websockets

class WebsiteSocketServer():
    def __init__(self):
        pass

    async def echo(websocket, path):
        async for message in websocket:
            await websocket.send(message)

if __name__ == "__main__":
    website_socket_server = WebsiteSocketServer()
    start_server = websockets.serve(website_socket_server.echo, "localhost", 65430)

    asyncio.get_event_loop().run_until_complete(start_server)
    asyncio.get_event_loop().run_forever()
