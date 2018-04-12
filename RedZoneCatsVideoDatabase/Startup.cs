using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RedZoneCatsVideoDatabase.Startup))]
namespace RedZoneCatsVideoDatabase
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
