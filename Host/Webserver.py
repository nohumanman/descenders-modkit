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
