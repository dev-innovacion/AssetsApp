//
; (function ($) {

    SVG.extend(SVG.Element, {
        //Make an element resizable and movable
        //@param onMove : The action to perfom while the element is moving
        makeResizable: function (onMove) {
            var element = this,
                parent = this.parent,
                wrapper, nested, action = 0, limit = false;
            var box;

            //Create the moving custom event
            var movingEvent = new Event("moving");

            if (typeof element.fixResize != "undefined")
                return this;

            var mouseLastX = 0, mouseLastY = 0,
                actionList = {
                    none: 0,
                    neResize: 1,
                    nwResize: 2,
                    seResize: 3,
                    swResize: 4,
                    move: 5
                }
            //parent  = this.parent._parent(SVG.Nested) || this._parent(SVG.Doc)

            //
            init = (function () {
                if (element.type == "path") {
                    box = element.bbox();
                    nested = parent.nested().attr({ width: box.width, height: box.height });
                } else {
                    nested = parent.nested().attr({ width: element.width(), height: element.height() });
                }

                wrapper = nested.group()
                //.draggable();
                if (element.type == "path") {
                    wrapper.move(box.x, box.y);
                } else
                    wrapper.move(element.x(), element.y());

                //var wrap = nested.rect("100%", "100%")
                //  .attr({"opacity":0,"style":"cursor:move","fill":"red"});
                element.attr({ "style": "cursor:move" });

                circleAttr = {
                    "stroke": "#0000FF",
                    "stroke-width": "1",
                    "fill": "#CCCCFF"
                };
                var NE = nested.circle(10).attr({ cx: "100%", cy: 0, style: "cursor:ne-resize" }).attr(circleAttr);
                var NW = nested.circle(10).attr({ cx: 0, cy: 0, style: "cursor:nw-resize" }).attr(circleAttr);
                var SE = nested.circle(10).attr({ cx: "100%", cy: "100%", style: "cursor:se-resize" }).attr(circleAttr);
                var SW = nested.circle(10).attr({ cx: 0, cy: "100%", style: "cursor:sw-resize" }).attr(circleAttr);

                //its rotated
                var rotated;
                if (element.attr("transform")) {
                    element.rotate();
                    rotated = true;
                }
                if (element.type == "rect") {
                    element.attr({ x: 0, y: 0, width: "100%", height: "100%" });
                }
                if (element.type == "ellipse") {

                    element.attr({ rx: "50%", ry: "50%", cx: "50%", cy: "50%", x: 0, y: 0, width: "100%", height: "100%" });
                }
                if (element.type == "path") {
                    var newd = "M";
                    var num;
                    var arraycad = element.attr("d").split(" ");
                    for (i = 0; i < arraycad.length; i++) {
                        if (arraycad[i].match(/M/)) {
                            num = parseInt(arraycad[i].replace("M", "").replace(" ", ""));
                            num = num - box.x;
                            newd = newd + num.toString();
                        }
                        else if (arraycad[i].match(/L/)) {
                            num = parseInt(arraycad[i].replace("L", "").replace(" ", ""));
                            num = num - box.x;
                            newd = newd + " L" + num.toString();
                        }
                        else if (arraycad[i].match(/Z/)) {
                            newd = newd + " Z";
                        }
                        else {
                            num = parseInt(arraycad[i].replace(" ", ""));
                            num = num - box.y;
                            newd = newd + " " + num.toString();
                        }
                    }

                    element.attr("d", newd);

                }


                wrapper.add(element);
                //wrapper.add(wrap);
                wrapper.add(NE).add(NW).add(SE).add(SW);

                //set rotated again
                if (rotated)
                    element.rotate(45);

                element.on("mousedown", function (event) {
                    if (event.button == 0) {
                        action = actionList.move;
                    }
                });

                NE.on("mousedown", function (event) {
                    if (event.button == 0) {
                        action = actionList.neResize;
                    }
                });
                NW.on("mousedown", function (event) {
                    if (event.button == 0) {
                        action = actionList.nwResize;
                    }
                });
                SE.on("mousedown", function (event) {
                    if (event.button == 0) {
                        action = actionList.seResize;
                    }
                });
                SW.on("mousedown", function (event) {
                    if (event.button == 0) {
                        action = actionList.swResize;
                    }
                });

                //Catch the position of the cursor on mousedown before start moving
                $("#" + parent.node.id).unbind("mousedown.move");
                $("#" + parent.node.id).bind("mousedown.move", function (event) {
                    if (event.button == 0) {
                        mouseLastX = event.offsetX;
                        mouseLastY = event.offsetY;

                        // Stop propagation
                        if (action != actionList.none)
                            event.stopPropagation();
                    }
                });

                $("#" + parent.parent.node.id).unbind("mouseup.resize");
                $("#" + parent.parent.node.id).bind("mouseup.resize", function (event) {
                    if (event.button == 0) {
                        action = actionList.none;
                        limit = false;
                    }
                });

                $("#" + parent.parent.node.id).unbind("mousemove.move");
                $("#" + parent.parent.node.id).bind("mousemove.move", function (event) {
                    if (action != 0) {
                        var currMouseX = event.offsetX;
                        var currMouseY = event.offsetY;

                        var deltaX = currMouseX - mouseLastX;
                        var deltaY = currMouseY - mouseLastY;

                        resizing(deltaX, deltaY);

                        mouseLastX = currMouseX;
                        mouseLastY = currMouseY;
                    }
                });

            })();

            var resizing = function (deltaX, deltaY) {

                var deltaTop = 0,
                    deltaLeft = 0,
                    deltaHeight = 0,
                    deltaWidth = 0;

                if (action == actionList.nwResize || action == actionList.neResize) {
                    deltaTop = deltaY;
                    deltaHeight = -deltaY;
                }
                if (action == actionList.nwResize || action == actionList.swResize) {
                    deltaLeft = deltaX;
                    deltaWidth = -deltaX;
                }
                if (action == actionList.neResize || action == actionList.seResize) {
                    deltaWidth = deltaX;
                }
                if (action == actionList.swResize || action == actionList.seResize) {
                    deltaHeight = deltaY;
                }
                if (action == actionList.move) {
                    deltaLeft = deltaX;
                    deltaTop = deltaY;


                }

                //Fire the moving event
                if (action != actionList.none) {
                    element.node.dispatchEvent(movingEvent);
                }

                updateSize(deltaWidth, deltaHeight);
                updatePosition(deltaLeft, deltaTop);

            };

            var updateSize = function (deltaWidth, deltaHeight) {
                var newWidth = nested.width() + deltaWidth;
                var newHeight = nested.height() + deltaHeight;

                if (newWidth > 0 && newHeight > 0)
                    nested.attr({ "width": newWidth, "height": newHeight });
                else
                    limit = true;
            };

            var updatePosition = function (deltaLeft, deltaTop) {
                var newLeft = wrapper.x() + deltaLeft;
                var newTop = wrapper.y() + deltaTop;

                if (!limit)
                    wrapper.move(newLeft, newTop);
            };

            // Return the element to its original form, no wrapped
            element.fixResize = function () {
                element.addTo(parent);
                if (element.type == "rect") {
                    //its rotated
                    var rotated;
                    if (element.attr("transform")) {
                        element.rotate();
                        rotated = true;
                    }

                    element.attr({ x: wrapper.x(), y: wrapper.y(), width: nested.width(), height: nested.height() });

                    //return rotation
                    if (rotated)
                        element.rotate(45);
                }
                if (element.type == "ellipse") {
                    element.attr({ rx: nested.width() / 2, ry: nested.height() / 2, cx: wrapper.x() + nested.width() / 2, cy: wrapper.y() + nested.height() / 2, x: wrapper.x(), y: wrapper.y(), width: nested.width(), height: nested.height() });
                }
                if (element.type == "path") {
                    var newd = "M";
                    var num;
                    var box = element.bbox();
                    if (box.x != wrapper.x() || box.y != wrapper.y()) {


                        var arraycad = element.attr("d").split(" ");
                        for (i = 0; i < arraycad.length; i++) {
                            if (arraycad[i].match(/M/)) {
                                num = parseInt(arraycad[i].replace("M", "").replace(" ", ""));
                                num = num + wrapper.x();
                                newd = newd + num.toString();
                            }
                            else if (arraycad[i].match(/L/)) {
                                num = parseInt(arraycad[i].replace("L", "").replace(" ", ""));
                                num = num + wrapper.x();
                                newd = newd + " L" + num.toString();
                            }
                            else if (arraycad[i].match(/Z/)) {
                                newd = newd + " Z";
                            }
                            else {
                                num = parseInt(arraycad[i].replace(" ", ""));
                                num = num + wrapper.y();
                                newd = newd + " " + num.toString();
                            }
                        }

                        element.attr("d", newd);
                    }
                }

                nested.remove();
                element.attr({ style: "cursor:hand" });
                $("#" + parent.node.id).unbind("mousedown.resize");
                $("#" + parent.node.id).unbind("mousedown.resize");
                $("#" + parent.parent.node.id).unbind("mousemove.move");

                element.fixResize = undefined;

            };

            return this;
        },

        //Make Custom resize :: Karina
        makeResizableControl: function (onMove) {
            var element = this,
                parent = this.parent,
                wrapper, nested, action = 0, limit = false;
            var box;

            //Create the moving custom event
            var movingEvent = new Event("moving");

            if (typeof element.fixResize != "undefined")
                return this;

            var mouseLastX = 0, mouseLastY = 0,
                actionList = {
                    none: 0,
                    neResize: 1,
                    nwResize: 2,
                    seResize: 3,
                    swResize: 4,
                    move: 5
                }
            //parent  = this.parent._parent(SVG.Nested) || this._parent(SVG.Doc)

            //
            init = (function () {
                if (element.type == "path") {
                    box = element.bbox();
                    nested = parent.nested().attr({ width: box.width, height: box.height });
                } else {
                    nested = parent.nested().attr({ width: element.width(), height: element.height() });
                }

                wrapper = nested.group()
                //.draggable();
                if (element.type == "path") {
                    wrapper.move(box.x, box.y);
                } else
                    wrapper.move(element.x(), element.y());

                //var wrap = nested.rect("100%", "100%")
                //  .attr({"opacity":0,"style":"cursor:move","fill":"red"});
                element.attr({ "style": "cursor:move" });

                circleAttr = {
                    "stroke": "#0000FF",
                    "stroke-width": "1",
                    "fill": "#CCCCFF"
                };
                var NE = nested.circle(10).attr({ cx: "100%", cy: 0 }).attr(circleAttr);
                var NW = nested.circle(10).attr({ cx: 0, cy: 0 }).attr(circleAttr);
                var SE = nested.circle(10).attr({ cx: "100%", cy: "100%" }).attr(circleAttr);
                var SW = nested.circle(10).attr({ cx: 0, cy: "100%" }).attr(circleAttr);

                //its rotated
                var rotated;
                if (element.attr("transform")) {
                    element.rotate();
                    rotated = true;
                }
                if (element.type == "rect") {
                    element.attr({ x: 0, y: 0, width: "100%", height: "100%" });
                }
                if (element.type == "ellipse") {

                    element.attr({ rx: "50%", ry: "50%", cx: "50%", cy: "50%", x: 0, y: 0, width: "100%", height: "100%" });
                }
                if (element.type == "path") {
                    var newd = "M";
                    var num;
                    var arraycad = element.attr("d").split(" ");
                    for (i = 0; i < arraycad.length; i++) {
                        if (arraycad[i].match(/M/)) {
                            num = parseInt(arraycad[i].replace("M", "").replace(" ", ""));
                            num = num - box.x;
                            newd = newd + num.toString();
                        }
                        else if (arraycad[i].match(/L/)) {
                            num = parseInt(arraycad[i].replace("L", "").replace(" ", ""));
                            num = num - box.x;
                            newd = newd + " L" + num.toString();
                        }
                        else if (arraycad[i].match(/Z/)) {
                            newd = newd + " Z";
                        }
                        else {
                            num = parseInt(arraycad[i].replace(" ", ""));
                            num = num - box.y;
                            newd = newd + " " + num.toString();
                        }
                    }

                    element.attr("d", newd);

                }


                wrapper.add(element);
                //wrapper.add(wrap);
                wrapper.add(NE).add(NW).add(SE).add(SW);

                //set rotated again
                if (rotated)
                    element.rotate(45);

                element.on("mousedown", function (event) {
                    if (event.button == 0) {
                        action = actionList.move;
                    }
                });

                //NE.on("mousedown", function (event) {
                //    if (event.button == 0) {
                //        action = actionList.neResize;
                //    }
                //});
                //NW.on("mousedown", function (event) {
                //    if (event.button == 0) {
                //        action = actionList.nwResize;
                //    }
                //});
                //SE.on("mousedown", function (event) {
                //    if (event.button == 0) {
                //        action = actionList.seResize;
                //    }
                //});
                //SW.on("mousedown", function (event) {
                //    if (event.button == 0) {
                //        action = actionList.swResize;
                //    }
                //});

                //Catch the position of the cursor on mousedown before start moving
                $("#" + parent.parent.node.id).bind("mousedown.move");
                $("#" + parent.parent.node.id).bind("mousedown.move", function (event) {
                    mouseLastX = event.offsetX;
                    mouseLastY = event.offsetY;
                });

                $("#" + parent.parent.node.id).unbind("mouseup.resize");
                $("#" + parent.parent.node.id).bind("mouseup.resize", function (event) {
                    if (event.button == 0) {
                        action = actionList.none;
                        limit = false;
                    }
                });

                $("#" + parent.parent.node.id).unbind("mousemove.move");
                $("#" + parent.parent.node.id).bind("mousemove.move", function (event) {
                    if (action != 0) {
                        var currMouseX = event.offsetX;
                        var currMouseY = event.offsetY;

                        var deltaX = currMouseX - mouseLastX;
                        var deltaY = currMouseY - mouseLastY;

                        resizing(deltaX, deltaY);

                        mouseLastX = currMouseX;
                        mouseLastY = currMouseY;

                    }
                });

            })();

            var resizing = function (deltaX, deltaY) {

                var deltaTop = 0,
                    deltaLeft = 0,
                    deltaHeight = 0,
                    deltaWidth = 0;

                if (action == actionList.nwResize || action == actionList.neResize) {
                    deltaTop = deltaY;
                    deltaHeight = -deltaY;
                }
                if (action == actionList.nwResize || action == actionList.swResize) {
                    deltaLeft = deltaX;
                    deltaWidth = -deltaX;
                }
                if (action == actionList.neResize || action == actionList.seResize) {
                    deltaWidth = deltaX;
                }
                if (action == actionList.swResize || action == actionList.seResize) {
                    deltaHeight = deltaY;
                }
                if (action == actionList.move) {
                    deltaLeft = deltaX;
                    deltaTop = deltaY;


                }

                //Fire the moving event
                if (action != actionList.none) {
                    element.node.dispatchEvent(movingEvent);
                }

                updateSize(deltaWidth, deltaHeight);
                updatePosition(deltaLeft, deltaTop);

            };

            var updateSize = function (deltaWidth, deltaHeight) {
                var newWidth = nested.width() + deltaWidth;
                var newHeight = nested.height() + deltaHeight;

                if (newWidth > 0 && newHeight > 0)
                    nested.attr({ "width": newWidth, "height": newHeight });
                else
                    limit = true;
            };

            var updatePosition = function (deltaLeft, deltaTop) {
                var newLeft = wrapper.x() + deltaLeft;
                var newTop = wrapper.y() + deltaTop;

                if (!limit)
                    wrapper.move(newLeft, newTop);
            };

            // Return the element to its original form, no wrapped
            element.fixResize = function () {
                element.addTo(parent);
                if (element.type == "rect") {
                    //its rotated
                    var rotated;
                    if (element.attr("transform")) {
                        element.rotate();
                        rotated = true;
                    }

                    element.attr({ x: wrapper.x(), y: wrapper.y(), width: nested.width(), height: nested.height() });

                    //return rotation
                    if (rotated)
                        element.rotate(45);
                }
                if (element.type == "ellipse") {
                    element.attr({ rx: nested.width() / 2, ry: nested.height() / 2, cx: wrapper.x() + nested.width() / 2, cy: wrapper.y() + nested.height() / 2, x: wrapper.x(), y: wrapper.y(), width: nested.width(), height: nested.height() });
                }
                if (element.type == "path") {
                    var newd = "M";
                    var num;
                    var box = element.bbox();
                    if (box.x != wrapper.x() || box.y != wrapper.y()) {


                        var arraycad = element.attr("d").split(" ");
                        for (i = 0; i < arraycad.length; i++) {
                            if (arraycad[i].match(/M/)) {
                                num = parseInt(arraycad[i].replace("M", "").replace(" ", ""));
                                num = num + wrapper.x();
                                newd = newd + num.toString();
                            }
                            else if (arraycad[i].match(/L/)) {
                                num = parseInt(arraycad[i].replace("L", "").replace(" ", ""));
                                num = num + wrapper.x();
                                newd = newd + " L" + num.toString();
                            }
                            else if (arraycad[i].match(/Z/)) {
                                newd = newd + " Z";
                            }
                            else {
                                num = parseInt(arraycad[i].replace(" ", ""));
                                num = num + wrapper.y();
                                newd = newd + " " + num.toString();
                            }
                        }

                        element.attr("d", newd);
                    }
                }

                nested.remove();
                element.attr({ style: "cursor:hand" });
                $("#" + parent.node.id).unbind("mousedown.resize");
                $("#" + parent.node.id).unbind("mousedown.resize");
                $("#" + parent.parent.node.id).unbind("mousemove.move");

                element.fixResize = undefined;

            };

            return this;
        },

        //Make the scenario zoomable with pan and scan
        makeZoomable: function () {
            var drawing = this.parent;
            var wa = drawing.parent;
            var z1 = wa.childNodes[1];
            var z2 = wa.childNodes[3];
            var view_box = drawing.viewbox();
            var mouseLastPosition = { x: 0, y: 0 };
            var action = 0;
            var actions = {
                none: 0,
                grabbing: 1,
            }

            $(drawing.node).css("cursor", "-webkit-grab");

            // Zoom In -->  
            var zoomin = function (cursor) {
                if (view_box.zoom < 8) {
                    var newView = {};
                    newView.width = view_box.width * 0.5;
                    newView.height = view_box.height * 0.5;

                    var relativeX = (view_box.width / drawing.width());
                    var relativeY = (view_box.height / drawing.height());

                    var newX = (cursor.x * relativeX + view_box.x) - (newView.width / 2);
                    var newY = (cursor.y * relativeY + view_box.y) - (newView.height / 2);

                    if (newX <= 0)
                        newView.x = 0;
                    else
                        newView.x = newX;

                    if (newY <= 0)
                        newView.y = 0;
                    else
                        newView.y = newY;

                    drawing.viewbox(newView);
                    view_box = drawing.viewbox();   
                }
            };

            // Zoom out <--
            var zoomout = function (cursor) {
                //if (view_box.zoom >= 1) {
                    var newView = {};
                    newView.width = view_box.width * 2;
                    newView.height = view_box.height * 2;

                    var relativeX = (view_box.width / drawing.width());
                    var relativeY = (view_box.height / drawing.height());

                    var newX = (cursor.x * relativeX + view_box.x) - (newView.width / 2);
                    var newY = (cursor.y * relativeY + view_box.y) - (newView.height / 2);

                    if (newX <= 0)
                        newView.x = 0;
                    else
                        newView.x = newX;

                    if (newY <= 0)
                        newView.y = 0;
                    else
                        newView.y = newY;

                    drawing.viewbox(newView);
                    view_box = drawing.viewbox();
                //}

                //if (view_box.zoom < 1)
                    //resetZoom();
            };

            var resetZoom = function () {
                drawing.viewbox(0, 0, drawing.width(), drawing.height());
                view_box = drawing.viewbox();
            };

            var init = (function () {
                if (view_box.zoom != 1 || view_box.x != 0 || view_box.y != 0)
                    resetZoom();
            })();

            // Event to capture the click down action
            $(drawing.node).unbind("mousedown.grab");
            $(drawing.node).bind("mousedown.grab", function () {
                $(drawing.node).css("cursor", "-webkit-grabbing");
                action = actions.grabbing;
                
                mouseLastPosition.x = event.clientX;
                mouseLastPosition.y = event.clientY;

            });

            $(drawing.node).unbind("mouseup.release");
            $(drawing.node).bind("mouseup.release", function () {
                $(drawing.node).css("cursor", "-webkit-grab");
                action = actions.none;
            });

            $(drawing.node).unbind("mouseout.leave");
            $(drawing.node).bind("mouseout.leave", function () {
                action = actions.none;
            });

            $(drawing.node).unbind("mousemove.grabbing");
            $(drawing.node).bind("mousemove.grabbing", function () {
                if (action == actions.grabbing) {
                   // if (view_box.zoom != 1 || view_box.x != 0 || view_box.y != 0) {
                        var newX = event.clientX - mouseLastPosition.x;
                        var newY = event.clientY - mouseLastPosition.y;

                        view_box.x -= newX;
                        view_box.y -= newY;

                        drawing.viewbox(view_box);
                        mouseLastPosition.x = event.clientX;
                        mouseLastPosition.y = event.clientY;
                    //}
                }
                else
                    $(drawing.node).css("cursor", "-webkit-grab");
            });

            // Event to capture the mouse wheel action
            drawing.on("mousewheel", function (event) {
                var cursor = {
                    x: event.offsetX,
                    y: event.offsetY
                };

                if (event.wheelDeltaY > 0)
                    zoomin(cursor);
                else
                    zoomout(cursor);

                event.preventDefault();
            });

            z1.onclick = function () {
                var cursor = {
                    x: 400,
                    y: 300
                };
                zoomin(cursor);
            };

            z2.onclick = function () {
                var cursor = {
                    x: 400,
                    y: 300
                };
                zoomout(cursor);

            };

            return this;
        }

    })

})(jQuery);