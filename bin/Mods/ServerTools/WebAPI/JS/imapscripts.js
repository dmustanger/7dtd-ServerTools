var ClientId, Pin, Name, SelectedRegion;
var RegionMax = 0;
var RegionMin = 0;
var X = 0;
var Y = 0;
var XPos = 0;
var YPos = 0;
var XPosLock = 0;
var YPosLock = 0;
var Highlighted = "";
var TabState = true;
var GridState = true;
var ZoomState = false;
var ZoomGridState = true;

function FreshPage() {
	let request = new XMLHttpRequest();
	request.open('POST', window.location.href.replace('imap.html', 'MapIPSync'), true);
	request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
	request.onerror = function() {
		alert("Error. Unable to communicate with server");
	};
	request.onload = function() {
		if (request.status == 200 && request.readyState == 4) {
			let responseSplit = request.responseText.split('☼');
			Name = responseSplit[0];
			ClientId = responseSplit[1];
			Pin = CryptoJS.SHA512(ClientId + responseSplit[2]).toString();
			RegionMax = parseInt(responseSplit[3]);
			RegionMin = parseInt(responseSplit[3]) * -1;
			let remapImage = window.location.href.replace('imap.html', 'Map/');
			let mapTable = document.getElementById("MapBody");
			let gridTable = document.getElementById("GridBody");
			mapTable.innerHTML = "";
			gridTable.innerHTML = "";
			for (let i = RegionMax; i >= RegionMin; i--) {
				let gridRow = gridTable.insertRow(-1);
				let mapRow = mapTable.insertRow(-1);
				for (let j = RegionMin; j <= RegionMax; j++)
				{
					let gridCell = gridRow.insertCell(-1);
					let mapCell = mapRow.insertCell(-1);
					let gridCellTitle = "r." + j + "." + i + ".7rg";
					let mapCellTitle = j + "/" + i;
					mapCell.setAttribute("title", mapCellTitle);
					mapCell.setAttribute("id", mapCellTitle);
					mapCell.setAttribute("class", "MapCell");
					mapCell.style.backgroundImage = "url(Map/2/" + mapCellTitle + ".png)";
					gridCell.setAttribute("title", gridCellTitle);
					gridCell.setAttribute("id", gridCellTitle);
					gridCell.setAttribute("class", "GridCell");
					gridCell.onclick = function() {
						Highlight(gridCellTitle);
					};
				}
			}
			let offsetCell = document.getElementById("0/-1");
			X = offsetCell.offsetLeft;
			Y = offsetCell.offsetTop;
			window.scroll(X, Y);
			document.getElementById("Target").style.left = (X - 14) + "px";
			document.getElementById("Target").style.top = (Y - 14) + "px";
			let listener1 = document.getElementById("MapContainer");
			listener1.addEventListener("mousemove", MousePos);
			let listener2 = document.getElementById("Tab1");
			listener2.addEventListener("click", Zoom);
			let listener3 = document.getElementById("Tab2");
			listener3.addEventListener("click", OpenCloseTab);
		}
		else if (request.status == 401 && request.readyState == 4) {
			//
		}
		else if (request.status == 402 && request.readyState == 4) {
			alert("Unable to sync your IP with a player in game. Check you are still in game and try again");
		}
	};
	request.send(" ");
};

function Highlight(gridCellTitle) {
	document.getElementById("LockedX").innerHTML = XPos;
	document.getElementById("LockedY").innerHTML = YPos;
	document.getElementById("Target").style.left = XPosLock + "px";
	document.getElementById("Target").style.top = YPosLock + "px";
	if (Highlighted !== gridCellTitle) {
		if (XPos === 0 && YPos === 0) {
			gridCellTitle = "r.0.0.7rg";
		}
		document.getElementById("RegionFile").innerHTML = gridCellTitle;
		if (Highlighted !== "") {
			document.getElementById(Highlighted).style.border = "1px solid #000";
		}
		document.getElementById(gridCellTitle).style.border = "1px solid #FFF";
		Highlighted = gridCellTitle;
		if (ZoomState) {
			let id = gridCellTitle.replace("r.", "");
			id = id.replace(".7rg", "");
			let idSplit = id.split(".");
			let id1 = idSplit[0] * 4;
			let id2 = idSplit[1] * 4;
			for (let i = 0; i < 4; i++) {
				for (let j = 0; j < 4; j++) {
					document.getElementById(id1).style.backgroundImage = "url(Map/4/" + id1 + "/" + id2 + ".png)";
					id2 += 1;
				}
				id1 += 1;
				id2 -= 4;
			}
		}
	}
};

function MousePos(evt) {
	XPos = Math.round(evt.pageX - X) * 4;
	XPosLock = evt.pageX - 14;
	document.getElementById("PositionX").innerHTML = XPos;
	YPos = Math.round(evt.pageY - Y) * 4;
	YPosLock = evt.pageY - 14;
	document.getElementById("PositionY").innerHTML = YPos;
};

function OpenCloseTab(evt) {
	if (TabState) {
		document.getElementById("Controller").style.height = "0px";
		document.getElementById("Tab2").style.bottom = "0px";
		TabState = false;
	}
	else if (!TabState) {
		document.getElementById("Controller").style.height = "100px";
		document.getElementById("Tab2").style.bottom = "100px";
		TabState = true;
	}
};

function Grid() {
	if (GridState) {
		document.getElementById("GridTable").style.opacity = "0.0";
		GridState = false;
	}
	else if (!GridState) {
		document.getElementById("GridTable").style.opacity = "0.5";
		GridState = true;
	}
};

function Zoom() {
	if (!ZoomState) {
		document.getElementById("ZoomContainer").style.top = "0px";
		document.getElementById("Tab1").style.top = "514px";
		ZoomState = true;
	}
	else if (ZoomState) {
		document.getElementById("ZoomContainer").style.top = "-600px";
		document.getElementById("Tab1").style.top = "0px";
		ZoomState = false;
	}
};