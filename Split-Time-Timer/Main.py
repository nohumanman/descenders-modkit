from flask import Flask, render_template, jsonify, request, make_response, redirect
from Player import Player
from Timer import Timer
import time
from PlayerDB import PlayerDB
from RidersGate import RidersGate


app = Flask(__name__)

timer = Timer()
riders_gate = RidersGate()

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
    #time_entered_checkpoint = request.args.get("time_entered_checkpoint")
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
        if timer.monitored_player is not None:
            timer.monitored_player.being_monitored = False
        timer.monitored_player = None
    else:
        if timer.monitored_player is not None:
            timer.monitored_player.being_monitored = False
        timer.monitored_player = timer.players[steam_id]
        timer.monitored_player.being_monitored = True
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

@app.route("/API/DASHBOARD/LEADERBOARD")
def api_dashboard_leaderboard():
    return jsonify({"data" : PlayerDB.get_leaderboard_data()})

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

@app.route("/API/DESCENDERS-LEADERBOARD")
def api_descenders_leaderboard():
    try:
        num = int(request.args.get("num"))
    except:
        num = 10
    trail_name = request.args.get("trail_name")
    leaderboard_data = PlayerDB.get_leaderboard_descenders(trail_name, num=num)
    is_competitors = [data["is_competitor"] for data in leaderboard_data]
    steam_ids = [data["steam_id"] for data in leaderboard_data]
    steam_names = [data["steam_name"] for data in leaderboard_data]
    time_ids = [data["time_id"] for data in leaderboard_data]
    timestamps = [data["timestamp"] for data in leaderboard_data]
    total_times = [data["total_time"] for data in leaderboard_data]
    trail_names = [data["trail_name"] for data in leaderboard_data]
    was_monitoreds = [data["was_monitored"] for data in leaderboard_data]

    return jsonify({
        "is_competitors" : is_competitors,
        "steam_ids" : steam_ids,
        "steam_names": steam_names,
        "time_ids": time_ids,
        "timestamps" : timestamps,
        "total_times": total_times,
        "trail_names": trail_names,
        "was_monitoreds": was_monitoreds
    })

@app.route("/API/DESCENDERS-GET-FASTEST-TIME")
def api_descenders_get_fastest_split_times():
    trail_name = request.args.get("trail_name")
    return {"fastest_split_times" : PlayerDB.get_fastest_split_times(trail_name)}

@app.route("/API/TOGGLE-RIDERS-GATE-START")
def api_toggle_riders_gate_start():
    riders_gate.refresh_random_delay()
    return "Done"

@app.route("/API/GET-RIDERS-GATE")
def api_get_riders_gate():
    return {"random_delay" : riders_gate.random_delay}

@app.route("/API/GET-DATA")
def api_get_data():
    comp_only = request.args.get("competitor_only")
    timestamp = request.args.get("min_timestamp")
    monitored_only = request.args.get("monitored_only")
    if comp_only == "True":
        comp_only = True
    else:
        comp_only = False
    if monitored_only == "True":
        monitored_only = True
    else:
        monitored_only = False
    
    try:
        return jsonify({
            "time_start" : timer.monitored_player.trail_start_time,
            "name" : timer.monitored_player.steam_name,
            "split_times" : timer.monitored_player.split_times,
            "fastest_split_times" : PlayerDB.get_fastest_split_times(timer.monitored_player.current_trail, competitors_only=comp_only, min_timestamp=timestamp, monitored_only=monitored_only),
            "entered_checkpoint" : timer.monitored_player.has_entered_checkpoint,
            "monitoring" : True
        })
    except AttributeError as e:
        return jsonify({"monitoring" : False})


@app.route("/login-api", methods=["POST"])
def login():
    if request.method == "POST":
        user = request.form['password']
        if user == "Big Badonkas123":
            resp = make_response("200")
            resp.set_cookie('id', "4565421332145321234565tr5")
            return resp
        else:
            return "nope"


@app.route("/")
def dashboard():
    name = request.cookies.get('id')
    if name != "4565421332145321234565tr5":
        return render_template("Login.html")
    else:
        return render_template("Dashboard.html")

@app.route("/MONITOR")
def ui_monitor():
    return render_template("UI.html")

app.run(host="0.0.0.0", port="8443", ssl_context="adhoc")
