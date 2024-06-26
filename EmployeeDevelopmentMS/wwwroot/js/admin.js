﻿
$(document).ready(function () {
    $("#chkActivateAll").click(function () {
        $('input:checkbox.inactiveChk').not(this).prop('checked', this.checked);
    });

    $("#chkDeactivateAll").click(function () {
        $('input:checkbox.activeChk').not(this).prop('checked', this.checked);
    });

    $('#btnSearch').click(function () {
        SearchUsers();
    });
});

function SaveActiveStatus(targetTableID, isForActivation) {
    var data = [];
    $('#' + targetTableID + ' tbody tr').each(function () {
        var id = $(this).data('id');
        var isActive = $(this).find('.chkUserActivation').prop('checked');
        if (!isForActivation) {
            isActive = !isActive;
        }
        data.push({ UserID: id, IsActive: isActive });
    });

    $.ajax({
        contentType: 'application/json; charset=UTF-8',
        dataType: 'json',
        type: 'POST',
        url: 'SaveActivation',
        data: JSON.stringify(data),
        success: function (response) {
            window.location.href = response.redirectToUrl;
        },
        error: function (jqXHR, textStatus, errorThrown) {

        }
    });
}

function SearchUsers() {
    var data = {
        companyIDs: $("#companies").val().join(','),
        firstName: $('#firstName').val(),
        lastName: $('#lastName').val(),
        roleKey: $('#role').val(),
        status: $('#status').val()
    };

    var jsonData = JSON.stringify(data);

    $.ajax({
        type: 'POST',
        url: '/User/SearchUsers',
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

    $('#allUsersTable tbody').empty();

    if (filteredData != null && filteredData.length > 0) {
        filteredData.forEach(function (element) {
            var row = '<tr data-id=' + element.userID + '>';
            row += '<td>' + element.companyName + '</td>';
            row += '<td>' + element.firstName + '</td>';
            row += '<td>' + element.lastName + '</td>';
            row += '<td>' + element.role.roleName + '</td>';
            row += '<td>' + (element.registrationDate ? FormatDate(element.registrationDate) : "") + '</td>';
            row += '<td>' + (element.lastLoginDate ? FormatDate(element.lastLoginDate) : "") + '</td>';
            row += '<td>' + (element.isActive ? "Активен" : "Неактивен") + '</td>';
            row += '</tr>';

            $('#allUsersTable tbody').append(row);
        });
    }
}
