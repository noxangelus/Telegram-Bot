// Підключаємо бібліотеки
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
// Телеграм бібліотеки
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TestBotTg
{
    internal class Program
    {
        // Зберігання стану реєстрації для кожного користувача
        static ConcurrentDictionary<long, UserRegistration> registrationStates = new ConcurrentDictionary<long, UserRegistration>();

        // Список ID адміністраторів
        static List<long> adminChatIds = new List<long> { 467437260 };

        // Список підтверджених реєстрацій
        static ConcurrentDictionary<string, UserRegistration> acceptedRegistrations = new ConcurrentDictionary<string, UserRegistration>();

        // Зберігання причин відмови для користувачів
        static ConcurrentDictionary<long, long> declineReasons = new ConcurrentDictionary<long, long>();

        static void Main(string[] args)
        {
            var client = new TelegramBotClient("7359000088:AAGT6YXv4jtiFKv7ujVGY4c1zsd6ua6WE40"); // Підключення бота, API

            client.StartReceiving(Update, Error); // Отримання оновлень від Telegram
            Console.ReadLine();
        }

        // Метод обробки оновлень від Telegram
        async static Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            var message = update.Message; // Повідомлення
            var callbackQuery = update.CallbackQuery; // Зворотний запит від користувача

            if (message != null && message.Text != null)
            {
                // Перевірка стану реєстрації користувача
                if (registrationStates.ContainsKey(message.Chat.Id))
                {
                    await HandleRegistration(botClient, message); // Обробка реєстрації
                }
                else if (declineReasons.ContainsKey(message.Chat.Id))
                {
                    await HandleDeclineReason(botClient, message); // Обробка відмови
                }
                else
                {
                    switch (message.Text.ToLower()) // Обробка команд
                    {
                        case "/start":
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Вас вітає Photo бот, через нього ви зможете записатись на фотозйомку та дізнатись трохи про нас. Для навігації використовуйте команди \n\n" +
                                "Команди: \n/start - Початкова сторінка, де можна передивитись наші роботи та соцмережі \n/location - Наші локації, але якщо у вас є своя або хочете іншу можете вказати її \n/contact - Наші контакти, якщо є запитання - будемо раді відповісти \n/price - Наші ціни, але якщо ви знайшли що вам підходить, можна буде обговорити з фотографом після реєстрації \n/reg - Реєстрація на фотосесію, після її заповнювання зможете уточнити все що вам необхідно з фотографом",
                                parseMode: ParseMode.Html,
                                disableWebPagePreview: false,
                                disableNotification: false,
                                allowSendingWithoutReply: true,
                                replyMarkup: new InlineKeyboardMarkup(new[]
                                {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithUrl("WebSite", "https://www.instagram.com/you_know_studio?igsh=eHRoeGthbHJ2azJ0"),
                                        InlineKeyboardButton.WithUrl("Portfolio", "https://t.me/youknow_studio")
                                    }
                                }));
                            break;

                        case "/location":
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Наразі є такі локації, але якщо ви хочете запропонувати свою, то при реєстрації введіть свою локацію:");

                            await botClient.SendVenueAsync(message.Chat.Id, latitude: 48.462430854896f, longitude: 35.07167380554882f,
                                title: "Парк культури та відпочинку ім. Т. Г. Шевченка",
                                address: "Дніпро, Дніпропетровська область, 49000",
                                cancellationToken: token);

                            await botClient.SendVenueAsync(message.Chat.Id, latitude: 48.43426238895275f, longitude: 35.0697582184348f,
                               title: "Прибережний сквер",
                               address: "вулиця Набережна Перемоги, 66, Дніпро, Дніпропетровська область, 49000",
                               cancellationToken: token);

                            await botClient.SendVenueAsync(message.Chat.Id, latitude: 48.46002593276368f, longitude: 35.049732269201634f,
                               title: "Катеринославський бульвар 2",
                               address: "вулиця Південна, Дніпро, Дніпропетровська область, 49000",
                               cancellationToken: token);

                            await botClient.SendVenueAsync(message.Chat.Id, latitude: 48.41399676061234f, longitude: 35.080748928871515f,
                               title: "Яхт-клуб Січ",
                               address: "вулиця Набережна Перемоги, 77Б, Дніпро, Дніпропетровська область, 49000",
                               cancellationToken: token);

                            await botClient.SendVenueAsync(message.Chat.Id, latitude: 48.46893050266662f, longitude: 35.05826015972895f,
                               title: "Набережна Дніпра",
                               address: "вулиця Січеславська Набережна, 35В, Дніпро, Дніпропетровська область, 49000",
                               cancellationToken: token);
                            break;

                        case "/contact":
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Наші контакти, не соромтесь писати, якщо є запитання або побажання:");
                            await botClient.SendContactAsync(message.Chat.Id, phoneNumber: "+380962930856", firstName: "Petro",
                                vCard: "BEGIN:VCARD\nVERSION:3.0\nN:Kurochka;Petro\nORG:Photo\nTEL;TYPE=voice,work,pref:+380962930856\nEMAIL:email@mail\nEND:VCARD");
                            break;

                        case "/reg":
                            var registration = new UserRegistration();
                            registrationStates[message.Chat.Id] = registration;
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Введіть ваше ім'я:");
                            break;

                        case "/price":
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Наші ціни: \n\n" +
                                "1500 грн - Одна людина\n" +
                                "2000 грн - LoveStory, Sport \n\n" +
                                "Ціни розраховані на 1 годину \n\n" +
                                "Ціну можна буде обговорити з фотографом після реєстрації \n" +
                                "Також якщо вам нічого не підходе, ви можете вибрати щось своє - це обговорюєтся з фотографом");
                            break;

                        case "/list":
                            if (adminChatIds.Contains(message.Chat.Id))
                            {
                                StringBuilder sb = new StringBuilder();
                                if (acceptedRegistrations.IsEmpty)
                                {
                                    sb.AppendLine("Тут нічого немає");
                                }
                                else
                                {
                                    sb.AppendLine("Погоджені реєстрації:");
                                    foreach (var reg in acceptedRegistrations)
                                    {
                                        sb.AppendLine($"{reg.Key} - Локація: {reg.Value.Location}, Дата: {reg.Value.Date}, Час: {reg.Value.Time}, Телефон: {reg.Value.PhoneNumber}");
                                    }
                                }
                                await botClient.SendTextMessageAsync(message.Chat.Id, sb.ToString());
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, "У вас недостатньо прав для цієї команди");
                            }
                            break;

                        case "/remove":
                            if (adminChatIds.Contains(message.Chat.Id))
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, "Введіть ім'я та прізвище користувача, якого потрібно видалити:");
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, "У вас недостатньо прав для цієї команди");
                            }
                            break;

                        default:
                            await HandleRemoveRequest(botClient, message);
                            break;
                    }
                }
            }

            if (callbackQuery != null)
            {
                await HandleAdminResponse(botClient, callbackQuery); // Обробка відповіді від адміністратора
            }
        }

        // Метод для поетапної реєстрації
        private static async Task HandleRegistration(ITelegramBotClient botClient, Message message)
        {
            var chatId = message.Chat.Id;
            if (registrationStates.TryGetValue(chatId, out var registration))
            {
                switch (registration.Stage)
                {
                    case RegistrationStage.AwaitingFirstName:
                        registration.FirstName = message.Text;
                        registration.Stage = RegistrationStage.AwaitingLastName;
                        await botClient.SendTextMessageAsync(chatId, "Введіть ваше прізвище:");
                        break;

                    case RegistrationStage.AwaitingLastName:
                        registration.LastName = message.Text;
                        registration.Stage = RegistrationStage.AwaitingLocation;
                        await botClient.SendTextMessageAsync(chatId, "Яку локацію вибираєте? \nЯкщо у вас своя локація - напишіть її адресу:");
                        break;

                    case RegistrationStage.AwaitingLocation:
                        registration.Location = message.Text;
                        registration.Stage = RegistrationStage.AwaitingDate;
                        await botClient.SendTextMessageAsync(chatId, "На яку дату плануєте?");
                        break;

                    case RegistrationStage.AwaitingDate:
                        registration.Date = message.Text;
                        registration.Stage = RegistrationStage.AwaitingTime;
                        await botClient.SendTextMessageAsync(chatId, "О котрій годині хотіли б фотосесію?");
                        break;

                    case RegistrationStage.AwaitingTime:
                        registration.Time = message.Text;
                        registration.Stage = RegistrationStage.AwaitingNumberOfPeople;
                        await botClient.SendTextMessageAsync(chatId, "Скільки буде людей?");
                        break;

                    case RegistrationStage.AwaitingNumberOfPeople:
                        registration.NumberOfPeople = message.Text;
                        registration.Stage = RegistrationStage.AwaitingPhoneNumber;
                        await botClient.SendTextMessageAsync(chatId, "Введіть номер телефону:");
                        break;

                    case RegistrationStage.AwaitingPhoneNumber:
                        registration.PhoneNumber = message.Text;

                        // Відправлення анкети адміністраторам
                        foreach (var adminId in adminChatIds)
                        {
                            // Кнопки для вибору рішення
                            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new InlineKeyboardButton[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Прийняти", $"confirm_{chatId}"), // Кнопка для підтвердження
                                    InlineKeyboardButton.WithCallbackData("Відмовити", $"decline_{chatId}") // Кнопка для відмови
                                }
                            });
                            // Вигляд анкети для адміністраторів
                            string adminMessage = $"Нова заявка від @{message.From.Username ?? "NoUsername"}:\n" +
                                                  $"Ім'я: {registration.FirstName}\n" +
                                                  $"Прізвище: {registration.LastName}\n" +
                                                  $"Локація: {registration.Location}\n" +
                                                  $"Дата: {registration.Date}\n" +
                                                  $"Час: {registration.Time}\n" +
                                                  $"Кількість людей: {registration.NumberOfPeople}\n" +
                                                  $"Телефон: {registration.PhoneNumber}";
                            // Відправка адміністратору
                            await botClient.SendTextMessageAsync(adminId, adminMessage, replyMarkup: inlineKeyboard);
                        }

                        await botClient.SendTextMessageAsync(chatId, "Заявка відправлена, чекайте на відповідь");
                        break;

                    default:
                        registrationStates.TryRemove(chatId, out _);
                        await botClient.SendTextMessageAsync(chatId, "Реєстрація завершена. Вас повідомлять про результат. Якщо будуть питання, можете звернутись до нас за командою /contact");
                        break;
                }
            }
        }

        // Метод для обробки відповіді адміністратора
        private static async Task HandleAdminResponse(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var chatId = callbackQuery.Message.Chat.Id; // ID чату, з якого прийшла відповідь
            var data = callbackQuery.Data; // Дані про запит (підтвердження або відмова) та ID заявки

            if (data.StartsWith("confirm_")) // Якщо підтвердження заявки
            {
                var userId = long.Parse(data.Split('_')[1]); // Розбиваємо дані на частини для отримання ID користувача

                if (registrationStates.TryRemove(userId, out var registration)) // Видаляємо з етапу реєстрації
                {
                    // Додаємо до списку підтверджених
                    acceptedRegistrations.TryAdd($"{registration.FirstName} {registration.LastName}", registration);
                    await botClient.SendTextMessageAsync(userId, "Заявка підтверджена! Незабаром вам напише фотограф, щоб обговорити всі деталі");
                    await botClient.SendTextMessageAsync(chatId, "Заявка підтверджена");
                }
            }
            else if (data.StartsWith("decline_")) // Якщо відмова заявки
            {
                var userId = long.Parse(data.Split('_')[1]); // Розбиваємо дані на частини для отримання ID користувача
                declineReasons.TryAdd(chatId, userId); // Додаємо ID адміністратора та користувача до списку обробки відмов
                await botClient.SendTextMessageAsync(chatId, "Введіть причину відмови"); // Запит причини відмови
            }
        }

        // Метод для обробки причини відмови
        private static async Task HandleDeclineReason(ITelegramBotClient botClient, Message message)
        {
            var adminId = message.Chat.Id; // ID адміністратора, що надав причину відмови

            if (declineReasons.TryRemove(adminId, out var userId)) // Видаляємо з обробки відмов
            {
                if (registrationStates.TryRemove(userId, out var registration)) // Видаляємо з етапу реєстрації
                {
                    await botClient.SendTextMessageAsync(userId, $"Заявка відмовлена через: {message.Text}"); // Повідомлення користувачу про відмову
                    await botClient.SendTextMessageAsync(adminId, "Відмова надіслана"); // Повідомлення адміністратору про успішну відмову
                }
            }
        }

        // Метод для обробки запиту на видалення
        private static async Task HandleRemoveRequest(ITelegramBotClient botClient, Message message)
        {
            var chatId = message.Chat.Id;
            if (acceptedRegistrations.TryRemove($"{message.Text}", out var registration))
            {
                await botClient.SendTextMessageAsync(chatId, "Реєстрацію видалено");
            }
            else
            {
                await botClient.SendTextMessageAsync(chatId, "Не зрозуміло, повторить");
            }
        }

        // Метод для обробки помилок
        static Task Error(ITelegramBotClient botClient, Exception exception, CancellationToken token)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
            return Task.CompletedTask;
        }
    }

    // Етапи реєстрації
    public enum RegistrationStage
    {
        AwaitingFirstName,
        AwaitingLastName,
        AwaitingLocation,
        AwaitingDate,
        AwaitingTime,
        AwaitingNumberOfPeople,
        AwaitingPhoneNumber,
        Completed
    }

    // Клас для зберігання інформації про замовника
    public class UserRegistration
    {
        public RegistrationStage Stage { get; set; } = RegistrationStage.AwaitingFirstName;
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Location { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string NumberOfPeople { get; set; }
        public string PhoneNumber { get; set; }
    }
}
