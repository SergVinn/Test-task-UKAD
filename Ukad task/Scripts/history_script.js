let FillTable = (reports) => {
    $(document).ready(() => {
        reports = reports.Reports.map((e) => {
            return {
                Url: e.Url,
                SpendTime: e.SpendTime,
                Message: e.Message
            };
        });
        let grid = $("#Table").data().kendoGrid;
        grid.dataSource.data().empty();
        reports.forEach((e) => {
            grid.dataSource.add(e);
        });
    });
    $("#spinner").hide();
    $(".container.body-content").show();
};

$(document).ready(() => {
    $("#Table").kendoGrid({
        toolbar: '<button id="remove-reports">Remove all reports</button>',
        columns: [
        {
            field: "Url",
            title: "Address"
        },
        {
            field: "SpendTime",
            title: "Time"
        },
        {
            field: "Message",
            title: "Message"
        }
        ],
        sortable: true,
        pageable: {
            pageSize: 20
        }
    });

    $("#remove-reports").click(() =>{
        $("#spinner").show();
        $(".container.body-content").hide();
        $.ajax({
        url: "/Home/RemoveAllReports",
        type: 'POST'
    }).then(
        FillTable,
        function (e) {
            alert("Could not remove data");
        });
    });
});

$.ajax({
    url: "/Home/GetAllReports",
    type: 'POST'
    }).then(
        FillTable,
        function (e) {
            alert("Could not retrive data");
        });