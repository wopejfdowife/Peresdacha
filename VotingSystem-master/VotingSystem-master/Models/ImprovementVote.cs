// Объявление пространства имён моделей приложения
namespace VotingSystem.Models;

// Модель голоса за проект благоустройства — сущность, фиксирующая голосование пользователя за проект
public class ImprovementVote
{
    // Уникальный идентификатор голоса (первичный ключ в БД)
    public int Id { get; set; }
    // Внешний ключ — идентификатор проекта, за который отдан голос
    public int ProjectId { get; set; }
    // Идентификатор пользователя (Identity), который проголосовал
    public string UserId { get; set; } = string.Empty;
    // Дата и время голосования (по умолчанию — текущее время UTC)
    public DateTime VoteDate { get; set; } = DateTime.UtcNow;
    // Навигационное свойство — ссылка на проект (связь многие-к-одному)
    public ImprovementProject? Project { get; set; }
}
