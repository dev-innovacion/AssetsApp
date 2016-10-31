using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Rivka.Form.Field
{
    public class MultiSelectField : FieldBase
    {
        [Display(Name = "Seleccione Lista: ")]
        public List<SelectListItem> CustomList { get; set; }

        [HiddenInput(DisplayValue = false)]
        public String listId {get;set;}

        /// <summary>
        ///     This method initializes the list
        /// </summary>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        public MultiSelectField()
        {
            CustomList = new List<SelectListItem>();
            Rivka.Db.MongoDb.MongoModel model = new Rivka.Db.MongoDb.MongoModel("Lists");
            String listList = model.GetRows();
            JArray lists = JsonConvert.DeserializeObject<JArray>(listList);
            foreach (JObject list in lists) {
                CustomList.Add(new SelectListItem() { Text = list["name"].ToString(), Value = list["_id"].ToString(), Selected = lists.First == list ?  true : false });
            }
        }

    }
}