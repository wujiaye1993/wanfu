using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Qwe.WeChatMP.MessageRecord.Services
{
    public interface IMsgService
    {
        /// <summary>
        /// 返回所有
        /// </summary>
        /// <returns></returns>
        Task<List<Models.Msg>> GetAsync();

        /// <summary>
        /// Persist an 
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        Task SaveAsync(Models.Msg tree);
        //Task SaveAsync(WeChatMP tree);

        /// <summary>
        /// Returns an .
        /// </summary>
        Task<Models.Msg> GetByIdAsync(string id);

        /// <summary>
        /// Deletes an 
        /// </summary>
        /// <param name="tree"></param>
        /// <returns>The count of deleted items</returns>
        Task<int> DeleteAsync(Models.Msg tree);


    }
}
