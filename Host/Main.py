from SocketServer import SocketServer
import threading

HOST = "172.26.14.70"
PORT = 65432

webserverHost = SocketServer(HOST, PORT)

threading.Thread(target=webserverHost.start).start()

from flask import Flask, render_template, request, jsonify

app = Flask(__name__)

@app.route("/")
def index():
    return render_template("Dev.html")

@app.route("/eval/<id>")
def hello(id):
    args = request.args.get("order")
    print(args)
    webserverHost.get_player_by_id(id).send(args)
    return "Hello World!"

@app.route("/get")
def get():
    return jsonify({'ids': [{"id" : player.steam_id, "name" : player.steam_name} for player in webserverHost.players]})

app.run("0.0.0.0", port=8080)