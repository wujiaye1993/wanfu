using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Qwe.WeChatMP.MessageRecord.ViewModels
{
    public class MsgCreateVM
    {
        [Required]
        public string KfName { get; set; }
        [Required]
        public DateTime CreateTime { get; set; }
        [Required]
        public string ToUserName { get; set; }
        [Required]
        public string FromUserName { get; set; }
        [Required]
        public string Speaker { get; set; }
        [Required]
        public string Content { get; set; }
    }
}
