using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TesteVeriduSDK
{
    class Program
    {
        static void Main(string[] args)
        {
           Veridu.Request request = new Veridu.Request("<API Key>", 
                                                       "<API Secret>",
                                                       "0.3");

           Veridu.Session session = new Veridu.Session(request);

           session.Create(false);
           session.Assign("demo-user");
           session.Extend();

           request.fetchResource("POST", "personal/demo-user", "fname=demo&lname=user");
           request.fetchSignedResource("GET", "user/demo-user", "");
        }
    }
}
