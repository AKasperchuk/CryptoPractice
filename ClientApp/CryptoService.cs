using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace ClientApp
{
    public class CryptoService
    {
        private RSACryptoServiceProvider rsa;
        private byte[] aesKey;
        private int aesKeyCount = 0;
        private int rsaKeyCount = 0;

        public CryptoService()
        {
            WriteRsaKeys();
            GetAesKey();
        }

        private void WriteRsaKeys()
        {
            rsa = new RSACryptoServiceProvider(2048);
            var publicKey = rsa.ExportSubjectPublicKeyInfo();
            File.WriteAllBytes("D:/NIX/Инфобез/Криптография практика/CryptoPractice/Files/rsa.pem", publicKey);
            var privateKey = rsa.ExportRSAPrivateKey();
            File.AppendAllText("D:/NIX/Инфобез/Криптография практика/CryptoPractice/Files/rsa_private.txt", Convert.ToBase64String(privateKey) + Environment.NewLine);
        }

        private void GetAesKey()
        {
            string aesKeyFilePath = "D:/NIX/Инфобез/Криптография практика/CryptoPractice/Files/aes.txt";
            string rsaPrivateKeysFilePath = "D:/NIX/Инфобез/Криптография практика/CryptoPractice/Files/rsa_private.txt";

            while (!File.Exists(aesKeyFilePath))
            {
                Console.WriteLine("Waiting for the AES key file to be created...");
                Thread.Sleep(1000); 
            }

            var data = File.ReadAllLines(aesKeyFilePath);
            var privateKeys = File.ReadAllLines(rsaPrivateKeysFilePath);
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKeys[rsaKeyCount]), out _);
            rsaKeyCount++;
            aesKey = rsa.Decrypt(Convert.FromBase64String(data[aesKeyCount]), RSAEncryptionPadding.Pkcs1);
            aesKeyCount++;
        }

        private string DecryptAes(string text)
        {
            var fullCipher = Convert.FromBase64String(text);

            using Aes aesAlg = Aes.Create();
            aesAlg.Key = aesKey;

            byte[] iv = new byte[aesAlg.BlockSize / 8];
            byte[] cipherText = new byte[fullCipher.Length - iv.Length];

            Array.Copy(fullCipher, iv, iv.Length);
            Array.Copy(fullCipher, iv.Length, cipherText, 0, cipherText.Length);

            aesAlg.IV = iv;

            var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using var msDecrypt = new MemoryStream(cipherText);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            return srDecrypt.ReadToEnd();
        }

        public string[] ReadFileData()
        {
            string filePath = "D:/NIX/Инфобез/Криптография практика/CryptoPractice/Files/text.txt";
            while (!File.Exists(filePath))
            {
                Console.WriteLine("Waiting for the text file to be created...");
                Thread.Sleep(1000);
            }

            var text = File.ReadAllLines(filePath);
            var resList = new List<string>();
            for (int i = 0; i < text.Length; i++)
            {
                if (!string.IsNullOrEmpty(text[i]))
                {
                    if (text[i] == "#" )
                    {
                        if (i < text.Length - 1)
                            GetAesKey();
                    }
                    else
                    {
                        resList.Add(DecryptAes(text[i]));
                    }
                }
            }
            return resList.ToArray();
        }
    }
}
