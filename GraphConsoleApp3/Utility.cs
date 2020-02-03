extern alias BetaLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Security.Cryptography;
using Beta = BetaLib.Microsoft.Graph;

namespace AppRolesTesting
{
    public static class Utility
    {
        /// <summary>
        /// Create a password that can be used as an application key
        /// </summary>
        /// <returns></returns>
        private static string ComputePassword()
        {
            AesManaged aesManaged = new AesManaged()
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.Zeros,
                BlockSize = 128,
                KeySize = 256
            };

            aesManaged.GenerateKey();
            return Convert.ToBase64String(aesManaged.Key);
        }

        /// <summary>
        /// Generates a key for an Azure AD application
        /// </summary>
        /// <param name="fromDate">From date.</param>
        /// <param name="durationInYears">The key duration in years.</param>
        /// <param name="password">The password.</param>
        /// <remarks>https://www.sabin.io/blog/adding-an-azure-active-directory-application-and-key-using-powershell/</remarks>
        /// <returns></returns>
        private static Beta.PasswordCredential CreateAppKey(DateTime fromDate, int durationInYears, string password)
        {
            Beta.PasswordCredential passwordCredential = new Beta.PasswordCredential()
            {
                StartDateTime = fromDate,
                EndDateTime = fromDate.AddYears(durationInYears),
                SecretText = password,
                KeyId = Guid.NewGuid()
            };

            return passwordCredential;
        }
    }
}
