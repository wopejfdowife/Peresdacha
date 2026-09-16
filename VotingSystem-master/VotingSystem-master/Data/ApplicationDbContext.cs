// Подключение пространства имён для IdentityDbContext (базовый контекст с поддержкой Identity)
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
// Подключение пространства имён Entity Framework Core (аспекты конфигурации модели, DbSet и т.д.)
using Microsoft.EntityFrameworkCore;
// Подключение пространства имён с моделями данных приложения
using VotingSystem.Models;

// Объявление пространства имён слоя данных
namespace VotingSystem.Data;

// Контекст базы данных приложения — наследуется от IdentityDbContext<ApplicationUser>,
// что добавляет поддержку таблиц Identity (Users, Roles и т.д.) к собственным таблицам
// Использует первичный конструктор (C# 12) для передачи параметров конфигурации
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    // DbSet<Candidate> — представление таблицы кандидатов в базе данных
    // Синтаксис Set<T>() использует expression-bodied property для краткости
    public DbSet<Candidate> Candidates => Set<Candidate>();
    // DbSet<Vote> — представление таблицы голосов за кандидатов
    public DbSet<Vote> Votes => Set<Vote>();
    // DbSet<District> — представление таблицы районов
    public DbSet<District> Districts => Set<District>();
    // DbSet<ImprovementProject> — представление таблицы проектов благоустройства
    public DbSet<ImprovementProject> ImprovementProjects => Set<ImprovementProject>();
    // DbSet<ImprovementVote> — представление таблицы голосов за проекты благоустройства
    public DbSet<ImprovementVote> ImprovementVotes => Set<ImprovementVote>();

    // Переопределение метода OnModelCreating — настройка модели данных (связи, индексы, ограничения)
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Вызов базовой реализации IdentityDbContext для настройки таблиц Identity
        base.OnModelCreating(builder);

        // Настройка связи Vote → Candidate: один кандидат имеет много голосов
        builder.Entity<Vote>()
            // Каждый Vote ссылается на одного Candidate через CandidateId
            .HasOne(v => v.Candidate)
            // У Candidate может быть много Vote (связь один-ко-многим, без навигации)
            .WithMany()
            // Внешний ключ — CandidateId
            .HasForeignKey(v => v.CandidateId)
            // При удалении кандидата — каскадное удаление всех его голосов
            .OnDelete(DeleteBehavior.Cascade);

        // Создание уникального составного индекса (UserId + CandidateId)
        // Гарантирует, что один пользователь может проголосовать за кандидата только один раз
        builder.Entity<Vote>()
            .HasIndex(v => new { v.UserId, v.CandidateId })
            .IsUnique();

        // Настройка связи ImprovementProject → District: один район имеет много проектов
        builder.Entity<ImprovementProject>()
            .HasOne(p => p.District)          // Каждый проект ссылается на один район
            .WithMany(d => d.Projects)        // У района много проектов (навигационное свойство Projects)
            .HasForeignKey(p => p.DistrictId) // Внешний ключ — DistrictId
            .OnDelete(DeleteBehavior.Cascade); // Каскадное удаление при удалении района

        // Настройка связи ImprovementVote → ImprovementProject: один проект имеет много голосов
        builder.Entity<ImprovementVote>()
            .HasOne(v => v.Project)           // Каждый ImprovementVote ссылается на один проект
            .WithMany(p => p.Votes)           // У проекта много голосов (навигационное свойство Votes)
            .HasForeignKey(v => v.ProjectId)  // Внешний ключ — ProjectId
            .OnDelete(DeleteBehavior.Cascade); // Каскадное удаление при удалении проекта

        // Создание уникального составного индекса (UserId + ProjectId)
        // Гарантирует, что один пользователь может проголосовать за проект только один раз
        builder.Entity<ImprovementVote>()
            .HasIndex(v => new { v.UserId, v.ProjectId })
            .IsUnique();
    }
}
