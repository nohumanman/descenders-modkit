""" Main function of descenders-modkit server """
import threading
import logging
import asyncio
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
        level=logging.WARNING,
        format=(
            '%(asctime)s - %(name)s\t - %(levelname)s\t'
            ' - %(filename)s\t - Line %(lineno)d:\t %(message)s'
        )
    )

UNITY_SOCKET_IP = "0.0.0.0"
UNITY_SOCKET_PORT = 65432
WEBSITE_IP = "0.0.0.0"
WEBSITE_PORT = 8082
WEBSITE_SOCKET_IP = "0.0.0.0"
WEBSITE_SOCKET_PORT = 65430

loop = asyncio.get_event_loop() # get the event loop

# Set the working directory to the script path
script_path = os.path.dirname(os.path.realpath(__file__))
os.chdir(script_path)
# Set the log file path
_log_file = os.path.join(script_path, "modkit.log")
setup_logging(_log_file)
# - Database Management System -
dbms_instance = DBMS()

# - Unity Socket Server -
unity_socket_server = UnitySocketServer(
    UNITY_SOCKET_IP,
    UNITY_SOCKET_PORT,
    dbms_instance
) # create the server instance

server_coroutine = asyncio.start_server(
    unity_socket_server.create_client,
    unity_socket_server.host,
    unity_socket_server.port,
) # create the server coroutine
loop.run_until_complete(server_coroutine) # run the server coroutine

# Set riders gate to run
loop.create_task(unity_socket_server.riders_gate())

# - Website Server -
webserver = Webserver(unity_socket_server, dbms_instance)

# - Discord Bot -
discord_bot = DiscordBot(DISCORD_TOKEN, "!", unity_socket_server, dbms_instance)

# assign discord bot in unity socket server and webserver
unity_socket_server.discord_bot = discord_bot
webserver.discord_bot = discord_bot

# run loop in a new thread (so it doesn't block the main thread)
threading.Thread(target=loop.run_forever).start()

# run webserver
if __name__ == "__main__":
    # NOTE: when debug=True, flask initialises twice! Causes issues with the unity socket server
    print(f"Server available from https://localhost:{WEBSITE_PORT}/")
    webserver.webserver_app.run(
        WEBSITE_IP, port=WEBSITE_PORT,
        debug=False, ssl_context='adhoc'
    )
