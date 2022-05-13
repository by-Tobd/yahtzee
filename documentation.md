# Documentation

## Tiefere Funktionsweise

### Allgemeine Serverinformationen

Ein Client ist ein Programm, welches sich zu einem Server verbindet. In userem Fall ist dies die GUI, die auf [yahtzee.tobd.ga](https://yahtzee.tobd.ga) zu finden ist.

Die Kommunikation zwischen Server und Client funktioniert über das Websocket Protokoll. Der Server wartet auf Websocket Anfragen von dem Client, dies haben wir mit Hilfe der [Websockets]() library gemacht. Wenn sich ein GUI verbindet, setzt der Server einige Variabeln und akzeptiert die Anfrage. Bei einer normalen HTTP Verbindung (diese wird bei jeder Website verwendet) kann der Server nur auf Anfragen von dem Client reagieren und nicht einfach so Daten an den Client schicken. Bei Websockets ist dies nach Verbindungsaufbau möglich, was bei einem Spiel wie diesem wichtig ist, da der Client über die Aktionen der Anderen die ganze Zeit informiert werden muss.

### Datenstruktur

Sobald die Verbindung aufgebaut ist kann der Client verschiedene Anfragen senden und wird über Änderungen informiert, die für ihn relevant sind. Die Datenübertragung funktioniert in Form von JSON strings. JSON ist eine Möglichkeit zur Datendarstellung bei der Werte zu Namen geordnet werden. Die Namen werden "Keys" gennannt und die Werte "Values".  
Beispiel mit "name", "age" und "car" als Keys und "John", 30 und null als deren Values:

    {"name":"John", "age":30, "car":null}

Wir benutzen Json immer in Form von zwei Keys nämlich "type" und "data". Den "type" Key benutzen wir um die Art der Anfrage zu erkennen und diese davon abhängig richtig weiter zugeben.  
Beispiel:

    {"type":"roll", "data":[0,1,3]}

<br>

<details>
<summary> Mögliche Nachrichten </summary>
In diesem Absatz werden die einzelnen Nachrichten erklärt die der Server senden bzw. empfangen kann.
<br><br>

<details>
<summary> Von Client zu Server </summary>
Das sind die Nachrichten die der Server von dem Client empfängt.
<br><br>

<details>
<summary>Typ: roll</summary>
Würfeln der gewählten Würfel.<br>
Daten: Eine Liste mit Indizes, welche Würfel neu gewürfelt werden sollen.  

    {"type":"roll", "data":[0,1,3]}
</details>

<details>
<summary>Typ: end_turn</summary>
Eintragen des aktuellen Wurfs in die Wertetabelle.<br>
Daten: Ein Index in welche Reihe eingetragen werden soll.

    {"type":"end_turn", "data":3}
</details>

<details>
<summary>Typ: list_rooms</summary>
Anfordern der aktuellen Raumliste.<br>
Daten: Keine, Feld muss aber trotzdem vorhanden sein.

    {"type":"list_rooms", "data":None}
</details>

<details>
<summary>Typ: create_room</summary>
Erstellen eines Raums.<br>
Daten: Ein Dictionary mit den Keys "room_name" und "username"

    {"type":"create_room", "data":{"room_name":"Test Room", "username":"Tobd"}}
</details>

<details>
<summary>Typ: join_room </summary>
Beitreten eines Raums.<br>
Daten: Ein Dictionary mit den Keys "room_name" und "username"

    {"type":"join_room", "data":{"room_name":"Test Room", "username":"Tobd"}}
</details>

<details>
<summary>Typ: leave_room </summary>
Verlassen eines Raums.<br>
Daten: Keine, Feld muss aber vorhanden sein

    {"type":"leave_room", "data":None}
</details>
</details>


<details>
<summary> Von Server zu Client </summary>
Das sind die Nachrichten die der Server an den Client sendet, um ihn über Änderungen zu informieren.
<br><br>

<details>
<summary>Typ: roll_update</summary>
Aktuelle Würfel eines Spielers.<br>
Daten: Eine Dictionary mit den Keys "player" und "dice".<br>
player: Nutzername des Spielers, dessen Würfel sich geändert haben.<br>
dice: Eine Liste der Länge 5 mit Werten 1-6.

    {"type":"roll_update", "data":{"player":"Omega", "dice":[1,3,5,1,2]}}
</details>

<details>
<summary>Typ: score_update</summary>
Aktuelle Wertetabelle eines Spielers.<br>
Daten: Eine Dictionary mit den Keys "player" und "score".<br>
player: Nutzername des Spielers, dessen Score sich geändert haben.<br>
score: Eine Liste der Länge 14 mit den Punkten der einzelnen Reihen. -1 wenn kein Wert eingetragen ist.  

    {"type":"score_update", "data":{"player":"Jerry", "score":[-1,-1,3,6,1,-1,2,23,-1,-1,-1,-1,-1,0]}}
</details>

<details>
<summary>Typ: room_update</summary>
Aktuelle Informationen über einen Raum.<br>
Daten: Eine Liste der aktuellen Spieler.<br>
Spieler: Ein Spieler ist ein Dictionary mit den Keys "name", "score" und "dice".<br>
name: Nutzername des Spielers.<br>
dice: Eine Liste der Länge 5 mit Werten 1-6.<br>
score: Eine Liste der Länge 14 mit den Punkten der einzelnen Reihen. -1 wenn kein Wert eingetragen ist.   

    {"type":"room_update", "data":{"name":"Test Room", "players":[{"name":"Omega", "score":[-1,-1,3,6,1,-1,2,23,-1,-1,-1,-1,-1,0], "dice":[1,3,5,1,2]}]}}
</details>

<details>
<summary>Typ: finish_update</summary>
Informationen über einen Spieler, der seine Tabelle fertig ausgefüllt hat.<br>
Daten: Ein Dictionary mit den Keys "player", "uppersum" und "total".<br>
player: Nutzername des Spielers.<br>
uppersum: Summe der Punkte der oberen Tabelle.<br>
total: Endpunktzahl des Spielers.   

    {"type":"finish_update","data":{"player":"Tobd","uppersum":123,"total":444}}
</details>

<details>
<summary>Typ: end_game</summary>
Signalisiert das Spielende und gibt den Gewinner preis.<br>
Daten: Ein Dictionary mit dem Key "winner".<br>
winner: Nutzername des Spielers, der gewonnen hat.

    {"type":"end_game","data":{"winner":"Tobd"}}
</details>

<details>
<summary>Typ: warning</summary>
Sendet eine Warnung. Hat keinen direkten Einfluss auf das Spiel, gibt nur Informationen an den Spieler.<br>
Daten: Ein string mit der Art der Warnung.

    {"type":"warning", "data":"invalid_username"}
</details>

<details>
<summary>Typ: room_list</summary>
Sendet eine aktuelle Liste der Räume an den Client.<br>
Daten: Ein Liste von Räumen.<br>
Raum: Ein Raum ist ein Dictionary mit den Keys "name" und "players".<br>
name: Name des Raums. <br>
players: Anzahl an Spieler im Raum.

    {"type":"room_list", "data":[{"name":"Test Room", "players":2}, {"name":"Test Room 2", "players":1}]}
</details>
</details>

</details>
