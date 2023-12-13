
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
            $(".mainImage img").attr("src", data.MealImages[0].Source);
            $(".image-item").empty();
            var imageItemsHTML = "";
            data.MealImages.forEach((image) => {
                if (image.Source == data.MealImages[0].Source) {
                    imageItemsHTML += `<img class="col-lg-3 imgageActive" src="${image.Source}" onclick="image('${image.Source}')" />`;
                } else {
                    imageItemsHTML += `<img class="col-lg-3" src="${image.Source}" onclick="image('${image.Source}')" />`;
                }
            });
            $(".image-item").append(imageItemsHTML);
            $("#mealTitle").append(data.Title);
            $("#birdMealPrice").append(formatNumber(data.Price + ""));
            var status = "Inactive";
            var statusColor = "text-danger";
            if (data.Status == 1) {
                status = "Stocking";
                statusColor = "text-success";
            } else if (data.Status == 0) {
                status = "Out of Stock";
                statusColor = "text-primary";
            }
            $("#birdMealStatus").append(status);
            $("#birdMealStatus").attr("class", statusColor);
            var modifiedDate = new Date(data.ModifiedDate);
            $("#birdMealModifiedDate").append(`${modifiedDate.getDate()}/${modifiedDate.getMonth() + 1}/${modifiedDate.getFullYear()}`);
            $("#birdMealModifiedBy").append(`${data.ModifiedStaff}`);
            $(".productDetails").append(`${data.Description}`);
            var buttonContainerHTML = `
                <a href="/Staff/Meals" class="btn btn-outline-secondary btn-updateProducts">Back To List Products</a>
                <a href="/Staff/UpdateMeal/${data.ID}" class="btn btn-outline-primary btn-updateProducts">Update Meal</a>
            `;
            $(".button-container").append(buttonContainerHTML);

            $(".products").empty();
            var productMealsHTML = "";
            data.ProductMeals.forEach((productMeal) => {
                productMealsHTML += `
                                <div class="col-lg-3 birdMealItem">
                                    <div class="birdFoodItem-container">
                                        <a href="/Staff/Product/${productMeal.Product.ID}">
                                            <img src="${productMeal.Product.ProductImages[0].Source}" />
                                            <span class="title">${productMeal.Product.Name}</span>
                                            <span class="price">price <strong>${formatNumber(productMeal.Price + "")}VNĐ</strong> / <span id="quality">${productMeal.Amount} Kilogram</span></span>
                                            <span class="btn btn-BirdMeal">View</span>
                                        </a>
                                    </div>
                                </div>
                `;
            });
            $(".products").append(productMealsHTML);
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

        var UpdateMealSuccessfully = sessionStorage.getItem("UpdateMealSuccessfully");
        if (UpdateMealSuccessfully != null) {
            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: UpdateMealSuccessfully,
                showConfirmButton: false,
                timer: 1000
            });
            sessionStorage.removeItem("UpdateMealSuccessfully");
        }
    } else {
        location.assign("/Login");
    }
}

function image(imageSrc) {
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