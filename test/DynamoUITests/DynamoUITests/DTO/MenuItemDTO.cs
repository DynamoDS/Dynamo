using System.Collections.Generic;

namespace DynamoTests.DTO
{
    public class MenuItemDTO : MenuBaseDTO
    {
        public List<MenuItemDTO> SubMenus { get; set; }
    }
}


