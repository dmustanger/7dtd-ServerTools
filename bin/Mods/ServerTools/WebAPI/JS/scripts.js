var PlayerClock;
var ConsoleClock;
var ClientId;
var Pin;
var LastCommand = "";
var LineCount = 0;
var ConfigXml;

function FreshPage() {
	document.getElementById('BackgroundContainer').style.visibility = "hidden";
	document.getElementById('LoginContainer').style.visibility = "hidden";
	document.getElementById('PasswordContainer').style.visibility = "hidden";
	document.getElementById('LogoutContainer').style.visibility = "hidden";
	document.getElementById('SetPassButton').style.visibility = "hidden";
	document.getElementById('MenuContainer').style.visibility = "hidden";
	document.getElementById('NewPassButton').style.visibility = "hidden";
	document.getElementById('CancelButton').style.visibility = "hidden";
	document.getElementById('DisclaimerContainer').style.visibility = "visible";
};

function Cancel() {
	document.getElementById('CancelButton').style.visibility = "hidden";
	document.getElementById('SetPassButton').style.visibility = "hidden";
	document.getElementById("PasswordContainer").style.visibility = "hidden";
	document.getElementById("LogoutContainer").style.visibility = "visible";
	document.getElementById('NewPassButton').style.visibility = "visible";
	document.getElementById("Text3").value = "";
	document.getElementById("Text4").value = "";
	document.getElementById("NotBotBox2").checked = false;
};

function HomePage() {
	StopPlayerClock();
	StopConsoleClock();
	document.getElementById('BackgroundContainer').style.visibility = "hidden";
	document.getElementById("ConfigContainer").style.visibility = "hidden";
	document.getElementById("ConsoleContainer").style.visibility = "hidden";
	document.getElementById("ContactsContainer").style.visibility = "hidden";
	document.getElementById("DevsContainer").style.visibility = "hidden";
	document.getElementById("PlayersContainer").style.visibility = "hidden";
};

function PlayersPage() {
	StopPlayerClock();
	StopConsoleClock();
	let table = document.getElementById("PlayersBody");
	table.innerHTML = "";
	document.getElementById("PlayerCount").value = table.rows.length;
	document.getElementById('BackgroundContainer').style.visibility = "hidden";
	document.getElementById("ConfigContainer").style.visibility = "hidden";
	document.getElementById("ConsoleContainer").style.visibility = "hidden";
	document.getElementById("ContactsContainer").style.visibility = "hidden";
	document.getElementById("DevsContainer").style.visibility = "hidden";
	document.getElementById("PlayersContainer").style.visibility = "visible";
	Players();
	StartPlayerClock();
};

function ConfigPage() {
	StopPlayerClock();
	StopConsoleClock();
	document.getElementById('BackgroundContainer').style.visibility = "hidden";
	document.getElementById("ConsoleContainer").style.visibility = "hidden";
	document.getElementById("ContactsContainer").style.visibility = "hidden";
	document.getElementById("DevsContainer").style.visibility = "hidden";
	document.getElementById("PlayersContainer").style.visibility = "hidden";
	document.getElementById("ConfigContainer").style.visibility = "visible";
	document.getElementById("AcceptSave").checked = true;
	Config();
};

function DevPage() {
	StopPlayerClock();
	StopConsoleClock();
	document.getElementById("ConfigContainer").style.visibility = "hidden";
	document.getElementById("ConsoleContainer").style.visibility = "hidden";
	document.getElementById("ContactsContainer").style.visibility = "hidden";
	document.getElementById("PlayersContainer").style.visibility = "hidden";
	document.getElementById('BackgroundContainer').style.visibility = "visible";
	document.getElementById("DevsContainer").style.visibility = "visible";
};

function ConsolePage() {
	StopPlayerClock();
	StopConsoleClock();
	document.getElementById("ConfigContainer").style.visibility = "hidden";
	document.getElementById("ConsoleContainer").style.visibility = "visible";
	document.getElementById("PlayersContainer").style.visibility = "hidden";
	document.getElementById("DevsContainer").style.visibility = "hidden";
	document.getElementById('BackgroundContainer').style.visibility = "hidden";
	document.getElementById("ContactsContainer").style.visibility = "hidden";
	Console();
};

function ContactsPage() {
	StopPlayerClock();
	StopConsoleClock();
	document.getElementById("ConfigContainer").style.visibility = "hidden";
	document.getElementById("ConsoleContainer").style.visibility = "hidden";
	document.getElementById("PlayersContainer").style.visibility = "hidden";
	document.getElementById("DevsContainer").style.visibility = "hidden";
	document.getElementById('BackgroundContainer').style.visibility = "visible";
	document.getElementById("ContactsContainer").style.visibility = "visible";
};

function Accept() {
	document.getElementById("DisclaimerContainer").style.visibility = "hidden";
	document.getElementById('LoginContainer').style.visibility = "visible";
};

function NewPass() {
	document.getElementById("LogoutContainer").style.visibility = "hidden";
	document.getElementById('NewPassButton').style.visibility = "hidden";
	document.getElementById('CancelButton').style.visibility = "visible";
	document.getElementById('SetPassButton').style.visibility = "visible";
	document.getElementById("PasswordContainer").style.visibility = "visible";
};

function SignIn() {
	let text1 = document.getElementById("Text1").value;
	if (text1.length > 5 && text1.length < 31) {
		let text2 = document.getElementById("Text2").value;
		if (text2.length > 5 && text2.length < 31) {
			if (document.getElementById("NotBotBox1").checked) {
				let request = new XMLHttpRequest();
				request.open('POST', window.location.href.replace('st.html', 'SignIn'), true);
				request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
				request.onerror = function() {
					alert("Error");
				};
				request.onload = function () {
					if (request.status == 200 && request.readyState == 4) {
						ClientId = text1;
						Pin = text2 + request.responseText;
						document.getElementById('LoginContainer').style.visibility = "hidden";
						document.getElementById('LogoutContainer').style.visibility = "visible";
						document.getElementById('NewPassButton').style.visibility = "visible";
						document.getElementById("MenuContainer").style.visibility = "visible";
						document.getElementById("ClientId").innerHTML = text1;
						document.getElementById("NotBotBox1").checked = false;
						document.getElementById("Text1").value = "";
						document.getElementById("Text2").value = "";
					}
					else if (request.status == 401 && request.readyState == 4) {
						alert("Login failed. User or password was incorrect");
					}
					else if (request.status == 403 && request.readyState == 4) {
						alert("No response from the server. It may be restarting");
					}
				};
				request.send(text1 + "☼" + CryptoJS.SHA512(text2).toString());
			}
			else {
				alert("Are you a robot? Click the sign in checkbox first");
			}
		}
		else {
			alert("Invalid login password. Password must be 6 to 30 chars in length");
		}
	}
	else {
		alert("Invalid login id. Id must be 6 to 30 chars in length");
	}
};

function SignOut() {
	if (document.getElementById("ConfirmSignout").checked) {
		let request = new XMLHttpRequest();
		request.open('POST', window.location.href.replace('st.html', 'SignOut'), true);
		request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
		request.onerror = function() {
			alert("Error");
		};
		request.onload = function () {
			if (request.status == 200 && request.readyState == 4) {
				StopPlayerClock();
				StopConsoleClock();
				window.location.href = window.location.href;
			}
			else if (request.status == 403 && request.readyState == 4) {
				alert("No response from the server. It may be restarting");
			}
		};
		request.send(ClientId + "☼" + CryptoJS.SHA512(Pin).toString());
	}
	else {
		alert ("Are you a robot? Click the sign out checkbox first");
	}
};

function SetPass() {
	if (document.getElementById("Text3").value) {
		let pass1 = document.getElementById("Text3").value;
		if (pass1.length > 5 && pass1.length < 31) {
			if (document.getElementById("Text4").value) {
				let pass2 = document.getElementById("Text4").value;
				if (pass1 == pass2) {
					if (document.getElementById("NotBotBox2").checked) {
						let pinCut = Pin.substring(0, 4);
						let key = CryptoJS.enc.Utf8.parse(pinCut + pinCut + pinCut + pinCut);
						let iv = CryptoJS.enc.Utf8.parse(pinCut + pinCut + pinCut + pinCut);
						let encrypted = CryptoJS.AES.encrypt(CryptoJS.enc.Utf8.parse(pass1), key,
						{
							keySize: 128 / 8,
							iv: iv,
							mode: CryptoJS.mode.CBC,
							padding: CryptoJS.pad.Pkcs7
						});
						let request = new XMLHttpRequest();
						request.open('POST', window.location.href.replace('st.html', 'NewPass'), true);
						request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
						request.onerror = function() {
							alert("Error");
						};
						request.onload = function () {
							if (request.status == 200 && request.readyState == 4) {
								Pin = pass1 + request.responseText;
								document.getElementById('CancelButton').style.visibility = "hidden";
								document.getElementById('SetPassButton').style.visibility = "hidden";
								document.getElementById("PasswordContainer").style.visibility = "hidden";
								document.getElementById("LogoutContainer").style.visibility = "visible";
								document.getElementById('NewPassButton').style.visibility = "visible";
								document.getElementById("NotBotBox2").checked = false;
								document.getElementById("Text3").value = "";
								document.getElementById("Text4").value = "";
								alert("New password has been set");
							}
							else if (request.status == 403 && request.readyState == 4) {
								alert("No response from the server. It may be restarting");
							}
						};
						request.send(ClientId + "☼" + CryptoJS.SHA512(Pin).toString() + "☼" + encrypted);
					}
					else {
						alert("Are you a robot? Click the new password checkbox first");
					}
				}
				else {
					alert("Passwords do not match");
				}
			}
		}
		else {
			alert("Invalid password. Must be 6-30 characters with no spaces");
		}
	}
	else {
		alert("Invalid password. Password can not be blank");
	}
};

function Console() {
	let request = new XMLHttpRequest();
	request.open('POST', window.location.href.replace('st.html', 'Console'), true);
	request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
	request.onerror = function() {
		alert("Error");
	};
	request.onload = function () {
		if (request.status == 200 && request.readyState == 4) {
			let responseSplit = request.responseText.split('☼');
			LineCount = responseSplit[1];
			Pin += responseSplit[2];
			let log = document.getElementById('Console');
			log.textContent += responseSplit[0];
			log.scrollIntoView(false);
			StartConsoleClock();
		}
		else if (request.status == 403 && request.readyState == 4) {
			alert("No response from the server. It may be restarting");
		}
	};
	request.send(ClientId + "☼" + CryptoJS.SHA512(Pin).toString() + "☼" + LineCount);
};

function Command() {
	if (document.getElementById('ConsoleCommand').value != "") {
		let command = document.getElementById('ConsoleCommand').value;
		if (command.value == "/r") {
			command.value = LastCommand;
		}
		let request = new XMLHttpRequest();
		request.open('POST', window.location.href.replace('st.html', 'Command'), true);
		request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
		request.onerror = function() {
			alert("Error");
		};
		request.onload = function () {
			if (request.status == 200 && request.readyState == 4) {
				StopConsoleClock();
				let responseSplit = request.responseText.split('☼');
				LineCount = responseSplit[1];
				Pin += responseSplit[2];
				let log = document.getElementById('Console');
				log.textContent += responseSplit[0];
				log.scrollIntoView(false);
				StartConsoleClock();
				LastCommand = command.value;
				command.value = "";
			}
			else if (request.status == 402 && request.readyState == 4) {
				document.getElementById('ConsoleCommand').value = "";
				Pin += responseSplit[2];
				alert("Unknown command");
			}
			else if (request.status == 403 && request.readyState == 4) {
				alert("No response from the server. It may be restarting");
			}
		};
		request.send(ClientId + "☼" + CryptoJS.SHA512(Pin).toString() + "☼" + LineCount + "☼" + command);
	}
};

function Players() {
	let request = new XMLHttpRequest();
	request.open('POST', window.location.href.replace('st.html', 'Players'), true);
	request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
	request.onerror = function() {
		alert("Error");
	};
	request.onload = function () {
		if (request.status == 200 && request.readyState == 4) {
			if (request.responseText.includes("╚") == true) {
				let responseSplit = request.responseText.split('╚');
				Pin += responseSplit[1];
				if (responseSplit[0].includes("☼") == true) {
					let responseSubSplit = responseSplit[0].split('☼');
					let table = document.getElementById("PlayersBody");
					table.innerHTML = "";
					for (let i = 0; i < responseSubSplit.length; i++) {
						let playerData = responseSubSplit[i].split('§');
						let rows = table.insertRow(-1);
						let cell0 = rows.insertCell(-1);
						cell0.onclick = function() {
							document.getElementById('PlayerId').value = playerData[0];
						};
						let cell1 = rows.insertCell(-1);
						let cell2 = rows.insertCell(-1);
						let cell3 = rows.insertCell(-1);
						let cell4 = rows.insertCell(-1);
						let cell5 = rows.insertCell(-1);
						cell0.innerHTML = playerData[0];
						cell1.innerHTML = playerData[1];
						cell2.innerHTML = playerData[2];
						cell3.innerHTML = playerData[3];
						cell4.innerHTML = playerData[4];
						cell5.innerHTML = playerData[5];
					}
				}
				else {
					let playerData = responseSplit[0].split('§');
					let table = document.getElementById("PlayersBody");
					table.innerHTML = "";
					let rows = table.insertRow(-1);
					let cell0 = rows.insertCell(-1);
					cell0.onclick = function() {
						document.getElementById('PlayerId').value = playerData[0];
					};
					let cell1 = rows.insertCell(-1);
					let cell2 = rows.insertCell(-1);
					let cell3 = rows.insertCell(-1);
					let cell4 = rows.insertCell(-1);
					let cell5 = rows.insertCell(-1);
					cell0.innerHTML = playerData[0];
					cell1.innerHTML = playerData[1];
					cell2.innerHTML = playerData[2];
					cell3.innerHTML = playerData[3];
					cell4.innerHTML = playerData[4];
					cell5.innerHTML = playerData[5];
				}
				document.getElementById("PlayerCount").value = table.rows.length;
			}
			else {
				Pin += request.responseText;
				let table = document.getElementById("PlayersBody");
				table.innerHTML = "";
				document.getElementById("PlayerCount").value = table.rows.length;
			}
		}
		else if (request.status == 403 && request.readyState == 4) {
			alert("No response from the server. It may be restarting");
		}
	};
	request.send(ClientId + "☼" + CryptoJS.SHA512(Pin).toString());
}

function ConfigPage() {
	if (document.getElementById('SaveButton').checked) {
		document.getElementById('SaveButton').checked = false;
	}
	document.getElementById('BackgroundContainer').style.visibility = "hidden";
	document.getElementById("ConfigContainer").style.visibility = "visible";
	document.getElementById("ContactsContainer").style.visibility = "hidden";
	document.getElementById("DevsContainer").style.visibility = "hidden";
	document.getElementById("PlayersContainer").style.visibility = "hidden";
	document.getElementById("ConsoleContainer").style.visibility = "hidden";
	Config();
}

function Config() {
	let request1 = new XMLHttpRequest();
	request1.open('POST', window.location.href.replace('st.html', 'Config'), true);
	request1.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
	request1.onerror = function() {
		alert("Error");
	};
	request1.onload = function () {
		if (request1.status == 200 && request1.readyState == 4) {
			Pin += request1.responseText;
			let request2 = new XMLHttpRequest();
			request2.open('GET', window.location.href.replace('st.html', 'Config'), true);
			request2.setRequestHeader('Content-Type', 'text/xml; charset=utf-8');
			request2.onerror = function() {
				alert("Error");
			};
			request2.onload = function () {
				if (request2.status == 200 && request2.readyState == 4) {
					ConfigXml = request2.responseXML;
					let root = ConfigXml.documentElement;
					if (root != null) {
						let table = document.getElementById("ConfigBody");
						table.innerHTML = "";
						let childNodes = root.getElementsByTagName("Tool");
						if (childNodes != null && childNodes.length > 0) {
							let count = 0;
							for (i = 0; i < childNodes.length; i++) {
								let attributes = childNodes[i].attributes;
								if (attributes != null) {
									let newRow = table.insertRow(-1);
									for (j = 0; j < attributes.length; j++) {
										let newCell = newRow.insertCell(-1);
										if (j === 0) {
											newCell.innerHTML = attributes[j].value;
											newCell.setAttribute("name", "title");
											newCell.setAttribute("toolName", attributes[j].value);
											newCell.setAttribute("style", "border: 2px solid #000;");
										}
										else if (attributes[j].value.toLowerCase() == "true" || attributes[j].value.toLowerCase() == "false") {
											newCell.innerHTML = attributes[j].name + " ";
											newCell.setAttribute("name", "box");
											newCell.setAttribute("optionName", attributes[j].name);
											newCell.setAttribute("value", attributes[j].value);
											let checkBox = document.createElement("input");
											checkBox.setAttribute("type", "checkbox");
											checkBox.setAttribute("value", attributes[j].value);
											checkBox.setAttribute("id", count);
											if (attributes[j].value.toLowerCase() === "true") {
												checkBox.checked = true;
											}
											else {
												checkBox.checked = false;
											}
											newCell.appendChild(checkBox);
										}
										else {
											newCell.innerHTML = attributes[j].name + " ";
											newCell.setAttribute("name", "text");
											newCell.setAttribute("optionName", attributes[j].name);
											newCell.setAttribute("value", attributes[j].value);
											let textBox = document.createElement("input");
											textBox.setAttribute("type", "text");
											textBox.setAttribute("value", attributes[j].value);
											textBox.setAttribute("id", count);
											newCell.appendChild(textBox);
										}
										count++;
									}
								}
							}
						}
					}
				}
				else if (request2.status == 403 && request2.readyState == 4) {
					alert("Code02: No response from the server. It may be restarting");
				}
			};
			request2.send("");
		}
		else if (request1.status == 403 && request1.readyState == 4) {
			alert("Code01: No response from the server. It may be restarting");
		}
	};
	request1.send(ClientId + "☼" + CryptoJS.SHA512(Pin).toString());
};

function SaveConfig() {
	if (document.getElementById("AcceptSave").checked) {
		document.getElementById("AcceptSave").checked = false;
		if (ConfigXml.documentElement != null) {
			let tableRows = document.getElementById("ConfigBody").rows;
			let count = 0;
			let newconfig = ""
			for (i = 0; i < tableRows.length; i++) {
				let cells = tableRows[i].cells;
				for (j = 0; j < cells.length; j++) {
					if (j == 0) {
						newconfig += cells[j].attributes[1].value + "§";
					}
					if (cells[j].attributes[0].value != "title") {
						if (cells[j].attributes[0].value == "box") {
							var checkBox = document.getElementById(count);
							if (checkBox.checked) {
								newconfig += cells[j].attributes[1].value + "σ";
								if (j == cells.length - 1) {
									newconfig += "True" + "☼";
								}
								else {
									newconfig += "True" + "╚";
								}
							}
							else {
								newconfig += cells[j].attributes[1].value + "σ";
								if (j == cells.length - 1) {
									newconfig += "False" + "☼";
								}
								else {
									newconfig += "False" + "╚";
								}
							}
						}
						else {
							var textBox = document.getElementById(_count);
							newconfig += cells[j].attributes[1].value + "σ";
							if (j == cells.length - 1) {
								newconfig += textBox.value + "☼";
							}
							else {
								newconfig += textBox.value + "╚";
							}
						}
					}
					count++;
				}
			}
			let request = new XMLHttpRequest();
			request.open('POST', window.location.href.replace('st.html', 'SaveConfig'), true);
			request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
			request.onerror = function() {
				alert("Error");
			};
			request.onload = function () {
				if (request.status == 200 && request.readyState == 4) {
					Pin += request.responseText;
					alert("ServerToolsConfig.xml successfully updated");
				}
				else if (request.status == 402 && request.readyState == 4) {
					alert("Unable to update ServerToolsConfig.xml. Server refused");
				}
				else if (request.status == 403 && request.readyState == 4) {
					alert("No response from the server. It may be restarting");
				}
			};
			request.send(ClientId + "☼" + CryptoJS.SHA512(Pin).toString() + "☼" + newconfig);
		}
	}
	else {
		alert("Are you a robot? Click the config checkbox first");
	}
};

function Kick() {
	let id = document.getElementById("PlayerId").value;
	let request = new XMLHttpRequest();
	request.open('POST', window.location.href.replace('st.html', 'Kick'), true);
	request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
	request.onerror = function() {
		alert("Error");
	};
	request.onload = function () {
		if (request.status == 200 && request.readyState == 4) {
			Pin += request.responseText;
			alert(id + " has been kicked");
		}
		else if (request.status == 402 && request.readyState == 4) {
			Pin += request.responseText;
			alert(id + " was not found online. Unable to kick user");
		}
		else if (request.status == 403 && request.readyState == 4) {
			alert("No response from the server. It may be restarting");
		}
	};
	request.send(ClientId + "☼" + CryptoJS.SHA512(Pin).toString() + "☼" + id);
};

function Ban() {
	let id = document.getElementById("PlayerId").value;
	let request = new XMLHttpRequest();
	request.open('POST', window.location.href.replace('st.html', 'Ban'), true);
	request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
	request.onerror = function() {
		alert("Error");
	};
	request.onload = function () {
		if (request.status == 200 && request.readyState == 4) {
			Pin += request.responseText;
			alert(id + " has been banned");
		}
		else if (request.status == 403 && request.readyState == 4) {
			alert("No response from the server. It may be restarting");
		}
	};
	request.send(ClientId + "☼" + CryptoJS.SHA512(Pin).toString() + "☼" + id);
};

function Mute() {
	let id = document.getElementById("PlayerId").value;
	let request = new XMLHttpRequest();
	request.open('POST', window.location.href.replace('st.html', 'Mute'), true);
	request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
	request.onerror = function() {
		alert("Error");
	};
	request.onload = function () {
		if (request.status == 200 && request.readyState == 4) {
			Pin += request.responseText;
			alert(id + " has been muted");
		}
		else if (request.status == 202 && request.readyState == 4) {
			Pin += request.responseText;
			alert(id + " has been unmuted");
		}
		else if (request.status == 402 && request.readyState == 4) {
			Pin += request.responseText;
			alert("Mute tool is not enabled. Unable to mute this id");
		}
		else if (request.status == 403 && request.readyState == 4) {
			alert("No response from the server. It may be restarting");
		}
	};
	request.send(ClientId + "☼" + CryptoJS.SHA512(Pin).toString() + "☼" + id);
};

function Jail() {
	let id = document.getElementById("PlayersSteamId").value;
	let request = new XMLHttpRequest();
	request.open('POST', window.location.href.replace('st.html', 'Jail'), true);
	request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
	request.onerror = function() {
		alert("Error");
	};
	request.onload = function () {
		if (request.status == 200 && request.readyState == 4) {
			Pin += request.responseText;
			alert(id + " has been jailed");
		}
		else if (request.status == 202 && request.readyState == 4) {
			Pin += request.responseText;
			alert(id + " has been unjailed");
		}
		else if (request.status == 402 && request.readyState == 4) {
			Pin += request.responseText;
			alert("Jail tool is not enabled. Unable to jail this id");
		}
		else if (request.status == 403 && request.readyState == 4) {
			alert("No response from the server. It may be restarting");
		}
	};
	request.send(ClientId + "☼" + CryptoJS.SHA512(Pin).toString() + "☼" + id);
};

function Reward() {
	let id = document.getElementById("PlayerId").value;
	let request = new XMLHttpRequest();
	request.open('POST', window.location.href.replace('st.html', 'Reward'), true);
	request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
	request.onerror = function() {
		alert("Error");
	};
	request.onload = function () {
		if (request.status == 200 && request.readyState == 4) {
			Pin += request.responseText;
			alert(id + " has received a vote reward");
		}
		else if (request.status == 202 && request.readyState == 4) {
			Pin += request.responseText;
			alert(id + " was not found online. No reward given");
		}
		else if (request.status == 402 && request.readyState == 4) {
			Pin += request.responseText;
			alert("Vote reward tool is not enabled");
		}
		else if (request.status == 403 && request.readyState == 4) {
			alert("No response from the server. It may be restarting");
		}
	};
	request.send(ClientId + "☼" + CryptoJS.SHA512(Pin).toString() + "☼" + id);
};

function StartPlayerClock() {
	PlayerClock = setInterval(Players, 10000);
};

function StopPlayerClock() {
	clearTimeout(PlayerClock);
};

function StartConsoleClock() {
	ConsoleClock = setInterval(Console, 10000);
};

function StopConsoleClock() {
	clearTimeout(ConsoleClock);
};
