using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using Microsoft.AspNetCore.Mvc;
using Senparc.Weixin.MP.Entities.Request;

using Senparc.Weixin.MP.Sample.CommonService.CustomMessageHandler;
using System.IO;
using Senparc.Weixin.HttpUtility;
using Senparc.Weixin.MP.Sample.CommonService.Utilities;
using Senparc.CO2NET.HttpUtility;
using System.Xml.Linq;
using System.Threading;
using Senparc.NeuChar.MessageHandlers;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.MvcExtension;
using Senparc.Weixin.MP.MessageHandlers;
using Microsoft.AspNetCore.SignalR;
using Senparc.Weixin.MP.Entities;
using Qwe.WeChatMP.Message.Services;
using Qwe.WeChatMP.Message.Models;
using OrchardCore.DisplayManagement.Notify;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.AdvancedAPIs;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Qwe.WeChatMP.Message.Controllers
{

    
    public class MessageMPAsyncController : Controller
    {

        private readonly INotifier _notifier;
        //注入_hubContext，注册管道
        private readonly IHubContext<WeChatHub> _hubContext;
        public MessageMPAsyncController(
             INotifier notifier,
             IHubContext<WeChatHub> hubContext
            )
        {
            _notifier = notifier;
            _hubContext = hubContext;
        }

        public readonly Func<string> _getRandomFileName = () => SystemTime.Now.ToString("yyyyMMdd-HHmmss") +
            "_Async_" + Guid.NewGuid().ToString("n").Substring(0, 6);

        //构建顾客消息对象
        MessageBody body = new MessageBody();
        //公众号集统一服务器验证Token
        public readonly string Token = "wanfu";
        public readonly string AppId = "wxc3a7e87e3d3cb81a";
        public readonly string Secret = "7f331ad801e1cf978024c31c5f0a5b9f";
        [HttpGet]
        [ActionName("Get")]
        public Task<ActionResult> Get(string signature, string timestamp, string nonce, string echostr)
        {
            return Task.Factory.StartNew(() =>
            {
                if (CheckSignature.Check(signature, timestamp, nonce, Token))
                {
                    return echostr; //返回随机字符串则表示验证通过
                }
                else
                {
                    return "failed:" + signature + "," + CheckSignature.GetSignature(timestamp, nonce, Token) + "。" +
                        "如果你在浏览器中看到这句话，说明此地址可以被作为微信公众账号后台的Url，请注意保持Token一致。";
                }
            }).ContinueWith<ActionResult>(task => Content(task.Result));
        }



        /// <summary>
        /// 最简化的处理流程
        /// </summary>
        [HttpPost]
        [ActionName("Index")]
        [IgnoreAntiforgeryToken]
        public async Task<ActionResult> Post(PostModel postModel)
        {
/*            if (!CheckSignature.Check(postModel.Signature, postModel.Timestamp, postModel.Nonce, Token))
            {
                return new WeixinResult("参数错误！");
            }*/



            //var cancellationToken = new CancellationToken();//给异步方法使用

            var messageHandler = new CustomMessageHandler(Request.GetRequestMemoryStream(), postModel, 10);

            #region 没有重写的异步方法将默认尝试调用同步方法中的代码（为了偷懒）

            /* 使用 SelfSynicMethod 的好处是可以让异步、同步方法共享同一套（同步）代码，无需写两次，
             * 当然，这并不一定适用于所有场景，所以是否选用需要根据实际情况而定，这里只是演示，并不盲目推荐。*/
            messageHandler.DefaultMessageHandlerAsyncEvent = DefaultMessageHandlerAsyncEvent.SelfSynicMethod;

            #endregion

            #region 设置消息去重 设置

            /* 如果需要添加消息去重功能，只需打开OmitRepeatedMessage功能，SDK会自动处理。
             * 收到重复消息通常是因为微信服务器没有及时收到响应，会持续发送2-5条不等的相同内容的RequestMessage*/
            messageHandler.OmitRepeatedMessage = true;//默认已经开启，此处仅作为演示，也可以设置为false在本次请求中停用此功能

            #endregion

            messageHandler.SaveRequestMessageLog();//记录 Request 日志（可选）


            //这里调用WeCHatHub类的send方法建立SignalR管道，前端-服务器-微信(CustomApi发送客服消息，白名单)，微信-服务器-前端(Post)要用同个hub实例管道对象


            //拿到请求消息内容 仿消息处理
            var textMessageFromWeixin = messageHandler.RequestMessage as RequestMessageText;
            var contentFromWeixin = textMessageFromWeixin.Content;
            
            //把 contentFromWeixin 放到 SignalR 管道中（SingleHub），再由管道发送到前端显示     

            //赋给SingleHub管道里的字段，构建顾客消息对象 //这里传入User的Id，即客服的Id，管道Client的Id以需要，下面的123可以替换成UserId
            //await _hubContext.Clients.Client("123").SendAsync("Recv", body);
            //var name = System.Security.Claims.ClaimsPrincipal.Current.Identity.Name;

            #region 从消息中获取哪个公众号
            var AppIdFromWeixin = textMessageFromWeixin.ToUserName;
            #endregion

            #region 判断缓存中是否有该关于AppId信息，有就从中获取，没有查数据库AppId获取对应的Secret

            #endregion

            #region 从缓存中获取AccessToken
            var Result = CommonApi.GetToken(AppIdFromWeixin, Secret);
            #endregion

            #region 从消息中获取哪个用户
            var OpenIdFromWeixin = textMessageFromWeixin.FromUserName;
            #endregion

            #region 获取用户信息
            var InfoResult = UserApi.Info(Result.access_token, OpenIdFromWeixin);
            var nickname = InfoResult.nickname;
            var sex = InfoResult.sex;
            var city = InfoResult.city;
            var remark = InfoResult.remark;
            var headimgurl = InfoResult.headimgurl;
            var subscribe = InfoResult.subscribe; //用户是否订阅该公众号标识，值为0时，代表此用户没有关注该公众号，拉取不到其余信息。
            #endregion

            var time = textMessageFromWeixin.CreateTime;

            #region 构建接收消息体到前端
            body.ToUserName = textMessageFromWeixin.ToUserName;
            body.FromUserName = textMessageFromWeixin.FromUserName;
            body.CreateTime = time.ToString("yyyy-MM-dd HH:mm:ss");
            body.UserName = nickname;
            body.Content = contentFromWeixin;
            #endregion

            //在Controller里使用管道输送
            await _hubContext.Clients.All.SendAsync("Recv", body);

            //Senparc消息处理
            //await messageHandler.ExecuteAsync(cancellationToken); //执行微信处理过程（关键）

            messageHandler.SaveResponseMessageLog();//记录 Response 日志（可选）

            //消息记录存储到数据库 

            return new FixWeixinBugWeixinResult(messageHandler);
        }
        public async Task<IActionResult> Index()
        {
            await _hubContext.Clients.All.SendAsync("Recv", body);
            return View();
        }

        public IActionResult Admin()
        {
            return RedirectToAction("Index", "Admin", new { area = "OrchardCore.Admin" });
        }

    }
}
