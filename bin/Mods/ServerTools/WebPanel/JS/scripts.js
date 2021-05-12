var playerClock;
var consoleClock;
var clientId = "";
var clientSessionId = "";
var serverKeys = "";
var numSet = "4829017536";
var sessionSet = "jJk9Kl3wWxXAbyYz0ZLmMn5NoO6dDe1EfFpPqQrRsStaBc2CgGhH7iITu4U8vV";
var lastCommand = "";
var lineCount = 0;

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
	SetClientId();
	SetSessionId();
	Handshake();
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
	let _table = document.getElementById("PlayersBody");
	_table.innerHTML = "";
	document.getElementById("PlayerCount").value = _table.rows.length;
	document.getElementById('BackgroundContainer').style.visibility = "hidden";
	document.getElementById("ConfigContainer").style.visibility = "hidden";
	document.getElementById("ConsoleContainer").style.visibility = "hidden";
	document.getElementById("ContactsContainer").style.visibility = "hidden";
	document.getElementById("DevsContainer").style.visibility = "hidden";
	document.getElementById("PlayersContainer").style.visibility = "visible";
	UpdatePlayers();
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

function SetClientId() {
	clientId = "";
	for (i = 0; i < 8; i++)
	{
		clientId += numSet.charAt(Math.floor(Math.random() * 10));
	}
};

function SetSessionId() {
	clientSessionId = "";
	for (i = 0; i < 20; i++)
	{
		clientSessionId += sessionSet.charAt(Math.floor(Math.random() * 62));
	}
};

function Handshake() {
	let _handShake = clientId;
	let _uri = window.location.href;
	let _request = new XMLHttpRequest();
	_request.open('POST', window.location.href.replace('st.html', 'Handshake'), false);
	_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
	_request.onerror = function() {
		alert("Request failed. Server may be restarting");
	};
	_request.onload = function () {
		if (_request.status === 200 && _request.readyState === 4 && _request.responseText.length === 2000) {
			let _serverResponse = _request.responseText;
			if (_serverResponse.substring(0, 8) === clientId) {
				serverKeys = _serverResponse.substring(8);
				let _fistbump = Encrypt(clientSessionId);
				let _request2 = new XMLHttpRequest();
				_request2.open('POST', window.location.href.replace('st.html', 'Secure'), false);
				_request2.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
				_request2.onerror = function() {
					alert("Request failed. Server may be restarting");
				};
				_request2.onload = function () {
					if (_request2.status === 200 && _request2.readyState === 4 && _request2.responseText.length === 2000) {
						if (_request2.responseText.substring(0, 8) == clientId) {
							serverKeys = _request2.responseText.substring(8);
						}
					}
					else if (_request.status === 401) {
						window.location.href = _uri;
					}
				};
				_request2.send(_fistbump);
			}
			else {
				window.location.href = _uri;
			}
		}
		else if (_request.status === 401) {
			window.location.href = _uri;
		}
	};
	_request.send(_handShake);
};

function Accept() {
	document.getElementById("DisclaimerContainer").style.visibility = "hidden";
	document.getElementById('LoginContainer').style.visibility = "visible";
};

function SignIn() {
	if (document.getElementById("Text1").value) {
		let _text1 = document.getElementById("Text1").value;
		if (_text1.length === 17) {
			if (document.getElementById("Text2").value) {
				let _text2 = document.getElementById("Text2").value;
				if (document.getElementById("NotBotBox1").checked) {
					let _encrypted = Encrypt(clientSessionId + "`" + _text1 + "`" + _text2);
					let _uri = window.location.href;
					let _request = new XMLHttpRequest();
					_request.open('POST', window.location.href.replace('st.html', 'SignIn'), false);
					_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
					_request.onerror = function() {
						alert("Request failed. Server may be restarting");
					};
					_request.onload = function () {
						if (_request.status === 200 && _request.readyState === 4 && _request.responseText.length === 2000) {
						let _serverResponse = _request.responseText;
							if (_serverResponse.substring(0, 8) === clientId) {
								serverKeys = _serverResponse.substring(8);
								document.getElementById('LoginContainer').style.visibility = "hidden";
								document.getElementById('LogoutContainer').style.visibility = "visible";
								document.getElementById('NewPassButton').style.visibility = "visible";
								document.getElementById("MenuContainer").style.visibility = "visible";
								document.getElementById("ClientId").innerHTML = _text1;
								document.getElementById("NotBotBox1").checked = false;
								document.getElementById("Text1").value = "";
								document.getElementById("Text2").value = "";
							}
						}
						else if (_request.status === 401 && _request.readyState === 4 && _request.responseText.length === 2000) {
							let _serverResponse = _request.responseText;
							if (_serverResponse.substring(0, 8) === clientId) {
								serverKeys = _serverResponse.substring(8);
								alert("Authorization failed");
							}
						}
					};
					_request.send(_encrypted);
				}
				else {
					alert("Are you a robot? Click the sign in checkbox first");
				}
			}
			else {
				alert("Invalid Password. Password can not be blank");
			}
		}
		else {
			alert("Invalid Client. Id must be 17 chars in length");
		}
	}
	else {
		alert("Invalid Client. Id can not be blank");
	}
};

function SignOut() {
	if (document.getElementById("ConfirmSignout").checked) {
		let _encrypted = Encrypt(clientSessionId);
		let _uri = window.location.href;
		let _request = new XMLHttpRequest();
		_request.open('POST', window.location.href.replace('st.html', 'SignOut'), false);
		_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
		_request.onerror = function() {
			alert("Request failed. Server may be restarting");
		};
		_request.onload = function () {
			if ((_request.status === 200 || _request.status === 401) && _request.readyState === 4) {
				window.location.href = _uri;
			}
		};
		_request.send(_encrypted);
	}
	else {
		alert ("Are you a robot? Click the sign out checkbox first");
	}
};

function NewPass() {
	let _encrypted = Encrypt(clientSessionId);
	let _uri = window.location.href;
	let _request = new XMLHttpRequest();
	_request.open('POST', window.location.href.replace('st.html', 'NewPass'), false);
	_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
	_request.onerror = function() {
		alert("Request failed. Server may be restarting");
	};
	_request.onload = function () {
		if (_request.status === 200 && _request.readyState === 4 && _request.responseText.length === 2000) {
			let _serverResponse = _request.responseText;
			if (_serverResponse.substring(0, 8) === clientId) {
				serverKeys = _serverResponse.substring(8);
				document.getElementById("LogoutContainer").style.visibility = "hidden";
				document.getElementById('NewPassButton').style.visibility = "hidden";
				document.getElementById('CancelButton').style.visibility = "visible";
				document.getElementById('SetPassButton').style.visibility = "visible";
				document.getElementById("PasswordContainer").style.visibility = "visible";
			}
		}
		else if (_request.status === 401) {
			window.location.href = _uri;
		}
	};
	_request.send(_encrypted);
};

function SetPass() {
	if (document.getElementById("Text3").value) {
		let _password = document.getElementById("Text3").value;
		if (_password.length > 5 && _password.length < 21) {
			if (document.getElementById("Text4").value) {
				let _confirm = document.getElementById("Text4").value;
				if (_password === _confirm) {
					if (document.getElementById("NotBotBox2").checked) {
						let _encrypted = Encrypt(clientSessionId + "`" + _password);
						let _uri = window.location.href;
						let _request = new XMLHttpRequest();
						_request.open('POST', window.location.href.replace('st.html', 'SetPass'), false);
						_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
						_request.onerror = function() {
							alert("Request failed. Server may be restarting");
						};
						_request.onload = function () {
							if (_request.status === 200 && _request.readyState === 4 && _request.responseText.length === 2000) {
								let _serverResponse = _request.responseText;
								if (_serverResponse.substring(0, 8) === clientId) {
									serverKeys = _serverResponse.substring(8);
									document.getElementById('CancelButton').style.visibility = "hidden";
									document.getElementById('SetPassButton').style.visibility = "hidden";
									document.getElementById("PasswordContainer").style.visibility = "hidden";
									document.getElementById("LogoutContainer").style.visibility = "visible";
									document.getElementById('NewPassButton').style.visibility = "visible";
									document.getElementById("NotBotBox2").checked = false;
									document.getElementById("Text3").value = "";
									document.getElementById("Text4").value = "";
								}
							}
							else if (_request.status === 401) {
								window.location.href = _uri;
							}
						};
						_request.send(_encrypted);
					}
					else {
						alert("Are you a robot? Click the new password checkbox first");
					}
				}
				else {
					alert("Passwords do not match");
				}
			}
			else {
				alert("Passwords do not match");
			}
		}
		else {
			alert("Invalid password. Must be 6-20 characters with no spaces");
		}
	}
	else {
		alert("Invalid password. Password can not be blank");
	}
};

function ConsolePage() {
	StopPlayerClock();
	StopConsoleClock();
	let _encrypted = Encrypt(clientSessionId);
	let _uri = window.location.href;
	let _request = new XMLHttpRequest();
	_request.open('POST', window.location.href.replace('st.html', 'Console'), false);
	_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
	_request.onerror = function() {
		alert("Request failed. Server may be restarting");
	};
	_request.onload = function () {
		if (_request.status === 200 && _request.readyState === 4 && _request.responseText.length === 2000) {
			document.getElementById('BackgroundContainer').style.visibility = "hidden";
			document.getElementById("ConfigContainer").style.visibility = "hidden";
			document.getElementById("ContactsContainer").style.visibility = "hidden";
			document.getElementById("DevsContainer").style.visibility = "hidden";
			document.getElementById("PlayersContainer").style.visibility = "hidden";
			document.getElementById("ConsoleContainer").style.visibility = "visible";
			let _serverResponse = _request.responseText;
			if (_serverResponse.substring(0, 8) === clientId) {
				serverKeys = _serverResponse.substring(8);
			}
			let _request2 = new XMLHttpRequest();
			_request2.open('POST', window.location.href.replace('st.html', 'Console'), false);
			_request2.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
			_request2.onerror = function() {
				alert("Request failed. Server may be restarting");
			};
			_request2.onload = function () {
				if (_request2.status === 200 && _request2.readyState === 4) {
					if (_request2.responseText.length > 0) {
						let _linesAndCount = _request2.responseText.split('☼');
						lineCount = _linesAndCount[1];
						let _log = document.getElementById('Console');
						_log.textContent += _linesAndCount[0];
						_log.scrollIntoView(false);
					}
					StartConsoleClock();
				}
				else if (_request2.status === 401) {
					window.location.href = _uri;
				}
			};
			_request2.send(clientId + lineCount);
		}
		else if (_request.status === 401) {
			window.location.href = _uri;
		}
	};
	_request.send(_encrypted);
};

function UpdateConsole() {
	let _encrypted = Encrypt(clientSessionId);
	let _uri = window.location.href;
	let _request = new XMLHttpRequest();
	_request.open('POST', window.location.href.replace('st.html', 'Console'), false);
	_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
	_request.onerror = function() {
		alert("Request failed. Server may be restarting");
	};
	_request.onload = function () {
		if (_request.status === 200 && _request.readyState === 4 && _request.responseText.length === 2000) {
			let _serverResponse = _request.responseText;
			if (_serverResponse.substring(0, 8) === clientId) {
				serverKeys = _serverResponse.substring(8);
			}
			let _request2 = new XMLHttpRequest();
			_request2.open('POST', window.location.href.replace('st.html', 'Console'), false);
			_request2.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
			_request2.onerror = function() {
				alert("Request failed. Server may be restarting");
			};
			_request2.onload = function () {
				if (_request2.status === 200 && _request2.readyState === 4) {
					if (_request2.responseText.length > 0) {
						let _linesAndCount = _request2.responseText.split('☼');
						lineCount = _linesAndCount[1];
						let _log = document.getElementById('Console');
						_log.textContent += _linesAndCount[0];
						_log.scrollIntoView(false);
					}
				}
				else if (_request2.status === 401) {
					window.location.href = _uri;
				}
			};
			_request2.send(clientId + lineCount);
		}
		else if (_request.status === 401) {
			window.location.href = _uri;
		}
	};
	_request.send(_encrypted);
};

function Command() {
	if (document.getElementById('ConsoleCommand').value !== "") {
		let _encrypted = Encrypt(clientSessionId);
		let _uri = window.location.href;
		let _request = new XMLHttpRequest();
		_request.open('POST', window.location.href.replace('st.html', 'Command'), false);
		_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
		_request.onerror = function() {
			alert("Request failed. Server may be restarting");
		};
		_request.onload = function () {
			if (_request.status === 200 && _request.readyState === 4 && _request.responseText.length === 2000) {
				let _serverResponse = _request.responseText;
				if (_serverResponse.substring(0, 8) === clientId) {
					serverKeys = _serverResponse.substring(8);
				}
				let _command = document.getElementById('ConsoleCommand');
				if (_command.value === "/r") {
					_command.value = lastCommand;
				}
				let _request2 = new XMLHttpRequest();
				_request2.open('POST', window.location.href.replace('st.html', 'Command'), false);
				_request2.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
				_request2.onerror = function() {
					alert("Request failed. Server may be restarting");
				};
				_request2.onload = function () {
					if (_request2.status === 200 && _request2.readyState === 4) {
						StopConsoleClock();
						let _linesAndCount = _request2.responseText.split('☼');
						lineCount = _linesAndCount[1];
						let _log = document.getElementById('Console');
						_log.appendChild(document.createTextNode(_linesAndCount[0]));
						_log.scrollIntoView(false);
						StartConsoleClock();
					}
					else if (_request2.status === 400 && _request2.readyState === 4) {
						alert(_request2.responseText);
					}
					else if (_request2.status === 406 && _request2.readyState === 4) {
						alert(_request2.responseText);
					}
				};
				_request2.send(clientId + "`" + lineCount + "`" + _command.value);
				lastCommand = _command.value;
				_command.value = "";
			}
			else if (_request.status === 401) {
				window.location.href = _uri;
			}
		};
		_request.send(_encrypted);
	}
};

function UpdatePlayers() {
	let _encrypted = Encrypt(clientSessionId);
	let _uri = window.location.href;
	let _request = new XMLHttpRequest();
	_request.open('POST', window.location.href.replace('st.html', 'Players'), false);
	_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
	_request.onerror = function() {
		alert("Request failed. Server may be restarting");
	};
	_request.onload = function () {
		if (_request.status === 200 && _request.readyState === 4 && _request.responseText.length === 2000) {
			let _serverResponse = _request.responseText;
			if (_serverResponse.substring(0, 8) === clientId) {
				serverKeys = _serverResponse.substring(8);
			}
			let _request2 = new XMLHttpRequest();
			_request2.open('POST', window.location.href.replace('st.html', 'Players'), false);
			_request2.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
			_request2.onerror = function() {
				alert("Request failed. Server may be restarting");
			};
			_request2.onload = function () {
				if (_request2.status === 200 && _request2.readyState === 4) {
					let _table = document.getElementById("PlayersBody");
					_table.innerHTML = "";
					if (_request2.responseText != "") {
						let _count = 0;
						if (_request2.responseText.includes('☼')) {
							let _serverResponseSplit1 = _request2.responseText.split('☼');
							let _serverResponseSplit1Length = _serverResponseSplit1.length;
							for (let j = 0; j < _serverResponseSplit1Length; j++) {
								let _serverResponseSplit2 = _serverResponseSplit1[j].split('§');
								let _rows = _table.insertRow(-1);
								let _cell0 = _rows.insertCell(-1);
								_cell0.onclick = function() {
									let _idSplit = _cell0.innerHTML.split('/');
									let _idField = document.getElementById('PlayersSteamId');
									_idField.value = _idSplit[0];
								};
								let _cell1 = _rows.insertCell(-1);
								let _cell2 = _rows.insertCell(-1);
								let _cell3 = _rows.insertCell(-1);
								let _cell4 = _rows.insertCell(-1);
								let _cell5 = _rows.insertCell(-1);
								_cell0.innerHTML = _serverResponseSplit2[0];
								_cell1.innerHTML = _serverResponseSplit2[1];
								_cell2.innerHTML = _serverResponseSplit2[2];
								_cell3.innerHTML = _serverResponseSplit2[3];
								_cell4.innerHTML = _serverResponseSplit2[4];
								_cell5.innerHTML = _serverResponseSplit2[5];
								_count++;
							}
						}
						else {
							let _serverResponseSplit1 = _request2.responseText.split('§');
							let _rows = _table.insertRow(-1);
							let _cell0 = _rows.insertCell(-1);
							_cell0.onclick = function() {
								let _idSplit = _cell0.innerHTML.split('/');
								let _idField = document.getElementById('PlayersSteamId');
								_idField.value = _idSplit[0];
							};
							let _cell1 = _rows.insertCell(-1);
							let _cell2 = _rows.insertCell(-1);
							let _cell3 = _rows.insertCell(-1);
							let _cell4 = _rows.insertCell(-1);
							let _cell5 = _rows.insertCell(-1);
							_cell0.innerHTML = _serverResponseSplit1[0];
							_cell1.innerHTML = _serverResponseSplit1[1];
							_cell2.innerHTML = _serverResponseSplit1[2];
							_cell3.innerHTML = _serverResponseSplit1[3];
							_cell4.innerHTML = _serverResponseSplit1[4];
							_cell5.innerHTML = _serverResponseSplit1[5];
							_count++;
							
						}
					}
					document.getElementById("PlayerCount").value = _table.rows.length;
				}
				else if (_request2.status === 401) {
					window.location.href = _uri;
				}
			};
			_request2.send(clientId);
		}
		else if (_request.status === 401) {
			window.location.href = _uri;
		}
	};
	_request.send(_encrypted);
};

function Config() {
	if (document.getElementById("AcceptSave").checked) {
		document.getElementById("AcceptSave").checked = false;
		let _encrypted = Encrypt(clientSessionId);
		let _uri = window.location.href;
		let _request = new XMLHttpRequest();
		_request.open('POST', window.location.href.replace('st.html', 'Config'), false);
		_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
		_request.onerror = function() {
			alert("Request failed. Server may be restarting");
		};
		_request.onload = function () {
			if (_request.status === 200 && _request.readyState === 4 && _request.responseText.length === 2000) {
				let _serverResponse = _request.responseText;
				if (_serverResponse.substring(0, 8) === clientId) {
					serverKeys = _serverResponse.substring(8);
				}
				let _request2 = new XMLHttpRequest();
				_request2.open('GET', window.location.href.replace('st.html', 'Config'), false);
				_request2.setRequestHeader('Content-Type', 'text/xml; charset=utf-8');
				_request2.onerror = function() {
					alert("Request failed. Server may be restarting");
				};
				_request2.onload = function () {
					if (_request2.status === 200 && _request2.readyState === 4) {
						if (_request2.responseXML != null) {
							_configXml = _request2.responseXML;
							let _root = _configXml.documentElement;
							if (_root != null) {
								let _table = document.getElementById("ConfigBody");
								if (_table != null) {
									_table.innerHTML = "";
									let _childNodes = _root.getElementsByTagName("Tool");
									if (_childNodes != null && _childNodes.length > 0) {
										let _count = 0;
										for (i = 0; i < _childNodes.length; i++) {
											let _attributes = _childNodes[i].attributes;
											if (_attributes != null) {
												let _newRow = _table.insertRow(-1);
												for (j = 0; j < _attributes.length; j++) {
													let _newCell = _newRow.insertCell(-1);
													if (j === 0) {
														_newCell.innerHTML = _attributes[j].value;
														_newCell.setAttribute("name", "title");
														_newCell.setAttribute("toolName", _attributes[j].value);
														_newCell.setAttribute("style", "border: 2px solid #000;");
													}
													else if (_attributes[j].value.toLowerCase() === "true" || _attributes[j].value.toLowerCase() === "false") {
														_newCell.innerHTML = _attributes[j].name + " ";
														_newCell.setAttribute("name", "box");
														_newCell.setAttribute("optionName", _attributes[j].name);
														_newCell.setAttribute("value", _attributes[j].value);
														let _checkBox = document.createElement("input");
														_checkBox.setAttribute("type", "checkbox");
														_checkBox.setAttribute("value", _attributes[j].value);
														_checkBox.setAttribute("id", _count);
														if (_attributes[j].value.toLowerCase() === "true") {
															_checkBox.checked = true;
														}
														else {
															_checkBox.checked = false;
														}
														_newCell.appendChild(_checkBox);
													}
													else {
														_newCell.innerHTML = _attributes[j].name + " ";
														_newCell.setAttribute("name", "text");
														_newCell.setAttribute("optionName", _attributes[j].name);
														_newCell.setAttribute("value", _attributes[j].value);
														let _textBox = document.createElement("input");
														_textBox.setAttribute("type", "text");
														_textBox.setAttribute("value", _attributes[j].value);
														_textBox.setAttribute("id", _count);
														_newCell.appendChild(_textBox);
													}
													_count++;
												}
											}
										}
									}
								}
							}
						}
					}
					else if (_request2.status === 404) {
						alert("Server failed to send the ServerToolsConfig.xml");
					}
				};
				_request2.send(String.empty);
			}
			else if (_request.status === 401) {
				window.location.href = _uri;
			}
		};
		_request.send(_encrypted);
	}
};

function SaveConfig() {
	if (document.getElementById("AcceptSave").checked) {
		document.getElementById("AcceptSave").checked = false;
		let _encrypted = Encrypt(clientSessionId);
		let _uri = window.location.href;
		let _request = new XMLHttpRequest();
		_request.open('POST', window.location.href.replace('st.html', 'UpdateConfig'), false);
		_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
		_request.onerror = function() {
			alert("Request failed. Server may be restarting");
		};
		_request.onload = function () {
			if (_request.status === 200 && _request.readyState === 4 && _request.responseText.length === 2000) {
				let _serverResponse = _request.responseText;
				if (_serverResponse.substring(0, 8) === clientId) {
					serverKeys = _serverResponse.substring(8);
				}
				if (_configXml.documentElement != null) {
					let _tableRows = document.getElementById("ConfigBody").rows;
					let _count = 0;
					let _newconfig = ""
					for (i = 0; i < _tableRows.length; i++) {
						let _cells = _tableRows[i].cells;
						for (j = 0; j < _cells.length; j++) {
							if (j === 0) {
								_newconfig += _cells[j].attributes[1].value + "§";
							}
							if (_cells[j].attributes[0].value !== "title") {
								if (_cells[j].attributes[0].value === "box") {
									var _checkBox = document.getElementById(_count);
									if (_checkBox.checked) {
										_newconfig += _cells[j].attributes[1].value + "σ";
										if (j === _cells.length - 1) {
											_newconfig += "True" + "☼";
										}
										else {
											_newconfig += "True" + "╚";
										}
									}
									else {
										_newconfig += _cells[j].attributes[1].value + "σ";
										if (j === _cells.length - 1) {
											_newconfig += "False" + "☼";
										}
										else {
											_newconfig += "False" + "╚";
										}
									}
								}
								else {
									var _textBox = document.getElementById(_count);
									_newconfig += _cells[j].attributes[1].value + "σ";
									if (j === _cells.length - 1) {
										_newconfig += _textBox.value + "☼";
									}
									else {
										_newconfig += _textBox.value + "╚";
									}
								}
							}
							_count++;
						}
					}
					let _request2 = new XMLHttpRequest();
					_request2.open('POST', window.location.href.replace('st.html', 'UpdateConfig'), false);
					_request2.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
					_request2.onerror = function() {
						alert("Request failed. Server may be restarting");
					};
					_request2.onload = function () {
						if (_request2.status === 200 && _request2.readyState === 4) {
							alert("Received and saved");
						}
						else if (_request2.status === 406 && _request.readyState === 4) {
							alert("Received but no changes were detected");
						}
						else if (_request2.status === 401) {
							alert("Failed to save changes. Server may be restarting");
						}
						else if (_request.failed) {
							alert("Request failed. Server may be restarting");
						}
					};
					_request2.send(_newconfig);
				}
			}
			else if (_request.status === 401) {
				alert("Failed to save changes. Server may be restarting");
			}
			else if (_request.failed) {
				alert("Request failed. Server may be restarting");
			}
		};
		_request.send(_encrypted);
	}
	else {
		alert("Are you a robot? Click the config checkbox first");
	}
};

function Kick() {
	let _steamId = document.getElementById("PlayersSteamId").value;
	if (_steamId != "" && _steamId.length === 17) {
		let _encrypted = Encrypt(clientSessionId + "`" + _steamId);
		let _uri = window.location.href;
		let _request = new XMLHttpRequest();
		_request.open('POST', window.location.href.replace('st.html', 'Kick'), false);
		_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
		_request.onerror = function() {
			alert("Request failed. Server may be restarting");
		};
		_request.onload = function () {
			if (_request.status === 200 && _request.readyState === 4 && _request.responseText.length === 2000) {
				let _serverResponse = _request.responseText;
				if (_serverResponse.substring(0, 8) === clientId)
				{
					serverKeys = _serverResponse.substring(8);
				}
				alert(_steamId + " has been kicked");
			}
			else if (_request.status === 406 && _request.readyState === 4 && _request.responseText.length === 2000) {
				let _serverResponse = _request.responseText;
				if (_serverResponse.substring(0, 8) === clientId)
				{
					serverKeys = _serverResponse.substring(8);
				}
				alert(_steamId + " was not found online. Unable to kick user");
			}
			else if (_request.status === 401) {
				window.location.href = _uri;
			}
			else if (_request.failed) {
				alert("Request failed. Server may be restarting");
			}
		};
		_request.send(_encrypted);
	}
	else {
		alert("Steam id is the wrong length");
	}
};

function Ban() {
	let _steamId = document.getElementById("PlayersSteamId").value;
	if (_steamId != "" && _steamId.length === 17) {
		let _encrypted = Encrypt(clientSessionId + "`" + _steamId);
		let _uri = window.location.href;
		let _request = new XMLHttpRequest();
		_request.open('POST', window.location.href.replace('st.html', 'Ban'), false);
		_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
		_request.onerror = function() {
			alert("Request failed. Server may be restarting");
		};
		_request.onload = function () {
			if (_request.status === 200 && _request.readyState === 4 && _request.responseText.length === 2000) {
				let _serverResponse = _request.responseText;
				if (_serverResponse.substring(0, 8) === clientId)
				{
					serverKeys = _serverResponse.substring(8);
				}
				alert(_steamId + " has been banned");
			}
			else if (_request.status === 406 && _request.readyState === 4 && _request.responseText.length === 2000) {
				let _serverResponse = _request.responseText;
				if (_serverResponse.substring(0, 8) === clientId)
				{
					serverKeys = _serverResponse.substring(8);
				}
				alert(_steamId + " was not found online but the id has been banned");
			}
			else if (_request.status === 401) {
				window.location.href = _uri;
			}
			else if (_request.failed) {
				alert("Request failed. Server may be restarting");
			}
		};
		_request.send(_encrypted);
	}
	else {
		alert("Steam id is empty or too short");
	}
};

function Mute() {
	let _steamId = document.getElementById("PlayersSteamId").value;
	if (_steamId != "" && _steamId.length === 17) {
		let _encrypted = Encrypt(clientSessionId + "`" + _steamId);
		let _uri = window.location.href;
		let _request = new XMLHttpRequest();
		_request.open('POST', window.location.href.replace('st.html', 'Mute'), false);
		_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
		_request.onerror = function() {
			alert("Request failed. Server may be restarting");
		};
		_request.onload = function () {
			if (_request.status === 200 && _request.readyState === 4 && _request.responseText.length === 2000) {
				let _serverResponse = _request.responseText;
				if (_serverResponse.substring(0, 8) === clientId)
				{
				serverKeys = _serverResponse.substring(8);
				}
				alert(_steamId + " has been muted");
			}
			else if (_request.status === 202 && _request.readyState === 4 && _request.responseText.length === 2000) {
				let _serverResponse = _request.responseText;
				if (_serverResponse.substring(0, 8) === clientId)
				{
				serverKeys = _serverResponse.substring(8);
				}
				alert(_steamId + " has been unmuted");
			}
			else if (_request.status === 406 && _request.readyState === 4 && _request.responseText.length === 2000) {
				let _serverResponse = _request.responseText;
				if (_serverResponse.substring(0, 8) === clientId)
				{
					serverKeys = _serverResponse.substring(8);
				}
				alert("Mute tool is disabled. Enable before using it");
			}
			else if (_request.status === 401) {
				window.location.href = _uri;
			}
			else if (_request.failed) {
				alert("Request failed. Server may be restarting");
			}
		};
		_request.send(_encrypted);
	}
	else {
		alert("Steam id is empty or too short");
	}
};

function Jail() {
	let _steamId = document.getElementById("PlayersSteamId").value;
	if (_steamId != "" && _steamId.length === 17) {
		let _encrypted = Encrypt(clientSessionId + "`" + _steamId);
		let _uri = window.location.href;
		let _request = new XMLHttpRequest();
		_request.open('POST', window.location.href.replace('st.html', 'Jail'), false);
		_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
		_request.onerror = function() {
			alert("Request failed. Server may be restarting");
		};
		_request.onload = function () {
			if (_request.status === 200 && _request.readyState === 4 && _request.responseText.length === 2000) {
				let _serverResponse = _request.responseText;
				if (_serverResponse.substring(0, 8) === clientId) {
					serverKeys = _serverResponse.substring(8);
				}
				alert(_steamId + " has been jailed/unjailed");
			}
			else if (_request.status === 406 && _request.readyState === 4) {
				let _serverResponse = _request.responseText;
				if (_serverResponse.substring(0, 8) === clientId) {
					serverKeys = _serverResponse.substring(8);
				}
				alert(_steamId + " was not found online but has been jailed/unjailed");
			}
			else if (_request.status === 500 && _request.readyState === 4) {
				let _serverResponse = _request.responseText;
				if (_serverResponse.substring(0, 8) === clientId) {
					serverKeys = _serverResponse.substring(8);
				}
				alert("Jail tool is not enabled. Enable before using it");
			}
			else if (_request.status === 409 && _request.readyState === 4) {
				alert("Jail position has not been set. Unable to run command");
			}
			else if (_request.status === 401 && _request.readyState === 4) {
				window.location.href = _uri;
			}
			else if (_request.failed) {
				alert("Request failed. Server may be restarting");
			}
		};
		_request.send(_encrypted);
	}
	else {
		alert("Steam id is empty or too short");
	}
};

function Reward() {
	let _steamId = document.getElementById("PlayersSteamId").value;
	if (_steamId != "" && _steamId.length === 17) {
		let _encrypted = Encrypt(clientSessionId + "`" + _steamId);
		let _uri = window.location.href;
		let _request = new XMLHttpRequest();
		_request.open('POST', window.location.href.replace('st.html', 'Reward'), false);
		_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
		_request.onerror = function() {
			alert("Request failed. Server may be restarting");
		};
		_request.onload = function () {
			if (_request.status === 200 && _request.readyState === 4 && _request.responseText.length === 2000) {
				let _serverResponse = _request.responseText;
				if (_serverResponse.substring(0, 8) === clientId) {
					serverKeys = _serverResponse.substring(8);
				}
				alert(_steamId + " has received a vote reward");
			}
			else if (_request.status === 406 && _request.readyState === 4) {
				let _serverResponse = _request.responseText;
				if (_serverResponse.substring(0, 8) === clientId)
				{
					serverKeys = _serverResponse.substring(8);
				}
				alert(_steamId + " was not found online. Unable to give reward");
			}
			else if (_request.status === 500 && _request.readyState === 4) {
				let _serverResponse = _request.responseText;
				if (_serverResponse.substring(0, 8) === clientId) {
					serverKeys = _serverResponse.substring(8);
				}
				alert("Vote reward is not enabled. Enable before using it");
			}
			else if (_request.status === 401) {
				window.location.href = _uri;
			}
			
			else if (_request.failed) {
				alert("Request failed. Server may be restarting");
			}
		};
		_request.send(_encrypted);
	}
	else {
		alert("Steam id is empty or too short");
	}
};

function StartPlayerClock() {
	playerClock = setInterval(UpdatePlayers, 10000);
};

function StopPlayerClock() {
	clearTimeout(playerClock);
};

function StartConsoleClock() {
	consoleClock = setInterval(UpdateConsole, 10000);
};

function StopConsoleClock() {
	clearTimeout(consoleClock);
};

function Encrypt(_input) {
	let _outputLength = _input.length;
	let _keys = serverKeys.match(/.{1,8}/g);
	let _encrypted = clientId;
	for (let i = 0; i < _outputLength; i++) {
		let _char = _input.charCodeAt(i);
		if (_char > 31 && _char < 127) {
			let _charParse = parseInt(_char.toString(2));
			let _binaryParse = parseInt(_keys[i]);
			let _binaryKey = (_charParse + _binaryParse).toString().padStart(8, '0');
			_encrypted += _binaryKey;
		}
		else {
			alert("Unable to complete encryption. Do not use special symbols");
			return;
		}
	}
	return _encrypted;
};
