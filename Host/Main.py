from DashboardSocketServer import DashboardSocketServer
from Webserver import Webserver
from UnitySocketServer import UnitySocketServer
from DiscordBot import DiscordBot
from Tokens import discord_token
import threading
import time
import random
import logging
import os

script_path = os.path.dirname(os.path.realpath(__file__))
log_location = script_path + "/SplitTimer.log"
logging.basicConfig(
    filename=log_location,
    filemode="w",
    level=logging.DEBUG,
    format='%(asctime)s - %(message)s',
    datefmt='%d-%b-%y %H:%M:%S'
)

logging.info(
    "--------------------------------"
    " Descenders Split Timer Started "
    "--------------------------------"
)

# -- Unity Socket Server --

UNITY_SOCKET_IP = "0.0.0.0"
UNITY_SOCKET_PORT = 65432
unity_socket_server = UnitySocketServer(
    UNITY_SOCKET_IP,
    UNITY_SOCKET_PORT
)

# -- Dashboard Socket Server --

DASHBOARD_SOCKET_IP = "0.0.0.0"
DASHBOARD_SOCKET_PORT = 65432
dashboard_socket_server = DashboardSocketServer(
    DASHBOARD_SOCKET_IP,
    DASHBOARD_SOCKET_PORT
)

# -- Website Server --

WEBSITE_IP = "0.0.0.0"
WEBSITE_PORT = 8080
webserver = Webserver(
    WEBSITE_IP,
    WEBSITE_PORT
)

discord_bot = DiscordBot(discord_token, "!", unity_socket_server)
unity_socket_server.discord_bot = discord_bot
shouldRandomise = True


def riders_gate():
    while True:
        time.sleep(25)
        if (shouldRandomise):
            rand = str(random.randint(0, 3000) / 1000)
            logging.info("Sending Random Gate to all players...")
            for player in unity_socket_server.players:
                try:
                    player.send("RIDERSGATE|" + rand)
                except Exception:
                    logging.warning("Failed to send random gate to player!")
                    try:
                        logging.warning(player.steam_id)
                    except Exception:
                        logging.warning(
                            "Failed to get steam id from failed player!"
                        )


riders_gate_thread = threading.Thread(target=riders_gate)
riders_gate_thread.start()

if __name__ == "__main__":
    webserver.run(
        WEBSITE_IP, port=WEBSITE_PORT,
        debug=True, ssl_context='adhoc'
    )
