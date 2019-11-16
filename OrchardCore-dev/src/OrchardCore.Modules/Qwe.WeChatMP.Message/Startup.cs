using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using Qwe.WeChatMP.Message.Services;
using Senparc.CO2NET;
using Senparc.CO2NET.RegisterServices;
using Senparc.WebSocket;
using Senparc.Weixin;
using Senparc.Weixin.Entities;
using Senparc.Weixin.MP.Sample.CommonService.MessageHandlers.WebSocket;
using Senparc.Weixin.RegisterServices;

namespace Qwe.WeChatMP.Message
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





            services.AddHttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();



            /*
             * CO2NET 是从 Senparc.Weixin 分离的底层公共基础模块，经过了长达 6 年的迭代优化，稳定可靠。
             * 关于 CO2NET 在所有项目中的通用设置可参考 CO2NET 的 Sample：
             * https://github.com/Senparc/Senparc.CO2NET/blob/master/Sample/Senparc.CO2NET.Sample.netcore/Startup.cs
             */

            services.AddSenparcGlobalServices(Configuration)//Senparc.CO2NET 全局注册
                    .AddSenparcWeixinServices(Configuration)//Senparc.Weixin 注册
                    .AddSenparcWebSocket<CustomNetCoreWebSocketMessageHandler>();//Senparc.WebSocket 注册（按需）

            services.AddSenparcWeixinServices(Configuration); //Senparc.Weixin 注册

            //如果部署在linux系统上，需要加上下面的配置：
            //services.Configure<KestrelServerOptions>(options => options.AllowSynchronousIO = true);
            //如果部署在IIS上，需要加上下面的配置：
            services.Configure<IISServerOptions>(options => options.AllowSynchronousIO = true);


            services.AddSignalR();//使用 SignalR

        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IEndpointRouteBuilder routes
            , IOptions<SenparcSetting> senparcSetting, IServiceProvider serviceProvider)
        {



            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<WeChatHub>("/wechatHub");
            });

            //启动 CO2NET 全局注册，必须！
            // 关于 UseSenparcGlobal() 的更多用法见 CO2NET Demo：https://github.com/Senparc/Senparc.CO2NET/blob/master/Sample/Senparc.CO2NET.Sample.netcore3/Startup.
            var registerService = app.UseSenparcGlobal(env, senparcSetting.Value, globalRegister =>
            {

                #region 注册日志（按需，建议）

                globalRegister.RegisterTraceLog(ConfigTraceLog);//配置TraceLog

                #endregion

            });

            /*            routes.MapAreaControllerRoute
                        (
                            name: "Home",
                            areaName: "OrchardCore.Mvc.HelloWorld",
                            pattern: "",
                            defaults: new { controller = "Home", action = "Index" }
                        );*/
        }

        /// <summary>
        /// 配置微信跟踪日志
        /// </summary>
        private void ConfigTraceLog()
        {
            //这里设为Debug状态时，/App_Data/WeixinTraceLog/目录下会生成日志文件记录所有的API请求日志，正式发布版本建议关闭

            //如果全局的IsDebug（Senparc.CO2NET.Config.IsDebug）为false，此处可以单独设置true，否则自动为true
            Senparc.CO2NET.Trace.SenparcTrace.SendCustomLog("系统日志", "系统启动");//只在Senparc.Weixin.Config.IsDebug = true的情况下生效

            //全局自定义日志记录回调
            Senparc.CO2NET.Trace.SenparcTrace.OnLogFunc = () =>
            {
                //加入每次触发Log后需要执行的代码
            };

            //当发生基于WeixinException的异常时触发
            WeixinTrace.OnWeixinExceptionFunc = async ex =>
            {
                //加入每次触发WeixinExceptionLog后需要执行的代码

                //发送模板消息给管理员                             -- DPBMARK Redis
                var eventService = new Senparc.Weixin.MP.Sample.CommonService.EventService();
                await eventService.ConfigOnWeixinExceptionFunc(ex);      // DPBMARK_END
            };
        }
    }
}
