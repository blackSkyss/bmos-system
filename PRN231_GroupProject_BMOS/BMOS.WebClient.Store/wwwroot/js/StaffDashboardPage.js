
window.onload = () => {
    let staffJson = localStorage.getItem("Staff");
    let staff = JSON.parse(staffJson);
    if (staff != null) {
        $("#staffName").append(`${staff.fullName}`);
        $(".loading--part").css("display", "flex");
        var request = $.ajax({
            type: "GET",
            url: `http://localhost:5093/odata/StaffDashBoards?$expand=ExpirationProducts,ExpirationMeals,BoughtMeals,BoughtProducts`,
            dataType: "json",
            contentType: "application/json",
            headers: { Authorization: `Bearer ` + staff.accessToken }
        });

        request.done((data, textStatus, jqXHR) => {
            $(".loading--part").css("display", "none");
            $("#productAmount").append(data.TotalProducts);
            $("#MealAmount").append(data.TotalMeals);
            $("#DoneOrderAmount").append(data.TotalDoneOrders);
            $("#NewOrderAmount").append(data.TotalNewOrders);
            $("#ProfitInThisMonthAmount").append(`${formatNumber(data.TotalMonthProfits + "")}VNĐ`);
            GetBoughtProductThisMonth(data.BoughtProducts);
            GetBoughtMealThisMonth(data.BoughtMeals);
            GetExpiredProducts(data.ExpirationProducts);
            GetExpiredMeals(data.ExpirationMeals);
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
        var LoginSuccessfully = sessionStorage.getItem("LoginSuccessfully");
        if (LoginSuccessfully != null) {
            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: LoginSuccessfully,
                showConfirmButton: false,
                timer: 1000
            });
            sessionStorage.removeItem("LoginSuccessfully");
        }
    } else {
        location.assign("/Login");
    }
}

function formatNumber(n) {
    // format number 1000000 to 1,234,567
    return n.replace(/\D/g, "").replace(/\B(?=(\d{3})+(?!\d))/g, ".")
}

function GetBoughtProductThisMonth(BoughtProducts) {
    $(".BoughtProductsTable").empty();
    var number = 0;
    var boughtProductsHTML = "";
    BoughtProducts.forEach((product) => {
        number++;
        boughtProductsHTML += `
                <tr>
                    <td>${number}</td>
                    <td>${product.ID}</td>
                    <td>${product.Name}</td>
                    <td>${formatNumber(product.Price + "")}VNĐ</td>
                    <td>${product.BoughtAmount} Kilogram</td>
                    <td>${product.Total} Kilogram</td>
                    <td><a href="/Staff/Product/${product.ID}"><i class="fa-solid fa-circle-info"></i></a></td>
                </tr>
        `;
    });
    $(".BoughtProductsTable").append(boughtProductsHTML);
    Pagination($("#numberOfBirdFoods").val());
}

function GetBoughtMealThisMonth(BoughtMeals) {
    $(".BoughtMealsTable").empty();
    var number = 0;
    var boughtMealsHTML = "";
    BoughtMeals.forEach((meal) => {
        number++;
        boughtMealsHTML += `
                <tr>
                    <td>${number}</td>
                    <td>${meal.ID}</td>
                    <td>${meal.Title}</td>
                    <td>${formatNumber(meal.Price + "")}VNĐ</td>
                    <td>${meal.BoughtAmount}</td>
                    <td><a href="/Staff/Meal/${meal.ID}"><i class="fa-solid fa-circle-info"></i></a></td>
                </tr>
        `;
    });
    $(".BoughtMealsTable").append(boughtMealsHTML);
    PaginationBoughtMeals($("#numberOfBoughtMeals").val());
}

function GetExpiredProducts(ExpirationProducts) {
    $(".ExpiredProductsTable").empty();
    var number = 0;
    var expiredProductsHTML = "";
    ExpirationProducts.forEach((product) => {
        number++;
        var expireDate = new Date(product.ExpiredDate);
        var date = expireDate.getDate();
        if (date < 10) {
            date = "0" + date;
        }
        var month = expireDate.getMonth() + 1;
        if (month < 10) {
            month = "0" + month;
        }
        expiredProductsHTML += `
                <tr>
                    <td>${number}</td>
                    <td>${product.ID}</td>
                    <td>${product.Name}</td>
                    <td>${product.Total} Kilogram</td>
                    <td>${date}/${month}/${expireDate.getFullYear()}</td>
                    <td><a href="/Staff/Meal/${product.ID}"><i class="fa-solid fa-circle-info"></i></a></td>
                </tr>
        `;
    });
    $(".ExpiredProductsTable").append(expiredProductsHTML);
    PaginationExpiredProducts($("#numberOfExpiredProducts").val());
}

function GetExpiredMeals(ExpirationMeals){
    $(".ExpiredMealsTable").empty();
    var number = 0;
    var expiredMealsHTML = "";
    ExpirationMeals.forEach((meal) => {
        number++;
        expiredMealsHTML += `
                <tr>
                    <td>${number}</td>
                    <td>${meal.ID}</td>
                    <td>${meal.Title}</td>
                    <td>${formatNumber(meal.Price + "")}VNĐ</td>
                    <td><a href="/Staff/Meal/${meal.ID}"><i class="fa-solid fa-circle-info"></i></a></td>
                </tr>
        `;
    });
    $(".ExpiredMealsTable").append(expiredMealsHTML);
    PaginationExpiredMeals($("#numberOfExpiredMeals").val());
}


$("#numberOfBirdFoods").change(() => {
    Pagination($("#numberOfBirdFoods").val());
});

$("#numberOfBoughtMeals").change(() => {
    PaginationBoughtMeals($("#numberOfBoughtMeals").val());
});

$("#numberOfExpiredMeals").change(() => {
    PaginationExpiredMeals($("#numberOfExpiredMeals").val());
});

$("#numberOfExpiredProducts").change(() => {
    PaginationExpiredProducts($("#numberOfExpiredProducts").val());
});

function Pagination(productsPerPage) {
    $(function () {
        var numberOfItems = $(".BoughtProductsTable tr").length;
        var limitPerPage = productsPerPage;
        var totalPages = Math.ceil(numberOfItems / limitPerPage);
        var paginationSize = 7;
        var currentPage;

        function ShowPage(whichPage) {
            if (whichPage < 1 || whichPage > totalPages) return false;

            currentPage = whichPage;

            $(".BoughtProductsTable tr").hide().slice((currentPage - 1) * limitPerPage, currentPage * limitPerPage).show();

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

function PaginationBoughtMeals(productsPerPage) {
    $(function () {
        var numberOfItems = $(".BoughtMealsTable tr").length;
        var limitPerPage = productsPerPage;
        var totalPages = Math.ceil(numberOfItems / limitPerPage);
        var paginationSize = 7;
        var currentPage;

        function ShowPage(whichPage) {
            if (whichPage < 1 || whichPage > totalPages) return false;

            currentPage = whichPage;

            $(".BoughtMealsTable tr").hide().slice((currentPage - 1) * limitPerPage, currentPage * limitPerPage).show();

            $(".paginationBoughtMeals li").slice(1, -1).remove();

            GetPageList(totalPages, currentPage, paginationSize).forEach(item => {
                $("<li>").empty();
                $("<li>").addClass("page-item").addClass(item ? "curent-pageBoughtMeals" : "dotsBoughtMeals").toggleClass("active", item === currentPage).append($("<a>").attr({ href: "javascript:void(0)" }).text(item || "...")).insertBefore(".next-pageBoughtMeals");
            });

            $(".previous-pageBoughtMeals").toggleClass("disable", currentPage === 1);
            $(".next-pageBoughtMeals").toggleClass("disable", currentPage === totalPages);
            return true;
        }
        $(".paginationBoughtMeals .numberPagesBoughtMeals ul").append(
            $("<li>").addClass("page-item").addClass("previous-pageBoughtMeals").append($("<a>").attr({ href: "javascript:void(0)" }).text("Prev")),
            $("<li>").addClass("page-item").addClass("next-pageBoughtMeals").append($("<a>").attr({ href: "javascript:void(0)" }).text("Next"))
        );
        if (numberOfItems == 0) {
            $(".paginationBoughtMeals .numberPagesBoughtMeals ul").empty();
        }
        ShowPage(1);

        $(document).on("click", ".paginationBoughtMeals li.curent-pageBoughtMeals:not(.active)", function () {
            return ShowPage(+$(this).text());
        });

        $(".next-pageBoughtMeals").on("click", function () {
            return ShowPage(currentPage + 1);
        });

        $(".previous-pageBoughtMeals").on("click", function () {
            return ShowPage(currentPage - 1);
        });
    });
}

function PaginationExpiredMeals(productsPerPage) {
    $(function () {
        var numberOfItems = $(".ExpiredMealsTable tr").length;
        var limitPerPage = productsPerPage;
        var totalPages = Math.ceil(numberOfItems / limitPerPage);
        var paginationSize = 7;
        var currentPage;

        function ShowPage(whichPage) {
            if (whichPage < 1 || whichPage > totalPages) return false;

            currentPage = whichPage;

            $(".ExpiredMealsTable tr").hide().slice((currentPage - 1) * limitPerPage, currentPage * limitPerPage).show();

            $(".paginationExpiredMeals li").slice(1, -1).remove();

            GetPageList(totalPages, currentPage, paginationSize).forEach(item => {
                $("<li>").empty();
                $("<li>").addClass("page-item").addClass(item ? "curent-pageExpiredMeals" : "dotsExpiredMeals").toggleClass("active", item === currentPage).append($("<a>").attr({ href: "javascript:void(0)" }).text(item || "...")).insertBefore(".next-pageExpiredMeals");
            });

            $(".previous-pageExpiredMeals").toggleClass("disable", currentPage === 1);
            $(".next-pageExpiredMeals").toggleClass("disable", currentPage === totalPages);
            return true;
        }
        $(".paginationExpiredMeals .numberPagesExpiredMeals ul").append(
            $("<li>").addClass("page-item").addClass("previous-pageExpiredMeals").append($("<a>").attr({ href: "javascript:void(0)" }).text("Prev")),
            $("<li>").addClass("page-item").addClass("next-pageExpiredMeals").append($("<a>").attr({ href: "javascript:void(0)" }).text("Next"))
        );
        if (numberOfItems == 0) {
            $(".paginationExpiredMeals .numberPagesExpiredMeals ul").empty();
        }
        ShowPage(1);

        $(document).on("click", ".paginationExpiredMeals li.curent-pageExpiredMeals:not(.active)", function () {
            return ShowPage(+$(this).text());
        });

        $(".next-pageExpiredMeals").on("click", function () {
            return ShowPage(currentPage + 1);
        });

        $(".previous-pageExpiredMeals").on("click", function () {
            return ShowPage(currentPage - 1);
        });
    });
}

function PaginationExpiredProducts(productsPerPage) {
    $(function () {
        var numberOfItems = $(".ExpiredProductsTable tr").length;
        var limitPerPage = productsPerPage;
        var totalPages = Math.ceil(numberOfItems / limitPerPage);
        var paginationSize = 7;
        var currentPage;

        function ShowPage(whichPage) {
            if (whichPage < 1 || whichPage > totalPages) return false;

            currentPage = whichPage;

            $(".ExpiredProductsTable tr").hide().slice((currentPage - 1) * limitPerPage, currentPage * limitPerPage).show();

            $(".paginationExpiredProducts li").slice(1, -1).remove();

            GetPageList(totalPages, currentPage, paginationSize).forEach(item => {
                $("<li>").empty();
                $("<li>").addClass("page-item").addClass(item ? "curent-pageExpiredProducts" : "dotsExpiredProducts").toggleClass("active", item === currentPage).append($("<a>").attr({ href: "javascript:void(0)" }).text(item || "...")).insertBefore(".next-pageExpiredProducts");
            });

            $(".previous-pageExpiredProducts").toggleClass("disable", currentPage === 1);
            $(".next-pageExpiredProducts").toggleClass("disable", currentPage === totalPages);
            return true;
        }
        $(".paginationExpiredProducts .numberPagesExpiredProducts ul").append(
            $("<li>").addClass("page-item").addClass("previous-pageExpiredProducts").append($("<a>").attr({ href: "javascript:void(0)" }).text("Prev")),
            $("<li>").addClass("page-item").addClass("next-pageExpiredProducts").append($("<a>").attr({ href: "javascript:void(0)" }).text("Next"))
        );
        if (numberOfItems == 0) {
            $(".paginationExpiredProducts .numberPagesExpiredProducts ul").empty();
        }
        ShowPage(1);

        $(document).on("click", ".paginationExpiredProducts li.curent-pageExpiredProducts:not(.active)", function () {
            return ShowPage(+$(this).text());
        });

        $(".next-pageExpiredProducts").on("click", function () {
            return ShowPage(currentPage + 1);
        });

        $(".previous-pageExpiredProducts").on("click", function () {
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