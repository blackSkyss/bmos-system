
window.onload = () => {
    let staffJson = localStorage.getItem("Staff");
    let staff = JSON.parse(staffJson);
    if (staff != null) {
        $("#staffName").append(`${staff.fullName}`);

        $(".loading--part").css("display", "flex");
        var request = $.ajax({
            type: "GET",
            url: "http://localhost:5093/odata/meals",
            dataType: "json",
            contentType: "application/json",
            headers: { Authorization: `Bearer ${staff.accessToken}` }
        });

        request.done((data, textStatus, jqXHR) => {
            $(".loading--part").css("display", "none");
            GetMeals(data.value);
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

        var CreateMealSuccessfully = sessionStorage.getItem("CreateMealSuccessfully");
        if (CreateMealSuccessfully != null) {
            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: CreateMealSuccessfully,
                showConfirmButton: false,
                timer: 1000
            });
            sessionStorage.removeItem("CreateMealSuccessfully");
        }

        var MealIdNotFound = sessionStorage.getItem("MealIdNotFound");
        if (MealIdNotFound != null) {
            Swal.fire({
                icon: 'error',
                title: 'Oops...',
                text: MealIdNotFound
            });
            sessionStorage.removeItem("MealIdNotFound");
        }
    } else {
        location.assign("/Login");
    }
}

function GetMeals(meals) {
    $(".MealTable").empty();
    var mealRowHTML = "";
    var number = 0;

    meals.forEach((meal) => {
        number++;
        var status = "Inactive";
        var statusColor = "text-danger";
        var disabled = "disabled";
        if (meal.Status == 1) {
            status = "Stocking";
            statusColor = "text-success";
            disabled = "";
        } else if (meal.Status == 0) {
            status = "Out of Stock";
            statusColor = "text-primary";
            disabled = "";
        }
        mealRowHTML += `
                            <tr>
                                <th scope="row">${number}</th>
                                <td>${meal.Title}</td>
                                <td>${formatNumber(meal.Price + "")}VNĐ</td>
                                <td><span class="${statusColor}">${status}</span></td>
                                <td>
                                    <a href="/Staff/Meal/${meal.ID}"><i class="fa-solid fa-circle-info"></i></a> |
                                    <a href="/Staff/UpdateMeal/${meal.ID}"><i class="fa-solid fa-pen-to-square"></i></a> |
                                    <form method="post" onsubmit="return DeleteMealForm(${meal.ID})" style="display: contents;">
                                        <button type="submit" id="BtnDelete" ${disabled}><i class="fa-solid fa-trash"></i></button>
                                    </form>
                                </td>
                            </tr>
                        `;
    });

    $(".MealTable").append(mealRowHTML);
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
        var numberOfItems = $(".MealTable tr").length;
        var limitPerPage = productsPerPage;
        var totalPages = Math.ceil(numberOfItems / limitPerPage);
        var paginationSize = 7;
        var currentPage;

        function ShowPage(whichPage) {
            if (whichPage < 1 || whichPage > totalPages) return false;

            currentPage = whichPage;

            $(".MealTable tr").hide().slice((currentPage - 1) * limitPerPage, currentPage * limitPerPage).show();

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

function SearchMealNameForm() {
    let staffJson = localStorage.getItem("Staff");
    let staff = JSON.parse(staffJson);
    $(".loading--part").css("display", "flex");
    var request = $.ajax({
        type: "GET",
        url: `http://localhost:5093/odata/meals?$filter=contains(tolower(title),'${$("#searchProducts").val().toLowerCase()}')`,
        dataType: "json",
        contentType: "application/json",
        headers: { Authorization: `Bearer ${staff.accessToken}` }
    });
    request.done((data, textStatus, jqXHR) => {
        $(".loading--part").css("display", "none");
        GetMeals(data.value);
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

function DeleteMealForm(mealId) {
    let staffJson = localStorage.getItem("Staff");
    let staff = JSON.parse(staffJson);
    Swal.fire({
        title: "Are you sure?",
        text: `[MealId-${mealId}] will be changed action status in the system.`,
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (request.isConfirmed) {
            $(".loading--part").css("display", "flex");
            var request = $.ajax({
                type: "DELETE",
                url: `http://localhost:5093/odata/meals/${mealId}`,
                contentType: "application/json",
                headers: { Authorization: `Bearer ${staff.accessToken}` }
            });

            request.done((data, textStatus, jqXHR) => {
                $(".loading--part").css("display", "none");
                location.reload();
                Swal.fire({
                    position: 'top-end',
                    icon: 'success',
                    title: "Delete Product Successfully.",
                    showConfirmButton: false,
                    timer: 1000
                });
                $("#BtnDelete").attr("disabled", "disabled");
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
        }
    });
    return false;
}
