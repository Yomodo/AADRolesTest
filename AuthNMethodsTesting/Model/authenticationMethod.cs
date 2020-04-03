using System;
using System.Collections.Generic;
using System.Text;

namespace AuthNMethodsTesting.Model
{
    public class authenticationMethod
    {
        public string id { get; set; }

        public bool? isUsable { get; set; }

        public string phoneNumber { get; set; }
    }
}
