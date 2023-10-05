using System.Diagnostics;
using System.Text;
using AdminBot.Entities.Users;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AdminBot.MenuStates.States;

public class DownloadFileMenu : IStateMenu
{
    private readonly ITelegramBotClient _bot;
    private const string Button1 = "Главное Меню";
    public DownloadFileMenu(ITelegramBotClient bot)
    {
        _bot = bot ?? throw new ArgumentNullException(nameof(bot));
    }
    public async Task ProcessMessage(Update update,
        UserBot userBot,
        TelegramBotMenuContext context)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                switch (update.Message.Type)
                {
                    case MessageType.Document:
                        await StartDownload(update);
                        await context.SetState(update, 
                            new UserBot(userBot.Id,
                                new StartState(_bot),
                                userBot.Role));
                        break;
                    case MessageType.Text:
                        switch (update.Message.Text)
                        {
                            case Button1:
                                await context.SetState(update, 
                                    new UserBot(userBot.Id,
                                        new StartState(_bot),
                                        userBot.Role));
                                break;
                        }
                        break;
                }
                break;
            default:
                break;
        }
    }

    public async Task SendStateMessage(Update update, UserBot userBot)
    {

        await _bot.SendTextMessageAsync(update.Message.Chat.Id, GetStateTitle(), replyMarkup: GetKeyboard());
    }

    public static string GetStateTitle()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Меню загрузки файла:");
        sb.AppendLine("Отправь файл в сообщении, сейчас реализована поддержка только рейтинга выполнения планов");
        sb.AppendLine("!Важно! Если ты отправишь что-то кроме детализации продаж - то скорей всего бот сломается");
        return sb.ToString();
    }

    public static IReplyMarkup GetKeyboard()
    {
        return new ReplyKeyboardMarkup(new List<List<KeyboardButton>>
        {
            new()
            {
                new KeyboardButton(Button1)
            }
        }
        );
    }
    public async Task StartDownload(Update update)
    {

        var sw = new Stopwatch();
        sw.Restart();
        await _bot.SendTextMessageAsync(update.Message.Chat.Id,
            "Обновляю данные, дождись скачивания данных...",
            replyMarkup: new ReplyKeyboardRemove());
        var fileId = update.Message.Document.FileId;
        var fileInfo = await _bot.GetFileAsync(fileId);
        var filePath = fileInfo.FilePath;
        var destinationFilePath = $"{Environment.CurrentDirectory}\\economic.xlsx";
        Console.WriteLine(destinationFilePath);
        var sw1 = new Stopwatch();
        sw1.Restart();
        try
        {
            var fileStream = new FileStream(destinationFilePath, FileMode.OpenOrCreate);
            await _bot.DownloadFileAsync(
                filePath,
                fileStream);
            sw1.Stop();
            await _bot.SendTextMessageAsync(update.Message.Chat.Id,
                $"Скачивание файла произошло успешно. Время скачивания {sw1.Elapsed}");
            fileStream.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

    }
}