using AdminBot.Repository;
using AdminBot.Services;
using Telegram.Bot;

namespace AdminBot
{
    public  class Program
    {
        public static void Main()
        {
            //6410857523 - admin id
            var token = "6206880800:AAEJhQglpNQApcq0w0wmJR8IgnI_QXmMKvM";
            var connectionString =
                "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\ntwdc\\source\\repos\\AdminBot\\SQLRepository.mdf;Integrated Security=True";
            var sqlProvider = new SqlConnectionProvider(connectionString);
            var bot = new TelegramBotClient(token);
            var telegramService = new TelegramService(bot, sqlProvider);
            telegramService.StartListening();

            Console.ReadKey();
        }
    }
}