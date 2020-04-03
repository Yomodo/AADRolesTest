using System;
using System.Collections.Generic;
using System.Text;

namespace AuthNMethodsTesting.Model
{
    public enum authenticationMethodSignInState
    {
        notSupported,
        notAllowedByPolicy,
        notConfigured,
        phoneNumberNotUnique,
        ready,
        unknownFutureValue
    }
}
