""" Used to manipulate socket connection """
from typing import TYPE_CHECKING
from trail_timer import TrailTimer
from tokens import STEAM_API_KEY

if TYPE_CHECKING: # for imports with intellisense
    from website_socket_server import WebsiteSocketServer

script_path = os.path.dirname(os.path.realpath(__file__))

operations = {
    "STEAM_ID":
        lambda client, data: client.set_steam_id(str(data[1])),
    "STEAM_NAME":
        lambda client, data: client.set_steam_name(data[1]),
}

class WebsiteSocket():
    """ Used to handle the connection to the descenders unity client """
    def __init__(self,
                 addr,
                 parent: 'WebsiteSocketServer',
                 reader:  asyncio.StreamReader,
                 writer: asyncio.StreamWriter):
        logging.info(
            "%s- New Instance created", addr
        )
        self.addr = addr
        self.parent = parent
        self.dbms = parent.dbms
        self.reader = reader
        self.writer = writer

    async def send(self, data: str):
        """ Send data to the descenders unity client """
        logging.info(
            "%s '%s'\t- sending data '%s'", self.info.steam_id, self.info.steam_name, data
        )
        try:
            self.writer.write((data + "\n").encode("utf-8"))
            await self.writer.drain()
        except (BrokenPipeError, ConnectionResetError):
            logging.info(
                "%s '%s'\t- connection closed '%s'", self.info.steam_id, self.info.steam_name, data
            )
            self.parent.delete_player(self)
        except Exception as e:
            logging.info(
                "%s '%s'\t- exception '%s'", self.info.steam_id, self.info.steam_name, e
            )

    async def handle_data(self, data: str):
        """ Handle data sent from the descenders unity client """
        self.last_contact = time.time()
        if data == "":
            return
        data_list = data.split("|")
        for operator, function in operations.items():
            self.last_contact = time.time()
            if data.startswith(operator):
                await function(self, data_list)
