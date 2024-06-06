using System;

namespace HostApp
{
    class Program
    {
        static void Main()
        {
            var service = new CryptoService();
            Console.WriteLine("Handshake complete");
            var line = "";
            while(!string.IsNullOrEmpty(line = Console.ReadLine()))
            {
                service.WriteInput(line);
            }
            
            service.Terminate();
        }
    }
}
