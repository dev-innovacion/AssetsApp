//Class Widget 
var Widget = function (widget) {
    var $ = jQuery;
    var _id = null;
    var reference = this;
    var interval = null;

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
    this.position = [0, 0];
    this.dimension = [2, 1];

    if (widget.attrs != undefined) {
        this.position[0] = parseInt(widget.attrs.col);
        this.position[1] = parseInt(widget.attrs.row);

        //this.dimension = [];
        this.dimension[0] = parseInt(widget.attrs.sizex);
        this.dimension[1] = parseInt(widget.attrs.sizey);
    }

    //Returns the widget's name
    this.getTitle = function () {
        return title;
    }
    //Returns the widget's module name
    this.getModule = function () {
        return module;
    }

    var setOptions = function (info) {
        options = info;
    }

    var setData = function (info) {
        if (module != "Notifications") {
            if (type == "graph")
                data = google.visualization.arrayToDataTable(info);
            else if (type == "summary")
                data = info;
        }
        else if (module == "Notifications")
            data = info;
    }

    //Set the widget ID
    var setId = function (id) {
        _id = id;
    }

    //Get the widget ID
    this.getId = function (id) {
        return _id;
    }

    var drawVisualization = function () {
        //Get the widget body
        var thisContainer = widgetHtml.find(".widget_body .body")[0];
        //Set the title
        widgetHtml.find(".widget_body .title").html(reference.getTitle());

        if (module != "Notifications") {

            if (type == "graph") {
                var chart = null;
                switch (visualization.graphType) {
                    case "piechart": {
                        chart = new google.visualization.PieChart(thisContainer);
                    }; break;
                    case "combochart": {
                        chart = new google.visualization.ComboChart(thisContainer);
                    }; break;
                    case "linechart": {
                        chart = new google.visualization.LineChart(thisContainer);
                    }; break;
                }

                if (chart != null)
                    chart.draw(data, options);
            }
            else if (type == "summary") {
                $(thisContainer).html("");
                $(thisContainer).append(
                    $("<div>", { "class": "row-fluid summary" }).append(
                        $("<div>", { "class": "span4" }).append(
                            $("<i>", { "class": "fa " + visualization.icon })
                        )
                    ).append(
                        $("<div>", { "class": "span8" }).append(
                            $("<span>", { "class": "pull-left" }).html(data)
                        )
                    )
                )
            }

        }// End of graph and summary widgets

        else if (module == "Notifications") {
            widgetHtml.find(".widget_body .body").html("<ul class='rows notification_body'>");
            reference.loadNotifications(data);
            widgetHtml.find(".notification_body").perfectScrollbar({
                wheelSpeed: 100,
                suppressScrollX: true
            });
        }

    }

    //Loads the initial info (data and graph)
    this.loadInfo = function (refresh) {
        var reference = this;
        var loadingClass = "loading";
        if (refresh != undefined && refresh == true)
            loadingClass = "";

        var loading = widgetHtml.find(".content").append(
                $("<div>", { "class": loadingClass })
            ).find(".loading");

        var sendFilters = {
            'module': module,
            'date': watching.last,
            'type': type,
            'filters': filters,
            'graphtype': visualization.graphType
        };

        //set graph options
        var gOptions = {};
        if (visualization.graphType == "combochart") {
            gOptions = {
                vAxis: { title: "Cantidad" },
                hAxis: { title: "Fecha" },
                seriesType: "bars",
                backgroundColor: { fill: "transparent" }
                //series: { 5: { type: "line" } }
            };
        }
        else {
            gOptions = {
                backgroundColor: { fill: "transparent" }
            };
        }
        setOptions(gOptions);

        //Call to the server function for the result
        $.ajax({
            url: "/Reports/Reports/GeneralReport",
            method: "POST",
            data: { filter: JSON.stringify(sendFilters) },
            success: function (data) {
                var result = JSON.parse(data);
                setData(result);
                drawVisualization();
                //Glow Effect
                glowEffect();
            },
            error: function () {
            },
            complete: function () {
                loading.remove();
            }
        });

    }

    //Open a window modal to edit the widget
    this.editModal = function () {
        var widgetModal = $("#widget_modal_config");
        var reference = this;

        if (widgetModal.length > 0) {
            fillModal();
            //Call inner function to fill the filters
            (new fillModal()).fillFilters();

            widgetModal.find("[name=save]").unbind("click.save");
            widgetModal.find("[name=save]").bind("click.save", function () {
                saveWidget();
            });
            widgetModal.modal("show");
        }
        else {
            //Creates the modal HTML
            var wModal = $("<div>", {
                "class": "modal hide fade in",
                "id": "widget_modal_config"
            }).append( //Modal Header
                $("<div>", {
                    "class": "modal-header"
                }).append(
                    $("<h3>").html("Configurar Widget")
                ).append(
                    $("<button>", {
                        "class": "close",
                        "data-dismiss": "modal",
                        "aria-hidden": "true"
                    }).html(
                        '<i class="icon-remove"></i>'
                    )
                )
            ).append( //Modal Body
                $("<div>", {
                    "class": "modal-body"
                })
            ).append( //Modal Footer
                $("<div>", {
                    "class": "modal-footer"
                }).append(
                    $("<button>", {
                        "class": "btn blue",
                        "name": "save"
                    }).html("Guardar")
                ).append(
                    $("<button>", {
                        "class": "btn red",
                        "name": "cancel",
                        "data-dismiss": "modal"
                    }).html("Cancelar")
                )
            );
            $("body").append(wModal);

            wModal.find("[name=save]").unbind("click.save");
            wModal.find("[name=save]").bind("click.save", function () {
                saveWidget();
            });

            //Load the content via Ajax for the first time
            $.ajax({
                url: "/Content/Scripts/library/gridster/widget.html?cache=" + Date.now(),
                method: "GET",
                beforeSend: function () { },
                success: function (data) {
                    wModal.find(".modal-body").html(data);
                    fillModal();
                    fillSources();
                },
                error: function () { },
                complete: function () { }
            });

            wModal.modal("show");
        }
    }

    // Saves the widget 
    var saveWidget = function () {
        var widgetModal = $("#widget_modal_config");

        try { //trying to get the values
            reference.dimension[0] = parseInt(widgetHtml.attr("data-sizex"));
            reference.dimension[1] = parseInt(widgetHtml.attr("data-sizey"));
            reference.position[0] = parseInt(widgetHtml.attr("data-col"));
            reference.position[1] = parseInt(widgetHtml.attr("data-row"));
        }
        catch (Exception) { }

        //Create the object to save
        var widgetObject = {
            _id: _id,
            dashboard: _dashboard,
            title: widgetModal.find("[name=title]").val(),
            module: module,
            type: widgetModal.find("[name=visualization]").val(),
            visualization: {
                graphType: "",
                icon: ""
            },
            watching: {
                last: [
                    widgetModal.find("[name=lastTime]").val(),
                    widgetModal.find("[name=lastType]").val()
                ],
                update: [
                    widgetModal.find("[name=updateTime]").val(),
                    widgetModal.find("[name=updateType]").val()
                ]
            },
            filters: {
                modules: [],
                group: ""
            },
            attrs: {
                col: reference.position[0],
                row: reference.position[1],
                sizex: reference.dimension[0],
                sizey: reference.dimension[1],
            }
        };
        // Sets the graph type
        if (widgetObject.type == "graph") {
            widgetObject.visualization.graphType = widgetModal.find("[name=graphType]").val();
        }
            // Sets the summary icon
        else {
            widgetObject.visualization.icon = widgetModal.find("input[name=iconPicker]").val();
        }

        // Sets filters
        var selectedFilters = $(".opt-filter [type=checkbox]:checked");
        selectedFilters.each(function () {
            var row = {
                mod: this.value,
                v: $(this).closest("tr").find("select").select2("val")
            };
            widgetObject.filters.modules.push(row);
        });
        widgetObject.filters.group = widgetModal.find("[name=group]:checked").val();

        //Update the Class Widget Attributes
        title = widgetObject.title;
        type = widgetObject.type;
        visualization = widgetObject.visualization;
        watching = widgetObject.watching;
        filters = widgetObject.filters;

        //Sent data to the server
        $.ajax({
            url: "/Home/SaveWidget",
            method: "POST",
            data: { widget: JSON.stringify(widgetObject) },
            beforeSend: function () {
                _loading();
            },
            success: function (data) {
                if (reference.getId() == null) {
                    setId(data);
                    Dashboard.addWidget(reference);
                    reference.updateWidget();
                }
                else
                    reference.loadInfo();

                //Refresh action just for non Notification Widgets
                if (module != "Notifications")
                    refreshWidget();

                widgetModal.modal("hide");
            },
            error: function () {
            },
            complete: function () {
                _loading();
            }
        });


    }

    //Fill the modal with all widget data
    var fillModal = function () {
        var widgetModal = $("#widget_modal_config");

        //Module title & Filters
        var icon = $("#moduleTitle").find("i");
        $(".opt-filter tbody tr").hide();
        $(".no-notifications").show();
        //Set the default title for "Users"
        $(".opt-users .title").text("Usuarios");
        icon.prop("class", "")
        switch (module) {
            case "ObjectReal":
                icon.addClass("icon-tag");
                $("#moduleTitle").find("span").html("Activos");

                //Show related filters
                $(".opt-locations, .opt-reference, .opt-users").show();

                //Set the correct values for group Option
                $(".opt-users").find("[name=group]").prop("value", "userid");

                if (filters.group == undefined)
                    $("[name=group][value=location]").prop("checked", true);

                break;
            case "Locations":
                icon.addClass("icon-home");
                $("#moduleTitle").find("span").html("Ubicaciones");

                //Show related filters
                $(".opt-location-profile, .opt-users").show();

                //Set the correct values for group Option
                $(".opt-users").find("[name=group]").prop("value", "responsable");
                $(".opt-location-profile").find("[name=group]").prop("value", "profileId");
                $(".opt-user-profile").find("[name=group]").prop("value", "");

                if (filters.group == undefined)
                    $("[name=group][value=profileId]").prop("checked", true);
                //Change "Usuario" title for "Responsable"
                $(".opt-users .title").text("Responsable");

                break;
            case "Users":
                icon.addClass("icon-user");
                $("#moduleTitle").find("span").html("Usuarios");

                //Set the correct values for group Option
                $(".opt-location-profile").find("[name=group]").prop("value", "");
                $(".opt-user-profile").find("[name=group]").prop("value", "profileId");

                //Show related filters
                $(".opt-user-profile").show();
                if (filters.group == undefined)
                    $("[name=group][value=profileId]").prop("checked", true);

                break;
            case "Demand":
                icon.addClass("icon-random");
                $("#moduleTitle").find("span").html("Movimientos");

                //Set the correct values for group Option
                $(".opt-users").find("[name=group]").prop("value", "Creator");

                //Show related filters
                $(".opt-locations, .opt-movement, .opt-movement-status, .opt-users").show();
                if (filters.group == undefined)
                    $("[name=group][value=movement]").prop("checked", true);

                break;
            case "Reports":
                icon.addClass("icon-bar-chart");
                $("#moduleTitle").find("span").html("Reportes");
                break;
            case "Notifications":
                icon.addClass("icon-warning-sign");
                $("#moduleTitle").find("span").html("Notificaciones");
                $(".no-notifications").hide();

                //Show related filters
                $("[class*=opt-notifications]").show();

                break;
        }

        //Filters selected
        this.fillFilters = function () {
            $(".opt-filter [type=checkbox]").prop("checked", false);
            //Clear options
            $(".opt-filter select").select2("val", []);
            if (filters.group != undefined)
                $("[name=group][value=" + filters.group + "]").prop("checked", true);
            if (filters.modules != undefined) {
                for (var m in filters.modules) {
                    var mod = filters.modules[m].mod;
                    var values = filters.modules[m].v;
                    var selectOptions = $(".opt-filter [type=checkbox][value=" + mod + "]").prop("checked", true);
                    selectOptions.closest("tr").find("select").select2("val", values);
                }
            }
        }

        //Title
        widgetModal.find("[name=title]").val(title);

        //Visualization
        widgetModal.find("[name=visualization]").val(type);
        widgetModal.find("[name=visualization]").trigger("change");

        //Graph Type
        if (type == "graph") {
            widgetModal.find("[name=graphType]").val(visualization.graphType);
            widgetModal.find("[name=graphType]").trigger("change");
        }
        else {
            $("#icon").iconpicker("setIcon", visualization.icon);
        }

        //Watch last
        widgetModal.find("[name=lastTime]").val(watching.last[0]);
        widgetModal.find("[name=lastType]").val(watching.last[1]);

        widgetModal.find("[name=updateTime]").val(watching.update[0]);
        widgetModal.find("[name=updateType]").val(watching.update[1]);

    }

    //Fill the sources required for the <select> elements
    var fillSources = function () {
        var reference = this;
        var widgetModal = $("#widget_modal_config");
        var loading = widgetModal.find(".opt-filter tbody").append(
                $("<div>", { "class": "loading" })
            ).find(".loading");

        $.ajax({
            url: "/Home/GetSources",
            method: "POST",
            success: function (data) {
                var sources = JSON.parse(data);

                //Fill all filter options
                /* User Table */
                for (var u in sources.Users) {
                    $(".opt-users").find("select").append(
                            $("<option>", {
                                "value": sources.Users[u].id
                            }).html(sources.Users[u].value)
                        );
                }
                /* Locations Table */
                for (var u in sources.Locations) {
                    $(".opt-locations").find("select").append(
                            $("<option>", {
                                "value": sources.Locations[u].id
                            }).html(sources.Locations[u].value)
                        );
                }
                /* Location Profiles Table */
                for (var u in sources.LocationProfiles) {
                    $(".opt-location-profile").find("select").append(
                            $("<option>", {
                                "value": sources.LocationProfiles[u].id
                            }).html(sources.LocationProfiles[u].value)
                        );
                }
                /* Reference Objects Table */
                for (var u in sources.ReferenceObjects) {
                    $(".opt-reference").find("select").append(
                            $("<option>", {
                                "value": sources.ReferenceObjects[u].id
                            }).html(sources.ReferenceObjects[u].value)
                        );
                }
                /* User Profiles Table */
                for (var u in sources.Profiles) {
                    $(".opt-user-profile").find("select").append(
                            $("<option>", {
                                "value": sources.Profiles[u].id
                            }).html(sources.Profiles[u].value)
                        );
                }
                /* User Profiles Table */
                for (var u in sources.MovementProfiles) {
                    $(".opt-movement").find("select").append(
                            $("<option>", {
                                "value": sources.MovementProfiles[u].id
                            }).html(sources.MovementProfiles[u].value)
                        );
                }

                $(".opt-filter select").attr("style", "width:100%")
                $(".opt-filter select").select2();

                (new fillModal()).fillFilters();

            },
            error: function () {
            },
            complete: function () {
                loading.remove();
            }
        });

    }

    //Returns the raw html widget
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
                    "class": "options delete",
                    "rel": "tooltip",
                    "title": "Borrar"
                }).on("click", function () {
                    _confirm({
                        title: "Borrar Widget",
                        message: "Desea borrar este widget?",
                        action: function () {
                            deleteWidget();
                        }
                    });
                }).html("<i class='icon-trash'></i>")
            ).append(
                $("<div>", {
                    "class": "widget_body"
                }).append(
                    $("<div>", {
                        "class": "title",
                        "rel": "tooltip",
                        "title": this.getTitle()
                    }).html(this.getTitle())
                ).append(
                    $("<div>", { "class": "body" })
                )
            )
        );

        widgetHtml = li;

        //Add resized event
        li[0].addEventListener("resized", resize);
        //Add timer defined
        if (module != "Notifications")
            refreshWidget();

        return li;
    }

    //Events
    /* Resize Event */
    var resize = function () {
        if (module != "Notifications")
            drawVisualization();
    }

    //Updates size and position
    this.updateWidget = function () {
        var newAttr = {
            sizex: parseInt(widgetHtml.attr("data-sizex")),
            sizey: parseInt(widgetHtml.attr("data-sizey")),
            row: parseInt(widgetHtml.attr("data-row")),
            col: parseInt(widgetHtml.attr("data-col"))
        }
        if (reference.dimension[0] != newAttr.sizex ||
            reference.dimension[1] != newAttr.sizey ||
            reference.position[0] != newAttr.col ||
            reference.position[1] != newAttr.row
            ) {

            reference.dimension[0] = newAttr.sizex;
            reference.dimension[1] = newAttr.sizey;
            reference.position[0] = newAttr.col;
            reference.position[1] = newAttr.row;

            $.ajax({
                url: "/Home/SaveWidgetAttributes",
                method: "POST",
                data: {
                    id: _id,
                    attributes: JSON.stringify(newAttr)
                }
            });
        }
    }

    // Delete this widget from dashboard and Database
    var deleteWidget = function () {
        Dashboard.removeWidget(reference);
        //Stop refreshing
        clearInterval(interval);
        //Send delete action to the server
        $.ajax({
            url: "/Home/DeleteWidget",
            method: "POST",
            data: {
                id: _id
            }
        });
    }

    var glowEffect = function () {
        widgetHtml.find(".content").addClass("glow")
        setTimeout(function () {
            widgetHtml.find(".content").removeClass("glow");
        }, 500);
    }

    //Refresh widget every time interval
    var refreshWidget = function () {
        clearInterval(interval);

        var timer = 10000;

        switch (watching.update[1]) {
            case "seconds":
                timer = watching.update[0] * 1000;
                break;
            case "minutes":
                timer = watching.update[0] * 60 * 1000;
                break;
            case "hours":
                timer = watching.update[0] * 60 * 60 * 1000;
                break;
        }
        // Set the update interval 
        interval = setInterval(function () {
            reference.loadInfo(true);
        }, timer);
    }

    //Create Notifications functions
    if (module == "Notifications") {
        var rowClass = "";

        reference.pushNotification = function (message) {
            insertNotificationRow(message);
        }

        reference.loadNotifications = function (notifications) {
            for (var n in notifications.reverse())
                insertNotificationRow(notifications[n])
        }

        var insertNotificationRow = function (message) {

            var module = message["module"];
            var msg = message["msg"];
            var date = message["date"];
            var type = message["type"];
            var icon = message["icon"];
            var color;
            var title;

            var showNotification = false;
            var keepSearching = true;

            //Check if its a valid notification
            var activeModules = filters.modules;
            for (var n in activeModules) {
                if (activeModules[n].mod == module + "_Notification") {
                    for (var v in activeModules[n].v) {
                        if (activeModules[n].v[v] == type) {
                            showNotification = true;
                            keepSearching = false;
                            break;
                        }
                    }
                }
                if (!keepSearching)
                    break;
            }


            if (showNotification) {

                // Selecting icon
                switch (module) {
                    case "Users":
                        icon = "icon-user";
                        title = "Usuario ";
                        break;
                    case "Locations":
                        icon = "icon-home";
                        title = "Ubicación ";
                        break;
                    case "Objects":
                        icon = "icon-tag";
                        title = "Objeto ";
                        break;
                    case "Movements":
                        icon = "icon-random";
                        title = "Movimiento ";
                        break;
                    case "Rules":
                        icon = "icon-gears";
                        title = "Regla ";
                        break;
                    default: icon = "icon-info";
                }
                //Selecting color
                switch (type) {
                    case "Create":
                        color = "success";
                        if (module == "Locations")
                            title += "creada";
                        else
                            title += "creado";
                        break;
                    case "Update":
                        color = "warning";
                        if (module == "Locations")
                            title += "modificada";
                        else
                            title += "modificado";
                        break;
                    case "Delete":
                        color = "red";
                        if (module == "Locations")
                            title += "borrada";
                        else
                            title += "borrado";
                        break;
                    case "Deny":
                        color = "red";
                        title += "denegado";
                        break;
                    case "Authorize":
                        color = "info";
                        title += "autorizado";
                        break;
                    case "Invalid":
                        color = "red";
                        title += "inválida";
                        break;

                    default: color = "info";
                }


                widgetHtml.find(".widget_body .body .notification_body").prepend(
                        $("<li>", {
                            "class": "notification_row " + rowClass
                        }).append(
                            $("<span>", {
                                "class": "icon " + color
                            }).append(
                                $("<i>", {
                                    "class": icon
                                })
                            )
                        ).append(
                            $("<div class='notification_content'>").append(
                                $("<div>", {
                                    "class": "title"
                                }).html(title)
                            ).append(
                                $("<span>").html(msg)
                            )
                        ).hide().fadeIn("slow")
                    )

                // Alternate row class
                if (rowClass == "")
                    rowClass = "odd";
                else
                    rowClass = "";

                glowEffect();
            }
        }

    }



}// End Class Widget //