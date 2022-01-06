from flask import Flask, request, jsonify
from TombstoneHandler import TombstoneHandler

app = Flask(__name__)

# TOMBSTONE COMPONENT

tombstoneHandler = TombstoneHandler()

@app.route("/tombstones/submit_tombstone_data")
def submit_tombstone_data():
    map = request.args.get("map")
    posX = request.args.get("xPos")
    posY = request.args.get("yPos")
    posZ = request.args.get("zPos")
    steamName = request.args.get("name")
    steamId = request.args.get("steamId")
    tombstoneHandler.add_new_death_to_map(map, posX, posY, posZ, steamName, steamId)
    return "Complete!"

@app.route("/tombstones/get_tombstone_data")
def get_tombstone_data():
    map = request.args.get("map")
    just_today = (request.args.get("just_today") == "True")
    just_me = (request.args.get("just_me") == "True")
    just_recent_deaths = (request.args.get("just_recent_deaths") == "True")
    print(request.args.get("just_today"))
    print(just_today)
    try:
        steam_id = request.args.get("steamId")
    except:
        steam_id = None
    return jsonify(tombstoneHandler.get_tombstones_for_map(map, just_today, just_me, just_recent_deaths, steam_id))

@app.route("/tombstones/get_message_on_just_die")
def get_instantiation_data():
    return "literally just then."

@app.route("/tombstones/get_additional_message")
def get_additional_message():
    return ""

# BAN HANDLER COMPONENT

@app.route("/ban-handler/check/<steam_id>")
def ban_handler_check(steam_id):
    is_first = request.args.get("first")
    map_name = request.args.get("map")
    json_return = {
        "is_banned" : "False",
        "is_god" : "False",
        "message" : ""
    }
    print("Ban Handler Request")
    print(steam_id)
    return json_return

# SAVE SYSTEM COMPONENT

@app.route("/save-system/<steam_id>/get-save")
def get_save(steam_id):
    return "Not configured..."

app.run(host="0.0.0.0", port="8080")

