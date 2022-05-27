from SocketServer import SocketServer
from DiscordBot import DiscordBot
from Tokens import discord_token
from DBMS import DBMS
from flask import Flask, render_template, request, jsonify
import threading
import time
import random
import logging
import os

script_path = os.path.dirname(os.path.realpath(__file__))

log_location = script_path + "/splittimer.log"

logging.basicConfig(
    filename=log_location,
    filemode="a",
    level=logging.DEBUG,
    format='%(asctime)s - %(message)s',
    datefmt='%d-%b-%y %H:%M:%S'
)

logging.info(
    "-------------------------------- "
    "Descenders Split Timer Started"
    " --------------------------------"
)

# Create Socket Server

SOCKET_HOST = "0.0.0.0"
SOCKET_PORT = 65432

socket_server = SocketServer(SOCKET_HOST, SOCKET_PORT)
socket_server_thread = threading.Thread(target=socket_server.start)
socket_server_thread.start()

# Could have another instance of socket server to handle
# the dashboard (to prevent periodic get requests)
# or could use the websocket to prompt a reload?


# Create Website Server

WEBSITE_HOST = "0.0.0.0"
WEBSITE_PORT = 8080

app = Flask(__name__)


@app.route("/get-log")
def log():
    with open(log_location) as my_file:
        log = my_file.read()
    return jsonify({"log": log.split("\n")})


@app.route("/logs")
def logs():
    return render_template("log.html")


@app.route("/")
def index():
    return render_template("Dashboard.html")


@app.route("/leaderboard")
def get_leaderboard():
    return render_template("Leaderboard.html")


@app.route("/leaderboard/<trail>")
def get_leaderboard_trail(trail):
    return jsonify(DBMS().get_leaderboard(trail))


@app.route("/eval/<id>")
def hello(id):
    args = request.args.get("order")
    print(args)
    try:
        socket_server.get_player_by_id(id).send(args)
    except Exception as e:
        logging.error(e)
        return e
    return "Hello World!"


@app.route("/get")
def get():
    return jsonify(
        {
            'ids':
            [
                {
                    "id": player.steam_id,
                    "name": player.steam_name,
                    "steam_avatar_src": player.get_avatar_src(),
                    "total_time": player.get_total_time(),
                    "world_name": player.world_name,
                    "reputation": player.reputation
                } for player in socket_server.players
            ]
        }
    )


@app.route("/randomise")
def randomise():
    global shouldRandomise
    shouldRandomise = not shouldRandomise
    return "123"


shouldRandomise = True


def riders_gate():
    while True:
        time.sleep(25)
        if (shouldRandomise):
            rand = str(random.randint(0, 3000) / 1000)
            logging.info("Sending Random Gate to all players...")
            for player in socket_server.players:
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


discord_bot = DiscordBot(discord_token, "!", socket_server)

socket_server.discord_bot = discord_bot

if __name__ == "__main__":
    app.run(WEBSITE_HOST, port=WEBSITE_PORT)
