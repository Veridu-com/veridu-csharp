using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Veridu
{
    class Request
    {
        private String clientId;
        private String version = "0.3";
        private Signature signature;
        private String sessionToken = null;
        private int lastCode = 0;
        private String lastError = null;
        private static String sdkVersion = "0.1.1-beta";
        private System.Net.HttpWebRequest connection;

        private String PerformRequest(String method, String url, String data)
        {
            
            if ((method.CompareTo("GET") == 0) && (!String.IsNullOrEmpty(data)))
            {
                if (url.Contains("?"))
                {
                    url = url + "&";
                }
                else
                {
                    url = url + "?";
                }
                url = url + data;
            }

            //Create connection
            connection = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
            connection.UserAgent = "Veridu-CSharp/" + sdkVersion;
            connection.Headers.Add("Veridu-Client", this.clientId);

            if (!String.IsNullOrEmpty(this.sessionToken))
            {
                connection.Headers.Add("Veridu-Session", this.sessionToken);
            }

            connection.Method = method;
            connection.Timeout = 10000;
            connection.ReadWriteTimeout = 10000;
            connection.ContentType = "application/x-www-form-urlencoded";

            //Send request
            if (!String.IsNullOrWhiteSpace(data) && !method.Contains("GET"))
            {
                UTF8Encoding encoding = new UTF8Encoding();
                byte[] bytes = encoding.GetBytes(data);

                connection.ContentLength = bytes.Length;

                using (System.IO.Stream dataStream = connection.GetRequestStream())
                {
                    dataStream.Write(bytes, 0, bytes.Length);
                }
            }
            else
            {
                connection.ContentLength = 0;
            }

            //Get Response
            string result = "";
            try
            {
                using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)connection.GetResponse())
                {
                    System.Net.HttpStatusCode status = response.StatusCode;
                    System.IO.Stream receiveStream = response.GetResponseStream();
                    System.IO.StreamReader sr = new System.IO.StreamReader(receiveStream);
                    result = sr.ReadToEnd().Trim();
                }
            }
            catch (Exception ex)
            {
                System.Net.WebException wex = (System.Net.WebException)ex;
                var s = wex.Response.GetResponseStream();
                result = "";
                int lastNum = 0;
                do
                {
                    lastNum = s.ReadByte();
                    result += (char)lastNum;
                } while (lastNum != -1);

                s.Close();
                s = null;
            }
            return result;
        }

        private static string GenerateNonce()
        {
            byte[] randomBytes = new byte[10];
            Random rand = new Random();
            rand.NextBytes(randomBytes);
            StringBuilder hex = new StringBuilder(randomBytes.Length * 2);

            foreach (byte b in randomBytes)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }

        public Request(String clientId, String secret, String version)
        {
            this.clientId = clientId;
            this.version = version;
            this.signature = new Signature(clientId, secret, version);
        }

        public JObject fetchSignedResource(String method, String resource, String data)
        {
            String payload, nonce, sign;
            JObject response;

            try
            {
                nonce = GenerateNonce();
                sign = this.signature.SignRequest(method, resource, nonce);
                

                if (!String.IsNullOrEmpty(data))
                    payload = sign + ((data[0] == '&') ? data : "&" + data);
                else
                    payload = sign;

                response = this.fetchResource(method, resource, payload);
                if (response == null)
                    return null;
                if ((bool)response["status"])
                {
                    if (nonce.CompareTo((String)response["nonce"]) != 0)
                    {
                        this.lastError = "Invalid nonce in response";
                        return null;
                    }
                    return response;
                }
                return null;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
                throw new Exception("Could not fetch resource at the moment.");
            }
        }

        public JObject fetchResource(String method, String resource, String data)
        {
            try
            {
                String url = "https://api.veridu.com/" + this.version;
                if (resource[0] != '/')
                {
                    url = url + "/";
                }
                url = url + resource;
                String response = this.PerformRequest(method, url, data);
                if (String.IsNullOrEmpty(response))
                {
                    this.lastError = "Empty response from server";
                    return null;
                }

                return JObject.Parse(response);
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
                return null;
            }
        }

        public void setSessionToken(String token)
        {
            this.sessionToken = token;
        }

        public void setVersion(String version)
        {
            this.version = version;
        }

    }
}
