using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Cross_Game.DataManipulation
{
    class Crypto
    {
        private static readonly string separator = "\r\n";

        public static string CreateSHA256(string s)
        {
            StringBuilder sb = new StringBuilder();
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = new SHA256Managed().ComputeHash(GetBytes(s));

                for (int i = 0; i < hashBytes.Length; i++)
                    sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static void WriteData(string filePath, byte[] key, string[] data)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;

                    byte[] iv = aes.IV;
                    fileStream.Write(iv, 0, iv.Length);

                    using (CryptoStream cryptoStream = new CryptoStream(fileStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        using (StreamWriter encryptWriter = new StreamWriter(cryptoStream))
                            foreach (string s in data)
                                encryptWriter.WriteLine(s);
                }
        }

        public static string[] ReadData(string filePath, byte[] key)
        {
            string data;

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                using (Aes aes = Aes.Create())
                {
                    byte[] iv = new byte[aes.IV.Length];
                    int numBytesToRead = aes.IV.Length;
                    int numBytesRead = 0;

                    while (numBytesToRead > 0)
                    {
                        int n = fileStream.Read(iv, numBytesRead, numBytesToRead);
                        if (n == 0) break;

                        numBytesRead += n;
                        numBytesToRead -= n;
                    }

                    using (CryptoStream cryptoStream = new CryptoStream(fileStream, aes.CreateDecryptor(key, iv), CryptoStreamMode.Read))
                        using (StreamReader decryptReader = new StreamReader(cryptoStream))
                            data = decryptReader.ReadToEnd();
                }

            return data.Split(new string[] { separator }, StringSplitOptions.None);
        }

        public static byte[] GetBytes(string s) => Encoding.ASCII.GetBytes(s);

        public static string GetString(byte[] bytes, int index, int count) => Encoding.ASCII.GetString(bytes, index, count);
    }
}
