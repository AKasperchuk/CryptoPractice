using System;
using System.Linq;
using System.Threading;

namespace ClientApp
{
    class Program
    {
        static void Main(string[] args)
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
                // var text = new string[] { };
                // while (true)
                // {
                //     if (!Enumerable.SequenceEqual(text, service.ReadFileData()))
                //     {
                //         text = service.ReadFileData();
                //         foreach (var line in text)
                //         {
                //             Console.WriteLine(line);
                //         }
                //
                //         Thread.Sleep(5000);
                //     }
                // }
            }
            catch (Exception ex)
            {
                goto Try;
            }
        }
    }
}