using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ClientApp
{
    public class CryptoService
    {
        private const string _dirPath = "../../../../Files/";
        private RSACryptoServiceProvider _rsa;
        private byte[] _aesKey;
        private int _aesKeyCount;
        private int _rsaKeyCount;
        private string connectionString;

        public CryptoService()
        {
            GetEnv();
            WriteRsaKeys();
            GetAesKey();
        }

        private void GetEnv()
        {
            DotNetEnv.Env.TraversePath().Load();
            connectionString = DotNetEnv.Env.GetString("ConnectionString");
        }

        private void WriteRsaKeys()
        {
            _rsa = new RSACryptoServiceProvider(2048);
            var publicKey = _rsa.ExportSubjectPublicKeyInfo();
            File.WriteAllBytes($"{_dirPath}/rsa.pem", publicKey);
            var privateKey = _rsa.ExportRSAPrivateKey();
            using IDbConnection connection = new SqlConnection(connectionString);
            connection.Execute("INSERTKEY", new {key = Convert.ToBase64String(privateKey)}, commandType: CommandType.StoredProcedure);
            // File.AppendAllText($"{_dirPath}/rsa_private.txt", Convert.ToBase64String(privateKey) + Environment.NewLine);
        }

        private string[] GetRsaKeys()
        {
            using IDbConnection connection = new SqlConnection(connectionString);
            return (connection.Query<string>("GetAllKeys",  commandType: CommandType.StoredProcedure)).ToArray();
        }
        
        private void GetAesKey()
        {
            var aesKeyFilePath = $"{_dirPath}/aes.txt";
            var rsaPrivateKeysFilePath = $"{_dirPath}/rsa_private.txt";

            while (!File.Exists(aesKeyFilePath))
            {
                Console.WriteLine("Waiting for the AES key file to be created...");
                Thread.Sleep(1000); 
            }

            var data = File.ReadAllLines(aesKeyFilePath);
            var privateKeys = GetRsaKeys();
            _rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKeys[_rsaKeyCount]), out _);
            _rsaKeyCount++;
            _aesKey = _rsa.Decrypt(Convert.FromBase64String(data[_aesKeyCount]), RSAEncryptionPadding.Pkcs1);
            _aesKeyCount++;
        }

        private string DecryptAes(string text)
        {
            var fullCipher = Convert.FromBase64String(text);

            using var aesAlg = Aes.Create();
            aesAlg.Key = _aesKey;

            var iv = new byte[aesAlg.BlockSize / 8];
            var cipherText = new byte[fullCipher.Length - iv.Length];

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
            var filePath = $"{_dirPath}/text.txt";
            while (!File.Exists(filePath))
            {
                Console.WriteLine("Waiting for the text file to be created...");
                Thread.Sleep(1000);
            }

            var text = File.ReadAllLines(filePath);
            var resList = new List<string>();
            for (var i = 0; i < text.Length; i++)
            {
                if (string.IsNullOrEmpty(text[i])) continue;
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
            return resList.ToArray();
        }
    }
}
