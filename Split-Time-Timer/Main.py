from flask import Flask, render_template, jsonify, request
from Player import Player
from Timer import Timer
import time
from PlayerDB import PlayerDB


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
        timer.players[steam_id].online = True
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

@app.route("/API/MONITOR/<steam_id>")
def app_monitor_steam_id(steam_id):
    if timer.monitored_player == timer.players[steam_id]:
        timer.monitored_player = None
    else:
        timer.monitored_player = timer.players[steam_id]
    return "Monitoring"

@app.route("/API/BECOME-COMPETITOR/<steam_id>")
def api_become_competitor_toggle(steam_id):
    if timer.players[steam_id].is_competitor:
        PlayerDB.become_competitor(steam_id, False, timer.players[steam_id].steam_name)
        timer.players[steam_id].is_competitor = False
    else:
        PlayerDB.become_competitor(steam_id, True, timer.players[steam_id].steam_name)
        timer.players[steam_id].is_competitor = True
    return "Monitoring"

@app.route("/API/DASHBOARD/GET-PLAYERS")
def api_get_players():
    return {
        "players":
            [
                {
                    "steam_name": timer.players[player].steam_name,
                    "steam_id": str(timer.players[player].steam_id),
                    "current_world": timer.players[player].current_world,
                    "trail_start_time": timer.players[player].trail_start_time,
                    "split_times": str(timer.players[player].split_times),
                    "is_competitor" : timer.players[player].is_competitor,
                    "online" : timer.players[player].online,
                    "colour" : (lambda online, is_competitor : "purple" if (online and is_competitor) else("green" if (online) else("red")))(timer.players[player].online, timer.players[player].is_competitor),
                    "being_monitored" : (lambda player : True if (timer.players[player] == timer.monitored_player) else False)(player),
                    "current_trail" : timer.players[player].current_trail
                }
                for player in timer.players
            ]
    }

@app.route("/API/GET-DATA")
def api_get_data():
    try:
        return jsonify({
            "time_start" : timer.monitored_player.trail_start_time,
            "name" : timer.monitored_player.steam_name,
            "split_times" : timer.monitored_player.split_times,
            "fastest_split_times" : PlayerDB.get_fastest_split_times(timer.monitored_player.current_trail),
            "entered_checkpoint" : timer.monitored_player.has_entered_checkpoint,
            "monitoring" : True
        })
    except AttributeError as e:
        return jsonify({"monitoring" : False})

@app.route("/")
def dashboard():
    return render_template("Dashboard.html")

@app.route("/MONITOR")
def ui_monitor():
    return render_template("UI.html")

app.run(host="0.0.0.0", port="8080")
