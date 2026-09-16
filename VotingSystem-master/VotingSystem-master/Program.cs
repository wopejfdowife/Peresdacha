// Подключение пространства имён для работы с ASP.NET Identity (система аутентификации и авторизации)
using Microsoft.AspNetCore.Identity;
// Подключение пространства имён для работы с Entity Framework Core (ORM для работы с базой данных)
using Microsoft.EntityFrameworkCore;
// Подключение пространства имён с контекстом базы данных приложения
using VotingSystem.Data;
// Подключение пространства имён с моделями данных приложения
using VotingSystem.Models;
// Подключение пространства имён с сервисами (JSON-экспорт данных)
using VotingSystem.Services;

// Создание билдера (конструктора) приложения ASP.NET Core с параметрами командной строки
var builder = WebApplication.CreateBuilder(args);

// Получение строки подключения к базе данных из файла конфигурации (appsettings.json)
// Если строка не найдена — выбрасывается исключение с описанием ошибки
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Регистрация контекста базы данных ApplicationDbContext в контейнере зависимостей (DI)
// Настройка провайдера SQLite с полученной строкой подключения
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Добавление фильтра для отображения страницы диагностики ошибок EF Core в режиме разработки
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Регистрация сервисов Identity (системы учётных записей пользователей) с типом ApplicationUser
// Настройка параметров пароля и аутентификации (ослабленные требования для учебного проекта)
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        // Не требовать подтверждение аккаунта (email-верификация) при регистрации
        options.SignIn.RequireConfirmedAccount = false;
        // Не требовать обязательную цифру в пароле
        options.Password.RequireDigit = false;
        // Минимальная длина пароля — 6 символов
        options.Password.RequiredLength = 6;
        // Не требовать спецсимволы в пароле
        options.Password.RequireNonAlphanumeric = false;
        // Не требовать заглавные буквы в пароле
        options.Password.RequireUppercase = false;
    })
    // Привязка Identity к контексту базы данных Entity Framework (хранилище пользователей в БД)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Регистрация сервисов MVC (контроллеры + представления) в контейнере зависимостей
builder.Services.AddControllersWithViews();
// Регистрация сервиса JsonDataService как Scoped (создаётся один экземпляр на запрос HTTP)
builder.Services.AddScoped<JsonDataService>();

// Сборка (создание) приложения из билдера — финальный этап конфигурации
var app = builder.Build();

// Проверка: если приложение запущено в режиме разработки
if (app.Environment.IsDevelopment())
{
    // Добавление middleware для перехода на страницу миграций EF Core (при ошибках БД)
    app.UseMigrationsEndPoint();
}
// Иначе — режим продакшн
else
{
    // Добавление обработчика исключений (перенаправление на страницу ошибок)
    app.UseExceptionHandler("/Home/Error");
    // Включение HSTS (HTTP Strict Transport Security) для принудительного HTTPS
    app.UseHsts();
}

// Перенаправление всех HTTP-запросов на HTTPS
app.UseHttpsRedirection();
// Настройка маршрутизации (определение URL-путей к контроллерам и страницам)
app.UseRouting();

// Включение middleware аутентификации (проверка, кто пользователь)
app.UseAuthentication();
// Включение middleware авторизации (проверка, имеет ли пользователь доступ к ресурсу)
app.UseAuthorization();

// Маппинг статических файлов (CSS, JS, изображения) с поддержкой кеширования (.NET 9 API)
app.MapStaticAssets();

// Регистрация маршрута по умолчанию: {controller=Home}/{action=Index}/{id?}
// Например: /Home/Index, /Candidates/Details/5
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets(); // Привязка статических ассетов к маршрутам

// Маппинг страниц Razor (Identity страницы: вход, регистрация, управление аккаунтом)
app.MapRazorPages()
   .WithStaticAssets();

// Создание временной области видимости (scope) для работы с контекстом базы данных
// Используется для автоматического применения миграций и заполнения БД начальными данными
using (var scope = app.Services.CreateScope())
{
    // Получение экземпляра ApplicationDbContext из контейнера зависимостей
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // Автоматическое применение всех миграций EF Core (создание/обновление структуры БД)
    context.Database.Migrate();
    // Заполнение базы данных начальными данными (кандидаты, районы, проекты)
    await DbSeeder.SeedAsync(context);
}

// Запуск приложения — начало прослушивания входящих HTTP-запросов
app.Run();
