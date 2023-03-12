
$(document).ready(function () {
    $("#chkActivateAll").click(function () {
        $('input:checkbox').not(this).prop('checked', this.checked);
    });

    $('#btnSave').click(function () {
        var data = [];
        $('#allUsersTable tbody tr').each(function () {
            var id = $(this).data('id');
            var isActive = $(this).find('.chkUserActivation').prop('checked');
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
    });

    $('.selectpicker').selectpicker();
});