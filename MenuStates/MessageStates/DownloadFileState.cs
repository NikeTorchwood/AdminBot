using AdminBot.Entities;
using AdminBot.Services;
using Aspose.Cells;
using System.Diagnostics;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AdminBot.MenuStates.States;

public class DownloadFileState : IStateMenu
{
    private readonly ITelegramBotClient _bot;
    private const string MainMenuButton = "Главное Меню";
    public DownloadFileState(ITelegramBotClient bot)
    {
        _bot = bot ?? throw new ArgumentNullException(nameof(bot));
    }
    public async Task ProcessMessage(Update update,
        UserBot userBot,
        UpdateHandlerService updateHandlerService)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                switch (update.Message.Type)
                {
                    case MessageType.Document:
                        await StartDownload(update, updateHandlerService);
                        var usersId = await updateHandlerService.GetAllUsers();
                        foreach (var userId in usersId)
                        {
                            await _bot.SendTextMessageAsync(userId, "Отчет был обновлен");
                        }
                        await updateHandlerService.SetState(userBot, new StartState(_bot));
                        break;
                    case MessageType.Text:
                        switch (update.Message.Text)
                        {
                            case MainMenuButton:
                                await updateHandlerService.SetState(userBot, new StartState(_bot));
                                break;
                        }
                        break;
                }
                break;
            default:
                break;
        }
    }

    public async Task SendStateMessage(UserBot user, UpdateHandlerService updateHandlerService)
    {

        await _bot.SendTextMessageAsync(user.Id, GetStateTitle(), replyMarkup: GetKeyboard());
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
                new KeyboardButton(MainMenuButton)
            }
        }
        );
    }
    public async Task StartDownload(Update update, UpdateHandlerService updateHandlerService)
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

        var workbook = new Workbook(destinationFilePath);
        var report = await updateHandlerService.CreateReport(workbook);
        await report.StartUpdate();
        foreach (var errorCodeStore in report.ErrorStoreCodeList)
        {
            await _bot.SendTextMessageAsync(update.Message.Chat.Id,
                $"При составлении магазина {errorCodeStore}, произошла ошибка, возможно его нет в отчете.");
        }
    }
}