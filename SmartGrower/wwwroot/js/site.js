function drawChart(dispositivoId) {

    $.ajax(
        {
            url: '/Home/Last24Hour',
            dataType: "json",
            data: { },
            type: "GET",
            success: function (jsonData) {
                var data = new google.visualization.DataTable(jsonData);
                var options = { chart: { title: 'Leituras - último dia' } };
                var chart = new google.charts.Line(document.getElementById('chart_div'));
                chart.draw(data, options);
            }
        });

    return false;
}


