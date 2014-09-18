using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Veridu
{
    class Session
    {
        public Request request;
        private String token;
        private long expires;
        private String username;
        public String lastError;

        public Session(Request request)
        {
            this.request = request;
        }

        public bool Create(bool isreadonly)
        {
            try
            {
                String resource = (isreadonly ? "/session/" : "/session/write/");
                JObject response = this.request.fetchSignedResource("POST", resource, "");

                if ((bool)response["status"])
                {
                    this.token = (String)response["token"];
                    this.expires = (long)response["expires"];
                    this.request.setSessionToken((String)response["token"]);
                    return true;
                }
                this.lastError = (String)response["error"]["message"];
                return false;
            }
            catch (Exception e)
            {
                this.lastError = e.Message;
                return false;
            }
        }

        public bool Extend()
        {
            if (String.IsNullOrEmpty(this.token))
            {
                this.lastError = "Cannot extend an empty session";
                return false;
            }

            try
            {
                String resource = "/session/" + this.token;
                JObject response = this.request.fetchSignedResource("PUT", resource, "");

                if ((bool)response["status"])
                {
                    this.expires = (long)response["expires"];
                    return true;
                }
                this.lastError = (String)response["error"]["message"];
                return false;
            }
            catch (Exception e)
            {
                this.lastError = e.Message;
                return false;
            }
        }

        public bool Expire()
        {
            if (String.IsNullOrEmpty(this.token))
            {
                this.lastError = "Cannot expire an empty session";
                return false;
            }

            try
            {
                String resource = "/session/" + this.token;
                dynamic response = this.request.fetchSignedResource("DELETE", resource, "");
                if ((bool)response["status"])
                {
                    this.request.setSessionToken(null);
                    this.expires = 0;
                    this.token = null;
                    this.username = null;
                    return true;
                }
                this.lastError = (String)response["error"]["message"];
                return false;
            }
            catch (Exception e)
            {
                this.lastError = e.Message;
                return false;
            }
        }

        public bool Assign(String username)
        {
            if (String.IsNullOrEmpty(this.token))
            {
                this.lastError = "Cannot assign an user to an empty session";
                return false;
            }
            try
            {
                String resource = "/user/" + username;
                dynamic response = this.request.fetchSignedResource("POST", resource, "");

                if ((bool)response["status"])
                {
                    this.username = username;
                    return true;
                }
                this.lastError = (String)response["error"]["message"];
                return false;
            }
            catch (Exception e)
            {
                this.lastError = e.Message;
                return false;
            }
        }

        public String getToken()
        {
            return this.token;
        }

        public long getExpires()
        {
            return this.expires;
        }

        public String getUsername()
        {
            return this.username;
        }

    }
}
