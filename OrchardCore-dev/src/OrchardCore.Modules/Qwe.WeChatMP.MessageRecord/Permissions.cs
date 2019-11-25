using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;
using Qwe.WeChatMP.MessageRecord.Services;

namespace Qwe.WeChatMP.MessageRecord
{
    public class Permissions : IPermissionProvider
    {

        public static readonly Permission ManageMessageRecord = new Permission("ManageMessageRecord", "ManageMessageRecord");

        public static readonly Permission ViewMessageRecordAll = new Permission("ViewMessageRecordAll", "ViewManageMessageRecordAll", new[] { ManageMessageRecord });

        private static readonly Permission ViewMessageRecord = new Permission("ViewMessageRecord{0}", "ViewMessageRecord - {0}", new[] { ManageMessageRecord, ViewMessageRecordAll });

        private readonly IMsgService _msgService;

        public Permissions(IMsgService msgService)
        {
            _msgService = msgService;
        }

        //IPermissionProvider实现方法，获取权限
        public async Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            var list = new List<Permission> { ManageMessageRecord, ViewMessageRecordAll };

            foreach (var adminMenu in await _msgService.GetAsync())
            {
                list.Add(CreatePermissionForAdminMenu(adminMenu.KfName));
            }

            return list;
        }
        public static Permission CreatePermissionForAdminMenu(string name)
        {
            return new Permission(
                    String.Format(ViewMessageRecord.Name, name),
                    String.Format(ViewMessageRecord.Description, name),
                    ViewMessageRecord.ImpliedBy
                );
        }


        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageMessageRecord }
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] { ManageMessageRecord }
                }
            };
        }


    }
}
