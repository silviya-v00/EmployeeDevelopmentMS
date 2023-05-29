
$(document).ready(function () {
    $('#btnSaveCourseStatus').click(function () {
        SaveCourseStatus();
    });
});

function SaveCourseStatus() {
    var data = [];
    $('#allCoursesTable tbody tr').each(function () {
        var id = $(this).data('id');
        var isCompleted = $(this).find('.chkCourseStatus').prop('checked');
        data.push({ CourseID: id, IsCompleted: isCompleted });
    });

    var jsonData = JSON.stringify(data);

    $.ajax({
        type: 'POST',
        url: '/User/SaveCourseStatus',
        datatype: "text",
        data: { json: jsonData },
        success: function (response) {
            window.location.href = response.redirectToUrl;
        },
        error: function (jqXHR, textStatus, errorThrown) {

        }
    });
}
