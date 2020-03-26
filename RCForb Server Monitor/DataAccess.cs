using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace RCForb_Server_Monitor
{
    static class DataAccess
    {
        public static dynamic GetJson(string url, bool stripCallback)
        {
            using (var httpClient = new HttpClient())
            {
                var json = httpClient.GetStringAsync(url).Result;

                if (stripCallback)
                {
                    int start = json.IndexOf('(') + 1;
                    int end = json.LastIndexOf(')');
                    json = json.Substring(start, end - start);
                }

                dynamic stuff = JObject.Parse(json);
                return stuff;
            }

        }
    }
}
