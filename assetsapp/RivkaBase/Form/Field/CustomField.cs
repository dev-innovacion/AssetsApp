using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rivka.Db.MongoDb;

namespace Rivka.Form.Field
{

    /// <summary>
    ///     This class helps to generate a field's html code
    /// </summary>
    public class CustomField
    {

        /// <summary>
        ///     This functions makes the field html string from the array [ _id, position, size, required ]
        /// </summary>
        /// <param name="fieldArray">
        ///     An array in the form [ _id, position, size, required ]
        ///     Where
        ///         _id: is the field's document id in the db,
        ///         position: is 1 for left or 2 for right,
        ///         size: is 1 for half size(50%) or 2 for full size(100%),
        ///         required: is 0 for false or 1 for true
        /// </param>
        /// <param name="owner">
        ///     The collections's field name
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns the HTML div view from the specified custom field
        /// </returns>
        public static String getFieldView(dynamic fieldArray, String collectionName)
        {
            string documentId = fieldArray["fieldID"];
            CustomFieldsTable fieldTable;
            JObject customField;

            try //try to get the field json document if owner and documentId are valid, else fieldView is null
            {
                fieldTable = new CustomFieldsTable(collectionName);
                String customFieldString = fieldTable.GetRow(documentId);
                customField = JsonConvert.DeserializeObject<JObject>(customFieldString);
            }
            catch (Exception e) { return null;  }

            if (customField == null) return null; //if customfield id does not exists

            String result = "<div";

            try //trying to assign the class of the customfield view
            {
                result += " class='";
                result += (fieldArray["position"] == 1) ? " fl" : " fr";
                result += (fieldArray["size"] == 1) ? " mediumSize" : " fullSize";
                result += "'>"; 
            }
            catch (Exception e) { /*Ignored*/ }

            try //trying to draw the label if it exists
            {
                result += "<label>" + customField["label"].ToString() + ": </label><br/>";
            }
            catch (Exception e) { /*Ignored*/ }

            String interText = "";

            //CustomField's specific elements
            switch ( customField["type"].ToString() ){
                case "TextField":
                    result += "<input class='customInput2' type='text'";
                    try //trying to set the patter to the field text
                    {
                        result += " pattern='" + customField["regularExp"].ToString() + "'";
                    }
                    catch (Exception e) { /*Ignored*/ }
                    try //trying to set the placeholder to the field text
                    {
                        result += " placeholder='" + customField["placeHolder"].ToString() + "'";
                    }
                    catch (Exception e) { /*Ignored*/ }
                    //try //trying to set the maxlenght to the field text
                    //{
                    //    result += " maxlength='" + customField["maxSize"].ToString() + "'";    
                    //}catch(Exception e){ /*Ignored*/ }
                    break;
                case "CheckBoxField":
                    result += "<input class='customInput2' type='checkbox'";
                    try //trying to set the value name to the check box field
                    {
                        result += " value='" + customField["name"].ToString() + "'";
                    }
                    catch (Exception e) { /*Ignored*/ }
                    try //trying to set the checked attribute to those which are pre-selected
                    {
                        result += (customField["check"].ToString() == "true" ? " CHECKED" : "");
                    }
                    catch (Exception e) { /*Ignored*/ }
                    break;
                case "CurrencyField":
                    try{ //trying to draw the currency symbol before the field
                        result += "<label>" + customField["currencySymbol"].ToString() + "</label>";
                    }
                    catch (Exception e) { /*Ignored*/ }
                    result += " <input class='customInput2' type='number' size='4' step='0.01'";
                    try //trying to set the field default value
                    {
                        result += " value='" + customField["defaultValue"].ToString() + "'";
                    }
                    catch (Exception e) { /*Ignored*/ }
                    break;
                case "DateField":
                    result += " <input class='customInput2' type='date'";
                    try //trying to set the field default value
                    {
                        if( customField["defaultValue"].ToString() == "true" )
                            result += " value='"+ DateTime.Today.ToString("yyyy-MM-dd") + "'";
                    }
                    catch (Exception e) { /*Ignored*/ }
                    break;
                case "DateTimeField":
                    result += " <input class='customInput2' type='dateTime-local'";
                    try //trying to set the field default value
                    {
                        string defaultValue = "";
                        if (customField["defaultDate"].ToString() == "true")
                            defaultValue += DateTime.Today.ToString("yyyy-MM-dd");
                        if (customField["defaultHour"].ToString() == "true")
                            defaultValue += "T" + DateTime.Now.ToString("HH:mm:ss");
                        result += " value='" + defaultValue + "'";
                    }
                    catch (Exception e) { /*Ignored*/ }
                    break;
                case "TextAreaField":
                    result += " <textarea class='customInput2'";
                    try //trying to draw the placeholder
                    {
                        result += " placeholder='" + customField["placeHolder"].ToString() + "'";
                    }
                    catch (Exception e) { /*Ignored*/ }
                    break;
                case "DecimalField":
                    result += " <input class='customInput2' type='number'";
                    try //trying to set the field default value
                    {
                        result += " value='" + customField["defaultValue"].ToString() + "'";
                    }
                    catch (Exception e) { /*Ignored*/ }
                    try //trying to set the field max value
                    {
                        result += " max='" + customField["maxSize"].ToString() + "'";
                    }
                    catch (Exception e) { /*Ignored*/ }
                    try //trying to set the field default value
                    {
                        result += " step='" + (1d / Math.Pow(10, ((Double)customField["precision"] + 1))).ToString().Replace(",", ".") + "'";
                    }
                    catch (Exception e) { /*Ignored*/ }
                    break;
                case "ImageField":
                case "FileField":
                    result += "<img id='_HTKField" + customField["name"].ToString() + "Preview' style='width:100px; height: 100px;'/>";
                    result += "<input class='customInput2' type='file'";
                    break;
                case "MultiSelectField":
                case "DropDownField":
                    result += "<select";
                    if (customField["type"].ToString() == "MultiSelectField")
                    {
                        result += " multiple";
                    }
                    
                    MongoModel listModel = new MongoModel("Lists");
                    String listString = listModel.GetRow(customField["listId"].ToString());
                    JObject list = (JObject)JsonConvert.DeserializeObject<JObject>(listString)["elements"];
                    foreach(JObject element in (JArray)list["order"]){
                        foreach (KeyValuePair<String,JToken> token in element)
                        {
                            interText += "<option value='" + token.Key + "'> " + token.Value + "</option>\n";
                            break;
                        }
                    }
                    interText += "<option disabled='disabled'>---------------------</option>\n";
                    foreach (JObject element in (JArray)list["unorder"])
                    {
                        foreach (KeyValuePair<String, JToken> token in element)
                        {
                            interText += "<option value='" + token.Key + "'> " + token.Value + "</option>\n";
                            break;
                        }
                    }
                    break;
                case "RadioField":
                    MongoModel listModelRadio = new MongoModel("Lists");
                    String listStringRadio = listModelRadio.GetRow(customField["listId"].ToString());
                    JObject listRadio = (JObject)JsonConvert.DeserializeObject<JObject>(listStringRadio)["elements"];
                    foreach(JObject element in (JArray)listRadio["order"]){
                        foreach (KeyValuePair<String,JToken> token in element)
                        {
                            interText += "<input name='_HTKField" + customField["name"].ToString() + "' id='_HTKField" + customField["name"].ToString() + "' type='radio' value='" + token.Key + "'/>";
                            interText += "<label for = '" + token.Key + "'>" + token.Value + "</label>\n<br/>";
                            break;
                        }
                    }
                    foreach (JObject element in (JArray)listRadio["unorder"])
                    {
                        foreach (KeyValuePair<String, JToken> token in element)
                        {
                            interText += "<input name='_HTKField" + customField["name"].ToString() + "' id='_HTKField" + customField["name"].ToString() + "' type='radio' value='" + token.Key + "'/>";
                            interText += "<label for = '" + token.Key + "'>" + token.Value + "</label>\n<br/>";
                            break;
                        }
                    }
                    break;
                case "URLField":
                    result += "<input type='url'";
                    break;
                case "IntegerField":
                    result += "<input type='number'";
                    break;
                case "PhoneField":
                    result += "<input type='tel'";
                    break;
                default:
                    return null;
            }

            if (customField["type"].ToString() != "RadioField")
            {
                List<String> hasTooltip = new List<string>() { 
                    "TextField", 
                    "CurrencyField",
                    "DateField",
                    "DateTimeField",
                    "TextAreaField",
                    "DecimalField",
                    "IntegerField",
                    "PhoneField",
                    "URLField"};
                if (hasTooltip.Contains(customField["type"].ToString()))
                {
                    try //trying to draw the tooltip
                    {

                        result += " data-original-title='" + customField["toolTip"].ToString() + "' rel='tooltip' data-placement='right'";
                    }
                    catch (Exception e) { /*Ignored*/ }
                }
                try
                { //trying to draw the field name if it can't be set the view must not be generated(name is required for javascript's actions)
                    result += " name='_HTKField" + customField["name"].ToString() + "'";
                }
                catch (Exception e) { return null; }
                try //trying to draw the field id, if it can't be set, the view must not be genarated(Id is required for javascript's actions)
                {
                    result += " id='_HTKField" + customField["name"].ToString() + "'";
                }
                catch (Exception e) { return null; }
                try
                {
                    if (fieldArray["required"] == 1) //is this field required?
                        result += " required";
                }
                catch (Exception e) { /*Ignored*/ }
                result += ">";
            }
            result += interText;
            if (customField["type"].ToString() == "TextAreaField") //if it's a textarea close the tag
            {
                result += "</textarea>";
            }
            else if (customField["type"].ToString() == "DropDownField" || customField["type"].ToString() == "MultiSelectField")
            {
                result += "</select>";
            }
            result += "</div>"; //end of the field's div

            return result;
        }

    }
}