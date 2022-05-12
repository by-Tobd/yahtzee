from random import randint
from collections import Counter

RULES = [
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

def straight(rolls:list,lenght:int):
	super = set(rolls)
	for i in range(7-lenght): #6-lenght-1
		if super.issuperset({j+i+1 for j in range(lenght)}):
			return True
	return False

def isyahtzee(rolls:list):
    return rolls.count(rolls[0]) == 5


def roll(die:list, index:list):
	#return type:list 
	for i in index:
		die[i] = randint(1,6)
	return die

def points(rolls:list, row:int):
	#row between 0 and 12
	#return type:int
	return RULES[row](rolls) 

def uppersum(score:list):
    total = sum(score[:6]) 
    return total + (total>63) * 35

def lowersum(score:list):
    return sum(score[6:-1]) + score[-1] * 100

