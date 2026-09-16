// Объявление пространства имён моделей приложения
namespace VotingSystem.Models;

// Модель представления (ViewModel) для страницы списка кандидатов
// Объединяет данные кандидатов, количество голосов и фильтры для передачи во View
public class CandidatesViewModel
{
    // Список кандидатов для отображения на странице (инициализируется пустым списком)
    public List<Candidate> Candidates { get; set; } = new();
    // Словарь: ID кандидата → количество голосов за него
    public Dictionary<int, int> VoteCounts { get; set; } = new();
    // Выбранная категория для фильтрации (null — показывать все категории)
    public string? SelectedCategory { get; set; }
    // Список всех уникальных категорий кандидатов (для выпадающего списка фильтров)
    public List<string> Categories { get; set; } = new();
}
