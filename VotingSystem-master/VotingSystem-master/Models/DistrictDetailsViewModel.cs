// Объявление пространства имён моделей приложения
namespace VotingSystem.Models;

// Модель представления для страницы детальной информации о районе
// Объединяет данные района, количество голосов за проекты и уже проголосовавшие проекты пользователя
public class DistrictDetailsViewModel
{
    // Объект района со всеми его проектами (обязательное поле, не null)
    public District District { get; set; } = null!;
    // Словарь: ID проекта → количество голосов за этот проект
    public Dictionary<int, int> ProjectVoteCounts { get; set; } = new();
    // Множество ID проектов, за которые текущий пользователь уже проголосовал
    // Используется HashSet для быстрой проверки вхождения через Contains()
    public HashSet<int> VotedProjectIds { get; set; } = new();
}
