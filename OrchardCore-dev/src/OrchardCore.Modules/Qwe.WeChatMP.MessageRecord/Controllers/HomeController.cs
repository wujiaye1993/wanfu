using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Settings;
using Qwe.WeChatMP.MessageRecord.Services;
using Qwe.WeChatMP.MessageRecord.ViewModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Qwe.WeChatMP.MessageRecord.Controllers
{
    [Admin]
    public class HomeController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService; //权限注入
        private readonly ISiteService _siteService; //站点设置注入
        private readonly IMsgService _msgService;   //本服务注入
        private readonly INotifier _notifier;   //警告插件注入

        public IStringLocalizer T { get; set; }
        public IHtmlLocalizer H { get; set; }
        public ILogger Logger { get; set; }
        public dynamic New { get; set; } //运行时解析其操作对象？？？把Json对象解析？

        public HomeController(
           IAuthorizationService authorizationService,
           ISiteService siteService,
           IMsgService msgService,
           INotifier notifier,
           IStringLocalizer<HomeController> stringLocalizer,    //字符串本地化构建注入
           IHtmlLocalizer<HomeController> htmlLocalizer,    //html页面本地化注入
           ILogger<HomeController> logger,  //日志注入
           IShapeFactory shapeFactory   //动态形状 注入

           //IAdminMenuService AdminMenuService

           )
        {
            _authorizationService = authorizationService;
            _siteService = siteService;
            _msgService = msgService;
            _notifier = notifier;
            New = shapeFactory;

            // _AdminMenuService = AdminMenuService;

            T = stringLocalizer;
            H = htmlLocalizer;
            Logger = logger;
        }

        //记录页面
        public async Task<IActionResult> List(MsgListOptions options, PagerParameters pagerParameters)
        {
            //1、权限判断
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMessageRecord))
            {
                return Unauthorized();
            }
            //2、站点设置
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);

            // default options
            if (options == null)
            {
                options = new MsgListOptions();
            }
            //3、调用服务，获取所有
            var trees = (await _msgService.GetAsync()).OrderByDescending(t => t.CreateTime).ToList();//排序
            
            //var trees = await _AdminMenuService.GetAsync();
            //4、配置搜索栏。  搜索栏不是为空或者空格， 搜索出来的对象转为集合，赋给trees

            if (!string.IsNullOrWhiteSpace(options.SearchKfName))
            {
                if (!string.IsNullOrWhiteSpace(options.SearchSpeaker))
                {
                    if (DateTime.Compare(options.SearchMaxTime, options.SearchMinTime) > 0)
                    {
                        var Aa = trees.Where(dp => dp.KfName.Contains(options.SearchKfName)).ToList();
                        var Bb = trees.Where(dp => dp.Speaker.Contains(options.SearchSpeaker)).ToList();
                        var Min = trees.Where(dp => dp.CreateTime >= options.SearchMinTime).ToList();
                        var Max = trees.Where(dp => dp.CreateTime <= options.SearchMaxTime).ToList();
                        var Dd = Aa.Intersect(Bb).Intersect(Min).Intersect(Max).OrderByDescending(t => t.CreateTime).ToList();
                        goto five;
                    }
                    var A = trees.Where(dp => dp.KfName.Contains(options.SearchKfName)).ToList();
                    var B = trees.Where(dp => dp.Speaker.Contains(options.SearchSpeaker)).ToList();
                    trees = A.Intersect(B).OrderByDescending(t => t.CreateTime).ToList();
                    goto five;
                }
                trees = trees.Where(dp => dp.KfName.Contains(options.SearchKfName)).OrderByDescending(t => t.CreateTime).ToList();
                System.Threading.Thread.Sleep(1000);
                goto five;
            }
            else if (!string.IsNullOrWhiteSpace(options.SearchSpeaker))
            {
                if (DateTime.Compare(options.SearchMaxTime, options.SearchMinTime) > 0)
                {
                    var C = trees.Where(dp => dp.Speaker.Contains(options.SearchSpeaker)).ToList();
                    var A = trees.Where(dp => dp.CreateTime >= options.SearchMinTime).ToList();
                    var B = trees.Where(dp => dp.CreateTime <= options.SearchMaxTime).ToList();
                    trees = A.Intersect(B).Intersect(C).OrderByDescending(t => t.CreateTime).ToList(); //交集,排序（时间）
                    goto five;
                }
                trees = trees.Where(dp => dp.Speaker.Contains(options.SearchSpeaker)).OrderByDescending(m => m.CreateTime).ToList();
                System.Threading.Thread.Sleep(1000);
                goto five;
            }
            else if (DateTime.Compare(options.SearchMaxTime, options.SearchMinTime) > 0)
            {
                var A = trees.Where(dp => dp.CreateTime >= options.SearchMinTime).ToList();
                var B = trees.Where(dp => dp.CreateTime <= options.SearchMaxTime).ToList();
                trees = A.Intersect(B).OrderByDescending(t => t.CreateTime).ToList(); //交集
                goto five;
            }

            //5、配置分页
            //获取tress的记录条数
            five: var count = trees.Count();
            //起始页，每页条数大小
            var startIndex = pager.GetStartIndex();
            var pageSize = pager.PageSize;
            IEnumerable<Models.Msg> results = new List<Models.Msg>(); //???

            //todo: handle the case where there is a deserialization exception on some of the presets.
            // load at least the ones without error. Provide a way to delete the ones on error.
            //处理在某些预设上存在反序列化异常的情况，至少加载没有错误的预设提供删除出错项的方法
            try
            {
                results = trees
                .Skip(startIndex)
                .Take(pageSize)
                .ToList();
            }

            catch (Exception ex)
            {
                Logger.LogError(ex, "Error when retrieving the list of admin menus");
                _notifier.Error(H["Error when retrieving the list of admin menus"]);
            }

            // Maintain previous route data when generating page links 生成页面链接时维护以前的路由数据
            var routeData = new RouteData();
            routeData.Values.Add("Options.Search", options.SearchKfName); //路由数据增加键值对？？？
            //页面形状 有查页面键的时候显示查的，没有就按最大页面形状构建
            var pagerShape = (await New.Pager(pager)).TotalItemCount(count).RouteData(routeData);

            //6、构建模型，同时与分页结合
            var model = new MsgListVM
            {
                Msg = results.Select(x => new AdminMenuEntry { Msg = x }).OrderByDescending(t => t.Msg.CreateTime).ToList(),
                Options = options,
                Pager = pagerShape
            };

            //7、把模型放在要返回的视图
            return View(model);

        }

        //创建页面
        public async Task<IActionResult> Create()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMessageRecord))
            {
                return Unauthorized();
            }

            var model = new MsgCreateVM();

            return View(model);
        }
        //创建记录
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Create(MsgCreateVM model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMessageRecord))
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                //var checkResult = await _msgService.CheckAccessAsync(model.AppId, model.Secret);

                var tree = new Models.Msg
                {
                    KfName = model.KfName,
                    CreateTime = model.CreateTime,
                    ToUserName = model.ToUserName,
                    FromUserName = model.FromUserName,
                    Speaker = model.Speaker,
                    Content = model.Content
                };
                await _msgService.SaveAsync(tree);

                _notifier.Success(H["消息记录成功！"]);
                return RedirectToAction(nameof(List));
            }
            return View(model);
        }
        //删除记录
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMessageRecord))
            {
                return Unauthorized();
            }

            var tree = await _msgService.GetByIdAsync(id);

            if (tree == null)
            {
                _notifier.Error(H["Can't find the Record."]);
                return RedirectToAction(nameof(List));
            }

            var removed = await _msgService.DeleteAsync(tree);


            if (removed == 1)
            {
                _notifier.Success(H["Record deleted successfully"]);
            }
            else
            {
                _notifier.Error(H["Can't delete the Record."]);
            }

            return RedirectToAction(nameof(List));
        }


        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }
    }
}
