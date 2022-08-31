var ClientId, Pin, MatchList, TableId, PlayerNumber, Winner;
var AICount = 0;
var BalloonMovement = 0;
var EventCount = 0;
var PlayerCount = 0;
var Roll = 1;
var Timeout = 60;
var Active = false;
var Animating = false;
var BalloonsAnimating = false;
var Host = false;
var HTP = false;
var Shuffling = false;
var Started = false;
var ClaimedSquares = new Object();
var DiceList = new Object();
var SquareList = new Object();
var DiceFace = new Array(15);
var HeldDice = new Array("false", "false", "false", "false", "false");
var Dice = new Array(0, 0, 0, 0, 0);
var AnimationQue = new Array();
var AnimationObserver;
var AnimationTimer;
var BalloonTimer;
var LobbyTimer;
var UpdateTimer;

var AudioClaim = new Audio("Audio/claim.mp3");
var AudioSingleRoll1 = new Audio("Audio/singleRoll1.mp3");
var AudioSingleRoll2 = new Audio("Audio/singleRoll2.mp3");
var AudioRoll1 = new Audio("Audio/roll1.mp3");
var AudioRoll2 = new Audio("Audio/roll2.mp3");
var AudioRoll3 = new Audio("Audio/roll3.mp3");
var AudioRoll4 = new Audio("Audio/roll4.mp3");
var AudioTurn = new Audio("Audio/turn.mp3");
var AudioJoin = new Audio("Audio/join.mp3");
var AudioLeave = new Audio("Audio/leave.mp3");
var AudioWin = new Audio("Audio/win.mp3");
var AudioLost = new Audio("Audio/lost.mp3");

function FreshPage() {
	BuildDice();
	BuildSquareList();
	StartObserver();
};

function BuildDice() {
	DiceList[0] = "Img/Die1.png";
	DiceList[1] = "Img/Die2.png";
	DiceList[2] = "Img/Die3.png";
	DiceList[3] = "Img/Die4.png";
	DiceList[4] = "Img/Die5.png";
	DiceList[5] = "Img/Die6.png";
};

function BuildSquareList() {
	SquareList["1_1"] = "5_0_0";
	SquareList["1_2"] = "2_6_0";
	SquareList["1_3"] = "3_3_2";
	SquareList["1_4"] = "1_1_0";
	SquareList["1_5"] = "6_0_0";
	SquareList["1_6"] = "3_5_2";
	SquareList["1_7"] = "1_3_0";
	SquareList["1_8"] = "3_6_1";
	SquareList["1_9"] = "5_0_0";
	SquareList["2_1"] = "4_0_0";
	SquareList["2_2"] = "7_0_0";
	SquareList["2_3"] = "1_5_0";
	SquareList["2_4"] = "3_6_3";
	SquareList["2_5"] = "3_2_1";
	SquareList["2_6"] = "3_5_4";
	SquareList["2_7"] = "3_4_1";
	SquareList["2_8"] = "7_0_0";
	SquareList["2_9"] = "4_0_0";
	SquareList["3_1"] = "1_4_0";
	SquareList["3_2"] = "3_6_4";
	SquareList["3_3"] = "5_0_0";
	SquareList["3_4"] = "3_4_3";
	SquareList["3_5"] = "2_5_0";
	SquareList["3_6"] = "3_4_2";
	SquareList["3_7"] = "4_0_0";
	SquareList["3_8"] = "3_5_1";
	SquareList["3_9"] = "1_6_0";
	SquareList["4_1"] = "2_1_0";
	SquareList["4_2"] = "1_2_0";
	SquareList["4_3"] = "3_6_5";
	SquareList["4_4"] = "4_0_0";
	SquareList["4_5"] = "2_3_0";
	SquareList["4_6"] = "6_0_0";
	SquareList["4_7"] = "3_3_1";
	SquareList["4_8"] = "3_6_2";
	SquareList["4_9"] = "3_5_3";
	SquareList["5_1"] = "6_0_0";
	SquareList["5_2"] = "2_4_0";
	SquareList["5_3"] = "7_0_0";
	SquareList["5_4"] = "2_2_0";
	SquareList["5_5"] = "8_0_0";
	SquareList["5_6"] = "2_2_0";
	SquareList["5_7"] = "7_0_0";
	SquareList["5_8"] = "2_4_0";
	SquareList["5_9"] = "6_0_0";
	SquareList["6_1"] = "3_5_3";
	SquareList["6_2"] = "3_6_2";
	SquareList["6_3"] = "3_3_1";
	SquareList["6_4"] = "6_0_0";
	SquareList["6_5"] = "2_3_0";
	SquareList["6_6"] = "4_0_0";
	SquareList["6_7"] = "3_6_5";
	SquareList["6_8"] = "1_2_0";
	SquareList["6_9"] = "2_1_0";
	SquareList["7_1"] = "1_6_0";
	SquareList["7_2"] = "3_5_1";
	SquareList["7_3"] = "4_0_0";
	SquareList["7_4"] = "3_4_2";
	SquareList["7_5"] = "2_5_0";
	SquareList["7_6"] = "3_4_3";
	SquareList["7_7"] = "5_0_0";
	SquareList["7_8"] = "3_6_4";
	SquareList["7_9"] = "1_4_0";
	SquareList["8_1"] = "4_0_0";
	SquareList["8_2"] = "7_0_0";
	SquareList["8_3"] = "3_4_1";
	SquareList["8_4"] = "3_5_4";
	SquareList["8_5"] = "3_2_1";
	SquareList["8_6"] = "3_6_3";
	SquareList["8_7"] = "1_5_0";
	SquareList["8_8"] = "7_0_0";
	SquareList["8_9"] = "4_0_0";
	SquareList["9_1"] = "5_0_0";
	SquareList["9_2"] = "3_6_1";
	SquareList["9_3"] = "1_3_0";
	SquareList["9_4"] = "3_5_2";
	SquareList["9_5"] = "6_0_0";
	SquareList["9_6"] = "1_1_0";
	SquareList["9_7"] = "3_3_2";
	SquareList["9_8"] = "2_6_0";
	SquareList["9_9"] = "5_0_0";
};

function Balloons(playerNumber) {
	BalloonsAnimating = true;
	let balloonImage;
	if (playerNumber === "1") {
		balloonImage = "Img/BlueBalloon.png";
	}
	else if (playerNumber === "2") {
		balloonImage = "Img/RedBalloon.png";
	}
	else if (playerNumber === "3") {
		balloonImage = "Img/GreenBalloon.png";
	}
	else {
		balloonImage = "Img/YellowBalloon.png";
	}
	AnimationQue.push(document.getElementById("Balloon1").src = balloonImage);
	AnimationQue.push(document.getElementById("Balloon2").src = balloonImage);
	AnimationQue.push(document.getElementById("Balloon3").src = balloonImage);
	AnimationQue.push("balloons");
	AnimationDelay();
};

function BalloonUp () {
	BalloonMovement += 2;
	if (BalloonMovement !== 1800) {
		let position = 800 - BalloonMovement;
		document.getElementById("Balloon1").style.top = position + 'px';
		document.getElementById("Balloon2").style.top = position + 'px';
		document.getElementById("Balloon3").style.top = position + 'px';
		BalloonAnimation();
    }
	else {
		BalloonsAnimating = false;
		AnimationDelay();
	}
};

function RollDice() {
	if (Active && !Shuffling && Roll != 4) {
		if (HeldDice[0] === "false" || HeldDice[1] === "false" || HeldDice[2] === "false" || HeldDice[3] === "false" || HeldDice[4] === "false") {
			if (Roll > 1) {
				for (let [key] of Object.entries(SquareList)) {
					if (document.getElementById(key).style.backgroundColor != "" && ClaimedSquares[key] == null) {
						AnimationQue.push(document.getElementById(key).style.backgroundColor = "");
					}
				}
			}
			Roll += 1;
			if (Roll === 2) {
				AnimationQue.push(document.getElementById("RollNumber").innerHTML = "1");
			}
			else if (Roll === 3) {
				AnimationQue.push(document.getElementById("RollNumber").innerHTML = "2");
			}
			else {
				AnimationQue.push(document.getElementById("RollNumber").innerHTML = "3");
				AnimationQue.push(document.getElementById("Roller").style.boxShadow = "");
			}
			let heldDice = HeldDice[0] + "," + HeldDice[1] + "," + HeldDice[2] + "," + HeldDice[3] + "," + HeldDice[4];
			let request = new XMLHttpRequest();
			request.open('POST', window.location.href.replace('rio.html', 'RollRio'), true);
			request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
			request.onerror = function() {
				StopUpdate();
				alert("RollDice Error: No response from server");
			};
			request.onload = function () {
				if (request.status === 200 && request.readyState === 4) {
					let newDice = "";
					let responseSplit = request.responseText.split('☼');
					Pin = CryptoJS.SHA512(ClientId + responseSplit[0]).toString();
					newDice = responseSplit[1].split(',');
					for (let i = 0; i < 5; i++) {
						let die = newDice[i] - 1;
						DiceFace[i] = die;
					}
					Shuffle();
				}
				if (request.status === 401 && request.readyState === 4) {
					StopUpdate();
					alert("This security ID has expired. Please acquire a new one");
				}
				else if (request.status === 403 && request.readyState === 4) {
					StopUpdate();
					alert("RollDice: Server rejected request");
				}
			};
			request.send(Pin + "☼" + TableId + "☼" + heldDice + "☼" + PlayerNumber);
			AnimationDelay();
		}
	}
};

function Shuffle() {
	Shuffling = true;
	let heldDice = 0;
	for (let i = 0; i < 5; i++) {
		if (HeldDice[i] === "false") {
			AnimationQue.push(document.getElementById("Die" + (i + 1)).style.transform = "rotate(40deg)");
		}
		else {
			heldDice += 1;
		}
	}
	AnimationDelay();
	if (heldDice === 4) {
		let number = Math.floor((Math.random() * 2) + 1);
		if (number === 1) {
			AudioSingleRoll1.play();
		}
		else {
			AudioSingleRoll2.play();
		}
	}
	else {
		let number = Math.floor((Math.random() * 4) + 1);
		if (number === 1) {
			AudioRoll1.play();
		}
		else if (number === 2) {
			AudioRoll2.play();
		}
		else if (number === 3) {
			AudioRoll3.play();
		}
		else {
			AudioRoll4.play();
		}
	}
	setTimeout(function(){  
		for (let i = 0; i < 5; i++) {
			if (HeldDice[i] === "false") {
				AnimationQue.push(document.getElementById("Die" + (i + 1)).style.transform = "rotate(80deg)");
			}
		}
		AnimationDelay();
		setTimeout(function(){  
			for (let i = 0; i < 5; i++) {
				if (HeldDice[i] === "false") {
					AnimationQue.push(document.getElementById("Die" + (i + 1)).style.transform = "rotate(120deg)");
				}
			}
			AnimationDelay();
			setTimeout(function(){  
				for (let i = 0; i < 5; i++) {
					if (HeldDice[i] === "false") {
						AnimationQue.push(document.getElementById("Die" + (i + 1)).style.transform = "rotate(160deg)");
					}
				}
				AnimationDelay();
				setTimeout(function(){  
					for (let i = 0; i < 5; i++) {
						if (HeldDice[i] === "false") {
							AnimationQue.push(document.getElementById("Die" + (i + 1)).style.transform = "rotate(200deg)");
						}
					}
					AnimationDelay();
					setTimeout(function(){  
						for (let i = 0; i < 5; i++) {
							if (HeldDice[i] === "false") {
								AnimationQue.push(document.getElementById("Die" + (i + 1)).style.transform = "rotate(240deg)");
								AnimationQue.push(document.getElementById("Die" + (i + 1)).src = DiceList[DiceFace[i]]);
								Dice[i] = DiceFace[i] + 1;
							}
						}
						AnimationDelay();
						setTimeout(function(){  
							for (let i = 0; i < 5; i++) {
								if (HeldDice[i] === "false") {
									AnimationQue.push(document.getElementById("Die" + (i + 1)).style.transform = "rotate(280deg)");
								}
							}
							AnimationDelay();
							setTimeout(function(){  
								for (let i = 0; i < 5; i++) {
									if (HeldDice[i] === "false") {
										AnimationQue.push(document.getElementById("Die" + (i + 1)).style.transform = "rotate(320deg)");
									}
								}
								AnimationDelay();
								setTimeout(function(){
									for (let i = 0; i < 5; i++) {
										if (HeldDice[i] === "false") {
											AnimationQue.push(document.getElementById("Die" + (i + 1)).style.transform = "rotate(0deg)");
											
										}
									}
									MatchList = "";
									let one = 0;
									let two = 0;
									let three = 0;
									let four = 0;
									let five = 0;
									let six = 0;
									let super7 = 0;
									let lucky11 = 0;
									for (let i = 0; i < 5; i++) {
										let die = Dice[i];
										super7 += die;
										lucky11 += die;
										if (die === 1) {
											one += 1;
										}
										else if (die === 2) {
											two += 1;
										}
										else if (die === 3) {
											three += 1;
										}
										else if (die === 4) {
											four += 1;
										}
										else if (die === 5) {
											five += 1;
										}
										else if (die === 6) {
											six += 1;
										}
									}
									if (one >= 2) {
										if (two >= 2) {
											MatchList += "2_5 8_5 ";
										}                         
										else if (three >= 2) {    
											MatchList += "4_7 6_3 ";
										}                         
										else if (four >= 2) {     
											MatchList += "2_7 8_3 ";
										}                         
										else if (five >= 2) {     
											MatchList += "3_8 7_2 ";
										}                         
										else if (six >= 2) {      
											MatchList += "1_8 9_2 ";
										}
										if (two >= 3) {
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}                                         
										else if (three >= 3) {                    
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}                                         
										else if (four >= 3) {                     
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}                                         
										else if (five >= 3) {                     
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}                                         
										else if (six >= 3) {                      
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}
									}
									if (two >= 2) {
										if (three >= 2) {
											MatchList += "1_3 9_7 ";
										}                         
										else if (four >= 2) {     
											MatchList += "3_6 7_4 ";
										}                         
										else if (five >= 2) {     
											MatchList += "1_6 9_4 ";
										}                         
										else if (six >= 2) {      
											MatchList += "4_8 6_2 ";
										}
										if (one >= 3) {
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}                                         
										else if (three >= 3) {                    
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}                                         
										else if (four >= 3) {                     
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}                                         
										else if (five >= 3) {                     
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}                                         
										else if (six >= 3) {                      
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}
									}
									if (three >= 2) {
										if (four >= 2) {
											MatchList += "3_4 7_6 ";
										}
										else if (five >= 2) {
											MatchList += "4_9 6_1 ";
										}
										else if (six >= 2) {
											MatchList += "2_4 8_6 ";
										}
										if (one >= 3) {
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}                                         
										else if (two >= 3) {                      
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}                                         
										else if (four >= 3) {                     
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}                                         
										else if (five >= 3) {                     
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}                                         
										else if (six >= 3) {                      
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}
									}
									if (four >= 2) {
										if (five >= 2) {
											MatchList += "2_6 8_4 ";
										}
										else if (six >= 2) {
											MatchList += "3_2 7_8 ";
										}
										if (one >= 3) {
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}                                         
										else if (two >= 3) {                      
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}                                         
										else if (three >= 3) {                    
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}                                         
										else if (five >= 3) {                     
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}                                         
										else if (six >= 3) {                      
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}                                         
									}                                             
									if (five >= 2) {                              
										if (six >= 2) {                           
											MatchList += "4_3 6_7 ";              
										}                                         
										if (one >= 3) {                           
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}                                         
										else if (two >= 3) {                      
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}                                         
										else if (three >= 3) {                    
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}                                         
										else if (four >= 3) {                     
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}                                         
										else if (six >= 3) {                      
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}                                         
									}                                             
									if (six >= 2) {                               
										if (one >= 3) {                           
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}                                         
										else if (two >= 3) {                      
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}                                         
										else if (three >= 3) {                    
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}                                         
										else if (four >= 3) {                     
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}                                         
										else if (five >= 3) {                     
											MatchList += "2_2 2_8 5_3 5_7 8_2 8_8 ";
										}
									}
									if (one >= 3) {
										MatchList += "1_4 9_6 ";
									}
									if (two >= 3) {
										MatchList += "4_2 6_8 ";
									}
									if (three >= 3) {
										MatchList += "1_7 9_3 ";
									}
									if (four >= 3) {
										MatchList += "3_1 7_9 ";
									}
									if (five >= 3) {
										MatchList += "2_3 8_7 ";
									}
									if (six >= 3) {
										MatchList += "3_9 7_1 ";
									}
									if (one >= 4) {
										MatchList += "4_1 6_9 ";
									}
									if (two >= 4) {
										MatchList += "5_4 5_6 ";
									}
									if (three >= 4) {
										MatchList += "4_5 6_5 ";
									}
									if (four >= 4) {
										MatchList += "5_2 5_8 ";
									}
									if (five >= 4) {
										MatchList += "3_5 7_5 ";
									}
									if (six >= 4) {
										MatchList += "1_2 9_8 ";
									}
									if ((one >= 1 && two >= 1 && three >= 1) || (four >= 1 && five >= 1 && six >= 1)) {
										MatchList += "2_1 2_9 3_7 4_4 6_6 7_3 8_1 8_9 ";
									}
									if (super7 === 7) {
										MatchList += "1_1 1_9 3_3 7_7 9_1 9_9 ";
									}
									else if (lucky11 === 11) {
										MatchList += "1_5 4_6 5_1 5_9 6_4 9_5 ";
									}
									if (one === 5 || two === 5 || three === 5 || four === 5 || five === 5 || six === 5) {
										MatchList += "ultra";
										for (let [key, value] of Object.entries(SquareList)) {
											if (ClaimedSquares[key] == null) {
												AnimationQue.push(document.getElementById(key).style.backgroundColor = "#BFBFBF");
											}
										}
										AnimationDelay();
									}
									else if (MatchList.length > 0) {
										for (let [key, value] of Object.entries(SquareList)) {
											if (ClaimedSquares[key] == null && MatchList.includes(key)) {
												AnimationQue.push(document.getElementById(key).style.backgroundColor = "#BFBFBF");
											}
										}
										if (Roll === 4) {
											AnimationQue.push(document.getElementById("Roller").style.boxShadow = "");
											for (let i = 1; i <= 5; i++) {
												AnimationQue.push(document.getElementById("Die" + i).style.boxShadow = "");
											}
										}
										AnimationDelay();
									}
									else {
										if (Roll === 4) {
											AnimationQue.push(document.getElementById("Roller").style.boxShadow = "");
											for (let i = 1; i <= 5; i++) {
												AnimationQue.push(document.getElementById("Die" + i).style.boxShadow = "");
											}
											AnimationDelay();
											EndTurn();
										}
									}
									Shuffling = false;
								}, 100);
							}, 100);
						}, 100);
					}, 100);
				}, 100);
			}, 100);
		}, 100);
	}, 100);
};

function EndTurn() {
	Roll = 1;
	let request = new XMLHttpRequest();
	request.open('POST', window.location.href.replace('rio.html', 'EndTurnRio'), true);
	request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
	request.onerror = function() {
		StopUpdate();
		alert("EndTurn Error: No response from server");
	};
	request.onload = function () {
		if (request.status === 200 && request.readyState === 4) {
			Pin = CryptoJS.SHA512(ClientId + request.responseText).toString();
		}
		else if (request.status === 401 && request.readyState === 4) {
			StopUpdate();
			alert("This security ID has expired. Please acquire a new one");
		}
		else if (request.status === 403 && request.readyState === 4) {
			StopUpdate();
			alert("EndTurn: Server rejected request");
		}
	};
	request.send(Pin + "☼" + TableId + "☼" + PlayerNumber);
};

function SetDie(number) {
	if (!Shuffling) {
		if (Roll > 1) {
			let die = number - 1;
			if (HeldDice[die] === "false") {
				HeldDice[die] = "true";
				if (PlayerNumber === "1") {
					AnimationQue.push(document.getElementById("Die" + number).style.boxShadow = "0px 0px 20px #0000B3");
				}
				else if (PlayerNumber === "2") {
					AnimationQue.push(document.getElementById("Die" + number).style.boxShadow = "0px 0px 20px #E60000");
				}
				else if (PlayerNumber === "3") {
					AnimationQue.push(document.getElementById("Die" + number).style.boxShadow = "0px 0px 20px #00CC00");
				}
				else if (PlayerNumber === "4") {
					AnimationQue.push(document.getElementById("Die" + number).style.boxShadow = "0px 0px 20px #E6E600");
				}
			}
			else {
				HeldDice[die] = "false";
				AnimationQue.push(document.getElementById("Die" + number).style.boxShadow = "");
			}
			AnimationDelay();
		}
	}
};

function SetSquare(id) {
	if (!Shuffling && Roll > 1 && (MatchList.includes("ultra") || ClaimedSquares[id] == null && MatchList.includes(id))) {
		ClaimedSquares[id] = PlayerNumber;
		if (PlayerNumber === "1") {
			AnimationQue.push(document.getElementById(id).style.backgroundColor = "#0000B3");
		}
		else if (PlayerNumber === "2") {
			AnimationQue.push(document.getElementById(id).style.backgroundColor = "#E60000");
		}
		else if (PlayerNumber === "3") {
			AnimationQue.push(document.getElementById(id).style.backgroundColor = "#00CC00");
		}
		else if (PlayerNumber === "4") {
			AnimationQue.push(document.getElementById(id).style.backgroundColor = "#E6E600");
		}
		for (let [key, value] of Object.entries(SquareList)) {
			if (ClaimedSquares[key] == null) {
				AnimationQue.push(document.getElementById(key).style.backgroundColor = "");
			}
		}
		for (let i = 1; i <= 5; i++) {
			AnimationQue.push(document.getElementById("Die" + i).style.boxShadow = "");
		}
		AnimationQue.push(document.getElementById("Roller").style.boxShadow = "");
		AnimationQue.push(document.getElementById("RollNumber").innerHTML = "");
		AnimationDelay();
		AudioClaim.play();
		HeldDice[0] = "false";
		HeldDice[1] = "false";
		HeldDice[2] = "false";
		HeldDice[3] = "false";
		HeldDice[4] = "false";
		Roll = 1;
		let winner = false;
		let claims = Object.entries(ClaimedSquares);
		if (claims.length >= 4) {
			let squares = Object.keys(SquareList);
			let clientClaims = new Array();
			for (let [key, value] of claims) {
				if (value == PlayerNumber) {
					clientClaims.push(key);
				}
			}
			clientClaims.push("5_5");
			let claimCount = clientClaims.length;
			if (claimCount >= 4) {
				for (let j = 0; j < claimCount; j++) {
					let claimSplit = clientClaims[j].split("_");
					let parsed1 = parseInt(claimSplit[0]);
					let parsed2 = parseInt(claimSplit[1]);
					for (let k = 1; k < 4; k++) {
						let position = parsed1 - k;
						let up = clientClaims[j].replace(claimSplit[0] + "_", position + "_");
						if (squares.includes(up)) {
							if (clientClaims.includes(up)) {
								if (k === 3) {
									winner = true;
									break;
								}
							}
							else {
								break;
							}
						}
						else {
							break;
						}
					}
					if (!winner) {
						for (let l = 1; l < 4; l++) {
							let position = parsed1 + l;
							let down = clientClaims[j].replace(claimSplit[0] + "_", position + "_");
							if (squares.includes(down)) {
								if (clientClaims.includes(down)) {
									if (l === 3) {
										winner = true;
										break;
									}
								}
								else {
									break;
								}
							}
							else {
								break;
							}
						}
					}
					if (!winner) {
						for (let m = 1; m < 4; m++) {
							let position = parsed2 - m;
							let left = clientClaims[j].replace("_" + claimSplit[1], "_" + position);
							if (squares.includes(left)) {
								if (clientClaims.includes(left)) {
									if (m === 3) {
										winner = true;
										break;
									}
								}
								else {
									break;
								}
							}
							else {
								break;
							}
						}
					}
					if (!winner) {
						for (let n = 1; n < 4; n++) {
							let position = parsed2 + n;
							let right = clientClaims[j].replace("_" + claimSplit[1], "_" + position);
							if (squares.includes(right)) {
								if (clientClaims.includes(right)) {
									if (n === 3) {
										winner = true;
										break;
									}
								}
								else {
									break;
								}
							}
							else {
								break;
							}
						}
					}
				}
			}
		}
		let request = new XMLHttpRequest();
		request.open('POST', window.location.href.replace('rio.html', 'ClaimRio'), true);
		request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
		request.onerror = function() {
			StopUpdate();
			alert("SetSquare Error: No response from server");
		};
		request.onload = function () {
			if (request.status === 200 && request.readyState === 4) {
				Pin = CryptoJS.SHA512(ClientId + request.responseText).toString();
			}
			if (request.status === 401 && request.readyState === 4) {
				StopUpdate();
				alert("This security ID has expired. Please acquire a new one");
			}
			else if (request.status === 403 && request.readyState === 4) {
				StopUpdate();
				alert("SetSquare: Server rejected request");
			}
		};
		request.send(Pin + "☼" + TableId + "☼" + id + "☼" + PlayerNumber + "☼" + winner);
	}
};

function EnterGame() {
	let secureId = document.getElementById("SecureId").value;
	if (secureId.length > 3 && secureId.length < 5) {
		if (document.getElementById("ConfirmEnter").checked) {
			let request = new XMLHttpRequest();
			request.open('POST', window.location.href.replace('rio.html', 'EnterRio'), true);
			request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
			request.onerror = function() {
				alert("EnterGame Error: No response from server");
			};
			request.onload = function () {
				if (request.status === 200 && request.readyState === 4) {
					ClientId = secureId;
					let responseSplit = request.responseText.split('☼');
					Pin = CryptoJS.SHA512(ClientId + responseSplit[0]).toString();
					TableId = responseSplit[1];
					PlayerNumber = responseSplit[2];
					AnimationQue.push(document.getElementById('SecuritySync').style.top = "-2000px");
					AnimationQue.push(document.getElementById('Header').style.top = "0px");
					AnimationQue.push(document.getElementById('LobbyContainer').style.top = "0px");
					AnimationDelay();
					StartUpdate();
				}
				else if (request.status === 202 && request.readyState === 4) {
					Host = true;
					ClientId = secureId;
					let responseSplit = request.responseText.split('☼');
					Pin = CryptoJS.SHA512(ClientId + responseSplit[0]).toString();
					TableId = responseSplit[1];
					PlayerNumber = responseSplit[2];
					AnimationQue.push(document.getElementById('SecuritySync').style.top = "-2000px");
					AnimationQue.push(document.getElementById('Header').style.top = "0px");
					AnimationQue.push(document.getElementById('LobbyContainer').style.top = "0px");
					AnimationDelay();
					StartUpdate();
				}
				else if (request.status === 400 && request.readyState === 4) {
					alert("Invalid security ID");
				}
				else if (request.status === 401 && request.readyState === 4) {
					alert("This security ID has expired. Please acquire a new one");
				}
				else if (request.status === 402 && request.readyState === 4) {
					alert("You must be in game to join");
				}
				else if (request.status === 403 && request.readyState === 4) {
					alert("EnterGame: Server rejected request");
				}
			};
			request.send(CryptoJS.SHA512(secureId).toString());
		}
		else {
			alert("Click the confirmation checkbox first");
		}
	}
	else {
		alert("Invalid security ID. ID must be 4 chars in length");
	}
};

function RequestUpdate() {
	let request = new XMLHttpRequest();
	request.open('POST', window.location.href.replace('rio.html', 'UpdateRio'), true);
	request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
	request.onerror = function() {
		StopUpdate();
		alert("RequestUpdate Error: No response from server");
	};
	request.onload = function () {
		if (request.status === 200 && request.readyState === 4) {
			let responseSplit = request.responseText.split('☼');
			Pin = CryptoJS.SHA512(ClientId + responseSplit[0]).toString();
			if (responseSplit[1].includes("§")) {
				let newEvents = responseSplit[1].split("§");
				for (let i = 0; i < newEvents.length; i++) {
					RunEvent(newEvents[i], 0);
				}
			}
			else {
				RunEvent(responseSplit[1], 0);
			}
		}
		if (request.status === 202 && request.readyState === 4) {
			Pin = CryptoJS.SHA512(ClientId + request.responseText).toString();
		}
		else if (request.status === 401 && request.readyState === 4) {
			StopUpdate();
			alert("This security ID has expired. Please acquire a new one");
			
		}
		else if (request.status === 403 && request.readyState === 4) {
			StopUpdate();
			alert("RequestUpdate: Server rejected request");
		}
	};
	request.send(Pin + "☼" + TableId + "☼" + EventCount);
};

function RunEvent(newEvent, pass) {
	if (pass === 0) {
		EventCount += 1;
	}
	if (!Shuffling) {
		let eventSplit = newEvent.split('╚');
		if (eventSplit[0] === "Host") {
			PlayerCount += 1;
			AnimationQue.push(document.getElementById('ConnectingUser1').innerHTML = eventSplit[1]);
			if (eventSplit[1].includes(" ")) {
				let nameSplit = eventSplit[1].split(" ");
				let nameReduced = nameSplit[0].substring(0, 12);
				AnimationQue.push(document.getElementById('PlayerName1').innerHTML = nameReduced);
			}
			else {
				let nameReduced = eventSplit[1].substring(0, 12);
				AnimationQue.push(document.getElementById('PlayerName1').innerHTML = nameReduced);
			}
			if (Host) {
				AnimationQue.push(document.getElementById('Status').innerHTML = "You are the host. Start the game when ready");
				AnimationQue.push(document.getElementById('StartButton').style.top = "0px");
				AnimationQue.push(document.getElementById('AddAIButton').style.top = "0px");
				AnimationQue.push(document.getElementById('RemoveAIButton').style.top = "0px");
			}
			else {
				AnimationQue.push(document.getElementById('Status').innerHTML = "Waiting for the host to start the game. Please be patient");
			}
			AnimationDelay();
		}
		else if (eventSplit[0] === "Join") {
			PlayerCount += 1;
			AnimationQue.push(document.getElementById('ConnectingUser' + eventSplit[1]).innerHTML = eventSplit[2]);
			if (eventSplit[2].includes(" ")) {
				let nameSplit = eventSplit[2].split(" ");
				let nameReduced = nameSplit[0].substring(0, 12);
				if (nameSplit[0].length > 12) {
					nameReduced += "...";
				}
				AnimationQue.push(document.getElementById('PlayerName' + eventSplit[1]).innerHTML = nameReduced);
			}
			else {
				let nameReduced = eventSplit[2].substring(0, 12);
				if (eventSplit[2].length > 12) {
					nameReduced += "...";
				}
				AnimationQue.push(document.getElementById('PlayerName' + eventSplit[1]).innerHTML = nameReduced);
			}
			AnimationDelay();
			AudioJoin.play();
		}
		else if (eventSplit[0] === "Start") {
			Started = true;
			AnimationQue.push(document.getElementById('LobbyContainer').style.top = "2000px");
			AnimationQue.push(document.getElementById('Board').style.top = "0px");
			if (PlayerNumber === eventSplit[1]) {
				Roll = 1;
				Active = true;
				AnimationQue.push(document.getElementById('Roller').style.boxShadow = "0px 0px 15px #FFF");
				AnimationQue.push(document.getElementById('Status').innerHTML = "Your turn");
				AudioTurn.play();
			}
			else {
				Roll = 0;
				let name = document.getElementById('PlayerName' + eventSplit[1]).innerHTML;
				AnimationQue.push(document.getElementById('Status').innerHTML = "Player " + name + " plays first");
			}
			AnimationDelay();
		}
		else if (eventSplit[0] === "HostLeft") {
			for (let i = 0; i < 4; i++)
			{
				AnimationQue.push(document.getElementById('ConnectingUser' + i).innerHTML = "");
				AnimationQue.push(document.getElementById('PlayerName' + i).innerHTML = "");
			}
			AnimationQue.push(document.getElementById('Status').innerHTML = "Host has left the lobby. Exit this game and rejoin");
			AnimationDelay();
		}
		else if (eventSplit[0] === "Left") {
			PlayerCount -= 1;
			AnimationQue.push(document.getElementById('ConnectingUser' + eventSplit[1]).innerHTML = "");
			AnimationQue.push(document.getElementById('PlayerName' + eventSplit[1]).innerHTML = "");
			AnimationDelay();
			AudioLeave.play();
		}
		else if (eventSplit[0] === "Roll") {
			if (eventSplit[2] != PlayerNumber) {
				Roll += 1;
				for (let [key] of Object.entries(SquareList)) {
					if (document.getElementById(key).style.backgroundColor != "" && ClaimedSquares[key] == null) {
						AnimationQue.push(document.getElementById(key).style.backgroundColor = "");
					}
				}
				if (Roll < 4) {
					AnimationQue.push(document.getElementById('Status').innerHTML = "Player " + eventSplit[2] + " is on roll " + Roll);
				}
				let dice = eventSplit[1].split(",");
				for (let i = 0; i < 5; i++) {
					HeldDice[i] = dice[i];
				}
				for (let i = 5; i < 20; i++) {
					let die = dice[i] - 1;
					DiceFace[i - 5] = die;
				}
				Shuffle();
			}
		}
		else if (eventSplit[0] === "Claim") {
			if (PlayerNumber !== eventSplit[2]) {
				ClaimedSquares[eventSplit[1]] = eventSplit[2];
				if (eventSplit[2] === "1") {
					AnimationQue.push(document.getElementById(eventSplit[1]).style.backgroundColor = "#0000B3");
				}
				else if (eventSplit[2] === "2") {
					AnimationQue.push(document.getElementById(eventSplit[1]).style.backgroundColor = "#E60000");
				}
				else if (eventSplit[2] === "3") {
					AnimationQue.push(document.getElementById(eventSplit[1]).style.backgroundColor = "#00CC00");
				}
				else if (eventSplit[2] === "4") {
					AnimationQue.push(document.getElementById(eventSplit[1]).style.backgroundColor = "#E6E600");
				}
				HeldDice[0] = "false";
				HeldDice[1] = "false";
				HeldDice[2] = "false";
				HeldDice[3] = "false";
				HeldDice[4] = "false";
				for (let [key] of Object.entries(SquareList)) {
					if (document.getElementById(key).style.backgroundColor != "" && ClaimedSquares[key] == null) {
						AnimationQue.push(document.getElementById(key).style.backgroundColor = "");
					}
				}
			}
			if (eventSplit[4] === "false") {
				if (PlayerNumber === eventSplit[3]) {
					Roll = 1;
					Active = true;
					AnimationQue.push(document.getElementById('Roller').style.boxShadow = "0px 0px 15px #FFF");
					AnimationQue.push(document.getElementById('Status').innerHTML = "Your turn");
					AudioTurn.play();
					if (Object.entries(ClaimedSquares).length >= 23) {
						let squares = Object.keys(SquareList);
						let claims = Object.entries(ClaimedSquares);
						let stale = true;
						for (let j = 0; j < 81; j++) {
							let squareSplit = squares[j].split("_");
							let parsed1 = parseInt(squareSplit[0]);
							let parsed2 = parseInt(squareSplit[1]);
							for (let k = 0; k < 4; k++) {
								let position = parsed1 - k;
								let up = squares[j].replace(squareSplit[0] + "_", position + "_");
								if (squares.includes(up)) {
									if (claims[up] != null && claims[up] != PlayerNumber) {
										break;
									}
									else if (k === 3) {
										stale = false;
										break;
									}
								}
								else {
									break;
								}
							}
							if (stale) {
								for (let k = 0; k < 4; k++) {
									let position = parsed1 + k;
									let down = squares[j].replace(squareSplit[0] + "_", position + "_");
									if (squares.includes(down)) {
										if (claims[down] != null && claims[down] != PlayerNumber) {
											break;
										}
										else if (k === 3) {
											stale = false;
											break;
										}
									}
									else {
										break;
									}
								}
							}
							if (stale) {
								for (let k = 0; k < 4; k++) {
									let position = parsed2 - k;
									let left = squares[j].replace("_" + squareSplit[1], "_" + position);
									if (squares.includes(left)) {
										if (claims[left] != null && claims[left] != PlayerNumber) {
											break;
										}
										else if (k === 3) {
											stale = false;
											break;
										}
									}
									else {
										break;
									}
								}
							}
							if (stale) {
								for (let k = 0; k < 4; k++) {
									let position = parsed2 + k;
									let right = squares[j].replace("_" + squareSplit[1], "_" + position);
									if (squares.includes(right)) {
										if (claims[right] != null && claims[right] != PlayerNumber) {
											break;
										}
										else if (k === 3) {
											stale = false;
											break;
										}
									}
									else {
										break;
									}
								}
							}
						}
						if (stale) {
							alert("STALE MATE!")
						}
					}
				}
				else {
					Roll = 0;
					AnimationQue.push(document.getElementById("RollNumber").innerHTML = "");
					AnimationQue.push(document.getElementById('Status').innerHTML = "Player " + eventSplit[3] + " is starting their roll");
				}
			}
			else {
				StopUpdate();
				AnimationQue.push(document.getElementById("RollNumber").innerHTML = "");
				let name = document.getElementById('PlayerName' + eventSplit[2]).innerHTML;
				AnimationQue.push(document.getElementById('Status').innerHTML = name + " has won. Congratulations!");
				if (PlayerNumber === eventSplit[2]) {
					Balloons(eventSplit[2]);
					AudioWin.play();
				}
				else {
					AudioLost.play();
				}
			}
			AnimationDelay();
		}
		else if (eventSplit[0] === "Turn") {
			HeldDice[0] = "false";
			HeldDice[1] = "false";
			HeldDice[2] = "false";
			HeldDice[3] = "false";
			HeldDice[4] = "false";
			for (let [key, value] of Object.entries(SquareList)) {
				if (ClaimedSquares[key] == null) {
					AnimationQue.push(document.getElementById(key).style.backgroundColor = "");
				}
			}
			if (PlayerNumber == eventSplit[1]) {
				Roll = 1;
				Active = true;
				AnimationQue.push(document.getElementById('Roller').style.boxShadow = "0px 0px 15px #FFF");
				AnimationQue.push(document.getElementById('Status').innerHTML = "Your turn");
				AudioTurn.play();
				if (Object.entries(ClaimedSquares).length >= 24) {
					let squares = Object.keys(SquareList);
					let claims = Object.entries(ClaimedSquares);
					let stale = true;
					for (let j = 0; j < 81; j++) {
						let squareSplit = squares[j].split("_");
						let parsed1 = parseInt(squareSplit[0]);
						let parsed2 = parseInt(squareSplit[1]);
						for (let k = 1; k <= 4; k++) {
							let position = parsed1 - k;
							let up = clientClaims[j].replace(squareSplit[0] + "_", position + "_");
							if (squares.includes(up)) {
								if (claims[up] != null && claims[up] != PlayerNumber) {
									break;
								}
								else if (k === 4) {
									stale = false;
									break;
								}
							}
							else {
								break;
							}
						}
						if (stale) {
							for (let k = 1; k <= 4; k++) {
								let position = parsed1 + k;
								let down = clientClaims[j].replace(squareSplit[0] + "_", position + "_");
								if (squares.includes(down)) {
									if (claims[down] != null && claims[down] != PlayerNumber) {
										break;
									}
									else if (k === 4) {
										stale = false;
										break;
									}
								}
								else {
									break;
								}
							}
						}
						if (stale) {
							for (let k = 1; k <= 4; k++) {
								let position = parsed2 - k;
								let left = clientClaims[j].replace("_" + squareSplit[1], "_" + position);
								if (squares.includes(left)) {
									if (claims[left] != null && claims[left] != PlayerNumber) {
										break;
									}
									else if (k === 4) {
										stale = false;
										break;
									}
								}
								else {
									break;
								}
							}
						}
						if (stale) {
							for (let k = 1; k <= 4; k++) {
								let position = parsed2 + k;
								let right = clientClaims[j].replace("_" + squareSplit[1], "_" + position);
								if (squares.includes(right)) {
									if (claims[right] != null && claims[right] != PlayerNumber) {
										break;
									}
									else if (k === 4) {
										stale = false;
										break;
									}
								}
								else {
									break;
								}
							}
						}
					}
					if (stale) {
						alert("STALE MATE!")
					}
				}
			}
			else {
				Roll = 0;
				AnimationQue.push(document.getElementById("RollNumber").innerHTML = "");
				AnimationQue.push(document.getElementById('Status').innerHTML = "Player " + eventSplit[1] + " is starting their roll");
			}
			AnimationDelay();
		}
		else if (eventSplit[0] === "Win") {
			StopUpdate();
			AnimationQue.push(document.getElementById("RollNumber").innerHTML = "");
			let name = document.getElementById('PlayerName' + eventSplit[1]).innerHTML;
			AnimationQue.push(document.getElementById('Status').innerHTML = name + " has won. Congratulations!");
			if (PlayerNumber === eventSplit[1]) {
				Balloons(eventSplit[1]);
				AudioWin.play();
			}
			else {
				AudioLost.play();
			}
			AnimationDelay();
		}
		else if (eventSplit[0] === "AddAI") {
			AnimationQue.push(document.getElementById('PlayerName' + eventSplit[1]).innerHTML = "AI");
			AnimationQue.push(document.getElementById('ConnectingUser' + eventSplit[1]).innerHTML = "AI");
			AnimationDelay();
		}
		else if (eventSplit[0] === "RemoveAI") {
			AnimationQue.push(document.getElementById('PlayerName' + eventSplit[1]).innerHTML = "");
			AnimationQue.push(document.getElementById('ConnectingUser' + eventSplit[1]).innerHTML = "");
			AnimationDelay();
		}
	}
	else {
		setTimeout(function(){  
			RunEvent(newEvent, 1);
		}, 1000);
	}
};

function StartGame() {
	if (!Active && !Started && Host) {
		if (PlayerCount > 1) {
			let request = new XMLHttpRequest();
			request.open('POST', window.location.href.replace('rio.html', 'StartGameRio'), true);
			request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
			request.onerror = function() {
				StopUpdate();
				alert("StartGame Error: No response from server");
			};
			request.onload = function () {
				if (request.status === 200 && request.readyState === 4) {
					Started = true;
					Pin = CryptoJS.SHA512(ClientId + request.responseText).toString();
					AnimationQue.push(document.getElementById('StartButton').style.top = "2000px");
					AnimationQue.push(document.getElementById('AddAIButton').style.top = "2000px");
					AnimationQue.push(document.getElementById('RemoveAIButton').style.top = "2000px");
					AnimationDelay();
					StartUpdate();
				}
				else if (request.status === 401 && request.readyState === 4) {
					StopUpdate();
					alert("This security ID has expired. Please acquire a new one");
				}
				else if (request.status === 403 && request.readyState === 4) {
					StopUpdate();
					alert("StartGame: Server rejected request");
				}
			};
			request.send(Pin + "☼" + TableId);
		}
	}
};

function ExitGame() {
	let request = new XMLHttpRequest();
	request.open('POST', window.location.href.replace('rio.html', 'ExitRio'), true);
	request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
	request.onerror = function() {
		StopUpdate();
		alert("ExitGame Error: No response from server");
	};
	request.onload = function () {
		if (request.status === 200 && request.readyState === 4) {
			window.location.href = window.location.href;
		}
		else if (request.status === 401 && request.readyState === 4) {
			window.location.href = window.location.href;
		}
		else if (request.status === 403 && request.readyState === 4) {
			window.location.href = window.location.href;
		}
	};
	request.send(Pin + "☼" + TableId);
};

function HowToPlay() {
	if (!HTP) {
		HTP = true;
		AnimationQue.push(document.getElementById('HowToPlayContainer').style.top = "0px");
	}
	else {
		HTP = false;
		AnimationQue.push(document.getElementById('HowToPlayContainer').style.top = "2000px");
	}
	AnimationDelay();
};

function AddAI() {
	alert("Coming soon");
	//if (!Active && !Started && Host && PlayerCount < 4) {
	//	Active = true;
	//	let request = new XMLHttpRequest();
	//	request.open('POST', window.location.href.replace('rio.html', 'AddAIRio'), true);
	//	request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
	//	request.onerror = function() {
	//		Active = false;
	//		alert("AddAI Error: No response from server");
	//	};
	//	request.onload = function () {
	//		if (request.status === 200 && request.readyState === 4) {
	//			Active = false;
	//			AICount += 1;
	//			PlayerCount += 1;
	//		}
	//		else if (request.status === 401 && request.readyState === 4) {
	//			Active = false;
	//		}
	//		else if (request.status === 403 && request.readyState === 4) {
	//			Active = false;
	//		}
	//	};
	//	if (ClientId != "DBG1" && ClientId != "DBG2" && ClientId != "DBG3" && ClientId != "DBG4") {
	//		request.send(Pin + "☼" + TableId);
	//	}
	//	else {
	//		request.send(ClientId + "☼" + TableId);
	//	}
	//}
};

function RemoveAI() {
	alert("Coming soon");
	//if (!Active && !Started && Host && AICount > 0) {
	//	Active = true;
	//	let request = new XMLHttpRequest();
	//	request.open('POST', window.location.href.replace('rio.html', 'RemoveAIRio'), true);
	//	request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
	//	request.onerror = function() {
	//		Active = false;
	//		alert("RemoveAI Error: No response from server");
	//	};
	//	request.onload = function () {
	//		if (request.status === 200 && request.readyState === 4) {
	//			Active = false;
	//			AICount -= 1;
	//			PlayerCount -= 1;
	//		}
	//		else if (request.status === 401 && request.readyState === 4) {
	//			Active = false;
	//		}
	//		else if (request.status === 403 && request.readyState === 4) {
	//			Active = false;
	//		}
	//	};
	//	if (ClientId != "DBG1" && ClientId != "DBG2" && ClientId != "DBG3" && ClientId != "DBG4") {
	//		request.send(Pin + "☼" + TableId);
	//	}
	//	else {
	//		request.send(ClientId + "☼" + TableId);
	//	}
	//}
};

function Animate() {
	let length = AnimationQue.length;
	if (!Animating && length > 0) {
		Animating = true;
		for (let i = 0; i < length; i++) {
			if (AnimationQue[0] !== "balloons") {
				AnimationQue[0];
			}
			else {
				BalloonUp();
			}
			AnimationQue.splice(0, 1);
		}
	}
};

function StartObserver() {
	let body = document.getElementById('GeneralBody'),
	options = {
		childList: true,
		subtree: true
	},
	AnimationObserver = new MutationObserver(MutationCalled);
	AnimationObserver.observe(body, options);
};

function StopObserver() {
	AnimationObserver.disconnect();
};

function MutationCalled () {
	Animating = false;
	if (AnimationQue.length > 0) {
		AnimationDelay();
	}
};

function StartUpdate() {
	UpdateTimer = setInterval(RequestUpdate, 5000);
};

function StopUpdate() {
	clearInterval(UpdateTimer);
};

function BalloonAnimation() {
	BalloonTimer = setTimeout(BalloonUp, 10);
};

function AnimationDelay() {
	AnimationTimer = setTimeout(Animate, 1000);
};