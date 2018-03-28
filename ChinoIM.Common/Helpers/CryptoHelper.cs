using BCrypt;
using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ChinoIM.Common.Helpers
{
    public class CryptoHelper
    {
        private static MD5 md5Hasher = MD5.Create();
        private static UTF8Encoding utf8Encoding = new UTF8Encoding();
        private static NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;

        public static string EncryptString(string message, byte[] key, ref string iv)
        {
            string encrypted = null;
            using (var rj = new RijndaelManaged())
            {
                rj.Key = key;
                rj.BlockSize = 256;
                rj.Mode = CipherMode.CBC;

                if (iv != null)
                    rj.IV = Convert.FromBase64String(iv);
                else
                {
                    rj.GenerateIV();
                    iv = Convert.ToBase64String(rj.IV);
                }

                try
                {
                    var ms = new MemoryStream();

                    using (var cs = new CryptoStream(ms, rj.CreateEncryptor(key, rj.IV), CryptoStreamMode.Write))
                    {
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(message);
                            sw.Close();
                        }
                        cs.Close();
                    }
                    byte[] encoded = ms.ToArray();
                    encrypted = Convert.ToBase64String(encoded);

                    ms.Close();
                }
                finally
                {
                    rj.Clear();
                }
            }

            return encrypted;
        }

        public static string GetMd5(string path)
        {
            lock (md5Hasher)
            {
                if (!File.Exists(path)) return string.Empty;

                try
                {
                    using (var s = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        byte[] data = md5Hasher.ComputeHash(s);

                        var sBuilder = new StringBuilder();

                        for (int i = 0; i < data.Length; i++)
                            sBuilder.Append(data[i].ToString("x2"));

                        return sBuilder.ToString();
                    }
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
        }

        public static string GetMd5String(string instr)
        {
            return GetMd5String(utf8Encoding.GetBytes(instr));
        }

        public static string GetMd5String(byte[] data)
        {
            lock (md5Hasher)
            {
                try
                {
                    // Convert the input string to a byte array and compute the hash.
                    data = md5Hasher.ComputeHash(data);
                }
                catch (Exception)
                {
                    return "fail";
                }
            }

            char[] str = new char[data.Length * 2];
            for (int i = 0; i < data.Length; i++)
                data[i].ToString("x2", nfi).CopyTo(0, str, i * 2, 2);

            return new string(str);
        }

        public static byte[] GetMd5ByteArray(byte[] data)
        {
            lock (md5Hasher)
            {
                try
                {
                    // Convert the input string to a byte array and compute the hash.
                    data = md5Hasher.ComputeHash(data);
                }
                catch (Exception)
                {
                    return new byte[0];
                }
            }

            return data;
        }

        public static byte[] GetMd5ByteArrayString(string instr)
        {

            byte[] data;

            lock (md5Hasher)
            {
                try
                {
                    // Convert the input string to a byte array and compute the hash.
                    data = md5Hasher.ComputeHash(utf8Encoding.GetBytes(instr));
                }
                catch (Exception)
                {
                    return null;
                }
            }
            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            //StringBuilder sBuilder = new StringBuilder();

            return data;

        }

        public static string BCryptHash(string text)
        {
            return BCryptHelper.HashPassword(text, BCryptHelper.GenerateSalt());
        }

        public static bool BCryptVerify(string text, string hash)
        {
            return BCryptHelper.CheckPassword(text, hash);
        }
    }
}
