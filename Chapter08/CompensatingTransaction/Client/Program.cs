namespace Client
{
    using System;
    using System.Configuration;
    using System.Threading.Tasks;

    using Nito.AsyncEx;

    using Workflow;

    class Program
    {
        private static readonly string manageKey = ConfigurationManager.AppSettings[nameof(manageKey)];

        private static readonly string manageKeyName = ConfigurationManager.AppSettings[nameof(manageKeyName)];

        private static readonly string namespaceAddress = ConfigurationManager.AppSettings[nameof(namespaceAddress)];

        static void Main(string[] args)
        {
            AsyncContext.Run(() => MainAsync(args));
            Console.ReadKey();
        }

        private static async Task MainAsync(string[] args)
        {
            var host = new Host();
            await host.Run(namespaceAddress, manageKeyName, manageKey);
            var booking = new Booking
                {
                    TravellerName = ReadInputForMessage("Enter traveler's name:"),
                    Destination = ReadInputForMessage("Enter destination:"),
                    CreditLimit = Convert.ToDecimal(ReadInputForMessage("Enter credit limit:"))
                };
            Console.WriteLine($"Wallet Balance: {booking.CreditLimit}");
            Console.WriteLine($"Booking process initiated for traveler {booking.TravellerName}");
            await host.BookTravel(booking);
            Console.WriteLine("Clearing workflow. Please wait.");
            await host.DeleteQueues();
            Console.WriteLine("Booking completed. Press any key to exit.");
        }

        private static string ReadInputForMessage(string command)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(command);
            Console.ResetColor();
            return Console.ReadLine();
        }
    }
}