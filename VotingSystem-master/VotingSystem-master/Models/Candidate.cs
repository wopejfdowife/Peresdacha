// Объявление пространства имён, к которому относится модель кандидата
namespace VotingSystem.Models;

// Модель кандидата — сущность, отображающая информацию о человеке, за которого можно проголосовать
public class Candidate
{
    // Уникальный идентификатор кандидата (первичный ключ в базе данных)
    public int Id { get; set; }
    // Полное ФИО кандидата (инициализируется пустой строкой для избежания null)
    public string FullName { get; set; } = string.Empty;
    // Краткое описание кандидата (должность, достижения)
    public string Description { get; set; } = string.Empty;
    // Подробная биография кандидата
    public string Biography { get; set; } = string.Empty;
    // Категория кандидата (Культура, Спорт, Образование и т.д.)
    public string Category { get; set; } = string.Empty;
    // URL-путь к фотографии кандидата (путь к SVG-файлу в wwwroot)
    public string PhotoUrl { get; set; } = string.Empty;
    // Дата и время создания записи (по умолчанию — текущее время по UTC)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
