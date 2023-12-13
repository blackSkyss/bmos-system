// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


function Logout() {
    localStorage.removeItem("StoreOwner");
    localStorage.removeItem("Staff");
    localStorage.removeItem("Customer");
    localStorage.removeItem("Cart");
    localStorage.clear();
    location.assign("/Login");
}