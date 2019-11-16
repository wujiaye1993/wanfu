using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.AdminMenuTest1.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenuTest1
{
    public class Permissions : IPermissionProvider
    {

        public static readonly Permission ManageWeChatMP = new Permission("ManageWeChatMP", "ManageWeChatMP");

        public static readonly Permission ViewWeChatMPAll = new Permission("ViewWeChatMPAll", "ViewWeChatMPAll", new[] { ManageWeChatMP });

        private static readonly Permission ViewWeChatMP = new Permission("ViewWeChatMP{0}", "ViewWeChatMP - {0}", new[] { ManageWeChatMP, ViewWeChatMPAll });

        private readonly IWeChatMPService _weChatMPServic;

        public Permissions(IWeChatMPService weChatMPServic)
        {
            _weChatMPServic = weChatMPServic;
        }

        //IPermissionProvider实现方法，获取权限
        public async Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            var list = new List<Permission> { ManageWeChatMP, ViewWeChatMPAll };

            foreach (var adminMenu in await _weChatMPServic.GetAsync())
            {
                list.Add(CreatePermissionForAdminMenu(adminMenu.Name));
            }

            return list;
        }
        public static Permission CreatePermissionForAdminMenu(string name)
        {
            return new Permission(
                    String.Format(ViewWeChatMP.Name, name),
                    String.Format(ViewWeChatMP.Description, name),
                    ViewWeChatMP.ImpliedBy
                );
        }


        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageWeChatMP }
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] { ManageWeChatMP }
                }
            };
        }


    }
}
