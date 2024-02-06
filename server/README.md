# Server-side code
Written in python using flask and asyncio. The server is responsible for handling HTTP requests as a website and socket requests from the client and communicating with the database.

The server is written with asyncio to allow for asynchronous communication with the database and the client. This allows for the server to handle multiple requests at once and not block the main thread.

## Setup and Usage
1. Install python and create a virtual environment
1. Install dependencies: `pip install -r requirements.txt`
1. Run the server in src: `python server.py`

