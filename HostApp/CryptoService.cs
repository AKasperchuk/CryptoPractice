using System;
using System.IO;
using System.Security.Cryptography;

namespace HostApp
{
    public class CryptoService
    {
        private readonly string _dirPath = "../../../../Files/";
        private readonly byte[] _aesKey = new byte[32];
        private byte[] _rsaKey;

        public CryptoService()
        {
            using var provider = new RNGCryptoServiceProvider();
            provider.GetBytes(_aesKey);
            GetRsaKey();
            WriteAesKey();
        }

        private void GetRsaKey()
        {
            _rsaKey = File.ReadAllBytes($"{_dirPath}/rsa.pem");
        }

        private void WriteAesKey()
        {
            using var rsa = new RSACryptoServiceProvider() ;
            rsa.ImportSubjectPublicKeyInfo(_rsaKey, out _);
            var encrypted = rsa.Encrypt(_aesKey, RSAEncryptionPadding.Pkcs1);
            var data = Convert.ToBase64String(encrypted);
            File.AppendAllText($"{_dirPath}/aes.txt", data + Environment.NewLine);
        }

        private string EncryptWithAes(string plainText)
        {
            using var aesAlg = Aes.Create();
            aesAlg.Key = _aesKey;
            aesAlg.GenerateIV();

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using var msEncrypt = new MemoryStream();
            msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length); 
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(plainText);
            }
            return Convert.ToBase64String(msEncrypt.ToArray());
        }

        public void WriteInput(string input)
        {
            var encrypted = EncryptWithAes(input);
            File.AppendAllText($"{_dirPath}/text.txt", encrypted + Environment.NewLine);
        }

        public void Terminate()
        {
            File.AppendAllText($"{_dirPath}/text.txt", "#" + Environment.NewLine);
        }
    }
}
