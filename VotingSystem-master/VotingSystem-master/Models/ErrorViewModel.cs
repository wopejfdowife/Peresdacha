// Объявление пространства имён моделей приложения
namespace VotingSystem.Models;

// Модель представления для страницы ошибок — передаёт данные об ошибке во View
public class ErrorViewModel
{
    // Уникальный идентификатор запроса (для диагностики ошибок; может быть null)
    public string? RequestId { get; set; }

    // Вычисляемое свойство: возвращает true, если RequestId не пустой и не null
    // Используется в представлении для условия отображения ID запроса
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
