using Microsoft.Owin;
using Shopping.Web;

[assembly: OwinStartup(typeof(Startup))]

namespace Shopping.Web
{
    using Owin;


    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}