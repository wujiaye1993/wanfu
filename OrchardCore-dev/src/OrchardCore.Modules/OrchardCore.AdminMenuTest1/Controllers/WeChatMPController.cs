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
using OrchardCore.AdminMenuTest1.Services;
//using OrchardCore.AdminMenu;
using OrchardCore.AdminMenuTest1.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Settings;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OrchardCore.AdminMenuTest1.Controllers
{
    [Admin]
    public class WeChatMPController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ISiteService _siteService;
        private readonly IWeChatMPService _weChatMPservice;
        private readonly INotifier _notifier;

        //private readonly IAdminMenuService _AdminMenuService;

        public WeChatMPController(
            IAuthorizationService authorizationService,
            ISiteService siteService,
            IWeChatMPService weChatMPService,
            INotifier notifier,
            IStringLocalizer<WeChatMPController> stringLocalizer,
            IHtmlLocalizer<WeChatMPController> htmlLocalizer,
            ILogger<WeChatMPController> logger,
            IShapeFactory shapeFactory

            //IAdminMenuService AdminMenuService

            )
        {
            _authorizationService = authorizationService;
            _siteService = siteService;
            _weChatMPservice = weChatMPService;
            _notifier = notifier;
            New = shapeFactory;

            // _AdminMenuService = AdminMenuService;

            T = stringLocalizer;
            H = htmlLocalizer;
            Logger = logger;
        }


        public IStringLocalizer T { get; set; }
        public IHtmlLocalizer H { get; set; }
        public ILogger Logger { get; set; }
        public dynamic New { get; set; }


        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> List(WeChatMPListOptions options, PagerParameters pagerParameters)
        {
            ////权限判断
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWeChatMP))
            {
                return Unauthorized();
            }
            //站点设置
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);

            // default options
            if (options == null)
            {
                options = new WeChatMPListOptions();
            }
            //调用服务，获取所有公众号
            var trees = await _weChatMPservice.GetAsync();
            //var trees = await _AdminMenuService.GetAsync();

            //搜索栏不是为空或者空格， 搜索出来的对象转为集合，赋给trees
            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                trees = trees.Where(dp => dp.Name.Contains(options.Search)).ToList();
            }
            //获取tress的记录条数
            var count = trees.Count();
            //起始页，每页条数大小
            var startIndex = pager.GetStartIndex();
            var pageSize = pager.PageSize;
            IEnumerable<Models.WeChatMP> results = new List<Models.WeChatMP>(); //???

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
            routeData.Values.Add("Options.Search", options.Search); //路由数据增加键值对
            //页面形状 有查页面键的时候显示查的，没有就按最大页面形状构建
            var pagerShape = (await New.Pager(pager)).TotalItemCount(count).RouteData(routeData);
            //构建模型
            var model = new WeChatMPListVM
            {
                WeChatMP = results.Select(x => new AdminMenuEntry { WeChatMP = x }).ToList(),
                Options = options,
                Pager = pagerShape
            };
            //把模型放在要返回的视图
            return View(model);

        }



        public async Task<IActionResult> Create()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWeChatMP))
            {
                return Unauthorized();
            }

            var model = new WeChatMPCreateVM();

            return View(model);
        }


        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Create(WeChatMPCreateVM model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWeChatMP))
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                var checkResult = await _weChatMPservice.CheckAccessAsync(model.AppId, model.Secret);

                if(checkResult.check)
                {
                    var tree = new Models.WeChatMP {
                        Name = model.Name,
                        AppId = model.AppId,
                        Secret = model.Secret
                    };
                    await _weChatMPservice.SaveAsync(tree);

                    _notifier.Success(H["公众号绑定成功！"]);
                    return RedirectToAction(nameof(List));
                }
                _notifier.Error(H["错误："+ checkResult.msg]);
            }


            return View(model);
        }


        public async Task<IActionResult> Edit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWeChatMP))
            {
                return Unauthorized();
            }

            var tree = await _weChatMPservice.GetByIdAsync(id);

            if (tree == null)
            {
                return NotFound();
            }

            var model = new WeChatMPEditVM
            {
                Id = tree.Id,
                Name = tree.Name,
                AppId = tree.AppId,
                Secret = tree.Secret
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(WeChatMPEditVM model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWeChatMP))
            {
                return Unauthorized();
            }

            var tree = await _weChatMPservice.GetByIdAsync(model.Id);

            if (tree == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                //把VM的内容赋给新对象，然后再用这个对象去执行 Save业务
                tree.Name = model.Name;
                tree.AppId = model.AppId;
                tree.Secret = model.Secret;

                await _weChatMPservice.SaveAsync(tree);

                _notifier.Success(H["Admin menu updated successfully"]);

                return RedirectToAction(nameof(List));
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWeChatMP))
            {
                return Unauthorized();
            }

            var tree = await _weChatMPservice.GetByIdAsync(id);

            if (tree == null)
            {
                _notifier.Error(H["Can't find the admin menu."]);
                return RedirectToAction(nameof(List));
            }

            var removed = await _weChatMPservice.DeleteAsync(tree);


            if (removed == 1)
            {
                _notifier.Success(H["Admin menu deleted successfully"]);
            }
            else
            {
                _notifier.Error(H["Can't delete the admin menu."]);
            }

            return RedirectToAction(nameof(List));
        }

        //公众号状态切换，即是否起效
        [HttpPost]
        public async Task<IActionResult> Toggle(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWeChatMP))
            {
                return Unauthorized();
            }

            var tree = await _weChatMPservice.GetByIdAsync(id);

            if (tree == null)
            {
                return NotFound();
            }

            tree.Enabled = !tree.Enabled;

            await _weChatMPservice.SaveAsync(tree);

            _notifier.Success(H["WeChatMP toggled successfully"]);

            return RedirectToAction(nameof(List));
        }


    }
}
