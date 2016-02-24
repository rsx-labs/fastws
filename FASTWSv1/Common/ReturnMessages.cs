using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace FASTWSv1.Common
{
    public static class ReturnMessages
    {

        public static HttpResponseMessage RESPONSE_OK()
        {
            HttpRequestMessage response = new HttpRequestMessage();
            response.SetConfiguration(new System.Web.Http.HttpConfiguration());

            return response.CreateResponse(HttpStatusCode.OK, "SUCCESSFUL");
        }
        public static HttpResponseMessage RESPONSE_CREATED() 
        {
            HttpRequestMessage response = new HttpRequestMessage();
            response.SetConfiguration(new System.Web.Http.HttpConfiguration());
            
            return response.CreateResponse(HttpStatusCode.Created, "SUCCESSFUL");
        }

        public static HttpResponseMessage RESPONSE_NOTFOUND()
        {
            HttpRequestMessage response = new HttpRequestMessage();
            response.SetConfiguration(new System.Web.Http.HttpConfiguration());

            return response.CreateResponse(HttpStatusCode.OK, "NOT FOUND");
        }

        public static HttpResponseMessage RESPONSE_NOTSUCCESSFUL(string optionalMessage = "")
        {
            HttpRequestMessage response = new HttpRequestMessage();
            response.SetConfiguration(new System.Web.Http.HttpConfiguration());
            if (optionalMessage.Trim().Length > 0)
            {
                return response.CreateResponse(HttpStatusCode.OK, optionalMessage);
            }
            else
            {
                return response.CreateResponse(HttpStatusCode.OK, "NOT SUCCESSFUL");
            }

        }


        
    }
}