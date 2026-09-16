// Лицензионное соглашение: файл распространяется по лицензии MIT (.NET Foundation)
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// Директива: отключить проверку nullable-типов для всего файла
#nullable disable

// Пространство имён для атрибутов валидации
using System.ComponentModel.DataAnnotations;
// Пространство имён Identity (UserManager, SignInManager)
using Microsoft.AspNetCore.Identity;
// Пространство имён MVC (IActionResult)
using Microsoft.AspNetCore.Mvc;
// Пространство имён Razor Pages (PageModel)
using Microsoft.AspNetCore.Mvc.RazorPages;
// Пространство имён для логирования (ILogger)
using Microsoft.Extensions.Logging;
// Пространство имён с моделью пользователя (ApplicationUser)
using VotingSystem.Models;

// Объявление пространства имён страниц Identity для раздела Account/Manage
namespace VotingSystem.Areas.Identity.Pages.Account.Manage
{
    // Модель страницы смены пароля
    public class ChangePasswordModel : PageModel
    {
        // UserManager — сервис управления пользователями (смена пароля)
        private readonly UserManager<ApplicationUser> _userManager;
        // SignInManager — сервис управления входом (обновление сессии)
        private readonly SignInManager<ApplicationUser> _signInManager;
        // Логгер для записи событий смены пароля
        private readonly ILogger<ChangePasswordModel> _logger;

        // Конструктор модели — зависимости через DI
        public ChangePasswordModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ChangePasswordModel> logger)
        {
            // Сохранение UserManager в поле
            _userManager = userManager;
            // Сохранение SignInManager в поле
            _signInManager = signInManager;
            // Сохранение логгера в поле
            _logger = logger;
        }

        // Атрибут [BindProperty]: привязка полей формы к свойству Input
        [BindProperty]
        // Данные формы смены пароля
        public InputModel Input { get; set; }

        // Атрибут [TempData]: сообщение о результате операции
        [TempData]
        // Текст статусного сообщения
        public string StatusMessage { get; set; }

        // Вложенная модель данных формы смены пароля
        public class InputModel
        {
            // Обязательное поле — текущий пароль
            [Required(ErrorMessage = "Введите текущий пароль")]
            // Тип поля — пароль
            [DataType(DataType.Password)]
            // Подпись поля
            [Display(Name = "Текущий пароль")]
            // Текущий (старый) пароль пользователя
            public string OldPassword { get; set; }

            // Обязательное поле — новый пароль
            [Required(ErrorMessage = "Введите новый пароль")]
            // Ограничение длины: от 4 до 100 символов
            [StringLength(100, ErrorMessage = "Пароль должен быть от {2} до {1} символов.", MinimumLength = 4)]
            // Тип поля — пароль
            [DataType(DataType.Password)]
            // Подпись поля
            [Display(Name = "Новый пароль")]
            // Новый пароль пользователя
            public string NewPassword { get; set; }

            // Тип поля — пароль
            [DataType(DataType.Password)]
            // Подпись поля
            [Display(Name = "Подтвердите новый пароль")]
            // Атрибут [Compare]: значение должно совпадать с NewPassword
            [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
            // Подтверждение нового пароля
            public string ConfirmPassword { get; set; }
        }

        // Обработчик GET-запроса — при открытии страницы смены пароля
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

            // Проверка: есть ли у пользователя пароль (аккаунт мог быть создан через внешний вход)
            var hasPassword = await _userManager.HasPasswordAsync(user);
            // Если пароль отсутствует (внешний провайдер)
            if (!hasPassword)
            {
                // Перенаправление на страницу установки пароля
                return RedirectToPage("./SetPassword");
            }

            // Отображение страницы смены пароля
            return Page();
        }

        // Обработчик POST-запроса — при отправке формы смены пароля
        public async Task<IActionResult> OnPostAsync()
        {
            // Если данные формы не прошли валидацию
            if (!ModelState.IsValid)
            {
                // Повторный показ формы с ошибками валидации
                return Page();
            }

            // Загрузка текущего пользователя
            var user = await _userManager.GetUserAsync(User);
            // Если пользователь не найден
            if (user == null)
            {
                // Возврат ошибки 404
                return NotFound($"Не удалось загрузить пользователя с ID '{_userManager.GetUserId(User)}'.");
            }

            // Выполнение смены пароля (проверка старого пароля и установка нового)
            var changePasswordResult = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
            // Если смена пароля не удалась (например, неверный текущий пароль)
            if (!changePasswordResult.Succeeded)
            {
                // Перебор всех ошибок результата
                foreach (var error in changePasswordResult.Errors)
                {
                    // Добавление каждой ошибки в ModelState для отображения
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                // Повторный показ формы с ошибками
                return Page();
            }

            // Обновление cookie аутентификации пользователя (пароль изменился)
            await _signInManager.RefreshSignInAsync(user);
            // Запись события в журнал: пользователь сменил пароль
            _logger.LogInformation("Пользователь успешно сменил пароль.");
            // Сообщение об успешной смене пароля
            StatusMessage = "Пароль успешно изменён.";

            // Перезагрузка страницы смены пароля
            return RedirectToPage();
        }
    }
}