from UnitySocketServer import SocketServer
from DiscordBot import DiscordBot
from Tokens import discord_token, OAUTH2_CLIENT_ID, OAUTH2_CLIENT_SECRET
from DBMS import DBMS
from flask import Flask, render_template, request, jsonify
from flask import redirect, session
from requests_oauthlib import OAuth2Session
import threading
import time
import random
import logging
import os

script_path = os.path.dirname(os.path.realpath(__file__))

log_location = script_path + "/SplitTimer.log"


logging.basicConfig(
    filename=log_location,
    filemode="w",
    level=logging.DEBUG,
    format='%(asctime)s - %(message)s',
    datefmt='%d-%b-%y %H:%M:%S'
)

logging.info(
    "--------------------------------"
    " Descenders Split Timer Started "
    "--------------------------------"
)

# -- Socket Server --

SOCKET_HOST = "0.0.0.0"
SOCKET_PORT = 65432

socket_server = SocketServer(SOCKET_HOST, SOCKET_PORT)
socket_server_thread = threading.Thread(target=socket_server.start)
socket_server_thread.start()

# -- Website Server --

WEBSITE_HOST = "0.0.0.0"
WEBSITE_PORT = 8080

app = Flask(__name__)


OAUTH2_REDIRECT_URI = 'https://split-timer.nohumanman.com/callback'

API_BASE_URL = os.environ.get('API_BASE_URL', 'https://discordapp.com/api')
AUTHORIZATION_BASE_URL = API_BASE_URL + '/oauth2/authorize'
TOKEN_URL = API_BASE_URL + '/oauth2/token'

app.config['SECRET_KEY'] = OAUTH2_CLIENT_SECRET


def token_updater(token):
    session['oauth2_token'] = token


def make_session(token=None, state=None, scope=None):
    return OAuth2Session(
        client_id=OAUTH2_CLIENT_ID,
        token=token,
        state=state,
        scope=scope,
        redirect_uri=OAUTH2_REDIRECT_URI,
        auto_refresh_kwargs={
            'client_id': OAUTH2_CLIENT_ID,
            'client_secret': OAUTH2_CLIENT_SECRET,
        },
        auto_refresh_url=TOKEN_URL,
        token_updater=token_updater
    )


@app.route('/callback')
def callback():
    if request.values.get('error'):
        return request.values['error']
    discord = make_session(
        state=session.get('oauth2_state')
    )
    token = discord.fetch_token(
        TOKEN_URL,
        client_secret=OAUTH2_CLIENT_SECRET,
        authorization_response=request.url
    )
    user = discord.get(API_BASE_URL + '/users/@me').json()

    id = user["id"]
    try:
        email = user["email"]
        username = user["username"]
        connections = discord.get(API_BASE_URL + '/users/@me/connections').json()
        for connection in connections:
            if connection["type"] == "steam":
                steam_id = connection["id"]
        DBMS.discord_login(id, username, email, steam_id)
    except:
        pass
    session['oauth2_token'] = token
    return redirect("/")


@app.route('/me')
def me():
    discord = make_session(token=session.get('oauth2_token'))
    user = discord.get(API_BASE_URL + '/users/@me').json()
    connections = discord.get(API_BASE_URL + '/users/@me/connections').json()
    #DBMS.discord_login(id, username, email, steam_id)
    guilds = discord.get(API_BASE_URL + '/users/@me/guilds').json()
    return jsonify(user=user, guilds=guilds, connections=connections)


@app.route("/split-time")
def split_time():
    return render_template("SplitTime.html")


@app.route("/permission")
def permission_check():
    return permission()


@app.route("/tag")
def tag():
    return render_template("PlayerTag.html")

@app.route("/")
def index():
    if permission() == "AUTHORISED" or permission() == "UNAUTHORISED":
        return render_template("Dashboard.html")
    scope = request.args.get(
        'scope',
        'identify email connections guilds guilds.join'
    )
    scope = "identify"
    discord = make_session(scope=scope.split(' '))
    authorization_url, state = discord.authorization_url(
        AUTHORIZATION_BASE_URL
    )
    session['oauth2_state'] = state
    return redirect(authorization_url)


@app.route("/leaderboard")
def leaderboards():
    return render_template("Leaderboard.html")


@app.route("/get-leaderboard")
def get_leaderboards():
    timestamp = float(request.args.get("timestamp"))
    trail_name = request.args.get("trail_name")
    return jsonify(
        DBMS.get_times_after_timestamp(
            timestamp,
            trail_name
        )
    )


@app.route("/leaderboard")
def get_leaderboard():
    if permission() == "AUTHORISED" or permission() == "UNAUTHORISED":
        return render_template("Leaderboard.html")
    else:
        return redirect("/")


@app.route("/leaderboard/<trail>")
def get_leaderboard_trail(trail):
    return jsonify(DBMS().get_leaderboard(trail))


@app.route("/get-all-times")
def get_all_times():
    return jsonify({"times": DBMS.get_all_times()})


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
    app.run(WEBSITE_HOST, port=WEBSITE_PORT, debug=True, ssl_context='adhoc')
