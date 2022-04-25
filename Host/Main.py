from SocketServer import SocketServer

HOST = "127.0.0.1"
PORT = 65432

webserverHost = SocketServer(HOST, PORT)

webserverHost.start()

