Number.prototype.format = function(n, x) {
    var re = '\\d(?=(\\d{' + (x || 3) + '})+' + (n > 0 ? '\\.' : '$') + ')';
    return this.toFixed(Math.max(0, ~~n)).replace(new RegExp(re, 'g'), '$&,');
};

RivkaViewer.METHODS = { TABLE: 0, TILE: 1, SLIDER: 2, NONE: null };

function RivkaViewer(containerID) {
    if (typeof containerID == "undefined") throw "RivkaViewer: A container id value must be gived.";
    if (!jQuery("#" + containerID).length) throw "RivkaViewer: The specified element's id:" + containerID + " does not exist in this context.";

    var containerID = containerID;
    var containerElement = jQuery("#" + containerID);
    var displayMethod = RivkaViewer.METHODS.NONE;
    var displayOptions = null;
    var dataOptions = null; // An object with the following parameters {id: "", name: "", image: "", permissions: [], data: {key:"value"} }
    var data = null; //Data to be printed
    var extraAction = new Object();

    var sum = function (field) {
        var result = 0;
        for (var i = 0; i < data.length; i++) {
            if (!isNaN(data[i].profileFields["_HTKField" + field])) {
                result += parseFloat(data[i].profileFields["_HTKField" + field]);
            }
        }
        return result;
    };

    var min = function (field) {
        if (data == null) return null;
        var min = null;
        var value = null;
        for (var i = 0; i < data.length; i++) {
            if (!isNaN(data[i].profileFields["_HTKField" + field])) {
                value = parseFloat(data[i].profileFields["_HTKField" + field])
                if ((min == null || min > value) && value != 0) {
                    min = value;
                }
            }
        }
        return min;
    };

    var max = function (field) {
        if (data == null) return null;
        var max = null;
        var value = null;
        for (var i = 0; i < data.length; i++) {
            if (!isNaN(data[i].profileFields["_HTKField" + field])) {
                value = parseFloat(data[i].profileFields["_HTKField" + field])
                if (max == null || max < value) {
                    max = value;
                }
            }
        }
        return max;
    };

    this.doOperation = function (field, operation) {
        var result;
        switch (operation) {
            case "Sumatoria":
                result = sum(field);
                break;
            case "Promedio":
                result = sum(field) / data.length;
                break;
            case "Minimo":
                result = min(field);
                break;
            case "Maximo":
                result = max(field);
                break;
            default:
                break;
        }
        return result.format(2, 3);
    };

    this.addOption = function (option) {
        extraAction[option.name] = option;
    };

    this.getOptions = function () {
        return extraAction;
    };

    this.print = function () {
        containerElement.empty();
        switch (displayMethod) {
            case RivkaViewer.METHODS.TABLE:
                table.print();
                break;
            case RivkaViewer.METHODS.TILE:
                tile.print();
                break;
            case RivkaViewer.METHODS.SLIDER:
                slider.print();
                break;
        }
        containerElement.addClass("tab-rivkaviewer");
    };

    this.setData = function (dataString, options) {
        if (typeof data == "undefined") throw "RivkaViewer: data must be defined.";
        if (typeof options == "undefined") throw "RivkaViewer: options are required to set Viewer data.";
        if (typeof options.permissions == "undefined") console.error("RivkaViewer: permissions are required to set Viewer data.");
        try {
            data = JSON.parse(dataString);
        } catch (Exception) { throw "RivkaViewer: an error ocurred while parsing data."; }
        dataOptions = options;
    };

    this.getData = function () {
        return data;
    };

    this.setContainer = function (containerID) {
        if (typeof containerID == "undefined") throw "RivkaViewer: A container id value must be gived";
        if (!jQuery("#" + containerID).length) throw "RivkaViewer: The specified element's id:" + containerID + " does not exist in this context.";
        container = containerID;
        containerElement = jQuery("#" + containerID);
        displayMethod = RivkaViewer.METHODS.NONE;
    };

    this.getContainer = function () {
        return container;
    };

    this.setDisplayMethod = function (method, options) {
        if (typeof method == "undefined") throw "RivkaViewer: A RivkaViewer method must be gived as display method.";
        switch (method) {
            case RivkaViewer.METHODS.TABLE:
                displayMethod = RivkaViewer.METHODS.TABLE;
                if (typeof options == "undefined" || options.headers == undefined)
                    throw "RivkaViewer: some options are missing.";
                break;
            case RivkaViewer.METHODS.TILE:
                displayMethod = RivkaViewer.METHODS.TILE;
                break;
            case RivkaViewer.METHODS.SLIDER:
                displayMethod = RivkaViewer.METHODS.SLIDER;
                break;
            default:
                throw "RivkaViewer: Unknow display method for RivkaViewer.";
        }
        displayOptions = options;
    };

    this.getDisplayMethod = function () {
        return displayMethod;
    };

    var slider = {
        elementToPrint: 0,

        colors: ["orange", "blue", "gray", "purple", "green"],

        lineSize: 5,

        print: function () {
            slider.elementToPrint = 0;
            if (data == null) return;
            var sliderElement = jQuery("<div/>").addClass("sliderContainer ");
            sliderElement.append(
                jQuery("<div/>").addClass("sliderImage sliderdefault")
                .append(jQuery("<div/>").addClass("sliderDescription"))
            );
            var sliderContent = jQuery("<div/>").addClass("line");
            sliderContent.append(jQuery("<div/>").addClass("prev").bind("click.prev", function () { slider.prev(tilesContainer); }));
            var tilesContainer = jQuery("<div/>").addClass("tilesContainer");
            sliderContent.append(tilesContainer);
            sliderContent.append(jQuery("<div/>").addClass("next").bind("click.next", function () { slider.next(tilesContainer); }));

            for (; slider.elementToPrint < slider.lineSize && slider.elementToPrint < data.length; slider.elementToPrint++) {
                var tile = slider.createNode({
                    id: data[slider.elementToPrint][dataOptions.id],
                    name: data[slider.elementToPrint][dataOptions.name],
                    image: data[slider.elementToPrint][dataOptions.image],
                    color: slider.colors[slider.elementToPrint],
                    data: data[slider.elementToPrint]
                });
                tilesContainer.append(tile);
            }

            sliderElement.append(sliderContent);
            containerElement.append(sliderElement);
            sliderContent.find(".tile:first").mouseover();
        },

        next: function (tilesContainer) {

            if (slider.elementToPrint < data.length)
                tilesContainer.empty();
            for (i = 0; i < slider.lineSize && slider.elementToPrint < data.length; slider.elementToPrint++, i++) {
                var tile = slider.createNode({
                    id: data[slider.elementToPrint][dataOptions.id],
                    name: data[slider.elementToPrint][dataOptions.name],
                    image: data[slider.elementToPrint][dataOptions.image],
                    color: slider.colors[i],
                    data: data[slider.elementToPrint]
                });
                tilesContainer.append(tile);
            }
            tilesContainer.find(".tile:first").mouseover();
        },

        prev: function (tilesContainer) {

            if (slider.elementToPrint > 0)
                tilesContainer.empty();

            slider.elementToPrint = slider.elementToPrint - slider.elementToPrint % 5;
            slider.elementToPrint = (slider.elementToPrint - 5 > 0) ? slider.elementToPrint - 5 : 0;
            for (i = 0; i < slider.lineSize && slider.elementToPrint < data.length; slider.elementToPrint++, i++) {
                var tile = slider.createNode({
                    id: data[slider.elementToPrint][dataOptions.id],
                    name: data[slider.elementToPrint][dataOptions.name],
                    image: data[slider.elementToPrint][dataOptions.image],
                    color: slider.colors[i],
                    data: data[slider.elementToPrint]
                });
                tilesContainer.append(tile);
            }
            tilesContainer.find(".tile:first").mouseover();
        },

        createNode: function (obj) {
            var tile = jQuery("<div/>").addClass("tile ");
            if (typeof obj.image != "undefined") {
                tile.css("background", "url('" + obj.image + "')");

                tile.css("background-repeat", "no-repeat");
                tile.css("background-position", "50% 0%");
            } else {
                
                if (dataOptions.name == "user") {
                 tile.css("background-image", "url(/Content/Images/imgPerfil/avatar_06.png)");
                } else {
                   
                    tile.addClass(obj.color)
                }
                tile.css("background-repeat", "no-repeat");
                tile.css("background-position", "50% 0%")
            }
          
            tile.attr("data-id", obj.id);
            debugger;
            if (typeof dataOptions.data != "undefined") {
                var keys = Object.keys(dataOptions.data);
                for (key in keys) {
                    tile.attr("data-" + keys[key], obj.data[dataOptions.data[keys[key]]]);
                }
            }

            var div = jQuery("<div/>").addClass("btn-group");


            if (typeof dataOptions.permissions != "undefined" && dataOptions.permissions[0] == "u") {

                div.append(
                    jQuery("<a/>").addClass("btn btn-mini")
                        .append(jQuery("<i>").addClass("icon-edit"))
                        .attr("data-original-title", "Editar")
                        .attr("rel", "tooltip")
                        .attr("data-placement", "top")
                        .bind("click.edit", displayOptions.onEditTileAction)
                );
            }
            if (typeof dataOptions.permissions != "undefined" && dataOptions.permissions[1] == "d") {
                div.append(
                    jQuery("<a/>").addClass("btn btn-mini")
                        .attr("href", "#")
                        .attr("data-original-title", "Eliminar")
                        .attr("rel", "tooltip")
                        .attr("data-placement", "top")
                        .append(jQuery("<i>").addClass("icon-trash"))
                        .bind("click.delete", displayOptions.onDeleteTileAction)
                );
            }

            var actionKeys = Object.keys(extraAction);
            for (key in actionKeys) {
                var newOption = jQuery("<a/>").addClass("btn btn-mini")
                    .append(jQuery("<i>").addClass(extraAction[actionKeys[key]].icon))
                    .bind("click." + extraAction[actionKeys[key]].name, extraAction[actionKeys[key]].action);
                newOption.attr("data-original-title", extraAction[actionKeys[key]].description)
                    .attr("rel", "tooltip")
                    .attr("data-placement", "top");
                div.append(newOption);
            }

            tile.append(div);
            tile.append(jQuery("<div/>").addClass("tileName").text(obj.name));

            tile.bind("mouseover.displayDescription", function () {
                jQuery(this).siblings().removeClass("selected");
                jQuery(this).addClass("selected");

                var bg = jQuery(this).css("background");
                jQuery(this).closest(".sliderContainer").find(".sliderImage").css("background", bg);
                jQuery(this).closest(".sliderContainer").find(".sliderDescription").text(jQuery(this).find(".tileName").text());
            });

            tile.bind("click.select", displayOptions.onSelectTileAction);
            return tile;
        }
    };

    var tile = {

        print: function () {
            if (data == null) return;
            var colors1 = ["orange", "blue", "gray", "purple", "green"];
            var colors2 = ["gray", "green", "red", "blue", "pink"];
            var tileElement = jQuery("<div/>").addClass("page");
            var lineElement = jQuery("<div/>").addClass("line");
            var currentColor = null;

            for (i = 0, columnCounter = 0; i < data.length; i++, columnCounter++) {
                if (i % 5 == 0) {
                    //lineElement = jQuery("<div/>").addClass("line");
                    tileElement.append(lineElement);
                    currentColor = (currentColor == colors1) ? colors2 : colors1;
                    columnCounter = 0;
                }
                var tile = jQuery("<div/>").addClass("tile ");
                if (typeof data[i][dataOptions.image] != "undefined") {
                    tile.css("background", "url('" + data[i][dataOptions.image] + "')");
                    tile.css("background-size", "100% 100%");
                } else {
                    if (dataOptions.name == "user") {
                        tile.css("background-image", "url(/Content/Images/imgPerfil/avatar_06.png)");
                    } else {
                        tile.addClass(currentColor[columnCounter]);
                    }
                }

                tile.attr("data-id", data[i][dataOptions.id]);
                if (typeof dataOptions.data != "undefined") {
                    var keys = Object.keys(dataOptions.data);
                    for (key in keys) {
                        tile.attr("data-" + keys[key], data[i][dataOptions.data[keys[key]]]);
                    }
                }

                var div = jQuery("<div/>").addClass("btn-group");
                tile.append(div);

                if (typeof dataOptions.permissions != "undefined"  && dataOptions.permissions[0] == "u") {

                    div.append(
                        jQuery("<a/>").addClass("btn btn-mini")
                            .append(jQuery("<i>").addClass("icon-edit"))
                            .attr("data-original-title", "Editar")
                            .attr("rel", "tooltip")
                            .attr("data-placement", "top")
                            .bind("click.edit", displayOptions.onEditTileAction)
                    );
                }
                if (typeof dataOptions.permissions != "undefined" && dataOptions.permissions[1] == "d") {
                    div.append(
                        jQuery("<a/>").addClass("btn btn-mini")
                            .attr("href", "#")
                            .append(jQuery("<i>").addClass("icon-trash"))
                            .attr("data-original-title", "Borrar")
                            .attr("rel", "tooltip")
                            .attr("data-placement", "top")
                            .bind("click.delete", displayOptions.onDeleteTileAction)
                    );
                }
                var btngp = jQuery("<div/>").addClass("btn-group");
                var actionKeys = Object.keys(extraAction);
                for (key in actionKeys) {
                    var newOption = jQuery("<a/>").addClass("btn btn-mini")
                        .append(jQuery("<i>").addClass(extraAction[actionKeys[key]].icon))
                        .bind("click." + extraAction[actionKeys[key]].name, extraAction[actionKeys[key]].action)
                        .attr("data-original-title", extraAction[actionKeys[key]].description)
                        .attr("rel", "tooltip")
                        .attr("data-placement", "top");
                    btngp.append(newOption);
                }

                tile.append(btngp);

                tile.append(jQuery("<div/>").addClass("tileName").text(data[i][dataOptions.name]));

                tile.bind("click.select", displayOptions.onSelectTileAction);
                lineElement.append(tile);
            }
            containerElement.append(tileElement);
        }
    };

    var table = {

        print: function () {
            if (data == null) return;
            var tableElement = jQuery("<table/>").addClass("table table-striped table-bordered table-hover");
            tableElement.prop("id", containerID + "_rvtable");
            tableElement.append(jQuery("<thead/>"))
                 .append(jQuery("<tbody/>"));

            var headerElement = jQuery("<tr/>");

            if (typeof displayOptions.showSelecters != "undefined" && displayOptions.showSelecters == true) {
                var checkerHeader = jQuery("<th/>");

                checkerHeader.append(jQuery("<input/>").attr("type", "checkbox").attr("id", "checkall").addClass("checkall")
                    ).css("text-align", "center");

                checkerHeader.bind("change.select", function () {
                    if (checkerHeader.find("input:first").prop("checked") == true) {
                        checkerHeader.closest("table").find(".checker").prop("checked", true);
                    } else {
                        checkerHeader.closest("table").find(".checker").prop("checked", false);
                    }
                });

                headerElement.append(checkerHeader);
            }
            for (column in displayOptions.headers) {
                var newTD = jQuery("<th/>").text(displayOptions.headers[column]);
                headerElement.append(newTD);
            }
            var newTD = jQuery("<th/>").text("Acciones");
            headerElement.append(newTD);
            tableElement.find("thead").append(headerElement);

            if (data.length < 1) {
                var newTr = jQuery("<tr/>")
                    .append(jQuery("<td/>")
                        .attr("colspan", Object.keys(displayOptions.headers).length + 1)
                        .css("text-align", "center")
                        .text("No se encontraron datos.")
                    );
                tableElement.find("tbody").append(newTr);
            }

            for (i = 0; i < data.length; i++) {
                var newTr = table.createNode(data[i]);
                tableElement.find("tbody").append(newTr);
            }
            containerElement.append(tableElement);
        },

        createNode: function (obj) {
            var newTr = jQuery("<tr/>");
            var newTd = jQuery("<td/>");
            newTr.attr("data-id", obj[dataOptions.id]);

            if (typeof dataOptions.data != "undefined") {
                var keys = Object.keys(dataOptions.data);
                for (key in keys) {
                    newTr.attr("data-" + keys[key], obj[dataOptions.data[keys[key]]]);
                }
            }

            newTr.bind("click.select", displayOptions.onSelectRowAction);
            newTr.css("cursor", "pointer");

            if (typeof displayOptions.showSelecters != "undefined" && displayOptions.showSelecters == true) {
                newTr.append(
                    jQuery("<td/>").append(
                        jQuery("<input/>").attr("type", "checkbox").addClass("checker")
                    ).css("text-align", "center")
                );
            }

            for (column in displayOptions.headers) {
                newTr.append(jQuery("<td/>").text(obj[column]).addClass(column));
            }


            var div = jQuery("<div/>").addClass("btn-group");
            newTr.append(jQuery("<td/>").append(div));

            if (typeof dataOptions.permissions != "undefined" && dataOptions.permissions[0] == "u") {

                div.append(

                      jQuery("<a/>").addClass("btn")

                          .append(jQuery("<i>").addClass("icon-edit"))
                            .attr("data-original-title", "Editar")
                            .attr("rel", "tooltip")
                            .attr("data-placement", "top")
                          .bind("click.edit", displayOptions.onEditRowAction)
                  );
            }
            if (typeof dataOptions.permissions != "undefined" && dataOptions.permissions[1] == "d") {
                div.append(
                        jQuery("<a/>").addClass("btn")
                            .attr("href", "#")
                            .append(jQuery("<i>").addClass("icon-trash"))
                            .attr("data-original-title", "Borrar")
                            .attr("rel", "tooltip")
                            .attr("data-placement", "top")
                            .bind("click.delete", displayOptions.onDeleteRowAction)
                    );
            }


            var actionKeys = Object.keys(extraAction);
            for (key in actionKeys) {
                var newOption = jQuery("<a/>").addClass("btn")
                .append(jQuery("<i>").addClass(extraAction[actionKeys[key]].icon))
                .bind("click." + extraAction[actionKeys[key]].name, extraAction[actionKeys[key]].action)
                .attr("data-original-title", extraAction[actionKeys[key]].description)
                .attr("rel", "tooltip")
                .attr("data-placement", "top");
                div.append(newOption);
            }

            return newTr;
        }
    };

}