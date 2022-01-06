from flask import Flask, render_template, jsonify, request
from Player import Player
from Timer import Timer
import time

app = Flask(__name__)

timer = Timer()

@app.route("/full-test")
def test_with_full_run():
    new_player_joined = Player("nohumanman", "123423918283")
    timer.players[new_player_joined.id] = new_player_joined
    timer.players[new_player_joined.id].started_trail(time.time(), "Lactate", 8)
    time.sleep(12)
    timer.players[new_player_joined.id].entered_checkpoint(time.time(), "Lactate")
    time.sleep(12)
    timer.players[new_player_joined.id].entered_checkpoint(time.time(), "Lactate")
    time.sleep(12)
    timer.players[new_player_joined.id].entered_checkpoint(time.time(), "Lactate")
    time.sleep(12)
    timer.players[new_player_joined.id].entered_checkpoint(time.time(), "Lactate")
    time.sleep(12)
    timer.players[new_player_joined.id].entered_checkpoint(time.time(), "Lactate")
    time.sleep(12)
    timer.players[new_player_joined.id].entered_checkpoint(time.time(), "Lactate")
    time.sleep(12)
    timer.players[new_player_joined.id].entered_checkpoint(time.time(), "Lactate")
    time.sleep(12)
    timer.players[new_player_joined.id].finished_trail(time.time(), "Lactate")


@app.route("/API/TIMER/ENTER-CHECKPOINT/<checkpoint_num>")
def api_timer_enter_checkpoint(checkpoint_num):
    trail_name = request.args.get("trail_name")
    timestamp = request.args.get("timestamp")
    player_name = request.args.get("player_name")
    player_id = request.args.get("steam_id")
    timer.players[player_id].entered_checkpoint(timestamp, trail_name)
    timer.players[player_id].name = player_name
    return checkpoint_num


@app.route("/API/TIMER/LOADED")
def api_timer_loaded():
    player_name = request.args.get("player_name")
    player_id = request.args.get("steam_id")
    timer.players[player_id] = Player(player_name, player_id)
    return "E"


@app.route("/API/GET-DATA")
def api_get_data():
    if timer.monitored_player is not None:
        return {
            "time_start": timer.monitored_player.time_started,
            "name": timer.monitored_player.name,
            "split_times": timer.monitored_player.checkpoint_times,
            "fastest_split_times": [12, 13, 11, 5],
            "entered_checkpoint": timer.monitored_player.has_entered_checkpoint,
        }
    else:
        return {
            "time_start": 0,
            "name": "None",
            "current_split_time": "None",
            "fastest_split_time_secs": [12, 13, 11, 5],
            "entered_checkpoint": True,
            "is_racing": True,
        }


@app.route("/API/MONITOR/<player_id>")
def monitor_player(player_id):
    print(player_id)
    timer.monitored_player = timer.players[player_id]
    return player_id


@app.route("/API/GET-PLAYERS")
def get_player():
    return jsonify({"players": [{"name" : timer.players[player].name, "id" : timer.players[player].id} for player in timer.players]})


@app.route("/API/GET-COMPETITORS-NAMES")
def competitors_names():
    return jsonify({"competitors": ["BBB171", "nohumanman"]})


@app.route("/UI")
def ui():
    return render_template("UI.html")


@app.route("/")
def dashboard():
    return render_template("Dashboard.html")

app.run(host="0.0.0.0", port="8080")
