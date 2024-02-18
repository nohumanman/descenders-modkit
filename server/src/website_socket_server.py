""" Used to host the socket server. """
from typing import TYPE_CHECKING
from typing import Union
import time
import asyncio
from asyncio import StreamReader, StreamWriter
import logging
from unity_socket import UnitySocket
from dbms import DBMS

if TYPE_CHECKING: # for imports with intellisense
    from discord_bot import DiscordBot


class WebsiteSocketServer():
    """ Used to communicate quickly with the Descenders Unity client. """
    def __init__(self, ip: str, port: int, dbms: DBMS):
        self.host = ip
        self.port = port
        self.dbms = dbms
        self.timeout = 120
        self.discord_bot : DiscordBot | None = None
        self.clients: list[UnitySocket] = []

    def get_discord_bot(self) -> 'DiscordBot':
        """ Returns the discord bot """
        if self.discord_bot is None:
            raise RuntimeError("Discord bot not set")
        return self.discord_bot

    def delete_player(self, player: UnitySocket):
        """ Deletes the player from the socket server """
        if player not in self.clients:
            return # player already deleted
        self.clients.remove(player)
        player.writer.close()
        del player

    async def delete_timed_out_clients(self):
        """ Deletes clients that have timed out """
        for player in self.clients:
            await player.sanity_check()
            if (time.time() - player.last_contact) > self.timeout:
                logging.info("%s '%s' - contact timeout disconnect",
                             player.info.steam_id, player.info.steam_name)
                self.delete_player(player)

    def get_player_by_id(self, _id: str) -> UnitySocket:
        """ Finds the player connected to the socket server from their id """
        for player in self.clients:
            if player.info.steam_id == _id:
                return player
        raise PlayerNotFound("Cannot find player")

    def get_player_by_name(self, name: str) -> UnitySocket:
        """Used to find the player connected to the socket server from their name
          Not to be used reliably, some names may be identical. """
        for player in self.clients:
            if player.info.steam_name == name:
                return player
        raise PlayerNotFound("Cannot find player")

    def get_player_by_addr(self, addr: tuple[str, int]) -> Union[UnitySocket, None]:
        """ Finds the player connected to the socket server from their address """
        for player in self.clients:
            if player.addr == addr:
                return player
        return None

    async def riders_gate(self):
        while True:
            for player in self.clients:
                await player.send("RIDERSGATE|2")
            time.sleep(10)

    async def create_client(self, reader: StreamReader, writer: StreamWriter):
        """ Creates a client from their socket and address """
        logging.info("create_client")
        await self.delete_timed_out_clients()
        player = self.get_player_by_addr(writer.get_extra_info('peername'))
        if player is None:
            player = UnitySocket(writer.get_extra_info('peername'), self, reader, writer)
            self.clients.append(player)
        logging.info("Created player")
        await player.send("SUCCESS")
        message_buffer = ""
        while True:
            try:
                data = await asyncio.wait_for(reader.readline(), timeout=20)
            except (asyncio.TimeoutError, ConnectionResetError):
                logging.info("%s '%s' - asyncio timeout",
                             player.info.steam_id, player.info.steam_name)
                self.delete_player(player)
                return
            if data == b'':
                logging.info("%s '%s' - eof timeout", player.info.steam_id, player.info.steam_name)
                self.delete_player(player)
                return
            message = message_buffer + data.decode()
            if message[-1] != "\n":
                # message not complete, wait for next message
                message_buffer += message # store the message
            else:
                message_buffer = ""
                if player is None:
                    logging.error("Player is None")
                    return
                for mess in message.split("\n"):
                    if (mess != "" and not(mess.startswith("LOG_LINE") and not(mess.startswith("pong")))):
                        logging.info("%s '%s' - %s", player.info.steam_id, player.info.steam_name, mess)
                    try:
                        asyncio.create_task(player.handle_data(mess)) # asyncronously handle the data
                    except Exception as e:
                        logging.error(e)
