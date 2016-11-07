using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Rivka.Db;
using Rivka.Db.MongoDb;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RivkaAreas.Movement.Controllers
{

    [Authorize]
    public class AdjudicatingController : Controller
    {
        //
        // GET: /Movement/Adjudicating/

        private MongoModel listsModel;
        private MongoModel usersModel;
        private MongoModel adjudicatingModel;
        protected RivkaAreas.LogBook.Controllers.LogBookController _logTable;
        /// <summary>
        ///     Initializes the models
        /// </summary>
        public AdjudicatingController()
        {
            listsModel = new MongoModel("Lists");
            usersModel = new MongoModel("Users");
            adjudicatingModel = new MongoModel("Adjudicating");
            _logTable = new LogBook.Controllers.LogBookController();
        }

        /// <summary>
        ///     Allows to get the adjudicating index view
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            { //creating asssetsTypeOptions
                String assetsTypeString = listsModel.Get("name", "_HTKassetsType");
                JObject assetsType = JsonConvert.DeserializeObject<JArray>(assetsTypeString).First() as JObject;
                String assetsTypeOptions = "";
                foreach (JObject item in (JArray)assetsType["elements"]["order"])
                {
                    foreach (KeyValuePair<String, JToken> element in item)
                    {
                        assetsTypeOptions += "<option value='" + element.Key + "'>" + element.Value + "</option>";
                    }
                }

                assetsTypeOptions += "<option disabled>---------------------------</option>";

                foreach (JObject item in (JArray)assetsType["elements"]["unorder"])
                {
                    foreach (KeyValuePair<String, JToken> element in item)
                    {
                        assetsTypeOptions += "<option value='" + element.Key + "'>" + element.Value + "</option>";
                    }
                }
                ViewData["assetsTypeOptions"] = new HtmlString(assetsTypeOptions);
            }

            { //creating usersOptions
                String usersOptions = "";
                String usersString = usersModel.GetRows();
                JArray usersArray = JsonConvert.DeserializeObject<JArray>(usersString);
                foreach (JObject user in usersArray) {
                    usersOptions += "<option value='" + user["_id"].ToString() + "'>" + user["user"].ToString() + "</option>";
                }
                ViewData["usersOptions"] = new HtmlString( usersOptions );
            }
            return View();
        }

        /// <summary>
        ///     Allows to save a new adjudicating row
        /// </summary>
        /// <returns> Returns an Integer </returns>
        /// <author> Quijada Romero Luis Gonzalo</author>
        public String saveAdjudicating(String data) {
            if (this.Request.IsAjaxRequest())
            {
                try
                {
                    adjudicatingModel.SaveRow(data);
                    try
                    {
                        _logTable.SaveLog(Session["_id"].ToString(), "Dictaminadores", "Insert: Se ha guardado dictaminador.", "Adjudicating", DateTime.Now.ToString());
                    }
                    catch { }
                    return "1";
                }
                catch (Exception e) { 
                    //Ignored
                }
            }
            return "0";
        }

        /// <summary>
        ///     Allows to get the adjudicating table
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ActionResult getTable(String type) {
            if (this.Request.IsAjaxRequest()) {
                try
                {
                    String adjudicatingString = adjudicatingModel.Get("type.value", type);
                    JArray adjudicatingList = JsonConvert.DeserializeObject<JArray>(adjudicatingString);
                    ViewData["adjudicatingList"] = adjudicatingList;
                }
                catch (Exception e) { 
                    //ignored
                }

                return View();
            }
            return null;
        }

        /// <summary>
        ///     Allows to delete a configuration
        /// </summary>
        /// <param name="id">
        ///     The configuration's id to delete
        /// </param>
        /// <returns>
        /// 
        /// </returns>
        public String deleteConfiguration(String id) {
            if (this.Request.IsAjaxRequest()) {
                try
                {
                    adjudicatingModel.DeleteRow(id);
                    return "1";
                }
                catch (Exception e) { 
                    //ignored
                }
            }
            return "0";
        }
    }
}
