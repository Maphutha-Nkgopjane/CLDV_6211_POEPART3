using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Data.Entity; // You need this using directive for Database.SetInitializer

namespace NoxEventEazeAppp // This must match your project's root namespace
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Add this if you want to ensure the DB initializer is set early
            // This line is CRITICAL when you are managing your database schema manually (with SQL scripts)
            // and do NOT want Entity Framework to try and create, drop, or alter your database.
            // Database.SetInitializer<NoxEventEazeDBContext>(null);
        }
    }
}