

$(".bg-box").click((e) => {
    if (e.target.classList.contains("close")) {
        $(".bg-box").hide();
    }
});

$("#openDepositBox").click(() => {
    $(".bg-box").show();
});

window.onload = () => {
    var customerJson = localStorage.getItem("Customer");
    var customer = JSON.parse(customerJson);
    if (customer != null) {
        $("#unauthentication").hide();
        $("#authentication").show();
        $("#customerName").append(customer.fullName);

        $(".loading--part").css("display", "flex");
        var request = $.ajax({
            type: "GET",
            url: `http://localhost:5093/odata/Wallets/${customer.accountId}?$expand=WalletTransactions`,
            dataType: "json",
            contentType: "application/json",
            headers: { Authorization: `Bearer ${customer.accessToken}` }
        });

        request.done((data, textStatus, jqXHR) => {
            $(".loading--part").css("display", "none");
            $("#totalBalance").append(`${formatNumber(data.Balance + "")} VNĐ`);
            $(".TransactionsTable").empty();
            var transactionsHTML = "";
            var number = 0;
            if (data.WalletTransactions.length > 0) {
                data.WalletTransactions.forEach((transaction) => {
                    var rechargeTime = new Date(transaction.RechargeTime);
                    var date = rechargeTime.getDate();
                    var month = rechargeTime.getMonth() + 1;
                    if (date < 10) {
                        date = "0" + date;
                    }
                    if (month < 10) {
                        month = "0" + month;
                    }

                    var status = "Pending";
                    var statusColor = "text-primary";
                    if (transaction.RechargeStatus == 1) {
                        status = "Successed";
                        statusColor = "text-success";
                    } else if (transaction.RechargeStatus == 2) {
                        status = "Failed";
                        statusColor = "text-danger";
                    }
                    number++;
                    transactionsHTML += `
                            <tr>
                                <th scope="row">${number}</th>
                                <td>${date}/${month}/${rechargeTime.getFullYear()}</td>
                                <td>${formatNumber(transaction.Amount + "")} VNĐ</td>
                                <td>${transaction.Content}</td>
                                <td>${transaction.TransactionType.toLowerCase().charAt(0).toUpperCase() + transaction.TransactionType.slice(1)}</td>
                                <td class="${statusColor}">${status}</td>
                            </tr>
                `;
                });
                $(".TransactionsTable").append(transactionsHTML);
                Pagination($("#numberOfBirdFoods").val());
            } else {
                $(".numberOfBirdFoods").hide();
            }
        });

        request.fail((jqXHR, textStatus, errorThrown) => {
            $(".loading--part").css("display", "none");
            switch (jqXHR.status) {
                case 401: {
                    var tokens = {
                        AccessToken: customer.accessToken,
                        RefreshToken: customer.refreshToken
                    };
                    var regenerateTokenRequest = $.ajax({
                        type: "POST",
                        url: "http://localhost:5093/odata/authentications/recreate-token",
                        dataType: "json",
                        contentType: "application/json",
                        data: JSON.stringify(tokens),
                    });

                    regenerateTokenRequest.done((data, textStatus, jqXHR) => {
                        customer.accessToken = data.accessToken;
                        customer.refreshToken = data.refreshToken;
                        localStorage.setItem("Customer", JSON.stringify(customer));
                        Swal.fire({
                            icon: 'error',
                            title: 'Oops...',
                            text: "There are some errors in the processing. Please try again!"
                        }).then(() => {
                            location.reload();
                        });
                    });

                    regenerateTokenRequest.fail((jqXHR, textStatus, errorThrown) => {
                        location.assign("/Login");
                    });
                    break;
                }
                case 404: {
                    var error = JSON.parse(jqXHR.responseText);
                    sessionStorage.setItem("CustomerIdNotFound", error.Message[0].DescriptionError[0]);
                    location.assign("/Login");
                    break;
                }
                default: {
                    var error = JSON.parse(jqXHR.responseText);
                    Swal.fire({
                        icon: 'error',
                        title: 'Oops...',
                        text: error.Message[0].DescriptionError[0]
                    });
                    break;
                }
            }
        });
    } else {
        $("#unauthentication").show();
        $("#authentication").hide();
        location.assign("/Login");
    }
}

function formatNumber(n) {
    // format number 1000000 to 1,234,567
    return n.replace(/\D/g, "").replace(/\B(?=(\d{3})+(?!\d))/g, ".")
}

$("#numberOfBirdFoods").change(() => {
    Pagination($("#numberOfBirdFoods").val());
});

function Pagination(productsPerPage) {
    $(function () {
        var numberOfItems = $(".TransactionsTable tr").length;
        var limitPerPage = productsPerPage;
        var totalPages = Math.ceil(numberOfItems / limitPerPage);
        var paginationSize = 7;
        var currentPage;

        function ShowPage(whichPage) {
            if (whichPage < 1 || whichPage > totalPages) return false;

            currentPage = whichPage;

            $(".TransactionsTable tr").hide().slice((currentPage - 1) * limitPerPage, currentPage * limitPerPage).show();

            $(".pagination li").slice(1, -1).remove();

            GetPageList(totalPages, currentPage, paginationSize).forEach(item => {
                $("<li>").empty();
                $("<li>").addClass("page-item").addClass(item ? "curent-page" : "dots").toggleClass("active", item === currentPage).append($("<a>").attr({ href: "javascript:void(0)" }).text(item || "...")).insertBefore(".next-page");
            });

            $(".previous-page").toggleClass("disable", currentPage === 1);
            $(".next-page").toggleClass("disable", currentPage === totalPages);
            return true;
        }
        $(".pagination .numberPages ul").append(
            $("<li>").addClass("page-item").addClass("previous-page").append($("<a>").attr({ href: "javascript:void(0)" }).text("Prev")),
            $("<li>").addClass("page-item").addClass("next-page").append($("<a>").attr({ href: "javascript:void(0)" }).text("Next"))
        );
        if (numberOfItems == 0) {
            $(".pagination .numberPages ul").empty();
        }
        ShowPage(1);

        $(document).on("click", ".pagination li.curent-page:not(.active)", function () {
            return ShowPage(+$(this).text());
        });

        $(".next-page").on("click", function () {
            return ShowPage(currentPage + 1);
        });

        $(".previous-page").on("click", function () {
            return ShowPage(currentPage - 1);
        });
    });
}

function GetPageList(totalPages, page, maxLength) {
    function range(start, end) {
        return Array.from(Array(end - start + 1), (_, i) => i + start);
    }

    var sideWidth = maxLength < 9 ? 1 : 2;
    var leftWidth = (maxLength - sideWidth * 2 - 3) >> 1;
    var rightWidth = (maxLength - sideWidth * 2 - 3) >> 1;


    if (totalPages <= maxLength) {
        return range(1, totalPages);
    }

    if (page <= maxLength - sideWidth - 1 - rightWidth) {
        return range(1, maxLength - sideWidth - 1).concat(0, range(totalPages - sideWidth + 1, totalPages));
    }

    if (page >= totalPages - sideWidth - 1 - rightWidth) {
        return range(1, sideWidth).concat(0, range(totalPages - sideWidth - 1 - rightWidth - leftWidth, totalPages));
    }

    return range(1, sideWidth).concat(0, range(page - leftWidth, page + rightWidth), 0, range(totalPages - sideWidth + 1, totalPages));
}

function DepositForm() {
    var customerJson = localStorage.getItem("Customer");
    var customer = JSON.parse(customerJson);

    $(".loading--part").css("display", "flex");
    var confirmString = $("#confirmationText").val();
    var amount = $("#numberOfMoney").val();
    if (confirmString == "Agree" && amount != null) {
        var data = {
            Email: customer.email,
            Amount: ConvertToNumber($("#numberOfMoney").val()),
            RedirectUrl: "http://localhost:5284/DepositResult"
        };
        var request = $.ajax({
            type: "POST",
            url: `http://localhost:5093/odata/WalletTransactions`,
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify(data),
            headers: { Authorization: `Bearer ${customer.accessToken}` }
        });

        request.done((data, textStatus, jqXHR) => {
            $("#numberOfMoney").val("");
            $("#confirmationText").val("");
            $(".loading--part").css("display", "none");
            location.assign(data.PayUrl);
        });

        request.fail((jqXHR, textStatus, errorThrown) => {
            $(".loading--part").css("display", "none");
            switch (jqXHR.status) {
                case 400: {
                    var error = JSON.parse(jqXHR.responseText);
                    error.Message.forEach((errorDetail) => {
                        switch (errorDetail.FieldNameError.toLowerCase()) {
                            case "amount": {
                                $(".AmountErrors").empty();
                                var amountErrorHTML = "";
                                errorDetail.DescriptionError.forEach((descriptionErrorDetail) => {
                                    amountErrorHTML += `<br />${descriptionErrorDetail}`;
                                });
                                $(".AmountErrors").append(amountErrorHTML);
                                break;
                            }
                        }
                    });
                    break;
                }
                case 401: {
                    var tokens = {
                        AccessToken: customer.accessToken,
                        RefreshToken: customer.refreshToken
                    };
                    var regenerateTokenRequest = $.ajax({
                        type: "POST",
                        url: "http://localhost:5093/odata/authentications/recreate-token",
                        dataType: "json",
                        contentType: "application/json",
                        data: JSON.stringify(tokens),
                    });

                    regenerateTokenRequest.done((data, textStatus, jqXHR) => {
                        customer.accessToken = data.accessToken;
                        customer.refreshToken = data.refreshToken;
                        localStorage.setItem("Customer", JSON.stringify(customer));
                        Swal.fire({
                            icon: 'error',
                            title: 'Oops...',
                            text: "There are some errors in the processing. Please try again!"
                        }).then(() => {
                            location.reload();
                        });
                    });

                    regenerateTokenRequest.fail((jqXHR, textStatus, errorThrown) => {
                        location.assign("/Login");
                    });
                    break;
                }
                case 404: {
                    var error = JSON.parse(jqXHR.responseText);
                    sessionStorage.setItem("CustomerIdNotFound", error.Message[0].DescriptionError[0]);
                    location.assign("/Login");
                    break;
                }
                default: {
                    console.error(jqXHR);
                    console.error(jqXHR.status);
                    console.error(jqXHR.responseText);
                    console.error(errorThrown);
                    console.error(textStatus);
                    var error = JSON.parse(jqXHR.responseText);
                    Swal.fire({
                        icon: 'error',
                        title: 'Oops...',
                        text: error.Message[0].DescriptionError[0]
                    });
                    break;
                }
            }
        });
    } else {
        $(".loading--part").css("display", "none");
        $(".ConfirmStringErrors").empty();
        $(".AmountErrors").empty();
        if (confirmString == null) {
            $(".ConfirmStringErrors").append("Confirm String is required.");
        } else {
            $(".ConfirmStringErrors").append("Confirm String does not match with the requirement 'Agree'.");
        }
        if (amount == "") {
            $(".AmountErrors").append("Amount is requred.")
        }
    }
    return false;
}

function formatCurrency(input, blur) {
    // appends $ to value, validates decimal side
    // and puts cursor back in right position.

    // get input value
    var input_val = input.val();

    // don't validate empty input
    if (input_val === "") { return; }

    // initial caret position 
    /*var caret_pos = input.prop("selectionStart");*/

    // check for decimal
    if (input_val.indexOf(",") >= 0) {

        // get position of first decimal
        // this prevents multiple decimals from
        // being entered
        var decimal_pos = input_val.indexOf(",");

        // split number by decimal point
        var left_side = input_val.substring(0, decimal_pos);

        // add commas to left side of number
        left_side = formatNumber(left_side);

        // join number by .
        input_val = left_side;

    } else {
        // no decimal entered
        // add commas to number
        // remove all non-digits
        input_val = formatNumber(input_val);
        input_val = input_val;
    }

    // send updated string to input
    input.val(input_val);
}

function ConvertToNumber(priceStr) {
    var priceParts = priceStr.split(".");
    var price = "";
    for (var i = 0; i < priceParts.length; i++) {
        price += priceParts[i];
    }
    return Number.parseInt(price);
}

$("input[data-type='currency']").on({
    keyup: function () {
        formatCurrency($(this));
    },
    blur: function () {
        formatCurrency($(this), "blur");
    }
});