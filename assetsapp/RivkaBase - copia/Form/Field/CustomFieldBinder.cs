using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rivka.Form.Field
{
    public class CustomFieldBinder : IModelBinder
    {

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            HttpRequestBase request = controllerContext.HttpContext.Request;
            string type = (request.Form.Get("type").Split(',')).First();
            var model = Activator.CreateInstance(Type.GetType("Rivka.Form.Field." + type));
            var props = model.GetType().GetProperties();
            foreach (var property in props)
            {
                //TODO:Por el momento Ignora el el bindeo de la lista
                if (property.GetValue(model, null) is IList) {
                    continue;
                }
                if (property.Name == "listId" && (type == "MultiSelectField" || type == "DropDownField" || type == "RadioField"))
                {
                    object readvalue = request.Form.Get("CustomList");
                    property.SetValue(model, Convert.ChangeType(readvalue, property.PropertyType), null);
                    continue;
                }
                if (property.PropertyType == typeof(bool))
                {
                    var readvalue = request.Form.Get(property.Name);
                    bool value = readvalue.Contains("true");
                    property.SetValue(model, Convert.ChangeType(value, property.PropertyType), null);
                    continue;
                }
                else
                {
                    object readvalue = request.Form.Get(property.Name);
                    property.SetValue(model, Convert.ChangeType(readvalue, property.PropertyType), null);
                }
            }
            return model;
        }
    }
}