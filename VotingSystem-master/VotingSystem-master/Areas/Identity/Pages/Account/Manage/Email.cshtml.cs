// Лицензионное соглашение: файл распространяется по лицензии MIT (.NET Foundation)
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// Директива: отключить проверку nullable-типов для всего файла
#nullable disable

// Пространство имён для атрибутов валидации
using System.ComponentModel.DataAnnotations;
// Пространство имён для кодирования текста (Encoding.UTF8)
using System.Text;
// Пространство имён для HTML-кодирования ссылок (HtmlEncoder)
using System.Text.Encodings.Web;
// Пространство имён Identity (UserManager, SignInManager)
using Microsoft.AspNetCore.Identity;
// Пространство имён для сервисов отправки email (IEmailSender)
using Microsoft.AspNetCore.Identity.UI.Services;
// Пространство имён MVC (IActionResult)
using Microsoft.AspNetCore.Mvc;
// Пространство имён Razor Pages (PageModel)
using Microsoft.AspNetCore.Mvc.RazorPages;
// Пространство имён для Base64Url кодирования токенов (WebEncoders)
using Microsoft.AspNetCore.WebUtilities;
// Пространство имён с моделью пользователя (ApplicationUser)
using VotingSystem.Models;

// Объявление пространства имён страниц Identity для раздела Account/Manage
namespace VotingSystem.Areas.Identity.Pages.Account.Manage
{
    // Модель страницы управления email — просмотр, подтверждение и смена email
    public class EmailModel : PageModel
    {
        // UserManager — сервис управления пользователями
        private readonly UserManager<ApplicationUser> _userManager;
        // SignInManager — сервис управления входом
        private readonly SignInManager<ApplicationUser> _signInManager;
        // IEmailSender — сервис отправки email-сообщений
        private readonly IEmailSender _emailSender;

        // Конструктор модели — зависимости через DI
        public EmailModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender)
        {
            // Сохранение UserManager в поле
            _userManager = userManager;
            // Сохранение SignInManager в поле
            _signInManager = signInManager;
            // Сохранение отправителя email в поле
            _emailSender = emailSender;
        }

        // Текущий email пользователя (для отображения)
        public string Email { get; set; }

        // Флаг: подтверждён ли email пользователя (через ссылку из письма)
        public bool IsEmailConfirmed { get; set; }

        // Атрибут [TempData]: сообщение о статусе операции
        [TempData]
        // Текст статусного сообщения
        public string StatusMessage { get; set; }

        // Атрибут [BindProperty]: привязка полей формы к свойству Input
        [BindProperty]
        // Данные формы (новый email)
        public InputModel Input { get; set; }

        // Вложенная модель данных формы
        public class InputModel
        {
            // Обязательное поле
            [Required]
            // Значение должно быть валидным email-адресом
            [EmailAddress]
            // Подпись поля в форме
            [Display(Name = "Новый Email")]
            // Новый email пользователя
            public string NewEmail { get; set; }
        }

        // Приватный метод — загрузка данных текущего email пользователя в модель
        private async Task LoadAsync(ApplicationUser user)
        {
            // Получение текущего email пользователя
            var email = await _userManager.GetEmailAsync(user);
            // Сохранение email в публичное свойство
            Email = email;

            // Заполнение формы новым email (по умолчанию = текущий)
            Input = new InputModel
            {
                NewEmail = email,
            };

            // Проверка: подтверждён ли email пользователя
            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        }

        // Обработчик GET-запроса — при открытии страницы управления email
        public async Task<IActionResult> OnGetAsync()
        {
            // Загрузка текущего пользователя
            var user = await _userManager.GetUserAsync(User);
            // Если пользователь не найден
            if (user == null)
            {
                // Возврат ошибки 404
                return NotFound($"Не удалось загрузить пользователя с ID '{_userManager.GetUserId(User)}'.");
            }

            // Загрузка данных пользователя в модель
            await LoadAsync(user);
            // Отображение страницы
            return Page();
        }

        // Обработчик POST — смена email (asp-page-handler="ChangeEmail")
        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            // Загрузка текущего пользователя
            var user = await _userManager.GetUserAsync(User);
            // Если пользователь не найден
            if (user == null)
            {
                // Возврат ошибки 404
                return NotFound($"Не удалось загрузить пользователя с ID '{_userManager.GetUserId(User)}'.");
            }

            // Если данные формы не прошли валидацию
            if (!ModelState.IsValid)
            {
                // Перезагрузка данных пользователя
                await LoadAsync(user);
                // Повторный показ страницы с ошибками
                return Page();
            }

            // Получение текущего email пользователя
            var email = await _userManager.GetEmailAsync(user);
            // Если новый email отличается от текущего
            if (Input.NewEmail != email)
            {
                // Получение ID пользователя
                var userId = await _userManager.GetUserIdAsync(user);
                // Генерация токена для смены email
                var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
                // Кодирование токена в формат Base64Url
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                // Формирование ссылки для подтверждения смены email
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmailChange",                                          // Страница подтверждения смены
                    pageHandler: null,                                                      // Без обработчика
                    values: new { area = "Identity", userId = userId, email = Input.NewEmail, code = code }, // Параметры
                    protocol: Request.Scheme);                                              // Протокол
                // Отправка письма с ссылкой подтверждения смены email
                await _emailSender.SendEmailAsync(
                    Input.NewEmail,
                    "Подтверждение смены Email",
                    $"Пожалуйста, подтвердите смену email, <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>нажав здесь</a>.");

                // Информационное сообщение о необходимости подтвердить смену по ссылке
                StatusMessage = "Ссылка для подтверждения отправлена на новый email. Проверьте почту.";
                // Перезагрузка страницы
                return RedirectToPage();
            }

            // Если новый email совпадает с текущим — сообщение об этом
            StatusMessage = "Email не изменился.";
            // Перезагрузка страницы
            return RedirectToPage();
        }

        // Обработчик POST — повторная отправка письма подтверждения email
        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            // Загрузка текущего пользователя
            var user = await _userManager.GetUserAsync(User);
            // Если пользователь не найден
            if (user == null)
            {
                // Возврат ошибки 404
                return NotFound($"Не удалось загрузить пользователя с ID '{_userManager.GetUserId(User)}'.");
            }

            // Если данные формы не прошли валидацию
            if (!ModelState.IsValid)
            {
                // Перезагрузка данных пользователя
                await LoadAsync(user);
                // Повторный показ страницы
                return Page();
            }

            // Получение ID пользователя
            var userId = await _userManager.GetUserIdAsync(user);
            // Получение email пользователя
            var email = await _userManager.GetEmailAsync(user);
            // Генерация токена подтверждения email
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            // Кодирование токена в Base64Url
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            // Формирование ссылки подтверждения email
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",                                        // Страница подтверждения
                pageHandler: null,                                              // Без обработчика
                values: new { area = "Identity", userId = userId, code = code }, // Параметры
                protocol: Request.Scheme);                                      // Протокол
            // Отправка письма с ссылкой подтверждения email
            await _emailSender.SendEmailAsync(
                email,
                "Подтверждение Email",
                $"Пожалуйста, подтвердите ваш email, <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>нажав здесь</a>.");

            // Информационное сообщение об отправке письма
            StatusMessage = "Письмо с подтверждением отправлено. Проверьте почту.";
            // Перезагрузка страницы
            return RedirectToPage();
        }
    }
}