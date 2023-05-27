
$(document).ready(function () {
    $('#upAddTimeOff').click(function () {
        AddTimeOff();
    });
});

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
