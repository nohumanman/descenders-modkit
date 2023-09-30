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

split_timer_logger = logging.getLogger('DescendersSplitTimer')
split_timer_logger.setLevel(logging.DEBUG)


handler = logging.FileHandler(log_location)
handler.setFormatter(
    logging.Formatter(
        '%(asctime)s - %(name)s - %(levelname)s - %(message)s'
    )
)
handler.setLevel(logging.INFO)
split_timer_logger.addHandler(handler)

split_timer_logger.info(
    "--------------------------------"
    " Descenders Split Timer Started "
    "--------------------------------"
)

# -- Unity Socket Server --

split_timer_logger.info("Main.py - instantiating UnitySocketServer()")

UNITY_SOCKET_IP = "0.0.0.0"
UNITY_SOCKET_PORT = 65432
unity_socket_server = UnitySocketServer(
    UNITY_SOCKET_IP,
    UNITY_SOCKET_PORT
)
threading.Thread(target=unity_socket_server.start).start()

# -- Dashboard Socket Server --

split_timer_logger.info("Main.py - instantiating DashboardSocketServer()")

DASHBOARD_SOCKET_IP = "0.0.0.0"
DASHBOARD_SOCKET_PORT = 65432
dashboard_socket_server = DashboardSocketServer(
    DASHBOARD_SOCKET_IP,
    DASHBOARD_SOCKET_PORT
)

# -- Website Server --

split_timer_logger.info("Main.py - instantiating Webserver()")

WEBSITE_IP = "0.0.0.0"
WEBSITE_PORT = 8080
webserver = Webserver(unity_socket_server)

discord_bot = DiscordBot(discord_token, "!", unity_socket_server)
unity_socket_server.discord_bot = discord_bot
shouldRandomise = True


def riders_gate():
    while True:
        time.sleep(25)
        if (shouldRandomise):
            rand = str(random.randint(0, 3000) / 1000)
            for player in unity_socket_server.players:
                try:
                    player.send("RIDERSGATE|" + rand)
                except Exception:
                    try:
                        split_timer_logger.warning(
                            "Failed to send random gate to player"
                            f" '{player.steam_name}'!"
                        )
                    except Exception:
                        split_timer_logger.warning(
                            "Failed to send random gate to player 'UNKNOWN'!"
                        )


riders_gate_thread = threading.Thread(target=riders_gate)
riders_gate_thread.start()

if __name__ == "__main__":
    webserver.webserver_app.run(
        WEBSITE_IP, port=WEBSITE_PORT,
        debug=True, ssl_context='adhoc'
    )
