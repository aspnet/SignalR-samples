using System;
using System.Threading.Tasks;

namespace ChatClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                await new ChatClient("test").Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"The client stopped with an error: {ex}");
            }
        }
    }
}
