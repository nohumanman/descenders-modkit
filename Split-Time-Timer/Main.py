from flask import Flask, render_template, jsonify, request, make_response, redirect
from Player import Player
from Timer import Timer
import time
from PlayerDB import PlayerDB
from Tokens import webhook, bot_token
from RidersGate import RidersGate
import asyncio

app = Flask(__name__)

timer = Timer()
riders_gate = RidersGate()

import logging
log = logging.getLogger('werkzeug')
log.setLevel(logging.ERROR)
logging.basicConfig(
    filename="log.txt",
    filemode="a",
    level=logging.INFO,
    format='%(asctime)s,%(msecs)d %(name)s %(levelname)s %(message)s',
    datefmt='%H:%M:%S')

''' IN-Descenders API calls '''

@app.route("/API/DESCENDERS/ON-BOUNDRY-ENTER")
def on_boundry_enter():
    steam_id = request.args.get("steam_id")
    steam_name = request.args.get("steam_name")
    world_name = request.args.get("world_name")
    trail_name = request.args.get("trail_name")
    logging.info(f'''Player {steam_name} (id {steam_id}) on {world_name} has entered boundry on trail "{trail_name}"''')
    if timer.players[steam_id] is None:
        timer.players[steam_id] = Player(
            steam_name,
            steam_id,
            world_name,
            False
        )
    player = timer.players[steam_id]
    player.amount_of_boundaries_inside += 1
    #if player.is_competitor is timer.monitored_player:
    if player.amount_of_boundaries_inside:
        return "valid"
    else:
        return "INVALID; INTERNAL SERVER ERROR"


@app.route("/API/DESCENDERS/ON-BOUNDRY-EXIT")
def on_boundry_exit():
    steam_id = request.args.get("steam_id")
    steam_name = request.args.get("steam_name")
    world_name = request.args.get("world_name")
    trail_name = request.args.get("trail_name")
    logging.info(f'''Player {steam_name} (id {steam_id}) on {world_name} has exited the boundry on trail "{trail_name}"''')
    if timer.players[steam_id] is None:
        timer.players[steam_id] = Player(
            steam_name,
            steam_id,
            world_name,
            False
        )
    player = timer.players[steam_id]
    if player.amount_of_boundaries_inside > 0:
        player.amount_of_boundaries_inside -= 1
    if player.amount_of_boundaries_inside <= 0:
        print(f"You are in {player.amount_of_boundaries_inside}")
        if player.is_competitor == timer.monitored_player:
            return "valid"
        else:
            player.split_times = []
            return "INVALID; OUT OF BOUNDS"
    else:
        return "valid"

@app.route("/API/DESCENDERS/ON-CHECKPOINT-ENTER/<checkpoint_num>")
async def on_checkpoint_enter(checkpoint_num):
    steam_id = request.args.get("steam_id")
    steam_name = request.args.get("steam_name")
    world_name = request.args.get("world_name")
    trail_name = request.args.get("trail_name")
    total_checkpoints = request.args.get("total_checkpoints")
    checkpoint_type = request.args.get("checkpoint_type")
    logging.info(f'''Player {steam_name} (id {steam_id}) on {world_name} has entered a checkpoint on {trail_name} (Checkpoint {checkpoint_num}/{total_checkpoints} and type '{checkpoint_type}')''')
    if timer.players[steam_id] is None:
        timer.players[steam_id] = Player(
            steam_name,
            steam_id,
            world_name,
            False
        )
    player = timer.players[steam_id]
    player.loaded(world_name)
    if checkpoint_type == "start" and checkpoint_num != 0:
        player.cancel_time()
        player.current_trail = trail_name
        await player.entered_checkpoint(
            int(checkpoint_num),
            int(total_checkpoints),
            time.time(),
            trail_name
            
        )
    elif checkpoint_type == "intermediate" and player.trail_start_time == 0:
        return "INVALID; Didn't go through start!"
    elif checkpoint_type == "stop":
        if int(checkpoint_num) > int(total_checkpoints)-1:
            return "INVALID; SKIPPED CHECKPOINTS!"
        else:     
            await player.entered_checkpoint(
                int(checkpoint_num),
                int(total_checkpoints),
                time.time(),
                trail_name
                
            )
    else:
        await player.entered_checkpoint(
            int(checkpoint_num),
            int(total_checkpoints),
            time.time(),
            trail_name
            
        )
    return "valid"

@app.route("/API/DESCENDERS/ON-DEATH")
def on_death():
    steam_id = request.args.get("steam_id")
    steam_name = request.args.get("steam_name")
    world_name = request.args.get("world_name")
    logging.info(f'''Player {steam_name} (id {steam_id}) on {world_name} has died.''')
    if timer.players[steam_id] is None:
        timer.players[steam_id] = Player(
            steam_name,
            steam_id,
            world_name,
            False
        )
    player = timer.players[steam_id]
    return "TIME INVALID; RESPAWNED"
    #return "valid"

@app.route("/API/DESCENDERS/ON-MAP-ENTER")
def on_map_enter():
    steam_id = request.args.get("steam_id")
    steam_name = request.args.get("steam_name")
    world_name = request.args.get("world_name")
    logging.info(f'''Player {steam_name} (id {steam_id}) on {world_name} has entered the map.''')
    try:
        timer.players[steam_id]
    except:
        timer.players[steam_id] = None
    if timer.players[steam_id] is None:
        timer.players[steam_id] = Player(
            steam_name,
            steam_id,
            world_name,
            False
        )
    player = timer.players[steam_id]
    player.loaded(world_name)
    if (player.get_ban_status() == "unbanned" or player.get_ban_status() == "shadowban"):
        return "valid"
    else:
        return player.get_ban_status()
    

@app.route("/API/DESCENDERS/ON-MAP-EXIT")
def on_map_exit():
    steam_id = request.args.get("steam_id")
    steam_name = request.args.get("steam_name")
    world_name = request.args.get("world_name")
    logging.info(f'''Player {steam_name} (id {steam_id}) on {world_name} has exited the world.''')
    if timer.players[steam_id] is None:
        timer.players[steam_id] = Player(
            steam_name,
            steam_id,
            world_name,
            False
        )
    if timer.players[steam_id].time_started != None:
        timer.players[steam_id].unloaded()
    return "valid"

@app.route("/API/DESCENDERS/ON-BIKE-SWITCH")
def api_descenders_on_bike_switch():
    steam_id = request.args.get("steam_id")
    steam_name = request.args.get("steam_name")
    world_name = request.args.get("world_name")
    new_bike = request.args.get("new_bike")
    logging.info(f'''Player {steam_name} (id {steam_id}) on {world_name} has switched to bike {new_bike}''')
    if timer.players[steam_id] is None:
        timer.players[steam_id] = Player(
            steam_name,
            steam_id,
            world_name,
            False
        )
    player = timer.players[steam_id]
    player.current_bike = new_bike
    return "valid"

@app.route("/API/DESCENDERS/GET-RIDERS-GATE")
def api_get_riders_gate():
    steam_id = request.args.get("steam_id")
    steam_name = request.args.get("steam_name")
    world_name = request.args.get("world_name")
    logging.info(f'''Player {steam_name} (id {steam_id}) on {world_name} has requested the gate status.''')
    if timer.players[steam_id] is None:
        timer.players[steam_id] = Player(
            steam_name,
            steam_id,
            world_name,
            False
        )
    player = timer.players[steam_id]
    return {"random_delay" : riders_gate.random_delay}

@app.route("/API/MONITOR/<steam_id>")
def app_monitor_steam_id(steam_id):
    logging.info(f'''Monitoring player with id {steam_id}''')
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

@app.route("/API/UPDATE-BAN-STATUS/<steam_id>")
def api_update_ban_status(steam_id):
    new_ban_status = request.args.get("ban_status")
    PlayerDB.update_ban_status(steam_id, new_ban_status)
    return "complete"


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
                    "current_trail" : timer.players[player].current_trail,
                    "ban_state" : timer.players[player].get_ban_status(),
                    "current_bike" : timer.players[player].current_bike,
                    "total_time_on_world" : (lambda: PlayerDB.get_time_on_world(player, world=timer.players[player].current_world) if (timer.players[player].current_world != "none")  else "0")(),
                    "total_time" : PlayerDB.get_time_on_world(player)
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
    steam_name = request.args.get("steam_name")
    steam_id = request.args.get("steam_id")
    world_name = request.args.get("world_name")
    logging.info(f'''Player {steam_name} (id {steam_id}) on {world_name} has requested leaderboard info.''')
    leaderboard_data = PlayerDB.get_leaderboard_descenders(trail_name, num=num)
    is_competitors = [data["is_competitor"] for data in leaderboard_data]
    steam_ids = [data["steam_id"] for data in leaderboard_data]
    steam_names = [data["steam_name"] for data in leaderboard_data]
    time_ids = [data["time_id"] for data in leaderboard_data]
    timestamps = [data["timestamp"] for data in leaderboard_data]
    total_times = [data["total_time"] for data in leaderboard_data]
    trail_names = [data["trail_name"] for data in leaderboard_data]
    was_monitoreds = [data["was_monitored"] for data in leaderboard_data]
    print("leaderboard fetch returned:" + str(leaderboard_data))
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
    steam_id = request.args.get("steam_id")
    steam_name = request.args.get("steam_name")
    world_name = request.args.get("world_name")
    logging.info(f'''Player {steam_name} (id {steam_id}) on {world_name} has requested split times of trail {trail_name}''')
    return {"fastest_split_times" : PlayerDB.get_fastest_split_times(trail_name, competitors_only=timer.competitors_only, min_timestamp=timer.timestamp, monitored_only=timer.monitored_only)}


@app.route("/API/TOGGLE-RIDERS-GATE-START")
def api_toggle_riders_gate_start():
    riders_gate.refresh_random_delay()
    return "Done"


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

@app.route("/GET-LOG-AS-LIST")
def get_log_as_string():
    with open("log.txt", "rt") as file:
        log_as_list = [x for x in file.read().split("\n")]
        log_as_list.reverse()
        return jsonify({"data" : log_as_list})

@app.route("/MONITOR")
def ui_monitor():
    return render_template("UI.html")

@app.route("/LOG")
def log_html():
    return render_template("Log.html")


app.run(host="0.0.0.0", port="8443", ssl_context="adhoc", debug=True)
