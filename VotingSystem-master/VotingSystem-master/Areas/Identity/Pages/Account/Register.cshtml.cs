// Лицензионное соглашение: файл распространяется по лицензии MIT (.NET Foundation)
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// Директива: отключить проверку nullable-типов для всего файла
#nullable disable

// Пространство имён для атрибутов валидации ([Required], [EmailAddress], [Compare])
using System.ComponentModel.DataAnnotations;
// Пространство имён для кодирования текста (Encoding.UTF8)
using System.Text;
// Пространство имён для HTML-кодирования (HtmlEncoder)
using System.Text.Encodings.Web;
// Пространство имён для работы с аутентификацией (AuthenticationScheme)
using Microsoft.AspNetCore.Authentication;
// Пространство имён для атрибутов авторизации ([AllowAnonymous])
using Microsoft.AspNetCore.Authorization;
// Пространство имён Identity (UserManager, SignInManager, IUserStore)
using Microsoft.AspNetCore.Identity;
// Пространство имён для сервисов отправки email (IEmailSender)
using Microsoft.AspNetCore.Identity.UI.Services;
// Пространство имён MVC (IActionResult)
using Microsoft.AspNetCore.Mvc;
// Пространство имён Razor Pages (PageModel)
using Microsoft.AspNetCore.Mvc.RazorPages;
// Пространство имён для работы с Base64Url запросами (WebEncoders)
using Microsoft.AspNetCore.WebUtilities;
// Пространство имён с моделью пользователя (ApplicationUser)
using VotingSystem.Models;

// Объявление пространства имён страниц Identity для раздела Account
namespace VotingSystem.Areas.Identity.Pages.Account
{
    // Атрибут: страница регистрации доступна даже неавторизованным пользователям
    [AllowAnonymous]
    // Модель логики страницы регистрации — наследуется от PageModel
    public class RegisterModel : PageModel
    {
        // SignInManager — сервис для автоматического входа после регистрации
        private readonly SignInManager<ApplicationUser> _signInManager;
        // UserManager — сервис для создания и управления пользователями
        private readonly UserManager<ApplicationUser> _userManager;
        // IUserStore — хранилище пользователей (низкоуровневый интерфейс)
        private readonly IUserStore<ApplicationUser> _userStore;
        // IUserEmailStore — интерфейс хранения, поддерживающий email-поля
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        // Логгер для записи событий регистрации
        private readonly ILogger<RegisterModel> _logger;
        // Сервис отправки email (для подтверждения регистрации)
        private readonly IEmailSender _emailSender;

        // Конструктор модели — зависимости внедряются через DI
        public RegisterModel(
            UserManager<ApplicationUser> userManager,     // Менеджер пользователей
            IUserStore<ApplicationUser> userStore,         // Хранилище пользователей
            SignInManager<ApplicationUser> signInManager, // Менеджер входа
            ILogger<RegisterModel> logger,                // Логгер
            IEmailSender emailSender)                     // Отправитель email
        {
            // Сохранение UserManager в поле
            _userManager = userManager;
            // Сохранение хранилища пользователей в поле
            _userStore = userStore;
            // Получение интерфейса email-хранилища и сохранение его в поле
            _emailStore = GetEmailStore();
            // Сохранение SignInManager в поле
            _signInManager = signInManager;
            // Сохранение логгера в поле
            _logger = logger;
            // Сохранение отправителя email в поле
            _emailSender = emailSender;
        }

        // Атрибут [BindProperty]: привязка полей формы регистрации к свойству Input
        [BindProperty]
        // Данные, введённые пользователем в форму регистрации
        public InputModel Input { get; set; }

        // URL, куда вернуть пользователя после регистрации
        public string ReturnUrl { get; set; }

        // Список доступных схем внешней аутентификации
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        // Вложенная модель полей формы регистрации
        public class InputModel
        {
            // Обязательное поле — Имя
            [Required(ErrorMessage = "Имя обязательно")]
            // Подпись поля в форме
            [Display(Name = "Имя")]
            // Имя пользователя (дополнительное поле ApplicationUser)
            public string FirstName { get; set; }

            // Обязательное поле — Фамилия
            [Required(ErrorMessage = "Фамилия обязательна")]
            // Подпись поля в форме
            [Display(Name = "Фамилия")]
            // Фамилия пользователя
            public string LastName { get; set; }

            // Подпись поля в форме
            [Display(Name = "Регион")]
            // Регион проживания (по умолчанию — Владикавказ)
            public string Region { get; set; } = "Владикавказ";

            // Обязательное поле — email
            [Required]
            // Значение должно быть валидным email-адресом
            [EmailAddress]
            // Подпись поля в форме
            [Display(Name = "Email")]
            // Email пользователя (используется как логин)
            public string Email { get; set; }

            // Обязательное поле — пароль
            [Required]
            // Ограничение длины: от 4 до 100 символов (с подсказкой на русском)
            [StringLength(100, ErrorMessage = "{0} должен быть от {2} до {1} символов.", MinimumLength = 4)]
            // Тип поля — пароль
            [DataType(DataType.Password)]
            // Подпись поля
            [Display(Name = "Пароль")]
            // Пароль пользователя
            public string Password { get; set; }

            // Тип поля — пароль
            [DataType(DataType.Password)]
            // Подпись поля
            [Display(Name = "Подтвердите пароль")]
            // Атрибут [Compare]: значение должно совпадать с полем Password
            [Compare("Password", ErrorMessage = "Пароли не совпадают")]
            // Подтверждение пароля
            public string ConfirmPassword { get; set; }
        }

        // Обработчик GET-запроса — при открытии страницы регистрации
        public async Task OnGetAsync(string returnUrl = null)
        {
            // Сохранение URL возврата
            ReturnUrl = returnUrl;
            // Загрузка списка схем внешней аутентификации
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        // Обработчик POST-запроса — при отправке формы регистрации
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // Если URL не указан — главная страница
            returnUrl ??= Url.Content("~/");
            // Загрузка схем внешней аутентификации
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            // Проверка валидности данных формы
            if (ModelState.IsValid)
            {
                // Создание нового экземпляра пользователя
                var user = CreateUser();
                // Заполнение дополнительных полей пользователя данными из формы
                user.FirstName = Input.FirstName;              // Имя
                user.LastName = Input.LastName;                // Фамилия
                user.Region = Input.Region ?? "Владикавказ";  // Регион (или по умолчанию)

                // Установка email в качестве имени пользователя (UserName = Email)
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                // Установка email пользователя в хранилище
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                // Создание пользователя в базе данных с хешированным паролем
                var result = await _userManager.CreateAsync(user, Input.Password);

                // Если пользователь успешно создан
                if (result.Succeeded)
                {
                    // Запись события в журнал: создан новый аккаунт
                    _logger.LogInformation("User created a new account with password.");

                    // Получение ID созданного пользователя
                    var userId = await _userManager.GetUserIdAsync(user);
                    // Генерация токена подтверждения email
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    // Кодирование токена в формат Base64Url (безопасный для URL)
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    // Формирование ссылки для подтверждения email
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",                                       // Страница подтверждения
                        pageHandler: null,                                             // Без обработчика
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl }, // Параметры запроса
                        protocol: Request.Scheme);                                     // Протокол (http/https)

                    // Отправка email с ссылкой подтверждения аккаунта
                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    // Если в конфигурации требуется подтверждение аккаунта
                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        // Перенаправление на страницу "Подтвердите регистрацию"
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    // Иначе — подтверждение не требуется
                    else
                    {
                        // Автоматический вход пользователя в систему
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        // Редирект на страницу возврата
                        return LocalRedirect(returnUrl);
                    }
                }
                // Если при создании возникли ошибки
                foreach (var error in result.Errors)
                {
                    // Добавление каждой ошибки в ModelState для отображения
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Повторный показ формы регистрации (с ошибками валидации)
            return Page();
        }

        // Приватный метод — создание нового экземпляра ApplicationUser
        private ApplicationUser CreateUser()
        {
            // Блок try-catch для перехвата ошибок создания
            try
            {
                // Создание экземпляра типа ApplicationUser через параметрический конструктор по умолчанию
                return Activator.CreateInstance<ApplicationUser>();
            }
            // Если создание не удалось
            catch
            {
                // Выбрасывание исключения с пояснением причины
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        // Приватный метод — получение интерфейса email-хранилища
        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            // Если хранилище пользователей не поддерживает работу с email
            if (!_userManager.SupportsUserEmail)
            {
                // Выбрасывание исключения: стандартный UI требует email-хранилище
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            // Приведение IUserStore к IUserEmailStore и возврат
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}