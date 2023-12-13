
CKEDITOR.replace('mealDescription');

const files = [];
const removeImages = []; //RemoveProductImage
const displayFiles = [];
const uploadBox = document.querySelector(".upload-box");
const productImagesBody = document.querySelector(".productImages-body");
const text = document.querySelector(".inner");
const browse = document.querySelector(".select");
const input = document.querySelector(".upload-box input");

uploadBox.addEventListener("click", () => {
    input.click();

    //Input change event
    input.addEventListener('change', () => {
        const file = input.files;

        uploadBox.classList.remove('dragover');
        text.innerHTML = `Drag & drop images here or`;
        browse.classList.remove('hide');

        for (var i = 0; i < file.length; i++) {
            if (files.every(e => e.name != file[i].name)) {
                files.push(file[i]);
                var srcImage = URL.createObjectURL(file[i]);
                displayFiles.push(srcImage);
            }
        }
        showImages();
    });
});


//drag and drop
uploadBox.addEventListener('dragover', e => {
    e.preventDefault();

    uploadBox.classList.add('dragover');
    text.innerHTML = 'Drop images here';
    browse.classList.add('hide');
});

uploadBox.addEventListener('dragleave', e => {
    e.preventDefault();

    uploadBox.classList.remove('dragover');
    text.innerHTML = `Drag & drop image here or`;
    browse.classList.remove('hide');
});

uploadBox.addEventListener('drop', e => {
    e.preventDefault();

    uploadBox.classList.remove('dragover');
    text.innerHTML = `Drag & drop image here or`;
    browse.classList.remove('hide');

    let file = e.dataTransfer.files;
    for (var i = 0; i < file.length; i++) {
        if (files.every(e => e.name != file[i].name)) {
            files.push(file[i]);
            var srcImage = URL.createObjectURL(file[i]);
            displayFiles.push(srcImage);
        }
    }
    showImages();
});

const delImage = index => {
    var removedImage = displayFiles[index];

    var existedImage = files.find(x => URL.createObjectURL(x) == removedImage);
    if (existedImage != null) {
        var imageIndex = files.indexOf(existedImage);
        files.splice(imageIndex, 1);
    } else {
        var removedImage = displayFiles[index];
        displayFiles.splice(index, 1);
        removeImages.push(removedImage);
    }

    showImages();
}

const showImages = () => {
    let images = '';
    displayFiles.forEach((e, i) => {
        images += `<div class="image">
                    <img src="${e}"  />
                    <span onclick="delImage(${i})">&times;</span>
                </div>`;
    });
    productImagesBody.innerHTML = images;
};

window.onload = () => {
    let staffJson = localStorage.getItem("Staff");
    let staff = JSON.parse(staffJson);
    if (staff != null) {
        $("#staffName").append(`${staff.fullName}`);
        var urlPath = window.location.pathname.split('/');

        $(".loading--part").css("display", "flex");
        var request = $.ajax({
            type: "GET",
            url: `http://localhost:5093/odata/meals/${urlPath[3]}?$expand=productmeals($expand=product)`,
            dataType: "json",
            contentType: "application/json",
            headers: { Authorization: `Bearer ${staff.accessToken}` }
        });

        request.done((data, textStatus, jqXHR) => {
            $(".loading--part").css("display", "none");

            $("#mealTitle").val(data.Title);
            $("#mealStatus").val(data.Status);
            CKEDITOR.instances['mealDescription'].setData(data.Description);
            data.MealImages.forEach((image) => {
                displayFiles.push(image.Source);
                showImages();
            });

            var productMeals = new Array();
            data.ProductMeals.forEach((existedProductMeal) => {
                var productMeal = {
                    ProductID: existedProductMeal.Product.ID,
                    Amount: existedProductMeal.Amount,
                    Price: formatNumber(existedProductMeal.Price + "")
                };
                productMeals.push(productMeal);
            });

            sessionStorage.setItem("ProductMeals", JSON.stringify(productMeals));
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
                case 404: {
                    var error = JSON.parse(jqXHR.responseText);
                    sessionStorage.setItem("MealIdNotFound", error.Message[0].DescriptionError[0]);
                    location.assign("/Staff/Meals");
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


        var getProductsRequest = $.ajax({
            type: "GET",
            url: "http://localhost:5093/odata/products",
            dataType: "json",
            contentType: "application/json",
            headers: { Authorization: `Bearer ${staff.accessToken}` }
        });

        getProductsRequest.done((data, textStatus, jqXHR) => {
            GetProducts(data.value);
        });

        getProductsRequest.fail((jqXHR, textStatus, errorThrown) => {
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

    } else {
        location.assign("/Login");
    }
}


function GetProducts(products) {
    $(".products--container").empty();
    var productHTML = "";
    var inactiveProducts = new Array();
    products.forEach((product) => {
        var status = "Inactive";
        var statusColor = "text-danger";
        var disabled = "disabled";
        var icon = '<i class="fa-solid fa-xmark"></i>';
        var BtnSelect = `<button class="btn BtnSelect" style="background: red;" id="BtnSelect-${product.ID}" onclick="RemoveInactiveProductOfMeal(${product.ID})" disabled="disabled">Disabled Product</button>`;
        if (product.Status == 1) {
            status = "Stocking";
            statusColor = "text-success";
            disabled = "";
            icon = '<i class="fa-solid fa-check"></i>';
            BtnSelect = `<a class="btn BtnSelect" id="BtnSelect-${product.ID}" onclick="AddProductOfMeal(${product.ID})">Select</a>`;
        } else if (product.Status == 0) {
            status = "Out of Stock";
            statusColor = "text-primary";
            disabled = "";
        } else if (product.Status == 2) {
            inactiveProducts.push(product);
        }
        productHTML += `
            <div class="col-lg-3">
                <div class="productItem">
                    <a>
                        <img src="${product.ProductImages[0].Source}" />
                        <span class="titleProduct">${product.Name}</span> <br /> 
                        <div style="text-align: center; margin-bottom: 15px; margin-top: -25px;">
                            <span class="${statusColor} text-center">${icon} ${status}</span>
                        </div>
                        <span class="productPrice">Price: <span id="price-${product.ID}">${formatNumber(product.Price + "")}</span>VNĐ<strong> / 1 Kilogram</strong></span>
                    </a>
                    <span class="productQuantity">Quantity: <input type="number" value="1" id="quantity-${product.ID}" ${disabled} /><strong> / 1 Kilogram</strong></span>
                    ${BtnSelect}
                </div>
            </div>`;
    });

    $(".products--container").append(productHTML);
    Pagination($("#numberOfBirdFoods").val());
    var productMealsJson = sessionStorage.getItem("ProductMeals");
    var productMeals = JSON.parse(productMealsJson);
    var totalProducts = 0;
    var totalPrice = 0;
    if (productMeals != null && productMeals.length > 0) {
        productMeals.forEach((productMeal) => {
            var inactiveProductMeal = inactiveProducts.find(x => x.ID == productMeal.ProductID);
            if (inactiveProductMeal == null) {
                $(`#BtnSelect-${productMeal.ProductID}`).text("Selected");
                $(`#BtnSelect-${productMeal.ProductID}`).css("background", "#708b77");
                $(`#BtnSelect-${productMeal.ProductID}`).hover(function (e) {
                    $(this).css("background", e.type == "mouseenter" ? "#f77400" : "#708b77");
                });
                $(`#BtnSelect-${productMeal.ProductID}`).attr("onclick", `RemoveProductOfMeal(${productMeal.ProductID})`);
                $(`#quantity-${productMeal.ProductID}`).attr("disabled", false);
            } else {
                $(`#BtnSelect-${productMeal.ProductID}`).text("Remove Product");
                $(`#BtnSelect-${productMeal.ProductID}`).attr("disabled", false);
            }
            $(`#quantity-${productMeal.ProductID}`).attr("disabled", "disabled");
            
            totalPrice += ConvertToNumber(productMeal.Price) * productMeal.Amount;
        });
        $("#CreateNewMeal").attr("disabled", false);
    }
    totalProducts = productMeals.length;
    $("#totalProduct").empty();
    $("#totalPrice").empty();
    $("#totalProduct").append(totalProducts);
    $("#totalPrice").append(formatNumber(totalPrice + ""));
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
        var numberOfItems = $(".products--container .col-lg-3").length;
        var limitPerPage = productsPerPage;
        var totalPages = Math.ceil(numberOfItems / limitPerPage);
        var paginationSize = 7;
        var currentPage;

        function ShowPage(whichPage) {
            if (whichPage < 1 || whichPage > totalPages) return false;

            currentPage = whichPage;

            $(".products--container .col-lg-3").hide().slice((currentPage - 1) * limitPerPage, currentPage * limitPerPage).show();

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

function SearchProductNameForm(event) {
    let staffJson = localStorage.getItem("Staff");
    let staff = JSON.parse(staffJson);
    var request = $.ajax({
        type: "GET",
        url: `http://localhost:5093/odata/products?$filter=contains(tolower(name),'${event.target.value.toLowerCase()}')`,
        dataType: "json",
        contentType: "application/json",
        headers: { Authorization: `Bearer ${staff.accessToken}` }
    });
    request.done((data, textStatus, jqXHR) => {
        GetProducts(data.value);
    });

    request.fail((jqXHR, textStatus, errorThrown) => {
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

function ConvertToNumber(priceStr) {
    var priceParts = priceStr.split(".");
    var price = "";
    for (var i = 0; i < priceParts.length; i++) {
        price += priceParts[i];
    }
    return Number.parseInt(price);
}


function AddProductOfMeal(productId) {
    var quantity = Number.parseFloat($(`#quantity-${productId}`).val());
    var totalPrice = 0;
    if (quantity <= 0) {
        Swal.fire({
            icon: 'error',
            title: 'Oops...',
            text: "Quantity is required greater than 0."
        });
    } else {
        var productMealsJson = sessionStorage.getItem("ProductMeals");
        var productMeals = JSON.parse(productMealsJson);
        if (productMeals != null) {
            var productMeal = productMeals.find(x => x.ProductID == productId);
            if (productMeal != null) {
                productMeal.Amount = quantity;
            } else {
                var product = {
                    ProductID: productId,
                    Amount: quantity,
                    Price: $(`#price-${productId}`).text()
                };
                productMeals.push(product);
            }
            productMeals.forEach((productMeal) => {
                totalPrice += ConvertToNumber(productMeal.Price) * productMeal.Amount;
            });

        } else {
            productMeals = new Array();
            var product = {
                ProductID: productId,
                Amount: quantity,
                Price: $(`#price-${productId}`).text()
            };
            totalPrice += ConvertToNumber(product.Price) * product.Amount;
            productMeals.push(product);
        }
        var totalProduct = productMeals.length;
        sessionStorage.setItem("ProductMeals", JSON.stringify(productMeals));
        $("#totalProduct").empty();
        $("#totalPrice").empty();
        $("#totalProduct").append(totalProduct);
        $("#totalPrice").append(formatNumber(totalPrice + ""));
        $(`#BtnSelect-${productId}`).text("Selected");
        $(`#BtnSelect-${productId}`).css("background", "#708b77");
        $(`#BtnSelect-${productId}`).hover(function (e) {
            $(this).css("background", e.type == "mouseenter" ? "#f77400" : "#708b77");
        });
        $(`#BtnSelect-${productId}`).attr("onclick", `RemoveProductOfMeal(${productId})`);
        $(`#quantity-${productId}`).attr("disabled", "disabled");
        $("#CreateNewMeal").attr("disabled", false);
        Swal.fire({
            position: 'top-end',
            icon: 'success',
            title: "Selected Product for Meal Successfully.",
            showConfirmButton: false,
            timer: 1000
        });
    }
}

function RemoveProductOfMeal(productId) {
    var productMealsJson = sessionStorage.getItem("ProductMeals");
    var productMeals = JSON.parse(productMealsJson);
    var totalPrice = 0;
    var totalProducts = 0;
    var productMeal = productMeals.find(x => x.ProductID == productId);
    var index = productMeals.indexOf(productMeal);
    productMeals.splice(index, 1);
    if (productMeals.length == 0) {
        sessionStorage.removeItem("ProductMeals");
        $("#CreateNewMeal").attr("disabled", false);
    } else {
        sessionStorage.setItem("ProductMeals", JSON.stringify(productMeals));
    }
    totalProducts = productMeals.length;
    productMeals.forEach((productMeal) => {
        totalPrice += ConvertToNumber(productMeal.Price) * productMeal.Amount;
    });
    $("#totalProduct").empty();
    $("#totalPrice").empty();
    $("#totalProduct").append(totalProducts);
    $("#totalPrice").append(formatNumber(totalPrice + ""));
    $(`#BtnSelect-${productId}`).attr("onclick", `AddProductOfMeal(${productId})`);
    $(`#quantity-${productId}`).attr("disabled", false);
    $(`#BtnSelect-${productId}`).text("Select");
    $(`#BtnSelect-${productId}`).css("background", "#f77400");
    $(`#BtnSelect-${productId}`).hover(function (e) {
        $(this).css("background", e.type == "mouseenter" ? "#708b77" : "#f77400");
    });
    Swal.fire({
        position: 'top-end',
        icon: 'success',
        title: "Unselected Product for Meal Successfully.",
        showConfirmButton: false,
        timer: 1000
    });
}

function RemoveInactiveProductOfMeal(productId) {
    var productMealsJson = sessionStorage.getItem("ProductMeals");
    var productMeals = JSON.parse(productMealsJson);
    var totalPrice = 0;
    var totalProducts = 0;
    var productMeal = productMeals.find(x => x.ProductID == productId);
    var index = productMeals.indexOf(productMeal);
    productMeals.splice(index, 1);
    if (productMeals.length == 0) {
        sessionStorage.removeItem("ProductMeals");
        $("#CreateNewMeal").attr("disabled", false);
    } else {
        sessionStorage.setItem("ProductMeals", JSON.stringify(productMeals));
    }
    totalProducts = productMeals.length;
    productMeals.forEach((productMeal) => {
        totalPrice += ConvertToNumber(productMeal.Price) * productMeal.Amount;
    });
    $("#totalProduct").empty();
    $("#totalPrice").empty();
    $("#totalProduct").append(totalProducts);
    $("#totalPrice").append(formatNumber(totalPrice + ""));
    $(`#BtnSelect-${productId}`).empty();
    $(`#BtnSelect-${productId}`).append("Disabled Product");
    $(`#BtnSelect-${productId}`).attr("disabled", "disabled");
    Swal.fire({
        position: 'top-end',
        icon: 'success',
        title: "Remove Product out Meal Successfully.",
        showConfirmButton: false,
        timer: 1000
    });
}


function UpdateMealForm() {
    let staffJson = localStorage.getItem("Staff");
    let staff = JSON.parse(staffJson);
    var productMealsJson = sessionStorage.getItem("ProductMeals");
    var productMeals = JSON.parse(productMealsJson);
    var urlPath = window.location.pathname.split('/');
    var formData = new FormData();

    var mealTitle = $("#mealTitle").val();
    var mealDescription = CKEDITOR.instances['mealDescription'].getData();
    var mealStatus = $("#mealStatus").val();
    formData.append("Title", mealTitle);
    formData.append("Description", mealDescription);
    formData.append("Status", mealStatus);
    productMeals.forEach((productMeal, index) => {
        formData.append(`ProductMeals[${index}].ProductID`, productMeal.ProductID);
        formData.append(`ProductMeals[${index}].Amount`, productMeal.Amount);
    });
    files.forEach((image) => {
        formData.append("NewMealImages", image);
    });
    removeImages.forEach((image) => {
        formData.append("RemoveMealImages", image);
    });
    $(".loading--part").css("display", "flex");
    var request = $.ajax({
        type: "PUT",
        url: `http://localhost:5093/odata/meals/${urlPath[3]}`,
        processData: false,
        contentType: false,
        data: formData,
        headers: { Authorization: `Bearer ${staff.accessToken}` }
    });
    request.done((data, textStatus, jqXHR) => {
        $(".loading--part").css("display", "none");
        sessionStorage.setItem("UpdateMealSuccessfully", "Update Meal Successfully.");
        sessionStorage.removeItem("ProductMeals");
        location.assign(`/Staff/Meal/${urlPath[3]}`);
    });

    request.fail((jqXHR, textStatus, errorThrown) => {
        ClearErrorForm();
        $(".loading--part").css("display", "none");
        switch (jqXHR.status) {
            case 400: {
                var error = JSON.parse(jqXHR.responseText);
                if (error.Message.length >= 1 && error.Message[0].FieldNameError != "Exception") {
                    console.error("do 1");
                    error.Message.forEach((errorDetail) => {
                        switch (errorDetail.FieldNameError.toLowerCase()) {
                            case "title": {
                                $("#TitleErrors").empty();
                                var titleErrorHTML = "";
                                errorDetail.DescriptionError.forEach((descriptionErrorDetail) => {
                                    titleErrorHTML += `<br />${descriptionErrorDetail}`;
                                });
                                $("#TitleErrors").append(titleErrorHTML);
                                break;
                            }
                            case "description": {
                                $("#DescriptionErrors").empty();
                                var descriptionErrorHTML = "";
                                errorDetail.DescriptionError.forEach((descriptionErrorDetail) => {
                                    descriptionErrorHTML += `<br />${descriptionErrorDetail}`;
                                });
                                $("#DescriptionErrors").append(descriptionErrorHTML);
                                break;
                            }
                            case "mealimages": {
                                $("#ProductImagesErrors").empty();
                                var productImagesErrorHTML = "";
                                errorDetail.DescriptionError.forEach((descriptionErrorDetail) => {
                                    productImagesErrorHTML += `<br />${descriptionErrorDetail}`;
                                });
                                $("#ProductImagesErrors").append(productImagesErrorHTML);
                                break;
                            }
                            case "productmeals": {
                                $("#ProductMealsErrors").empty();
                                var productMealsErrorHTML = "";
                                errorDetail.DescriptionError.forEach((descriptionErrorDetail) => {
                                    productMealsErrorHTML += `<br />${descriptionErrorDetail}`;
                                });
                                $("#ProductMealsErrors").append(productMealsErrorHTML);
                                break;
                            }
                        }
                    });
                }

                if (error.Message.length == 1 && error.Message[0].FieldNameError == "Exception") {
                    console.error("do 2");
                    if (error.Message[0].FieldNameError.toLowerCase() == "exception") {
                        ClearErrorForm();
                        Swal.fire({
                            icon: 'error',
                            title: 'Oops...',
                            text: error.Message[0].DescriptionError[0]
                        });
                    }
                }
                break;
            }
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

function ClearErrorForm() {
    $("#TitleErrors").empty();
    $("#DescriptionErrors").empty();
    $("#ProductImagesErrors").empty();
    $("#ProductMealsErrors").empty();
}