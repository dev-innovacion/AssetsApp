// Shows an alert windows in color and message assigned
function _alert(type, msg) {
    jQuery("#alertmodal").removeClass("alert-success");
    jQuery("#alertmodal").removeClass("alert-danger");
    jQuery("#alertmodal").removeClass("alert-warning");
    if (type == "success") {
        jQuery("#alertmodal").addClass("alert-success");
    }
    if (type == "error")
    {
        jQuery("#alertmodal").addClass("alert-danger");
    }
    if (type == "warning") {
        jQuery("#alertmodal").addClass("alert-warning");
    }

    jQuery("#alertmodal").text(msg);

    jQuery("#alertmodal").stop().fadeIn("slow").delay(2000).fadeOut("slow");

}

var sett;

// Shows a confirm modal and then execute the action assigned
function _confirm(settings) {
    sett = settings;
    
    jQuery("#confirmmodal .modal-header-text").text(settings.title);
    jQuery("#confirmmodal .modal-body").text(settings.message);
    jQuery("#confirmmodal").modal("show");
    jQuery("[value=Sí]").unbind('click.yes');
    jQuery("[value=Sí]").bind('click.yes', function () {
        debugger;
        jQuery("#confirmmodal").modal("hide");
        sett.action();
    });
}

//
function _loadingMini() {

}