using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamoTests.DTO
{
    public class GuidedTourInfoDTO
    {
        public string Name { get; set; }
        public List<PopupInfoDTO> Popups { get; set; }
    }
}
