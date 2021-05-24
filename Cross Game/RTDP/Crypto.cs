using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RTDP
{
    public class Crypto
    {
        private static readonly string separator = "\r\n";
        /// <summary>
        /// Genera el hash de la cadena introducida con el algoritmo sha256.
        /// </summary>
        /// <param name="s">Cadena a la que se le generará el hash</param>
        /// <returns>Devuelve la cadena hash representada en caracteres hexadecimales.</returns>
        public static string CreateSHA256(string s)
        {
            StringBuilder sb = new StringBuilder();
            using (SHA256Managed sha256 = new SHA256Managed())
            {
                byte[] hashBytes = sha256.ComputeHash(GetBytes(s));

                for (int i = 0; i < hashBytes.Length; i++)
                    sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
        /// <summary>
        /// Escribe la información introducida en un fichero y la encripta con la clave proporcionada.
        /// </summary>
        /// <param name="filePath">Ruta al fichero donde se guardará la información.</param>
        /// <param name="key">Clave para encriptar</param>
        /// <param name="data">Información a encriptar en el fichero.</param>
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
        /// <summary>
        /// Desencripta el fichero introducido con la clave proporcionada y devuelve su información.
        /// </summary>
        /// <param name="filePath">Ruta al fichero donde está la información</param>
        /// <param name="key">Clave que se usará para la desencriptación.</param>
        /// <returns>Devuelve un array de string con toda la información desencriptada.</returns>
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

        public static byte[] Encrypt(byte[] data, int dataSize, byte[] key)
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = 128;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.Zeros;

                aes.Key = key;

                aes.IV = new byte[aes.IV.Length];

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    return PerformCryptography(data, dataSize, encryptor);
                }
            }
        }

        public static byte[] Decrypt(byte[] data, int dataSize, byte[] key)
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = 128;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.Zeros;

                aes.Key = key;
                aes.IV = new byte[aes.IV.Length];

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    return PerformCryptography(data, dataSize, decryptor);
                }
            }
        }

        private static byte[] PerformCryptography(byte[] data, int dataSize, ICryptoTransform cryptoTransform)
        {
            using (var ms = new MemoryStream())
            using (var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(data, 0, dataSize);
                cryptoStream.FlushFinalBlock();

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Codifica una cadena a binario.
        /// </summary>
        /// <param name="s">Cadena a obtener los bytes.</param>
        /// <returns>Devuelve un array de bytes que representa cara carácter de la cadena como un carácter ASCII.</returns>
        public static byte[] GetBytes(string s) => Encoding.ASCII.GetBytes(s);

        public static string GetString(byte[] data, int dataSize) => Encoding.ASCII.GetString(data, 0, dataSize);
        
    }
}
