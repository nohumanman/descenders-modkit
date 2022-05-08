from SocketServer import SocketServer
import threading
import DBMS

HOST = "172.26.14.70"
PORT = 65432

webserverHost = SocketServer(HOST, PORT)

threading.Thread(target=webserverHost.start).start()

from flask import Flask, render_template, request, jsonify

app = Flask(__name__)

@app.route("/")
def index():
    return render_template("Dev.html")

@app.route("/leaderboard")
def get_leaderboard():
    return render_template("Leaderboard.html")

@app.route("/leaderboard/<trail>")
def get_leaderboard_trail(trail):
    return jsonify(DBMS().get_leaderboard_descenders(trail))

@app.route("/eval/<id>")
def hello(id):
    args = request.args.get("order")
    print(args)
    webserverHost.get_player_by_id(id).send(args)
    return "Hello World!"

@app.route("/get")
def get():
    return jsonify(
        {'ids':
            [
                {
                    "id" : player.steam_id,
                    "name" : player.steam_name,
                    "steam_avatar_src" : player.get_avatar_src()
                } for player in webserverHost.players
            ]
        }
    )

import time
import random
shouldRandomise = True
def riders_gate():
    while True:
        time.sleep(10)
        if (shouldRandomise):
            rand = str(random.randint(0, 5000) / 1000)
            for player in webserverHost.players:
                player.send(
                    "RIDERSGATE|"
                    + rand
                )

@app.route("/randomise")
def randomise():
    global shouldRandomise
    shouldRandomise = not shouldRandomise
    return "123"

threading.Thread(target=riders_gate).start()

app.run("0.0.0.0", port=8080)