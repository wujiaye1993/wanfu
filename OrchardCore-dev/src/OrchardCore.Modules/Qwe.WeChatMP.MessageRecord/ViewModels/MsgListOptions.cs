using System;
using System.Collections.Generic;
using System.Text;

namespace Qwe.WeChatMP.MessageRecord.ViewModels
{
    public class MsgListOptions
    {
        public string SearchKfName { get; set; }
        public string SearchSpeaker { get; set; }
        public DateTime SearchCreateTime { get; set; }
        public DateTime SearchMaxTime { get; set; }
        public DateTime SearchMinTime { get; set; }
    }
}
