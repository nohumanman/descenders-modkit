""" Main function of descenders-modkit server """
import threading
import time
import random
import logging
import os
from unity_socket_server import UnitySocketServer
from discord_bot import DiscordBot
from tokens import DISCORD_TOKEN
from webserver import Webserver

script_path = os.path.dirname(os.path.realpath(__file__))

# Prevent logging from outputting to stdout
logging.basicConfig(
    filename=script_path + '/SplitTimer.log',
    level=logging.INFO,
    format=(
        '%(asctime)s - %(name)s\t - %(levelname)s\t'
        ' - %(filename)s\t - Line %(lineno)d:\t %(message)s'
    )
)

os.chdir(script_path)
log_location = script_path + "/SplitTimer.log"

split_timer_logger = logging.getLogger('DescendersSplitTimer')
split_timer_logger.setLevel(logging.INFO)

handler = logging.FileHandler(log_location)
handler.setFormatter(
    logging.Formatter(
        '%(asctime)s - %(name)s\t - %(levelname)s\t'
        ' - %(filename)s\t - Line %(lineno)d:\t %(message)s'
    )
)
handler.setLevel(logging.WARNING)
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

# -- Website Server --

split_timer_logger.info("Main.py - instantiating Webserver()")

WEBSITE_IP = "0.0.0.0"
WEBSITE_PORT = 8080
webserver = Webserver(unity_socket_server)

discord_bot = DiscordBot(DISCORD_TOKEN, "!", unity_socket_server)
unity_socket_server.discord_bot = discord_bot
SHOULD_RANDOMISE = True


def riders_gate():
    """ Function to call the 'randomgate' function on """
    while True:
        time.sleep(25)
        if SHOULD_RANDOMISE:
            rand = str(random.randint(0, 3000) / 1000)
            for player in unity_socket_server.players:
                player.send("RIDERSGATE|" + rand)

riders_gate_thread = threading.Thread(target=riders_gate)
riders_gate_thread.start()

if __name__ == "__main__":
    webserver.webserver_app.run(
        WEBSITE_IP, port=WEBSITE_PORT,
        debug=True, ssl_context='adhoc'
    )
