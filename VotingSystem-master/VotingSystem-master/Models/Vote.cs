// Объявление пространства имён для моделей данных
namespace VotingSystem.Models;

// Модель голоса — сущность, отражающая факт голосования пользователя за кандидата
public class Vote
{
    // Уникальный идентификатор голоса (первичный ключ в БД)
    public int Id { get; set; }
    // Внешний ключ — идентификатор кандидата, за которого отдан голос
    public int CandidateId { get; set; }
    // Идентификатор пользователя (Identity), который проголосовал
    public string UserId { get; set; } = string.Empty;
    // Дата и время голосования (по умолчанию — текущее время UTC)
    public DateTime VoteDate { get; set; } = DateTime.UtcNow;
    // Необязательный комментарий пользователя к голосу (может быть null)
    public string? Comment { get; set; }

    // Навигационное свойство — ссылка на объект кандидата (связь многие-к-одному)
    public Candidate? Candidate { get; set; }
}
