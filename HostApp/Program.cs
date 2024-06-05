using System;

namespace HostApp
{
    class Program
    {
        static void Main()
        {
            var service = new CryptoService();
            // service.EncryptAesKey();
            Console.WriteLine("Handshake complete");
            string line = "";
            while(!string.IsNullOrEmpty(line = Console.ReadLine()))
            {
                service.WriteInput(line);
            }
            
            service.Terminate();
        }
    }
}