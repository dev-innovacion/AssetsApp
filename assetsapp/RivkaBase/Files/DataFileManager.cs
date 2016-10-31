using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rivka.Security;

namespace Rivka.Files
{

    /// <summary>
    ///     This class allows to manage a file that contains a data json.
    /// </summary>
    public class DataFileManager
    {
        private String fileSource; //the file path
        protected String dataString; //File's string
        private DataJson dataObject; //the string parsed into jobject
        private String password = null;
       
        /// <summary>
        ///     Allows to initialize the object from the gived path
        /// </summary>
        /// <param name="path">The file's path</param>
        /// <author>Quijada Romero Luis Gonzalo</author>
        /// 
        public DataFileManager( String path, String password = null ) {
            this.fileSource = path;
            this.password = password;

            try
            {
                String encrypted = System.IO.File.ReadAllText(path);
                if (password == null)
                {
                    this.dataString = encrypted;
                }
                else
                {
                    this.dataString = BiEncrypt.Decrypt(encrypted, password);
                }
            }
            catch (Exception e) {
                this.dataString = null;
            }

            try
            {
                this.dataObject = new DataJson(dataString);
            }
            catch (Exception e) {
                this.dataObject = null;
            }
        }

        /// <summary>
        ///     Allows to check if the file is empty or corrupted
        /// </summary>
        /// <author>
        ///     Quijada Romero Luis Gonzalo
        /// </author>
        /// <returns>
        ///     True or False
        /// </returns>
        public bool empty() {
            if (dataString == null) return true;
            return false;
        }

        /// <summary>
        ///     Allows to set the data for saving a new string into a file
        /// </summary>
        /// <param name="JsonData"></param>
        /// <author>Quijada Romero Luis Gonzalo</author>
        public void SetData(String JsonData) {
            try
            {
                dataString = JsonData;
                dataObject = new DataJson(JsonData);
            }
            catch (Exception e) {
                dataString = null;
                dataObject = null;
            }
        }

        public void Save() {
            dataString = dataObject.ToString();
            String encrypted;

            if (password == null)
            {
                encrypted = dataString;
            }
            else
            {
                encrypted = BiEncrypt.Encrypt(dataString, password);
            }
            String directory = fileSource.Substring(0, fileSource.LastIndexOf("/"));
            bool exists = System.IO.Directory.Exists(directory);
            if (!exists)
                System.IO.Directory.CreateDirectory(directory);// .Directory.CreateDirectory(fileSource);
            System.IO.File.WriteAllText(fileSource,encrypted);
        }

        /// <summary>
        ///     Allows to use index with this class
        /// </summary>
        /// <param name="key">The key to search for</param>
        /// <author>Luis Gonzalo Quijada Romero</author>
        /// <returns>A DataJson that allows to do indexing recursively</returns>
        public dynamic this[String key]{
            get {
                if (dataObject == null) 
                {
                    return null;
                }
                return this.dataObject[key];
            }

            set {
                if (dataObject != null)
                {
                    this.dataObject[key] = value;
                }
            }
        }
    }
}