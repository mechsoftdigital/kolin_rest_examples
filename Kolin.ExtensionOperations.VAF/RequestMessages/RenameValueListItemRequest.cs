using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kolin.ExtensionOperations.VAF
{
    public class RenameValueListItemRequest
    {
        public int ValueListId { get; set; }
        public string ItemId { get; set; }
        public string Name { get; set; }
        public bool IsDisplayID { get; set; }
    }
}
