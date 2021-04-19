var _playerClock;
var _cl = "";
var _clId = "";
var _clSid = "";
var _srvK = "";
var _numSet = "4829017536";
var _sidSet = "jJk9Kl3wWxXAbyYz0ZLmMn5NoO6dDe1EfFpPqQrRsStaBc2CgGhH7iITu4U8vV";

function FreshPage() {
  document.getElementById('BackgroundContainer').style.visibility = "hidden";
  document.getElementById('SignInContainer').style.visibility = "hidden";
  document.getElementById('PasswordContainer').style.visibility = "hidden";
  document.getElementById('SignOutContainer').style.visibility = "hidden";
  document.getElementById('MenuContainer').style.visibility = "hidden";
  document.getElementById('NewPassButton').style.visibility = "hidden";
  document.getElementById('CancelButton').style.visibility = "hidden";
  document.getElementById('DisclaimerContainer').style.visibility = "visible";
  document.getElementById('AcceptBox').value = false;
  SetClId();
  SetSid();
  Handshake();
};

function SetClId() {
  _clId = "";
  for (let i = 0; i < 8; i++)
  {
    _clId += _numSet.charAt(Math.floor(Math.random() * 10));
  }
};

function SetSid() {
  _clSid = "";
  for (let i = 0; i < 20; i++)
  {
    _clSid += _sidSet.charAt(Math.floor(Math.random() * 62));
  }
};

function Handshake() {
  let _hS = _clId;
  for (let i = 0; i < 3992; i++) {
    _hS += _numSet.charAt(Math.floor(Math.random() * 10));
  }
  let _hR = "";
  for (let j = 3999; j >= 0; j--) {
    _hR += _hS[j];
  }
  let _uri = window.location.href;
  let _post = new XMLHttpRequest();
  _post.open('POST', window.location.href.replace('st.html', 'Handshake'), false);
  _post.setRequestHeader('Content-Type', 'text/plain; charset=utf-8');
  _post.onload = function () {
    if (_post.status === 200 && _post.responseText.length === 4000) {
      let _r = "";
      for (let k = 3999; k >= 0; k--) {
        _r += _post.responseText[k];
      }
      _srvK = _r.substring(8);
      let _s = Encrypt(_clSid);
      let _post2 = new XMLHttpRequest();
      _post2.open('POST', window.location.href.replace('st.html', 'Secure'), false);
      _post2.setRequestHeader('Content-Type', 'text/plain; charset=utf-8');
      _post2.onload = function () {
        if (_post2.status === 200 && _post2.responseText.length === 4000) {
          let _r2 = "";
          for (let j = 3999; j >= 0; j--) {
            _r2 += _post2.responseText[j];
          }
          if (_r2.substring(0, 8) == _clId)
          {
            _srvK = _r2.substring(8);
          }
        }
        else if (_post.status === 401) {
          window.location.href = _uri;
        }
      };
      _post2.send(_s);
    }
  };
  _post.send(_hR);
};

function Accept() {
	if (document.getElementById("AcceptBox").checked) {
    document.getElementById("SignInContainer").style.visibility = "visible";
    document.getElementById("DisclaimerContainer").style.visibility = "hidden";
    document.getElementById("AcceptBox").checked = false;
	}
	else {
    document.getElementById("SignInContainer").style.visibility = "hidden";
    document.getElementById("DisclaimerContainer").style.visibility = "visible";
	}
};

function SignIn() {
  if (document.getElementById("Text1").value) {
    let _text1 = document.getElementById("Text1").value;
    if (_text1.length === 17) {
      if (document.getElementById("Text2").value) {
        let _text2 = document.getElementById("Text2").value;
        if (_text2.length > 5 && _text2.length < 21) {
          if (document.getElementById("NotBotBox1").checked) {
            let _enc = Encrypt(_clSid + " " + _text1 + " " + _text2);
            let _uri = window.location.href;
            let _post = new XMLHttpRequest();
            _post.open('POST', window.location.href.replace('st.html', 'SignIn'), false);
            _post.setRequestHeader('Content-Type', 'text/plain; charset=utf-8');
            _post.onload = function () {
              if (_post.status === 200 && _post.responseText.length === 4000) {
                let _r = "";
                for (let i = 3999; i >= 0; i--) {
                  _r += _post.responseText[i];
                }
                if (_r.substring(0, 8) === _clId)
                {
                  _srvK = _r.substring(8);
                  document.getElementById("SignInContainer").style.visibility = "hidden";
                  document.getElementById("MenuContainer").style.visibility = "visible";
                  document.getElementById("SignOutContainer").style.visibility = "visible";
                  document.getElementById('NewPassButton').style.visibility = "visible";
                  document.getElementById("ClientId").innerHTML = _text1;
                  _cl = _text1;
                  document.getElementById("NotBotBox1").checked = false;
                  document.getElementById("Text1").value = "";
                  document.getElementById("Text2").value = "";
                }
              }
              else if (_post.status === 401 && _post.responseText.length === 4000) {
                let _r = "";
                for (let j = 3999; j >= 0; j--) {
                  _r += _post.responseText[j];
                }
                if (_r.substring(0, 8) === _clId)
                {
                  _srvK = _r.substring(8);
                  alert("Authorization failed");
                }
              }
              else {
                window.location.href = _uri;
              }
            };
            _post.send(_enc);
          }
          else {
            alert("Click the box before logging in");
          }
        }
        else {
          alert("Invalid Password");
        }
      }
      else {
        alert("Invalid Password");
      }
    }
    else {
      alert("Invalid Client");
    }
  }
  else {
    alert("Invalid Client");
  }
};

function SignOut() {
  if (document.getElementById("ConfirmSignout").checked) {
    let _enc = Encrypt(_clSid);
    let _uri = window.location.href;
    let _post = new XMLHttpRequest();
    _post.open('POST', window.location.href.replace('st.html', 'SignOut'), false);
    _post.setRequestHeader('Content-Type', 'text/plain; charset=utf-8');
    _post.onload = function () {
      if (_post.status === 200) {
        document.getElementById("ConfirmSignout").checked = false;
        window.location.href = _uri;
      }
    };
    _post.send(_enc);
  }
  else {
    alert ("Check the box before proceeding");
  }
};

function NewPass() {
  let _enc = Encrypt(_clSid + " " + _cl);
  let _uri = window.location.href;
  let _post = new XMLHttpRequest();
  _post.open('POST', window.location.href.replace('st.html', 'NewPass'), false);
  _post.setRequestHeader('Content-Type', 'text/plain; charset=utf-8');
  _post.onload = function () {
    if (_post.status === 200 && _post.responseText.length === 4000) {
      let _r = "";
      for (let i = 3999; i >= 0; i--) {
        _r += _post.responseText[i];
      }
      if (_r.substring(0, 8) === _clId)
      {
        _srvK = _r.substring(8);
        document.getElementById("SignOutContainer").style.visibility = "hidden";
        document.getElementById('NewPassButton').style.visibility = "hidden";
        document.getElementById('CancelButton').style.visibility = "visible";
        document.getElementById("PasswordContainer").style.visibility = "visible";
      }
    }
    else if (_post.status === 401) {
      window.location.href = _uri;
    }
  };
  _post.send(_enc);
};

function Cancel() {
  document.getElementById('CancelButton').style.visibility = "hidden";
  document.getElementById("PasswordContainer").style.visibility = "hidden";
  document.getElementById("SignOutContainer").style.visibility = "visible";
  document.getElementById('NewPassButton').style.visibility = "visible";
  document.getElementById("Text3").value = "";
  document.getElementById("Text4").value = "";
};

function SetPass() {
  if (document.getElementById("Text3").value) {
    let _password = document.getElementById("Text3").value;
    if (_password.length > 5 && _password.length < 21) {
      if (document.getElementById("Text4").value) {
        let _confirm = document.getElementById("Text4").value;
        if (_confirm.length > 5 && _confirm.length < 21) {
          if (_password === _confirm) {
            if (document.getElementById("NotBotBox2").checked) {
              let _enc = Encrypt(_clSid + " " + _cl + " " + _password);
              let _uri = window.location.href;
              let _post = new XMLHttpRequest();
              _post.open('POST', window.location.href.replace('st.html', 'SetPass'), false);
              _post.setRequestHeader('Content-Type', 'text/plain; charset=utf-8');
              _post.onload = function () {
                if (_post.status === 200 && _post.responseText.length === 4000) {
                  let _r = "";
                  for (let i = 3999; i >= 0; i--) {
                    _r += _post.responseText[i];
                  }
                  _srvK = _r.substring(8);
                  document.getElementById('CancelButton').style.visibility = "hidden";
                  document.getElementById("PasswordContainer").style.visibility = "hidden";
                  document.getElementById("SignOutContainer").style.visibility = "visible";
                  document.getElementById('NewPassButton').style.visibility = "visible";
                  document.getElementById("ClientId").innerHTML = _cl;
                  document.getElementById("NotBotBox2").checked = false;
                  document.getElementById("Text3").value = "";
                  document.getElementById("Text4").value = "";
                }
                else if (_post.status === 401) {
                  window.location.href = _uri;
                }
              };
              _post.send(_enc);
            }
            else {
              alert("Click the box before setting the new password");
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
        alert("Invalid password confirmation");
      }
    }
    else {
      alert("Invalid password. Must be 6-20 characters with no spaces");
    }
  }
  else {
    alert("Invalid password");
  }
};

function LogUpdate() {
  document.getElementById("demo").innerHTML = xmlhttp.responseText;
};

function Command() {
  if (document.getElementById('ConsoleCommand').value) {
    let _command = document.getElementById('ConsoleCommand').value;
    let _enc = Encrypt(_clSid + " " + _cl + " " + _command);
    let _uri = window.location.href;
    let _post = new XMLHttpRequest();
    _post.open('POST', window.location.href.replace('st.html', 'Command'), false);
    _post.setRequestHeader('Content-Type', 'text/plain; charset=utf-8');
    _post.onload = function () {
      if (_post.status === 200 && _post.responseText.length === 4000) {
        alert("Console command success");
        let _r = "";
        for (let i = 3999; i >= 0; i--) {
          _r += _post.responseText[i];
        }
        if (_r.substring(0, 8) === _clId)
        {
          _srvK = _r.substring(8);
        }

        //request log update
      }
      else if (_post.status === 401) {
        window.location.href = _uri;
      }
    };
    _post.send(_enc);
  }
};

function HomePage() {
  StopPlayerClock();
  document.getElementById('BackgroundContainer').style.visibility = "hidden";
  document.getElementById("ConfigContainer").style.visibility = "hidden";
  document.getElementById("ConsoleContainer").style.visibility = "hidden";
  document.getElementById("ContactsContainer").style.visibility = "hidden";
  document.getElementById("DevsContainer").style.visibility = "hidden";
  document.getElementById("PlayersContainer").style.visibility = "hidden";
};

function ConsolePage() {
  StopPlayerClock();
  let _enc = Encrypt(_clSid + " " + _cl);
  let _uri = window.location.href;
  let _post = new XMLHttpRequest();
  _post.open('POST', window.location.href.replace('st.html', 'Log'), false);
  _post.setRequestHeader('Content-Type', 'text/plain; charset=utf-8');
  _post.onload = function () {
    document.getElementById('BackgroundContainer').style.visibility = "hidden";
    document.getElementById("ConfigContainer").style.visibility = "hidden";
    document.getElementById("ContactsContainer").style.visibility = "hidden";
    document.getElementById("DevsContainer").style.visibility = "hidden";
    document.getElementById("PlayersContainer").style.visibility = "hidden";
    document.getElementById("ConsoleContainer").style.visibility = "visible";
    if (_post.status === 200 && _post.responseText.length === 4000) {
      let _r = "";
      for (let i = 3999; i >= 0; i--) {
        _r += _post.responseText[i];
      }
      _srvK = _r.substring(8);

    }
    else if (_post.status === 401) {
      window.location.href = _uri;
    }
    //request log update
  };
  _post.send(_enc);
};

function PlayersPage() {
  StopPlayerClock();
  let _table = document.getElementById("PlayersBody");
  _table.innerHTML = "";
  document.getElementById("PlayerCount").value = _table.rows.length;
  document.getElementById('BackgroundContainer').style.visibility = "hidden";
  document.getElementById("ConfigContainer").style.visibility = "hidden";
  document.getElementById("ConsoleContainer").style.visibility = "hidden";
  document.getElementById("ContactsContainer").style.visibility = "hidden";
  document.getElementById("DevsContainer").style.visibility = "hidden";
  document.getElementById("PlayersContainer").style.visibility = "visible";
  updatePlayers();
  StartPlayerLoop();
};

function ConfigPage() {
  StopPlayerClock();
  document.getElementById('BackgroundContainer').style.visibility = "hidden";
  document.getElementById("ConsoleContainer").style.visibility = "hidden";
  document.getElementById("ContactsContainer").style.visibility = "hidden";
  document.getElementById("DevsContainer").style.visibility = "hidden";
  document.getElementById("PlayersContainer").style.visibility = "hidden";
  document.getElementById("ConfigContainer").style.visibility = "visible";
  SetConfigId();
  Config();
};

function DevPage() {
  StopPlayerClock();
  document.getElementById("ConfigContainer").style.visibility = "hidden";
  document.getElementById("ConsoleContainer").style.visibility = "hidden";
  document.getElementById("ContactsContainer").style.visibility = "hidden";
  document.getElementById("PlayersContainer").style.visibility = "hidden";
  document.getElementById('BackgroundContainer').style.visibility = "visible";
  document.getElementById("DevsContainer").style.visibility = "visible";
};

function ContactsPage() {
  StopPlayerClock();
  document.getElementById("ConfigContainer").style.visibility = "hidden";
  document.getElementById("ConsoleContainer").style.visibility = "hidden";
  document.getElementById("PlayersContainer").style.visibility = "hidden";
  document.getElementById("DevsContainer").style.visibility = "hidden";
  document.getElementById('BackgroundContainer').style.visibility = "visible";
  document.getElementById("ContactsContainer").style.visibility = "visible";
};

function updatePlayers() {
  let _enc = Encrypt(_clSid + " " + _cl);
  let _uri = window.location.href;
  let _post = new XMLHttpRequest();
  _post.open('POST', window.location.href.replace('st.html', 'Players'), false);
  _post.setRequestHeader('Content-Type', 'text/plain; charset=utf-8');
  _post.onload = function () {
    if (_post.status === 200 && _post.responseText.length === 4000) {
      let _r = "";
      for (let i = 3999; i >= 0; i--) {
        _r += _post.responseText[i];
      }
      _srvK = _r.substring(8);
      let _req = _r.substring(0, 8);
      for (let j = 0; j < 3992; j++) {
        _req += _numSet.charAt(Math.floor(Math.random() * 10));
      }
      let _reqR = "";
      for (let k = 3999; k >= 0; k--) {
        _reqR += _req[k];
      }
      let _post2 = new XMLHttpRequest();
      _post2.open('POST', window.location.href.replace('st.html', 'Players'), false);
      _post2.setRequestHeader('Content-Type', 'text/plain; charset=utf-8');
      _post2.onload = function () {
        if (_post2.status === 200) {
          let _table = document.getElementById("PlayersBody");
          _table.innerHTML = "";
          if (_post2.status === 200) {
            if (_post2.responseText != "") {
              if (_post2.responseText.includes('☼')) {
                let _resp = _post2.responseText.split('☼');
                let _respLength = _resp.length;
                for (let j = 0; j < _respLength; j++) {
                  let _respSplit = _resp[j].split('§');
                  let _row = _table.insertRow(-1);
                  let _cell1 = _row.insertCell(0);
                  let _cell2 = _row.insertCell(1);
                  let _cell3 = _row.insertCell(2);
                  let _cell4 = _row.insertCell(3);
                  let _cell5 = _row.insertCell(4);
                  let _cell6 = _row.insertCell(5);
                  _cell1.innerHTML = _respSplit[0];
                  _cell2.innerHTML = _respSplit[1];
                  _cell3.innerHTML = _respSplit[2];
                  _cell4.innerHTML = _respSplit[3];
                  _cell5.innerHTML = _respSplit[4];
                  _cell6.innerHTML = _respSplit[5];
                }
              }
              else {
                let _respSplit = _post2.responseText.split('§');
                let _row = _table.insertRow(-1);
                let _cell1 = _row.insertCell(0);
                let _cell2 = _row.insertCell(1);
                let _cell3 = _row.insertCell(2);
                let _cell4 = _row.insertCell(3);
                let _cell5 = _row.insertCell(4);
                let _cell6 = _row.insertCell(5);
                _cell1.innerHTML = _respSplit[0];
                _cell2.innerHTML = _respSplit[1];
                _cell3.innerHTML = _respSplit[2];
                _cell4.innerHTML = _respSplit[3];
                _cell5.innerHTML = _respSplit[4];
                _cell6.innerHTML = _respSplit[5];
              }
            }
            document.getElementById("PlayerCount").value = _table.rows.length;
          }
          else if (_post2.status === 401) {
            window.location.href = _uri;
          }
        }
      };
      _post2.send(_reqR);
    }
    else if (_post.status === 401) {
      window.location.href = _uri;
    }
  };
  _post.send(_enc);
};

function Config() {
  let _enc = Encrypt(_clSid + " " + _cl);
  let _uri = window.location.href;
  let _post = new XMLHttpRequest();
  _post.open('POST', window.location.href.replace('st.html', 'Config'), false);
  _post.setRequestHeader('Content-Type', 'text/plain; charset=utf-8');
  _post.onload = function () {
    if (_post.status === 200 && _post.responseText.length === 4000) {
      let _r = "";
      for (let i = 3999; i >= 0; i--) {
        _r += _post.responseText[i];
      }
      _srvK = _r.substring(8);
      let _req = _r.substring(0, 8);
      for (let j = 0; j < 3992; j++) {
        _req += _numSet.charAt(Math.floor(Math.random() * 10));
      }
      let _reqR = "";
      for (let k = 3999; k >= 0; k--) {
        _reqR += _req[k];
      }
      let _post2 = new XMLHttpRequest();
      _post2.open('POST', window.location.href.replace('st.html', 'Config'), false);
      _post2.setRequestHeader('Content-Type', 'text/plain; charset=utf-8');
      _post2.onload = function () {
        if (_post2.status === 200) {
          let _resp = _post2.responseText.split('§');
          let _respL = _resp.length;
          for (let l = 0; l < _respL; l++) {
            let _input = document.getElementById(l);
            if (_input != null) {
              if (_resp[l].toLowerCase() === "true") {
                _input.checked = true;
              }
              else if (_resp[l].toLowerCase() === "false") {
                _input.checked = false;
              }
              else {
                _input.value = _resp[l];
              }
            }
          }
        }
      };
      _post2.send(_reqR);
    }
  };
  _post.send(_enc);
};

function ClearConfig() {
  let _count = 0;
  let _table = document.getElementById("ConfigBody");
  let _rowCount = _table.rows.length;
  for (let i = 0; i < _rowCount; i++) {
    let _cellCount = _table.rows[i].cells.length;
    for (let j = 1; j < _cellCount; j++) {
      let _input = document.getElementById(_count);
      if (_input != null) {
        if (_input.type === "checkbox") {
          _input.checked = false;
        }
        else {
          _input.value = "";
        }
        _count++;
      }
    }
  }
};

function SaveConfig() {
  if (document.getElementById("AcceptSave").checked) {
    document.getElementById("AcceptSave").checked = false;
    let _enc = Encrypt(_clSid + " " + _cl);
    let _uri = window.location.href;
    let _post = new XMLHttpRequest();
    _post.open('POST', window.location.href.replace('st.html', 'UpdateConfig'), false);
    _post.setRequestHeader('Content-Type', 'text/plain; charset=utf-8');
    _post.onload = function () {
      if (_post.status === 200 && _post.responseText.length === 4000) {
        let _r = "";
        for (let i = 3999; i >= 0; i--) {
          _r += _post.responseText[i];
        }
        _srvK = _r.substring(8);
        let _uid = _r.substring(0, 8);
        let _newConfig = _uid;
        let _count = 0;
        let _table = document.getElementById("ConfigBody");
        let _rowCount = _table.rows.length;
        for (let j = 0; j < _rowCount; j++) {
          let _cellCount = _table.rows[j].cells.length;
          for (let k = 1; k < _cellCount; k++) {
            let _input = document.getElementById(_count);
            if (_input != null) {
              if (_input.type === "checkbox") {
                if (_input.checked) {
                  _newConfig += "True§";
                }
                else {
                  _newConfig += "False§";
                }
              }
              else {
                _newConfig += _input.value + "§";
              }
              _count++;
            }
          }
        }
        _newConfig = _newConfig.trimEnd();
        _newConfig += "☼";
        let _length = 4000 - _newConfig.length;
        for (let l = 0; l < _length; l++) {
          _newConfig += "0";
        }
        let _cPR = "";
        for (let m = 3999; m >= 0; m--) {
          _cPR += _newConfig[m];
        }
        let _post2 = new XMLHttpRequest();
        _post2.open('POST', window.location.href.replace('st.html', 'UpdateConfig'), false);
        _post2.setRequestHeader('Content-Type', 'text/plain; charset=utf-8');
        _post2.send(_cPR);
      }
    };
    _post.send(_enc);
  }
  else {
    alert("Click the box before saving");
  }
};

function Kick() {
  let _steamId = document.getElementById("PlayersSteamId").value;
	if (_steamId != "" && _steamId.length === 17) {
    let _enc = Encrypt(_clSid + " " + _cl + " " + _steamId);
    let _uri = window.location.href;
    let _post = new XMLHttpRequest();
    _post.open('POST', window.location.href.replace('st.html', 'Kick'), false);
    _post.setRequestHeader('Content-Type', 'text/plain; charset=utf-8');
    _post.onload = function () {
      if (_post.status === 200 && _post.responseText.length === 4000) {
        let _r = "";
        for (let i = 3999; i >= 0; i--) {
          _r += _post.responseText[i];
        }
        if (_r.substring(0, 8) === _clId)
        {
          _srvK = _r.substring(8);
        }
        alert(_steamId + " has been kicked");
      }
      else if (_post.status === 202) {
        let _r = "";
        for (let j = 3999; j >= 0; j--) {
          _r += _post.responseText[j];
        }
        if (_r.substring(0, 8) === _clId)
        {
          _srvK = _r.substring(8);
        }
        alert(_steamId + " was not found online");
      }
      else if (_post.status === 401) {
        window.location.href = _uri;
      }
    };
    _post.send(_enc);
	}
  else {
    alert("Steam Id is empty or too short");
  }
};

function Ban() {
  let _steamId = document.getElementById("PlayersSteamId").value;
	if (_steamId != "" && _steamId.length === 17) {
    let _enc = Encrypt(_clSid + " " + _cl + " " + _steamId);
    let _uri = window.location.href;
    let _post = new XMLHttpRequest();
    _post.open('POST', window.location.href.replace('st.html', 'Ban'), false);
    _post.setRequestHeader('Content-Type', 'text/plain; charset=utf-8');
    _post.onload = function () {
      if (_post.status === 200 && _post.responseText.length === 4000) {
        let _r = "";
        for (let i = 3999; i >= 0; i--) {
          _r += _post.responseText[i];
        }
        if (_r.substring(0, 8) === _clId)
        {
          _srvK = _r.substring(8);
        }
        alert(_steamId + " has been banned");
      }
      else if (_post.status === 202) {
        let _r = "";
        for (let j = 3999; j >= 0; j--) {
          _r += _post.responseText[j];
        }
        if (_r.substring(0, 8) === _clId)
        {
          _srvK = _r.substring(8);
        }
        alert(_steamId + " was not found online");
      }
      else if (_post.status === 401) {
        window.location.href = _uri;
      }
    };
    _post.send(_enc);
	}
  else {
    alert("Steam Id is empty or too short");
  }
};

function Mute() {
  let _steamId = document.getElementById("PlayersSteamId").value;
	if (_steamId != "" && _steamId.length === 17) {
    let _enc = Encrypt(_clSid + " " + _cl + " " + _steamId);
    let _uri = window.location.href;
    let _post = new XMLHttpRequest();
    _post.open('POST', window.location.href.replace('st.html', 'Mute'), false);
    _post.setRequestHeader('Content-Type', 'text/plain; charset=utf-8');
    _post.onload = function () {
      if (_post.status === 200 && _post.responseText.length === 4000) {
        let _r = "";
        for (let i = 3999; i >= 0; i--) {
          _r += _post.responseText[i];
        }
        if (_r.substring(0, 8) === _clId)
        {
          _srvK = _r.substring(8);
        }
        alert(_steamId + " has been muted/unmuted");
      }
      else if (_post.status === 202) {
        let _r = "";
        for (let j = 3999; j >= 0; j--) {
          _r += _post.responseText[j];
        }
        if (_r.substring(0, 8) === _clId)
        {
          _srvK = _r.substring(8);
        }
        alert(_steamId + " was not found online");
      }
      else if (_post.status === 401) {
        window.location.href = _uri;
      }
    };
    _post.send(_enc);
	}
  else {
    alert("Steam Id is empty or too short");
  }
};

function Jail() {
  let _steamId = document.getElementById("PlayersSteamId").value;
	if (_steamId != "" && _steamId.length === 17) {
    let _enc = Encrypt(_clSid + " " + _cl + " " + _steamId);
    let _uri = window.location.href;
    let _post = new XMLHttpRequest();
    _post.open('POST', window.location.href.replace('st.html', 'Jail'), false);
    _post.setRequestHeader('Content-Type', 'text/plain; charset=utf-8');
    _post.onload = function () {
      if (_post.status === 200 && _post.responseText.length === 4000) {
        let _r = "";
        for (let i = 3999; i >= 0; i--) {
          _r += _post.responseText[i];
        }
        if (_r.substring(0, 8) === _clId)
        {
          _srvK = _r.substring(8);
        }
        alert(_steamId + " has been jailed/unjailed");
      }
      else if (_post.status === 202) {
        let _r = "";
        for (let j = 3999; j >= 0; j--) {
          _r += _post.responseText[j];
        }
        if (_r.substring(0, 8) === _clId)
        {
          _srvK = _r.substring(8);
        }
        alert(_steamId + " was not found online");
      }
      else if (_post.status === 401) {
        window.location.href = _uri;
      }
    };
    _post.send(_enc);
	}
  else {
    alert("Steam Id is empty or too short");
  }
};

function Reward() {
  let _steamId = document.getElementById("PlayersSteamId").value;
	if (_steamId != "" && _steamId.length === 17) {
    let _enc = Encrypt(_clSid + " " + _cl + " " + _steamId);
    let _uri = window.location.href;
    let _post = new XMLHttpRequest();
    _post.open('POST', window.location.href.replace('st.html', 'Reward'), false);
    _post.setRequestHeader('Content-Type', 'text/plain; charset=utf-8');
    _post.onload = function () {
      if (_post.status === 200 && _post.responseText.length === 4000) {
        let _r = "";
        for (let i = 3999; i >= 0; i--) {
          _r += _post.responseText[i];
        }
        if (_r.substring(0, 8) === _clId)
        {
          _srvK = _r.substring(8);
        }
        alert(_steamId + " has received a reward");
      }
      else if (_post.status === 202) {
        let _r = "";
        for (let j = 3999; j >= 0; j--) {
          _r += _post.responseText[j];
        }
        if (_r.substring(0, 8) === _clId)
        {
          _srvK = _r.substring(8);
        }
        alert(_steamId + " was not found online");
      }
      else if (_post.status === 401) {
        window.location.href = _uri;
      }
    };
    _post.send(_enc);
	}
  else {
    alert("Steam Id is empty or too short");
  }
};

function StartPlayerLoop() {
  _playerClock = setInterval(updatePlayers, 15000);
};

function StopPlayerClock() {
  clearTimeout(_playerClock);
};

function Encrypt(_input) {
  let _output = _input;
  let _outputLength = _output.length;
  let _kys = _srvK.match(/.{1,8}/g);
  let _pEnc = _clId;
  for (let i = 0; i < _outputLength; i++) {
    let _c = _output.charCodeAt(i);
    if (_c > 31 && _c < 127) {
      let _cBP = parseInt(_c.toString(2));
      let _kP = parseInt(_kys[(_outputLength - 1) - i]);
      let _bK = (_cBP + _kP).toString().padStart(8, '0');
      _pEnc += _bK;
    }
    else {
      alert("Invalid character used");
      return;
    }
  }
  let _f = 3992 - _pEnc.length;
  for (let j = 0; j < _f; j++) {
    _pEnc += _numSet.charAt(Math.floor(Math.random() * 10));
  }
   let _mK = _outputLength.toString().padStart(3, '0');
   for (let k = 0; k < 5; k++) {
     _mK += _numSet.charAt(Math.floor(Math.random() * 10));
   }
   _pEnc += _mK;
  let _enc = "";
  for (let l = 3999; l >= 0; l--) {
    _enc += _pEnc.charAt(l);
  }
  return _enc;
};

function SetConfigId() {
  let _count = 0;
  let _inputs = document.getElementsByName("option");
  let _length = _inputs.length;
  for (let i = 0; i < _length; i++) {
    _inputs[i].id = _count.toString();
    _count++;
  }
};
