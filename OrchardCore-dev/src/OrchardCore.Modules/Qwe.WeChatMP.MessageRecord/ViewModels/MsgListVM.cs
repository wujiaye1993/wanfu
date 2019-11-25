using System;
using System.Collections.Generic;
using System.Text;

namespace Qwe.WeChatMP.MessageRecord.ViewModels
{
    public class MsgListVM
    {
        public IList<AdminMenuEntry> Msg { get; set; }
        public MsgListOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }
    public class AdminMenuEntry
    {
        public Models.Msg Msg { get; set; }
        public bool IsChecked { get; set; }
    }
}
