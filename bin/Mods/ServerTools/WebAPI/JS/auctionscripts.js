var ClientId = "";
var Pin = "";
var Search = "";
var ItemString = "";
var Items = new Object();
var ItemId = -1;
var Page = 1;

function FreshPage() {
	document.getElementById('SecuritySync').style.visibility = "visible";
	document.getElementById('Header').style.visibility = "hidden";
	document.getElementById('Body').style.visibility = "hidden";
	document.getElementById('CancelItem').style.visibility = "hidden";
	document.getElementById('CancelButton').style.visibility = "hidden";
};

function EnterAuction() {
	let secureId = document.getElementById("SecureId").value;
	if (secureId.length > 3 && secureId.length < 5) {
		if (document.getElementById("ConfirmEnter").checked) {
			let request = new XMLHttpRequest();
			request.open('POST', window.location.href.replace('auction.html', 'EnterAuction'), true);
			request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
			request.onerror = function() {
				alert("Error");
			};
			request.onload = function () {
				if (request.status == 200 && request.readyState == 4) {
					ClientId = secureId;
					let responseSplit = request.responseText.split('☼');
					ItemString = responseSplit[4];
					if (ClientId != "DBUG") {
						Pin = CryptoJS.SHA512(ClientId + responseSplit[5]).toString();
					}
					else {
						Pin = ClientId;
					}
					document.getElementById('SecuritySync').style.visibility = "hidden";
					document.getElementById('Header').style.visibility = "visible";
					document.getElementById('Body').style.visibility = "visible";
					document.getElementById("PlayerName").innerHTML = responseSplit[0];
					document.getElementById("Balance").innerHTML = responseSplit[1] + " " + responseSplit[2];
					document.getElementById("AuctionName").innerHTML = responseSplit[3];
					let perPage = document.getElementById("PerPage");
					perPage.addEventListener('change', (event) => { UpdatePerPage (perPage.value) });
					let search = document.getElementById("SearchFilter");
					search.addEventListener("keydown", function(event) {
						if (event.key == "Enter") {
							RunSearch();
						}
					});
					if (ItemString == "") {
						alert("There are no items in the auction");
					}
					else {
						if (ItemString.length > 0) {
							let perPage = document.getElementById("PerPage").value;
							if (ItemString.includes("╚") == true) {
								let itemsSplit = ItemString.split('╚');
								for (let i = 0; i < itemsSplit.length; i++) {
									let itemSplit = itemsSplit[i].split('§');
									Items[itemSplit[0]] = itemsSplit[i];
									if (i < perPage) {
										let div = document.createElement("div");
										div.setAttribute("class", "IconContainer");
										div.setAttribute("id", itemSplit[0]);
										document.getElementById("Items").appendChild(div);
										let itemHeader = document.createElement('H3');
										itemHeader.setAttribute("class", "ItemHeader");
										if (itemSplit[4] == 1) {
											itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #C2C2A3'>Tier: " + itemSplit[4] + "<span>";
										}
										else if (itemSplit[4] == 2) {
											itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #FF751A'>Tier: " + itemSplit[4] + "<span>";
										}
										else if (itemSplit[4] == 3) {
											itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #E6E600'>Tier: " + itemSplit[4] + "<span>";
										}
										else if (itemSplit[4] == 4) {
											itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #33CC33'>Tier: " + itemSplit[4] + "<span>";
										}
										else if (itemSplit[4] == 5) {
											itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #005CE6'>Tier: " + itemSplit[4] + "<span>";
										}
										else if (itemSplit[4] == 6) {
											itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #B800E6'>Tier: " + itemSplit[4] + "<span>";
										}
										else if (itemSplit[4] == 7) {
											itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #FF8400'>Tier: " + itemSplit[4] + "<span>";
										}
										else if (itemSplit[4] == 8) {
											itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #E6C300'>Tier: " + itemSplit[4] + "<span>";
										}
										document.getElementById(itemSplit[0]).appendChild(itemHeader);
										let img = document.createElement("img");
										img.setAttribute("src", "Icon/" + itemSplit[7] + ".png");
										img.setAttribute("class", "Icon");
										img.setAttribute("title", itemSplit[3]);
										img.onclick = function () {
											document.getElementById("Price").innerHTML = "Price: " + itemSplit[6];
											document.getElementById("InfoContainer").innerHTML =  itemSplit[3] + " </br> " + "Count: " + itemSplit[1] + " </br> " + "Tier: " + itemSplit[4]
											+ " </br> " + "Durability: " + itemSplit[5] + " </br> " + "Price: " + itemSplit[6];
											if (itemSplit[8] == "true") {
												document.getElementById("InfoContainer").innerHTML += " </br> " + "*You own this*";
												document.getElementById('CancelItem').style.visibility = "visible";
												document.getElementById('CancelButton').style.visibility = "visible";
											}
											else {
												document.getElementById('CancelItem').style.visibility = "hidden";
												document.getElementById('CancelButton').style.visibility = "hidden";
											}
											ItemId = itemSplit[0];
										};
										document.getElementById(itemSplit[0]).appendChild(img);
									}
								}
							}
							else {
								let itemSplit = ItemString.split('§');
								Items[itemSplit[0]] = ItemString;
								let div = document.createElement("div");
								div.setAttribute("class", "IconContainer");
								div.setAttribute("id", itemSplit[0]);
								document.getElementById("Items").appendChild(div);
								let itemHeader = document.createElement('H3');
								itemHeader.setAttribute("class", "ItemHeader");
								if (itemSplit[4] == 1) {
									itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #C2C2A3'>Tier: " + itemSplit[4] + "<span>";
								}
								else if (itemSplit[4] == 2) {
									itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #FF751A'>Tier: " + itemSplit[4] + "<span>";
								}
								else if (itemSplit[4] == 3) {
									itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #E6E600'>Tier: " + itemSplit[4] + "<span>";
								}
								else if (itemSplit[4] == 4) {
									itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #33CC33'>Tier: " + itemSplit[4] + "<span>";
								}
								else if (itemSplit[4] == 5) {
									itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #005CE6'>Tier: " + itemSplit[4] + "<span>";
								}
								else if (itemSplit[4] == 6) {
									itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #B800E6'>Tier: " + itemSplit[4] + "<span>";
								}
								else if (itemSplit[4] == 7) {
									itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #FF8400'>Tier: " + itemSplit[4] + "<span>";
								}
								else if (itemSplit[4] == 8) {
									itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #E6C300'>Tier: " + itemSplit[4] + "<span>";
								}
								document.getElementById(itemSplit[0]).appendChild(itemHeader);
								let img = document.createElement("img");
								img.setAttribute("src", "Icon/" + itemSplit[7] + ".png");
								img.setAttribute("class", "Icon");
								img.setAttribute("title", itemSplit[3]);
								img.onclick = function () {
									document.getElementById("Price").innerHTML = "Price: " + itemSplit[6];
									document.getElementById("InfoContainer").innerHTML =  itemSplit[3] + " </br> " + "Count: " + itemSplit[1] + " </br> " + "Tier: " + itemSplit[4]
									+ " </br> " + "Durability: " + itemSplit[5] + " </br> " + "Price: " + itemSplit[6];
									if (itemSplit[8] == "true") {
										document.getElementById("InfoContainer").innerHTML += " </br> " + "*You own this*";
										document.getElementById('CancelItem').style.visibility = "visible";
										document.getElementById('CancelButton').style.visibility = "visible";
									}
									else {
										document.getElementById('CancelItem').style.visibility = "hidden";
										document.getElementById('CancelButton').style.visibility = "hidden";
									}
									ItemId = itemSplit[0];
								};
								document.getElementById(itemSplit[0]).appendChild(img);
							}
						}
					}
				}
			};
			if (secureId != "DBUG") {
				request.send(CryptoJS.SHA512(secureId).toString());
			}
			else {
				request.send(secureId);
			}
		}
	}
	else {
		alert("Invalid security ID. ID must be 4 chars in length");
	}
};

function ExitAuction() {
	let request = new XMLHttpRequest();
	request.open('POST', window.location.href.replace('auction.html', 'ExitAuction'), true);
	request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
	request.onerror = function() {
		alert("Error");
	};
	request.onload = function () {
		if (request.status == 200 && request.readyState == 4) {
			ClientId = "";
			Pin = "";
			ItemString = "";
			Items = new Object();
			ItemId = -1;
			window.location.href = "";
		}
	};
	request.send(Pin);
};

function UpdatePerPage(perPage) {
	document.getElementById("Items").innerHTML = "";
	document.getElementById("InfoContainer").innerHTML = "...";
	document.getElementById("Price").innerHTML = "Price: 0";
	document.getElementById('CancelItem').style.visibility = "hidden";
	document.getElementById('CancelButton').style.visibility = "hidden";
	ItemId = -1;
	Page = 1;
	let counter1 = 0;
	let counter2 = 0;
	let pagePoint1 = perPage * Page;
	let pagePoint2 = pagePoint1 - perPage;
	if (Search == "") {
		let ItemCount = Object.keys(Items).length;
		if (ItemCount > 0) {
			for (const [key, value] of Object.entries(Items)) {
				if (counter1 < pagePoint1 && counter1 >= pagePoint2) {
					let itemSplit = value.split('§');
					let div = document.createElement("div");
					div.setAttribute("class", "IconContainer");
					div.setAttribute("id", itemSplit[0]);
					document.getElementById("Items").appendChild(div);
					let itemHeader = document.createElement('H3');
					itemHeader.setAttribute("class", "ItemHeader");
					if (itemSplit[4] == 1) {
						itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #C2C2A3'>Tier: " + itemSplit[4] + "<span>";
					}
					else if (itemSplit[4] == 2) {
						itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #FF751A'>Tier: " + itemSplit[4] + "<span>";
					}
					else if (itemSplit[4] == 3) {
						itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #E6E600'>Tier: " + itemSplit[4] + "<span>";
					}
					else if (itemSplit[4] == 4) {
						itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #33CC33'>Tier: " + itemSplit[4] + "<span>";
					}
					else if (itemSplit[4] == 5) {
						itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #005CE6'>Tier: " + itemSplit[4] + "<span>";
					}
					else if (itemSplit[4] == 6) {
						itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #B800E6'>Tier: " + itemSplit[4] + "<span>";
					}
					else if (itemSplit[4] == 7) {
						itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #FF8400'>Tier: " + itemSplit[4] + "<span>";
					}
					else if (itemSplit[4] == 8) {
						itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #E6C300'>Tier: " + itemSplit[4] + "<span>";
					}
					document.getElementById(itemSplit[0]).appendChild(itemHeader);
					let img = document.createElement("img");
					img.setAttribute("src", "Icon/" + itemSplit[7] + ".png");
					img.setAttribute("class", "Icon");
					img.setAttribute("title", itemSplit[3]);
					img.onclick = function () {
						document.getElementById("Price").innerHTML = "Price: " + itemSplit[6];
						document.getElementById("InfoContainer").innerHTML =  itemSplit[3] + " </br> " + "Count: " + itemSplit[1] + " </br> " + "Tier: " + itemSplit[4]
						+ " </br> " + "Durability: " + itemSplit[5] + " </br> " + "Price: " + itemSplit[6];
						if (itemSplit[8] == "true") {
							document.getElementById("InfoContainer").innerHTML += " </br> " + "*You own this*";
							document.getElementById('CancelItem').style.visibility = "visible";
							document.getElementById('CancelButton').style.visibility = "visible";
						}
						else {
							document.getElementById('CancelItem').style.visibility = "hidden";
							document.getElementById('CancelButton').style.visibility = "hidden";
						}
						ItemId = itemSplit[0];
					};
					document.getElementById(itemSplit[0]).appendChild(img);
				}
				counter1 += 1;
			}
		}
		else {
			alert("There are no items in the auction");
		}
	}
	else {
		let ItemCount = Object.keys(Items).length;
		if (ItemCount > 0) {
			for (const [key, value] of Object.entries(Items)) {
				let itemSplit = value.split('§');
				if (itemSplit[2].includes(Search) || itemSplit[3].includes(Search)) {
					if (counter2 < pagePoint1 && counter2 >= pagePoint2) {
						let div = document.createElement("div");
						div.setAttribute("class", "IconContainer");
						div.setAttribute("id", itemSplit[0]);
						document.getElementById("Items").appendChild(div);
						let itemHeader = document.createElement('H3');
						itemHeader.setAttribute("class", "ItemHeader");
						let itemSplit = itemsSplit[counter1].split('§');
						if (itemSplit[4] == 1) {
							itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #C2C2A3'>Tier: " + itemSplit[4] + "<span>";
						}
						else if (itemSplit[4] == 2) {
							itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #FF751A'>Tier: " + itemSplit[4] + "<span>";
						}
						else if (itemSplit[4] == 3) {
							itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #E6E600'>Tier: " + itemSplit[4] + "<span>";
						}
						else if (itemSplit[4] == 4) {
							itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #33CC33'>Tier: " + itemSplit[4] + "<span>";
						}
						else if (itemSplit[4] == 5) {
							itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #005CE6'>Tier: " + itemSplit[4] + "<span>";
						}
						else if (itemSplit[4] == 6) {
							itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #B800E6'>Tier: " + itemSplit[4] + "<span>";
						}
						else if (itemSplit[4] == 7) {
							itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #FF8400'>Tier: " + itemSplit[4] + "<span>";
						}
						else if (itemSplit[4] == 8) {
							itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #E6C300'>Tier: " + itemSplit[4] + "<span>";
						}
						document.getElementById(itemSplit[0]).appendChild(itemHeader);
						let img = document.createElement("img");
						img.setAttribute("src", "Icon/" + itemSplit[7] + ".png");
						img.setAttribute("class", "Icon");
						img.setAttribute("title", itemSplit[3]);
						img.onclick = function () {
							document.getElementById("Price").innerHTML = "Price: " + itemSplit[6];
							document.getElementById("InfoContainer").innerHTML =  itemSplit[3] + " </br> " + "Count: " + itemSplit[1] + " </br> " + "Tier: " + itemSplit[4]
							+ " </br> " + "Durability: " + itemSplit[5] + " </br> " + "Price: " + itemSplit[6];
							if (itemSplit[8] == "true") {
								document.getElementById("InfoContainer").innerHTML += " </br> " + "*You own this*";
								document.getElementById('CancelItem').style.visibility = "visible";
								document.getElementById('CancelButton').style.visibility = "visible";
							}
							else {
								document.getElementById('CancelItem').style.visibility = "hidden";
								document.getElementById('CancelButton').style.visibility = "hidden";
							}
							ItemId = itemSplit[0];
						};
						document.getElementById(itemSplit[0]).appendChild(img);
						counter2 += 1;
					}
				}
				counter1 += 1;
			}
		}
		else {
			alert("There are no items in the auction");
		}
	}
};

function Cancel() {
	if (document.getElementById("CancelItem").checked) {
		if (ItemId != -1) {
			if (Items[ItemId] != undefined) {
				let item = Items[ItemId];
				let itemSplit = item.split('§');
				if (itemSplit[8] == "true") {
					let request = new XMLHttpRequest();
					request.open('POST', window.location.href.replace('auction.html', 'AuctionCancel'), true);
					request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
					request.onerror = function() {
						alert("Error");
					};
					request.onload = function () {
						if (request.status == 200 && request.readyState == 4) {
							Pin = CryptoJS.SHA512(ClientId + request.responseText).toString();
							delete Items[ItemId];
							document.getElementById(ItemId).style.visibility = "hidden";
							document.getElementById('CancelItem').style.visibility = "hidden";
							document.getElementById('CancelButton').style.visibility = "hidden";
							ItemId = -1;
							alert("Item has been cancelled successfully");
						}
						else if (request.status == 400 && request.readyState == 4) {
							alert("Your security ID has expired. Please acquire a new one and relog");
							ExitAuction();
						}
						else if (request.status == 401 && request.readyState == 4) {
							Pin = CryptoJS.SHA512(ClientId + request.responseText).toString();
							delete Items[ItemId];
							document.getElementById(ItemId).style.visibility = "hidden";
							ItemId = -1;
							alert("This item is no longer in the auction");
						}
						else if (request.status == 402 && request.readyState == 4) {
							let responseSplit = request.responseText.split('☼');
							Pin = CryptoJS.SHA512(ClientId + responseSplit[0]).toString();
							alert(responseSplit[1]);
						}
					};
					request.send(Pin + "☼" + ItemId);
				}
				else {
					alert("You can only cancel an item that you own");
				}
			}
		}
		else {
			alert("You must select an item to cancel");
		}
	}
	else {
		alert("Click the confirmation checkbox first");
	}
};

function Purchase() {
	if (document.getElementById("ConfirmPurchase").checked) {
		if (ItemId != -1) {
			let request = new XMLHttpRequest();
			request.open('POST', window.location.href.replace('auction.html', 'AuctionPurchase'), true);
			request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
			request.onerror = function() {
				alert("Error");
			};
			request.onload = function () {
				if (request.status == 200 && request.readyState == 4) {
					Pin = CryptoJS.SHA512(ClientId + request.responseText).toString();
					delete Items[ItemId];
					document.getElementById(ItemId).style.visibility = "hidden";
					ItemId = -1;
				}
				else if (request.status == 400 && request.readyState == 4) {
					alert("Your security ID has expired. Please acquire a new one and relog");
					ExitAuction();
				}
				else if (request.status == 401 && request.readyState == 4) {
					Pin = CryptoJS.SHA512(ClientId + request.responseText).toString();
					delete Items[ItemId];
					document.getElementById(ItemId).style.visibility = "hidden";
					ItemId = -1;
					alert("This item is no longer in the auction");
				}
				else if (request.status == 402 && request.readyState == 4) {
					let responseSplit = request.responseText.split('☼');
					Pin = CryptoJS.SHA512(ClientId + responseSplit[0]).toString();
					alert(responseSplit[1]);
				}
			};
			request.send(Pin + "☼" + ItemId);
		}
		else {
			alert("You must select an item to purchase");
		}
	}
	else {
		alert("Click the confirmation checkbox first");
	}
};

function RunSearch() {
	let search = document.getElementById("SearchFilter").value;
	if (search != "") {
		let ItemCount = Object.keys(Items).length;
		if (ItemCount > 0) {
			Search = search.toLowerCase();
			document.getElementById("Items").innerHTML = "";
			document.getElementById("InfoContainer").innerHTML = "...";
			document.getElementById("Price").innerHTML = "Price: 0";
			document.getElementById('CancelItem').style.visibility = "hidden";
			document.getElementById('CancelButton').style.visibility = "hidden";
			ItemId = -1;
			Page = 1;
			let counter1 = 0;
			let counter2 = 0;
			let perPage = document.getElementById("PerPage").value;
			let pagePoint1 = perPage * Page;
			let pagePoint2 = pagePoint1 - perPage;
			for (const [key, value] of Object.entries(Items)) {
				let itemSplit = value.split('§');
				let name = itemSplit[2].toLowerCase();
				let localName = itemSplit[3].toLowerCase();
				if (name.includes(Search) || localName.includes(Search) || itemSplit[4] == Search) {
					if (counter2 < pagePoint1 && counter2 >= pagePoint2) {
						let div = document.createElement("div");
						div.setAttribute("class", "IconContainer");
						div.setAttribute("id", itemSplit[0]);
						document.getElementById("Items").appendChild(div);
						let itemHeader = document.createElement('H3');
						itemHeader.setAttribute("class", "ItemHeader");
						if (itemSplit[4] == 1) {
							itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #C2C2A3'>Tier: " + itemSplit[4] + "<span>";
						}
						else if (itemSplit[4] == 2) {
							itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #FF751A'>Tier: " + itemSplit[4] + "<span>";
						}
						else if (itemSplit[4] == 3) {
							itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #E6E600'>Tier: " + itemSplit[4] + "<span>";
						}
						else if (itemSplit[4] == 4) {
							itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #33CC33'>Tier: " + itemSplit[4] + "<span>";
						}
						else if (itemSplit[4] == 5) {
							itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #005CE6'>Tier: " + itemSplit[4] + "<span>";
						}
						else if (itemSplit[4] == 6) {
							itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #B800E6'>Tier: " + itemSplit[4] + "<span>";
						}
						else if (itemSplit[4] == 7) {
							itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #FF8400'>Tier: " + itemSplit[4] + "<span>";
						}
						else if (itemSplit[4] == 8) {
							itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #E6C300'>Tier: " + itemSplit[4] + "<span>";
						}
						document.getElementById(itemSplit[0]).appendChild(itemHeader);
						let img = document.createElement("img");
						img.setAttribute("src", "Icon/" + itemSplit[7] + ".png");
						img.setAttribute("class", "Icon");
						img.setAttribute("title", itemSplit[3]);
						img.onclick = function () {
							document.getElementById("Price").innerHTML = "Price: " + itemSplit[6];
							document.getElementById("InfoContainer").innerHTML =  itemSplit[3] + " </br> " + "Count: " + itemSplit[1] + " </br> " + "Tier: " + itemSplit[4]
							+ " </br> " + "Durability: " + itemSplit[5] + " </br> " + "Price: " + itemSplit[6];
							if (itemSplit[8] == "true") {
								document.getElementById("InfoContainer").innerHTML += " </br> " + "*You own this*";
								document.getElementById('CancelItem').style.visibility = "visible";
								document.getElementById('CancelButton').style.visibility = "visible";
							}
							else {
								document.getElementById('CancelItem').style.visibility = "hidden";
								document.getElementById('CancelButton').style.visibility = "hidden";
							}
							ItemId = itemSplit[0];
						};
						document.getElementById(itemSplit[0]).appendChild(img);
					}
					counter2 += 1;
				}
				counter1 += 1;
			}
		}
		else {
			alert("There are no items in the auction");
		}
	}
	else {
		let ItemCount = Object.keys(Items).length;
		if (ItemCount > 0) {
			Search = "";
			document.getElementById("Items").innerHTML = "";
			document.getElementById("InfoContainer").innerHTML = "...";
			document.getElementById("Price").innerHTML = "Price: 0";
			document.getElementById('CancelItem').style.visibility = "hidden";
			document.getElementById('CancelButton').style.visibility = "hidden";
			ItemId = -1;
			Page = 1;
			let counter1 = 0;
			let perPage = document.getElementById("PerPage").value;
			for (const [key, value] of Object.entries(Items)) {
				if (counter1 < perPage) {
					let itemSplit = value.split('§');
					let div = document.createElement("div");
					div.setAttribute("class", "IconContainer");
					div.setAttribute("id", itemSplit[0]);
					document.getElementById("Items").appendChild(div);
					let itemHeader = document.createElement('H3');
					itemHeader.setAttribute("class", "ItemHeader");
					if (itemSplit[4] == 1) {
						itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #C2C2A3'>Tier: " + itemSplit[4] + "<span>";
					}
					else if (itemSplit[4] == 2) {
						itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #FF751A'>Tier: " + itemSplit[4] + "<span>";
					}
					else if (itemSplit[4] == 3) {
						itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #E6E600'>Tier: " + itemSplit[4] + "<span>";
					}
					else if (itemSplit[4] == 4) {
						itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #33CC33'>Tier: " + itemSplit[4] + "<span>";
					}
					else if (itemSplit[4] == 5) {
						itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #005CE6'>Tier: " + itemSplit[4] + "<span>";
					}
					else if (itemSplit[4] == 6) {
						itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #B800E6'>Tier: " + itemSplit[4] + "<span>";
					}
					else if (itemSplit[4] == 7) {
						itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #FF8400'>Tier: " + itemSplit[4] + "<span>";
					}
					else if (itemSplit[4] == 8) {
						itemHeader.innerHTML = itemSplit[3] + "</br> Count: " + itemSplit[1] + "</br> <span style='color: #E6C300'>Tier: " + itemSplit[4] + "<span>";
					}
					document.getElementById(itemSplit[0]).appendChild(itemHeader);
					let img = document.createElement("img");
					img.setAttribute("src", "Icon/" + itemSplit[7] + ".png");
					img.setAttribute("class", "Icon");
					img.setAttribute("title", itemSplit[3]);
					img.onclick = function () {
						document.getElementById("Price").innerHTML = "Price: " + itemSplit[6];
						document.getElementById("InfoContainer").innerHTML =  itemSplit[3] + " </br> " + "Count: " + itemSplit[1] + " </br> " + "Tier: " + itemSplit[4]
						+ " </br> " + "Durability: " + itemSplit[5] + " </br> " + "Price: " + itemSplit[6];
						if (itemSplit[8] == "true") {
							document.getElementById("InfoContainer").innerHTML += " </br> " + "*You own this*";
							document.getElementById('CancelItem').style.visibility = "visible";
							document.getElementById('CancelButton').style.visibility = "visible";
						}
						else {
							document.getElementById('CancelItem').style.visibility = "hidden";
							document.getElementById('CancelButton').style.visibility = "hidden";
						}
						ItemId = itemSplit[0];
					};
					document.getElementById(itemSplit[0]).appendChild(img);
				}
				counter1 += 1;
			}
		}
		else {
			alert("There are no items in the auction");
		}
	}
};

