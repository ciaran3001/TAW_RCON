using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RCON_HLL_MVC.Startup))]
namespace RCON_HLL_MVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
