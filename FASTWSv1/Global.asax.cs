using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace FASTWSv1
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Configure(System.Web.Http.GlobalConfiguration.Configuration);
        }

        public class ElmahErrorAttribute : ExceptionFilterAttribute
        {
            public override void OnException(
                System.Web.Http.Filters.HttpActionExecutedContext actionExecutedContext)
            {

                if (actionExecutedContext.Exception != null)
                    Elmah.ErrorSignal.FromCurrentContext().Raise(actionExecutedContext.Exception);

                base.OnException(actionExecutedContext);
            }
        }

        private void Configure(HttpConfiguration httpConfiguration)
        {
            httpConfiguration.Filters.Add(
                new ElmahErrorAttribute()
            );

        }
    }
}
