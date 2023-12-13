

window.onload = () => {
    let storeOwnerJson = localStorage.getItem("StoreOwner");
    let storeOwner = JSON.parse(storeOwnerJson);
    if (storeOwner != null) {
        $("#storeOwnerName").append(`${storeOwner.fullName}`);
        $(".loading--part").css("display", "flex");
        var request = $.ajax({
            type: "GET",
            url: "http://localhost:5093/odata/staffs?$expand=Account",
            dataType: "json",
            contentType: "application/json",
            headers: { Authorization: `Bearer ${storeOwner.accessToken}` }
        });

        request.done((data, textStatus, jqXHR) => {
            $(".loading--part").css("display", "none");
            GetStaffs(data.value);
        });

        request.fail((jqXHR, textStatus, errorThrown) => {
            $(".loading--part").css("display", "none");
            switch (jqXHR.status) {
                case 401: {
                    var tokens = {
                        AccessToken: storeOwner.accessToken,
                        RefreshToken: storeOwner.refreshToken
                    };
                    var regenerateTokenRequest = $.ajax({
                        type: "POST",
                        url: "http://localhost:5093/odata/authentications/recreate-token",
                        dataType: "json",
                        contentType: "application/json",
                        data: JSON.stringify(tokens),
                    });

                    regenerateTokenRequest.done((data, textStatus, jqXHR) => {
                        storeOwner.accessToken = data.accessToken;
                        storeOwner.refreshToken = data.refreshToken;
                        localStorage.setItem("StoreOwner", JSON.stringify(storeOwner));
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

        var StaffIdNotFound = sessionStorage.getItem("StaffIdNotFound");
        if (StaffIdNotFound != null) {
            Swal.fire({
                icon: 'error',
                title: 'Oops...',
                text: StaffIdNotFound
            });
            sessionStorage.removeItem("StaffIdNotFound");
        }

        var CreateStaffSuccessfully = sessionStorage.getItem("CreateStaffSuccessfully");
        if (CreateStaffSuccessfully != null) {
            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: CreateStaffSuccessfully,
                showConfirmButton: false,
                timer: 1000
            });
            sessionStorage.removeItem("CreateStaffSuccessfully");
        }

    } else {
        location.assign("/Login");
    }
};

function GetStaffs(staffs) {
    $(".staffTable").empty();
    var staffRowHTML = "";
    var number = 0;

    staffs.forEach((staff) => {
        number++;
        var gender = "Female";
        var status = "Inactive";
        var statusColor = "text-danger";
        var disabled = "disabled";
        if (staff.Gender == true) {
            gender = "Male";
        }
        if (staff.QuitDate == null) {
            status = "Active";
            statusColor = "text-success";
            disabled = "";
        }

        var registeredDate = new Date(staff.RegisteredDate);

        staffRowHTML += `<tr>
                                    <td scope="row">${number}</td>
                                    <td>${staff.IdentityNumber}</td>
                                    <td>${staff.Account.Email}</td>
                                    <td>${staff.FullName}</td>
                                    <td>${staff.Phone}</td>
                                    <td>${gender}</td>
                                    <td>${registeredDate.getDate()}/${registeredDate.getMonth() + 1}/${registeredDate.getFullYear()}</td>
                                    <td class="${statusColor}">${status}</td>
                                    <td>
                                        <a href="/StoreOwner/Staff/${staff.AccountID}"><i class="fa-solid fa-circle-info"></i></a> |
                                        <a href="/StoreOwner/UpdateStaff/${staff.AccountID}"><i class="fa-solid fa-pen-to-square"></i></a> |
                                        <form method="post" onsubmit="return DeleteStaffForm(${staff.AccountID})" style="display: contents;">
                                            <button type="submit" id="BtnDelete" ${disabled}><i class="fa-solid fa-trash"></i></button>
                                        </form>
                                    </td>
                           </tr>
            `;
    });
    $(".staffTable").append(staffRowHTML);
    Pagination($("#numberOfBirdFoods").val());
}

$("#numberOfBirdFoods").change(() => {
    Pagination($("#numberOfBirdFoods").val());
});

function Pagination(staffsPerPage) {
    $(function () {
        var numberOfItems = $(".staffTable tr").length;
        var limitPerPage = staffsPerPage;
        var totalPages = Math.ceil(numberOfItems / limitPerPage);
        var paginationSize = 7;
        var currentPage;

        function ShowPage(whichPage) {
            if (whichPage < 1 || whichPage > totalPages) return false;

            currentPage = whichPage;

            $(".staffTable tr").hide().slice((currentPage - 1) * limitPerPage, currentPage * limitPerPage).show();

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

function SearchStaffNameForm() {
    let storeOwnerJson = localStorage.getItem("StoreOwner");
    let storeOwner = JSON.parse(storeOwnerJson);
    $(".loading--part").css("display", "flex");
    var request = $.ajax({
        type: "GET",
        url: `http://localhost:5093/odata/staffs?$filter=contains(tolower(fullname),'${$("#searchStaffs").val().toLowerCase()}')&$expand=Account`,
        dataType: "json",
        contentType: "application/json",
        headers: { Authorization: `Bearer ${storeOwner.accessToken}` }
    });
    request.done((data, textStatus, jqXHR) => {
        $(".loading--part").css("display", "none");
        GetStaffs(data.value);
    });

    request.fail((jqXHR, textStatus, errorThrown) => {
        $(".loading--part").css("display", "none");
        switch (jqXHR.status) {
            case 401: {
                var tokens = {
                    AccessToken: storeOwner.accessToken,
                    RefreshToken: storeOwner.refreshToken
                };
                var regenerateTokenRequest = $.ajax({
                    type: "POST",
                    url: "http://localhost:5093/odata/authentications/recreate-token",
                    dataType: "json",
                    contentType: "application/json",
                    data: JSON.stringify(tokens),
                });

                regenerateTokenRequest.done((data, textStatus, jqXHR) => {
                    storeOwner.accessToken = data.accessToken;
                    storeOwner.refreshToken = data.refreshToken;
                    localStorage.setItem("StoreOwner", JSON.stringify(storeOwner));
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


function DeleteStaffForm(staffId) {
    let storeOwnerJson = localStorage.getItem("StoreOwner");
    let storeOwner = JSON.parse(storeOwnerJson);

    Swal.fire({
        title: "Are you sure?",
        text: `[StaffID-${staffId}] will be changed action status in the system.`,
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {

        $(".loading--part").css("display", "flex");
        var request = $.ajax({
            type: "DELETE",
            url: `http://localhost:5093/odata/staffs/${staffId}`,
            contentType: "application/json",
            headers: { Authorization: `Bearer ${storeOwner.accessToken}` }
        });

        request.done((data, textStatus, jqXHR) => {
            $(".loading--part").css("display", "none");
            location.reload();
            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: "Delete Staff Successfully.",
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
                        AccessToken: storeOwner.accessToken,
                        RefreshToken: storeOwner.refreshToken
                    };
                    var regenerateTokenRequest = $.ajax({
                        type: "POST",
                        url: "http://localhost:5093/odata/authentications/recreate-token",
                        dataType: "json",
                        contentType: "application/json",
                        data: JSON.stringify(tokens),
                    });

                    regenerateTokenRequest.done((data, textStatus, jqXHR) => {
                        storeOwner.accessToken = data.accessToken;
                        storeOwner.refreshToken = data.refreshToken;
                        localStorage.setItem("StoreOwner", JSON.stringify(storeOwner));
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
    });

    return false;

}