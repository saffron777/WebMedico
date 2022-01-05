using System.Web;
using System.Web.Optimization;

namespace WebMedico
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Content/Assets/js/jquery-3.1.1.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Content/Assets/js/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/customval").Include(
                        "~/Content/Assets/js/jquery.validate.js",
                        "~/Content/Assets/js/jquery.validate.custom.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            //bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
            //            "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/modules").Include(
                      "~/Content/Assets/js/modules/wow.js",
                      "~/Content/Assets/js/modules/forms.js",
                      "~/Content/Assets/js/modules/forms-basic.js",
                      "~/Content/Assets/js/modules/buttons.js",
                      "~/Content/Assets/js/modules/panel-popuot.js",
                      "~/Content/Assets/js/modules/dropdown.js"));

            bundles.Add(new ScriptBundle("~/bundles/mdb").Include(
                      "~/Content/Assets/js/mdb.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Content/Assets/js/bootstrap.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/imageviewer").Include(
                      "~/Content/Assets/js/viewer.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/tether").Include(
                      "~/Content/Assets/js/tether.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/ajax").Include(
                        "~/Content/Assets/js/jquery.unobtrusive-ajax.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/charts").Include(
                        "~/Content/Assets/js/Chart.bundle.min.js"));

            bundles.Add(new StyleBundle("~/css/css").Include(
                      "~/Content/Assets/css/font-awesome.min.css",
                      "~/Content/Assets/css/material-icons.css",
                      "~/Content/Assets/css/bootstrap.min.css",
                      "~/Content/Assets/css/mdb.min.css",
                      "~/Content/Assets/css/mdbfix.css",
                      "~/Content/Assets/css/mdb.custom-components.css",
                      "~/Content/Assets/css/style.css"
                     ));

            bundles.Add(new StyleBundle("~/css/imageviewer").Include(
                      "~/Content/Assets/css/viewer.min.css"));
        }
    }
}
