# DOCS:
# 1. https://github.com/dotnet/aspnetcore/blob/main/src/SignalR/docs/specs/TransportProtocols.md
# 2. https://github.com/dotnet/aspnetcore/blob/main/src/SignalR/docs/specs/HubProtocol.md

import asyncio
import websockets
import requests
import json
import uuid
import ssl
import pathlib
import re
import client
from log import logger as aihub_logger

class StreamCmdClient():
    def __init__(self, access_token):
        self.base_url = client.aihub_host # without / at the end
        self.wss_url = self.base_url.replace("https://", "wss://")
        self.hub_url = "streamcmd"
        self.access_token = access_token

    def toSignalRMessage(self, data):
        return f'{json.dumps(data)}\u001e'

    async def connectToHub(self, connectionId, project_name, file_run, howMuchVRAM, run_again):
        ssl_context = ssl.SSLContext(ssl.PROTOCOL_TLS_CLIENT)
        ssl_context.load_verify_locations(
            pathlib.Path(__file__).with_name("cert.pem")
        )

        invocationId = uuid.uuid4()
        projectName = project_name
        command = file_run
        howMuchRAM = howMuchVRAM
        runAgain = run_again

        uri = f"""{self.wss_url}/{self.hub_url}?id={connectionId}&projectName={project_name}&access_token={self.access_token}"""
        async with websockets.connect(uri, ssl=ssl_context) as websocket:        

            async def handshake():
                await websocket.send(self.toSignalRMessage({"protocol": "json", "version": 1}))
                handshake_response = await websocket.recv()
                #aihub_logger.info(f"handshake_response: {handshake_response}")

            async def start_pinging():
                nonlocal _running
                while _running:
                    await asyncio.sleep(10)
                    await websocket.send(self.toSignalRMessage({"type": 6}))

            async def listen():
                nonlocal _running
                while _running:
                    get_response = await websocket.recv()

                    get_response_re = re.split('\▲|\x1e+', get_response) # Server returns data with these strange letters at the end. Split by it to have human-readable one line info.
                    for get_response_one in filter(None, get_response_re): # filter removes empty strings
                        if get_response_one.find(f"\"type\":3") != -1: # -1 if not found, other number = index where "type":3 starts
                            print("[INFO] End of livestream. Press CTRL+C or wait a few seconds.")
                            _running = False
                            break

                        if get_response_one.find(f"\"type\":7") != -1:
                            print("[ERROR] Server disconnected you from resources. Your access token is probably expired. Please logout and login again.")
                            _running = False
                            break
                        
                        if get_response_one.find(f"\"type\":6") == -1: # ping
                            get_response_one_json = json.loads(get_response_one)
                            server_response = get_response_one_json["arguments"]
                            if(server_response[0] != None):
                                print(server_response[0]) # There should be only 1 in that arguments       
                        
            _running = True
            # First step - Handshake - get connectionId
            await handshake()        
            
            listen_task = asyncio.create_task(listen())
            ping_task  = asyncio.create_task(start_pinging())

            # GETTINGS MESSAGES: 
            message = {
                "type": 1, # 1 invocation, 4 stream
                "invocationId": f"{invocationId}", # An optional string encoding the Invocation ID for a message
                "target": "Stream", # Wywoływana funkcja
                "arguments": [ f"{projectName}", f"{command}", float(howMuchRAM), runAgain]            
            }
            #print(message)
            await websocket.send(self.toSignalRMessage(message))

            await ping_task
            await listen_task  

    def run(self, project_name, file, howMuchVRAM, runAgain):
        try:
            negotiation = requests.post(f"{self.base_url}/{self.hub_url}/negotiate?negotiateVersion=0", verify="cert.pem")
            # print(f"connectionId: {negotiation.json()['connectionId']}")
            asyncio.run(self.connectToHub(negotiation.json()['connectionId'], project_name, file, howMuchVRAM, runAgain))
        except Exception as e:
            aihub_logger.error("[ERROR] %s", e)
            print("[ERROR] Server error. Please try again later or follow the advice given to you earlier.")
