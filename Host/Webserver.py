from flask import Flask, session, request, redirect, jsonify, render_template
import os
from requests_oauthlib import OAuth2Session
import logging
from DBMS import DBMS
from Tokens import (
    OAUTH2_CLIENT_ID,
    OAUTH2_CLIENT_SECRET
)

OAUTH2_REDIRECT_URI = 'https://split-timer.nohumanman.com/callback'
API_BASE_URL = os.environ.get('API_BASE_URL', 'https://discordapp.com/api')
AUTHORIZATION_BASE_URL = API_BASE_URL + '/oauth2/authorize'
TOKEN_URL = API_BASE_URL + '/oauth2/token'


class WebserverRoute():
    def __init__(self, route, endpoint, view_func, methods):
        self.route = route
        self.endpoint = endpoint
        self.view_func = view_func
        self.methods = methods


class Webserver():
    def __init__(self, socket_server):
        self.webserver_app = Flask(__name__)
        self.webserver_app.config['SECRET_KEY'] = OAUTH2_CLIENT_SECRET
        self.socket_server = socket_server
        self.routes = [
            WebserverRoute(
                "/callback", "callback",
                self.callback, ["GET"]
            ),
            WebserverRoute(
                "/me", "me",
                self.me, ["GET"]
            ),
            WebserverRoute(
                "/split-time", "split_time",
                self.split_time, ["GET"]
            ),
            WebserverRoute(
                "/permission", "permission_check",
                self.permission, ["GET"]
            ),
            WebserverRoute(
                "/tag", "tag",
                self.tag, ["GET"]
            ),
            WebserverRoute(
                "/", "index",
                self.index, ["GET"]
            ),
            WebserverRoute(
                "/leaderboard", "leaderboard",
                self.leaderboard, ["GET"]
            ),
            WebserverRoute(
                "/get-leaderboard", "get_leaderboards",
                self.get_leaderboards, ["GET"]
            ),
            WebserverRoute(
                "/leaderboard", "get_leaderboard",
                self.get_leaderboard, ["GET"]
            ),
            WebserverRoute(
                "/leaderboard/<trail>", "get_leaderboard_trail",
                self.get_leaderboard_trail, ["GET"]
            ),
            WebserverRoute(
                "/get-all-times", "get_all_times",
                self.get_all_times, ["GET"]
            ),
            WebserverRoute(
                "/get", "get",
                self.get, ["GET"]
            ),
            WebserverRoute(
                "/eval/<id>", "eval",
                self.eval, ["GET"]
            )
        ]
        self.add_routes()

    def add_routes(self):
        for route in self.routes:
            self.webserver_app.add_url_rule(
                route.route,
                endpoint=route.endpoint,
                view_func=route.view_func,
                methods=route.methods
            )

    def eval(self, id):
        try:
            if self.permission() == "AUTHORISED":
                args = request.args.get("order")
                print(args)
                try:
                    self.socket_server.get_player_by_id(id).send(args)
                    if args.startswith("SET_BIKE"):
                        if args[9:10] == "1":
                            self.socket_server.get_player_by_id(
                                id
                            ).bike_type = "downhill"
                        elif args[9:10] == "0":
                            self.socket_server.get_player_by_id(
                                id
                            ).bike_type = "enduro"
                        elif args[9:10] == "2":
                            self.socket_server.get_player_by_id(
                                id
                            ).bike_type = "hardtail"
                except Exception as e:
                    logging.error(e)
                    return e
                return "Hello World!"
            else:
                return "FAILED - NOT VALID PERMISSIONS!"
        except Exception as e:
            return str(e)

    def get(self):
        return jsonify(
            {
                'ids':
                [
                    {
                        "id": player.steam_id,
                        "name": player.steam_name,
                        "steam_avatar_src": player.get_avatar_src(),
                        "total_time": player.get_total_time(),
                        "time_on_world": player.get_total_time(onWorld=True),
                        "world_name": player.world_name,
                        "reputation": player.reputation,
                        "last_trick": player.last_trick,
                        "version": player.version,
                        "trails": [
                            player.trails[trail].get_boundaries()
                            for trail in player.trails
                        ],
                        "bike_type": player.bike_type,
                        "time_loaded": player.time_started,
                    } for player in self.socket_server.players
                ]
            }
        )

    def permission(self):
        if session.get('oauth2_token') is None:
            return "UNKNOWN"
        discord = self.make_session(token=session.get('oauth2_token'))
        user = discord.get(API_BASE_URL + '/users/@me').json()
        if user["id"] in [str(x[0]) for x in DBMS.get_valid_ids()]:
            return "AUTHORISED"
        return "UNAUTHORISED"

    def logged_in(self):
        return (
            self.permission() == "AUTHORISED"
            or self.permission() == "UNAUTHORISED"
        )

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

    # routes
    def callback(self):
        if request.values.get('error'):
            return request.values['error']
        discord = self.make_session(
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
            connections = discord.get(
                API_BASE_URL + '/users/@me/connections'
            ).json()
            for connection in connections:
                if connection["type"] == "steam":
                    steam_id = connection["id"]
            DBMS.discord_login(id, username, email, steam_id)
        except Exception:
            pass
        session['oauth2_token'] = token
        return redirect("/")

    def me(self):
        discord = self.make_session(token=session.get('oauth2_token'))
        user = discord.get(API_BASE_URL + '/users/@me').json()
        connections = discord.get(
            API_BASE_URL + '/users/@me/connections'
        ).json()
        # DBMS.discord_login(id, username, email, steam_id)
        guilds = discord.get(API_BASE_URL + '/users/@me/guilds').json()
        return jsonify(user=user, guilds=guilds, connections=connections)

    def split_time(self):
        return render_template("SplitTime.html")

    def tag(self):
        return render_template("PlayerTag.html")

    def index(self):
        if self.logged_in():
            return render_template("Dashboard.html")
        scope = request.args.get(
            'scope',
            'identify email connections guilds guilds.join'
        )
        scope = "identify"
        discord = self.make_session(scope=scope.split(' '))
        authorization_url, state = discord.authorization_url(
            AUTHORIZATION_BASE_URL
        )
        session['oauth2_state'] = state
        return redirect(authorization_url)

    def leaderboard(self):
        return render_template("Leaderboard.html")

    def get_leaderboards(self):
        timestamp = float(request.args.get("timestamp"))
        trail_name = request.args.get("trail_name")
        return jsonify(
            DBMS.get_times_after_timestamp(
                timestamp,
                trail_name
            )
        )

    def get_leaderboard(self):
        if self.logged_in():
            return render_template("Leaderboard.html")
        else:
            return redirect("/")

    def get_leaderboard_trail(self, trail):
        return jsonify(DBMS().get_leaderboard(trail))

    def get_all_times(self):
        return jsonify({"times": DBMS.get_all_times()})