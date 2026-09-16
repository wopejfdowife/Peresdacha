// Объявление пространства имён моделей приложения
namespace VotingSystem.Models;

// Модель информации о разработчике — используется на странице "О проекте"
public class DeveloperInfo
{
    // Полное имя разработчика (ФИО)
    public string FullName { get; set; } = string.Empty;
    // Роль разработчика в проекте (Fullstack, Backend, Frontend)
    public string Role { get; set; } = string.Empty;
    // Описание вклада разработчика в проект
    public string Description { get; set; } = string.Empty;
}
