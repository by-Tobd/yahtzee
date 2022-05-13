from random import randint
from collections import Counter

RULES = [ 
    # Regel der Spalte
	lambda x:x.count(1),
	lambda x:x.count(2)*2,
	lambda x:x.count(3)*3,
	lambda x:x.count(4)*4,
	lambda x:x.count(5)*5,
	lambda x:x.count(6)*6,
	lambda x:sum(x) if x.count(sorted(x)[2])>=3 else 0,
	lambda x:sum(x) if x.count(sorted(x)[2])>=4 else 0,
	lambda x:25 if sorted(Counter(x).values()) == [2,3] else 0,
	lambda x:30 if straight(x,4) else 0,
	lambda x:40 if straight(x,5) else 0,
	lambda x:50 if isyahtzee(x) else 0,
	lambda x:sum(x),
]

def straight(rolls:list,lenght:int) -> bool:
	"""Prüft ob "rolls" ein Straße der Länge "length" ist und gibt das Ergebnis als Boolean zurück."""
	super = set(rolls)
	for i in range(7-lenght):
		if super.issuperset({j+i+1 for j in range(lenght)}):
			return True
	return False

def isyahtzee(rolls:list) -> bool:
    """Prüft ob "rolls" ein Yahtzee ist und gibt das Ergebnis als Boolean zurück."""
    return rolls.count(rolls[0]) == 5


def roll(die:list, index:list) -> list:
	"""Rollt die Würel "die" i wenn der jeweilige Wert in "index" Wahr ist."""
	for i in index:
		die[i] = randint(1,6)
	return die

def points(rolls:list, row:int) -> int:
	"""Berechnet die Punkte für den Wurf "rolls" in der Reihe "rows"."""
	return RULES[row](rolls) 

def uppersum(score:list) -> int:
    """Berechnet die Punkte der oberen Reihen und rechnet den Bonus ein."""
    total = sum(score[:6]) 
    return total + (total>=63) * 35

def lowersum(score:list) -> int:
    """Berechnet die Punkte der oberen Reihen und rechnet den Bonus ein."""
    return sum(score[6:-1]) + score[-1] * 100
