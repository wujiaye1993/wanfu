using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.SignalR;
using OrchardCore.AdminMenuTest1.Services;
using OrchardCore.DisplayManagement.Notify;
using Qwe.WeChatMP.Message.Models;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.CommonAPIs;

namespace Qwe.WeChatMP.Message.Services
{
    public class WeChatHub : Hub
    {
        #region 原始代码
        /*        public void Send(MessageBody body)
                {
                    Clients.All.SendAsync("Recv", body);
                    //1、body存到数据库

                    //2、body发到微信公众号

                }

                public override Task OnConnectedAsync()
                {
                    Console.WriteLine("哇，有人进来了：{0}", this.Context.ConnectionId);
                    return base.OnConnectedAsync();
                }

                public override Task OnDisconnectedAsync(Exception exception)
                {
                    Console.WriteLine("靠，有人跑路了：{0}", this.Context.ConnectionId);
                    return base.OnDisconnectedAsync(exception);
                }

                //8.如果这个类中还有其他的字段，那么直接在下面写出来即可
                //如下:
                //public string Name;
                //public int Age;*/
        #endregion


        private readonly INotifier _notifier;
        private readonly IWeChatMPService _weChatMPservice;


        public WeChatHub(
             IHtmlLocalizer<WeChatHub> htmlLocalizer,
             INotifier notifier,
             IWeChatMPService weChatMPService
            )
        {
            H = htmlLocalizer;
            _weChatMPservice = weChatMPService;
            _notifier = notifier;
        }

        public IHtmlLocalizer H { get; set; }

        public static readonly string appId = "wxc3a7e87e3d3cb81a";
        public static readonly string appSecret = "7f331ad801e1cf978024c31c5f0a5b9f";
        public static readonly string OpenId = "oZ23J1XK3gFFHApw9aNxcf33oeJ0";

        public void Send(MessageBody body)
        {
            //await _hubContext.Clients.All.SendAsync("Notify", $"Home page loaded at: {DateTime.Now}");
            Clients.All.SendAsync("Recv", body);
            //1、body存到数据库
            //2、body发到微信公众号,post请求

            //var accessToken = AccessTokenContainer.TryGetAccessToken(appId, appSecret);
            string text = body.Content.ToString();
            var sasas = CommonApi.GetToken("wxc3a7e87e3d3cb81a", "7f331ad801e1cf978024c31c5f0a5b9f");

            CustomApi.SendText(sasas.access_token, OpenId, text);

        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine("真的吗，有人进来了：{0}", this.Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine("真的耶，有人跑路了：{0}", this.Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
