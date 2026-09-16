// Подключение пространства имён System.Diagnostics для работы с Activity (трассировка запросов)
using System.Diagnostics;
// Подключение пространства имён ASP.NET Core MVC (базовый класс Controller, атрибуты)
using Microsoft.AspNetCore.Mvc;
// Подключение пространства имён с моделями данных (DeveloperInfo, ErrorViewModel)
using VotingSystem.Models;

// Объявление пространства имён контроллеров приложения
namespace VotingSystem.Controllers;

// Контроллер главной страницы — отвечает за страницы: Главная, О проекте, Ошибка
public class HomeController : Controller
{
    // Метод действия (Action) — отображает главную страницу сайта
    public IActionResult Index()
    {
        // Возврат представления Index (страница с описанием системы голосования)
        return View();
    }

    // Метод действия — отображает страницу "О проекте" с информацией о разработчиках
    public IActionResult About()
    {
        // Создание списка разработчиков проекта с их ролями и описанием вклада
        var developers = new List<DeveloperInfo>
        {
            // Разработчик 1 — Неграш Никита (архитектура, серверная логика, UI)
            new() { FullName = "Неграш Никита", Role = "Fullstack-разработчик", Description = "Разработка архитектуры приложения, серверной логики и пользовательского интерфейса" },
            // Разработчик 2 — Дзагоев Марат (серверная логика, API, БД)
            new() { FullName = "Дзагоев Марат", Role = "Backend-разработчик", Description = "Разработка серверной логики, API и базы данных" },
            // Разработчик 3 — Тотоев Александр (UI и вёрстка)
            new() { FullName = "Тотоев Александр", Role = "Frontend-разработчик", Description = "Разработка пользовательского интерфейса и вёрстка страниц" }
        };

        // Возврат представления About с переданным списком разработчиков
        return View(developers);
    }

    // Атрибут: запретить кеширование ответа (страница ошибок не должна кешироваться)
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    // Метод действия — отображает страницу ошибок
    public IActionResult Error()
    {
        // Возврат представления Error; RequestId — ID текущего запроса
        // (Activity.Current?.Id — ID трассы, либо HttpContext.TraceIdentifier — ID запроса)
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}