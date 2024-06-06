using System;

namespace ClientApp
{
    class Program
    {
        static void Main()
        {
            var service = new CryptoService();
            Console.WriteLine("Handshake complete");
            Try:
            try
            {
                var text = service.ReadFileData();
                foreach (var line in text)
                {
                    Console.WriteLine(line);
                }
            }
            catch (Exception ex)
            {
                goto Try;
            }
        }
    }
}
