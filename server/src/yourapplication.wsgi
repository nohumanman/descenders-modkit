
import sys
sys.path.insert(0, '/var/www/descenders-modkit')

from main import webserver as a

application = a.webserver_app

