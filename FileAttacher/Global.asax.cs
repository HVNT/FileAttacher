using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Raven.Client.Document;
using Raven.Abstractions.Data;
using Raven.Client.Indexes;
using System.Reflection;
using System.Threading;
using System.Globalization;

using FileAttacher.Controllers;

namespace FileAttacher
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        
        public static DocumentStore Store;

        /*
         * Fired when the first instance of the HttpApplication class is created. 
         * It allows you to create objects that are accessible by all HttpApplication instances.
         */
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // ravendb.net
            /* initialize DocumentStore */
            //Store = new DocumentStore { ConnectionStringName = "RavenDB" };
            Store = new DocumentStore { Url = "http://localhost:8888" };
            Store.Initialize();

            /* Automatically creates all indexes that are declared in code */
            IndexCreation.CreateIndexes(Assembly.GetCallingAssembly(), Store);


            RavenApiController.DocumentStore = Store;
        }

        /*
        private static void RavenDBInit()
        {
            var parser = ConnectionStringParser<RavenConnectionStringOptions>.FromConnectionStringName("RavenDB");
            parser.Parse();

            Store = new DocumentStore
            {
                ApiKey = parser.ConnectionStringOptions.ApiKey,
                Url = parser.ConnectionStringOptions.Url,
            };

            Store.Initialize();


            IndexCreation.CreateIndexes(Assembly.GetCallingAssembly(), Store);

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
        }
        */

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        /*
         * Fired when an application request is received. It's the first event fired for
         * a request, which is often a page request (URL) that a user enters.
         */
        protected void Application_BeginRequest()
        {
            //TODO: MVC takes the culture of the browser as the default. This culture is later used in 
            //order to validate the model -> issue while javascript will use english culture on client side...
            Thread.CurrentThread.CurrentCulture
              = CultureInfo.InvariantCulture;
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapHttpRoute(
                name: "FilesGetAll",
                routeTemplate: "api/FileAtt/GetAll",
                defaults: new { Controller = "FileAtt", Action = "GetAll" },
                constraints: new { httpMethod = new HttpMethodConstraint(new[] { "GET", "POST" }) }
                );
            routes.MapHttpRoute(
                name: "SaveUploads",
                routeTemplate: "api/FileAtt/SaveUploads",
                defaults: new { Controller = "FileAtt", Action = "SaveUploads" },
                constraints: new { httpMethod = new HttpMethodConstraint(new[] { "POST" }) }
                );
            routes.MapHttpRoute(
                name: "RemoveFile",
                routeTemplate: "api/FileAtt/RemoveFile",
                defaults: new { Controller = "FileAtt", Action = "RemoveFile", id = RouteParameter.Optional},
                constraints: new { httpMethod = new HttpMethodConstraint(new[] { "GET", "POST" }) }
                );
        }
    }
}
