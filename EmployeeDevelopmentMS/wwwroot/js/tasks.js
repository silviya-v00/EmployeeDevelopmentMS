
$(document).ready(function () {
    var userRole = $('#hdnUserRole').val();

    if (userRole == 'EMPLOYEE') {
        $("#btnSaveTask").prop('disabled', true);
    }

    $("#ddRatePoints").prop('disabled', true);
    $("#ddRatePoints").parent(".form-group").css('visibility', 'hidden');

    $('#btnSaveTask').click(function () {
        if (userRole == 'EMPLOYEE') {
            CompleteTask();
        }
        else {
            if ($('#hdnTaskIsCompleted').val() == "true") {
                RateTask();
            }
            else {
                SaveTask();
            }
        }
    });
});

function SaveTask() {
    var employee = {
        employeeID: $('#ddEmployees').val()
    };

    var data = {
        taskID: $("#hdnTaskID").val(),
        taskTitle: $('#inputTaskTitle').val(),
        taskDescription: $('#inputTaskDescription').val(),
        employee: employee,
        estimatedHours: $('#inputEstHours').val(),
        createdByID: $('#hdnCreatedByID').val()
    };

    var jsonData = JSON.stringify(data);

    $.ajax({
        type: 'POST',
        url: '/User/SaveTask',
        datatype: "text",
        data: { json: jsonData },
        success: function (response) {
            window.location.href = response.redirectToUrl;
        },
        error: function (jqXHR, textStatus, errorThrown) {

        }
    });
}

function CompleteTask() {
    var data = {
        taskID: $("#hdnTaskID").val(),
        workedHours: $("#inputWorkedHours").val()
    };

    var jsonData = JSON.stringify(data);

    $.ajax({
        type: 'POST',
        url: '/User/CompleteTask',
        datatype: "text",
        data: { json: jsonData },
        success: function (response) {
            window.location.href = response.redirectToUrl;
        },
        error: function (jqXHR, textStatus, errorThrown) {

        }
    });
}

function RateTask() {
    var data = {
        taskID: $("#hdnTaskID").val(),
        ratePoints: $("#ddRatePoints").val()
    };

    var jsonData = JSON.stringify(data);

    $.ajax({
        type: 'POST',
        url: '/User/RateTask',
        datatype: "text",
        data: { json: jsonData },
        success: function (response) {
            window.location.href = response.redirectToUrl;
        },
        error: function (jqXHR, textStatus, errorThrown) {

        }
    });
}

function EditTask(taskID) {
    var taskTitle = $('#task-' + taskID + ' > #taskTitle').text();
    var taskDescription = $('#task-' + taskID + ' > #taskDescription').text();
    var employee = $('#task-' + taskID + ' > div > div > span#employee').attr('data-id');
    var workedHours = $('#task-' + taskID + ' > div > div > span#hours').attr('worked-hours');
    var estHours = $('#task-' + taskID + ' > div > div > span#hours').attr('est-hours');
    var isCompleted = $('#task-' + taskID + ' > div > span#hdnIsCompleted').text().toLowerCase();
    var ratePoints = $('#task-' + taskID + ' > div > span#hdnRatePoints').text();

    $('#hdnTaskID').val(taskID);
    $('#inputTaskTitle').val(taskTitle);
    $('#inputTaskDescription').val(taskDescription);
    $('#ddEmployees').val(employee);
    $('#inputWorkedHours').val(workedHours);
    $('#inputEstHours').val(estHours);
    $('#hdnTaskIsCompleted').val(isCompleted);
    $('#ddRatePoints').val(ratePoints);

    $("#btnSaveTask").prop('disabled', false);

    var userRole = $('#hdnUserRole').val();

    if (userRole == 'EMPLOYEE') {
        $("#inputTaskTitle").prop('disabled', true);
        $("#inputTaskDescription").prop('disabled', true);
        $("#ddEmployees").prop('disabled', true);
        $("#inputEstHours").prop('disabled', true);
        $("#inputWorkedHours").prop('disabled', false);
    }
    else {
        if (isCompleted == "true") {
            $("#inputTaskTitle").prop('disabled', true);
            $("#inputTaskDescription").prop('disabled', true);
            $("#ddEmployees").prop('disabled', true);
            $("#inputEstHours").prop('disabled', true);
            $("#inputWorkedHours").prop('disabled', true);

            $("#ddRatePoints").prop('disabled', false);
            $("#ddRatePoints").parent(".form-group").css('visibility', '');
        }
        else {
            $("#inputTaskTitle").prop('disabled', false);
            $("#inputTaskDescription").prop('disabled', false);
            $("#ddEmployees").prop('disabled', false);
            $("#inputEstHours").prop('disabled', false);
            $("#inputWorkedHours").prop('disabled', true);

            $("#ddRatePoints").prop('disabled', true);
            $("#ddRatePoints").parent(".form-group").css('visibility', 'hidden');
        }
    }
}

function DeleteTask(taskID) {

    var data = {
        taskID: taskID.toString()
    };

    var jsonData = JSON.stringify(data);

    $.ajax({
        type: 'POST',
        url: '/User/DeleteTask',
        datatype: "text",
        data: { json: jsonData },
        success: function (response) {
            window.location.href = response.redirectToUrl;
        },
        error: function (jqXHR, textStatus, errorThrown) {

        }
    });
}
