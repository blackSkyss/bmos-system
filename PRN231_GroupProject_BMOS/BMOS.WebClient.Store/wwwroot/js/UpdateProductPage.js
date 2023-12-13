

CKEDITOR.replace('description');

const files = []; //NewProductImage
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
            url: `http://localhost:5093/odata/products/${urlPath[3]}`,
            dataType: "json",
            contentType: "application/json",
            headers: { Authorization: `Bearer ${staff.accessToken}` }
        });

        request.done((data, textStatus, jqXHR) => {
            $(".loading--part").css("display", "none");
            $("#productName").val(data.Name);
            $("#sellPrice").val(formatNumber(data.Price + ""));
            $("#originalPrice").val(formatNumber(data.OriginalPrice + ""));
            var expiredDate = new Date(data.ExpiredDate);
            var month = `${expiredDate.getMonth() + 1}`;
            if (expiredDate.getMonth() + 1 < 10) {
                month = "0" + month;
            }
            var date = `${expiredDate.getDate()}`;
            if (expiredDate.getDate() < 10) {
                date = "0" + date;
            }

            $("#expiredDate").val(`${expiredDate.getFullYear()}-${month}-${date}`);
            $("#status").val(data.Status);
            CKEDITOR.instances['description'].setData(data.Description);
            $("#totalProduct").val(`${data.Total}`);
            data.ProductImages.forEach((image) => {
                displayFiles.push(image.Source);
                showImages();
            });
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
                    sessionStorage.setItem("ProductIdNotFound", error.Message[0].DescriptionError[0]);
                    location.assign("/Staff/Products");
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

function UpdateProductForm() {
    let staffJson = localStorage.getItem("Staff");
    let staff = JSON.parse(staffJson);
    var urlPath = window.location.pathname.split('/');
    $(".loading--part").css("display", "flex");

    var productName = $("#productName").val();
    var originalPrice = $("#originalPrice").val();
    var sellPrice = $("#sellPrice").val();
    var totalProduct = $("#totalProduct").val();
    var expiredDate = $("#expiredDate").val();
    var status = $("#status").val();
    var description = CKEDITOR.instances['description'].getData();

    var formData = new FormData();

    formData.append("Name", productName);
    formData.append("Description", description);
    formData.append("ExpiredDate", expiredDate);
    formData.append("Total", totalProduct);
    formData.append("Price", ConvertToNumber(sellPrice));
    formData.append("OriginalPrice", ConvertToNumber(originalPrice));
    formData.append("Status", status);

    files.forEach((image) => {
        formData.append("NewProductImages", image);
    });

    removeImages.forEach((image) => {
        formData.append("RemoveProductImages", image);
    });

    var request = $.ajax({
        type: "PUT",
        url: `http://localhost:5093/odata/products/${urlPath[3]}`,
        processData: false,
        contentType: false,
        data: formData,
        headers: { Authorization: `Bearer ${staff.accessToken}` }
    });

    request.done((data, textStatus, jqXHR) => {
        $(".loading--part").css("display", "none");
        sessionStorage.setItem("UpdateProductSuccessfully", "Update Product Successfully.");
        location.assign(`/Staff/Product/${urlPath[3]}`);
    });

    request.fail((jqXHR, textStatus, errorThrown) => {
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
                                $("#TotalErrors").empty();
                                var totalProductErrorHTML = "";
                                errorDetail.DescriptionError.forEach((descriptionErrorDetail) => {
                                    totalProductErrorHTML += `<br />${descriptionErrorDetail}`;
                                });
                                $("#TotalErrors").append(totalProductErrorHTML);
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
            case 404: {
                var error = JSON.parse(jqXHR.responseText);
                sessionStorage.setItem("ProductIdNotFound", error.Message[0].DescriptionError[0]);
                location.assign("/Staff/Products");
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
