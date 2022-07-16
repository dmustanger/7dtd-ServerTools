var ClientId = "";
var Pin = "";
var Items = new Object();
var Categories = new Object();
var ActiveFilter = 0;
var PurchaseNumber = -1;
var Page = 1;

function FreshPage() {
	document.getElementById('SecuritySync').style.visibility = "visible";
	document.getElementById('Header').style.visibility = "hidden";
	document.getElementById('FilterContainer').style.visibility = "hidden";
	document.getElementById('ContentContainer').style.visibility = "hidden";
	document.getElementById('Filters').style.visibility = "hidden";
};

function EnterShop() {
	let secureId = document.getElementById("SecureId").value;
	if (secureId.length > 3 && secureId.length < 5) {
		if (document.getElementById("ConfirmEnter").checked) {
			let request = new XMLHttpRequest();
			request.open('POST', window.location.href.replace('shop.html', 'EnterShop'), false);
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
					document.getElementById('FilterContainer').style.visibility = "visible";
					document.getElementById('ContentContainer').style.visibility = "visible";
					document.getElementById('Filters').style.visibility = "visible";
					document.getElementById("PlayerName").innerHTML = responseSplit[0];
					document.getElementById("Balance").innerHTML = responseSplit[1] + " " + responseSplit[2];
					document.getElementById("ShopName").innerHTML = responseSplit[3];

					let categorySplit = responseSplit[5].split('§');
					let categories = new Array(categorySplit.length + 1);
					
					categories[0] = "all";
					let firstFilter = document.createElement('H4');
					firstFilter.setAttribute("class", "FilterIcons");
					firstFilter.setAttribute("id", "FilterIcon0");
					firstFilter.setAttribute("title", "all");
					firstFilter.innerHTML = 0;
					firstFilter.onclick = function () {
						Filter(0);
					};
					document.getElementById("FilterContainer").appendChild(firstFilter);
					
					for (let i = 0; i < categorySplit.length; i++) {
						let filter = document.createElement('H4');
						filter.setAttribute("class", "FilterIcons");
						filter.setAttribute("id", "FilterIcon" + i + 1);
						filter.setAttribute("title", categorySplit[i]);
						filter.innerHTML = i + 1;
						filter.onclick = function () {
							Filter(i + 1);
						};
						document.getElementById("FilterContainer").appendChild(filter);
						
						if (categorySplit[i].includes("Tier:") == true) {
							categorySplit[i] = categorySplit[i].replace("Tier:", "");
						}
						categories[i + 1] = categorySplit[i];
					}
					
					Categories = categories;
					
					let selection = document.createElement('select');
					selection.setAttribute("id", "PerPage");
					selection.setAttribute("title", "Items Per Page");
					selection.addEventListener('change', (event) => { UpdatePerPage (selection.value) });
					document.getElementById("FilterContainer").appendChild(selection);
					let option20 = document.createElement('option');
					option20.value = 20;
					option20.text = 20;
					document.getElementById("PerPage").appendChild(option20);
					let option15 = document.createElement('option');
					option15.value = 15;
					option15.text = 15;
					document.getElementById("PerPage").appendChild(option15);
					let option10 = document.createElement('option');
					option10.value = 10;
					option10.text = 10;
					document.getElementById("PerPage").appendChild(option10);
					let option5 = document.createElement('option');
					option5.value = 5;
					option5.text = 5;
					document.getElementById("PerPage").appendChild(option5);
					if (responseSplit[4].length > 0) {
						if (responseSplit[4].includes("╚") == true) {
							let perPage = document.getElementById("PerPage").value;
							let itemsSplit = responseSplit[4].split('╚');
							for (let j = 0; j < itemsSplit.length; j++)
							{
								Items[j] = itemsSplit[j];
								
								if (j < perPage) {
									let div = document.createElement("div");
									div.setAttribute("class", "IconContainer");
									div.setAttribute("id", j);
									document.getElementById("Items").appendChild(div);
									
									let itemHeader = document.createElement('H3');
									itemHeader.setAttribute("class", "ItemHeader");
									
									let itemSplit = itemsSplit[j].split('§');
									if (itemSplit[5] == 1) {
										itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #C2C2A3'>Tier: " + itemSplit[5] + "<span>";
									}
									else if (itemSplit[5] == 2) {
										itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #FF751A'>Tier: " + itemSplit[5] + "<span>";
									}
									else if (itemSplit[5] == 3) {
										itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #E6E600'>Tier: " + itemSplit[5] + "<span>";
									}
									else if (itemSplit[5] == 4) {
										itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #33CC33'>Tier: " + itemSplit[5] + "<span>";
									}
									else if (itemSplit[5] == 5) {
										itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #005CE6'>Tier: " + itemSplit[5] + "<span>";
									}
									else if (itemSplit[5] == 6) {
										itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #B800E6'>Tier: " + itemSplit[5] + "<span>";
									}
									else if (itemSplit[5] == 7) {
										itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #FF8400'>Tier: " + itemSplit[5] + "<span>";
									}
									else if (itemSplit[5] == 8) {
										itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #E6C300'>Tier: " + itemSplit[5] + "<span>";
									}
									document.getElementById(j).appendChild(itemHeader);
									
									let img = document.createElement("img");
									img.setAttribute("src", "Icon/" + itemSplit[3] + ".png");
									img.setAttribute("class", "Icon");
									img.setAttribute("title", itemSplit[2]);
									img.onclick = function () {
										document.getElementById("Total").innerHTML = "Total: " + document.getElementById("Counter").value * itemSplit[6];
										document.getElementById("InfoContainer").innerHTML =  itemSplit[2] + " </br> " + itemSplit[7] + " </br> " + itemSplit[9];
										PurchaseNumber = itemSplit[8];
									};
									document.getElementById(j).appendChild(img);
								}
							}
							if (itemsSplit.length > perPage) {
								let div = document.createElement("div");
								div.setAttribute("class", "IconContainer");
								div.setAttribute("id", ">");
								div.setAttribute("title", "Next Page");
								document.getElementById("Items").appendChild(div);
								let pageHeader = document.createElement('H3');
								pageHeader.setAttribute("class", "ItemHeader");
								pageHeader.innerHTML = "Next Page";
								document.getElementById(">").appendChild(pageHeader);
								let pageUp = document.createElement('H3');
								pageUp.setAttribute("class", "PageHeader");
								pageUp.setAttribute("title", "Next Page");
								pageUp.innerHTML = ">";
								pageUp.onclick = function () {
									NextPage();
								};
								document.getElementById(">").appendChild(pageUp);
							}
						}
						else {
							Items[0] = responseSplit[4];
							
							let div = document.createElement("div");
							div.setAttribute("class", "IconContainer");
							div.setAttribute("id", 0);
							document.getElementById("Items").appendChild(div);

							let itemHeader = document.createElement('H3');
							itemHeader.setAttribute("class", "ItemHeader");
							
							let itemSplit = ItemString.split('§');
							if (itemSplit[5] == 1) {
								itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #C2C2A3'>Tier: " + itemSplit[5] + "<span>";
							}
							else if (itemSplit[5] == 2) {
								itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #FF751A'>Tier: " + itemSplit[5] + "<span>";
							}
							else if (itemSplit[5] == 3) {
								itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #E6E600'>Tier: " + itemSplit[5] + "<span>";
							}
							else if (itemSplit[5] == 4) {
								itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #33CC33'>Tier: " + itemSplit[5] + "<span>";
							}
							else if (itemSplit[5] == 5) {
								itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #005CE6'>Tier: " + itemSplit[5] + "<span>";
							}
							else if (itemSplit[5] == 6) {
								itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #B800E6'>Tier: " + itemSplit[5] + "<span>";
							}
							else if (itemSplit[5] == 7) {
								itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #FF8400'>Tier: " + itemSplit[5] + "<span>";
							}
							else if (itemSplit[5] == 8) {
								itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #E6C300'>Tier: " + itemSplit[5] + "<span>";
							}
							document.getElementById(0).appendChild(itemHeader);

							let img = document.createElement("img");
							img.setAttribute("src", "Icon/" + itemSplit[3] + ".png");
							img.setAttribute("class", "Icon");
							img.setAttribute("title", itemSplit[2]);
							img.onclick = function () {
								document.getElementById("Total").innerHTML = "Total: " + document.getElementById("Counter").value * itemSplit[6];
								document.getElementById("InfoContainer").innerHTML = itemSplit[2] + " </br> " + itemSplit[7] + " </br> " + itemSplit[9];
								PurchaseNumber = itemSplit[8];
							};
							document.getElementById(0).appendChild(img);
						}
					}
				}
				else if (request.status == 401 && request.readyState == 4) {
					alert("Invalid security ID");
				}
				else if (request.status == 402 && request.readyState == 4) {
					alert("This security ID has expired. Please acquire a new one");
				}
				else if (request.status == 403 && request.readyState == 4) {
					alert("No response from the server");
				}
			};
			if (secureId != "DBUG") {
				request.send(CryptoJS.SHA512(secureId).toString())
			}
			else {
				request.send(secureId)
			}
		}
		else {
			alert("Click the confirmation checkbox first");
		}
	}
	else {
		alert("Invalid security ID. ID must be 4 chars in length");
	}
};



function ExitShop() {
	if (document.getElementById("ConfirmExit").checked) {
		let request = new XMLHttpRequest();
		request.open('POST', window.location.href.replace('shop.html', 'ExitShop'), false);
		request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
		request.onerror = function() {
			alert("Error");
		};
		request.onload = function () {
			if (request.status == 200 && request.readyState == 4) {
				ClientId = "";
				Pin = "";
				Items = new Object();
				Categories = new Object();
				ActiveFilter = 0;
				PurchaseNumber = -1;
				Page = 1;
				PerPage = 10;
				window.location.href = "";
			}
		};
		if (ClientId != "DBUG") {
			request.send(Pin);
		}
		else {
			request.send(ClientId);
		}
	}
	else {
		alert("Click the confirmation checkbox first");
	}
};


function Filter(filter) {
	if (ActiveFilter != filter) {
		ActiveFilter = filter;
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
						document.getElementById("Items").appendChild(div);
						let itemHeader = document.createElement('H3');
						itemHeader.setAttribute("class", "ItemHeader");
						if (itemSplit[5] == 1) {
							itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #C2C2A3'>Tier: " + itemSplit[5] + "<span>";
						}
						else if (itemSplit[5] == 2) {
							itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #FF751A'>Tier: " + itemSplit[5] + "<span>";
						}
						else if (itemSplit[5] == 3) {
							itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #E6E600'>Tier: " + itemSplit[5] + "<span>";
						}
						else if (itemSplit[5] == 4) {
							itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #33CC33'>Tier: " + itemSplit[5] + "<span>";
						}
						else if (itemSplit[5] == 5) {
							itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #005CE6'>Tier: " + itemSplit[5] + "<span>";
						}
						else if (itemSplit[5] == 6) {
							itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #B800E6'>Tier: " + itemSplit[5] + "<span>";
						}
						else if (itemSplit[5] == 7) {
							itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #FF8400'>Tier: " + itemSplit[5] + "<span>";
						}
						else if (itemSplit[5] == 8) {
							itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #E6C300'>Tier: " + itemSplit[5] + "<span>";
						}
						document.getElementById(counter1).appendChild(itemHeader);
						let img = document.createElement("img");
						img.setAttribute("src", "Icon/" + itemSplit[3] + ".png");
						img.setAttribute("class", "Icon");
						img.setAttribute("title", itemSplit[2]);
						img.onclick = function () {
							document.getElementById("Total").innerHTML = "Total: " + document.getElementById("Counter").value * itemSplit[6];
							document.getElementById("InfoContainer").innerHTML =  itemSplit[2] + " </br> " + itemSplit[7] + " </br> " + itemSplit[9];
							PurchaseNumber = itemSplit[8];
						};
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
				document.getElementById("Items").appendChild(div);
				let pageHeader = document.createElement('H3');
				pageHeader.setAttribute("class", "ItemHeader");
				pageHeader.innerHTML = "Next Page";
				document.getElementById(">").appendChild(pageHeader);
				let pageUp = document.createElement('H3');
				pageUp.setAttribute("class", "PageHeader");
				pageUp.setAttribute("title", "Next Page");
				pageUp.innerHTML = ">";
				pageUp.onclick = function () {
					NextPage();
				};
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
			request.open('POST', window.location.href.replace('shop.html', 'ShopPurchase'), false);
			request.setRequestHeader('Content-Type', 'text/html; charset=utf-8');
			request.onerror = function() {
				alert("Error");
			};
			request.onload = function () {
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
					document.getElementById("Items").appendChild(div);
					let itemHeader = document.createElement('H3');
					itemHeader.setAttribute("class", "ItemHeader");
					if (itemSplit[5] == 1) {
						itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #C2C2A3'>Tier: " + itemSplit[5] + "<span>";
					}
					else if (itemSplit[5] == 2) {
						itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #FF751A'>Tier: " + itemSplit[5] + "<span>";
					}
					else if (itemSplit[5] == 3) {
						itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #E6E600'>Tier: " + itemSplit[5] + "<span>";
					}
					else if (itemSplit[5] == 4) {
						itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #33CC33'>Tier: " + itemSplit[5] + "<span>";
					}
					else if (itemSplit[5] == 5) {
						itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #005CE6'>Tier: " + itemSplit[5] + "<span>";
					}
					else if (itemSplit[5] == 6) {
						itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #B800E6'>Tier: " + itemSplit[5] + "<span>";
					}
					else if (itemSplit[5] == 7) {
						itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #FF8400'>Tier: " + itemSplit[5] + "<span>";
					}
					else if (itemSplit[5] == 8) {
						itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #E6C300'>Tier: " + itemSplit[5] + "<span>";
					}
					document.getElementById(count).appendChild(itemHeader);
					let img = document.createElement("img");
					img.setAttribute("src", "Icon/" + itemSplit[3] + ".png");
					img.setAttribute("class", "Icon");
					img.setAttribute("title", itemSplit[2]);
					img.onclick = function () {
						document.getElementById("Total").innerHTML = "Total: " + document.getElementById("Counter").value * itemSplit[6];
						document.getElementById("InfoContainer").innerHTML =  itemSplit[2] + " </br> " + itemSplit[7] + " </br> " + itemSplit[9];
						PurchaseNumber = itemSplit[8];
					};
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
			document.getElementById("Items").appendChild(div);
			let pageHeader = document.createElement('H3');
			pageHeader.setAttribute("class", "ItemHeader");
			pageHeader.innerHTML = "Next Page";
			document.getElementById(">").appendChild(pageHeader);
			let pageUp = document.createElement('H3');
			pageUp.setAttribute("class", "PageHeader");
			pageUp.setAttribute("title", "Next Page");
			pageUp.innerHTML = ">";
			pageUp.onclick = function () {
				NextPage();
			};
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
	document.getElementById("Items").appendChild(div);
	let pageHeader = document.createElement('H3');
	pageHeader.setAttribute("class", "ItemHeader");
	pageHeader.innerHTML = "Last Page";
	document.getElementById("<").appendChild(pageHeader);
	let pageDown = document.createElement('H3');
	pageDown.setAttribute("class", "PageHeader");
	pageDown.setAttribute("title", "Last Page");
	pageDown.innerHTML = "<";
	pageDown.onclick = function () {
		LastPage();
	};
	document.getElementById("<").appendChild(pageDown);
	for (const [key, value] of Object.entries(Items)) {
		let itemSplit = value.split('§');
		if (itemSplit[0].includes(category) || category == "all" || itemSplit[5] == category) {
			if (counter2 < pagePoint1 && counter2 >= pagePoint2) {
				let div = document.createElement("div");
				div.setAttribute("class", "IconContainer");
				div.setAttribute("id", counter1);
				document.getElementById("Items").appendChild(div);
				
				let itemHeader = document.createElement('H3');
				itemHeader.setAttribute("class", "ItemHeader");
				if (itemSplit[5] == 1) {
					itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #C2C2A3'>Tier: " + itemSplit[5] + "<span>";
				}
				else if (itemSplit[5] == 2) {
					itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #FF751A'>Tier: " + itemSplit[5] + "<span>";
				}
				else if (itemSplit[5] == 3) {
					itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #E6E600'>Tier: " + itemSplit[5] + "<span>";
				}
				else if (itemSplit[5] == 4) {
					itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #33CC33'>Tier: " + itemSplit[5] + "<span>";
				}
				else if (itemSplit[5] == 5) {
					itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #005CE6'>Tier: " + itemSplit[5] + "<span>";
				}
				else if (itemSplit[5] == 6) {
					itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #B800E6'>Tier: " + itemSplit[5] + "<span>";
				}
				else if (itemSplit[5] == 7) {
					itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #FF8400'>Tier: " + itemSplit[5] + "<span>";
				}
				else if (itemSplit[5] == 8) {
					itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #E6C300'>Tier: " + itemSplit[5] + "<span>";
				}
				document.getElementById(counter1).appendChild(itemHeader);
	
				let img = document.createElement("img");
				img.setAttribute("src", "Icon/" + itemSplit[3] + ".png");
				img.setAttribute("class", "Icon");
				img.setAttribute("title", itemSplit[2]);
				img.onclick = function () {
					document.getElementById("Total").innerHTML = "Total: " + document.getElementById("Counter").value * itemSplit[6];
					document.getElementById("InfoContainer").innerHTML =  itemSplit[2] + " </br> " + itemSplit[7] + " </br> " + itemSplit[9];
					PurchaseNumber = itemSplit[8];
				};
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
		document.getElementById("Items").appendChild(div);
		let pageHeader = document.createElement('H3');
		pageHeader.setAttribute("class", "ItemHeader");
		pageHeader.innerHTML = "Next Page";
		document.getElementById(">").appendChild(pageHeader);
		let pageUp = document.createElement('H3');
		pageUp.setAttribute("class", "PageHeader");
		pageUp.setAttribute("title", "Next Page");
		pageUp.innerHTML = ">";
		pageUp.onclick = function () {
			NextPage();
		};
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
		document.getElementById("Items").appendChild(div);
		let pageHeader = document.createElement('H3');
		pageHeader.setAttribute("class", "ItemHeader");
		pageHeader.innerHTML = "Last Page";
		document.getElementById("<").appendChild(pageHeader);
		let pageDown = document.createElement('H3');
		pageDown.setAttribute("class", "PageHeader");
		pageDown.setAttribute("title", "Last Page");
		pageDown.innerHTML = "<";
		pageDown.onclick = function () {
			LastPage();
		};
		document.getElementById("<").appendChild(pageDown);
	}
	for (const [key, value] of Object.entries(Items)) {
		let itemSplit = value.split('§');
		if (itemSplit[0].includes(category) || category == "all" || itemSplit[5] == category) {
			if (counter2 < pagePoint1 && counter2 >= pagePoint2) {
				let div = document.createElement("div");
				div.setAttribute("class", "IconContainer");
				div.setAttribute("id", counter1);
				document.getElementById("Items").appendChild(div);
				
				let itemHeader = document.createElement('H3');
				itemHeader.setAttribute("class", "ItemHeader");
				if (itemSplit[5] == 1) {
					itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #C2C2A3'>Tier: " + itemSplit[5] + "<span>";
				}
				else if (itemSplit[5] == 2) {
					itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #FF751A'>Tier: " + itemSplit[5] + "<span>";
				}
				else if (itemSplit[5] == 3) {
					itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #E6E600'>Tier: " + itemSplit[5] + "<span>";
				}
				else if (itemSplit[5] == 4) {
					itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #33CC33'>Tier: " + itemSplit[5] + "<span>";
				}
				else if (itemSplit[5] == 5) {
					itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #005CE6'>Tier: " + itemSplit[5] + "<span>";
				}
				else if (itemSplit[5] == 6) {
					itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #B800E6'>Tier: " + itemSplit[5] + "<span>";
				}
				else if (itemSplit[5] == 7) {
					itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #FF8400'>Tier: " + itemSplit[5] + "<span>";
				}
				else if (itemSplit[5] == 8) {
					itemHeader.innerHTML = itemSplit[2] + "</br> Count: " + itemSplit[4] + "</br> <span style='color: #E6C300'>Tier: " + itemSplit[5] + "<span>";
				}
				document.getElementById(counter1).appendChild(itemHeader);
	
				let img = document.createElement("img");
				img.setAttribute("src", "Icon/" + itemSplit[3] + ".png");
				img.setAttribute("class", "Icon");
				img.setAttribute("title", itemSplit[2]);
				img.onclick = function () {
					document.getElementById("Total").innerHTML = "Total: " + document.getElementById("Counter").value * itemSplit[6];
					document.getElementById("InfoContainer").innerHTML =  itemSplit[2] + " </br> " + itemSplit[7] + " </br> " + itemSplit[9];
					PurchaseNumber = itemSplit[8];
				};
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
		document.getElementById("Items").appendChild(div);
		let pageHeader = document.createElement('H3');
		pageHeader.setAttribute("class", "ItemHeader");
		pageHeader.innerHTML = "Next Page";
		document.getElementById(">").appendChild(pageHeader);
		let pageUp = document.createElement('H3');
		pageUp.setAttribute("class", "PageHeader");
		pageUp.setAttribute("title", "Next Page");
		pageUp.innerHTML = ">";
		pageUp.onclick = function () {
			NextPage();
		};
		document.getElementById(">").appendChild(pageUp);
	}
};

function UpdateTotal() {
	let item = Items[PurchaseNumber];
	let itemSplit = item.split('§');
	let total = document.getElementById("Counter").value * itemSplit[6];
	document.getElementById("Total").innerHTML = "Total: " + total;
};