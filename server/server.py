import asyncio

import websockets
import json
from websockets.legacy.server import WebSocketServerProtocol
import random
import yahtzee

MAX_PLAYER_PER_ROOM = 4

class Server:

    def __init__(self):
        self.HANDLERS = {
            "roll": self.roll,
            "end_turn": self.end_turn,
            "list_rooms": self.list_rooms,
            "create_room": self.create_room,
            "join_room": self.join_room,
            "leave_room":self.handle_leave
        }
        self.players = dict()
        self.rooms = dict()

    async def run(self) -> None:
        """Startet den Server und lässt ihn bis zu einem harten Abschalten laufen.
        Die Domain ist "localhost" und der Port ist "8765"."""
        async with websockets.serve(self.connected, "localhost", 8765):
            await asyncio.Future()  # run forever

    async def connected(self, websocket:WebSocketServerProtocol) -> None:
        """Fügt einen Spieler zu einem Spiel zu."""
        # Gibt dem Spieler eine freie Id.
        id = random.randint(0, 10000)
        while id in self.players.keys():
            id = random.randint(0, 10000)
        
        # Initialisiert den Spieler.
        self.players[id] = dict()
        self.players[id]["socket"] = websocket
        self.players[id]["score"] = [0 if i == 13 else -1 for i in range(14)]
        self.players[id]["rolls"] = [-1 for i in range(5)]
        self.players[id]["roll_times"] = 0
        self.players[id]["username"] = None
        self.players[id]["room"] = None
        self.players[id]["finished"] = None
        await websocket.send(json.dumps({"type":"player_id", "data":id}))

        # Prüft die Verbindung des Spielers.
        try:
            async for message in websocket:
                await self.handle(message, id)
        except websockets.exceptions.ConnectionClosedError:
            pass
        finally:
            await self.leave_room(id)
            del self.players[id]

    async def leave_room(self, id) -> None:
        """Wirft den Spieler der Id "id" raus.
        Wenn der Raum leer ist wird er geschlossen."""
        room = self.players[id]["room"]
        if room:
            self.rooms[room]["players"].remove(id)
            await self.update_room(room)
            if len(self.rooms[room]["players"]) == 0:
                del self.rooms[room]

    async def handle(self, msg, sender_id) -> None:
        """Decodet Daten vom Spieler und gibt Daten in die richtige Weiterverarbeitung."""
        try:
            json_msg = json.loads(msg)
        except json.JSONDecodeError:
            return None
        print(f"\/ {json_msg}")

        if json_msg["type"] in self.HANDLERS:
            await self.HANDLERS[json_msg["type"]](json_msg["data"], sender_id)


    async def send(self, player_id, type:str, data) -> None:
        """Encodet Daten und sendet diese an den Spieler."""
        msg = json.dumps({"type":type, "data":data})
        print(f"/\ {msg}")
        await self.players[player_id]["socket"].send(msg)

    async def roll(self, data, sender_id) -> None:
        """Rollt die Würfel des Spielers und sendet diese an den Spieler.
        Wenn die maximale Anzahl an Würfen erreicht ist, wird eine Warnung an den Spieler gesendet."""
        if self.players[sender_id]["roll_times"] < 3:
            self.players[sender_id]["roll_times"] += 1
            current_rolls = self.players[sender_id]["rolls"]
            if self.players[sender_id]["roll_times"] == 1:
                data = [0,1,2,3,4]
            self.players[sender_id]["rolls"] = yahtzee.roll(current_rolls, data)

            room_name = self.players[sender_id]["room"]
            data = {"player":self.players[sender_id]["username"], "dice":self.players[sender_id]["rolls"]}
            for player in self.rooms[room_name]["players"]:
                await self.send(player, "roll_update", data)
        else:
            await self.send(sender_id, "warning", "maximum_rolls")

    async def handle_leave(self, data, sender_id) -> None:
        """Syntaktisch Notwendig!"""
        await self.leave_room(sender_id)

    async def end_turn(self, row, sender_id) -> None:
        """Trägt die Punkte eines Spielers ein und sendet den neuen Stand an alle.
        Wenn der Spieler noch nicht gerollt hat oder die Punkte nicht eintragen kann, wird ihm eine Wahnung geschickt."""

        if self.players[sender_id]["finished"]:
            await self.send(sender_id, "warning", "finished")
            return

        if not self.players[sender_id]["roll_times"] > 0:
            await self.send(sender_id, "warning", "not_rolled")
            return
        
        if self.players[sender_id]["score"][row] != -1:
            await self.send(sender_id, "warning", "row_in_use")
            return

        rolls = self.players[sender_id]["rolls"]
        self.players[sender_id]["roll_times"] = 0

        if yahtzee.isyahtzee(rolls) and self.players[sender_id]["score"][11] != -1:
            self.players[sender_id]["score"][13] += 1
        
        points = yahtzee.points(rolls, row)
        self.players[sender_id]["score"][row] = points
        
        room_name = self.players[sender_id]["room"]
        data = {"player":self.players[sender_id]["username"], "score":self.players[sender_id]["score"]}
        for player in self.rooms[room_name]["players"]:
            await self.send(player, "score_update", data)


        if -1 not in self.players[sender_id]["score"]:
            data = {"player":self.players[sender_id]["username"],"uppersum":yahtzee.uppersum(self.players[sender_id]["score"])}
            data["total"] = data["uppersum"] + yahtzee.lowersum(self.players[sender_id]["score"])

            self.players[sender_id]["finished"] = data["total"]
            for player in self.rooms[room_name]["players"]:
                await self.send(player, "finish_update", data)

            room_name = self.players[sender_id]["room"]
            room_scores = [self.players[id]["finished"] for id in self.rooms[room_name]["players"]]
            if None not in room_scores:
                winner_score = max(room_scores)
                for id in self.rooms[room_name]["players"]:
                    if self.players[id]["finished"] == winner_score:
                        winner = self.players[id]["username"]
                for id in self.rooms[room_name]["players"]:
                    await self.send(id,"end_game",{"winner":winner})
                



    async def update_room(self, room_name) -> None:
        """Läd die Daten aller Spieler und sendet sie an die Spieler."""
        data = dict()
        data["name"] = room_name
        data["players"] = list()
        
        for player in self.rooms[room_name]["players"]:
            pl_data = dict()
            pl_data["name"] = self.players[player]["username"]
            pl_data["score"] = self.players[player]["score"]
            pl_data["dice"] = self.players[player]["rolls"]
            data["players"].append(pl_data)
            
        for player in self.rooms[room_name]["players"]:
            await self.send(player, "room_update", data)

    async def list_rooms(self, _, sender_id) -> None:
        """Gibt dem Spieler die Liste an Spielräumen.
        Wenn der Spieler bereits in einem Raum ist, wird ihm eine Warnung geschickt."""
        data = list()

        for name, room in self.rooms.items():
            data.append(dict())
            data[-1]["name"] = name
            data[-1]["players"] = len(room["players"])
        await self.send(sender_id, "room_list", data)

    async def create_room(self, data, sender_id) -> None:
        """Eröffnet einen Spielraum und lässt den Spieler beitreten."""
        if self.players[sender_id]["room"] != None:
            await self.send(sender_id, "warning", "already_in_room")
            return

        if not data["username"]:
            await self.send(sender_id, "warning", "invalid_username")
            return

        if not data["room_name"]:
            await self.send(sender_id, "warning", "invalid_room_name")
            return

        if data["room_name"] in self.rooms.keys():
            await self.send(sender_id, "warning", "room_name_in_use")
            return


        for id, player in self.players.items():
            if data["username"] == player["username"]:
                if id != sender_id:
                    await self.send(sender_id, "warning", "invalid_username")
                    return

        room = dict()
        room["players"] = list()

        self.rooms[data["room_name"]] = room

        self.players[sender_id]["username"] = data["username"]

        await self.send_room_join(data["room_name"], sender_id)

    async def send_room_join(self, room_name, player_id) -> None:
        """Initiiert den beitretenden Spieler,
        sendet ihm ihm die Daten des Raumes und schickt allen im Raum die aktuellen Daten."""
        data = dict()
        data["name"] = room_name
        data["players"] = list()
        
        for player in self.rooms[room_name]["players"]:
            pl_data = dict()
            pl_data["name"] = self.players[player]["username"]
            pl_data["score"] = self.players[player]["score"]
            pl_data["dice"] = self.players[player]["rolls"]
            data["players"].append(pl_data)
            
        self.rooms[room_name]["players"].append(player_id)
        self.players[player_id]["room"] = room_name

        await self.update_room(room_name)        


    async def join_room(self, data, sender_id) -> None:
        """Lässt Spieler in einen Spielraum beitreten."""
        if self.players[sender_id]["room"] != None:
            await self.send(sender_id, "warning", "already_in_room")
            return
        
        if not data["username"]:
            await self.send(sender_id, "warning", "invalid_username")
            return
        if not data["room_name"] in self.rooms.keys():
            await self.send(sender_id, "warning", "room_does_not_exists")
            return

        if len(self.rooms[data["room_name"]]["players"]) >= MAX_PLAYER_PER_ROOM:
            await self.send(sender_id, "warning", "room_full")
            return

        for player in self.rooms[data["room_name"]]["players"]:
            if self.players[player]["username"] == data["username"]:
                await self.send(sender_id, "warning", "username_already_in_room")
                return

        self.players[sender_id]["username"] = data["username"]
        await self.send_room_join(data["room_name"], sender_id)
            

def main() -> None:
    server = Server()

    asyncio.run(server.run())

if __name__ == "__main__":
    main()

    rooms = {"room_name":{"players":[132]}}
    players = {id:{"socket":"websocket", "score":[i for i in range(14)],"rolls":[i for i in range(5)],"roll_times":0,"username":"str","room":"str","final_score":700}}

    #Recieve
    roll = {"type":"roll", "data":[0,1,3]}
    end_turn = {"type":"end_turn", "data":3}

    list_rooms = {"type":"list_rooms", "data":None}
    create_room = {"type":"create_room", "data":{"room_name":"Test Room", "username":"Tobd"}}
    join_room = {"type":"join_room", "data":{"room_name":"Test Room", "username":"Tobd"}}
    leave_room = {"type":"leave_room", "data":None}

    #Send
    roll_update = {"type":"roll_update", "data":{"player":"Omega", "dice":[1,3,5,1,2]}}
    score_update = {"type":"score_update", "data":{"player":"Jerry", "score":[-1,-1,3,6,1,-1,2,23,-1,-1,-1,-1,-1,0]}}
    room_update = {"type":"room_update", "data":{"name":"Test Room", "players":[{"name":"Omega", "score":[-1,-1,3,6,1,-1,2,23,-1,-1,-1,-1,-1,0], "dice":[1,3,5,1,2]}]}}
    finish_update = {"type":"finish_update","data":{"player":"Tobd","uppersum":123,"total":444}}
    end_game = {"type":"end_game","data":{"winner":"Tobd"}}

    warnings = [
        {"type":"warning", "data":"maximum_rolls"},
        {"type":"warning", "data":"not_rolled"},
        {"type":"warning", "data":"row_in_use"},
        {"type":"warning", "data":"invalid_username"},
        {"type":"warning", "data":"room_full"},
        {"type":"warning", "data":"room_name_in_use"},
        {"type":"warning", "data":"username_already_in_room"},
        {"type":"warning", "data":"room_does_not_exists"},
        {"type":"warning", "data":"invalid_room_name"},
        {"type":"warning", "data":"already_in_room"},
        {"type":"warning", "data":"finished"}
    ]

    room_list = {"type":"room_list", "data":[{"name":"Test Room", "players":2}, {"name":"Test Room 2", "players":1}]}
