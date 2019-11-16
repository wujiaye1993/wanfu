using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.AdminMenuTest1.Models
{
    /// <summary>
    /// The list of all the WeChat stored on the system.
    /// </summary>
    public class WeChatMPList
    {
        public int Id { get; set; }
        public List<WeChatMP> WeChatMP { get; set; } = new List<WeChatMP>();
    }
}
