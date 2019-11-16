using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.AdminMenuTest1.Models
{
    public class WeChatMP
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("n");
        public string AppId { get; set; }
        public string Secret { get; set; }
        public string Name { get; set; }

        public bool Enabled { get; set; } = true; //公众号起效标志位
    }
}
