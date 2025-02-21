using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SplittingScript;
using SplittingScript.Checking;
using SplittingScript.PractiseDocuments.PractiseReport;
using SplittingScript.PractiseDiaryKNU;
using PractiseDiaryKNU;
using Task = System.Threading.Tasks.Task;
using DotNetEnv;

class Program
{
    private static readonly ITelegramBotClient botClient;
    private static readonly DocumentChecker checker = new DocumentChecker();
    static Program()
    {
        Env.Load();
        botClient = new TelegramBotClient(token: Environment.GetEnvironmentVariable("TOKEN"));
    }

    public static async Task Main()
    {
        
        using var cts = new CancellationTokenSource();
        var receiverOptions = new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() };

        botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cts.Token);

        Console.WriteLine("Bot is running...");
        await Task.Delay(-1);
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
    {
        if (update.Message != null)
        {
            await HandleMessageAsync(bot, update.Message, token);
        }
        else if (update.CallbackQuery != null)
        {
            await HandleCallbackAsync(bot, update.CallbackQuery, token);
        }
    }

    private static async Task HandleMessageAsync(ITelegramBotClient bot, Message message, CancellationToken token)
    {
        if (message.Type == MessageType.Document)
        {
            await HandleDocumentAsync(bot, message, token);
        }
        else if (message.Type == MessageType.Text)
        {
            switch (message?.Text?.ToLower())
            {
                case "/start":
                    await bot.SendMessage(
                        message.Chat.Id,
                        "👋 Вітаю! Я бот для перевірки щоденників та звітів з виробничої практики.\n" +
                        "📄 Надішліть мені .docx-файл, і я допоможу його перевірити.\n\n" +
                        "Доступні команди:\n" +
                        "🔹 /help — як користуватись ботом.\n" +
                        "🔹 Надішліть документ — для перевірки.",
                        cancellationToken: token);
                    break;

                case "/help":
                    await bot.SendMessage(
                        message.Chat.Id,
                        "📌 Як користуватись ботом:\n\n" +
                        "1️⃣ Надішліть мені документ у форматі .docx.\n" +
                        "2️⃣ Якщо файл коректний, я запропоную вибір дії:\n" +
                        "   - 🧐 *Аналіз* — показує структуру документа.\n" +
                        "   - ✅ *Пошук помилок* — перевіряє відповідність вимогам.\n" +
                        "3️⃣ Після натискання кнопки я виконаю дію та виведу результат.",
                        parseMode: ParseMode.Markdown,
                        cancellationToken: token);
                    break;

                default:
                    await bot.SendMessage(message.Chat.Id, "❓ Невідома команда. Використовуйте /help для списку доступних команд.", cancellationToken: token);
                    break;
            }
        }
    }

    private static async Task HandleDocumentAsync(ITelegramBotClient bot, Message message, CancellationToken token)
    {
        var file = message.Document;
        if (!file.FileName.EndsWith(".docx"))
        {
            await bot.SendMessage(message.Chat.Id, "⚠️ Непідтримуваний формат файлу. Надішліть .docx.", cancellationToken: token);
            return;
        }

        var filePath = await bot.GetFile(file.FileId, token);
        var localPath = await SaveDocumentAsync(bot, file, message.Chat.Id, message.Id, token);

        var document = LoadDocument(localPath);
        if (document == null)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Не вдалося обробити документ.", cancellationToken: token);
            return;
        }

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("🧐 Аналіз", $"аналіз|{message.Id}|{message.Chat.Id}") },
            new[] { InlineKeyboardButton.WithCallbackData("✅ Пошук помилок", $"пошук помилок|{message.Id}|{message.Chat.Id}") }
        });

        await bot.SendMessage(message.Chat.Id, $"📂 Файл отримано: *{file.FileName}*\n📌 Виберіть дію:", parseMode: ParseMode.Markdown, replyMarkup: keyboard, cancellationToken: token);
    }

    private static async Task HandleCallbackAsync(ITelegramBotClient bot, CallbackQuery callbackQuery, CancellationToken token)
    {
        var data = callbackQuery?.Data?.Split('|');
        if (data is null || data.Length < 3) return;

        string command = data[0];
        int messageId = int.Parse(data[1]);
        long chatId = long.Parse(data[2]);

        string userDirectory = Path.Combine("docs", chatId.ToString());
        string filePath = Path.Combine(userDirectory, $"{messageId}.docx");
        Console.WriteLine($"[INFO] Loading file from: {filePath}");

        var document = LoadDocument(filePath);
        if (document == null)
        {
            await bot.SendMessage(callbackQuery.Message.Chat.Id, "❌ Не вдалося обробити документ.", cancellationToken: token);
            return;
        }

        checker.Checker = GetCheckerForDocument(document);

        string result = command switch
        {
            "аналіз" => document.PrintParsedContent(),
            "пошук помилок" => await checker.CheckAsync(document),
            _ => "⚠️ Невідома команда."
        };

        await SendLongMessageAsync(bot, callbackQuery.Message.Chat.Id, $"🔍 *{command.ToUpper()}* для документа:\n\n{result}", token, replyId: messageId, parsemode: ParseMode.Markdown);
    }

    private static IPractiseDocument? LoadDocument(string filePath)
    {
        try
        {
            var report = new PractiseReport(filePath);
            if (report.Title.ToLower().Contains("звіт"))
                return report;

            var diary = new PractiseDiary(filePath);
            if (diary.TitlePage.Title.ToLower().Contains("щоденник"))
                return diary;

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken token)
    {
        Console.WriteLine($"Error: {exception.Message}");
        return System.Threading.Tasks.Task.CompletedTask;
    }

    private static ICheckingStrategy GetCheckerForDocument(IPractiseDocument document)
    {
        return document switch
        {
            PractiseDiary => new PractiseDiaryChecker(),
            PractiseReport => new PractiseReportChecker(),
            _ => throw new InvalidOperationException("Unsupported document type")
        };
    }

    private static async Task SendLongMessageAsync(ITelegramBotClient bot, long chatId, string message, CancellationToken token, long replyId, ParseMode parsemode = ParseMode.Markdown)
    {
        const int MaxMessageLength = 4096;
        
        if (message.Length <= MaxMessageLength)
        {
            await bot.SendMessage(chatId, message, cancellationToken: token, parseMode: parsemode, replyParameters: (ReplyParameters)replyId);
            return;
        }

        for (int i = 0; i < message.Length; i += MaxMessageLength)
        {
            string part = message.Substring(i, Math.Min(MaxMessageLength, message.Length - i));
            if(i == 0)
            {
                await bot.SendMessage(chatId, part, cancellationToken: token, parseMode: parsemode, replyParameters: (ReplyParameters)replyId);
            } 
            else
            {
                await bot.SendMessage(chatId, part, parseMode: parsemode, cancellationToken: token);
            }
        }
    }

    private static async Task<string> SaveDocumentAsync(ITelegramBotClient bot, Document document, long userId, int messageId, CancellationToken token)
    {
        string userDirectory = Path.Combine("docs", userId.ToString());
        Directory.CreateDirectory(userDirectory); // Створюємо папку, якщо її немає

        string fileExtension = Path.GetExtension(document.FileName) ?? ".docx";
        string uniqueFileName = $"{messageId}{fileExtension}"; // Назва = MessageId.docx
        string filePath = Path.Combine(userDirectory, uniqueFileName);

        Console.WriteLine($"[INFO] Saving file as: {filePath}");

        var tgFile = await bot.GetFile(document.FileId, token);
        
        if (tgFile == null)
        {
            Console.WriteLine("[ERROR] Failed to get file from Telegram.");
            throw new Exception("Could not retrieve file from Telegram.");
        }

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await bot.DownloadFile(tgFile.FilePath, stream);
        }

        return filePath; // Повертаємо шлях до файлу
    }
}
