

class Webserver():
    def __init__(self):
        pass

    def permission():
        if session.get('oauth2_token') is None:
            return "UNKNOWN"
        discord = make_session(token=session.get('oauth2_token'))
        user = discord.get(API_BASE_URL + '/users/@me').json()
        if user["id"] in [str(x[0]) for x in DBMS.get_valid_ids()]:
            return "AUTHORISED"
        return "UNAUTHORISED"