var table;
function UploadSalaryFiles(event) {
    //stop submit the form, we will post it manually.
    event.preventDefault();
    $("#DownLoadZip").prop("disabled", true);
    $("#SendMail").prop("disabled", true);

    // Get form
    var form = $('#fileUploadForm')[0];

    // Create an FormData object
    var data = new FormData(form);
    //data.append("salaryDay", $('#SalaryMonthYear')[0]);

    $("#divLoading").show();
    $.ajax({
        type: "POST",
        enctype: 'multipart/form-data',
        url: "/PaySlip/LoadFiles?salaryDay='" + $('#SalaryMonthYear').val() + "'",
        data: data,
        processData: false,
        contentType: false,
        cache: false,
        //timeout: 600000,
        success: function (retData) {
            $("#divForAllSalarySlip").empty();
            $("#divForAllSalarySlip").html(retData);
            $("#paySlipTable").dataTable({ paging: false });
            $("#divLoading").hide();
        },
        error: function (e) {
            $("#divLoading").hide();
            alert(e.responseText);
        }
    });
};

function GeneratePDF() {
    $("#divLoading").show();
    $("#divForAllSalarySlip").empty();
    $("#divForAllSalarySlip").load("/PaySlip/LoadAllPaySlip?salaryDay='" + $('#SalaryMonthYear').val() + "'",
        function() {
            $("#divLoading").hide();
            table = $("#paySlipTable").DataTable({
                paging: false,
                columnDefs: [
                    { targets: 'no-sort', orderable: false, searchable: false }
                ],
                order: [[1, 'asc']]
            });
            $("html, body").animate({
                    scrollTop: 210 // Means Less header height
                },
                400);
            $('#select-all').on('click',
                function() {
                    var rows = table.rows({ 'search': 'applied' }).nodes();
                    $('input[type="checkbox"]', rows).prop('checked', this.checked);
                });
            $('#paySlipTable tbody').on('change', 'input[type="checkbox"]', function () {
                
                // If checkbox is not checked
                if (!this.checked) {
                    var el = $('#select-all').get(0);
                    // If "Select all" control is checked and has 'indeterminate' property
                    if (el && el.checked && ('indeterminate' in el)) {
                        // Set visual state of "Select all" control
                        // as 'indeterminate'
                        el.indeterminate = true;
                    }
                }
            });
            $('#paySlipTable').on('change', 'input[type="checkbox"]', function() {
                var counterChecked = $('input[name="checkedValues"]:checked').length;
                //disbale or enable the download button
                //this.checked ? counterChecked++ : counterChecked--;
                $('#DownLoadZip').prop("disabled", counterChecked <= 0);
                $('#SendMail').prop("disabled", counterChecked <= 0);
            });

        }
    );
}

function SendMail() {
    $("#divLoading").show();
    var checkedValues = table.$('input:checkbox:checked').map(function () {
        return $(this).val();
    }).get();
    var postData = { values: checkedValues}
    $.ajax({
        type: 'POST',
        url: '/PaySlip/SendMail',
        //contentType: 'application/json; charset=utf-8',
        data: postData,
        success: function (emailLogMessage) {
            $("#divLoading").hide();
            $("#mailDialog").text(emailLogMessage);
            $(function () {
                $("#myModal").modal();
            });
        },
        error: function (xhr, status, error) {
            $("#divLoading").hide();
            $("#mailDialog").text(error);
            $(function () {
                $("#myModal").modal();
            });
        }
    });
}

function DownLoadFiles() {
    $("#divLoading").show();
    var checkedValues = table.$('input:checkbox:checked').map(function () {
        return $(this).val();
    }).get();
    var postData = JSON.stringify(checkedValues);

    window.open('/PaySlip/DownloadSalarySlip?values=' + postData);

    $("#divLoading").hide();
}

