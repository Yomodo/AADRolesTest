using System;
using System.Collections.Generic;
using System.Text;

namespace AuthNMethodsTesting.Model
{
    public class phoneAuthenticationMethod
    {
        public string phoneNumber { get; set; }

        public authenticationPhoneType phoneType { get; set; }

        public authenticationMethodSignInState smsSignInState { get; set; }
    }
}
