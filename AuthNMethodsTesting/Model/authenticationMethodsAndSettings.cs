using System;
using System.Collections.Generic;
using System.Text;

namespace AuthNMethodsTesting.Model
{
    public class authenticationMethodsAndSettings
    {
        public List<authenticationMethod> methods { get; set; }

        public authenticationMethod defaultAuthenticationMethod { get; set; }
    }
}
