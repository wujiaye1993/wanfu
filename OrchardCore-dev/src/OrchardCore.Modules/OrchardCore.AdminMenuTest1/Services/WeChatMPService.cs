using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.AdminMenuTest1.Models;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Shell.Scope;
using YesSql;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Senparc.Weixin.MP.CommonAPIs;

namespace OrchardCore.AdminMenuTest1.Services
{
    public class WeChatMPService : IWeChatMPService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;

        private const string WeChatMPCacheKey = "WeChatMPervice";

        public WeChatMPService(
            IMemoryCache memoryCache,
            ISignal signal
            )
        {
            _memoryCache = memoryCache;
            _signal = signal;
              
        }

        public async Task<List<Models.WeChatMP>> GetAsync()
        {
            return (await GetWeChatMPList()).WeChatMP;
        }

        

        private async Task<WeChatMPList> GetWeChatMPList()
        {
            WeChatMPList treeList;

            if (!_memoryCache.TryGetValue(WeChatMPCacheKey, out treeList))
            {
                var session = GetSession();

                treeList = await session.Query<WeChatMPList>().FirstOrDefaultAsync();

                if (treeList == null)
                {
                    lock (_memoryCache)
                    {
                        if (!_memoryCache.TryGetValue(WeChatMPCacheKey, out treeList))
                        {
                            treeList = new WeChatMPList();
                            session.Save(treeList);
                            _memoryCache.Set(WeChatMPCacheKey, treeList);
                            _signal.SignalToken(WeChatMPCacheKey);
                        }
                    }
                }
                else
                {
                    _memoryCache.Set(WeChatMPCacheKey, treeList);
                    _signal.SignalToken(WeChatMPCacheKey);
                }
            }

            return treeList;
        }

        private ISession GetSession()
        {
            return ShellScope.Services.GetService<ISession>();
            //using Microsoft.Extensions.DependencyInjection;
        }

        public async Task SaveAsync(Models.WeChatMP tree)
        {
            var adminMenuList = await GetWeChatMPList();
            var session = GetSession();

            //根据id判断是否重复
            var preexisting = adminMenuList.WeChatMP.Where(x => x.Id == tree.Id).FirstOrDefault();

            // it's new? add it
            if (preexisting == null)
            {
                adminMenuList.WeChatMP.Add(tree);
            }
            else // not new: replace it
            {
                var index = adminMenuList.WeChatMP.IndexOf(preexisting);
                adminMenuList.WeChatMP[index] = tree;
            }

            session.Save(adminMenuList);

            _memoryCache.Set(WeChatMPCacheKey, adminMenuList);
            _signal.SignalToken(WeChatMPCacheKey);
        }

        public async Task<Models.WeChatMP> GetByIdAsync(string id)
        {
            return (await GetWeChatMPList())
               .WeChatMP
               .Where(x => String.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase))
               .FirstOrDefault();

        }

        public async Task<int> DeleteAsync(Models.WeChatMP tree)
        {
            //获取公众号集合
            var WeChatMPList = await GetWeChatMPList();
            var session = GetSession(); //拿到session
            //获取删除数量
            var count = WeChatMPList.WeChatMP.RemoveAll(x => String.Equals(x.Id, tree.Id));
            //保存到session
            session.Save(WeChatMPList);
            //重新缓存
            _memoryCache.Set(WeChatMPCacheKey, WeChatMPList);
            //更改口令也重新对应缓存
            _signal.SignalToken(WeChatMPCacheKey);

            return count;
        }

        public async Task<CheckAccess> CheckAccessAsync(string appId, string secret)
        {
            var checkAccess = new CheckAccess();
            var Result = await CommonApi.GetTokenAsync(appId,secret);
            if (Result.errcode == 0)
            {

                checkAccess.msg = Result.errmsg;
                checkAccess.check = true;
                return checkAccess;
            }else if(Result.ErrorCodeValue == 40164){
                checkAccess.msg = "不合法的IP地址，可能公众号没有绑定！"+Result.errmsg; //提示ip地址
                checkAccess.check = false;
                return checkAccess;
            }
            else if(Result.ErrorCodeValue == 40013)
            {
                checkAccess.msg = "不合法的APPID";
                checkAccess.check = false;
                return checkAccess;
            }
            else if (Result.ErrorCodeValue == 40125)
            {
                checkAccess.msg = "appsecret不正确";
                checkAccess.check = false;
                return checkAccess;
            }
            else if (Result.ErrorCodeValue == 41004)
            {
                checkAccess.msg = "不合法的APPID";
                checkAccess.check = false;
                return checkAccess;
            }
            checkAccess.msg = Result.errmsg;
            checkAccess.check = false;

            return checkAccess;
            
        }

        public CheckAccess CheckAccess(string appId, string secret)
        {
            var checkAccess = new CheckAccess();
            var Result = CommonApi.GetToken(appId, secret);
            if (Result.errcode == 0)
            {

                checkAccess.msg = Result.errmsg;
                checkAccess.check = true;
                return checkAccess;
            }
            else if (Result.ErrorCodeValue == 40164)
            {
                checkAccess.msg = "不合法的IP地址，可能公众号没有绑定！" + Result.errmsg; //提示ip地址
                checkAccess.check = false;
                return checkAccess;
            }
            else if (Result.ErrorCodeValue == 40013)
            {
                checkAccess.msg = "不合法的APPID";
                checkAccess.check = false;
                return checkAccess;
            }
            else if (Result.ErrorCodeValue == 40125)
            {
                checkAccess.msg = "appsecret不正确";
                checkAccess.check = false;
                return checkAccess;
            }
            else if (Result.ErrorCodeValue == 41004)
            {
                checkAccess.msg = "不合法的APPID";
                checkAccess.check = false;
                return checkAccess;
            }
            checkAccess.msg = Result.errmsg;
            checkAccess.check = false;

            return checkAccess;
        }
    }
}
