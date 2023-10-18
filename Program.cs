using AdminBot.Repository;
using AdminBot.Services;
using Telegram.Bot;
using System.Configuration;

namespace AdminBot
{
    public class Program
    {
        public static void Main()
        {
            //6410857523 - admin id
            var token = ConfigurationManager.ConnectionStrings["debugToken"].ConnectionString;
            var connectionString = ConfigurationManager.ConnectionStrings["debugDB"].ConnectionString;
            var sqlProvider = new SqlConnectionProvider(connectionString);
            var bot = new TelegramBotClient(token);
            var telegramService = new TelegramService(bot, sqlProvider);
            telegramService.StartListening();

            Console.ReadKey();
        }
    }
}