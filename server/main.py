""" Main function of descenders-modkit server """
import threading
import logging
import os
from unity_socket_server import UnitySocketServer
from discord_bot import DiscordBot
from tokens import DISCORD_TOKEN
from webserver import Webserver
from dbms import DBMS

def setup_logging(log_file):
    """ Setup logging for the server """
    # Prevent logging from outputting to stdout
    logging.basicConfig(
        filename=log_file,
        level=logging.DEBUG,
        format=(
            '%(asctime)s - %(name)s\t - %(levelname)s\t'
            ' - %(filename)s\t - Line %(lineno)d:\t %(message)s'
        )
    )

UNITY_SOCKET_IP = "0.0.0.0"
UNITY_SOCKET_PORT = 65432
WEBSITE_IP = "0.0.0.0"
WEBSITE_PORT = 8080


""" Main function of descenders-modkit server """
script_path = os.path.dirname(os.path.realpath(__file__))
os.chdir(script_path)

log_file = os.path.join(script_path, "modkit.log")
setup_logging(log_file)

dbms_instance = DBMS()

# - Unity Socket Server -
unity_socket_server = UnitySocketServer(UNITY_SOCKET_IP, UNITY_SOCKET_PORT, dbms_instance)
threading.Thread(target=unity_socket_server.start).start()

# - Website Server -
webserver = Webserver(unity_socket_server, dbms_instance)

# - Discord Bot -
discord_bot = DiscordBot(DISCORD_TOKEN, "!", unity_socket_server, dbms_instance)

# assign discord bot in unity socket server and webserver
unity_socket_server.discord_bot = discord_bot
webserver.discord_bot = discord_bot

# run webserver
if __name__ == "__main__":
    webserver.webserver_app.run(
        WEBSITE_IP, port=WEBSITE_PORT,
        debug=True, ssl_context='adhoc'
    )
    print("Server available from https://localhost:8080/")

