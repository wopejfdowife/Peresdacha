// Подключение пространства имён для атрибутов авторизации ([Authorize], [AllowAnonymous])
using Microsoft.AspNetCore.Authorization;
// Подключение пространства имён Identity для работы с UserManager
using Microsoft.AspNetCore.Identity;
// Подключение пространства имён ASP.NET Core MVC (контроллер, действия)
using Microsoft.AspNetCore.Mvc;
// Подключение пространства имён EF Core для асинхронных операций с БД
using Microsoft.EntityFrameworkCore;
// Подключение пространства имён слоя данных (ApplicationDbContext)
using VotingSystem.Data;
// Подключение пространства имён с моделями данных (District, ImprovementVote и др.)
using VotingSystem.Models;

// Объявление пространства имён контроллеров приложения
namespace VotingSystem.Controllers;

// Атрибут [Authorize] на уровне класса: доступ только авторизованным пользователям
// Публичные действия помечены отдельно атрибутом [AllowAnonymous]
[Authorize]
// Контроллер районов — голосование за проекты благоустройства районов
public class DistrictsController : Controller
{
    // Контекст базы данных для работы с районами, проектами и голосами
    private readonly ApplicationDbContext _context;
    // UserManager для получения ID текущего пользователя
    private readonly UserManager<ApplicationUser> _userManager;

    // Конструктор контроллера — зависимости внедряются через DI
    public DistrictsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        // Сохранение контекста БД в приватное поле
        _context = context;
        // Сохранение менеджера пользователей в приватное поле
        _userManager = userManager;
    }

    // Атрибут: список районов доступен и без авторизации
    [AllowAnonymous]
    // Метод действия — отображает список всех районов с их проектами
    public async Task<IActionResult> Index()
    {
        // Загрузка всех районов с подгрузкой связанных проектов (Include)
        var districts = await _context.Districts
            .Include(d => d.Projects)       // Подгрузка проектов благоустройства каждого района
            .ToListAsync();                // Выполнение запроса

        // Возврат представления Index со списком районов
        return View(districts);
    }

    // Атрибут: детальная информация доступна и без авторизации
    [AllowAnonymous]
    // Метод действия — отображает детальную информацию о районе и его проектах
    public async Task<IActionResult> Details(int id)
    {
        // Поиск района по ID с подгрузкой его проектов (первый подходящий или null)
        var district = await _context.Districts
            .Include(d => d.Projects)       // Подгрузка проектов района
            .FirstOrDefaultAsync(d => d.Id == id); // Поиск по ID

        // Если район не найден — возврат 404
        if (district == null)
            return NotFound();

        // Подсчёт количества голосов за каждый проект данного района
        // Группировка голосов по ProjectId, фильтр — проекты этого района
        var projectVoteCounts = await _context.ImprovementVotes
            .Where(v => v.Project!.DistrictId == id) // Только голоса проектов этого района
            .GroupBy(v => v.ProjectId)               // Группировка по ID проекта
            .ToDictionaryAsync(g => g.Key, g => g.Count()); // В словарь: проект → число голосов

        // Множество ID проектов, за которые проголосовал текущий пользователь (по умолчанию пустое)
        var votedProjectIds = new HashSet<int>();
        // Если пользователь аутентифицирован
        if (User.Identity?.IsAuthenticated == true)
        {
            // Получение ID текущего пользователя
            var userId = _userManager.GetUserId(User);
            // Загрузка ID проектов района, за которые голосовал этот пользователь
            var userVotes = await _context.ImprovementVotes
                .Where(v => v.UserId == userId && v.Project!.DistrictId == id) // Только его голоса в этом районе
                .Select(v => v.ProjectId)            // Выбор только ID проектов
                .ToListAsync();                      // Преобразование в список

            // Преобразование списка в HashSet для быстрой проверки Contains()
            votedProjectIds = new HashSet<int>(userVotes);
        }

        // Создание модели представления для передачи в View
        var viewModel = new DistrictDetailsViewModel
        {
            District = district,                      // Округ со всеми данными
            ProjectVoteCounts = projectVoteCounts,    // Количество голосов по проектам
            VotedProjectIds = votedProjectIds         // Проекты, за которые голосовал пользователь
        };

        // Возврат представления Details с моделью viewModel
        return View(viewModel);
    }

    // Атрибут [HttpPost]: обработка только POST-запросов (отправка формы голосования)
    [HttpPost]
    // Метод действия — обработка голосования за проект благоустройства
    public async Task<IActionResult> VoteProject(int projectId)
    {
        // Получение ID текущего авторизованного пользователя
        var userId = _userManager.GetUserId(User);
        // Если пользователь не авторизован — требование входа
        if (userId == null)
            return Challenge();

        // Поиск проекта благоустройства по ID в базе данных
        var project = await _context.ImprovementProjects.FindAsync(projectId);
        // Если проект не найден — возврат 404
        if (project == null)
            return NotFound();

        // Проверка: голосовал ли уже этот пользователь за этот проект
        var existing = await _context.ImprovementVotes
            .FirstOrDefaultAsync(v => v.UserId == userId && v.ProjectId == projectId);

        // Если голос уже существует (повторное голосование)
        if (existing != null)
        {
            // Сообщение об ошибке в TempData (переживает редирект)
            TempData["Error"] = "Вы уже голосовали за этот проект";
            // Редирект на страницу деталей района
            return RedirectToAction(nameof(Details), new { id = project.DistrictId });
        }

        // Создание нового объекта ImprovementVote (голос за проект)
        var vote = new ImprovementVote
        {
            ProjectId = projectId,          // ID проекта, за который голосуют
            UserId = userId,                // ID проголосовавшего пользователя
            VoteDate = DateTime.UtcNow      // Текущее время (UTC)
        };

        // Добавление голоса в DbSet ImprovementVotes (отслеживание для вставки)
        _context.ImprovementVotes.Add(vote);
        // Сохранение изменений — фактическая запись голоса в БД
        await _context.SaveChangesAsync();

        // Сообщение об успехе в TempData
        TempData["Success"] = "Ваш голос за проект учтён";
        // Редирект на страницу деталей района
        return RedirectToAction(nameof(Details), new { id = project.DistrictId });
    }

    // Атрибут [Authorize]: доступно только авторизованным пользователям
    [Authorize]
    // Метод действия — отображает список голосов текущего пользователя за проекты
    public async Task<IActionResult> MyVotes()
    {
        // Получение ID текущего пользователя
        var userId = _userManager.GetUserId(User);
        // Загрузка голосов пользователя с подгрузкой связанных данных
        // Include(v => v.Project) — данные проекта, ThenInclude — данные района проекта
        var votes = await _context.ImprovementVotes
            .Include(v => v.Project)          // Подгрузка проекта для каждого голоса
            .ThenInclude(p => p!.District)    // Подгрузка района внутри проекта
            .Where(v => v.UserId == userId)   // Фильтрация по текущему пользователю
            .OrderByDescending(v => v.VoteDate) // Сортировка от новых к старым
            .ToListAsync();                   // Выполнение запроса

        // Возврат представления MyVotes со списком голосов
        return View(votes);
    }
}