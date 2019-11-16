using System;
using System.Collections.Generic;
using System.Text;

namespace Qwe.WeChatMP.Message.Models
{
    public class MessageBody
    {
        //客服账号
        public string KefuIdAccount { get; set; }
        //公众号
        public string WeChatMPId { get; set; }
        public string CustomerId { get; set; }
        //数据类型
        public int Type { get; set; }
        //开发者微信号
        public string ToUserName { get; set; }
        //发送方账号
        public string FromUserName { get; set; }
        //创建时间
        public string CreateTime { get; set; }
        //消息内容
        public string Content { get; set; }

        public string UserName { get; set; }

        //消息id，64位整型
        public long MsgId { get; set; }
        //public string CustomMessageFromWeixin { get; set; }
    }
}
