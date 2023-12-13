

window.onload = () => {
    let staffJson = localStorage.getItem("Staff");
    let staff = JSON.parse(staffJson);
    if (staff != null) {
        $("#staffName").append(`${staff.fullName}`);
        $(".loading--part").css("display", "flex");
        var request = $.ajax({
            type: "GET",
            url: `http://localhost:5093/odata/orders?$expand=customer($expand=account)`,
            dataType: "json",
            contentType: "application/json",
            headers: { Authorization: `Bearer ` + staff.accessToken }
        });

        request.done((data, textStatus, jqXHR) => {
            $(".loading--part").css("display", "none");
            GetOrders(data.value);
        });

        request.fail((jqXHR, textStatus, errorThrown) => {
            $(".loading--part").css("display", "none");
            switch (jqXHR.status) {
                case 401: {
                    var tokens = {
                        AccessToken: staff.accessToken,
                        RefreshToken: staff.refreshToken
                    };
                    var regenerateTokenRequest = $.ajax({
                        type: "POST",
                        url: "http://localhost:5093/odata/authentications/recreate-token",
                        dataType: "json",
                        contentType: "application/json",
                        data: JSON.stringify(tokens),
                    });

                    regenerateTokenRequest.done((data, textStatus, jqXHR) => {
                        staff.accessToken = data.accessToken;
                        staff.refreshToken = data.refreshToken;
                        localStorage.setItem("Staff", JSON.stringify(staff));
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
                default: {
                    console.error(jqXHR.responseText);
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

        var OrderIdNotFound = sessionStorage.getItem("OrderIdNotFound");
        if (OrderIdNotFound != null) {
            Swal.fire({
                icon: 'error',
                title: 'Oops...',
                text: OrderIdNotFound
            });
            sessionStorage.removeItem("OrderIdNotFound");
        }
    } else {
        location.assign("/Login");
    }
}

function GetOrders(orders) {
    $(".ordersTable").empty();
    var number = 0;
    var ordersHTML = "";
    orders.forEach((order) => {
        number++;
        var orderedDate = new Date(order.OrderedDate);
        var month = orderedDate.getMonth() + 1;
        var date = orderedDate.getDate();
        if (month < 10) {
            month = "0" + month;
        }
        if (date < 10) {
            date = "0" + date;
        }
        var status = "New Order";
        var statusColor = "text-primary";
        if (order.OrderStatus == 1) {
            status = "Processing";
            statusColor = "text-info";
        } else if (order.OrderStatus == 2) {
            status = "Done";
            statusColor = "text-success";
        } else if (order.OrderStatus == 3) {
            status = "Cancled";
            statusColor = "text-danger";
        }
        ordersHTML += `
                        <tr>
                            <th scope="row">${number}</th>
                            <td>${order.Customer.FullName}</td>
                            <td>${order.Customer.Account.Email}</td>
                            <td>${date}/${month}/${orderedDate.getFullYear()}</td>
                            <td>${formatNumber(order.Total + "")}VNĐ</td>
                            <td class="${statusColor}">${status}</td>
                            <td><a href="/Staff/Order/${order.ID}" style="color: #000; font-size: 18px;"><i class="fa-solid fa-circle-info" id="historyOrder"></i></a></td>
                        </tr>
                    `;
    });
    $(".ordersTable").append(ordersHTML);
    Pagination($("#numberOfBirdFoods").val());
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
        var numberOfItems = $(".ordersTable tr").length;
        var limitPerPage = productsPerPage;
        var totalPages = Math.ceil(numberOfItems / limitPerPage);
        var paginationSize = 7;
        var currentPage;

        function ShowPage(whichPage) {
            if (whichPage < 1 || whichPage > totalPages) return false;

            currentPage = whichPage;

            $(".ordersTable tr").hide().slice((currentPage - 1) * limitPerPage, currentPage * limitPerPage).show();

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

function SearchCustomerName() {
    let staffJson = localStorage.getItem("Staff");
    let staff = JSON.parse(staffJson);
    $(".loading--part").css("display", "flex");
    var request = $.ajax({
        type: "GET",
        url: `http://localhost:5093/odata/orders?$expand=customer($expand=account)&$filter=contains(tolower(customer/fullname),'${$("#searchProducts").val().toLowerCase()}')`,
        dataType: "json",
        contentType: "application/json",
        headers: { Authorization: `Bearer ` + staff.accessToken }
    });

    request.done((data, textStatus, jqXHR) => {
        $(".loading--part").css("display", "none");
        GetOrders(data.value);
    });

    request.fail((jqXHR, textStatus, errorThrown) => {
        $(".loading--part").css("display", "none");
        switch (jqXHR.status) {
            case 401: {
                var tokens = {
                    AccessToken: staff.accessToken,
                    RefreshToken: staff.refreshToken
                };
                var regenerateTokenRequest = $.ajax({
                    type: "POST",
                    url: "http://localhost:5093/odata/authentications/recreate-token",
                    dataType: "json",
                    contentType: "application/json",
                    data: JSON.stringify(tokens),
                });

                regenerateTokenRequest.done((data, textStatus, jqXHR) => {
                    staff.accessToken = data.accessToken;
                    staff.refreshToken = data.refreshToken;
                    localStorage.setItem("Staff", JSON.stringify(staff));
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
    return false;
}