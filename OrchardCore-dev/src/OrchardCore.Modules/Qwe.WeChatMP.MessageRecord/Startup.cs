using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Security.Permissions;
using Qwe.WeChatMP.MessageRecord.Services;

namespace Qwe.WeChatMP.MessageRecord
{
    public class Startup 
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
            //services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddSingleton<IMsgService, MsgService>();




        }


      
    }
}
