using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kolin.REST.Examples.OldFrameWork
{
    public class ResponseMessage
    {
        public int Code { get; set; }
        public string Description { get; set; }
        public object ResponseObject { get; set; }
    }

}
