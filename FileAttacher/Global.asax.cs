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
            
            /* File routes */
            routes.MapHttpRoute(
                name: "SaveUploadsApi",
                routeTemplate: "api/v1/FileAtt/SaveUploads",
                defaults: new { Controller = "FileAtt", Action = "SaveUploads" },
                constraints: new { httpMethod = new HttpMethodConstraint(new[] { "GET", "POST" }) }
                );
            routes.MapHttpRoute(
                name: "RemoveS3FileApi",
                routeTemplate: "api/v1/FileAtt/RemoveS3File",
                defaults: new { Controller = "FileAtt", Action = "RemoveS3File", id = RouteParameter.Optional },
                constraints: new { httpMethod = new HttpMethodConstraint(new[] { "GET", "POST" }) }
                );
            
            /* Folder routes */
            routes.MapHttpRoute(
                name: "SaveFolderApi",
                routeTemplate: "api/v1/Folder/SaveFolder",
                defaults: new { Controller = "Folder", Action = "SaveFolder" },
                constraints: new { httpMethod = new HttpMethodConstraint(new[] { "GET", "POST" }) }
                );
            routes.MapHttpRoute(
                name: "GetFolderApi",
                routeTemplate: "api/v1/Folder/GetFolder",
                defaults: new { Controller = "Folder", Action = "GetFolder", id = RouteParameter.Optional },
                constraints: new { httpMethod = new HttpMethodConstraint(new[] { "GET", "POST" }) }
                );
        }
    }
}
