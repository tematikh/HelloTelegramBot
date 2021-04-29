using System;
using System.Net;
using System.IO;
using System.Timers;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Newtonsoft.Json;

namespace TelegramBot
{
    class Program
    {
        static TelegramBotClient client;
        static string key = "1736792483:AAFBUroBDkmOSL3oOdA1mVgDNwbkPDopFC4";
        static string tempUrl = "http://api.openweathermap.org/data/2.5/weather?id=520555&units=metric&appid=ed7f7be00e110903bc7098a1b1ec5efd";

        static void Main(string[] args)
        {
            client = new TelegramBotClient(key);
            client.OnMessage += Client_OnMessage;
            client.OnCallbackQuery += Client_OnCallbackQuery;
            var me = client.GetMeAsync().Result;
            client.StartReceiving();
            Console.ReadLine();
            client.StopReceiving();
        }

        private static async void Client_OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            string buttonText = e.CallbackQuery.Data;
            switch(buttonText)
            {
                case "Close":
                    await client.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "You have closed the inline keyboard.");
                    await client.EditMessageReplyMarkupAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId, null);
                    break;
            }
        }

        private static async void Client_OnMessage(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            if (message.Type != MessageType.Text)
                return;
            switch(message.Text)
            {
                case "/start":
                    break;
                case "Hello!":
                    await client.SendTextMessageAsync(message.From.Id, "Hello!");
                    break;
                case "How are you?":
                    await client.SendTextMessageAsync(message.From.Id, "Excellent. Thank you!");
                    break;
                case "Random number":
                    Random rng = new Random();
                    int random = rng.Next(1, 100);
                    await client.SendTextMessageAsync(message.From.Id, $"{random}");
                    break;
                case "Countdown":
                    int count = 10;
                    Message msg = await client.SendTextMessageAsync(message.From.Id, $"{count}");
                    Timer timer = new Timer(1000);
                    timer.Start();
                    timer.Elapsed += async (sender, e) => 
                    { 
                        await client.EditMessageTextAsync(message.From.Id, msg.MessageId, $"{--count}");
                        if (count < 1) 
                        { 
                            timer.Enabled = false; 
                            await client.SendTextMessageAsync(message.From.Id, "The countdown is over"); 
                        } 
                    };
                    break;
                case "Weather":
                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(tempUrl);
                    HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    string response;
                    using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                    {
                        response = streamReader.ReadToEnd();
                    }
                    WeatherResponse weatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(response);
                    await client.SendTextMessageAsync(message.From.Id, $"Temperature in {weatherResponse.Name}: {weatherResponse.Main.Temp} °C");
                    break;
                case "/inline":
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    { 
                        new[]
                        {
                            InlineKeyboardButton.WithUrl("VK", "https://vk.com/tikhonovartem")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Close")
                        }
                    });
                    await client.SendTextMessageAsync(message.From.Id, "Inline", replyMarkup: inlineKeyboard);
                    break;
                case "/keyboard":
                    var replyKeyboard = new ReplyKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            new KeyboardButton("Hello!"),
                            new KeyboardButton("How are you?")
                        },
                        new[]
                        {
                            new KeyboardButton("Random number"),
                            new KeyboardButton("Countdown")
                        },
                        new[]
                        {
                            new KeyboardButton("Weather")
                        }
                    }, true);
                    await client.SendTextMessageAsync(message.Chat.Id, "Open keyboard", replyMarkup: replyKeyboard);
                    break;
                case "/closekeyboard":
                    await client.SendTextMessageAsync(message.Chat.Id, "Close keyboard", replyMarkup: new ReplyKeyboardRemove());
                    break;
                default:
                    await client.SendTextMessageAsync(message.From.Id, "What?");
                    break;
            }
        }
    }
}
