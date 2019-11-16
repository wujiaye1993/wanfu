using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OrchardCore.AdminMenuTest1.ViewModels
{
    public class WeChatCreateVm
    {
        [Required]
        public string AppId { get; set; }
        [Required]
        public string Secret { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
