using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace WebApplication
{
    public class PasswordManager
    {
        #region Constants
        private static readonly Encoding InternalEncoding = Encoding.Default;
        /// <summary>
        /// The timeout expressed in seconds
        /// </summary>
        public const int ExpireTimeout = 30;
        #endregion

        public static string GetTempPassword(string userID, DateTime currentDateTime, out DateTime? expirationDateTime)
        {
            if (String.IsNullOrWhiteSpace(userID))
            {
                throw new ArgumentNullException(nameof(userID));
            }

            var utcDateTime = currentDateTime.ToUniversalTime();
            expirationDateTime = currentDateTime.AddSeconds(ExpireTimeout);

            string passFirstPart = PasswordManager.GeneratePasswordFirstPart(userID);
            if (String.IsNullOrWhiteSpace(passFirstPart))
            {
                throw new InvalidOperationException();
            }

            var tStampHash = PasswordManager.InternalEncoding.GetBytes(utcDateTime.ToString());
            string passSecondPart = Convert.ToBase64String(tStampHash);
            if (String.IsNullOrWhiteSpace(passSecondPart))
            {
                throw new InvalidOperationException();
            }

            return $"{passFirstPart}{passSecondPart}";
        }

        public static bool LoginUser(string userID, string password)
        {
            if (String.IsNullOrWhiteSpace(userID) || String.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            var utcDateTime = DateTime.Now.ToUniversalTime().ToString();

            var tStampHash = PasswordManager.InternalEncoding.GetBytes(utcDateTime);
            int datePartLength = Convert.ToBase64String(tStampHash).Length;
            if (datePartLength < 0 || datePartLength > password.Length)
            {
                return false;
            }

            string passFirstPart = password.Substring(0, password.Length - datePartLength);
            string passwordFirstPartStandard = PasswordManager.GeneratePasswordFirstPart(userID);
            if (String.IsNullOrWhiteSpace(passFirstPart) || String.IsNullOrWhiteSpace(passwordFirstPartStandard))
            {
                throw new InvalidOperationException();
            }

            if (passFirstPart != passwordFirstPartStandard)
            {
                return false;
            }

            string passSecondPart = password.Substring(password.Length - datePartLength);
            if (String.IsNullOrWhiteSpace(passSecondPart))
            {
                return false;
            }

            var secondPartBytes = Convert.FromBase64String(passSecondPart);
            string tStamp = PasswordManager.InternalEncoding.GetString(secondPartBytes);
            DateTime originalDate = DateTime.Parse(tStamp);

            if (DateTime.Now.ToUniversalTime() > originalDate.AddSeconds(PasswordManager.ExpireTimeout))
            {
                return false;
            }

            return true;
        }

        #region Helper methods
        private static string GeneratePasswordFirstPart(string userID)
        {
            if (String.IsNullOrWhiteSpace(userID))
            {
                throw new ArgumentNullException(nameof(userID));
            }

            var userIDBytes = PasswordManager.InternalEncoding.GetBytes(userID);

            //Can be improved. For ex: an app installation ID, or database stored value
            var secret = PasswordManager.InternalEncoding.GetBytes("secret"); 

            List<byte> bytesList = new List<byte>();
            bytesList.AddRange(userIDBytes);
            bytesList.AddRange(secret);

            var hash = PasswordManager.GenerateHash(bytesList.ToArray());
            string passwordFirstPart = Convert.ToBase64String(hash);

            return passwordFirstPart;
        }

        private static byte[] GenerateHash(byte[] input)
        {
            using (var hashGenerator = MD5.Create())
            {
                byte[] result = hashGenerator.ComputeHash(input);
                return result;
            }
        }
        #endregion
    }
}
