var ClientId;
var ItemString = "";
var Items = new Object();
var PurchaseNumber = -1;

function FreshPage() {
	document.getElementById('SecuritySync').style.visibility = "visible";
	document.getElementById('Header').style.visibility = "hidden";
	document.getElementById('ContentContainer').style.visibility = "hidden";
};

function EnterAuction() {
	let secureId = document.getElementById("SecureId").value;
	if (secureId.length > 3 && secureId.length < 5) {
		if (document.getElementById("ConfirmEnter").checked) {
			let request = new XMLHttpRequest();
			request.open('POST', window.location.href.replace('auction.html', 'EnterShop'), false);
			request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
			request.onerror = function() {
				alert("Error");
			};
			request.onload = function () {
				if (request.status == 200 && request.readyState == 4) {
					ClientId = secureId;
					let responseSplit = request.responseText.split('☼');
					Pin = CryptoJS.SHA512(ClientId + responseSplit[6]).toString();
					
					document.getElementById('SecuritySync').style.visibility = "hidden";
					document.getElementById('Header').style.visibility = "visible";
					document.getElementById('ContentContainer').style.visibility = "visible";
					document.getElementById('Search').style.visibility = "visible";
					document.getElementById("PlayerName").innerHTML = responseSplit[0];
					document.getElementById("Balance").innerHTML = responseSplit[1] + " " + responseSplit[2];
					document.getElementById("ShopName").innerHTML = responseSplit[3];
					
					
				}
			};
		}
	}
};

function ExitAuction() {
	
};

function Purchase() {
	
};