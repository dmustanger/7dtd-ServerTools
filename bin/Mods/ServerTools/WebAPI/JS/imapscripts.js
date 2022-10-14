var ClientId, Pin, Name;
var RegionMax = 0;
var RegionMin = 0;
var Highlights = new Array();

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
			let table = document.getElementById("MapBody");
			table.innerHTML = "";
			for (let i = RegionMax; i > RegionMin; i--) {
				let row = table.insertRow(-1);
				for (let j = RegionMin; j <= RegionMax; j++)
				{
					let cell = row.insertCell(-1);
					let cellTitle = "r." + j + "." + i + ".7rg";
					let imageAddress = "Map/r." + j + "." + i + ".png";
					cell.setAttribute("title", cellTitle);
					cell.setAttribute("id", cellTitle);
					cell.setAttribute("class", "RegionCell");
					cell.style.backgroundImage = "url(Map/" + j + "/" + i + ".png)";
					cell.onclick = function() {
						if (!Highlights.includes(cellTitle)) {
							Highlights.push(cellTitle);
							document.getElementById(cellTitle).style.border = "1px solid #FFF";
						}
						else if (Highlights.includes(cellTitle)) {
							let index = Highlights.indexOf(cellTitle);
							Highlights.splice(index, 1);
							document.getElementById(cellTitle).style.border = "1px solid #000";
						}
					};
				}
			}
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

