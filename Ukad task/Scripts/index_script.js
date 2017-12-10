let table;

function FillTable(reports) {
    reports = reports.map((e) => {
        return {
            Url: e.Url,
            SpendTime: e.SpendTime,
            Message: e.Message
        };
    });

    table.kendoGrid.dataSource.data().empty();
    reports.forEach((e) => {
        table.kendoGrid.dataSource.add(e);
    });

    var titels = reports.map((e) => {
        var str = e.Url.slice(0, 50);
        if (e.Url.length > str.length) {
            str = str + "...";
        }
        return str;
    });
    var values = reports.map((e) => {
        return e.SpendTime;
    });

    $("#spinner").hide();
    $(".container.body-content").show();

    $("#chart").kendoChart({
        categoryAxis: {
            categories: titels,
            title: {
                text: "URL's"
            },
            labels: {
                rotation: 270
            }
        },
        chartArea: {
            height: 600
        },
        series: [
          { data: values }
        ]
    });
}

function SendPost(actionPath) {
    let URL = $("#URL").val();
    let Timeout = $("#Timeout").val();
    let IncludeInnerSitemaps = $("#IncludeInnerSitemaps").prop("checked");
    $.ajax({
        url: actionPath,
        type: 'POST',
        dataType: 'json',
        data: {
            URL: URL,
            Timeout: Timeout,
            IncludeInnerSitemaps: IncludeInnerSitemaps
        }
    }).then(
        function (data) {
            FillTable(data.Reports);
        },
        function (e) {
            $("#spinner").hide();
            $(".container.body-content").show();
            e.responseJSON.forEach((error) => {
                error.errors.forEach((message) => {
                    console.dir($('[data-valmsg-for="' + error.key + '"]').text(message));
                });
            });
        }
    );
}

$("#apply-btn").click(() => {
    $("#spinner").show();
    $(".container.body-content").hide();

    $(".field-validation-valid").text("");
    SendPost("/Home/TestSpeed");
});

table = $("#table").kendoGrid({
    columns: [
        {
            field: "Url",
            title: "Url"
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
}).data();

$("#chart").kendoChart({
    categoryAxis: {
        title: {
            text: "URL's"
        },
        labels: {
            rotation: 270
        }
    },
    chartArea: {
        height: 600
    }
});
$("#spinner").hide();
$(".container.body-content").show();