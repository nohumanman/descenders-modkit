""" Main function of descenders-modkit server """
import threading
import logging
import os
from unity_socket_server import UnitySocketServer
from discord_bot import DiscordBot
from tokens import DISCORD_TOKEN
from webserver import Webserver
from dbms import DBMS

script_path = os.path.dirname(os.path.realpath(__file__))

# Prevent logging from outputting to stdout
logging.basicConfig(
    filename=script_path + '/modkit.log',
    level=logging.DEBUG,
    format=(
        '%(asctime)s - %(name)s\t - %(levelname)s\t'
        ' - %(filename)s\t - Line %(lineno)d:\t %(message)s'
    )
)

os.chdir(script_path)

# -- Unity Socket Server --

logging.info("Main.py - instantiating UnitySocketServer()")

dbms = DBMS()

UNITY_SOCKET_IP = "0.0.0.0"
UNITY_SOCKET_PORT = 65432
unity_socket_server = UnitySocketServer(
    UNITY_SOCKET_IP,
    UNITY_SOCKET_PORT,
    dbms
)
threading.Thread(target=unity_socket_server.start).start()

# -- Website Server --

logging.info("Main.py - instantiating Webserver()")

WEBSITE_IP = "0.0.0.0"
WEBSITE_PORT = 8080
webserver = Webserver(unity_socket_server, dbms)

discord_bot = DiscordBot(DISCORD_TOKEN, "!", unity_socket_server, dbms)
unity_socket_server.discord_bot = discord_bot

webserver.discord_bot = discord_bot

if __name__ == "__main__":
    webserver.webserver_app.run(
        WEBSITE_IP, port=WEBSITE_PORT,
        debug=True, ssl_context='adhoc'
    )
