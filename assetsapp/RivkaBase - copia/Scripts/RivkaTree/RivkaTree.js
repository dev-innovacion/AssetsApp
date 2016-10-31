RivkaTree.METHODS = { AJAX : 0 } //methods to expand the tree nodes

function RivkaTree(containerID, options) {
    if (typeof containerID == "undefined") throw "RivkaTree: An element's id must be gived.";
    if (!jQuery("#" + containerID).length) throw "RivkaTree: The specified element's id:" + containerID + " does not exist in this context.";
    if (typeof options == "undefined") throw "RivkaTree: Initial options must be gived.";
    if (typeof options.method == "undefined") throw "RivkaTree: Tree method not defined.";
    if (options.method == RivkaTree.METHODS.AJAX && typeof options.url == "undefined") throw "RivkaTree: An url must be gived to use the ajax method.";
    if (typeof options.idKey == "undefined") throw "RivkaTree: An id key must be provided.";
    if (typeof options.nameKey == "undefined") throw "RivkaTree: A name key must be provided.";
    if (options.dragdrop == "active" && typeof options.dragurl == "undefined") throw "RivkaTree: A drag url must be provided.";

    var url = options.url;
    var extraParams = options.extraParams;
    var containerID = containerID;
    var container = jQuery("#" + containerID);
    var expandMethod = options.method;
    var idKey = options.idKey;
    var nameKey = options.nameKey;
    var dragdrop = options.dragdrop;
    var dragurl = options.dragurl;
    var nameStart = '';
    var onSelectNode = (typeof options.onNodeSelectAction == "undefined") ? null : options.onNodeSelectAction;

    this.setContainer = function (containerIDString) {
        if (typeof containerIDString == "undefined") throw "RivkaTree: An element's id must be gived.";
        if (!jQuery("#" + containerIDString).length) throw "RivkaTree: The specified element's id:" + containerIDString + " does not exist in this context.";
        containerID = containerIDString;
        container = jQuery("#" + containerID);
    };

    this.getContainer = function () {
        return containerID;
    };

    this.getUrl = function(){
        return url;
    };

    this.setUrl = function (urlString) {
        if (typeof urlString == "undefined") throw "RivkaTree: A url must be gived.";
        url = urlString;
    };

    this.init = function (rootData) {
        if (typeof rootData == "undefined") throw "RivkaTree: root's data must be gived.";
        if (typeof rootData.id == "undefined") throw "RivkaTree: root's id must be gived.";
        if (typeof rootData.name == "undefined") throw "RivkaTree: root's name must be gived.";
        nameStart = rootData.name;

        container.empty();
        var root = createNode(rootData);
        root.find("a label").addClass("selected");
        root.find("a").unbind("click.ajax");
        root.find("a").bind("click.ajax", function () { expandNode(root, false);});
        container.append(root);
        root.find("a").click();
        root.addClass("loaded");
    };

    var createNode = function (obj) {
        var id;
        var name;
        try {
            id = obj.id;
        } catch (Exception) { return null }
        try {
            name = obj.name;
        } catch (Exception) { return null }

        var newNode = jQuery("<li/>");

        if (dragdrop != "active") {
            newNode.attr("data-idCategory", id);
            newNode.append(
                jQuery("<a/>")
                    .addClass("tree-toggle")
                    .addClass("closed")
                    .attr("data-role", "branch")
                    .append(jQuery("<label>").text(name))
            );
            newNode.append(
                jQuery("<ul/>").addClass("branch")
            );

        } else { //Dag && Drop Option
            container.nestable(dragurl); //Drag && Drop (activation)
            newNode.attr("data-idCategory", id).addClass("dd-item dd3-item");

            newNode.append(jQuery("<div />").addClass("dd3-content")
                .append(jQuery("<a/>")
                    .addClass("tree-toggle")
                    .addClass("closed")
                    .attr("data-role", "branch")
                    .append(jQuery("<label>").text(name))
            ));

            if (name != nameStart) {
                newNode.append(
                    jQuery(jQuery("<div/>").addClass("dd-handle dd3-handle")));
            }

            newNode.append(
                jQuery("<ul/>").addClass("branch")
            );
        }

        newNode.find("a:first").unbind("click.ajax");
        newNode.find("a:first").bind("click.ajax", function () { expandNode(newNode); });

        newNode.find("a:first").unbind("click.open");
        newNode.find("a:first").bind("click.open", function () { openNode(newNode); });

        newNode.find("a:first label").unbind("click.loadTable");
        newNode.find("a:first label").bind("click.loadTable", function () {
            if (!newNode.find("ul:first").hasClass("in"))
                jQuery(this).closest("a").click();
            jQuery("ul.tree li a label.selected").removeClass("selected");
            jQuery(this).addClass("selected");
            return false;
        });

        newNode.find("a:first label").unbind("click.custom");
        newNode.find("a:first label").bind("click.custom", onSelectNode );
        return newNode;
    };

    var expandNode = function (node, async) {
        var liElement = jQuery(node);
        liElement.find("a:first").unbind("click.ajax");
        var id = liElement.data("idcategory");
        jQuery.ajax({
            url: url,
            type: "POST",
            data: { parentCategory: id, extraParams: extraParams },
            async: async == true ? true : false,
            beforeSend: _loading(),
            success: function (data) {
                var nodesObject = JSON.parse(data);
                liElement.find("ul:first").empty();
                for (i = 0; i < nodesObject.length; i++) {
                    newNode = createNode({ id: nodesObject[i][idKey], name: nodesObject[i][nameKey] });
                    liElement.find("ul:first").append(newNode);
                }
                liElement.addClass("loaded");
                _loading();
            },
            error: function (errorThrown) {
                liElement.find("a:first").bind("click.ajax", function () { expandNode(node,async); });
                _loading();
            }
        });
    };

    var openNode = function (node) {
        var node = node;
        node.find("a:first").removeClass("closed");
        node.find("ul:first").addClass("in");
        node.find("a:first").unbind("click.open");
        node.find("a:first").unbind("click.close");
        node.find("a:first").bind("click.close", function () { closeNode(node); });
    };

    var closeNode = function (node) {
        var node = node;
        node.find("a:first").addClass("closed");
        node.find("ul:first").removeClass("in");
        node.find("a:first").unbind("click.close");
        node.find("a:first").unbind("click.open");
        node.find("a:first").bind("click.open", function () { openNode(node) });
    };

    this.openRoute = function (object) {
        for (var i = object.length - 1; i >= 0; i--) {
            var actual = object[i];
            container.find("label.selected").removeClass("selected");
            var treeElement = container.find("[data-idcategory='" + actual.id + "']:first");



            treeElement.find("a:first label:first").addClass("selected");
            if (!treeElement.hasClass("loaded")) {
                treeElement.find("a:first").unbind("click.ajax");
                treeElement.find("a:first").bind("click.ajax", function () { expandNode(treeElement, false); });
                treeElement.find("a:first").click();
                treeElement.find("a:first").unbind("click.ajax");
            }
            treeElement.find("a:first").removeClass("closed");
            treeElement.find("ul:first").addClass("in");

        }
    };

    this.selectNode = function (id) {
        jQuery("ul.tree li a label.selected").removeClass("selected");
        container.find("[data-idcategory='" + id + "']").find("a label:first").addClass("selected");
        container.find("[data-idcategory='" + id + "']").find("a:first").click();
        container.find("[data-idcategory='" + id + "']").find("a:first").removeClass("closed");
        container.find("[data-idcategory='" + id + "']").find("ul:first").addClass("in");
    }
}