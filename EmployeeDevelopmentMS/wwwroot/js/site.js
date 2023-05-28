
$(document).ready(function () {
    $('#btnUPShowPersonalInfoSection').click(function () {
        PrefillPersonalInfo();
    });

    $('#btnUPSavePersonalInfoSection').click(function () {
        UpdatePersonalInfo();
    });

    $('#btnUPShowChangePasswordSection').click(function () {
        GenerateNewPassword();
    });

    $('#btnUPChangePasswordSection').click(function () {
        ChangePassword();
    });

    $('#btnUPAddTimeOff').click(function () {
        AddTimeOff();
    });
});

function ShowSection(btnID, section) {
    $("#" + btnID).css("display", "none");
    $("#" + section).css("display", "");
}

function HideSection(btnID, section) {
    $("#" + btnID).css("display", "");
    $("#" + section).css("display", "none");
}

function PrefillPersonalInfo() {
    $.ajax({
        url: '/User/PrefillPersonalInfo',
        type: 'GET',
        success: function (response) {
            $("#oldUserName").text(response.userName);
            $("#inputUPUserName").val(response.userName);
            $("#inputUPPhoneNumber").val(response.phoneNumber);
        },
        error: function (error) {
        }
    });
}

function UpdatePersonalInfo() {
    var data = {
        oldUserName: $("#oldUserName").text(),
        userName: $("#inputUPUserName").val(),
        phoneNumber: $("#inputUPPhoneNumber").val()
    };

    var jsonData = JSON.stringify(data);

    $.ajax({
        type: 'POST',
        url: '/User/UpdatePersonalInfo',
        datatype: "text",
        data: { json: jsonData },
        success: function (response) {
            if (response.errorMsg) {
                $("#invalidUserName").text(response.errorMsg);
            }
            else {
                window.location.href = response.redirectToUrl;
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {

        }
    });
}

function GenerateNewPassword() {
    $.ajax({
        url: '/User/GenerateNewPassword',
        type: 'GET',
        success: function (response) {
            $("#inputUPNewPassword").val(response.newPassword);
        },
        error: function (error) {
        }
    });
}

function ChangePassword() {
    var data = {
        password: $("#inputUPOldPassword").val(),
        confirmPassword: $("#inputUPNewPassword").val()
    };

    var jsonData = JSON.stringify(data);

    $.ajax({
        type: 'POST',
        url: '/User/ChangePassword',
        datatype: "text",
        data: { json: jsonData },
        success: function (response) {
            if (response.errorMsg) {
                $("#invalidOldPassword").text(response.errorMsg);
            }
            else {
                window.location.href = response.redirectToUrl;
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {

        }
    });
}

function AddTimeOff(maxVacationDays, totalVacationDays) {
    var startDate = $('#upTimeOffStartDate').val();
    var endDate = $('#upTimeOffEndDate').val();
    var newTimeOff = CalculateDateDifferenceInDays(new Date(startDate), new Date(endDate));

    if (IsDateValid(new Date(startDate)) && IsDateValid(new Date(endDate)) && ((newTimeOff + totalVacationDays) <= maxVacationDays)) {
        var data = {
            startDate: startDate,
            endDate: endDate
        };

        var jsonData = JSON.stringify(data);

        $.ajax({
            type: 'POST',
            url: '/User/AddTimeOff',
            datatype: "text",
            data: { json: jsonData },
            success: function (response) {
                window.location.href = response.redirectToUrl;
            },
            error: function (jqXHR, textStatus, errorThrown) {

            }
        });

        $("#upTimeOffStartDate").removeClass("invalid-input");
        $("#upTimeOffEndDate").removeClass("invalid-input");
    }
    else {
        $("#upTimeOffStartDate").addClass("invalid-input");
        $("#upTimeOffEndDate").addClass("invalid-input");
    }
}

function IsDateValid(date) {
    return date instanceof Date && !isNaN(date);
}

function CalculateDateDifferenceInDays(startDate, endDate) {
    var difference = endDate.getTime() - startDate.getTime();
    var totalDays = Math.ceil(difference / (1000 * 3600 * 24));

    return totalDays + 1;
}
