// Лицензионное соглашение: файл распространяется по лицензии MIT (.NET Foundation)
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// Директива: отключить проверку nullable-типов для всего файла
#nullable disable

// Пространство имён для базовых типов (String, StringComparison)
using System;
// Пространство имён для работы с ViewContext (контекст текущего представления)
using Microsoft.AspNetCore.Mvc.Rendering;

// Объявление пространства имён страниц Identity для раздела Account/Manage
namespace  VotingSystem.Areas.Identity.Pages.Account.Manage
{
    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    // Статический класс с константами-названиями страниц и методами определения активной страницы
    public static class ManageNavPages
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // Название страницы "Профиль" (используется для навигации и подсветки активного пункта)
        public static string Index => "Index";

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // Название страницы "Email"
        public static string Email => "Email";

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // Название страницы "Смена пароля"
        public static string ChangePassword => "ChangePassword";

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // Название страницы "Скачать персональные данные"
        public static string DownloadPersonalData => "DownloadPersonalData";

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // Название страницы "Удалить персональные данные"
        public static string DeletePersonalData => "DeletePersonalData";

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // Название страницы "Внешние входы" (вход через сторонние сервисы)
        public static string ExternalLogins => "ExternalLogins";

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // Название страницы "Персональные данные"
        public static string PersonalData => "PersonalData";

        // Метод: возвращает CSS-класс "active", если активна страница Профиль (Index)
        public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // Метод: возвращает CSS-класс "active", если активна страница Email
        public static string EmailNavClass(ViewContext viewContext) => PageNavClass(viewContext, Email);

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // Метод: возвращает CSS-класс "active", если активна страница Смены пароля
        public static string ChangePasswordNavClass(ViewContext viewContext) => PageNavClass(viewContext, ChangePassword);

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // Метод: возвращает CSS-класс "active", если активна страница Скачивания данных
        public static string DownloadPersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, DownloadPersonalData);

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // Метод: возвращает CSS-класс "active", если активна страница Удаления данных
        public static string DeletePersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, DeletePersonalData);

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // Метод: возвращает CSS-класс "active", если активна страница Внешних входов
        public static string ExternalLoginsNavClass(ViewContext viewContext) => PageNavClass(viewContext, ExternalLogins);

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // Метод: возвращает CSS-класс "active", если активна страница Персональных данных
        public static string PersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, PersonalData);

        // Общий метод определения активной страницы в меню навигации
        public static string PageNavClass(ViewContext viewContext, string page)
        {
            // Определение активной страницы: из ViewData["ActivePage"] (если задано)
            var activePage = viewContext.ViewData["ActivePage"] as string
                // Иначе — берём имя файла текущей страницы (без расширения)
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            // Сравнение активной страницы с переданной (без учёта регистра символов)
            // Если совпадают — возврат CSS-класса "active", иначе null
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}