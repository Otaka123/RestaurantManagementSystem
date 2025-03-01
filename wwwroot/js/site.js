    //document.addEventListener("DOMContentLoaded", function () {
    //                var navbarNav = document.getElementById("navbarNav");
    //var navLinks = navbarNav.querySelectorAll(".nav-link");

    //navLinks.forEach(function (link) {
    //    link.addEventListener("click", function () {
    //        var bsCollapse = new bootstrap.Collapse(navbarNav, {
    //            toggle: false
    //        });
    //        bsCollapse.hide();
    //    });
    //                });
    //            });
//document.addEventListener("DOMContentLoaded", function () {
//    var navbarToggler = document.querySelector(".navbar-toggler");
//    var navbarNav = document.getElementById("navbarNav");

//    navbarToggler.addEventListener("click", function () {
//        if (navbarNav.classList.contains("show")) {
//            closeNavbar();
//        } else {
//            openNavbar();
//        }
//    });

//    function openNavbar() {
//        navbarNav.style.height = "auto";
//        navbarNav.classList.add("show", "opening");
//        setTimeout(() => {
//            navbarNav.classList.remove("opening");
//        }, 10);
//    }

//    function closeNavbar() {
//        navbarNav.classList.add("closing");
//        setTimeout(() => {
//            navbarNav.classList.remove("show", "closing");
//            navbarNav.style.height = "0px";
//        }, 300);
//    }

//    // إغلاق القائمة عند النقر على أي عنصر داخلها في الشاشات الصغيرة
//    document.querySelectorAll(".nav-link").forEach(function (navLink) {
//        navLink.addEventListener("click", function () {
//            if (window.innerWidth < 576) { // فقط عند الشاشات الصغيرة
//                closeNavbar();
//            }
//        });
//    });
//});

//document.addEventListener("DOMContentLoaded", function () {
//    var navbarToggler = document.querySelector(".navbar-toggler");
//    var navbarNav = document.getElementById("navbarNav");

//    // استخدام Bootstrap Collapse API
//    var bsCollapse = new bootstrap.Collapse(navbarNav, { toggle: false });

//    navbarToggler.addEventListener("click", function () {
//        if (navbarNav.classList.contains("show")) {
//            bsCollapse.hide(); // إغلاق القائمة
//        } else {
//            bsCollapse.show(); // فتح القائمة
//        }
//    });

//    // إغلاق القائمة عند النقر على أي عنصر داخلها في الشاشات الصغيرة
//    document.querySelectorAll(".nav-link").forEach(function (navLink) {
//        navLink.addEventListener("click", function () {
//            if (window.innerWidth < 576) { // فقط عند الشاشات الصغيرة
//                bsCollapse.hide();
//            }
//        });
//    });
//});
document.addEventListener("DOMContentLoaded", function () {
    var navbarToggler = document.querySelector(".navbar-toggler");
    var navbarNav = document.getElementById("navbarNav");

    // تفعيل Bootstrap Collapse API بدون تأخير
    var bsCollapse = new bootstrap.Collapse(navbarNav, { toggle: false });

    navbarToggler.addEventListener("click", function () {
        if (navbarNav.classList.contains("show")) {
            bsCollapse.hide(); // إغلاق سريع
        } else {
            bsCollapse.show(); // فتح سريع
        }
    });

    // إغلاق القائمة عند النقر على أي عنصر داخلها في الشاشات الصغيرة
    document.querySelectorAll(".nav-link").forEach(function (navLink) {
        navLink.addEventListener("click", function () {
            bsCollapse.hide();

         
        });
    });
});


$(document).ready(function () {
    $.ajax({
        url: '/Cart/GetUserCart', // تأكد من أن الرابط صحيح
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            $('#cartCount').text(data.count);
        },
        error: function (err) {
            console.error('Error fetching cart data', err);
        }
    });
});

