using Raven.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

/*
 * All the controllers inherit from a base controller called RavenApiController. 
 * This controller opens the session to RavenDB when is initialized and then 
 * potentially saves the changes to the database when the work is finished. 
 * The dispose method of the controller is the last method which is invoked 
 * when the work is over.
 */
namespace FileAttacher.Controllers
{
    public class RavenApiController : ApiController
    {
        public static IDocumentStore DocumentStore { get; set; }
        public IDocumentSession RavenSession { get; set; }
        public HttpRequestMessage RequestMessage { get; set; }

        /*
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            RavenSession = WebApiApplication.Store.OpenSession();
            //RavenSession = MvcApplication.Store.OpenSession();
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.IsChildAction)
                return;

            using (RavenSession)
            {
                if (filterContext.Exception != null)
                    return;

                if (RavenSession != null)
                    RavenSession.SaveChanges();
            }
        }
         * */

        //
        // GET: /RavenApi/
        protected override void Initialize(System.Web.Http.Controllers.HttpControllerContext controllerContext)
        {
            RequestMessage = controllerContext.Request;

            base.Initialize(controllerContext);
            if (RavenSession == null)
                RavenSession = WebApiApplication.Store.OpenSession();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            using (RavenSession)
            {
                if (RavenSession != null)
                    RavenSession.SaveChanges();
            }
        }
        
	}
}