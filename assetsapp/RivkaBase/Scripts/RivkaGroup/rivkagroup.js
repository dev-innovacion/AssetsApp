 var tableinit= jQuery("#mailtablex").clone();
 var callback="";
function initGroup(refreshCallback){
    updatetable();
    bindDropdowns();
    if(typeof(refreshCallback)=="function"){

     callback=refreshCallback;

     }
}

function updatetable(){
        tableinit= jQuery("#mailtablex").clone();

}
 function bindlevel0(){
            jQuery("[data-group-level=0]").unbind('click.root');
            jQuery("[data-group-level=0]").bind('click.root', function ( ) {
                debugger;
                var hasclass = jQuery(this).hasClass("expanded-group");
                jQuery(this).closest("tr").nextAll("tr").each(function () {

                    if (jQuery(this).find("[data-group-level=0]").length == 0) {
                        if (hasclass == true) {
                            jQuery(this).hide()
                        } else {
                            jQuery(this).show()
                        }
                    } else {
                        return false;
                    }

                })
            });
             
        }


function bindlevel3() {
            jQuery(".level3").unbind('click.level');
            jQuery(".level3").bind('click.level', function () {
               
                jQuery(this).closest("tr").nextAll("tr").each(function () {

                    if (jQuery(this).attr("id") == undefined) {
                        jQuery(this).toggle()
                    } else {
                        return false;
                    }

                })
            })

        }
function bindcolors() {
            jQuery("[data-group-level=0]").css({
                'background-color': '#6775A1',
                'color': 'white',
                'cursor': 'pointer'
            })
            jQuery("[data-group-level=1]").css({
                'background-color': '#939AB0',
                'color': 'white',
                'cursor': 'pointer'
            })
            jQuery(".level3 td").css({
                'background-color': '#B0B6C8',
                'color': 'white',
                'cursor': 'pointer'
            })
        }
function bindgroup() {

            jQuery(".subgroup").unbind('click.kk');
            jQuery(".subgroup").bind('click.kk', function () {

               
                var subgroup = jQuery(this).closest("tr").next("tr").attr("data-group")
                if (subgroup != undefined) {

                    var subtr = jQuery('[data-group=' + subgroup + ']')
                    subtr = jQuery(subtr).filter("tr")
                    if (jQuery(this).hasClass("expanded-group")) {

                        jQuery(subtr).each(
                          function () {

                              jQuery(this).hide();
                          }
                        )
                    } else {
                        var thead = jQuery('#mailtablex thead tr:first');
                        var tablenew = jQuery("<table/>").append(jQuery("<thead/>").append((thead.clone())))
                        jQuery(tablenew).append(jQuery("<tbody/>").append(subtr.clone()))
                        //$(tablenew).find("tr").each(function(){ $(this).removeAttr("data-group") })
                        try {
                            jQuery(tablenew).dataTable();

                        } catch (ex) { console.log(ex) }
                        try {
                            if (yearthis != "") {
                                jQuery(tablenew).dataTable().rowGrouping({ iGroupingColumnIndex: colum, sGroupBy: "year", fnGroupLabelFormat: function (label) { return "           +" + label + "  (Por " + labelthis + " )"; }, bHideGroupingColumn: false });

                            } else {
                                
                                jQuery(tablenew).dataTable().rowGrouping({ iGroupingColumnIndex: colum, fnGroupLabelFormat: function (label) { return "           +" + label + " (Por " + labelthis + " )"; }, bHideGroupingColumn: false });
                            }
                        } catch (ex) { console.log(ex) }
                        try{
                            jQuery(tablenew).find("tbody tr").each(function () {

                               

                                if (jQuery(this).attr("id") != undefined) {
                                    jQuery(this).addClass("level3")
                                }


                            })
                        } catch (ex) { }
                        setTimeout(function () { bindlevel3();},500);
                        subtr.remove()
                        jQuery(this).closest("tr").after(jQuery(tablenew).find("tbody tr"))
                    }
                } else {
                    
                    jQuery(this).closest("tr").nextAll("tr").each(
                    function () {

                        if (jQuery(this).find("td").hasClass("group") == true && jQuery(this).find("td").data("group-level")!=0) {
                           /* if (show) {
                                $(this).show();
                            } else {
                                $(this).hide();
                            }*/
                            jQuery(this).nextAll("tr").each(function () {
                              
                                if (jQuery(this).attr("id") == undefined) {
                                    jQuery(this).hide()
                                } else {
                                    return false;
                                }
                            });
                            jQuery(this).toggle();

                        }
                        if (jQuery(this).closest("tr").filter("[id*=group-id-mailtablex_]").length > 0) {

                            return false;
                        }
                    }

                    )


                }

            });
             jQuery(".subgroup").trigger("click");

         
        }
function toggleElements(option) {
            if (option) {
                try{
                    jQuery("#mailtablex_length select").val("10").trigger("change");
                    jQuery("#mailtablex_length").show();
                    jQuery("#mailtablex_filter").show();
                    jQuery("#mailtablex thead tr:last").show();
                    jQuery("#mailtablex_paginate").show();
                    jQuery("#mailtablex").closest("div").css({ "top": "50px" })
                }catch(ex){ }
            } else {
                try{
                jQuery("#mailtablex_length select").val("-1").trigger("change");
                jQuery("#mailtablex_length").hide();
                jQuery("#mailtablex_filter").hide();
                jQuery("#mailtablex thead tr:last").hide();
                jQuery("#mailtablex_paginate").hide();
                jQuery("#mailtablex").closest("div").css({ "top": "-50px" })
                } catch (ex) { }
            }
        }
function bindDropdowns(){

        jQuery('#firstFilter select').unbind("change.first");
        jQuery('#firstFilter select').bind("change.first", function () {

            jQuery('#secondFilter select option').attr("disabled", false);
            jQuery('#thridFilter select option').attr("disabled", false);
            jQuery('#secondFilter select').val("99").trigger("change");
            jQuery('#thridFilter select').val("99").trigger("change");
            jQuery(".subgroup").unbind('click.kk');
            if (jQuery(this).val() != 99) {
                
                jQuery('#secondFilter select').find("[value=" + jQuery(this).val() + "]").attr("disabled", true)
                jQuery('#thridFilter select').find("[value=" + jQuery(this).val() + "]").attr("disabled", true)

                jQuery('#secondFilter').show();
                jQuery('#thridFilter').hide();
            } else {
                jQuery('#secondFilter').hide();
                jQuery('#thridFilter').hide();
            }



        });
        jQuery('#secondFilter select').unbind("change.second");
        jQuery('#secondFilter select').bind("change.second", function () {

            jQuery('#thridFilter select option').attr("disabled", false);
           
            jQuery('#thridFilter select').val("99");
            jQuery('#thridFilter select').trigger("change")
            if (jQuery(this).val() != 99) {

                jQuery('#thridFilter select').find("[value=" + jQuery(this).val() + "]").attr("disabled", true)
                jQuery('#thridFilter select').find("[value=" + jQuery('#firstFilter select').val() + "]").attr("disabled", true)

                jQuery('#thridFilter').show();
            } else {
               
                jQuery('#thridFilter').hide();
            }



        });
       
        jQuery('#group').unbind("click.group");
        jQuery('#group').bind("click.group", function () {
            _loading();

            try{
            

            }catch(ex){ 
               console.log(ex);
             }
            var first = jQuery('#firstFilter select').val();
            var second = jQuery('#secondFilter select').val()
            var thrid = jQuery('#thridFilter select').val()
            jQuery(".subgroup").unbind('click.subgroup');
            debugger;
            if (first != 99) {

                var labelfirst = jQuery('#mailtablex thead tr:first th:eq(' + first + ')').text()
                var param1 = "iGroupingColumnIndex:" + first + ",bExpandableGrouping: true,bHideGroupingColumn: false,fnGroupLabelFormat: function(label) { return ' +'+ label + '  (Por " + labelfirst + ")'; }, ";
                var param2 = "";
                var year = (jQuery('#year1').is(":checked")) ? "sGroupBy: 'year'," : "";
                if (second != 99) {
                    year = (jQuery('#year2').is(":checked")) ? "sGroupBy: 'year'," : year;
                    var labelsecond = jQuery('#mailtablex thead tr:first th:eq(' + second + ')').text()

                    param2 = "iGroupingColumnIndex2:" + second + ",bExpandableGrouping2: true,bHideGroupingColumn2: false,fnGroupLabelFormat2: function(label) { return '     +'+ label + '  ( Por " + labelsecond + ")'; }, ";
                     if (thrid != 99) {
                         yearthis = (jQuery('#year3').is(":checked")) ? "sGroupBy: 'year'," : "";
                         var labelthrid = jQuery('#mailtablex thead tr:first th:eq(' + thrid + ')').text()
                         colum=thrid;
                          
                         labelthis = labelthrid;
                         setTimeout(function () { bindgroup() }, 2000);
                         setTimeout(function () { bindlevel0() }, 2000);
                         
                    } else {
                      
                    }

                    
                }  

                var script = "jQuery('#mailtablex').dataTable().rowGrouping({" +
                param1 + param2 + year +
                "bExpandSingleGroup: false,iExpandGroupOffset: -1,asExpandedGroups: ['']})"

                try{
                    
                    toggleElements(0);
                    
                }catch(ex){ }
                try {
                    
                    jQuery.globalEval(script);
                    var thead=jQuery("#mailtablex thead tr:first").clone()
                    jQuery("#mailtablex thead tr:first").hide()
                    jQuery("#mailtablex thead tr:first").before(jQuery(thead))

                } catch (ex) {
                    console.log(ex.toString())
                }
                setTimeout(function () { bindcolors() }, 500);
               if(typeof(callback)=="function"){

                   try{
                      callback();
                    }catch(ex){ 
                    }

                     }
            } else {

                alert("elija una Columna")
            }
            setTimeout(function(){  _loading(); },1000)
        })

         
    jQuery("#ungroup").unbind('click.un');
    jQuery("#ungroup").bind('click.un', function () {
        debugger;
        try{
            jQuery(".fixedHeader").remove()
        }catch(ex){ }
        if(tableinit!=""){

                try{
                    
                

                 //   $("#mailtablex").html("")
                  //  $("#mailtablex").html($(tableinit));
                    jQuery("#mailtablex_wrapper").remove();
                    jQuery("#reports").find("div:first").after(jQuery(tableinit).clone());
                     var th = jQuery('#mailtablex thead tr').clone()
                    // jQuery('#mailtablex thead').append(th)
                    var colindex=jQuery("#mailtablex thead tr:first th").length-1;
                    var columnsi="";
            for(var i=0;i<colindex;i++)
              {
               columnsi+= "{ type: 'text' },"
                             
              }
              columnsi+= "{ type: 'text' }"
                               
        var evaltable="jQuery(\"#mailtablex\").dataTable({"+
                "\"sPaginationType\": \"full_numbers\","+
                "\"sDom\": \"<'tableHeader'<l><'clearfix'f>r>t<'tableFooter'<i><'clearfix'p>>\","+
               " \"iDisplayLength\": 10,"+
               " \"bSortable\": false," +
               "\"sScrollY\":\"250px\","+
                "aLengthMenu: ["+
               " [10, 50, 100, -1],"+
               " [10, 50, 100, \"Todos\"]"+
               " ],"+
               " \"oLanguage\": {"+
                "    \"sLengthMenu\": \"Mostrar _MENU_ registros\","+
                 "   \"sInfo\": \"Mostrando del _START_ al _END_ de _TOTAL_ registros\","+
                 "   \"sSearch\": \"Filtro\","+
                 "   \"oPaginate\": {"+
                  "      \"sFirst\": \"Primero\","+
                  "      \"sLast\": \"Ultimo\","+
                  "      \"sNext\": \"Siguiente\","+
                   "    \"sPrevious\": \"Anterior\""+
                  "   },"+
                  "  \"sEmptyTable\": \"Tabla Sin Datos\""+
                "}"+
                 "})"
                 /*".columnFilter({"+
                  " sPlaceHolder: \"head:after\","+
                  "  aoColumns: ["+
                  " "+ columnsi+" "+
                   "        ]"+
                " });"*/;
                if(typeof(callback)=="function"){
                    try{
                    
                      callback();
                     }catch(ex){

                     } 

                     }
                    
                try {
                    
                    jQuery.globalEval(evaltable);
                    jQuery('#mailtablex input').css("width", "100%")
                    headers1();
                  /*  setTimeout(function () {
                        var table = jQuery('#mailtablex').DataTable();
                        new jQuery.fn.dataTable.FixedHeader(table);
                    }, 2000);*/
                } catch (ex) {
                    console.log(ex.toString())
                }
                    
                }catch(ex){


                }
             }

            });
      }