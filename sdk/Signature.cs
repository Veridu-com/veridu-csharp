using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading.Tasks;

namespace Veridu
{
    class Signature
    {
        private String apikey;
        private String apisecret;
        private String version = "0.3";

        public Signature(String apikey, String secret, String version)
        {
            this.apisecret = secret;
            this.apikey = apikey;
            if (!String.IsNullOrEmpty(version))
                this.version = version;
        }

        public String SignRequest(String method, String resource, String nonce)
        {
            TimeSpan span = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            string timestamp = ((long)span.TotalSeconds).ToString();
            String url = URLEncoder(String.Format("https://api.veridu.com/{0}{1}",this.version,((resource[0] == '/') ? resource : "/" + resource)));
            String newdata = "client=" + this.apikey +
                         "&method=" + method +
                         "&nonce=" + nonce +
                         "&resource=" + url +
                         "&timestamp=" + timestamp +
                         "&version=" + this.version;
            String sign = HmacSHA1.Calculate(newdata, this.apisecret);

            return newdata + "&signature=" + sign;
        }

        public string URLEncoder(String text)
        {
            return System.Uri.EscapeDataString(text).Replace("%20", "+");
        }
    }
}
