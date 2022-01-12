from flask import Flask, render_template, jsonify, request
from Player import Player
from Timer import Timer
import time

app = Flask(__name__)

timer = Timer()

# Player enters map
@app.route("/API/TIMER/LOADED")
def api_timer_loaded():
    world_name = request.args.get("world_name")
    steam_name = request.args.get("steam_name")
    steam_id = request.args.get("steam_id")
    if steam_id in timer.players:
        timer.players[steam_id].current_world = world_name
        timer.players[steam_id].steam_name = steam_name
        timer.players[steam_id].online = True
    else:
        timer.players[steam_id] = Player(steam_name, steam_id, world_name, False)
    timer.players[steam_id].loaded()
    return "200"

# Player enters map
@app.route("/API/TIMER/UNLOADED")
def api_timer_unloaded():
    steam_name = request.args.get("steam_name")
    steam_id = request.args.get("steam_id")
    if steam_id in timer.players:
        timer.players[steam_id].online = False
        timer.players[steam_id].unloaded()
    return "200"

# Player enters checkpoint on trail
@app.route("/API/TIMER/ENTER-CHECKPOINT/<checkpoint_num>")
def api_timer_enter_checkpoint(checkpoint_num):
    trail_name = request.args.get("trail_name")
    steam_name = request.args.get("steam_name")
    steam_id = request.args.get("steam_id")
    total_checkpoints = int(request.args.get("total_checkpoints"))
    try:
        timer.players[steam_id].entered_checkpoint(int(checkpoint_num), total_checkpoints, time.time(), trail_name)
    except KeyError:
        timer.players[steam_id] = Player(steam_name, steam_id, "none", False)
        timer.players[steam_id].entered_checkpoint(int(checkpoint_num), total_checkpoints, time.time(), trail_name)
    return "200"

def get_colour(online, competitor):
    if online and competitor:
        return "purple"
    elif online:
        return "green"
    elif not online:
        return "red"

@app.route("/API/DASHBOARD/GET-PLAYERS")
def api_get_players():
    return {
        "players":
            [
                {
                    "steam_name": timer.players[player].steam_name,
                    "steam_id": timer.players[player].steam_id,
                    "current_world": timer.players[player].current_world,
                    "trail_start_time": timer.players[player].trail_start_time,
                    "split_times": str(timer.players[player].split_times),
                    "is_competitor" : timer.players[player].is_competitor,
                    "online" : timer.players[player].online,
                    "colour" : (lambda online, is_competitor : "purple" if (online and is_competitor) else("green" if (online) else("red")))(timer.players[player].online, timer.players[player].is_competitor),
                    "being_monitored" : (lambda player : True if (player == timer.monitored_player) else False)(player)
                }
                for player in timer.players
            ]
    }

@app.route("/")
def dashboard():
    return render_template("Dashboard.html")

app.run(host="0.0.0.0", port="8080")
