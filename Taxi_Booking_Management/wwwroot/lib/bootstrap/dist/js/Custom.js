$(document).ready(function () {

    $('#signInForm').on('submit', function (e) {
        e.preventDefault(); // Prevent the default form submission
        debugger;
        var failMessage = "Unable to Process your request, Please try after some time or contact the admin";
        var successMessage = "Login Successfully";

        // Serialize the form data
        var formData = $(this).serialize();

        $.ajax({
            type: "POST",
            url: "/Auth/SignIn",
            data: formData,
            dataType: "text",
            success: function (response) {
                if (response.includes('successfully')) {
                    toastr.success(successMessage)
                    setTimeout(function () {
                        window.location.href = '/DashBoard/Index';
                    }, 1000);
                } else {
               
                    toastr.error(response);
                }
            },
            error: function (response) {
                toastr.error(failMessage);
            }
        });
    });

    $('#signUpForm').on('submit', function (e) {
        e.preventDefault(); // Prevent the default form submission


        var failMessage = "Unable to Process your request, Please try after some time or contact the admin";
        var successMessage = "SignUp Successfully";

        // Serialize the form data
        var formData = $(this).serialize();

        $.ajax({
            type: "POST",
            url: "/Auth/SignUp",
            data: formData,
            dataType: "text",
            success: function (response) {
                if (response.includes('successfully')) {
                    toastr.success(successMessage)
                    setTimeout(function () {
                        window.location.href = '/Auth/SignIn';
                    }, 1000);
                } else {
                    toastr.error(response);
                }
            },
            error: function (response) {
                toastr.error(failMessage);
            }
        });
    });

});