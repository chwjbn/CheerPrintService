using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CheerLib.Lib
{
    public class EncryptHelper
    {

        public static string MD5Encrypt(string input, Encoding encode)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] t = md5.ComputeHash(encode.GetBytes(input));
            StringBuilder sb = new StringBuilder(32);
            for (int i = 0; i < t.Length; i++)
            {
                sb.Append(t[i].ToString("x").PadLeft(2, '0'));
            }
                
            return sb.ToString();
        }

        public static string DES3Encrypt(string data, string key)
        {
            key = MD5Encrypt(key, Encoding.UTF8);
            
            var ivKey = key.Substring(24);

            key = key.Substring(0, 24);

            var DES = new TripleDESCryptoServiceProvider();
            DES.Key = Encoding.UTF8.GetBytes(key);
            DES.Mode = CipherMode.CBC;
            DES.Padding = PaddingMode.PKCS7;
            DES.IV = Encoding.UTF8.GetBytes(ivKey);

            var DESEncrypt = DES.CreateEncryptor();

            string result = string.Empty;

            try
            {
                var mBuffer = Encoding.UTF8.GetBytes(data);

                var xBuffer = DESEncrypt.TransformFinalBlock(mBuffer, 0, mBuffer.Length);

                result = Convert.ToBase64String(xBuffer);
            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }

            return result;
        }

        public static string DES3Decrypt(string data, string key)
        {

            key = MD5Encrypt(key, Encoding.UTF8);
            
            var ivKey = key.Substring(24);

            key = key.Substring(0, 24);

            var DES = new TripleDESCryptoServiceProvider();
            DES.Key = Encoding.UTF8.GetBytes(key);
            DES.Mode = CipherMode.CBC;
            DES.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
            DES.IV = Encoding.UTF8.GetBytes(ivKey);

            var DESDecrypt = DES.CreateDecryptor();

            string result = string.Empty;

            try
            {
                var mBuffer = Convert.FromBase64String(data);

                var xBuffer = DESDecrypt.TransformFinalBlock(mBuffer, 0, mBuffer.Length);

                result = Encoding.UTF8.GetString(xBuffer);
            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }
            return result;
        }

    }
}
