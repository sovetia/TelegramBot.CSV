using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using CsvHelper;
using CsvHelper.Configuration;

namespace TelegramBot.CSV
{

    class Program
    {
        private static TelegramBotClient bot;
        static List<CSV> records;
        private static string token = "YOUR TOKEN";
        static string pathCsvFile = "database.csv";       

        static bool isSearch = false;
        static bool isEdit = false;
        static bool isWrite = false;

        static int rowCount = 0;

        static string answer = "Hello! \U0001F4A9\nHow can I help you?";
        static string edit;


        static void Main(string[] args)
        {

            bot = new TelegramBotClient(token);
            bot.StartReceiving();
            bot.OnMessage += BotOnMessage;
            bot.OnCallbackQuery += BotOnCallback;

            Console.ReadLine();
            bot.StopReceiving();

        }

        private static async void BotOnMessage(object sender, MessageEventArgs e)
        {

            if (e.Message != null && e.Message.Type == MessageType.Text)
            {
                var message = e.Message;

                try
                {
                    //start
                    if (message.Text == "/start" || message.Text.Contains("Hello") && !isSearch) StartMenu(e);

                    //search
                    else if (isSearch) SearchCSV(e);

                    //edit
                    else if (isEdit) StartEditor(e);

                    //write
                    else if (isWrite) WriteCSV(e);

                }

                catch (Exception ex)
                {

                }
            }


        }

        //Start Menu
        private static async void StartMenu(MessageEventArgs e)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                            InlineKeyboardButton.WithCallbackData("Search", "search"),
                            InlineKeyboardButton.WithCallbackData("Edit", "edit"),
                            InlineKeyboardButton.WithCallbackData("Quit", "stop")

            });

            bot.SendTextMessageAsync(e.Message.Chat.Id, answer, replyMarkup: keyboard);
        }


        //EditMenu
        private static void EditorMenu(MessageEventArgs e)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                 new []
                 {
                            InlineKeyboardButton.WithCallbackData("Name", "cell1"),
                            InlineKeyboardButton.WithCallbackData("Surname", "cell2"),
                 },
                new[]
                 {

                            InlineKeyboardButton.WithCallbackData("Age", "cell3"),
                            InlineKeyboardButton.WithCallbackData("Gender", "cell4"),
                 },

            });

            bot.SendTextMessageAsync(e.Message.Chat.Id, "Select a cell to edit", replyMarkup: keyboard);
        }


        // Callback
        private static void BotOnCallback(object sender, CallbackQueryEventArgs e)
        {
            var message = e.CallbackQuery.Message;


            switch (e.CallbackQuery.Data)
            {
                case "search":
                    bot.SendTextMessageAsync(message.Chat.Id, "What to find?\nEnter [Name], [Surname], [Age] or [Gender]");
                    isSearch = true;
                    isEdit = false;
                    isWrite = false;
                    break;
                
                case "edit":
                    bot.SendTextMessageAsync(message.Chat.Id, "Enter row number:");
                    isEdit = true;
                    isWrite = false;
                    isSearch = false;
                    break;

                case "cell1":
                    bot.SendTextMessageAsync(message.Chat.Id, "New [Name]:");
                    isEdit = false;
                    isWrite = true;
                    isSearch = false;
                    edit = "cell1";
                    break;
                case "cell2":
                    bot.SendTextMessageAsync(message.Chat.Id, "New [Surname]:");
                    isEdit = false;
                    isWrite = true;
                    isSearch = false;
                    edit = "cell2";
                    break;
                case "cell3":
                    bot.SendTextMessageAsync(message.Chat.Id, "New [Age]:");
                    isEdit = false;
                    isWrite = true;
                    isSearch = false;
                    edit = "cell3";
                    break;
                case "cell4":
                    bot.SendTextMessageAsync(message.Chat.Id, "New [Gender]:");
                    isEdit = false;
                    isWrite = true;
                    isSearch = false;
                    edit = "cell4";
                    break;

                case "stop":
                    bot.SendTextMessageAsync(message.Chat.Id, "Goodbye \U0001F64B");
                    isEdit = false;
                    isWrite = false;
                    isSearch = false;
                    answer = "Hello! \U0001F4A9\nHow can I help you?";
                    break;
            }
        }

        // Search
        static async void SearchCSV(MessageEventArgs e)
        {

            CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Delimiter = ";"
            };

            using (var reader = new StreamReader(pathCsvFile))

            using (var csv = new CsvReader(reader, config))
            {

                records = csv.GetRecords<CSV>().ToList();

                bool find = false;
                int count = 0;

                foreach (var row in records)
                {
                    count++;
                    
                    if (row.cell1 == e.Message.Text || row.cell2 == e.Message.Text || row.cell3 == e.Message.Text || row.cell4 == e.Message.Text )

                    {
                       // Console.WriteLine($"| {count} | {row.cell1} | {row.cell2} | {row.cell3} | {row.cell4} |");
                        await bot.SendTextMessageAsync(e.Message.Chat.Id, $"[row]: {count}\n[Name]: {row.cell1}\n[Surname]: {row.cell2}\n[Age]: {row.cell3}\n[Gender]: {row.cell4}");
                        find = true;
                    }
                }

                if (!find) answer = "Not found!";
                else answer = "What else?";
                isSearch = false;
                StartMenu(e);
            }
        }

        // Edit
        static async void StartEditor(MessageEventArgs e)
        {
            CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Delimiter = ";"
            };

            int count = 0;
            bool find = false;

            using (var reader = new StreamReader(pathCsvFile))
            using (var csv = new CsvReader(reader, config))
                records = csv.GetRecords<CSV>().ToList();

            foreach (var row in records)
            {
                count++;
                if (count.ToString() == e.Message.Text)
                {
                    rowCount = count;
                    find = true;
                }
            }

            if (find)
            {
                isEdit = false;
                answer = "What else?";
                EditorMenu(e);
            }

            else
            {
                answer = "Not found!";
                isEdit = false;
                StartMenu(e);
            }
        }

        // Write
        static async void WriteCSV(MessageEventArgs e)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Delimiter = ";"
            };

            int count = 0;
            using (var reader = new StreamReader(pathCsvFile))
            using (var csv = new CsvReader(reader, config))
            records = csv.GetRecords<CSV>().ToList();

            foreach (var row in records)
            {
                count++;
                if (count == rowCount)
                {
                    switch (edit)
                    {
                        case "cell1":
                            row.cell1 = e.Message.Text;
                            break;
                        case "cell2":
                            row.cell2 = e.Message.Text;
                            break;
                        case "cell3":
                            row.cell3 = e.Message.Text;
                            break;
                        case "cell4":
                            row.cell4 = e.Message.Text;
                            break;
                    }

                   // Console.WriteLine($"{rowCount} | {row.cell1} | {row.cell2} | {row.cell3} | {row.cell4} |");
                }
            }

            // write to file
            using (var writer = new StreamWriter(pathCsvFile))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(records);
            }
            await bot.SendTextMessageAsync(e.Message.Chat.Id, "Ready!");
            isWrite = false;
            StartMenu(e);
        }
    }

    // your database
    public class CSV
    {
        public string cell1 { get; set; }
        public string cell2 { get; set; }
        public string cell3 { get; set; }
        public string cell4 { get; set; }
    }
}

