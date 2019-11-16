using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.AdminMenuTest1.Models;

namespace OrchardCore.AdminMenuTest1.Services
{
    public interface IWeChatMPService
    {

        /// <summary>
        /// 返回所有公众号
        /// </summary>
        /// <returns></returns>
        Task<List<Models.WeChatMP>> GetAsync();

        /// <summary>
        /// Persist an admin menu
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        Task SaveAsync(Models.WeChatMP tree);
        //Task SaveAsync(WeChatMP tree);

        /// <summary>
        /// Returns an admin menu.
        /// </summary>
        Task<Models.WeChatMP> GetByIdAsync(string id);

        /// <summary>
        /// Deletes an admin menu
        /// </summary>
        /// <param name="tree"></param>
        /// <returns>The count of deleted items</returns>
        Task<int> DeleteAsync(Models.WeChatMP tree);

        /// <summary>
        /// 检查公众号是否可以绑定
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        Task<CheckAccess> CheckAccessAsync(string appId, string secret);

        public CheckAccess CheckAccess(string appId, string secret);
    }
}
