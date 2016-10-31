using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using MongoDB.Bson;
using MongoDB.Driver;

using RivkaAreas.ObjectReference.Models;

namespace RivkaAreas.ObjectReference.Controllers
{
    public class CategoryController : Controller
    {
        //
        // GET: /ObjectReference/Category/

        protected CategoryTable categoryTable;

        public CategoryController()
        {
            categoryTable = new CategoryTable();
        }

        public ActionResult Index()
        {
            return this.Redirect("~/ObjectReference");
        }

        /// <summary>
        ///     This method allows to get all the categories
        /// </summary>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     
        /// </returns>
        public ActionResult getCategories()
        {
            if (!this.Request.IsAjaxRequest())
            {
                //return View(); 
            }
            return this.Redirect("~/ObjectReference");
        }

        /// <summary>
        ///     Get a category specified by the idCategory
        /// </summary>
        /// <param name="idCategory"></param>
        /// <returns>
        ///     Returns a Json text with the found category
        /// </returns>
        [HttpPost]
        public string getCategory(string idCategory)
        {
            String categoryString = categoryTable.getRow(idCategory);
            if (categoryString != null)
            {
                return categoryString;
            }
            return "";
        }

        /// <summary>
        ///     This method allows to get the document's childs by id
        /// </summary>
        /// <param name="parentCategory">
        ///     The category's id that we want to find its children
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns an array with the information needed to represent a tree
        /// </returns>
        public JsonResult getNodeContent(String parentCategory)
        {
            if (parentCategory == "") parentCategory = "null";
            String categoriesString = categoryTable.get("parentCategory", parentCategory);

            if (categoriesString == null) return null; //there are no subcategories

            JArray categoriesObject = JsonConvert.DeserializeObject<JArray>(categoriesString);
            foreach (JObject category in categoriesObject)
            {
                try
                { //try to remove customFields, if can't be removed it doesn't care
                    category.Remove("customFields");
                }
                catch (Exception e) { /*Ignored*/ }

                try
                {
                    category.Remove("parentCategory");
                }
                catch (Exception e) { /*Ignored*/ }

                try
                {
                    category.Remove("CreatedDate");
                }
                catch (Exception e) { /*Ignored*/ }

                try
                {
                    category.Remove("LastmodDate");
                }
                catch (Exception e) { /*Ignored*/ }
            }
            categoriesString = JsonConvert.SerializeObject(categoriesObject);

            return Json(categoriesString);
        }

        public JsonResult getNodeContent2(String parentCategory)
        {
            if (parentCategory == "") parentCategory = "null";
            String categoriesString = categoryTable.Get("parentCategory", parentCategory);

            if (categoriesString == null) return null; //there are no subcategories

            JArray categoriesObject = JsonConvert.DeserializeObject<JArray>(categoriesString);
            

            JArray newobjs = new JArray();

            foreach (JObject obj in categoriesObject)
            {
                JObject obj1 = new JObject();
                obj1["id"] = obj["_id"];
                obj1["text"] = obj["name"];
                obj1["hasChildren"] = true;
                // obj1["items"] = new JArray();
                //  obj1["items"] = "[]";
                obj1["spriteCssClass"] = "objectimg";
                newobjs.Add(obj1);
            }

            return Json(JsonConvert.SerializeObject(newobjs), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        ///     This method allows to update the parent
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newparent"></param>
        /// <author>
        ///     Karina
        /// </author>
        public String updateParent(string id, string newparent)
        {
            if (newparent == "") newparent = "null";
            if (this.Request.IsAjaxRequest())
            {
                if (id != "" && id != null && id != "null")
                {
                    String categoryString = categoryTable.GetRow(id);
                    var newCategory = JsonConvert.DeserializeObject<JObject>(categoryString);
                    if (newparent == id)
                    {
                        return "No puede ser padre de sí mismo.";
                    }
                    newCategory["parentCategory"] = newparent;

                    categoryTable.SaveRow(JsonConvert.SerializeObject(newCategory), id);
                    return "success";
                }

            }

            return null;
        }

    }
}
