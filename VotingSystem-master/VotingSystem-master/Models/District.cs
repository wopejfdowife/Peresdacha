// Объявление пространства имён моделей приложения
namespace VotingSystem.Models;

// Модель района — сущность, отображающая административный район Владикавказа
// Содержит информацию о районе и связанных проектах благоустройства
public class District
{
    // Уникальный идентификатор района (первичный ключ в БД)
    public int Id { get; set; }
    // Название района (например, "Затеречный муниципальный округ")
    public string Name { get; set; } = string.Empty;
    // Описание района (география, характеристики)
    public string Description { get; set; } = string.Empty;
    // URL-путь к изображению района (SVG-файл)
    public string ImageUrl { get; set; } = string.Empty;
    // Население района (количество жителей)
    public int Population { get; set; }
    // Площадь района в квадратных километрах
    public double Area { get; set; }
    // Дата создания записи о районе (по умолчанию — текущее время UTC)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // Список проектов благоустройства, принадлежащих этому району (связь один-ко-многим)
    public List<ImprovementProject> Projects { get; set; } = new();
}
