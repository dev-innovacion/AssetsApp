jQuery.noConflict();


jQuery(function($) {
   
    
  $('.datepicker').datepicker();

  //* Chosen plugin and easy tabs plugin on dashboard *//
  $('.chosen').chosen();
  
  $('#tab-container').easytabs({
    animationSpeed: 300,
    collapsible: false,
  });

  $('.typeahead').typeahead();

  function showTooltip(title, x, y, contents,data) {
     
    $('<div id="tooltip" class="chart-tooltip"><div class="date">' + title + '<\/div><div class="percentage">Porcentaje: <span>' + (x+184)/10 + '%<\/span><\/div><div class="visits">Activos: <span>' + data[0] + '<\/span><\/div><\/div>').css({
        position: 'absolute',
        display: 'none',
        top: y - 117,
        left: x - 91,
        'background-color': '#fff',
        border: '1px solid #5c5c5c'
    }).appendTo("body").fadeIn(200);
    }
  
      var d1 = [20000, 43000, 41000, 39000, 15000, 38400, 37182, 40200, 53400, 49200, 54039, 53092,
      58393, 59029, 69202, 64983, 57828, 74283, 69282, 71837, 77827, 79999, 80121, 83019,
      82938, 84213, 85232, 89322, 81093, 93029, 92030, 94232, 99323, 91323, 100323,
      100333, 101332, 110323, 111323, 103423, 108323, 112323, 115122, 113999, 114122, 115212, 116837,
      117212, 117799],

    d2 = [10000, 23000, 21000, 21000, 25000, 38400, 37182, 30200, 33400, 29200, 24039, 33092,
      38393, 39029, 43202, 41983, 37828, 54283, 49282, 41837, 57827, 59999, 50121, 53019,
      62938, 64213, 65232, 79322, 71093, 63029, 62030, 64232, 59323, 51323, 70323,
      67333, 71332, 80323, 81323, 81423, 93235, 83235, 71225, 69998, 68122, 55212, 68837,
      88212, 70799],

    options = {
        series: {
            lines: {
                show: true,
                fill: true,
                lineWidth: 2,
                steps: false,
                fillColor: { colors: [{ opacity: 0.25 }, { opacity: 0 }] }
            },
            points: {
                show: true,
                radius: 4,
                fill: true,
                lineWidth: 1.5
            }
        },
        tooltip: true,
        tooltipOpts: {
            content: '%s: %y'
        },
        xaxis: {
            mode: "time", minTickSize: [1, "hour"]
        },
        grid: { borderWidth: 0, hoverable: true },
        legend: {
            show: false
        }
    };
      var dt1 = [], dt2 = [], st = new Date(2014, 3, 1).getTime();

      for (var i = 0; i < d2.length; i++) {

          dt1.push([st + i * 3600000, parseFloat((d1[i]).toFixed(3))]);
          dt2.push([st + i * 3600000, parseFloat((d2[i]).toFixed(3))]);
      }

      var data = [
          { data: dt1, color: '#0072c6', label: 'This month sales', lines: { lineWidth: 1.5 } },
          { data: dt2, color: '#ac193d', label: 'Last month profit', points: { show: false }, lines: { lineWidth: 2, fill: false } }
      ];

      $.plot($("#chartLine01"), data, options);
  var previousPoint = null;
  $("#chartLine01").bind("plothover", function (event, pos, item) {
    $("#x").text(pos.x.toFixed(2));
    $("#y").text(pos.y.toFixed(2));
     
    if (item) {
      
        if (previousPoint != item.dataIndex) {
            previousPoint = item.dataIndex;

            $("#tooltip").remove();
       
            var x = item.datapoint[0].toFixed(2),
                y = item.datapoint[1].toFixed(2);
                
                var d = new Date(item.datapoint[0]);
                var activ = [item.datapoint[1], item.datapoint[0]];
      var monthNames = ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",  
        "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"];  
        var current_month = d.getMonth();  
        var month_name = monthNames[current_month]; 
      var day = d.getDate();
            
      var time = (d.getHours()<10?'0':'') + d.getHours() + ":" + (d.getMinutes()<10?'0':'') + d.getMinutes();
     
      var output = ((''+day).length<2 ? '0' : '') + day + ' ' +
      ((''+month_name).length<2 ? '0' : '') + month_name + ', ' +
      d.getFullYear() + '<span class="clock">' + time + '</span>';
     
            showTooltip(output, item.pageX, item.pageY, item.series.label + " of " + x + " = " + y,activ);
        }
    } else {
        $("#tooltip").remove();
        previousPoint = null;
    }
  });

  var d1 = [[1262304000000, 2043], [1264982400000, 2564], [1267401600000, 2043], [1270080000000, 2198], [1272672000000, 2660], [1275350400000, 2782], [1277942400000, 2430], [1280620800000, 2427], [1283299200000, 2100], [1285891200000, 1214], [1288569600000, 1557], [1291161600000, 2645]];
 
  var data1 = [
      { 
          label: "Earnings", 
          data: d1, 
          color: '#ac193d' 
      }
  ];
  /*
  $.plot($("#sidebarchart"), data1, {
      xaxis: {
          show: true,
          min: (new Date(2009, 12, 1)).getTime(),
          max: (new Date(2010, 11, 2)).getTime(),
          mode: "time",
          tickSize: [1, "month"],
          monthNames: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
          tickLength: 1,
          axisLabel: 'Month',
          axisLabelFontSizePixels: 11
      },
      yaxis: {
          axisLabel: 'Amount',
          axisLabelUseCanvas: true,
          axisLabelFontSizePixels: 11,
          autoscaleMargin: 0.01,
          axisLabelPadding: 5
      },
      series: {
          lines: {
              show: true, 
              fill: true,
              fillColor: { colors: [ { opacity: 0.5 }, { opacity: 0.2 } ] },
              lineWidth: 1.5
          },
          points: {
              show: true,
              radius: 2.5,
              fill: true,
              fillColor: "#ffffff",
              symbol: "circle",
              lineWidth: 1.1
          }
      },
     grid: { hoverable: true, clickable: true },
      legend: {
          show: false
      }
  });

  var d1 = [[1262304000000, 2043], [1264982400000, 2564], [1267401600000, 2043], [1270080000000, 2198], [1272672000000, 2660], [1275350400000, 2782], [1277942400000, 2430], [1280620800000, 2427], [1283299200000, 2100], [1285891200000, 1214], [1288569600000, 1557], [1291161600000, 2645]];
 
  var data1 = [
      { 
          label: "Earnings", 
          data: d1, 
          color: '#82ba00' 
      }
  ];
  
  $.plot($("#sidebarchart2"), data1, {
      xaxis: {
          show: true,
          min: (new Date(2009, 12, 1)).getTime(),
          max: (new Date(2010, 11, 2)).getTime(),
          mode: "time",
          tickSize: [1, "month"],
          monthNames: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
          tickLength: 1,
          axisLabel: 'Month',
          axisLabelFontSizePixels: 11
      },
      yaxis: {
          axisLabel: 'Amount',
          axisLabelUseCanvas: true,
          axisLabelFontSizePixels: 11,
          autoscaleMargin: 0.01,
          axisLabelPadding: 5
      },
      series: {
          lines: {
              show: true, 
              fill: true,
              fillColor: { colors: [ { opacity: 0.5 }, { opacity: 0.2 } ] },
              lineWidth: 1.5
          },
          points: {
              show: true,
              radius: 2.5,
              fill: true,
              fillColor: "#ffffff",
              symbol: "circle",
              lineWidth: 1.1
          }
      },
     grid: { hoverable: true, clickable: true },
      legend: {
          show: false
      }
  });
  */

});