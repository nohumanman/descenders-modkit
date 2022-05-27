import tweepy
from tokens import consumer_key, consumer_secret, access_token, access_token_secret

# Untested.

class TwitterBot():
    def __init__(self):
        self.__auth = tweepy.OAuth1UserHandler(
            consumer_key,
            consumer_secret,
            access_token,
            access_token_secret
        )
        self.__api = tweepy.API(auth)
    
    def tweet(self, tweet_text):
        self.__api.update_status("This Tweet was Tweeted using Tweepy!")
