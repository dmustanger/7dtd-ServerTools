var PlayerClock;
var ConsoleClock;
var SIv;
var SKey;
var ClientId = "";
var AlphaNumSet = "jJWxk9Kl3w8vfXAbyYz0ZLmPqMn5NoO6dDe1EpQrStaBc2CgGhH7iRsITu4UFV";
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
	SetClientId();
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

function SetClientId() {
	ClientId = "";
	for (i = 0; i < 16; i++)
	{
		ClientId += AlphaNumSet.charAt(Math.floor(Math.random() * 62));
	}
};

function Handshake() {
	SetClientId();
	let _request = new XMLHttpRequest();
	_request.open('POST', window.location.href.replace('st.html', 'Handshake'), false);
	_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
	_request.onerror = function() {
		alert("Request failed. Server may be restarting");
	};
	_request.onload = function () {
		if (_request.status === 401 && _request.readyState === 4) {
			SetClientId();
			Handshake();
		}
	};
	_request.send(ClientId);
};

function SignIn() {
	let _text1 = document.getElementById("Text1").value;
	if (_text1.length > 5 && _text1.length < 31) {
		let _text2 = document.getElementById("Text2").value;
		if (_text2.length > 5 && _text2.length < 31) {
			if (document.getElementById("NotBotBox1").checked) {
				let _request = new XMLHttpRequest();
				_request.open('POST', window.location.href.replace('st.html', 'SignIn'), false);
				_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
				_request.onerror = function() {
					alert("Request failed. Server may be restarting");
				};
				_request.onload = function () {
					if (_request.status === 200 && _request.readyState === 4) {
						let _key = "";
						if (_text2.length >= 16) {
							_key += _text2.substring(0, 16);
						}
						else if (_text2.length >= 8) {
							let _pChop = _text2.substring(0, 8);
							_key += _pChop + _pChop;
						}
						else {
							let _pChop = _text2.substring(0, 4);
							_key += _pChop + _pChop + _pChop + _pChop;
						}
						SKey = _key;
						SIv = _request.responseText;
						let encrypted = CryptoJS.AES.encrypt(CryptoJS.enc.Utf8.parse(_text2), CryptoJS.enc.Utf8.parse(SKey), {
							keySize: 256 / 32,
							iv: CryptoJS.enc.Utf8.parse(SIv),
							mode: CryptoJS.mode.CBC,
							padding: CryptoJS.pad.Pkcs7
						});
						_request.open('POST', window.location.href.replace('st.html', 'SignIn'), false);
						_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
						_request.onerror = function() {
							alert("Request failed. Server may be restarting");
						};
						_request.onload = function () {
							if (_request.status === 200 && _request.readyState === 4) {
								SIv = _request.responseText;
								document.getElementById('LoginContainer').style.visibility = "hidden";
								document.getElementById('LogoutContainer').style.visibility = "visible";
								document.getElementById('NewPassButton').style.visibility = "visible";
								document.getElementById("MenuContainer").style.visibility = "visible";
								document.getElementById("ClientId").innerHTML = _text1;
								document.getElementById("NotBotBox1").checked = false;
								document.getElementById("Text1").value = "";
								document.getElementById("Text2").value = "";
							}
							else if (_request.status === 403 && _request.readyState === 4) {
								alert("Login failed. Your credentials were denied by the server.");
							}
							else if (_request.status === 401 && _request.readyState === 4) {
								SetClientId();
								Handshake();
								alert("Security credentials reset. Submit your id and password again");
							}
						};
						_request.send(ClientId + encrypted);
					}
					else if (_request.status === 401 && _request.readyState === 4) {
						SetClientId();
						Handshake();
						alert("Security credentials reset. Submit your id and password again");
					}
					else if (_request.status === 403 && _request.readyState === 4) {
						alert("Login failed. Your credentials were denied by the server.");
					}
				};
				_request.send(ClientId + _text1);
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
		let _encrypted = CryptoJS.AES.encrypt(CryptoJS.enc.Utf8.parse(ClientId), CryptoJS.enc.Utf8.parse(SKey), {
			keySize: 256 / 32,
			iv: CryptoJS.enc.Utf8.parse(SIv),
			mode: CryptoJS.mode.CBC,
			padding: CryptoJS.pad.Pkcs7
		});
		let _request = new XMLHttpRequest();
		_request.open('POST', window.location.href.replace('st.html', 'SignOut'), false);
		_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
		_request.onerror = function() {
		alert("Request failed. Server may be restarting");
		};
		_request.onload = function () {
			if (_request.status === 200 && _request.readyState === 4) {
				window.location.href = window.location.href;
			}
		};
		_request.send(ClientId + _encrypted);
	}
	else {
		alert ("Are you a robot? Click the sign out checkbox first");
	}
};

function SetPass() {
	if (document.getElementById("Text3").value) {
		let _password = document.getElementById("Text3").value;
		if (_password.length > 5 && _password.length < 31) {
			if (document.getElementById("Text4").value) {
				let _confirm = document.getElementById("Text4").value;
				if (_password === _confirm) {
					if (document.getElementById("NotBotBox2").checked) {
						let _encrypted = CryptoJS.AES.encrypt(CryptoJS.enc.Utf8.parse(ClientId), CryptoJS.enc.Utf8.parse(SKey), {
							keySize: 256 / 32,
							iv: CryptoJS.enc.Utf8.parse(SIv),
							mode: CryptoJS.mode.CBC,
							padding: CryptoJS.pad.Pkcs7
						});
						let _request = new XMLHttpRequest();
						_request.open('POST', window.location.href.replace('st.html', 'NewPass'), false);
						_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
						_request.onerror = function() {
							alert("Request failed. Server may be restarting");
						};
						_request.onload = function () {
							if (_request.status === 200 && _request.readyState === 4) {
								SIv = _request.responseText;
								let _encrypted2 = CryptoJS.AES.encrypt(CryptoJS.enc.Utf8.parse(_confirm), CryptoJS.enc.Utf8.parse(SKey), {
									keySize: 256 / 32,
									iv: CryptoJS.enc.Utf8.parse(SIv),
									mode: CryptoJS.mode.CBC,
									padding: CryptoJS.pad.Pkcs7
								});
								_request.open('POST', window.location.href.replace('st.html', 'NewPass'), false);
								_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
								_request.onerror = function() {
									alert("Request failed. Server may be restarting");
								};
								_request.onload = function () {
									if (_request.status === 200 && _request.readyState === 4) {
										SIv = _request.responseText;
										document.getElementById('CancelButton').style.visibility = "hidden";
										document.getElementById('SetPassButton').style.visibility = "hidden";
										document.getElementById("PasswordContainer").style.visibility = "hidden";
										document.getElementById("LogoutContainer").style.visibility = "visible";
										document.getElementById('NewPassButton').style.visibility = "visible";
										document.getElementById("NotBotBox2").checked = false;
										document.getElementById("Text3").value = "";
										document.getElementById("Text4").value = "";
									}
								};
								_request.send(ClientId + _encrypted2);
							}
							else if (_request.status === 401) {
								window.location.href = window.location.href;
							}
						};
						_request.send(ClientId + _encrypted);
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
			alert("Invalid password. Must be 6-30 characters with no spaces");
		}
	}
	else {
		alert("Invalid password. Password can not be blank");
	}
};

function ConsolePage() {
	StopPlayerClock();
	StopConsoleClock();
	let _encrypted = CryptoJS.AES.encrypt(CryptoJS.enc.Utf8.parse(ClientId), CryptoJS.enc.Utf8.parse(SKey), {
		keySize: 256 / 32,
		iv: CryptoJS.enc.Utf8.parse(SIv),
		mode: CryptoJS.mode.CBC,
		padding: CryptoJS.pad.Pkcs7
	});
	let _request = new XMLHttpRequest();
	_request.open('POST', window.location.href.replace('st.html', 'Console'), false);
	_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
	_request.onerror = function() {
		alert("Request failed. Server may be restarting");
	};
	_request.onload = function () {
		if (_request.status === 200 && _request.readyState === 4) {
			SIv = _request.responseText;
			document.getElementById('BackgroundContainer').style.visibility = "hidden";
			document.getElementById("ConfigContainer").style.visibility = "hidden";
			document.getElementById("ContactsContainer").style.visibility = "hidden";
			document.getElementById("DevsContainer").style.visibility = "hidden";
			document.getElementById("PlayersContainer").style.visibility = "hidden";
			document.getElementById("ConsoleContainer").style.visibility = "visible";
			_request.open('POST', window.location.href.replace('st.html', 'Console'), false);
			_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
			_request.onerror = function() {
				alert("Request failed. Server may be restarting");
			};
			_request.onload = function () {
				if (_request.status === 200 && _request.readyState === 4) {
					let _linesAndCount = _request.responseText.split('☼');
					LineCount = _linesAndCount[1];
					let _log = document.getElementById('Console');
					_log.textContent += _linesAndCount[0];
					_log.scrollIntoView(false);
					StartConsoleClock();
				}
				else if (_request.status === 401) {
					window.location.href = window.location.href;
				}
			};
			_request.send(ClientId + LineCount);
		}
		else if (_request.status === 401) {
			window.location.href = window.location.href;
		}
	};
	_request.send(ClientId + _encrypted);
};

function UpdateConsole() {
	let _encrypted = CryptoJS.AES.encrypt(CryptoJS.enc.Utf8.parse(ClientId), CryptoJS.enc.Utf8.parse(SKey), {
		keySize: 256 / 32,
		iv: CryptoJS.enc.Utf8.parse(SIv),
		mode: CryptoJS.mode.CBC,
		padding: CryptoJS.pad.Pkcs7
	});
	let _request = new XMLHttpRequest();
	_request.open('POST', window.location.href.replace('st.html', 'Console'), false);
	_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
	_request.onerror = function() {
		alert("Request failed. Server may be restarting");
	};
	_request.onload = function () {
		if (_request.status === 200 && _request.readyState === 4) {
			SIv = _request.responseText;
			_request.open('POST', window.location.href.replace('st.html', 'Console'), false);
			_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
			_request.onerror = function() {
				alert("Request failed. Server may be restarting");
			};
			_request.onload = function () {
				if (_request.status === 200 && _request.readyState === 4) {
					let _linesAndCount = _request.responseText.split('☼');
					LineCount = _linesAndCount[1];
					let _log = document.getElementById('Console');
					_log.textContent += _linesAndCount[0];
					_log.scrollIntoView(false);
				}
				else if (_request.status === 401) {
					window.location.href = window.location.href;
				}
			};
			_request.send(ClientId + LineCount);
		}
		else if (_request.status === 401) {
			window.location.href = window.location.href;
		}
	};
	_request.send(ClientId + _encrypted);
};

function Command() {
	if (document.getElementById('ConsoleCommand').value !== "") {
		let _encrypted = CryptoJS.AES.encrypt(CryptoJS.enc.Utf8.parse(ClientId), CryptoJS.enc.Utf8.parse(SKey), {
			keySize: 256 / 32,
			iv: CryptoJS.enc.Utf8.parse(SIv),
			mode: CryptoJS.mode.CBC,
			padding: CryptoJS.pad.Pkcs7
		});
		let _request = new XMLHttpRequest();
		_request.open('POST', window.location.href.replace('st.html', 'Command'), false);
		_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
		_request.onerror = function() {
			alert("Request failed. Server may be restarting");
		};
		_request.onload = function () {
			if (_request.status === 200 && _request.readyState === 4) {
				SIv = _request.responseText;
				let _command = document.getElementById('ConsoleCommand');
				if (_command.value === "/r") {
					_command.value = LastCommand;
				}
				let _encrypted2 = CryptoJS.AES.encrypt(CryptoJS.enc.Utf8.parse(_command.value), CryptoJS.enc.Utf8.parse(SKey), {
					keySize: 256 / 32,
					iv: CryptoJS.enc.Utf8.parse(SIv),
					mode: CryptoJS.mode.CBC,
					padding: CryptoJS.pad.Pkcs7
				});
				_request.open('POST', window.location.href.replace('st.html', 'Command'), false);
				_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
				_request.onerror = function() {
					alert("Request failed. Server may be restarting");
				};
				_request.onload = function () {
					if (_request.status === 200 && _request.readyState === 4) {
						StopConsoleClock();
						let _linesAndCount = _request.responseText.split('☼');
						LineCount = _linesAndCount[1];
						SIv = _linesAndCount[2];
						let _log = document.getElementById('Console');
						_log.appendChild(document.createTextNode(_linesAndCount[0]));
						_log.scrollIntoView(false);
						StartConsoleClock();
					}
					else if (_request.status === 400 && _request.readyState === 4) {
						SIv = _request.responseText;
						alert("300 characters is the command size limit.");
					}
					else if (_request.status === 406 && _request.readyState === 4) {
						SIv = _request.responseText;
						alert("Unknown command");
					}
				};
				_request.send(ClientId + "☼" + LineCount + "☼" + _encrypted2);
				LastCommand = _command.value;
				_command.value = "";
			}
			else if (_request.status === 401) {
				window.location.href = window.location.href;
			}
		};
		_request.send(ClientId + _encrypted);
	}
};

function UpdatePlayers() {
	let _encrypted = CryptoJS.AES.encrypt(CryptoJS.enc.Utf8.parse(ClientId), CryptoJS.enc.Utf8.parse(SKey), {
		keySize: 256 / 32,
		iv: CryptoJS.enc.Utf8.parse(SIv),
		mode: CryptoJS.mode.CBC,
		padding: CryptoJS.pad.Pkcs7
	});
	let _request = new XMLHttpRequest();
	_request.open('POST', window.location.href.replace('st.html', 'Players'), false);
	_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
	_request.onerror = function() {
		alert("Request failed. Server may be restarting");
	};
	_request.onload = function () {
		if (_request.status === 200 && _request.readyState === 4) {
			SIv = _request.responseText;
			_request.open('POST', window.location.href.replace('st.html', 'Players'), false);
			_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
			_request.onerror = function() {
				alert("Request failed. Server may be restarting");
			};
			_request.onload = function () {
				if (_request.status === 200 && _request.readyState === 4) {
					if (_request.responseText !== "") {
						let _responseSplit = _request.responseText.split('☼');
						let _r64 = CryptoJS.lib.CipherParams.create({
							ciphertext: CryptoJS.enc.Base64.parse(_responseSplit[0])
						});
						let decrypted = CryptoJS.AES.decrypt(_r64, CryptoJS.enc.Utf8.parse(SKey), {
							keySize: 256 / 32,
							iv: CryptoJS.enc.Utf8.parse(SIv),
							mode: CryptoJS.mode.CBC,
							padding: CryptoJS.pad.Pkcs7
						});
						let _result = CryptoJS.enc.Utf8.stringify(decrypted);
						SIv = _responseSplit[1];
						let _table = document.getElementById("PlayersBody");
						_table.innerHTML = "";
						if (_responseSplit[2] !== "1") {
							let _configSplit1 = _result.split('☼');
							let _configSplit1Length = _configSplit1.length;
							for (let j = 0; j < _configSplit1Length; j++) {
								let _configSplit2 = _configSplit1[j].split('§');
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
								_cell0.innerHTML = _configSplit2[0];
								_cell1.innerHTML = _configSplit2[1];
								_cell2.innerHTML = _configSplit2[2];
								_cell3.innerHTML = _configSplit2[3];
								_cell4.innerHTML = _configSplit2[4];
								_cell5.innerHTML = _configSplit2[5];
							}
						}	
						else {
							let _configSplit = _result.split('§');
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
							_cell0.innerHTML = _configSplit[0];
							_cell1.innerHTML = _configSplit[1];
							_cell2.innerHTML = _configSplit[2];
							_cell3.innerHTML = _configSplit[3];
							_cell4.innerHTML = _configSplit[4];
							_cell5.innerHTML = _configSplit[5];
						}
						document.getElementById("PlayerCount").value = _table.rows.length;
					}
					else {
						let _table = document.getElementById("PlayersBody");
						_table.innerHTML = "";
						document.getElementById("PlayerCount").value = _table.rows.length;
					}
				}
				else if (_request.status === 401 && _request.readyState === 4) {
					window.location.href = window.location.href;
				}
			};
			_request.send(ClientId);
		}
		else if (_request.status === 401) {
			window.location.href = window.location.href;
		}
	};
	_request.send(ClientId + _encrypted);
};

function Config() {
	if (document.getElementById("AcceptSave").checked) {
		document.getElementById("AcceptSave").checked = false;
		let _encrypted = CryptoJS.AES.encrypt(CryptoJS.enc.Utf8.parse(ClientId), CryptoJS.enc.Utf8.parse(SKey), {
			keySize: 256 / 32,
			iv: CryptoJS.enc.Utf8.parse(SIv),
			mode: CryptoJS.mode.CBC,
			padding: CryptoJS.pad.Pkcs7
		});
		let _request = new XMLHttpRequest();
		_request.open('POST', window.location.href.replace('st.html', 'Config'), false);
		_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
		_request.onerror = function() {
			alert("Request failed. Server may be restarting");
		};
		_request.onload = function () {
			if (_request.status === 200 && _request.readyState === 4) {
				SIv = _request.responseText;
				_request.open('GET', window.location.href.replace('st.html', 'Config'), false);
				_request.setRequestHeader('Content-Type', 'text/xml; charset=utf-8');
				_request.onerror = function() {
					alert("Request failed. Server may be restarting");
				};
				_request.onload = function () {
					if (_request.status === 200 && _request.readyState === 4) {
						if (_request.responseXML != null) {
							ConfigXml = _request.responseXML;
							let _root = ConfigXml.documentElement;
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
					else if (_request.status === 404) {
						alert("Server failed to send the ServerToolsConfig.xml");
					}
				};
				_request.send(ClientId);
			}
			else if (_request.status === 401) {
				window.location.href = window.location.href;
			}
		};
		_request.send(ClientId + _encrypted);
	}
};

function SaveConfig() {
	if (document.getElementById("AcceptSave").checked) {
		document.getElementById("AcceptSave").checked = false;
		let _encrypted = CryptoJS.AES.encrypt(CryptoJS.enc.Utf8.parse(ClientId), CryptoJS.enc.Utf8.parse(SKey), {
			keySize: 256 / 32,
			iv: CryptoJS.enc.Utf8.parse(SIv),
			mode: CryptoJS.mode.CBC,
			padding: CryptoJS.pad.Pkcs7
		});
		let _request = new XMLHttpRequest();
		_request.open('POST', window.location.href.replace('st.html', 'SaveConfig'), false);
		_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
		_request.onerror = function() {
			alert("Request failed. Server may be restarting");
		};
		_request.onload = function () {
			if (_request.status === 200 && _request.readyState === 4) {
				SIv = _request.responseText;
				if (ConfigXml.documentElement != null) {
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
					let _encrypted2 = CryptoJS.AES.encrypt(CryptoJS.enc.Utf8.parse(_newconfig), CryptoJS.enc.Utf8.parse(SKey), {
						keySize: 256 / 32,
						iv: CryptoJS.enc.Utf8.parse(SIv),
						mode: CryptoJS.mode.CBC,
						padding: CryptoJS.pad.Pkcs7
					});
					_request.open('POST', window.location.href.replace('st.html', 'SaveConfig'), false);
					_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
					_request.onerror = function() {
						alert("Request failed. Server may be restarting");
					};
					_request.onload = function () {
						if (_request.status === 200 && _request.readyState === 4) {
							SIv = _request.responseText;
							alert("Received and saved");
						}
						else if (_request.status === 406 && _request.readyState === 4) {
							SIv = _request.responseText;
							alert("Received but no changes were detected");
						}
						else if (_request.status === 401) {
							alert("Failed to save changes. Server may be restarting");
						}
						else if (_request.failed) {
							alert("Request failed. Server may be restarting");
						}
					};
					_request.send(ClientId + _encrypted2);
				}
			}
			else if (_request.status === 401) {
				alert("Failed to save changes. Server may be restarting");
			}
			else if (_request.failed) {
				alert("Request failed. Server may be restarting");
			}
		};
		_request.send(ClientId + _encrypted);
	}
	else {
		alert("Are you a robot? Click the config checkbox first");
	}
};

function Kick() {
	let _steamId = document.getElementById("PlayersSteamId").value;
	if (_steamId != "" && _steamId.length === 17) {
		let _encrypted = CryptoJS.AES.encrypt(CryptoJS.enc.Utf8.parse(ClientId), CryptoJS.enc.Utf8.parse(SKey), {
			keySize: 256 / 32,
			iv: CryptoJS.enc.Utf8.parse(SIv),
			mode: CryptoJS.mode.CBC,
			padding: CryptoJS.pad.Pkcs7
		});
		let _request = new XMLHttpRequest();
		_request.open('POST', window.location.href.replace('st.html', 'Kick'), false);
		_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
		_request.onerror = function() {
			alert("Request failed. Server may be restarting");
		};
		_request.onload = function () {
			if (_request.status === 200 && _request.readyState === 4) {
				SIv = _request.responseText;
				alert(_steamId + " has been kicked");
			}
			else if (_request.status === 406 && _request.readyState === 4) {
				SIv = _request.responseText;
				alert(_steamId + " was not found online. Unable to kick user");
			}
			else if (_request.status === 401) {
				window.location.href = window.location.href;
			}
			else if (_request.failed) {
				alert("Request failed. Server may be restarting");
			}
		};
		_request.send(ClientId + _encrypted + "☼" + _steamId);
	}
	else {
		alert("Steam id is the wrong length");
	}
};

function Ban() {
	let _steamId = document.getElementById("PlayersSteamId").value;
	if (_steamId != "" && _steamId.length === 17) {
		let _encrypted = CryptoJS.AES.encrypt(CryptoJS.enc.Utf8.parse(ClientId), CryptoJS.enc.Utf8.parse(SKey), {
			keySize: 256 / 32,
			iv: CryptoJS.enc.Utf8.parse(SIv),
			mode: CryptoJS.mode.CBC,
			padding: CryptoJS.pad.Pkcs7
		});
		let _request = new XMLHttpRequest();
		_request.open('POST', window.location.href.replace('st.html', 'Ban'), false);
		_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
		_request.onerror = function() {
			alert("Request failed. Server may be restarting");
		};
		_request.onload = function () {
			if (_request.status === 200 && _request.readyState === 4) {
				SIv = _request.responseText;
				alert(_steamId + " has been banned");
			}
			else if (_request.status === 406 && _request.readyState === 4) {
				SIv = _request.responseText;
				alert(_steamId + " was not found online but the id has been banned");
			}
			else if (_request.status === 401) {
				window.location.href = window.location.href;
			}
			else if (_request.failed) {
				alert("Request failed. Server may be restarting");
			}
		};
		_request.send(ClientId + _encrypted + "☼" + _steamId);
	}
	else {
		alert("Steam id is empty or too short");
	}
};

function Mute() {
	let _steamId = document.getElementById("PlayersSteamId").value;
	if (_steamId != "" && _steamId.length === 17) {
		let _encrypted = CryptoJS.AES.encrypt(CryptoJS.enc.Utf8.parse(ClientId), CryptoJS.enc.Utf8.parse(SKey), {
			keySize: 256 / 32,
			iv: CryptoJS.enc.Utf8.parse(SIv),
			mode: CryptoJS.mode.CBC,
			padding: CryptoJS.pad.Pkcs7
		});
		let _request = new XMLHttpRequest();
		_request.open('POST', window.location.href.replace('st.html', 'Mute'), false);
		_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
		_request.onerror = function() {
			alert("Request failed. Server may be restarting");
		};
		_request.onload = function () {
			if (_request.status === 200 && _request.readyState === 4) {
				SIv = _request.responseText;
				alert(_steamId + " has been muted");
			}
			else if (_request.status === 202 && _request.readyState === 4) {
				SIv = _request.responseText;
				alert(_steamId + " has been unmuted");
			}
			else if (_request.status === 406 && _request.readyState === 4) {
				SIv = _request.responseText;
				alert("Mute tool is disabled. Enable before using it");
			}
			else if (_request.status === 401) {
				window.location.href = window.location.href;
			}
			else if (_request.failed) {
				alert("Request failed. Server may be restarting");
			}
		};
		_request.send(ClientId + _encrypted + "☼" + _steamId);
	}
	else {
		alert("Steam id is empty or too short");
	}
};

function Jail() {
	let _steamId = document.getElementById("PlayersSteamId").value;
	if (_steamId != "" && _steamId.length === 17) {
		let _encrypted = CryptoJS.AES.encrypt(CryptoJS.enc.Utf8.parse(ClientId), CryptoJS.enc.Utf8.parse(SKey), {
			keySize: 256 / 32,
			iv: CryptoJS.enc.Utf8.parse(SIv),
			mode: CryptoJS.mode.CBC,
			padding: CryptoJS.pad.Pkcs7
		});
		let _request = new XMLHttpRequest();
		_request.open('POST', window.location.href.replace('st.html', 'Jail'), false);
		_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
		_request.onerror = function() {
			alert("Request failed. Server may be restarting");
		};
		_request.onload = function () {
			if (_request.status === 200 && _request.readyState === 4) {
				SIv = _request.responseText;
				alert(_steamId + " has been jailed/unjailed");
			}
			else if (_request.status === 406 && _request.readyState === 4) {
				SIv = _request.responseText;
				alert(_steamId + " was not found online but has been jailed/unjailed");
			}
			else if (_request.status === 500 && _request.readyState === 4) {
				SIv = _request.responseText;
				alert("Jail tool is not enabled. Enable before using it");
			}
			else if (_request.status === 409 && _request.readyState === 4) {
				alert("Jail position has not been set. Unable to run command");
			}
			else if (_request.status === 401 && _request.readyState === 4) {
				window.location.href = window.location.href;
			}
			else if (_request.failed) {
				alert("Request failed. Server may be restarting");
			}
		};
		_request.send(ClientId + _encrypted + "☼" + _steamId);
	}
	else {
		alert("Steam id is empty or too short");
	}
};

function Reward() {
	let _steamId = document.getElementById("PlayersSteamId").value;
	if (_steamId != "" && _steamId.length === 17) {
		let _encrypted = CryptoJS.AES.encrypt(CryptoJS.enc.Utf8.parse(ClientId), CryptoJS.enc.Utf8.parse(SKey), {
			keySize: 256 / 32,
			iv: CryptoJS.enc.Utf8.parse(SIv),
			mode: CryptoJS.mode.CBC,
			padding: CryptoJS.pad.Pkcs7
		});
		let _request = new XMLHttpRequest();
		_request.open('POST', window.location.href.replace('st.html', 'Reward'), false);
		_request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
		_request.onerror = function() {
			alert("Request failed. Server may be restarting");
		};
		_request.onload = function () {
			if (_request.status === 200 && _request.readyState === 4) {
				SIv = _request.responseText;
				alert(_steamId + " has received a vote reward");
			}
			else if (_request.status === 406 && _request.readyState === 4) {
				SIv = _request.responseText;
				alert(_steamId + " was not found online. Unable to give reward");
			}
			else if (_request.status === 500 && _request.readyState === 4) {
				SIv = _request.responseText;
				alert("Vote reward is not enabled. Enable before using it");
			}
			else if (_request.status === 401) {
				window.location.href = window.location.href;
			}
			
			else if (_request.failed) {
				alert("Request failed. Server may be restarting");
			}
		};
		_request.send(ClientId + _encrypted + "☼" + _steamId);
	}
	else {
		alert("Steam id is empty or too short");
	}
};

function StartPlayerClock() {
	PlayerClock = setInterval(UpdatePlayers, 10000);
};

function StopPlayerClock() {
	clearTimeout(PlayerClock);
};

function StartConsoleClock() {
	ConsoleClock = setInterval(UpdateConsole, 10000);
};

function StopConsoleClock() {
	clearTimeout(ConsoleClock);
};

function Encrypt(_input) {

	return _input;
};
