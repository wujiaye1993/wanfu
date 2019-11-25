using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Qwe.WeChatMP.Message.Services
{
    public interface IMessageRecord
    {
        /// <summary>
        /// 返回所有消息记录
        /// </summary>
        /// <returns></returns>
        Task<List<Models.MessageBody>> GetAsync();

        /// <summary>
        /// Persist an MessageRecord
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        Task SaveAsync(Models.MessageBody tree);
        //Task SaveAsync(WeChatMP tree);

        /// <summary>
        /// Returns an MessageRecord
        /// </summary>
        Task<Models.MessageBody> GetByIdAsync(string id);

        /// <summary>
        /// Deletes an MessageRecord
        /// </summary>
        /// <param name="tree"></param>
        /// <returns>The count of deleted items</returns>
        Task<int> DeleteAsync(Models.MessageBody tree);
    }
}
