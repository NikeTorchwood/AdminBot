using AdminBot.Repository;
using AdminBot.Services;
using Telegram.Bot;

namespace AdminBot
{
    public  class Program
    {
        public static void Main()
        {
            var sqlProvider = new SqlConnectionProvider(connectionString);
            var bot = new TelegramBotClient(token);
            var telegramService = new TelegramService(bot, sqlProvider);
            telegramService.StartListening();

            Console.ReadKey();
        }
    }
}