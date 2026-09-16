// Лицензионное соглашение: файл распространяется по лицензии MIT (.NET Foundation)
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// Директива: отключить проверку nullable-типов для всего файла
#nullable disable

// Пространство имён для атрибутов валидации ([Phone])
using System.ComponentModel.DataAnnotations;
// Пространство имён Identity (UserManager, SignInManager)
using Microsoft.AspNetCore.Identity;
// Пространство имён MVC (IActionResult)
using Microsoft.AspNetCore.Mvc;
// Пространство имён Razor Pages (PageModel)
using Microsoft.AspNetCore.Mvc.RazorPages;
// Пространство имён с моделью пользователя (ApplicationUser)
using VotingSystem.Models;

// Объявление пространства имён страниц Identity для раздела Account/Manage
namespace VotingSystem.Areas.Identity.Pages.Account.Manage
{
    // Модель страницы "Профиль" — редактирование номера телефона и просмотр информации
    public class IndexModel : PageModel
    {
        // UserManager — сервис управления пользователями
        private readonly UserManager<ApplicationUser> _userManager;
        // SignInManager — сервис управления входом (для обновления сессии)
        private readonly SignInManager<ApplicationUser> _signInManager;

        // Конструктор модели — зависимости через DI
        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            // Сохранение UserManager в поле
            _userManager = userManager;
            // Сохранение SignInManager в поле
            _signInManager = signInManager;
        }

        // Имя пользователя (логин) для отображения в форме
        public string Username { get; set; }

        // Атрибут [TempData]: сообщение о результате операции (сохраняется между запросами)
        [TempData]
        // Текст сообщения о статусе (например, "Профиль обновлён")
        public string StatusMessage { get; set; }

        // Атрибут [BindProperty]: привязка полей формы к свойству Input
        [BindProperty]
        // Данные формы профиля
        public InputModel Input { get; set; }

        // Вложенная модель данных формы профиля
        public class InputModel
        {
            // Атрибут: значение должно быть валидным номером телефона
            [Phone]
            // Подпись поля в форме
            [Display(Name = "Номер телефона")]
            // Номер телефона пользователя
            public string PhoneNumber { get; set; }
        }

        // Приватный метод — загрузка данных пользователя в модель для отображения
        private async Task LoadAsync(ApplicationUser user)
        {
            // Получение имени пользователя (логина)
            var userName = await _userManager.GetUserNameAsync(user);
            // Получение текущего номера телефона пользователя
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            // Сохранение имени пользователя в свойство для отображения
            Username = userName;

            // Заполнение модели входных данных текущими данными пользователя
            Input = new InputModel
            {
                PhoneNumber = phoneNumber  // Текущий номер телефона
            };
        }

        // Обработчик GET-запроса — при открытии страницы профиля
        public async Task<IActionResult> OnGetAsync()
        {
            // Загрузка текущего аутентифицированного пользователя
            var user = await _userManager.GetUserAsync(User);
            // Если пользователь не найден
            if (user == null)
            {
                // Возврат ошибки 404 с пояснением
                return NotFound($"Не удалось загрузить пользователя с ID '{_userManager.GetUserId(User)}'.");
            }

            // Загрузка данных пользователя в модель страницы
            await LoadAsync(user);
            // Отображение страницы
            return Page();
        }

        // Обработчик POST-запроса — при отправке формы сохранения профиля
        public async Task<IActionResult> OnPostAsync()
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
                // Перезагрузка данных пользователя (чтобы корректно отобразить форму)
                await LoadAsync(user);
                // Повторный показ страницы с ошибками
                return Page();
            }

            // Получение текущего номера телефона из базы данных
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            // Если новый номер телефона отличается от текущего
            if (Input.PhoneNumber != phoneNumber)
            {
                // Сохранение нового номера телефона через UserManager
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                // Если операция не удалась
                if (!setPhoneResult.Succeeded)
                {
                    // Сохранение ошибки в StatusMessage
                    StatusMessage = "Ошибка при установке номера телефона.";
                    // Перезагрузка страницы (редирект на неё же)
                    return RedirectToPage();
                }
            }

            // Обновление cookie аутентификации (если изменились данные пользователя)
            await _signInManager.RefreshSignInAsync(user);
            // Сообщение об успешном обновлении профиля
            StatusMessage = "Профиль обновлён";
            // Перезагрузка страницы (редирект на неё же)
            return RedirectToPage();
        }
    }
}