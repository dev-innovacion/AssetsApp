using RivkaAreas.User.Models;

using MongoDB.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rivka.Security;
using Rivka.Form.Field;

namespace RivkaAreas.User.Controllers
{
    [Authorize]
    public class CustomTypeController : Controller
    {
        //
        // GET: /CustomType/
        protected validatePermissions validatepermissions=new validatePermissions();
        /// <summary>
        /// Index of customtype controller
        /// </summary>
        /// <returns>Index view of customtype</returns>
        /// <author>Galaviz Alejos Luis Angel</author>
        public ActionResult Index()
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("custom_fields", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("custom_fields", "r", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                 //Set fields types for the combobox
                 List<SelectListItem> DataTypes = new List<SelectListItem>()
            {
                new SelectListItem(){Text="CheckBox", Value="CheckBoxField"},
                new SelectListItem(){Text="Campo calculado", Value="ComputedField"},
                new SelectListItem(){Text="Moneda", Value="CurrencyField"},
                new SelectListItem(){Text="Fecha", Value="DateField"},
                new SelectListItem(){Text="Fecha y Hora", Value="DateTimeField"},
                new SelectListItem(){Text="Decimal", Value="DecimalField"},
                new SelectListItem(){Text="Combo Box", Value="DropDownField"},
                new SelectListItem(){Text="Archivo", Value="FileField"},
                new SelectListItem(){Text="Imagen", Value="ImageField"},
                new SelectListItem(){Text="Entero", Value="IntegerField"},
                new SelectListItem(){Text="Multi opción", Value="MultiSelectField"},
                new SelectListItem(){Text="Teléfono", Value="PhoneField"},
                new SelectListItem(){Text="Radio", Value="RadioField"},
                new SelectListItem(){Text="Área de texto", Value="TextAreaField"},
                new SelectListItem(){Text="Campo de texto", Value="TextField"},
                new SelectListItem(){Text="URL", Value="URLField"}
            };

                 //Send to view
                 ViewData["Types"] = DataTypes;

                 return View();
             }
             else
             {

                 return Redirect("~/Home");
             }
        }

        /// <summary>
        /// Draws the controllers on the PopUp based on the fieldtabe of the DropDownList
        /// </summary>
        /// <param name="type">CustomField type selected on the dropdownlist</param>
        /// <returns>Partial view containing the needed fields</returns>
        public ActionResult GetForm(string type)
        {
            //Construct the model based on the type name
            FieldBase model = (FieldBase)Activator.CreateInstance(Type.GetType("Rivka.Form.Field." + type));
            
            //Sends the object to view
            return PartialView("PopUpForm", model);
        }


        /// <summary>
        /// Get table on based on the names of the table
        /// </summary>
        /// <param name="tablename">Table name</param>
        /// <returns>Table with his customFields</returns>
        /// <author>Galaviz Alejos Luis Angel</author>
        public ActionResult GetTable(string tablename)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("custom_fields", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("custom_fields", "r", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                //Get the table based on his name
                CustomFieldsTable cft = new CustomFieldsTable(tablename);
                String listArray = cft.GetRows();
                JArray list = JsonConvert.DeserializeObject<JArray>(listArray);

                //Sends hidden input to use on DeleteField and EditField Method
                ViewData["TableName"] = tablename;

                //Setea el nombre de la table name en la variable de sesión para el metodo guardar
                Session["tablename"] = tablename;

                //Regresa la tabla, con el argumento list, que representa sus rows
                return PartialView("Table", list);
            }
            else
            {

                return Redirect("~/Home");
            }

        }

        /// <summary>
        /// Deletes a customField
        /// </summary>
        /// <param name="id">CustomField Id</param>
        /// <param name="tablename">Name of the table were the customField belongs</param>
        /// <author>Galaviz Alejos Luis Angel</author>
        public void DeleteField(string id, string tablename)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("custom_fields", "d", dataPermissions);
            accessClient = validatepermissions.getpermissions("custom_fields", "d", dataPermissionsClient);

            if (access == true && accessClient == true)
            {

                 CustomFieldsTable cft = new CustomFieldsTable(tablename);
                 cft.deleteRows(id);
             }
        }

        /// <summary>
        ///     This method allows to delete several customfields from the db
        /// </summary>
        /// <param name="array">
        ///     It's an array of customs fields ids
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns a message string
        /// </returns>
        public String deleteFields(List<String> array, string tablename)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("custom_fields", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("custom_fields", "r", dataPermissionsClient);

            if (access == true && accessClient == true)
            {

                CustomFieldsTable cft = new CustomFieldsTable(tablename);
                if (this.Request.IsAjaxRequest()) //only available with AJAX
                {
                    try //tryign to delete the fields
                    {
                        if (array.Count == 0) return null; //if array is empty there are no fields to delete
                        foreach (String id in array) //froeach id in the array we must delete the document with that id from the db
                        {
                            cft.deleteRows(id);
                        }

                        return "Borrado";
                    }
                    catch (Exception e)
                    {
                        return null;
                    }
                }
                return null;
            }
            else
            {
                return null;

            }
        }

        /// <summary>
        /// Check if the customField name already exist on a table
        /// </summary>
        /// <param name="fieldname">Name of the field</param>
        /// <param name="tablename">Name of the table</param>
        /// <returns>True if exist, false if it doesnt</returns>
        public bool CheckIfCustomFieldExist(string fieldname, string tablename)
        {
            CustomFieldsTable cft = new CustomFieldsTable(tablename);
            return cft.CustomFieldExist(fieldname);
        }


        /// <summary>
        /// Saves or updates a CustomField depending on the ID (uses a generic View Popup by a custommodel binder)  
        /// </summary>
        /// <param name="fb">Custom Field as an object</param>
        /// <returns>PopUp if it has errors, close popup if it doesnt</returns>
        /// <auth>Galaviz Alejos Luis Angel</auth>
        public ActionResult SaveField([ModelBinder(typeof(CustomFieldBinder))] object fb)
        {
            //Validate model
              String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            bool edit=false;
            bool editClient=false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("custom_fields", "c", dataPermissions);
            edit = validatepermissions.getpermissions("custom_fields", "u", dataPermissions);
            
            accessClient = validatepermissions.getpermissions("custom_fields", "c", dataPermissionsClient);
            editClient = validatepermissions.getpermissions("custom_fields", "u", dataPermissionsClient);

         
            


            if ((access == true && accessClient==true) || (edit == true && editClient==true))
            {
                if (TryValidateModel(fb))
                {
                    //Get table name
                    string tablename = (Session["tablename"].ToString());
                    //Save or updates field, depending 
                    var table = new CustomFieldsTable(tablename);
                    table.saveCustomField(fb);
                    return null;
                }
                else
                {
                    //In case of error, redraws the popup with error messages
                    return PartialView("PopUpForm", fb);
                }
            }
            else
            {
                return Redirect("~/Home");
            }
        }

        /// <summary>
        /// Get CustomField by Id and tablename where it belongs to be updated trought the method SaveField
        /// </summary>
        /// <param name="id">Id of the customField</param>
        /// <param name="tablename">Name of the table where it belongs</param>
        /// <returns>PopUp with populated Fields</returns>
        public ActionResult EditField(string id, string tablename)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("custom_fields", "u", dataPermissions);
            accessClient = validatepermissions.getpermissions("custom_fields", "u", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                 CustomFieldsTable cft = new CustomFieldsTable(tablename);
                 String docString = cft.GetRow(id);
                 JObject doc = JsonConvert.DeserializeObject<JObject>(docString);

                 //Reflexing properties ----------------------------------------------------------------------
                 var model = Activator.CreateInstance(Type.GetType("Rivka.Form.Field." + doc["type"]));
                 if (doc["type"].ToString() == "MultiSelectField")
                 {

                     foreach (SelectListItem list in ((MultiSelectField)model).CustomList)
                     {
                         if (list.Value == doc["listId"].ToString())
                         {
                             list.Selected = true;
                         }
                         else
                         {
                             list.Selected = false;
                         }
                     }
                 }
                 var props = model.GetType().GetProperties();
                 foreach (KeyValuePair<string, JToken> element in doc)
                 {
                     var currentprop = (from property in props
                                        where property.Name == element.Key.ToString()
                                        select property).FirstOrDefault();
                     if (currentprop != null)
                     {
                         currentprop.SetValue(model, Convert.ChangeType(element.Value.ToString(), currentprop.PropertyType), null);
                     }
                 }
                 //------------------------------------------------------------------------------------------
                 return PartialView("PopUpForm", model);
             }
             else
             {
                 return Redirect("~/Home");
             }

        }
        public bool getpermissions(string permission, string type)
        {
            var datos = "";
            if (Session["Permissions"] != null)
            {
                datos = Session["Permissions"].ToString();
                JObject allp2 = JsonConvert.DeserializeObject<JObject>(datos);

                if (allp2[permission]["grant"].Count() > 0)
                {
                    foreach (string x in allp2[permission]["grant"])
                    {
                        if (x.Contains(type))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            else
            {

                this.Session["LoggedUser"] = null;
                this.Session["_id"] = null;

                //Unset auth cookie
                FormsAuthentication.SignOut();


                //Redirect to index
                Response.Redirect("~/User/Login");
                return false;
            }



        }


    }
}
