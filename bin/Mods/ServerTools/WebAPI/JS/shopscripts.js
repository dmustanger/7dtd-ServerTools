var ClientId, Pin;
var Items = new Object();
var Categories = new Object();
var ActiveFilter = 0;
var PurchaseNumber = -1;
var Page = 1;
var FiltersOpen = false;

function FreshPage() {
	let request = new XMLHttpRequest();
	request.open('POST', window.location.href.replace('shop.html', 'ShopIPSync'), true);
	request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
	request.onerror = function() {
		alert("Error. Unable to communicate with server");
	};
	request.onload = function() {
		if (request.status == 200 && request.readyState == 4) {
			let responseSplit = request.responseText.split('☼');
			ClientId = responseSplit[6];
			Pin = CryptoJS.SHA512(ClientId + responseSplit[7]).toString();
			Accepted(responseSplit);
		}
		else if (request.status == 401 && request.readyState == 4) {
			//
		}
		else if (request.status == 402 && request.readyState == 4) {
			alert("Unable to sync your IP with a player in game. Check you are still in game");
		}
	};
	request.send(" ");
};

function EnterShop() {
	let secureId = document.getElementById("SecureId").value;
	if (secureId.length === 4) {
		let request = new XMLHttpRequest();
		request.open('POST', window.location.href.replace('shop.html', 'EnterShop'), true);
		request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
		request.onerror = function() {
			alert("Error. Unable to communicate with server");
		};
		request.onload = function() {
			if (request.status == 200 && request.readyState == 4) {
				ClientId = secureId;
				let responseSplit = request.responseText.split('☼');
				Pin = CryptoJS.SHA512(ClientId + responseSplit[6]).toString();
				Accepted(responseSplit);
			}
			else if (request.status == 401 && request.readyState == 4) {
				alert("Invalid security ID");
			}
			else if (request.status == 402 && request.readyState == 4) {
				alert("This security ID has expired. Please acquire a new one");
			}
			else if (request.status == 403 && request.readyState == 4) {
				alert("Server has refused entry");
			}
		};
		if (secureId != "DBUG") {
			request.send(CryptoJS.SHA512(secureId).toString());
		}
		else {
			request.send(secureId);
		}
	}
	else {
		alert("Invalid security ID. ID must be 4 chars in length");
	}
};

function Accepted(responseSplit) {
	document.getElementById('SecuritySync').style.top = "-2000px"
	document.getElementById('Header').style.top = "0px"
	document.getElementById('ContentContainer').style.top = "0px"
	document.getElementById("PlayerName").innerHTML = responseSplit[0];
	document.getElementById("Balance").innerHTML = responseSplit[1] + " " + responseSplit[2];
	document.getElementById("ShopName").innerHTML = responseSplit[3];

	let categorySplit = responseSplit[5].split('§');
	let categories = new Array(categorySplit.length + 1);
	
	categories[0] = "all";
	let firstFilter = document.createElement('button');
	firstFilter.setAttribute("class", "Filters");
	firstFilter.setAttribute("title", "all");
	firstFilter.innerHTML = "all";
	firstFilter.onclick = function() {
		Filter(0);
	};
	document.getElementById("FilterContainer").appendChild(firstFilter);
	
	for (let i = 0; i < categorySplit.length; i++) {
		let filter = document.createElement('button');
		filter.setAttribute("class", "Filters");
		filter.setAttribute("title", categorySplit[i]);
		filter.innerHTML = categorySplit[i];
		filter.onclick = function() {
			Filter(i + 1);
		};
		document.getElementById("FilterContainer").appendChild(filter);
		
		if (categorySplit[i].includes("Quality:") == true) {
			categorySplit[i] = categorySplit[i].replace("Quality:", "");
		}
		categories[i + 1] = categorySplit[i];
	}
	
	Categories = categories;
	
	let selection = document.getElementById('PerPage');
	selection.addEventListener("change", function() {
		UpdatePerPage (selection.value);
	});
	if (responseSplit[4].length > 0) {
		if (responseSplit[4].includes("╚") == true) {
			let itemsSplit = responseSplit[4].split('╚');
			for (let j = 0; j < itemsSplit.length; j++)
			{
				Items[j] = itemsSplit[j];
				
				if (j < selection.value) {
					let div = document.createElement("div");
					div.setAttribute("class", "IconContainer");
					div.setAttribute("id", j);
					div.onclick = function() {
						let number = document.getElementById("Counter").value * itemSplit[6];
						document.getElementById("Total").innerHTML = "Total: " + number;
						document.getElementById("InfoContainer").innerHTML =  itemSplit[2] + " </br> " + itemSplit[7] + " </br> " + itemSplit[9];
						PurchaseNumber = itemSplit[8];
					};
					document.getElementById("Items").appendChild(div);

					let itemSplit = itemsSplit[j].split('§');
					SetPalette(itemSplit, j);

					let img = document.createElement("img");
					img.setAttribute("src", "Icon/" + itemSplit[3] + ".png");
					img.setAttribute("class", "Icon");
					img.setAttribute("title", itemSplit[2]);
					document.getElementById(j).appendChild(img);
				}
			}
			if (itemsSplit.length > selection.value) {
				let div = document.createElement("div");
				div.setAttribute("class", "IconContainer");
				div.setAttribute("id", ">");
				div.setAttribute("title", "Next Page");
				div.onclick = function() {
					NextPage();
				};
				document.getElementById("Items").appendChild(div);
				let pageHeader = document.createElement('H3');
				pageHeader.setAttribute("class", "ItemHeader");
				pageHeader.innerHTML = "Next Page";
				document.getElementById(">").appendChild(pageHeader);
				let pageUp = document.createElement('H3');
				pageUp.setAttribute("class", "PageHeader");
				pageUp.setAttribute("title", "Next Page");
				pageUp.innerHTML = ">";
				document.getElementById(">").appendChild(pageUp);
			}
		}
		else {
			Items[0] = responseSplit[4];
			
			let div = document.createElement("div");
			div.setAttribute("class", "IconContainer");
			div.setAttribute("id", 0);
			div.onclick = function() {
				let number = document.getElementById("Counter").value * itemSplit[6];
				document.getElementById("Total").innerHTML = "Total: " + number;
				document.getElementById("InfoContainer").innerHTML = itemSplit[2] + " </br> " + itemSplit[7] + " </br> " + itemSplit[9];
				PurchaseNumber = itemSplit[8];
			};
			document.getElementById("Items").appendChild(div);

			let itemSplit = responseSplit[4].split('§');
			SetPalette(itemSplit, 0);

			let img = document.createElement("img");
			img.setAttribute("src", "Icon/" + itemSplit[3] + ".png");
			img.setAttribute("class", "Icon");
			img.setAttribute("title", itemSplit[2]);
			document.getElementById(0).appendChild(img);
		}
	}
};

function ExitShop() {
	let request = new XMLHttpRequest();
	request.open('POST', window.location.href.replace('shop.html', 'ExitShop'), true);
	request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
	request.onerror = function() {
		alert("Error. Unable to communicate with server");
	};
	request.onload = function() {
		if (request.readyState == 4) {
			window.location.href = window.location.href;
		}
	};
	if (ClientId != "DBUG") {
		request.send(Pin);
	}
	else {
		request.send(ClientId);
	}
};


function Filter(filter) {
	if (ActiveFilter != filter) {
		ActiveFilter = filter;
		document.getElementById('FilterContainer').style.top = "-2000px"
		let ItemCount = Object.keys(Items).length;
		if (ItemCount > 0) {
			document.getElementById("Items").innerHTML = "";
			document.getElementById("InfoContainer").innerHTML = "...";
			document.getElementById("Total").innerHTML = "Total: 0";
			PurchaseNumber = -1;
			Page = 1;
			let category = Categories[ActiveFilter]
			let counter1 = 0;
			let counter2 = 0;
			for (const [key, value] of Object.entries(Items)) {
				let itemSplit = value.split('§');
				if (itemSplit[0].includes(category) || category == "all" || itemSplit[5] == category) {
					let perPage = document.getElementById("PerPage").value;
					let pagePoint1 = perPage * Page;
					let pagePoint2 = pagePoint1 - perPage;
					if (counter2 < pagePoint1 && counter2 >= pagePoint2) {
						let div = document.createElement("div");
						div.setAttribute("class", "IconContainer");
						div.setAttribute("id", counter1);
						div.onclick = function() {
							let number = document.getElementById("Counter").value * itemSplit[6];
							document.getElementById("Total").innerHTML = "Total: " + number;
							document.getElementById("InfoContainer").innerHTML =  itemSplit[2] + " </br> " + itemSplit[7] + " </br> " + itemSplit[9];
							PurchaseNumber = itemSplit[8];
						};
						document.getElementById("Items").appendChild(div);
						
						SetPalette(itemSplit, counter1);

						let img = document.createElement("img");
						img.setAttribute("src", "Icon/" + itemSplit[3] + ".png");
						img.setAttribute("class", "Icon");
						img.setAttribute("title", itemSplit[2]);
						document.getElementById(counter1).appendChild(img);
					}
					counter2 += 1;
				}
				counter1 += 1;
			}
			if (counter2 > document.getElementById("PerPage").value) {
				let div = document.createElement("div");
				div.setAttribute("class", "IconContainer");
				div.setAttribute("id", ">");
				div.setAttribute("title", "Next Page");
				div.onclick = function() {
					NextPage();
				};
				document.getElementById("Items").appendChild(div);
				let pageHeader = document.createElement('H3');
				pageHeader.setAttribute("class", "ItemHeader");
				pageHeader.innerHTML = "Next Page";
				document.getElementById(">").appendChild(pageHeader);
				let pageUp = document.createElement('H3');
				pageUp.setAttribute("class", "PageHeader");
				pageUp.setAttribute("title", "Next Page");
				pageUp.innerHTML = ">";
				document.getElementById(">").appendChild(pageUp);
			}
		}
		else {
			alert("There are no items in the shop");
		}
	}
	else {
		alert("This filter is already active");
	}
};

function Purchase() {
	if (document.getElementById("ConfirmPurchase").checked) {
		if (PurchaseNumber > -1) {
			let request = new XMLHttpRequest();
			request.open('POST', window.location.href.replace('shop.html', 'ShopPurchase'), true);
			request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
			request.onerror = function() {
				alert("Error");
			};
			request.onload = function() {
				if (request.status == 200 && request.readyState == 4) {
					let responseSplit = request.responseText.split('§');
					document.getElementById("ConfirmPurchase").checked = false;
					document.getElementById("Balance").innerHTML = responseSplit[0] + " " + responseSplit[1];
					Balance = responseSplit[0];
					Pin = CryptoJS.SHA512(ClientId + responseSplit[2]).toString();
				}
				else if (request.status == 400 && request.readyState == 4) {
					alert("This security ID has expired. Please acquire a new one");
				}
				else if (request.status == 401 && request.readyState == 4) {
					document.getElementById("ConfirmPurchase").checked = false;
					Pin = CryptoJS.SHA512(ClientId + request.responseText).toString();
					alert("Unable to locate item in shop");
				}
				else if (request.status == 402 && request.readyState == 4) {
					let responseSplit = request.responseText.split('§');
					document.getElementById("ConfirmPurchase").checked = false;
					document.getElementById("Balance").innerHTML = responseSplit[0] + " " + responseSplit[1];
					Balance = responseSplit[0];
					Pin = CryptoJS.SHA512(ClientId + responseSplit[2]).toString();
					alert("Balance is too low to make this purchase");
				}
				else if (request.status == 403 && request.readyState == 4) {
					alert("No response from the server");
				}
			};
			if (ClientId != "DBUG") {
				request.send(Pin + "☼" + PurchaseNumber + "☼" + document.getElementById("Counter").value);
			}
			else {
				alert("Debug mode does not allow purchase");
			}
		}
		else {
			alert("No item selected");
		}
	}
	else {
		alert("Click the confirmation checkbox first");
	}
};

function UpdatePerPage(number) {
	let ItemCount = Object.keys(Items).length;
	if (ItemCount > 0) {
		document.getElementById("Items").innerHTML = "";
		document.getElementById("InfoContainer").innerHTML = "...";
		document.getElementById("Total").innerHTML = "Total: 0";
		PurchaseNumber = -1;
		Page = 1;
		let category = Categories[ActiveFilter]
		let count = 0;
		let pagePoint1 = number * Page;
		let pagePoint2 = pagePoint1 - number;
		for (const [key, value] of Object.entries(Items)) {
			let itemSplit = value.split('§');
			if (itemSplit[0].includes(category) || category == "all" || itemSplit[5] == category) {
				if (count < pagePoint1 && count >= pagePoint2) {
					let div = document.createElement("div");
					div.setAttribute("class", "IconContainer");
					div.setAttribute("id", count);
					div.onclick = function() {
						let number = document.getElementById("Counter").value * itemSplit[6];
						document.getElementById("Total").innerHTML = "Total: " + number;
						document.getElementById("InfoContainer").innerHTML =  itemSplit[2] + " </br> " + itemSplit[7] + " </br> " + itemSplit[9];
						PurchaseNumber = itemSplit[8];
					};
					document.getElementById("Items").appendChild(div);
					
					SetPalette(itemSplit, count);

					let img = document.createElement("img");
					img.setAttribute("src", "Icon/" + itemSplit[3] + ".png");
					img.setAttribute("class", "Icon");
					img.setAttribute("title", itemSplit[2]);
					document.getElementById(count).appendChild(img);
				}
			}
			count += 1;
		}
		if (ItemCount > document.getElementById("PerPage").value) {
			let div = document.createElement("div");
			div.setAttribute("class", "IconContainer");
			div.setAttribute("id", ">");
			div.setAttribute("title", "Next Page");
			div.onclick = function() {
				NextPage();
			};
			document.getElementById("Items").appendChild(div);
			let pageHeader = document.createElement('H3');
			pageHeader.setAttribute("class", "ItemHeader");
			pageHeader.innerHTML = "Next Page";
			document.getElementById(">").appendChild(pageHeader);
			let pageUp = document.createElement('H3');
			pageUp.setAttribute("class", "PageHeader");
			pageUp.setAttribute("title", "Next Page");
			pageUp.innerHTML = ">";
			document.getElementById(">").appendChild(pageUp);
		}
	}
	else {
		alert("There are no items in the shop");
	}
};

function NextPage() {
	document.getElementById("Items").innerHTML = "";
	document.getElementById("InfoContainer").innerHTML = "...";
	document.getElementById("Total").innerHTML = "Total: 0";
	PurchaseNumber = -1;
	Page += 1;
	let category = Categories[ActiveFilter]
	let counter1 = 0;
	let counter2 = 0;
	let perPage = document.getElementById("PerPage").value;
	let pagePoint1 = perPage * Page;
	let pagePoint2 = pagePoint1 - perPage;
	let div = document.createElement("div");
	div.setAttribute("class", "IconContainer");
	div.setAttribute("id", "<");
	div.setAttribute("title", "Last Page");
	div.onclick = function() {
		LastPage();
	};
	document.getElementById("Items").appendChild(div);
	let pageHeader = document.createElement('H3');
	pageHeader.setAttribute("class", "ItemHeader");
	pageHeader.innerHTML = "Last Page";
	document.getElementById("<").appendChild(pageHeader);
	let pageDown = document.createElement('H3');
	pageDown.setAttribute("class", "PageHeader");
	pageDown.setAttribute("title", "Last Page");
	pageDown.innerHTML = "<";
	document.getElementById("<").appendChild(pageDown);
	for (const [key, value] of Object.entries(Items)) {
		let itemSplit = value.split('§');
		if (itemSplit[0].includes(category) || category == "all" || itemSplit[5] == category) {
			if (counter2 < pagePoint1 && counter2 >= pagePoint2) {
				let div = document.createElement("div");
				div.setAttribute("class", "IconContainer");
				div.setAttribute("id", counter1);
				div.onclick = function() {
					let number = document.getElementById("Counter").value * itemSplit[6];
					document.getElementById("Total").innerHTML = "Total: " + number;
					document.getElementById("InfoContainer").innerHTML =  itemSplit[2] + " </br> " + itemSplit[7] + " </br> " + itemSplit[9];
					PurchaseNumber = itemSplit[8];
				};
				document.getElementById("Items").appendChild(div);
				
				SetPalette(itemSplit, counter1);
	
				let img = document.createElement("img");
				img.setAttribute("src", "Icon/" + itemSplit[3] + ".png");
				img.setAttribute("class", "Icon");
				img.setAttribute("title", itemSplit[2]);
				document.getElementById(counter1).appendChild(img);
			}
			counter2 += 1;
		}
		counter1 += 1;
	}
	if (counter2 > pagePoint1) {
		let div = document.createElement("div");
		div.setAttribute("class", "IconContainer");
		div.setAttribute("id", ">");
		div.setAttribute("title", "Next Page");
		div.onclick = function() {
			NextPage();
		};
		document.getElementById("Items").appendChild(div);
		let pageHeader = document.createElement('H3');
		pageHeader.setAttribute("class", "ItemHeader");
		pageHeader.innerHTML = "Next Page";
		document.getElementById(">").appendChild(pageHeader);
		let pageUp = document.createElement('H3');
		pageUp.setAttribute("class", "PageHeader");
		pageUp.setAttribute("title", "Next Page");
		pageUp.innerHTML = ">";
		document.getElementById(">").appendChild(pageUp);
	}
};

function LastPage() {
	document.getElementById("Items").innerHTML = "";
	document.getElementById("InfoContainer").innerHTML = "...";
	document.getElementById("Total").innerHTML = "Total: 0";
	PurchaseNumber = -1;
	Page -= 1;
	let category = Categories[ActiveFilter]
	let counter1 = 0;
	let counter2 = 0;
	let perPage = document.getElementById("PerPage").value;
	let pagePoint1 = perPage * Page;
	let pagePoint2 = pagePoint1 - perPage;
	if (Page > 1) {
		let div = document.createElement("div");
		div.setAttribute("class", "IconContainer");
		div.setAttribute("id", "<");
		div.setAttribute("title", "Last Page");
		div.onclick = function() {
			LastPage();
		};
		document.getElementById("Items").appendChild(div);
		let pageHeader = document.createElement('H3');
		pageHeader.setAttribute("class", "ItemHeader");
		pageHeader.innerHTML = "Last Page";
		document.getElementById("<").appendChild(pageHeader);
		let pageDown = document.createElement('H3');
		pageDown.setAttribute("class", "PageHeader");
		pageDown.setAttribute("title", "Last Page");
		pageDown.innerHTML = "<";
		document.getElementById("<").appendChild(pageDown);
	}
	for (const [key, value] of Object.entries(Items)) {
		let itemSplit = value.split('§');
		if (itemSplit[0].includes(category) || category == "all" || itemSplit[5] == category) {
			if (counter2 < pagePoint1 && counter2 >= pagePoint2) {
				let div = document.createElement("div");
				div.setAttribute("class", "IconContainer");
				div.setAttribute("id", counter1);
				div.onclick = function() {
					let number = document.getElementById("Counter").value * itemSplit[6];
					document.getElementById("Total").innerHTML = "Total: " + number;
					document.getElementById("InfoContainer").innerHTML =  itemSplit[2] + " </br> " + itemSplit[7] + " </br> " + itemSplit[9];
					PurchaseNumber = itemSplit[8];
				};
				document.getElementById("Items").appendChild(div);
				
				SetPalette(itemSplit, counter1);
	
				let img = document.createElement("img");
				img.setAttribute("src", "Icon/" + itemSplit[3] + ".png");
				img.setAttribute("class", "Icon");
				img.setAttribute("title", itemSplit[2]);
				document.getElementById(counter1).appendChild(img);
			}
			counter2 += 1;
		}
		counter1 += 1;
	}
	if (counter2 > pagePoint1) {
		let div = document.createElement("div");
		div.setAttribute("class", "IconContainer");
		div.setAttribute("id", ">");
		div.setAttribute("title", "Next Page");
		div.onclick = function() {
			NextPage();
		};
		document.getElementById("Items").appendChild(div);
		let pageHeader = document.createElement('H3');
		pageHeader.setAttribute("class", "ItemHeader");
		pageHeader.innerHTML = "Next Page";
		document.getElementById(">").appendChild(pageHeader);
		let pageUp = document.createElement('H3');
		pageUp.setAttribute("class", "PageHeader");
		pageUp.setAttribute("title", "Next Page");
		pageUp.innerHTML = ">";
		document.getElementById(">").appendChild(pageUp);
	}
};

function UpdateTotal() {
	let ItemCount = Object.keys(Items).length;
	if (ItemCount > 0) {
		for (const [key, value] of Object.entries(Items)) {
			let itemSplit = value.split('§');
			if (itemSplit[8] === PurchaseNumber) {
				let total = document.getElementById("Counter").value * itemSplit[6];
				document.getElementById("Total").innerHTML = "Total: " + total;
				break;
			}
		}
	}
};

function ShowFilters() {
	if (FiltersOpen) {
		FiltersOpen = false;
		document.getElementById("FilterContainer").style.top = "-2000px";
	}
	else if (!FiltersOpen) {
		FiltersOpen = true;
		document.getElementById("FilterContainer").style.top = "0px";
	}
};

function SetPalette(itemSplit, target) {
	let itemHeader = document.createElement('H3');
	itemHeader.setAttribute("class", "ItemHeader");
	if (itemSplit[5] == 1) {
		itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #C2C2A3'>Quality: " + itemSplit[5] + "<span>";
	}
	else if (itemSplit[5] == 2) {
		itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #FF751A'>Quality: " + itemSplit[5] + "<span>";
	}
	else if (itemSplit[5] == 3) {
		itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #E6E600'>Quality: " + itemSplit[5] + "<span>";
	}
	else if (itemSplit[5] == 4) {
		itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #33CC33'>Quality: " + itemSplit[5] + "<span>";
	}
	else if (itemSplit[5] == 5) {
		itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #005CE6'>Quality: " + itemSplit[5] + "<span>";
	}
	else if (itemSplit[5] == 6) {
		itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #B800E6'>Quality: " + itemSplit[5] + "<span>";
	}
	else if (itemSplit[5] == 7) {
		itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #FF8400'>Quality: " + itemSplit[5] + "<span>";
	}
	else if (itemSplit[5] == 8) {
		itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #E6C300'>Quality: " + itemSplit[5] + "<span>";
	}
	document.getElementById(target).appendChild(itemHeader);
};