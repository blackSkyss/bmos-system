

CKEDITOR.replace('description');

const files = [];
const uploadBox = document.querySelector(".upload-box");
const productImagesBody = document.querySelector(".productImages-body");
const text = document.querySelector(".inner");
const browse = document.querySelector(".select");
const input = document.querySelector(".upload-box input");

uploadBox.addEventListener('click', () => {
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
        }
    }
    showImages();
});

const delImage = index => {
    files.splice(index, 1);
    showImages();
}

const showImages = () => {
    let images = '';
    files.forEach((e, i) => {
        images += `<div class="image">
                    <img src="${URL.createObjectURL(e)}"  />
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
    } else {
        location.assign("/Login");
    }
};

function ConvertToNumber(priceStr) {
    var priceParts = priceStr.split(".");
    var price = "";
    for (var i = 0; i < priceParts.length; i++) {
        price += priceParts[i];
    }
    return Number.parseInt(price);
}

function CreateProductForm() {
    let staffJson = localStorage.getItem("Staff");
    let staff = JSON.parse(staffJson);
    var formData = new FormData();

    var productName = $("#productName").val();
    var originalPrice = ConvertToNumber($("#originalPrice").val());
    var sellPrice = ConvertToNumber($("#sellPrice").val());
    var totalProduct = $("#totalProduct").val();
    var expiredDate = $("#expiredDate").val();
    var description = CKEDITOR.instances['description'].getData();
    var productImages = files;

    if ($("#expiredDate").val() == "") {
        ClearErrorForm();
        $("#ExpiredDateErrors").append("<br /> Expired Date is null.");
        $("#ExpiredDateErrors").append("<br /> Expired Date is empty.");
    }

    formData.append("Name", productName);
    formData.append("Description", description);
    formData.append("ExpiredDate", expiredDate);
    formData.append("Total", totalProduct);
    formData.append("Price", sellPrice);
    formData.append("OriginalPrice", originalPrice);
    productImages.forEach((image) => {
        formData.append("ProductImages", image);
    });

    $(".loading--part").css("display", "flex");
    var request = $.ajax({
        type: "POST",
        url: "http://localhost:5093/odata/products",
        processData: false,
        contentType: false,
        data: formData,
        headers: { Authorization: `Bearer ${staff.accessToken}` }
    });

    request.done((data, textStatus, jqXHR) => {
        $(".loading--part").css("display", "none");
        sessionStorage.setItem("CreateProductSuccessfully", "Create Product Successfully.");
        location.assign("/Staff/Products");
    });

    request.fail((jqXHR, textStatus, errorThrown) => {
        ClearErrorForm();
        $(".loading--part").css("display", "none");
        switch (jqXHR.status) {
            case 400: {
                var error = JSON.parse(jqXHR.responseText);
                if (error.Message.length >= 1 && error.Message[0].FieldNameError != "Exception") {
                    error.Message.forEach((errorDetail) => {
                        switch (errorDetail.FieldNameError.toLowerCase()) {
                            case "name": {
                                $("#ProductNameErrors").empty();
                                var productNameErrorHTML = "";
                                errorDetail.DescriptionError.forEach((descriptionErrorDetail) => {
                                    productNameErrorHTML += `<br />${descriptionErrorDetail}`;
                                });
                                $("#ProductNameErrors").append(productNameErrorHTML);
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
                            case "expireddate": {
                                $("#ExpiredDateErrors").empty();
                                var expiredDateErrorHTML = "";
                                errorDetail.DescriptionError.forEach((descriptionErrorDetail) => {
                                    expiredDateErrorHTML += `<br />${descriptionErrorDetail}`;
                                });
                                $("#ExpiredDateErrors").append(expiredDateErrorHTML);
                                break;
                            }
                            case "total": {
                                $("#TotalProductErrors").empty();
                                var totalProductErrorHTML = "";
                                errorDetail.DescriptionError.forEach((descriptionErrorDetail) => {
                                    totalProductErrorHTML += `<br />${descriptionErrorDetail}`;
                                });
                                $("#TotalProductErrors").append(totalProductErrorHTML);
                                break;
                            }

                            case "price": {
                                $("#SellPriceErrors").empty();
                                var sellPriceErrorHTML = "";
                                errorDetail.DescriptionError.forEach((descriptionErrorDetail) => {
                                    sellPriceErrorHTML += `<br />${descriptionErrorDetail}`;
                                });
                                $("#SellPriceErrors").append(sellPriceErrorHTML);
                                break;
                            }

                            case "originalprice": {
                                $("#OriginalPriceErrors").empty();
                                var originalPriceErrorHTML = "";
                                errorDetail.DescriptionError.forEach((descriptionErrorDetail) => {
                                    originalPriceErrorHTML += `<br />${descriptionErrorDetail}`;
                                });
                                $("#OriginalPriceErrors").append(originalPriceErrorHTML);
                                break;
                            }

                            case "productimages": {
                                $("#ProductImagesErrors").empty();
                                var productImagesErrorHTML = "";
                                errorDetail.DescriptionError.forEach((descriptionErrorDetail) => {
                                    productImagesErrorHTML += `<br />${descriptionErrorDetail}`;
                                });
                                $("#ProductImagesErrors").append(productImagesErrorHTML);
                                break;
                            }
                        }
                    });
                }

                if (error.Message.length == 1 && error.Message[0].FieldNameError == "Exception") {
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
    $("#ProductNameErrors").empty();
    $("#DescriptionErrors").empty();
    $("#ProductImagesErrors").empty();
    $("#OriginalPriceErrors").empty();
    $("#SellPriceErrors").empty();
    $("#TotalProductErrors").empty();
    $("#ExpiredDateErrors").empty();
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


function formatNumber(n) {
    // format number 1000000 to 1,234,567
    return n.replace(/\D/g, "").replace(/\B(?=(\d{3})+(?!\d))/g, ".")
}


$("input[data-type='currency']").on({
    keyup: function () {
        formatCurrency($(this));
    },
    blur: function () {
        formatCurrency($(this), "blur");
    }
});