

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
            $(".mainImage img").attr("src", data.ProductImages[0].Source);
            $(".image-item").empty();
            var imageItemsHTML = "";
            data.ProductImages.forEach((image) => {
                if (image.Source == data.ProductImages[0].Source) {
                    imageItemsHTML += `<img class="col-lg-3 imgageActive" src="${image.Source}" onclick="image('${image.Source}')" />`;
                } else {
                    imageItemsHTML += `<img class="col-lg-3" src="${image.Source}" onclick="image('${image.Source}')" />`;
                }
            });
            $(".image-item").append(imageItemsHTML);
            $("#productName").append(data.Name);
            $("#birdFoodPrice").append(formatNumber(data.Price + ""));
            $("#birdFoodOriginalPrice").append(formatNumber(data.OriginalPrice + ""));
            var expiredDate = new Date(data.ExpiredDate);
            $("#expiredDate").append(`${expiredDate.getDate()}/${expiredDate.getMonth() + 1}/${expiredDate.getFullYear()}`);
            var status = "Inactive";
            var statusColor = "text-danger";
            if (data.Status == 1) {
                status = "Stocking";
                statusColor = "text-success";
            } else if (data.Status == 0) {
                status = "Out of Stock";
                statusColor = "text-primary";
            }
            $("#status").append(status);
            $("#status").attr("class", statusColor);
            var modifiedDate = new Date(data.ModifiedDate);
            var importedDate = new Date(data.ImportedTime);
            $("#modifiedDate").append(`${modifiedDate.getDate()}/${modifiedDate.getMonth() + 1}/${modifiedDate.getFullYear()}`);
            $("#importedDate").append(`${importedDate.getDate()}/${importedDate.getMonth() + 1}/${importedDate.getFullYear()}`);
            $("#modifiedBy").append(`${data.ModifiedStaff}`);
            $(".productDetails").append(`${data.Description}`);
            $("#total").append(`${data.Total}`);
            var buttonContainerHTML = `
                <a href="/Staff/Products" class="btn btn-outline-secondary btn-updateProducts">Back To List Products</a>
                <a href="/Staff/UpdateProduct/${data.ID}" class="btn btn-outline-primary btn-updateProducts">Update Product</a>
            `;
            $(".button-container").append(buttonContainerHTML);
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

        var UpdateProductSuccessfully = sessionStorage.getItem("UpdateProductSuccessfully");
        if (UpdateProductSuccessfully != null) {
            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: UpdateProductSuccessfully,
                showConfirmButton: false,
                timer: 1000
            });
            sessionStorage.removeItem("UpdateProductSuccessfully");
        }
    } else {
        location.assign("/Login");
    }
}

function image(imageSrc){
    $(".mainImage img").attr("src", imageSrc);
    var imageOptions = $(".image-item").find("img");
    for (var i = 0; i < imageOptions.length; i++) {
        if (imageOptions[i].classList.contains("imgageActive")) {
            imageOptions[i].classList.remove("imgageActive");
        }
        if (imageOptions[i].src == imageSrc) {
            imageOptions[i].classList.add("imgageActive");
        }
    }
}

function formatNumber(n) {
    // format number 1000000 to 1,234,567
    return n.replace(/\D/g, "").replace(/\B(?=(\d{3})+(?!\d))/g, ".")
}