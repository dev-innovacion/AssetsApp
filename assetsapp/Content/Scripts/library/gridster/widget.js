//Class Widget 
var Widget = function (widget) {
    var $ = jQuery;
    var _id = null;
    var reference = this;

    //Required Fields
    if (widget == undefined) {
        console.error("No parameters given");
        return null;
    }
    if (widget.dashboard == undefined) {
        console.error("No dashboard given");
        return null;
    }
    if (widget.module == undefined || widget.module == "") {
        console.error("Module is required");
        return null;
    }

    var _dashboard = widget.dashboard;
    var module = widget.module;
    var title = "";
    var data = []; // all widget data
    var options = {}; // all widget options

    var type = "graph";
    var visualization = {
        graphType: "combochart", // graph Option
        icon: "fa-home", // summary Option
    };
    var watching = {
        last: [5, "hours"],
        update: [5, "minutes"]
    }

    var filters = {};
    var attrs = {
        col: 1,
        row: 1,
        sizex: 2,
        sizey: 1
    }
    var widgetHtml = null;

    //Try to get the widget Values
    if (widget._id != undefined) {
        _id = widget._id;
    }
    if (widget.type != undefined) {
        type = widget.type;
    }
    if (widget.title != undefined)
        title = widget.title;
    if (widget.filters != undefined)
        filters = widget.filters;

    try {
        visualization.graphType = widget.visualization.graphType;
    } catch (Exception) { }
    try {
        if (widget.visualization.icon != "")
            visualization.icon = widget.visualization.icon;
    } catch (Exception) { }
    try {
        watching.last = widget.watching.last;
        watching.update = widget.watching.update;
    } catch (Exception) { }

    //End::Try to get the widget Values

    //Public attributes
    this.position = [1, 1];
    this.dimension = [2, 1];
    /*
    try {
        this.position = [];
        this.position.push(widget.attrs.col);
        this.position.push(widget.attrs.row);

        this.dimension = [];
        this.dimension.push(widget.attrs.sizex);
        this.dimension.push(widget.attrs.sizey);
    }
    catch (e) { }*/



    //Returns the widget's name
    this.getTitle = function () {
        return title;
    }

    var setOptions = function (info) {
        options = info;
    }

    var setData = function (info) {
        if (type == "graph")
            data = google.visualization.arrayToDataTable(info);
        else if (type == "summary")
            data = info;
    }

    //Set the widget ID
    var setId = function (id) {
        _id = id;
    }

    //Get the widget ID
    this.getId = function () {
        return _id;
    }

    this.getWidgetHtml = function () {
        var reference = this;

        if (widgetHtml != null)
            return widgetHtml;

        var li = $("<li>").append(
            $("<div>", {
                "class": "content"
            }).append(
                $("<div>", {
                    "class": "dragg blue"
                }).html("<i class='icon-move'></i>")
            ).append(
                $("<div>", {
                    "class": "options",
                    "rel": "tooltip",
                    "title": "Configurar"
                }).on("click", function () {
                    reference.editModal();
                }).html("<i class='icon-gear'></i>")
            ).append(
                $("<div>", {
                    "class": "widget_body"
                }).append(
                    $("<div>", {
                        "class": "title",
                        "rel": "tooltip",
                        "title": reference.getTitle()
                    }).html(reference.getTitle())
                ).append(
                    $("<div>", { "class": "body" })
                )
            )
        );

        widgetHtml = li;

        //li[0].addEventListener("resized", resize);
        //li[0].addEventListener("moved", move);

        return li;
    }


}// End Class Widget //