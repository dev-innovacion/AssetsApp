//kendo.message 1.8.2. Copyright (c) 2013 John Rummell (jrummell.com). Licensed under the GPL license  (http://www.gnu.org/licenses/gpl.html). 

kendo_module({id:"message",name:"Message",category:"web",description:"The Message widget renders styled messages.",depends:["core"]});(function(jQuery)
{var methods={init:function(options)
{options=jQuery.extend({},jQuery.fn.kendoMessage.defaults,options);return this.each(function()
{var $this=jQuery(this);var data=$this.data("message");if(!data)
{var messageText=options.message;if(messageText==null||messageText=="")
{messageText=$this.html();}
var messageClass=options.type=="info"?"k-block k-info-colored":"k-block k-error-colored";var iconClass=options.type=="info"?"k-i-tick":"k-i-note";var messageHtml="<div class='k-content message-container'>";messageHtml+="<div class='"+messageClass+" ui-corner-all' >";messageHtml+="<p><span class='k-icon "+iconClass+"' style='float:left;'></span>";messageHtml+="<span class='message-text'>"+messageText+"</span>";if(options.dismiss)
{messageHtml+="<a class='message-dismiss' href='#'>Dismiss</a>";}
messageHtml+="</p></div></div>";$this.html(messageHtml);if(options.dismiss)
{jQuery(".message-dismiss",$this).click(function()
{$this.hide('normal');return false;});}
if(options.autoShow)
{$this.show();}
$this.data("message",options);}});},options:function(options)
{return this.each(function()
{var $this=jQuery(this);var currentOptions=$this.data("message")||{};options=jQuery.extend({},currentOptions,options);$this.kendoMessage("destroy").kendoMessage("init",options);});},show:function()
{jQuery(this).show();},hide:function()
{jQuery(this).hide();},destroy:function()
{return this.each(function()
{var $this=jQuery(this);var data=$this.data("message");jQuery(".message-container",$this).remove();$this.html(data.message).css("display:none");$this.removeData("message");});}};jQuery.fn.kendoMessage=function(method)
{if(methods[method])
{return methods[method].apply(this,Array.prototype.slice.call(arguments,1));}
else if(typeof(method)==='object'||!method)
{return methods.init.apply(this,arguments);}
else
{jQuery.error("Method "+method+" does not exist on jQuery.kendoMessage");}};jQuery.fn.kendoMessage.defaults={message:"",type:"info",dismiss:true,autoShow:true};})(jQuery);