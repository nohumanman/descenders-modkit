from flask import Flask, session
import os
from Tokens import (
    OAUTH2_CLIENT_ID,
    OAUTH2_CLIENT_SECRET
)
from requests_oauthlib import OAuth2Session
from DBMS import DBMS

OAUTH2_REDIRECT_URI = 'https://split-timer.nohumanman.com/callback'
API_BASE_URL = os.environ.get('API_BASE_URL', 'https://discordapp.com/api')
AUTHORIZATION_BASE_URL = API_BASE_URL + '/oauth2/authorize'
TOKEN_URL = API_BASE_URL + '/oauth2/token'


class Webserver(Flask):
    def __init__(self, ip, port):
        self.config['SECRET_KEY'] = OAUTH2_CLIENT_SECRET

    def permission(self):
        if session.get('oauth2_token') is None:
            return "UNKNOWN"
        discord = self.make_session(token=session.get('oauth2_token'))
        user = discord.get(API_BASE_URL + '/users/@me').json()
        if user["id"] in [str(x[0]) for x in DBMS.get_valid_ids()]:
            return "AUTHORISED"
        return "UNAUTHORISED"

    def make_session(self, token=None, state=None, scope=None):
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
            token_updater=self.token_updater
        )

    def token_updater(self, token):
        session['oauth2_token'] = token

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


