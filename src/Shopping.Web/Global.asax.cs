namespace Shopping.Web
{
    using System;
    using System.Configuration;
    using System.Web;
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;
    using MassTransit;


    public class MvcApplication :
        HttpApplication
    {
        static IBusControl _bus;
        static BusHandle _busHandle;

        public static IBus Bus
        {
            get { return _bus; }
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            _bus = MassTransit.Bus.Factory.CreateUsingRabbitMq(x =>
            {
                x.Host(new Uri(ConfigurationManager.AppSettings["RabbitMQHost"]), h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
            });

            _busHandle = MassTransit.Util.TaskUtil.Await<BusHandle>(()=>_bus.StartAsync());
        }

        protected void Application_End()
        {
            if (_busHandle != null)
                _busHandle.Stop(TimeSpan.FromSeconds(30));
        }
    }
}