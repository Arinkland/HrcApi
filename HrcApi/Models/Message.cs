using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HrcApi.Models
{
    public class Message
    {
        public int MyProperty { get; set; } = 200;
        public object data { get; set; }
        public string ErrorMessage { get; set; }
    }
}