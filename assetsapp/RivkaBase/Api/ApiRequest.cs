using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Rivka.Api
{
    public class ApiRequest
    {

        public static WebRequest getRequest(string method, string contentType, string endPoint, string content)
        {
            WebRequest request = ApiRequest.getRequest(method, contentType, endPoint);
            byte[] dataArray = Encoding.UTF8.GetBytes(content.ToString());
            request.ContentLength = dataArray.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(dataArray, 0, dataArray.Length);
            requestStream.Flush();
            requestStream.Close();
            return request;
        }

        public static WebRequest getRequest(string method, string contentType, string endPoint)
        {
            WebRequest request = WebRequest.Create(endPoint);
            request.Method = method;
            request.ContentType = contentType;
            return request;
        }

        //Get a WebRequest Asynchronously
        public static void getAsync(string method, string contentType, string endPoint, string content)
        {
            JObject request = new JObject();

            request.Add("method", method);
            request.Add("contentType", contentType);
            request.Add("endPoint", endPoint);
            request.Add("content", content);

            System.Threading.Thread task =
                new System.Threading.Thread(
                    new System.Threading.ParameterizedThreadStart(AsyncTask));

            task.Name = "HTTPAsync";
            task.Start(request);

            //Se revisa si ya terminó la otra tarea
            if (task.ThreadState != System.Threading.ThreadState.Stopped)
                System.Diagnostics.Debug.WriteLine("La otra tarea sigue corriendo");  
            else
                System.Diagnostics.Debug.WriteLine("La otra tarea termino");  
           
        }

        /// <summary>
        ///     Método que será llamado por un hilo 
        /// </summary>
        /// <param name="param"></param>
        static void AsyncTask(object request)
        {
            JObject newRequest = (JObject)request;
            getRequest(newRequest["method"].ToString(), newRequest["contentType"].ToString(), newRequest["endPoint"].ToString(), newRequest["content"].ToString());
            
            //Debugging the task
            System.Diagnostics.Debug.WriteLine("HTTPAsync " + Task.CurrentId + " its done");
        }

        public static string unPackResponse(WebResponse response)
        {
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            return reader.ReadToEnd();
        }

    }

}