// Подключение пространства имён для атрибутов авторизации ([Authorize], [AllowAnonymous])
using Microsoft.AspNetCore.Authorization;
// Подключение пространства имён Identity для работы с UserManager (управление пользователями)
using Microsoft.AspNetCore.Identity;
// Подключение пространства имён ASP.NET Core MVC (контроллер, действия, атрибуты)
using Microsoft.AspNetCore.Mvc;
// Подключение пространства имён EF Core для асинхронных операций с БД (ToListAsync и др.)
using Microsoft.EntityFrameworkCore;
// Подключение пространства имён слоя данных (ApplicationDbContext)
using VotingSystem.Data;
// Подключение пространства имён с моделями данных (Candidate, Vote, CandidatesViewModel)
using VotingSystem.Models;
// Подключение пространства имён сервисов (JsonDataService)
using VotingSystem.Services;

// Объявление пространства имён контроллеров приложения
namespace VotingSystem.Controllers;

// Атрибут [Authorize] на уровне класса: все действия контроллера доступны только авторизованным
// Исключения помечены отдельно атрибутом [AllowAnonymous]
[Authorize]
// Контроллер кандидатов — голосование за кандидатов, просмотр списка и деталей
public class CandidatesController : Controller
{
    // Контекст базы данных для CRUD-операций с кандидатами и голосами
    private readonly ApplicationDbContext _context;
    // UserManager для получения ID текущего пользователя и работы с учётными записями
    private readonly UserManager<ApplicationUser> _userManager;
    // Сервис для экспорта данных в JSON-файлы
    private readonly JsonDataService _jsonService;

    // Конструктор контроллера — зависимости внедряются через DI (внедрение зависимостей)
    public CandidatesController(
        ApplicationDbContext context,          // Контекст БД
        UserManager<ApplicationUser> userManager, // Менеджер пользователей
        JsonDataService jsonService)           // Сервис JSON-экспорта
    {
        // Сохранение контекста БД в приватное поле
        _context = context;
        // Сохранение менеджера пользователей в приватное поле
        _userManager = userManager;
        // Сохранение JSON-сервиса в приватное поле
        _jsonService = jsonService;
    }

    // Атрибут: действие доступно даже неавторизованным пользователям (публичный просмотр списка)
    [AllowAnonymous]
    // Метод действия — отображает список кандидатов с фильтром по категории
    public async Task<IActionResult> Index(string? category)
    {
        // Асинхронная загрузка всех кандидатов из базы данных
        var candidates = await _context.Candidates.ToListAsync();

        // Группировка всех голосов по CandidateId и подсчёт количества голосов для каждого кандидата
        // Результат — словарь: ID кандидата → число голосов
        var voteCounts = await _context.Votes
            .GroupBy(v => v.CandidateId)      // Группировка голосов по ID кандидата
            .ToDictionaryAsync(g => g.Key, g => g.Count()); // Преобразование в словарь

        // Получение списка всех уникальных категорий кандидатов для выпадающего фильтра
        var categories = candidates
            .Select(c => c.Category)          // Выбор только категории каждого кандидата
            .Distinct()                        // Удаление дубликатов категорий
            .OrderBy(c => c)                   // Сортировка категорий по алфавиту
            .ToList();                         // Преобразование в список

        // Если пользователь выбрал категорию в фильтре (параметр не пустой)
        if (!string.IsNullOrEmpty(category))
            // Фильтрация кандидатов: оставляем только из выбранной категории
            candidates = candidates.Where(c => c.Category == category).ToList();

        // Создание модели представления для передачи данных в View
        var viewModel = new CandidatesViewModel
        {
            Candidates = candidates,          // Список (отфильтрованных) кандидатов
            VoteCounts = voteCounts,          // Словарь количества голосов
            SelectedCategory = category,      // Выбранная пользователем категория
            Categories = categories           // Все доступные категории
        };

        // Возврат представления Index с моделью viewModel
        return View(viewModel);
    }

    // Атрибут: просмотр деталей доступен и неавторизованным пользователям
    [AllowAnonymous]
    // Метод действия — отображает детальную информацию о конкретном кандидате
    public async Task<IActionResult> Details(int id)
    {
        // Поиск кандидата по ID в базе данных (по первичному ключу)
        var candidate = await _context.Candidates.FindAsync(id);
        // Если кандидат не найден — возврат ошибки 404 Not Found
        if (candidate == null)
            return NotFound();

        // Подсчёт общего количества голосов за данного кандидата (CountAsync с условием)
        var voteCount = await _context.Votes.CountAsync(v => v.CandidateId == id);

        // Флаг: голосовал ли текущий пользователь за кандидата (по умолчанию false)
        var userVote = false;
        // Если пользователь аутентифицирован (вошёл в систему)
        if (User.Identity?.IsAuthenticated == true)
        {
            // Получение ID текущего пользователя из claims
            var userId = _userManager.GetUserId(User);
            // Проверка: существует ли голос этого пользователя за данного кандидата
            userVote = await _context.Votes.AnyAsync(v => v.CandidateId == id && v.UserId == userId);
        }

        // Передача количества голосов в View через ViewBag (динамическое свойство)
        ViewBag.VoteCount = voteCount;
        // Передача флага "пользователь голосовал" в View через ViewBag
        ViewBag.UserVote = userVote;

        // Возврат представления Details с объектом кандидата
        return View(candidate);
    }

    // Атрибут [HttpPost]: метод обрабатывает только POST-запросы (отправку формы голосования)
    [HttpPost]
    // Метод действия — обработка голосования за кандидата
    public async Task<IActionResult> Vote(int candidateId, string? comment)
    {
        // Получение ID текущего авторизованного пользователя
        var userId = _userManager.GetUserId(User);
        // Если пользователь не авторизован — требование входа (401)
        if (userId == null)
            return Challenge();

        // Поиск кандидата в базе данных по ID
        var candidate = await _context.Candidates.FindAsync(candidateId);
        // Если кандидат не найден — возврат 404
        if (candidate == null)
            return NotFound();

        // Поиск существующего голоса: голосовал ли уже этот пользователь за этого кандидата
        var existingVote = await _context.Votes
            .FirstOrDefaultAsync(v => v.UserId == userId && v.CandidateId == candidateId);

        // Если голос уже существует (повторное голосование)
        if (existingVote != null)
        {
            // Сохранение сообщения об ошибке в TempData (переживает редирект)
            TempData["Error"] = "Вы уже голосовали за этого кандидата";
            // Редирект обратно на страницу деталей кандидата
            return RedirectToAction(nameof(Details), new { id = candidateId });
        }

        // Создание нового объекта Vote (голос)
        var vote = new Vote
        {
            CandidateId = candidateId,       // ID кандидата, за которого голосуют
            UserId = userId,                 // ID проголосовавшего пользователя
            VoteDate = DateTime.UtcNow,      // Текущая дата и время (UTC)
            Comment = comment                // Комментарий пользователя (может быть null)
        };

        // Добавление голоса в DbSet Vote (отслеживание для последующей вставки)
        _context.Votes.Add(vote);
        // Сохранение изменений — фактическая запись голоса в базу данных
        await _context.SaveChangesAsync();

        // Экспорт актуальных кандидатов и голосов в JSON-файлы
        await ExportToJson();

        // Сохранение сообщения об успехе в TempData
        TempData["Success"] = "Ваш голос учтён";
        // Редирект на страницу деталей кандидата
        return RedirectToAction(nameof(Details), new { id = candidateId });
    }

    // Атрибут [Authorize]: доступно только авторизованным пользователям (дополнительно к уровню класса)
    [Authorize]
    // Метод действия — отображает список голосов текущего пользователя
    public async Task<IActionResult> MyVotes()
    {
        // Получение ID текущего пользователя
        var userId = _userManager.GetUserId(User);
        // Загрузка всех голосов пользователя с данными кандидатов (Include — подгрузка связанных данных)
        var votes = await _context.Votes
            .Include(v => v.Candidate)       // Подгрузка данных кандидата для каждого голоса
            .Where(v => v.UserId == userId)  // Фильтрация по ID текущего пользователя
            .OrderByDescending(v => v.VoteDate) // Сортировка от новых к старым
            .ToListAsync();                  // Выполнение запроса

        // Возврат представления MyVotes со списком голосов пользователя
        return View(votes);
    }

    // Приватный асинхронный метод — экспорт кандидатов и голосов в JSON-файлы
    private async Task ExportToJson()
    {
        // Загрузка всех кандидатов из БД для экспорта
        var candidates = await _context.Candidates.ToListAsync();
        // Загрузка всех голосов из БД для экспорта
        var votes = await _context.Votes.ToListAsync();

        // Сохранение списка кандидатов в JSON-файл
        await _jsonService.SaveCandidatesAsync(candidates);
        // Сохранение списка голосов в JSON-файл
        await _jsonService.SaveVotesAsync(votes);
    }
}