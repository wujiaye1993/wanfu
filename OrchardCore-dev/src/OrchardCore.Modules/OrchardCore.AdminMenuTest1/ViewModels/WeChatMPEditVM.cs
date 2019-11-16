using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OrchardCore.AdminMenuTest1.ViewModels
{
    public class WeChatMPEditVM
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public string AppId { get; set; }
        [Required]
        public string Secret { get; set; }
    }
}
