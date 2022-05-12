

# Spielanleitung
Für jeden Spieler ist auf dem Yahtzee-Block eine
Spalte vorgesehen, in der seine Ergebnisse
eingetragen werden. In jeder Runde muss jeder
Mitspieler in einem der 13 Kästchen eine
Eintragung machen. Dies sollte möglichst die
Summe der gewürfelten Augen sein, aber –
wenn’s nicht anders geht – ist leider auch mal eine
Null dabei...



## Spielverlauf
Jeder Spieler darf pro Runde bis
zu dreimal würfeln. Der erste Wurf erfolgt mit
allen fünf Würfeln.
Danach kann der Spieler sich entscheiden, mit
wie vielen Würfeln er beim 2. und 3. Versuch
würfeln möchte. Dabei klickt er die Würfel an, die er
noch einmal würfeln möchte und klickt auf den "Roll"-Knopf. 
So kann er versuchen, sein Ergebnis mit dem 2. und 3.
Versuch zu verbessern. Er kann aber auch auf
einen oder beide Zusatzwürfe verzichten. Spätestens 
nach dem dritten Wurf muss der Spieler
sein Wurfergebnis in den Yahtzee-Block eintragen. 
Dazu klickt er auf das gewünschte Feld.
Erfüllt der Wurf keine der Bedingungen
für die Kästchen, wird das gewählte Kästchen
„gestrichen“ (= eine Null wird eingetragen).


## Die Punkte
Der Yahtzee-Block ist unterteilt in einen oberen
und einen unteren Teil. Im oberen Teil sind die
Kästchen für Einsen, Zweien, Dreien, Vieren,
Fünfen und Sechsen. Entschließt sich ein Spieler, 
sein Würfelergebnis hier eintragen zu lassen,
zählt er alle gewürfelten gleichen Zahlen zusammen 
und der Schreiber trägt das Ergebnis in das
entsprechende Kästchen ein.

### Bonus
Um den Bonus von 35 Punkten zu bekommen,
müssen die Spieler mindestens 63 Punkte im
gesamten oberen Teil zusammenaddiert erreichen.

Im unteren Teil sind die Kästchen für die
Sonderwürfe:

### Dreierpasch:
Mindestens drei gleiche Zahlen. Alle Augen
des Wurfes (nicht nur die der gleichen Zahlen) werden zusammengezählt.

### Viererpasch:
Mindestens vier gleiche Zahlen. Auch hier
werden alle Augen des Wurfes zusammengezählt.

### Full-House:
Drei gleiche und zwei gleiche, andere Zahlen
(z. B. drei Vieren und zwei Einsen). Für ein
Full-House gibt es pauschal 25 Punkte

### Kleine Straße:
Eine Folge von vier aufeinander folgenden
Würfeln. Der fünfte Würfel kann eine beliebige 
Zahl zeigen (z.B. 1, 2, 3, 4, 2). Eine kleine
Straße bringt pauschal 30 Punkte

### Große Straße:
Eine Folge von fünf aufeinander folgenden
Würfeln (z. B. 2, 3, 4, 5, 6). Eine große Straße
bringt pauschal 40 Punkte.

### Yahtzee:
5 gleiche Zahlen. Ein Yahtzee bringt 50 Punkte.

### Chance:
Gibt dem Spieler die Möglichkeit, einen missglückten Wurf, 
der keine Bedingung erfüllt,
trotzdem als Punkte in seinem „Chancefeld“
gutschreiben zu lassen. Alle Augen des
Wurfes werden zusammenaddiert.

### Zweites, drittes usw. Yahtzee:
Für jeden zweiten und weiteren Yahtzee bekommt
der Spieler 100 Zusatzpunkte. Zusätzlich muss er
das Gewürfelte Ergebnis in ein Feld des Yahtzee-Blocks eintragen. 


### Beispiel:
Marie hat drei Fünfen und zwei Dreien
gewürfelt. Sie hat nun vier Möglichkeiten:
<ul>
    <li> 15 Punkte. Im oberen Teil in ihrem Kästchen für die Fünfen (Die Würfel, die andere Zahlen zeigen, werden nicht gezählt.)
    <li> 6 Punkte. Im oberen Teil in ihrem Kästchen für die Dreien
    <li> 21 Punkte. Im unteren Teil in ihrem Kästchen für „Dreierpasch“
    <li> 25 Punkte. Im unteren Teil in ihrem Kästchen für „Full House“
</ul>

## Spielende
Das Spiel endet, sobald das letzte Kästchen
beim letzten Spieler ausgefüllt ist. 
<br> Der Spieler mit den meisten Punkten
ist der Sieger.

# Dokumentation

## Entwicklungsablauf
Zu beginn des Projekts haben wir zunächst die Aufgaben verteilt. Die Spiellogik wurde von Frederik geschrieben, die Serverinfrastruktur von Tobias und das graphische Benutzereingabe (GUI) haben Tobias und Jannik jeweils einen Prototypen entwickelt. Wir haben uns bei der GUI für Tobias' Prototypen entschieden, da das Werkzeug, das er benutzt hat, einfacher zu bedienen ist und wir somit deutlich effizienter arbeiten konnten.
Wir haben jetzt unsere Teile des Projekts fertiggestellt. 
Nun mussten wir die Spiellogik über den Server mit der GUI verbinden.
Abschließend haben wir Features wie das Raumauswählen und die Gewinneranzeige hinzugefügt.
Die Dokumentation und Spielanleitung wurde nachträglich von Jannik geschrieben.

## Funktionsweise
Das Spiel funktioniert indem der Server mit dem Spiellogik und dem GUI interagiert.
Es wird über Websockets im JSON-Format kommuniziert.
Der Spieler interagiert ausschließlich mit dem GUI.
<br>Geht der Spieler auf die Website, läd er die GUI und kann einen Raum erstellen (und diesem beitreten) oder einem Raum beitreten. Beim Erstellen des Raums erzeugt der Server ein Objekt, in das die Daten des Spieles eingetragen werden. Wenn der Spieler dem Raum beitritt werden seine Spieldaten dem Raum zugefügt. Nun kann er auf seinem Bildschirm duch das Anklicken von seinen Würfeln, dem Roll-Knopf und seine Punktefelder das Spiel speilen.
Dabei wird vom Server geschaut ob die Eingaben gültig sind. Wenn dies der Fall ist wird die Eingabe an die Spiellogik weitergegeben, diese gibt dann den entsprechenden Wert zurück (z.B. Punkte oder einen neuen Wurf). Dann von dem von dem Server gespeichert und an alle Spieler des Raumes gesendet.
Sobalt jeder fertig ist mit dem Ausfüllen seiner Tabelle, wird vom Server gesendet wer der Gewinner ist und die GUI zeigt dies an.

## Verwendete Libraries und Tools
[Unity](https://unity.com/)
<br>[LitJSON](https://litjson.net/)
<br>[NativeWebSocket](https://github.com/endel/NativeWebSocket)
<br>[websockets](https://pypi.org/project/websockets/)


