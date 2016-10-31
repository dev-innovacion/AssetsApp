using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;

//My comment
namespace Rivka.Api
{
    public class Agent
    {

        public String agentUrl; //The url where the agent is
        public String userName; //The user to authenticate 
        public String password; //The password to authenticate

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="url"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        public Agent(String url, String user = null, String password = null) {
            this.agentUrl = url;
        }

        /// <summary>
        ///     Allows to send a message to the api in the url set
        /// </summary>
        /// <param name="message">
        ///     The message to send, this is a json with the attributes action, collection and document
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Return the message returned by the api
        /// </returns>
        public String sendMessage( String message ) {
            if (agentUrl != null)
            {
                HttpWebResponse response = ApiRequest.getRequest("POST", "application/json", this.agentUrl, "\"" + message + "\"").GetResponse() as HttpWebResponse;
                String res = ApiRequest.unPackResponse(response);
                return res;
            }
            return "{\"status\":\"error\",\"message\":\"Not url was defined\"}";
        }


    }
}