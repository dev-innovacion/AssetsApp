using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Net;

using Rivka.Form;
using Rivka.Form.Field;

namespace Rivka.Form
{

    /// <summary>
    ///     This class provides helper functions to manipulate the bd's forms
    /// </summary>
    public class CustomForm
    {

        /// <summary>
        ///     This method creates the form's html structure
        /// </summary>
        /// <param name="customString">
        ///     The json's customFields field of the json ["tab1":[[leftFields],[rightFields]], "tab2":[[leftFields],[rightFields]], ... , "tabN":[[leftFields],[rightFields]]]
        /// </param>
        /// <param name="owner">
        ///     The collection's name
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns the HTML view for the form's custom fields
        /// </returns>
        public static String getFormView( String customString, String collectionName ){
            String customFieldString = "{'customFields':" + customString + "}"; //the customString is not a valid json, so we have to complete it to validate
            dynamic fieldsObject;
            try //checking if customString is valid
            {
                fieldsObject = Json.Decode(customFieldString);
            }
            catch ( Exception ){
                throw new FormatException( "The string is not a json." );
            }

            String response = ""; //response contains the form html code

            foreach ( dynamic tab in fieldsObject["customFields"] ) //for each tab in the customString 
            {
                response += "<div class='tab-pane perfilMargen' id='HTKTab" + tab["tabName"].Replace(" ","") + "'>"; //the id of the tab is the tab's name without spaces

                List<DynamicJsonObject> left = new List<DynamicJsonObject>();
                List<DynamicJsonObject> right = new List<DynamicJsonObject>();
                foreach( dynamic field in tab["fields"]){
                    if (field["position"] == 1)
                    {
                        left.Add(field);
                    }
                    else {
                        right.Add(field);
                    }
                }

                int leftSize = left.Count();
                int rightSize = right.Count();
                DynamicJsonObject currentField = null;

                //print fields by rows, first left then right, the sides coould have diferrent sizes
                for (int leftCounter = 0, rightCounter = 0, side = 0; leftCounter < leftSize || rightCounter < rightSize; )
                {

                    //it's left's fields turn
                    if (side == 0)
                    {
                        if (leftCounter < leftSize)   //if there are fields remaining in the left side then get its view
                        { 
                            currentField = left.First();
                            left.RemoveAt(0);
                            response += CustomField.getFieldView(currentField, collectionName);
                        }
                        if (rightCounter <rightSize) //if there are fields remaining in the right side then print it in the next iteration
                            side = 1;
                        ++leftCounter;
                    }
                    //it's right's fields turn
                    else
                    {
                        if (rightCounter < rightSize) //if there are fields remaining in the right side then get its view
                        { 
                            currentField = right.First();
                            right.RemoveAt(0);
                            response += CustomField.getFieldView(currentField, collectionName);
                        }
                        if (leftCounter < leftSize) //if there are fields remaining in the left side then print it in the next iteration
                            side = 0;
                        rightCounter++;
                    }
                }
                
                response += "</div>";
            }

            return response;
        }


        /// <summary>
        ///     This method creates the tabs headers 
        /// </summary>
        /// <param name="customString">
        ///     The json's customFields field of the json ["tab1":[[leftFields],[rightFields]], "tab2":[[leftFields],[rightFields]], ... , "tabN":[[leftFields],[rightFields]]]
        /// </param>
        /// <param name="owner">
        ///      The collection's name
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     The custom field's tabs titles, with the required attribute   
        /// </returns>
        public static String getFormTitlesView(String customString)
        {
            String customFieldString = "{'customFields':" + customString + "}"; //customString is not a valid document, so we have to complete it before testing
            dynamic fieldsObject;
            try //is the customString valid?
            {
                fieldsObject = Json.Decode(customFieldString);
            }
            catch (Exception)
            {
                throw new FormatException("The string is not a json.");
            }

            String response = ""; //the response contains the header html code
            foreach( dynamic tab in fieldsObject["customFields"] ){
                response += "<li><a href='#HTKTab" + tab["tabName"].Replace(" ", "") + "' data-toggle='tab'>" + tab["tabName"] + "</a></li>\n"; //setting the id from the tab's name without spaces
            }

            return response;
        }

        /// <summary>
        ///     Unserialize a formCollection 
        /// </summary>
        /// <param name="formCollection">
        ///     The form collection sended in the form 'var1=1&var2=2...varn=n'
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     An array in the form array['var1'] = 1, array['var2'] = 2, ...., array['varn'] = n
        /// </returns>
        public static FormCollection unserialize(FormCollection formCollection)
        {
            string data = Convert.ToString(formCollection["data"]);
            string[] formCollectionString = data.Split('&', '='); //geting an array from the String with delimiters & adn =
            FormCollection result = new FormCollection();  //the result contains the array to return
            for (int key = 0, value = 1; value < formCollectionString.Length; key+=2, value+=2) { //getting key and value for each loop
                if (result[formCollectionString[key]] != null)
                {
                    result[formCollectionString[key]] += "," + HttpUtility.UrlDecode(formCollectionString[value]);
                    continue;
                }
                result[formCollectionString[key]] = HttpUtility.UrlDecode( formCollectionString[value] );
            }
            return result;
        }

        /// <summary>
        ///     Unserialize a json string 
        /// </summary>
        /// <param name="data">
        ///     A string serialized
        /// </param>
        /// <author>
        ///     Juan Bautista
        /// </author>
        /// <returns>
        ///     A dictionary with the key as string and value
        /// </returns>
        public static Dictionary<string,string> unserialize(string data)
        {
            string[] formCollectionString = data.Split('&', '='); //geting an array from the String with delimiters & adn =
            Dictionary<string,string> result = new Dictionary<string,string>();
            for (int key = 0, value = 1; value < formCollectionString.Length; key += 2, value += 2)
            { //getting key and value for each loop
                if (result.ContainsKey(formCollectionString[key]))
                {
                    result[formCollectionString[key]] += "," + HttpUtility.UrlDecode(formCollectionString[value]);
                    continue;
                }
                result[formCollectionString[key]] = HttpUtility.UrlDecode(formCollectionString[value]);
            }
            return result;
        }
    }
}