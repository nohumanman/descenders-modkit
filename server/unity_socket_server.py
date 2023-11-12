""" Used to host the socket server. """
from typing import TYPE_CHECKING
from typing import Union
import asyncio
from asyncio import StreamReader, StreamWriter
import logging
from unity_socket import UnitySocket
from dbms import DBMS

if TYPE_CHECKING: # for imports with intellisense
    from discord_bot import DiscordBot

class PlayerNotFound(Exception):
    """ Exception called when the descenders unity client could not be found """


class UnitySocketServer():
    """ Used to communicate quickly with the Descenders Unity client. """
    def __init__(self, ip: str, port: int, dbms: DBMS):
        self.host = ip
        self.port = port
        self.dbms = dbms
        self.discord_bot : DiscordBot | None = None
        self.players: list[UnitySocket] = []

    def get_player_by_id(self, _id: str) -> UnitySocket:
        """ Finds the player connected to the socket server from their id """
        for player in self.players:
            if player.info.steam_id == _id:
                return player
        raise PlayerNotFound("Cannot find player")

    def get_player_by_name(self, name: str) -> UnitySocket:
        """Used to find the player connected to the socket server from their name
          Not to be used reliably, some names may be identical. """
        for player in self.players:
            if player.info.steam_name == name:
                return player
        raise PlayerNotFound("Cannot find player")

    def get_player_by_addr(self, addr: tuple[str, int]) -> Union[UnitySocket, None]:
        """ Finds the player connected to the socket server from their address """
        for player in self.players:
            if player.addr == addr:
                return player
        return None

    async def create_client(self, reader: StreamReader, writer: StreamWriter):
        """ Creates a client from their socket and address """
        logging.info("create_client")
        player = self.get_player_by_addr(writer.get_extra_info('peername'))
        if player is None:
            player = UnitySocket(writer.get_extra_info('peername'), self, reader, writer)
            self.players.append(player)
        logging.info("Created player")
        await player.send("SUCCESS")
        while True:
            data = await reader.read(1024)
            if data == b'':
                logging.info("Recieved EOF. Client disconnected.")
                return
            message = data.decode()
            addr = writer.get_extra_info('peername')
            #logging.info(f"Received {message!r} from {addr!r}")
            for mess in message.split("\n"):
                await player.handle_data(mess)
