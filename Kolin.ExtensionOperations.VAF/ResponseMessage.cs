using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kolin.ExtensionOperations.VAF
{
    public class ResponseMessage
    {
        public int Code { get; set; }
        public string Description { get; set; }
        public object ResponseObject { get; set; }
    }

}
