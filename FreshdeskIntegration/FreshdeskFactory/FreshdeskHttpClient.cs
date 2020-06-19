using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FreshdeskIntegration.FreshdeskFactory
{
    public class FreshdeskHttpClient
    {
        public static HttpClient GetClient(string requestedVersion = null)
        {

            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri("https://interswitch.freshdesk.com/");

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));


            return client;
        }

        public static HttpRequestMessage GetRequestMessage(HttpMethod httpMethod, string requestUri)
        {

            var appSettings = ConfigurationManager.AppSettings;
            string apiKey = appSettings["apikey"];

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(httpMethod, requestUri);

            string authInfo = apiKey + ":X"; // It could be your username:password also.
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", authInfo);

            return httpRequestMessage;
        }

        public static HttpWebRequest GetRequestMessage(string httpMethod, string requestUri)
        {

            var appSettings = ConfigurationManager.AppSettings;
            string apiKey = appSettings["apikey"];

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://interswitch.freshdesk.com" + requestUri);
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.Method = httpMethod;
            string authInfo = apiKey + ":X"; // It could be your username:password also.
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            request.Headers["Authorization"] = "Basic " + authInfo;

            return request;
        }
    }
}
