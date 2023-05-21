
$(document).ready(function () {

    $('#btnSaveTask').click(function () {
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
    });
});

function EditTask(taskID) {
    var taskTitle = $('#task-' + taskID + ' > #taskTitle').text();
    var taskDescription = $('#task-' + taskID + ' > #taskDescription').text();
    var employee = $('#task-' + taskID + ' > div > div > span#employee').attr('data-id');
    var workedHours = $('#task-' + taskID + ' > div > div > span#hours').attr('worked-hours');
    var estHours = $('#task-' + taskID + ' > div > div > span#hours').attr('est-hours');

    $('#hdnTaskID').val(taskID);
    $('#inputTaskTitle').val(taskTitle);
    $('#inputTaskDescription').val(taskDescription);
    $('#ddEmployees').val(employee);
    $('#inputEstHours').val(estHours);
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
