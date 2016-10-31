using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rivka.Security;
using Rivka.Form;
using Rivka.Form.Field;

//using RivkaAreas.Customer.Models.CustomFieldObjects;
//using RivkaAreas.Customer.Classes;
using RivkaAreas.Movement.Models;

namespace RivkaAreas.Movement.Controllers
{
    [Authorize]
    public class CustomTypeController : Controller
    {
        //
        // GET: /CustomType/

        /// <summary>
        /// Index of customtype controller
        /// </summary>
        /// <returns>Index view of customtype</returns>
        /// <author>Galaviz Alejos Luis Angel</author>
        /// 
        protected validatePermissions validatepermissions = new validatePermissions();
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
        /// </summary>CustomFieldsTable("MovementFields")
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
            //Get the table based on his name
            CustomFieldsTable cft = new CustomFieldsTable("MovementFields");
            String list = cft.GetRows();
            JArray fieldsArray = JsonConvert.DeserializeObject<JArray>(list);

            //Sends hidden input to use on DeleteField and EditField Method
            ViewData["TableName"] = tablename;

            //Setea el nombre de la table name en la variable de sesión para el metodo guardar
            Session["tablename"] = tablename;

            //Regresa la tabla, con el argumento list, que representa sus rows
            return PartialView("Table", fieldsArray);

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
                  CustomFieldsTable cft = new CustomFieldsTable("MovementFields");
                  cft.deleteRows(id);
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
            CustomFieldsTable cft = new CustomFieldsTable("MovementFields");
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
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;

            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("custom_fields", "u", dataPermissions);
            accessClient = validatepermissions.getpermissions("custom_fields", "u", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                //Validate model
                if (TryValidateModel(fb))
                {
                    //Get table name
                    string tablename = (Session["tablename"].ToString());
                    //Save or updates field, depending 
                    var table = new CustomFieldsTable("MovementFields");
                    table.saveCustomField(fb);
                    return null;
                }
                else
                {
                    //In case of error, redraws the popup with error messages
                    return PartialView("PopUpForm", fb);
                }

            } return Redirect("~/Home");
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
                CustomFieldsTable cft = new CustomFieldsTable("MovementFields");
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
            return Redirect("~/Home");
        }

        public bool getpermissions(string permission, string type)
        {
            var datos = Session["Permissions"].ToString();

            JObject allp = JsonConvert.DeserializeObject<JObject>(datos);

            if (allp[permission]["grant"].Count() > 0)
            {
                foreach (string x in allp[permission]["grant"])
                {
                    if (x.Contains(type))
                    {
                        return true;
                    }
                }
            }

            return false;

        }

    }
}
