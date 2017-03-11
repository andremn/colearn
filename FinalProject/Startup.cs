using FinalProject;
using Microsoft.Owin;
using Owin;
using System;

[assembly: OwinStartup(typeof(Startup))]

namespace FinalProject
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();
        }
    }
}