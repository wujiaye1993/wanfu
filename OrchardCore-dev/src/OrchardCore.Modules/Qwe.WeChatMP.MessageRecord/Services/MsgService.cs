using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Shell.Scope;
using Qwe.WeChatMP.MessageRecord.Models;
using YesSql;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Qwe.WeChatMP.MessageRecord.Services
{
    public class MsgService : IMsgService
    {

        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;

        private const string MsgCacheKey = "Msgervice";

        public MsgService(
            IMemoryCache memoryCache,
            ISignal signal
            )
        {
            _memoryCache = memoryCache;
            _signal = signal;

        }
        //获取yesSql.Session,操作数据用的
        private ISession GetSession()
        {
            return ShellScope.Services.GetService<ISession>();
            //using Microsoft.Extensions.DependencyInjection;
        }

        //获取全部
        public async Task<List<Models.Msg>> GetAsync()
        {
            return (await GetMsgList()).Msg;
        }
        //具体，获取全部Msglist
        private async Task<MsgList> GetMsgList()
        {
            MsgList treeList;

            if (!_memoryCache.TryGetValue(MsgCacheKey, out treeList))
            {
                var session = GetSession();

                treeList = await session.Query<MsgList>().FirstOrDefaultAsync();

                if (treeList == null)
                {
                    lock (_memoryCache)
                    {
                        if (!_memoryCache.TryGetValue(MsgCacheKey, out treeList))
                        {
                            treeList = new MsgList();
                            session.Save(treeList);
                            _memoryCache.Set(MsgCacheKey, treeList);
                            _signal.SignalToken(MsgCacheKey);
                        }
                    }
                }
                else
                {
                    _memoryCache.Set(MsgCacheKey, treeList);
                    _signal.SignalToken(MsgCacheKey);
                }
            }

            return treeList;
        }

        public async Task<int> DeleteAsync(Msg tree)
        {
            //获取公众号集合
            var MsgList = await GetMsgList();
            var session = GetSession(); //拿到session
            //获取删除数量
            var count = MsgList.Msg.RemoveAll(x => String.Equals(x.Id, tree.Id));
            //保存到session
            session.Save(MsgList);
            //重新缓存
            _memoryCache.Set(MsgCacheKey, MsgList);
            //更改口令也重新对应缓存
            _signal.SignalToken(MsgCacheKey);

            return count;
        }
        //根据Id获取
        public async Task<Models.Msg> GetByIdAsync(string id)
        {
            return (await GetMsgList())
             .Msg
             .Where(x => String.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase))
             .FirstOrDefault();
        }
        //保存（插入记录）
        public async Task SaveAsync(Models.Msg tree)
        {
            var msgList = await GetMsgList();
            var session = GetSession();

            //根据id判断是否重复 // where using System.Linq;
            var preexisting = msgList.Msg.Where(x => x.Id == tree.Id).FirstOrDefault();

            // it's new? add it
            if (preexisting == null)
            {
                msgList.Msg.Add(tree);
            }
            else // not new: replace it
            {
                var index = msgList.Msg.IndexOf(preexisting);
                msgList.Msg[index] = tree;
            }

            session.Save(msgList);

            _memoryCache.Set(MsgCacheKey, msgList);
            _signal.SignalToken(MsgCacheKey);
        }
    }
}
