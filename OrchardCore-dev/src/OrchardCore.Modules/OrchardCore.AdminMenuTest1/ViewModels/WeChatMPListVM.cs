using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.AdminMenuTest1.ViewModels
{
    public class WeChatMPListVM
    {
        public IList<AdminMenuEntry> WeChatMP { get; set; }
        public WeChatMPListOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class AdminMenuEntry
    {
        public Models.WeChatMP WeChatMP { get; set; }
        public bool IsChecked { get; set; }
    }
}
