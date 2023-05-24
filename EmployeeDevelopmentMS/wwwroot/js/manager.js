
$(document).ready(function () {
    $("#rowPositionActions").css('display', 'none');

    $("#ddEmployees").change(function () {
        SelectEmployee();
    });

    $('#btnSavePositions').click(function () {
        SavePositions();
    });
});

function SelectEmployee() {
    var employee = {
        userID: $('#ddEmployees').val()
    };

    var data = {
        employee: employee
    };

    var jsonData = JSON.stringify(data);

    $.ajax({
        type: 'POST',
        url: '/User/GetUserPositions',
        datatype: "text",
        data: { json: jsonData },
        success: function (response) {
            ReloadTable(response);
        },
        error: function (jqXHR, textStatus, errorThrown) {

        }
    });
}

function ReloadTable(filteredData) {

    $('#allPositionsTable tbody').empty();

    if (filteredData != null && filteredData.length > 0) {
        filteredData.forEach(function (element) {
            var row = '<tr data-id=' + element.positionID + '>';
            row += '<td>' + element.employee.userName + '</td>';
            row += '<td>' + element.position + '</td>';
            row += '<td>' + (element.salary ? element.salary : "") + '</td>';
            row += '<td>' + (element.startDate ? FormatDate(element.startDate) : "") + '</td>';
            row += '<td>' + (element.endDate ? FormatDate(element.endDate) : "<input type='date' id='prevPositionEndDate' class='form-control' style='width: 150px; display: inline-block;'>") + '</td>';
            row += '<td></td>';
            row += '</tr>';

            $('#allPositionsTable tbody').append(row);
        });
    }

    if ($('#ddEmployees').val() != "-1") {
        $("#iconAddNewPosition").css('display', '');
        $("#rowPositionActions").css('display', '');
    }
    else {
        $("#rowPositionActions").css('display', 'none');
    }
}

function FormatDate(dateStr) {
    var date = new Date(dateStr);

    return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit'
    });
}

function AddNewPositionRow() {
    var rowID = "newPositionRow";
    var row = '<tr id=' + rowID + '>';
    row += '<td>' + $('#ddEmployees option:selected').text() + '</td>';
    row += '<td>' + "<input type='text' class='form-control' id='newPositionTitle'>" + '</td>';
    row += '<td>' + "<input type='number' min='0' class='form-control' id='newPositionSalary'>" + '</td>';
    row += '<td>' + "<input type='date' id='newPositionStartDate' class='form-control' style='width: 150px; display: inline-block;'>" + '</td>';
    row += '<td>' + "<input type='date' id='newPositionEndDate' class='form-control' style='width: 150px; display: inline-block;'>" + '</td>';
    row += '<td>' + "<i class='fa-solid fa-trash-can' onclick='RemoveNewPositionRow(\"" + rowID + "\")'></i>" + '</td>';
    row += '</tr>';

    $('#allPositionsTable tbody').append(row);
    $("#iconAddNewPosition").css('display', 'none');
}

function RemoveNewPositionRow(positionID) {
    $('#allPositionsTable tr#' + positionID).remove();
    $("#iconAddNewPosition").css('display', '');
}

function SavePositions() {
    var selUserID = $('#ddEmployees').val();
    var newPosition = $('#newPositionTitle').val();
    var newSalary = $('#newPositionSalary').val();
    var newStartDate = $('#newPositionStartDate').val();
    var newEndDate = $('#newPositionEndDate').val();
    var hasNewPositionCheck = "false";

    if ($('#newPositionRow').length && newPosition != "" && newSalary != "" && newStartDate != "") {
        hasNewPositionCheck = "true";
    }
    else {
        hasNewPositionCheck = "false";
    }

    var employee = {
        userID: $('#ddEmployees').val()
    };

    var data = {
        employee: employee,
        position: newPosition,
        salary: newSalary,
        startDate: newStartDate,
        endDate: newEndDate,
        previousPositionEndDate: $('#prevPositionEndDate').val(),
        hasNewPositionRow: hasNewPositionCheck
    };

    var jsonData = JSON.stringify(data);

    $.ajax({
        type: 'POST',
        url: '/User/SavePosition',
        datatype: "text",
        data: { json: jsonData },
        success: function (response) {
            $('#ddEmployees').val(selUserID).trigger('change');
        },
        error: function (jqXHR, textStatus, errorThrown) {

        }
    });
}
