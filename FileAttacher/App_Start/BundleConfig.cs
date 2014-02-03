using System.Web;
using System.Web.Optimization;

namespace FileAttacher
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                    "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/base").Include(
                    "~/Scripts/knockout-2.3.0.js",
                    "~/Scripts/bootstrap.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/uploader.min.js",
                    "~/Scripts/jquery-ui-1.10.4.min.js",
                    "~/Scripts/jquery.fancybox-1.3.4_patch.js",
                    "~/Scripts/jquery.fineuploader-3.1.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/scripts").Include(
                    "~/Scripts/ModalViewModel.js",
                    "~/Scripts/MainViewModel.js",
                    "~/Scripts/FileAttacher.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                    "~/Content/bootstrap.css",
                    "~/Content/bootstrap-responsive.css",
                    "~/Content/site.css",
                    "~/Content/FancyBox/jquery.fancybox-1.3.4.css",                    
                    "~/Content/FineUploader/fineuploader.css"));
        }
    }
}
