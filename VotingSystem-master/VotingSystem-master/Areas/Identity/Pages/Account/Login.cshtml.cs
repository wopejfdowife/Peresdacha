// Лицензионное соглашение: файл распространяется по лицензии MIT (.NET Foundation)
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// Директива: отключить проверку nullable-типов для всего файла (стандартный шаблон Identity)
#nullable disable

// Пространство имён для базовых типов языка C# (String, DateTime)
using System;
// Пространство имён для обобщённых коллекций (IList)
using System.Collections.Generic;
// Пространство имён для атрибутов валидации данных ([Required], [EmailAddress])
using System.ComponentModel.DataAnnotations;
// Пространство имён LINQ (ToList, Where)
using System.Linq;
// Пространство имён для асинхронных операций (Task)
using System.Threading.Tasks;
// Пространство имён для атрибутов авторизации ([AllowAnonymous])
using Microsoft.AspNetCore.Authorization;
// Пространство имён для работы с аутентификацией (SignOutAsync, AuthenticationScheme)
using Microsoft.AspNetCore.Authentication;
// Пространство имён для Identity (SignInManager)
using Microsoft.AspNetCore.Identity;
// Пространство имён для сервисов отправки email в Identity UI
using Microsoft.AspNetCore.Identity.UI.Services;
// Пространство имён MVC (IActionResult, RedirectToPage)
using Microsoft.AspNetCore.Mvc;
// Пространство имён Razor Pages (PageModel)
using Microsoft.AspNetCore.Mvc.RazorPages;
// Пространство имён для логирования (ILogger)
using Microsoft.Extensions.Logging;
// Пространство имён с моделью пользователя (ApplicationUser)
using VotingSystem.Models;

// Объявление пространства имён страниц Identity для раздела Account
namespace VotingSystem.Areas.Identity.Pages.Account
{
    // Модель логики страницы входа — наследуется от PageModel (стандартный код Identity UI)
    public class LoginModel : PageModel
    {
        // SignInManager — сервис Identity для выполнения входа пользователя в систему
        private readonly SignInManager<ApplicationUser> _signInManager;
        // Логгер для записи событий входа (журнал приложения)
        private readonly ILogger<LoginModel> _logger;

        // Конструктор модели — получает зависимости через DI
        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger)
        {
            // Сохранение SignInManager в приватное поле
            _signInManager = signInManager;
            // Сохранение логгера в приватное поле
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // Атрибут [BindProperty]: привязка данных формы к свойству Input при POST-запросе
        [BindProperty]
        // Свойство с данными формы входа (Email, Password, RememberMe)
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // Список доступных внешних схем аутентификации (например, Google, Facebook)
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // URL, на который вернётся пользователь после успешного входа
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // Атрибут [TempData]: свойство хранит данные только между двумя запросами
        [TempData]
        // Сообщение об ошибке входа (передаётся при редиректе)
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // Вложенная модель данных формы входа
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            // Атрибут: поле обязательно для заполнения
            [Required]
            // Атрибут: значение должно быть корректным email-адресом
            [EmailAddress]
            // Email пользователя для входа
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            // Атрибут: поле обязательно для заполнения
            [Required]
            // Атрибут: тип поля — пароль (скрывает символы в браузере)
            [DataType(DataType.Password)]
            // Пароль пользователя
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            // Атрибут: текст подписи поля в форме
            [Display(Name = "Remember me?")]
            // Флаг: запомнить пользователя (сохранять cookie входа между сессиями)
            public bool RememberMe { get; set; }
        }

        // Обработчик GET-запроса — вызывается при открытии страницы входа
        public async Task OnGetAsync(string returnUrl = null)
        {
            // Если было сообщение об ошибке (передано через TempData)
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                // Добавление ошибки в ModelState для отображения в форме
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            // Если ReturnUrl не указан — установить главную страницу сайта как URL возврата
            returnUrl ??= Url.Content("~/");

            // Очистка существующей внешней cookie, чтобы гарантировать чистый процесс входа
            // (инициализация сессии входа)
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // Получение списка доступных схем внешней аутентификации
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // Сохранение URL возврата в публичное свойство
            ReturnUrl = returnUrl;
        }

        // Обработчик POST-запроса — вызывается при отправке формы входа
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // Если ReturnUrl не указан — главная страница сайта
            returnUrl ??= Url.Content("~/");

            // Получение списка доступных схем внешней аутентификации
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // Проверка: прошли ли данные формы валидацию
            if (ModelState.IsValid)
            {
                // Выполнение входа по email и паролю
                // lockoutOnFailure: false — неудачные попытки не учитываются для блокировки аккаунта
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                // Если вход успешен
                if (result.Succeeded)
                {
                    // Запись в журнал: пользователь вошёл в систему
                    _logger.LogInformation("User logged in.");
                    // Локальный редирект на URL возврата (защита от внешних URL)
                    return LocalRedirect(returnUrl);
                }
                // Если аккаунт заблокирован
                if (result.IsLockedOut)
                {
                    // Предупреждение в журнал: аккаунт заблокирован
                    _logger.LogWarning("User account locked out.");
                    // Перенаправление на страницу блокировки аккаунта
                    return RedirectToPage("./Lockout");
                }
                // Иначе — неверные данные входа
                else
                {
                    // Добавление сообщения об ошибке в форма (неверный email или пароль)
                    ModelState.AddModelError(string.Empty, "Неверный email или пароль.");
                    // Повторный показ страницы входа с ошибкой
                    return Page();
                }
            }

            // Если валидация не прошла — повторный показ формы (с ошибками валидации)
            return Page();
        }
    }
}