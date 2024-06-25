var dataTable;
function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/order/getall'},
        "columns": [
            { data: 'id', "width": "5%" },
            { data: 'name', "width": "15%" },
            { data: 'phoneNumber', "width": "15%" },
            { data: 'applicationUser.email', "width": "20%" },
            { data: 'orderStatus', "width": "10%" },
            { data: 'orderTotal', "width": "10%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-100 btn-group text-center" role="group">
                        <a href="/admin/order/details?orderId=${data}" class="btn btn-primary mx-2 rounded-pill"> <i class="bi bi-pencil-square"></i> Edit</a>
                       
                    </div>`
                },
                "width": "25%"
            }
        ]
    });
}

$(document).ready(function () {
    loadDataTable();
});
