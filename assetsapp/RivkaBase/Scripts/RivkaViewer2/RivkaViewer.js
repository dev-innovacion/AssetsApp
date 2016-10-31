/*! RivkaViewer v2.0
*   Class to manipulate a data set
*/


/* config example:
    var config = {
            id: "_id",
            name: "name",
            image: "image",
            permissions: [upd],
            data: { "referenceid": "objectReference" },
            columnNames: { "name": "Nombre", "epc": "EPC", "Creator": "Creador", "CreatedDate": "Fecha de Creación", "LastmodDate": "Última Modificación" },
            showSelecters: true,
            defaultGrouping: name | location | null,
            displaymenus:{ "operation": 1, "group": 1, "view": 1 },
            displayConfiguration: {
                tableOptions: {
                    onEditRowAction: function () {
                        var tr = jQuery(this).closest("tr");
                        objectModal.onEditAction(tr);
                    },
                    onDeleteRowAction: table.onDeleteObjectAction,
                    onSelectRowAction: null,
                },

                tilesOptions: {
                    onEditTileAction: function () {
                        var tr = jQuery(this).closest("div.tile");
                        objectModal.onEditAction(tr);
                    },
                    onDeleteTileAction: table.onDeleteObjectAction,
                    onSelectTileAction: null
                },

                sliderOptions: {
                    onEditTileAction: function () {
                        var tr = jQuery(this).closest("div.tile");
                        objectModal.onEditAction(tr);
                    },
                    onDeleteTileAction: table.onDeleteObjectAction,
                    onSelectTileAction: null
                }
            }
        };

********/

Number.prototype.format = function (n, x) {
    var re = '\\d(?=(\\d{' + (x || 3) + '})+' + (n > 0 ? '\\.' : '$') + ')';
    return this.toFixed(Math.max(0, ~~n)).replace(new RegExp(re, 'g'), '$&,');
};

RivkaViewer.METHODS = { TABLE: 0, TILE: 1, SLIDER: 2, NONE: null };

function RivkaViewer(containerID, configReceived) {
    var reference = this;
    var $ = jQuery;
    var config; //object's options
    var lastGrouping = null;
    var fieldGrouping = null;
    var displaymenus = { "operation": 1, "group": 1, "view": 1 };
    if (typeof containerID == "undefined") {
        console.error("RivkaViewer: A container id value must be gived.");
        return null;
    }
    if (!jQuery("#" + containerID).length) {
        console.error("RivkaViewer: The specified element's id:" + containerID + " does not exist in this context.");
        return null;
    }
    if (typeof configReceived == "undefined" || configReceived == null || !(typeof configReceived == "object")) {
        config = {};
    } else {
        if (typeof configReceived.defaultGrouping != "undefined") {
            fieldGrouping = configReceived.defaultGrouping;
        }
        if (typeof configReceived.displaymenus != "undefined") {
            displaymenus = configReceived.displaymenus;
        }
        config = configReceived;
    }
    var containerID = containerID; //Container DOM id
    var containerElement = jQuery("#" + containerID); //container's jquery object
    var displayMethod = RivkaViewer.METHODS.NONE;
    var data = null; //Data to be printViewed
    var originalData = null;
    var extraAction = new Object();
    var columnNames = null;
    var columnShow = null;
    var selectedOptions = [];
    

    if (typeof configReceived != "undefined" && configReceived != null && typeof configReceived.columnNames != "undefined" && configReceived.columnNames != null) {
        columnNames = configReceived.columnNames;
        columnShow = configReceived.columnShow;
    }

    this.doOperation = function (field, operation) {
        return operations.doOperation(field, operation);
    },

    this.getOperations = function () {
        return operations.getOperations();
    },

    this.getConfig = function () {
        return config;
    };

    this.setconfig = function (configReceived) {
        config = configReceived;
    };

    this.getdisplayMenu = function () { return config.displaymenus; };

    this.setconfigColumns = function (columns) {
        config.columnNames = columns;
    };

    this.getColumnShow = function () { return config.columnShow; };
    this.setColumnShow = function (newconfig) { config.columnShow = newconfig; };

    this.addConfig = function (configReceived) {
        var configKeys = null;
        if (typeof (configReceived) == "object") {
            configKeys = Object.keys(configReceived);
        }
        for (var key in configKeys) {
            config[configKeys[key]] = configReceived[configKeys[key]];
        }
    };

    this.getActions = function () {
        return extraAction;
    };

    this.setActions = function (actions) {
        extraAction = actions;
    };

    this.addAction = function (action) {
        extraAction[action.name] = action;
    };

    var getColumnValue = function (obj, column) {
        if (column.indexOf('.') === -1) {
            return obj[column];
        }
        var subs = column.split('.');
        var aux = obj;
        for (var i = 0; i < subs.length && typeof aux != "undefined"; i++) {
            aux = aux[subs[i]];
        }
        return aux;
    };

    var getColumnByName = function (field) {
        var keys = Object.keys(config.columnNames);
        for (var i in keys) {
            if (config.columnNames[keys[i]] == field) {
                return keys[i];
            }
        }
        return null;
    };

    this.printView = function () {
        if (displayMethod == null) {
            displayMethod = RivkaViewer.METHODS.TILE;
        }
        if (fieldGrouping != null && fieldGrouping != "null") {
            if (fieldGrouping != lastGrouping) {
                group(this);
                lastGrouping = fieldGrouping;
            }
        } else {
            fieldGrouping = null;
            lastGrouping = null;
            data = originalData;
            columnNames = config.columnNames;
            columnShow = config.columnShow;
        }

        reference.removeSelectedItems();

        containerElement.empty();
        switch (displayMethod) {
            case RivkaViewer.METHODS.TABLE:
                table.printView();
                columnsTable.bind(this);
                try { optionsMenu.changecolor(); } catch (e) { }
                break;
            case RivkaViewer.METHODS.TILE:
                tile.printView();
                try { optionsMenu.changetiles(); } catch (e) { }
                break;
            case RivkaViewer.METHODS.SLIDER:
                slider.printView();
                break;
        }
        optionsMenu.bind(this);

        containerElement.addClass("tab-rivkaviewer");
    };

    var group = function () {
        var valuesSeen = [];
        var currentValue = null;
        var currentElement = null;
        var dataAux = {};
        for (var i in data) {
            fieldGrouping=(fieldGrouping == "location") ? "namelocation" : fieldGrouping;
            currentValue = getColumnValue(data[i], fieldGrouping);

            if (valuesSeen.indexOf(currentValue) < 0) {
                valuesSeen.push(currentValue);
                currentElement = {};
                currentElement[config.name] = currentValue;
                currentElement[config.image] = data[i][config.image];
                currentElement["_RVCOUNT"] = 1;
                currentElement["elements"] = [];
                dataAux[currentValue] = currentElement;
            } else {
                dataAux[currentValue]["_RVCOUNT"] += 1;
            }
            dataAux[currentValue]["elements"].push(data[i]);
        }

        columnNames = { name: "Descripcion", _RVCOUNT: "Registros" }
        data = [];
        for (var i in dataAux) {
            data.push(dataAux[i]);
        }
    };

    this.setData = function (dataString, configReceived) {
        if (typeof dataString == "undefined") throw "RivkaViewer: data must be defined.";
        try {
            data = JSON.parse(dataString);
            var newData = {};
            //Save data with indexes
            for (var d in data) {
                try{
                    newData[data[d][config.id]] = data[d];
                } catch (ex) {
                    console.log("parse data:" + ex.toString());
                }
            }
            data = newData;

        } catch (Exception) { throw "RivkaViewer: an error ocurred while parsing data."; }
        originalData = data;
        lastGrouping = null;
        this.addConfig(configReceived);
    };

    this.getData = function () {
        return originalData;
    };

    this.setContainer = function (containerID) {
        if (typeof containerID == "undefined") {
            console.error("RivkaViewer: A container id value must be gived");
        }
        if (!jQuery("#" + containerID).length) {
            console.error("RivkaViewer: The specified element's id:" + containerID + " does not exist in this context.");
        }
        container = containerID;
        containerElement = jQuery("#" + containerID);
        displayMethod = RivkaViewer.METHODS.NONE;
    };

    this.getContainer = function () {
        return container;
    };

    //UNUSED
    this.setDisplayMethod = function (method, configReceived) {
        if (typeof method == "undefined") {
            console.error("RivkaViewer: A RivkaViewer method must be gived as display method, table view selected by default.");
            method = RivkaViewer.METHODS.TABLE;
        }
        switch (method) {
            case RivkaViewer.METHODS.TABLE:
                displayMethod = RivkaViewer.METHODS.TABLE;
                break;
            case RivkaViewer.METHODS.TILE:
                displayMethod = RivkaViewer.METHODS.TILE;
                break;
            case RivkaViewer.METHODS.SLIDER:
                displayMethod = RivkaViewer.METHODS.SLIDER;
                break;
            default:
                console.error("RivkaViewer: Unknow display method for RivkaViewer.");
        }
        this.addConfig(configReceived);
    };

    this.getDisplayMethod = function () {
        return displayMethod;
    };

    var optionsMenu = {

        getMenu: function () {
            var menu = jQuery("<div class='btn-group'/>");
            var menuTitle = jQuery('<a class="btn dropdown-toggle" data-toggle="dropdown" href="#"><i class=" icon-cog icon-white"><span class="caret"></span></a>');
            var menuBody = jQuery('<ul class="dropdown-menu" role="menu" aria-labelledby="dropdownMenu"/>');
            if(displaymenus.operation==1){
                menuBody.append(jQuery('<li><a tabindex="-1" href="#" id="_RV' + containerID + 'OperationButton"><i class=" icon-plus-sign icon-white"> Operaciones...</a></li>'));
            }
            
            var groupMenu = jQuery('<li class="dropdown-submenu">')
                .append(jQuery('<a tabindex="-1" href="#"><i class=" icon-zoom-in icon-white"></i> Agrupar por:</a>'));
            var groupSubmenu = jQuery('<ul class="dropdown-menu _RVGroupItems" role="menu" aria-labelledby="dropdownMenu">')
                .append(jQuery('<li><a tabindex="-1" href="#" data-groupdata="name"><i class=" icon-th-large icon-white"></i> Objeto de Referencia</a></li>'))
                .append(jQuery('<li><a tabindex="-1" href="#" data-groupdata="location"><i class=" icon-th-large icon-white"></i> Ubicación</a></li>'))
                .append(jQuery('<li><a tabindex="-1" href="#" data-groupdata="object_id"><i class=" icon-th-large icon-white"></i> ID Artículo</a></li>'))
                .append(jQuery('<li><a tabindex="-1" href="#" data-groupdata="null"><i class=" icon-th-large icon-white"></i> Ninguno</a></li>'))

            groupMenu.append(groupSubmenu);

            if (displaymenus.group == 1) {
                menuBody.append(groupMenu);
            }
            

            var viewMenu = jQuery('<li class="dropdown-submenu">')
                .append(jQuery('<a tabindex="-1" href="#"><i class=" icon-zoom-in icon-white"></i> Vistas:</a>'));
            var viewSubmenu = jQuery('<ul class="dropdown-menu" role="menu" aria-labelledby="dropdownMenu">')
                .append(jQuery('<li><a tabindex="-1" href="#" id="_RV' + containerID + 'TileButton"><i class=" icon-th-large icon-white"></i> Mosaicos</a></li>'))
                .append(jQuery('<li><a tabindex="-1" href="#" id="_RV' + containerID + 'SliderButton"><i class=" icon-resize-horizontal icon-white"></i> Carrusel</a></li>'))
                .append(jQuery('<li><a tabindex="-1" href="#" id="_RV' + containerID + 'TableButton"><i class=" icon-th-list icon-white"></i> Tabla</a></li>'));

            viewMenu.append(viewSubmenu);

            if (displaymenus.view == 1) {
                menuBody.append(viewMenu);
            }
            

            menu.append(menuTitle);
            menu.append(menuBody);
            return menu;
        },

        changecolor:function() {
            var rows = jQuery("#"+containerID + "_rvtable").find("tr[data-systemstatus=false]");
            rows.each(function () {
                jQuery(this).css("color", "red");
            });
        },

        changetiles: function () {
            var tiles = jQuery(".tile[data-systemstatus='false']");
            tiles.each(function () {
                jQuery(this).find(".abaja").remove();
                jQuery(jQuery(this).find(".btn-group")[0]).append("<a class='btn btn-mini abaja' data-original-title='Dado de baja' rel='tooltip' data-placement='top'><i class='icon-remove'></i></a>")
            });
        },

        bind: function (obj) {
            jQuery("#_RV" + containerID + "SliderButton").unbind("click.toSliderView");
            jQuery("#_RV" + containerID + "SliderButton").bind("click.toSliderView", function () {
                displayMethod = RivkaViewer.METHODS.SLIDER;
                obj.printView();
            });

            jQuery("#_RV" + containerID + "TableButton").unbind("click.toTableView");
            jQuery("#_RV" + containerID + "TableButton").bind("click.toTableView", function () {
                displayMethod = RivkaViewer.METHODS.TABLE;
                obj.printView();
            });

            jQuery("#_RV" + containerID + "TileButton").unbind("click.toTileView");
            jQuery("#_RV" + containerID + "TileButton").bind("click.toTileView", function () {
                displayMethod = RivkaViewer.METHODS.TILE;
                obj.printView();
            });

            jQuery("#_RV" + containerID + "OperationButton").unbind("click.doOperation");
            jQuery("#_RV" + containerID + "OperationButton").bind("click.doOperation", function () {
                jQuery("#_RVOperationsModal").modal("show");
            });

            jQuery("._RVGroupItems li a").unbind("click.groupData");
            jQuery("._RVGroupItems li a").bind("click.groupData", function () {
                fieldGrouping = jQuery(this).data("groupdata");
                //return the original data
                data = originalData;

                obj.printView();
                //if (fieldGrouping == "null" || fieldGrouping == null) {
                //    if (displayMethod == RivkaViewer.METHODS.TILE) {
                //        try { optionsMenu.changetiles(); } catch (e) { }
                //    }
                //    if (displayMethod == RivkaViewer.METHODS.TABLE) {
                //        try { optionsMenu.changecolor(); } catch (e) { }
                //    }
                //}
            });

            //jQuery("#globalSearchButton").bind("click.search");
            //jQuery("#globalSearchButton").bind("click.search", table.onSearchAction);

            //jQuery("#globalSearch").bind("keypress.checkSend", function (e) {
            //    if (e.which == 13) {
            //        jQuery("#globalSearchButton").click();
            //    }
            //});
        }
    };
    
    // Sets or unsets an element from the selected array
    var setSelectedOption = function (option) {
        if (selectedOptions.indexOf(option) < 0)
            selectedOptions.push(option);
        else
            selectedOptions.splice(selectedOptions.indexOf(option), 1);
    };

    this.setSelectedOption = function (option) {
        if (selectedOptions.indexOf(option) < 0)
            selectedOptions.push(option);
        //else
        //    selectedOptions.splice(selectedOptions.indexOf(option), 1);
    };
    // Retuns all selected items
    this.getSelectedItems = function () {
        var selectedData = [];
        for (var s in selectedOptions) {
            selectedData.push(data[selectedOptions[s]]);
        }
        return selectedData;
    };

    // Clean all selected options
    this.removeSelectedItems = function () {
        for (var e in selectedOptions) {
            if (displayMethod == RivkaViewer.METHODS.TILE) {
                var element = containerElement.find(".tile[data-id=" + selectedOptions[e] + "]")
                tile.selectTile(element);
            }
            else if (displayMethod == RivkaViewer.METHODS.TABLE) {
                containerElement.find("tr[data-id=" + selectedOptions[e] + "] input[type=checkbox]").prop("checked", false);
            }
        }
        selectedOptions = [];
    }

    var columnsTable = {
        getButton: function () {
            var boton = jQuery("<div class='btn-group left columns-filter'/>");
            var menuTitle = jQuery('<a class="btn dropdown-toggle" data-toggle="dropdown" href="#"><i class=" icon-filter icon-white"><span class="caret"></span></a>');
            var menuBody = jQuery('<ul class="dropdown-menu" aria-labelledby="dropdownMenu"/>');
            //.append(jQuery('<div id="tablecolumns"></div>'));
            var divtabla = jQuery("<div/>");
            var tabla = jQuery('<table id="ColumnsTable" class="table table-striped table-bordered">');
            debugger;
            for (var column in columnNames) {
                
                var newTR = jQuery("<tr/>").append(
                    jQuery("<td/>").append(
                        jQuery("<input/>").attr("type", "checkbox").attr("id",column).addClass("checkerColumn")
                    ).css("text-align", "center"));
                
                if (columnNames[column] == "Nombre") {
                    columnNames[column] = "Descripción";
                    columnShow[column] = 1;
                }
                if (columnNames[column] == "prueba") {
                    
                }else{
                    newTR.append(jQuery("<td/>").text(columnNames[column]));
                tabla.append(newTR);
                if (columnShow[column] == 1 ) {
                    newTR.find(".checkerColumn").prop("checked", true);
                }
               }
            }

            var footer = jQuery('<div><button id="btnaceptar" type="button" class="btn-mini pull-right">Aceptar</button></div>');
            menuBody.append(divtabla);
            menuBody.append(footer);
            divtabla.append(tabla);
            boton.append(menuTitle);
            boton.append(menuBody);
            return boton;
        },
        bind: function (obj) {
            jQuery("#btnaceptar").unbind("click.aceptar");
            jQuery("#btnaceptar").bind("click.aceptar", function () {
                var checklist = jQuery("#ColumnsTable").find("input[type='checkbox']");
                var columnShow1 = obj.getColumnShow();
                for (j = 0; j < checklist.length; j++) {
                    if (jQuery(checklist[j]).prop("checked") == false) {
                        columnShow1[jQuery(checklist[j]).attr("id")] = 0;
                    }
                    if (jQuery(checklist[j]).prop("checked") == true) {
                        columnShow1[jQuery(checklist[j]).attr("id")] = 1;
                    }
                }

                obj.setColumnShow(columnShow1);
                displayMethod = RivkaViewer.METHODS.TABLE;
                obj.printView();
            });

            jQuery('.dropdown-menu').find(".checkerColumn").on("click", function (e) {
                e.stopPropagation();
            });
        }
    };

    var slider = {
        elementToPrint: 0,

        colors: ["orange", "blue", "gray", "purple", "green"],

        lineSize: 5,

        printView: function () {
            slider.elementToPrint = 0;
            if (data == null) return;
            var sliderElement = jQuery("<div/>").addClass("sliderContainer");
            sliderElement.append(
                jQuery("<div/>").addClass("sliderImage")
                .append(jQuery("<div/>").addClass("sliderDescription"))
            );
            var sliderContent = jQuery("<div/>").addClass("line");
            sliderContent.append(jQuery("<div/>").addClass("prev").bind("click.prev", function () { slider.prev(tilesContainer); }));
            var tilesContainer = jQuery("<div/>").addClass("tilesContainer");
            sliderContent.append(tilesContainer);
            sliderContent.append(jQuery("<div/>").addClass("next").bind("click.next", function () { slider.next(tilesContainer); }));

            for (; slider.elementToPrint < slider.lineSize && slider.elementToPrint < data.length; slider.elementToPrint++) {
                var tile = slider.createNode({
                    id: data[slider.elementToPrint][config.id],
                    name: data[slider.elementToPrint][config.name],
                    image: data[slider.elementToPrint][config.image],
                    color: slider.colors[slider.elementToPrint],
                    data: data[slider.elementToPrint]
                });
                tilesContainer.append(tile);
            }

            sliderElement.append(sliderContent);
            containerElement.append(optionsMenu.getMenu());
            containerElement.append(operations.createPreview());
            containerElement.append(sliderElement);
            sliderContent.find(".tile:first").mouseover();
        },

        next: function (tilesContainer) {

            if (slider.elementToPrint < data.length)
                tilesContainer.empty();
            for (var i = 0; i < slider.lineSize && slider.elementToPrint < data.length; slider.elementToPrint++, i++) {
                var tile = slider.createNode({
                    id: data[slider.elementToPrint][config.id],
                    name: data[slider.elementToPrint][config.name],
                    image: data[slider.elementToPrint][config.image],
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
            for (var i = 0; i < slider.lineSize && slider.elementToPrint < data.length; slider.elementToPrint++, i++) {
                var tile = slider.createNode({
                    id: data[slider.elementToPrint][config.id],
                    name: data[slider.elementToPrint][config.name],
                    image: data[slider.elementToPrint][config.image],
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

                if (config.name == "user") {
                    tile.css("background-image", "url(/Content/Images/imgPerfil/avatar_06.png)");
                } else {

                    tile.addClass(obj.color)
                }
                tile.css("background-repeat", "no-repeat");
                tile.css("background-position", "50% 0%")
            }

            tile.attr("data-id", obj.id);
            if (typeof config.data != "undefined") {
                var keys = Object.keys(config.data);
                for (var key in keys) {
                    tile.attr("data-" + keys[key], obj.data[config.data[keys[key]]]);
                }
            }

            if (fieldGrouping == null) {
                var div = jQuery("<div/>").addClass("btn-group");
                if (typeof config.permissions != "undefined" && config.permissions[0] == "u") {

                    div.append(
                        jQuery("<a/>").addClass("btn btn-mini")
                            .append(jQuery("<i>").addClass("icon-edit"))
                            .attr("data-original-title", "Editar")
                            .attr("rel", "tooltip")
                            .attr("data-placement", "top")
                            .bind("click.edit", config.displayConfiguration.sliderOptions.onEditTileAction)
                    );
                }
                if (typeof config.permissions != "undefined" && config.permissions[1] == "d") {
                    div.append(
                        jQuery("<a/>").addClass("btn btn-mini")
                            .attr("href", "#")
                            .attr("data-original-title", "Eliminar")
                            .attr("rel", "tooltip")
                            .attr("data-placement", "top")
                            .append(jQuery("<i>").addClass("icon-trash"))
                            .bind("click.delete", config.displayConfiguration.sliderOptions.onDeleteTileAction)
                    );
                }

                var actionKeys = Object.keys(extraAction);
                for (var key in actionKeys) {
                    try{
                    var newOption = jQuery("<a/>").addClass("btn btn-mini")
                        .append(jQuery("<i>").addClass(extraAction[actionKeys[key]].icon))
                        .bind("click." + extraAction[actionKeys[key]].name, extraAction[actionKeys[key]].action);
                    newOption.attr("data-original-title", extraAction[actionKeys[key]].description)
                        .attr("rel", "tooltip")
                        .attr("data-placement", "top");
                    div.append(newOption);
                    } catch (e) {

                    }
                }

                tile.append(div);
            } else { // Group by some field
                var count = jQuery("<div class='_RVCounter'/>").text(obj.data._RVCOUNT);
                tile.append(count);
            }
            tile.append(jQuery("<div/>").addClass("tileName").text(obj.name));

            tile.bind("mouseover.displayDescription", function () {
                jQuery(this).siblings().removeClass("selected");
                jQuery(this).addClass("selected");

                var bg = jQuery(this).css("background");
                jQuery(this).closest(".sliderContainer").find(".sliderImage").css("background", bg);
                jQuery(this).closest(".sliderContainer").find(".sliderDescription").text(jQuery(this).find(".tileName").text());
            });

            tile.bind("click.select", config.displayConfiguration.sliderOptions.onSelectTileAction);
            return tile;
        }
    };

    var tile = {

        printView: function () {
            if (data == null) return;
            var colors1 = ["orange", "blue", "gray", "purple", "green"];
            var colors2 = ["gray", "green", "red", "blue", "pink"];
            var tileElement = jQuery("<div/>").addClass("page");
            var lineElement = jQuery("<div/>").addClass("line");
            var currentColor = null;
            var columnCounter = 0;
            var counter = 0;

            //Show filter input
            containerElement.append(
                $("<div class=tileFilter>").append(
                    $("<input>", {
                        "placeholder": "Filtro"
                    }).on("keyup", function () {
                        tile.searchFilter(this.value);
                    })
                )
            );// END :: Show filter input

            for (var i = 0 in data) {
                if (counter % 5 == 0) {
                    //lineElement = jQuery("<div/>").addClass("line");
                    tileElement.append(lineElement);
                    currentColor = (currentColor == colors1) ? colors2 : colors1;
                    columnCounter = 0;
                }

                //if (fieldGrouping == null) {
                    var tileNode = jQuery("<div/>").addClass("tile ");

                    if (typeof data[i][config.image] != "undefined") {
                        tileNode.css("background", "url('" + data[i][config.image] + "')");
                        tileNode.css("background-size", "100% 100%");
                    } else {
                        if (config.name == "user") {
                            tileNode.css("background-image", "url(/Content/Images/imgPerfil/avatar_06.png)");
                        } else {
                            tileNode.addClass(currentColor[columnCounter]);
                        }
                    }

                    tileNode.attr("data-id", data[i][config.id]);
                    if (typeof config.data != "undefined") {
                        var keys = Object.keys(config.data);
                        for (var key in keys) {
                            tileNode.attr("data-" + keys[key], data[i][config.data[keys[key]]]);
                        }
                    }

                    if (fieldGrouping == null) {
                        var div = jQuery("<div/>").addClass("btn-group");
                        tileNode.append(div);

                        if (typeof config.permissions != "undefined" && config.permissions[0] == "u") {

                            div.append(
                                jQuery("<a/>").addClass("btn btn-mini")
                                    .append(jQuery("<i>").addClass("icon-edit"))
                                    .attr("data-original-title", "Editar")
                                    .attr("rel", "tooltip")
                                    .attr("data-placement", "top")
                                    .bind("click.edit", config.displayConfiguration.tilesOptions.onEditTileAction)
                            );
                        }
                        if (typeof config.permissions != "undefined" && config.permissions[1] == "d") {
                            div.append(
                                jQuery("<a/>").addClass("btn btn-mini")
                                    .attr("href", "#")
                                    .append(jQuery("<i>").addClass("icon-trash"))
                                    .attr("data-original-title", "Borrar")
                                    .attr("rel", "tooltip")
                                    .attr("data-placement", "top")
                                    .bind("click.delete", config.displayConfiguration.tilesOptions.onDeleteTileAction)
                            );
                        }

                        var btngp = jQuery("<div/>").addClass("btn-group");
                        var actionKeys = Object.keys(extraAction);
                        for (var key in actionKeys) {
                            try{

                            var newOption = jQuery("<a/>").addClass("btn btn-mini")
                                .append(jQuery("<i>").addClass(extraAction[actionKeys[key]].icon))
                                .bind("click." + extraAction[actionKeys[key]].name, extraAction[actionKeys[key]].action)
                                .attr("data-original-title", extraAction[actionKeys[key]].description)
                                .attr("rel", "tooltip")
                                .attr("data-placement", "top");
                            btngp.append(newOption);
                            } catch (e) {

                            }
                        }
                        tileNode.append(btngp);
                    }else{
                        var count = jQuery("<div class='_RVCounter'/>").text(data[i]["_RVCOUNT"]);
                        tileNode.append(count);
                    }

                    tileNode.append(jQuery("<div/>").addClass("tileName").text(data[i][config.name]));

                    tileNode.bind("click.select", function () {
                        var nodeData = data[$(this).data("id")];
                        config.displayConfiguration.tilesOptions.onSelectTileAction(nodeData, $(this));
                    });

                    // Add this element to the selected array
                    if (config.displayConfiguration.tilesOptions.showSelectOption == true) {
                        tileNode.bind("click.select", function () {
                            tile.selectTile($(this))
                            setSelectedOption($(this).data("id"));
                        });
                    }

                    lineElement.append(tileNode);
                columnCounter++;
                counter++;
            }
            containerElement.append(optionsMenu.getMenu());
            containerElement.append(operations.createPreview());
            containerElement.append(tileElement);
        },

        //Add or remove a selected mark 
        selectTile: function (sTile) {
            //Add selected mark
            if (selectedOptions.indexOf(sTile.data("id")) < 0) {
                sTile.append(
                    $("<div>", {"class":"selectedTile"})
                ).append(
                    $("<span class=selectedArrow>").html("<i class=icon-ok>")
                );
            }
            else { //Remove selected mark
                sTile.find(".selectedTile").remove();
                sTile.find(".selectedArrow").remove();
            }
        },

        //Search for matching elements 
        searchFilter: function (filter) {
            filter = filter.toLowerCase().trim();
            for (var d in data) {
                var element = data[d];
                var match = false;

                //If name match
                if (element.name.toLowerCase().indexOf(filter) !== -1)
                    match = true;
                else {
                    //Check for custom fields
                    var moreDetails = element.profileFields;
                    if (moreDetails != undefined && moreDetails != "") {
                        var oKeys = Object.keys(moreDetails);
                        for (var o in oKeys) {
                            var attr = oKeys[o].split("_HTKField");
                            if (attr.length > 1) {
                                if (moreDetails[oKeys[o]].toLowerCase().indexOf(filter) !== -1) {
                                    match = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (match)
                    $(".line .tile[data-id=" + d + "]").show();
                else
                    $(".line .tile[data-id=" + d + "]").hide();

            }
        }
    };

    var table = {
        printView: function () {
            if (data == null) return;
            var tableElement = jQuery("<table/>").addClass("table table-striped table-bordered table-hover");
            tableElement.prop("id", containerID + "_rvtable");
            tableElement.append(jQuery("<thead/>"))
                 .append(jQuery("<tbody/>"));

            var headerElement = jQuery("<tr/>");

            if (typeof config.showSelecters != "undefined" && config.showSelecters == true) {
                var checkerHeader = jQuery("<th/>");

                checkerHeader.append(jQuery("<input/>").attr("type", "checkbox").attr("id", "checkall").addClass("checkall")
                    ).css("text-align", "center");

                checkerHeader.bind("change.select", function () {
                    if (checkerHeader.find("input:first").prop("checked") == true) {
                        checkerHeader.closest("table").find(".checker:not(:checked)").prop("checked", true).trigger("change");
                    } else {
                        checkerHeader.closest("table").find(".checker:checked").prop("checked", false).trigger("change");
                    }
                });

                headerElement.append(checkerHeader);
            }
            for (var column in columnNames) {
                if (columnShow[column] == 1) {
                    var newTD = jQuery("<th/>").text(columnNames[column]);
                    headerElement.append(newTD);
                }
            }
            var newTD = jQuery("<th/>").text("Acciones");
            headerElement.append(newTD);
            tableElement.find("thead").append(headerElement);

            if (data.length < 1) {
                var newTr = jQuery("<tr/>")
                    .append(jQuery("<td/>")
                        .attr("colspan", Object.keys(columnNames).length + 1)
                        .css("text-align", "center")
                        .text("No se encontraron datos.")
                    );
                tableElement.find("tbody").append(newTr);
            }

            for (var i in data) {
                var newTr = table.createNode(data[i]);
                tableElement.find("tbody").append(newTr);
            }
            containerElement.append(optionsMenu.getMenu());
            containerElement.append(columnsTable.getButton());
            containerElement.append(operations.createPreview());
            containerElement.append(tableElement);
            try{
                tableElement.dataTable({
                    "sPaginationType": "bootstrap",
                    "sDom": "<'tableHeader'<l><'clearfix'f>r>t<'tableFooter'<i><'clearfix'p>>",
                    "iDisplayLength": 10,
                    "sScrollY":"400px",
                    "aoColumnDefs": [{
                        'bSortable': false,
                        'aTargets': [0]
                    }],
                    "oLanguage": {
                        "sLengthMenu": "Mostrar _MENU_ registros",
                        "sInfo": "Mostrando del _START_ al _END_ de _TOTAL_ registros",
                        "sSearch": "Filtro"
                    }
                });
                setTimeout(function () {
                    var table = jQuery(tableElement).dataTable();
                    table.fnAdjustColumnSizing();

                }, 1000);
            } catch (e) { }
           
        },
        createNode: function (obj) {
            var newTr = jQuery("<tr/>");
            var newTd = jQuery("<td/>");
            try{
                newTr.attr("data-id", obj[config.id]);
            } catch (ex) {

            }

            if (typeof config.data != "undefined") {
                var keys = Object.keys(config.data);
                for (var key in keys) {
                    try{
                        newTr.attr("data-" + keys[key], obj[config.data[keys[key]]]);
                    } catch (ex) {

                    }
                }
            }

            newTr.bind("click.select", function () {
                try{
                    var nodeData = data[$(this).closest("tr").data("id")];
                    config.displayConfiguration.tableOptions.onSelectRowAction(nodeData, $(this))
                } catch (ex) {

                }
            });
            newTr.css("cursor", "pointer");

            if (typeof config.showSelecters != "undefined" && config.showSelecters == true) {
                newTr.append(
                    jQuery("<td/>").append(
                        jQuery("<input/>").attr("type", "checkbox").addClass("checker")
                    ).css("text-align", "center").on("change", function () {
                        var opt = $(this).closest("tr").data("id");
                        setSelectedOption(opt);
                    })
                );
            }

            for (var column in columnNames) {
                if (columnShow[column] == 1) {
                    if (column.indexOf('.') === -1) {
                        try{
                            newTr.append(jQuery("<td/>").text(obj[column]).addClass(column));
                        } catch (ex) {

                        }
                    } else {
                        try{
                            newTr.append(jQuery("<td/>").text(getColumnValue(obj, column)).addClass(column));
                        } catch (ex) {

                        }
                        }
                }

            }


            var div = jQuery("<div/>").addClass("btn-group");
            newTr.append(jQuery("<td/>").append(div));
            if (fieldGrouping == null) {
                if (typeof config.permissions != "undefined" && config.permissions[0] == "u") {

                    div.append(

                          jQuery("<a/>").addClass("btn")

                              .append(jQuery("<i>").addClass("icon-edit"))
                                .attr("data-original-title", "Editar")
                                .attr("rel", "tooltip")
                                .attr("data-placement", "top")
                              .bind("click.edit", config.displayConfiguration.tableOptions.onEditRowAction)
                      );
                }
                if (typeof config.permissions != "undefined" && config.permissions[1] == "d") {
                    div.append(
                            jQuery("<a/>").addClass("btn")
                                .attr("href", "#")
                                .append(jQuery("<i>").addClass("icon-trash"))
                                .attr("data-original-title", "Borrar")
                                .attr("rel", "tooltip")
                                .attr("data-placement", "top")
                                .bind("click.delete", config.displayConfiguration.tableOptions.onDeleteRowAction)
                        );
                }


                var actionKeys = Object.keys(extraAction);
                for (var key in actionKeys) {
                    try{
                        var newOption = jQuery("<a/>").addClass("btn")
                        .append(jQuery("<i>").addClass(extraAction[actionKeys[key]].icon))
                        .bind("click." + extraAction[actionKeys[key]].name, extraAction[actionKeys[key]].action)
                        .attr("data-original-title", extraAction[actionKeys[key]].description)
                        .attr("rel", "tooltip")
                        .attr("data-placement", "top");
                        div.append(newOption);
                    } catch (e) {

                    }
                }
            }

            if (newTr.data("systemstatus") == false)
                newTr.css("color", "red");

            return newTr;
        }
    };

    /*
        Operations to do with the data objects fields
    */
    var operations = {

        fieldOperations: [],

        operations: ["Sumatoria", "Promedio", "Minimo", "Maximo"],

        doOperation: function (field, operation) {
            var result;
            var m = [];
            for (var key in data) {
                console.log(key)
                m.push(key)
            }
            switch (operation) {
                case "Sumatoria":
                    result = operations.sum(field);
                    break;
                case "Promedio":
                    result = operations.sum(field);
                    result = result == 0 ? result : result / m.length;
                    break;
                case "Minimo":
                    result = operations.min(field);
                    break;
                case "Maximo":
                    result = operations.max(field);
                    break;
                default:
                    break;
            }
            return result.format(2, 3);
        },

        getOperations: function () {
            return operations.fieldOperations;
        },

        printViewModal: function (rvObj) {
            if (!jQuery("#_RVOperationsModal").length) {
                var modal = jQuery('<div class="modal fade" id="_RVOperationsModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true"/>');
                var dialog = jQuery('<div class="modal-dialog"/>');
                var content = jQuery('<div class="modal-content">');
                var header = jQuery('<div class="modal-header">');
                header.append('<button type="button" class="close" data-dismiss="modal" aria-hidden="true"><i class="icon-remove" style="margin-top: 10px; margin-right: 10px"></i></button>');
                header.append('<h4 class="modal-title" id="operation-title">Operaciones</h4>');

                var body = jQuery('<div class="modal-body">');
                var table = jQuery('<table width="100%">')
                    .append(jQuery("<tr/>")
                        .append(jQuery('<td width="50%">')
                            .append(jQuery('<label>Operación:</label>'))
                            .append(jQuery('<div class="staticField">')
                                .append(jQuery('<select id="_RVOperationsModaloperationSelecter">')

                                )
                            )
                        )
                    )
                    .append(jQuery("<tr/>")
                        .append(jQuery('<td width="50%">')
                            .append(jQuery('<label>Seleccione un campo para realizar la operación:</label>'))
                            .append(jQuery('<div class="staticField">')
                                .append(jQuery('<select id="_RVOperationsModaloperationFieldSelecter">')

                                )
                            )
                        )
                    )

                var selecter = table.find("#_RVOperationsModaloperationSelecter");
                for (var i in operations.operations) {
                    selecter.append(jQuery("<option value=" + operations.operations[i] + ">" + operations.operations[i] + "</option>"));
                }

                var fieldSelecter = table.find("#_RVOperationsModaloperationFieldSelecter");
                for (var i in columnNames) {
                    fieldSelecter.append(jQuery("<option value='" + columnNames[i] + "'>" + columnNames[i] + "</option>"));
                }

                var footer = jQuery('<div class="modal-footer"/>')
                    .append(jQuery('<div class="row-fluid"/>')
                        .append(jQuery('<div class="span8 error_msg" id="operation_result"/>'))
                        .append(jQuery('<div class="span4">')
                            .append(jQuery('<input value="Añadir" class="btn blue" type="button" id="_RVOperationsModaloperation_button" data-dismiss="modal">'))
                        )
                    );

                body.append(table);
                content.append(header);
                content.append(body);
                content.append(footer);
                dialog.append(content);
                modal.append(dialog);

                jQuery("body").append(modal);
                operations.bindElements(rvObj);
            }
        },

        sum: function (field) {
            var fieldRealName = getColumnByName(field);
            var result = 0;
            //new code
            var m = [];
            for (var key in originalData) {

                m.push(key)
            }
            for (var i = 0; i < m.length; i++) {
                var value = getColumnValue(originalData[m[i]], fieldRealName);
                if (!isNaN(value)) {
                    if (!isNaN(parseFloat(value))) {
                        result += parseFloat(value);
                    }
                }
            }
            //end new code
          /*  for (var i = 0; i < originalData.length; i++) {
                var value = getColumnValue(originalData[i], fieldRealName);
                if (!isNaN(value)) {
                    result += parseFloat(value);
                }
            }*/
            return result;
        },

        min: function (field) {
            if (originalData == null) return 0;
            var fieldRealName = getColumnByName(field);
            var min = null;
            var value = null;
            //new code
            var m = [];
            for (var key in originalData) {

                m.push(key)
            }
            for (var i = 0; i < m.length; i++) {
                var val = getColumnValue(originalData[m[i]], fieldRealName);
                if (!isNaN(val)) {
                    if (!isNaN(parseFloat(val))) {
                        value = parseFloat(val);
                        if ((min == null || min > value) && value != 0) {
                            min = value;
                        }
                    }
                }
            }
            //end new code
           /* for (var i = 0; i < originalData.length; i++) {
                var val = getColumnValue(originalData[i], fieldRealName);
                if (!isNaN(val)) {
                    value = parseFloat(val);
                    if ((min == null || min > value) && value != 0) {
                        min = value;
                    }
                }
            }*/
            if (min == null) return 0;
            return min;
        },

        max: function (field) {
            if (originalData == null) return null;
            var fieldRealName = getColumnByName(field);
            var max = null;
            var value = null;
            //new code
            var m = [];
            for (var key in originalData) {

                m.push(key)
            }
            for (var i = 0; i < m.length; i++) {
                var val = getColumnValue(originalData[m[i]], fieldRealName);
                if (!isNaN(val)) {
                    if (!isNaN(parseFloat(val))) {
                        value = parseFloat(val)
                        if (max == null || max < value) {
                            max = value;
                        }
                    }
                }
            }
            //end new code
           /* for (var i = 0; i < originalData.length; i++) {
                var val = getColumnValue(originalData[i], fieldRealName);
                if (!isNaN(val)) {
                    value = parseFloat(val)
                    if (max == null || max < value) {
                        max = value;
                    }
                }
            }*/
            if (max == null) return 0;
            return max;
        },

        createPreview: function (containerID) {
            var container = jQuery("<div class='RVOperationsContainer'/>");
            for (var elementIndex in operations.fieldOperations) {
                try{
                    var element = operations.fieldOperations[elementIndex];
                    var newElement = jQuery('<div class="alert alert-info" style="float:left;margin:5px;"/>')
                        //.append(jQuery('<button type="button" class="close operationCloseButton" data-dismiss="alert" data-operation="' + element.operation + '" data-field="' + element.field + '">x</button>'))
                    //var newElement = jQuery("<label class='alert-info' data-arraykey = '" + elementIndex + "'/>");
                    /*newElement*/.append(element.operation + " de " + element.field + ": " + operations.doOperation(element.field, element.operation));
                    container.append(newElement);
                } catch (ex) {

                }
            }
            return container;
        },

        inOperationArray: function (obj) {
            for (var elementIndex in operations.fieldOperations) {
                try{
                    var element = operations.fieldOperations[elementIndex];
                    if (element.field == obj.field && element.operation == obj.operation) {
                        return true;
                    }
                } catch (e) {

                }
            }
            return false;
        },

        //bindPreview: function () {
        //    jQuery(".operationCloseButton").unbind("click.close");
        //    jQuery(".operationCloseButton").bind("click.close", function () {
        //        var operation = $(this).data("operation");
        //        var field = $(this).data("field");

        //    });
        //},

        bindElements: function (rvObj) {

            jQuery("#_RVOperationsModaloperation_button").unbind("click.addOperation");
            jQuery("#_RVOperationsModaloperation_button").bind("click.addOperation", function () {
                debugger;
                var operation = jQuery("#_RVOperationsModaloperationSelecter").val();
                var field = jQuery("#_RVOperationsModaloperationFieldSelecter").val();
                var obj = { operation: operation, field: field };
                if (!operations.inOperationArray(obj)) {
                    operations.fieldOperations.push(obj);
                    rvObj.printView();
                }
            });
        }
    };

    operations.printViewModal(this);
}