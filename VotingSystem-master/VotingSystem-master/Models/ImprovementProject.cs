// Объявление пространства имён моделей приложения
namespace VotingSystem.Models;

// Модель проекта благоустройства — сущность, представляющая конкретный проект улучшения района
// Каждый проект привязан к одному району и может получить голоса от пользователей
public class ImprovementProject
{
    // Уникальный идентификатор проекта (первичный ключ в БД)
    public int Id { get; set; }
    // Внешний ключ — идентификатор района, которому принадлежит проект
    public int DistrictId { get; set; }
    // Название проекта (например, "Реконструкция парка 'Затеречный'")
    public string Title { get; set; } = string.Empty;
    // Описание проекта (что будет сделано)
    public string Description { get; set; } = string.Empty;
    // Категория проекта (Парки, Дороги, Спорт, Освещение и т.д.)
    public string Category { get; set; } = string.Empty;
    // Примерная стоимость проекта в рублях (строка, например "12 млн ₽")
    public string BudgetEstimate { get; set; } = string.Empty;
    // Навигационное свойство — ссылка на объект района (связь многие-к-одному)
    public District? District { get; set; }
    // Список голосов, отданных за этот проект (связь один-ко-многим)
    public List<ImprovementVote> Votes { get; set; } = new();
}
