from flask import Flask, session, request, redirect, jsonify, render_template
import os
from authlib.integrations.requests_client import OAuth2Session
import logging
import UnitySocketServer
from DBMS import DBMS
from Tokens import (
    OAUTH2_CLIENT_ID,
    OAUTH2_CLIENT_SECRET
)
import datetime

OAUTH2_REDIRECT_URI = 'https://split-timer.nohumanman.com/callback'
API_BASE_URL = os.environ.get('API_BASE_URL', 'https://discordapp.com/api')
AUTHORIZATION_BASE_URL = API_BASE_URL + '/oauth2/authorize'
TOKEN_URL = API_BASE_URL + '/oauth2/token'

split_timer_logger = logging.getLogger('DescendersSplitTimer')


class WebserverRoute():
    def __init__(self, route, endpoint, view_func, methods):
        self.route = route
        self.endpoint = endpoint
        self.view_func = view_func
        self.methods = methods


class Webserver():
    def __init__(self, socket_server: UnitySocketServer):
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
            ),
            WebserverRoute(
                "/get-spectated", "get_spectated",
                self.get_spectated, ["GET"]
            ),
            WebserverRoute(
                "/spectate", "spectate",
                self.spectate, ["GET"]
            ),
            WebserverRoute(
                "/login", "login",
                self.login, ["GET"]
            ),
            WebserverRoute(
                "/concurrency", "concurrency",
                self.concurrency, ["GET"]
            ),
            WebserverRoute(
                "/get-trails", "get_trails",
                self.get_trails, ["GET"]
            ),
            WebserverRoute(
                "/get-worlds", "get_worlds",
                self.get_worlds, ["GET"]
            ),
            WebserverRoute(
                "/upload-replay",
                "upload_replay",
                self.upload_replay,
                ["POST"]
            ),
            WebserverRoute(
                "/ignore-time/<time_id>/<value>",
                "ignore_time",
                self.ignore_time,
                ["GET"]
            ),
            WebserverRoute(
                "/get-output-log/<id>",
                "get_output_log",
                self.get_output_log,
                ["GET"]
            ),
        ]
        self.add_routes()
        self.webserver_app.register_error_handler(500, self.server_error)

    def add_routes(self):
        for route in self.routes:
            self.webserver_app.add_url_rule(
                route.route,
                endpoint=route.endpoint,
                view_func=route.view_func,
                methods=route.methods
            )

    def server_error(self, info):
        return render_template("500Error.html")

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
                return "FAILED - NOT VALID PERMISSIONS!", 401
        except Exception as e:
            return str(e)

    def get_output_log(self, id):
        if self.permission() == "AUTHORISED":
            lines = ""
            try:
                with open(
                    f"{os.getcwd()}/output_logs/{id}.txt",
                    "rt"
                ) as my_file:
                    file_lines = my_file.read().splitlines()
                    file_lines = file_lines[-50:]
                    for line in file_lines:
                        lines += f"> {line}<br>"
            except Exception as e:
                lines = f"Failed to get output log. One likely does not exist, has the user just loaded in?"
            return lines
        else:
            return "You are not authorised to fetch output log."

    def get(self):
        player_json = [
            {
                "id": player.steam_id,
                "name": player.steam_name,
                "steam_avatar_src": player.get_avatar_src(),
                "reputation": player.reputation,
                "total_time": player.get_total_time(),
                "time_on_world": player.get_total_time(onWorld=True),
                "world_name": player.world_name,
                "last_trick": player.last_trick,
                "version": player.version,
                "bike_type": player.bike_type,
                "address": (lambda: player.addr if self.permission() == "AUTHORISED" else "")()
            } for player in self.socket_server.players
        ]
        return jsonify({"players": player_json})

    def get_trails(self):
        return jsonify({"trails": DBMS.get_trails()})

    def ignore_time(self, time_id : int, value: str):
        if self.permission() == "AUTHORISED":
            # value should be 'False' or 'True
            DBMS.set_ignore_time(time_id, value)
            return "success"
        else:
            return "INVALID_PERMS"

    def upload_replay(self):
        logging.error(request.data)
        request.files["replay"].save(
            "/home/admin/desc-comp-toolkit/static/replays/"
            f"{request.form['time_id']}.replay"
        )

    def get_worlds(self):
        return jsonify({"worlds": DBMS.get_worlds()})

    def concurrency(self):
        from datetime import datetime
        try:
            map_name = request.args.get("map_name")
            if map_name == "":
                map_name = None
        except Exception:
            map_name = None
        return jsonify({
            "concurrency": DBMS.get_daily_plays(
                map_name,
                datetime(2022, 5, 1),
                datetime.now()
            )
        })

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
            refresh_token=self.token_updater
        )

    def token_updater(self, token):
        session['oauth2_token'] = token

    # routes
    def callback(self):
        try:
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
            session['oauth2_token'] = token
            user = discord.get(
                API_BASE_URL + '/users/@me'
            ).json()
            connections = discord.get(
                API_BASE_URL + '/users/@me/connections'
            ).json()
            split_timer_logger.info(
                "Webserver.py - discord login to website"
                f" - user '{user['id']}' named '{user['username']}'"
            )
            try:
                id = user['id']
                try:
                    email = user['email']
                except Exception:
                    email = ""
                username = user['username']
                steam_id = "NONE"
                try:
                    for connection in connections:
                        if connection['type'] == "steam":
                            steam_id = connection['id']
                except Exception as e:
                    logging.info("25123 - " + str(e))
                DBMS.discord_login(id, username, email, steam_id)
            except Exception as e:
                logging.info("User " + str(user) + " with error " + str(e))
                logging.info("With connections " + str(connections))
            return redirect("/")
        except Exception as e:
            return str(e)

    def me(self):
        try:
            discord = self.make_session(token=session.get('oauth2_token'))
            user = discord.get(API_BASE_URL + '/users/@me').json()
            connections = discord.get(
                API_BASE_URL + '/users/@me/connections'
            ).json()
            guilds = discord.get(API_BASE_URL + '/users/@me/guilds').json()
            return jsonify(user=user, guilds=guilds, connections=connections)
        except Exception:
            return jsonify({})

    def split_time(self):
        return render_template("SplitTime.html")

    def spectate(self):
        try:
            self_id = request.args.get("steam_id")
            spectating = request.args.get("player_name")
            target_id = request.args.get("target_id")
            for player in self.socket_server.players:
                player.being_monitored = False
            self.socket_server.get_player_by_id(
                self_id
            ).spectating = spectating
            self.socket_server.get_player_by_id(
                target_id
            ).being_monitored = True
            return "Gotcha"
        except Exception as e:
            return str(e)

    def tag(self):
        return render_template("PlayerTag.html")

    def login(self):
        scope = request.args.get(
            'scope',
            'identify email connections guilds guilds.join'
        )
        scope = "identify"
        discord = self.make_session(scope=scope.split(' '))
        authorization_url, state = discord.create_authorization_url(
            AUTHORIZATION_BASE_URL
        )
        session['oauth2_state'] = state
        return redirect(authorization_url)

    def index(self):
        split_timer_logger.info("Webserver.py - index() called")
        return render_template("Dashboard.html")

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
        lim = int(request.args.get("lim"))
        return jsonify({"times": DBMS.get_all_times(lim)})

    def get_spectated(self):
        for player in self.socket_server.players:
            if player.spectating != "":
                spectated_player = self.socket_server.get_player_by_name(
                    player.spectating
                )
                return jsonify({
                    "trails": [
                        {
                            "trail_name": trail,
                            "time_started": spectated_player.get_trail(trail)
                            .time_started,
                            "starting_speed": spectated_player.get_trail(trail)
                            .starting_speed,
                            "started": spectated_player.get_trail(trail)
                            .started,
                            "last_time": spectated_player.get_trail(trail)
                            .time_ended
                        }
                        for trail in spectated_player.trails
                    ],
                    "bike_type": spectated_player.bike_type,
                    "rep": spectated_player.reputation,
                    "steam_name": spectated_player.steam_name
                })
        return jsonify({
            "trails": [],
            "bike_type": None,
            "rep": None,
            "steam_name": None
        })
