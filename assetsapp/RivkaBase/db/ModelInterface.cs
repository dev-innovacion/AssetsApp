using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rivka.Db
{
    public interface ModelInterface
    {
        string GetRow(string id); //allows to get a document by its id in a json
        string GetRows(string fiel = null, string sortBy = null, int limit = 0); //allows to get the whole collection's documents in a json with true system_status
        string GetRowsAll(string fiel = null); //allows to get the whole collection's documents in a json 
        string GetRowsDeleted(string fiel = null); //allows to get the whole collection's documents in a json with false system_status
        string Get(string key, string value, string fiel = null); //allows to get all the documents where key = value and system_status = true
        string GetAll(string key, string value, string fiel = null); //allows to get all the documents where key = value
        string GetDeleted(string key, string value, string fiel = null); //allows to get all the documents where key = value and system_status = false
        string DeleteRow(string id, bool logic = false); // allows to delete a document by its id, returns success or error
        string SaveRow(string jsonData, string id); // allows to save or update a document, returns the saved document's id
        string UpdateRow(string field, string data, string id); // updates just the fields given
    }
}