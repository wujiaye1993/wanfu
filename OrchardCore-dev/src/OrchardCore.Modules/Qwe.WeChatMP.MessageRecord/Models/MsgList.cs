using System;
using System.Collections.Generic;
using System.Text;

namespace Qwe.WeChatMP.MessageRecord.Models
{
    public class MsgList
    {
        public int Id { get; set; }
        public List<Msg> Msg { get; set; } = new List<Msg>();
    }
}
