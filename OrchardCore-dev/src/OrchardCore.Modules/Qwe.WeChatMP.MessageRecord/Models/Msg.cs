using System;
using System.Collections.Generic;
using System.Text;

namespace Qwe.WeChatMP.MessageRecord.Models
{
    /*
    KfName
    CreateTime
    ToUserName
    FromUserName
    Speaker
    Content
             */
    public class Msg
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("n");
        public string KfName { get; set; }
        public DateTime CreateTime { get; set; }
        public string ToUserName { get; set; }
        public string FromUserName { get; set; }
        public string Speaker { get; set; }
        public string Content { get; set; }

    }
}
