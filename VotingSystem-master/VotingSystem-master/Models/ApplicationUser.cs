// Подключение пространства имён ASP.NET Identity для базового класса пользователя
using Microsoft.AspNetCore.Identity;

// Объявление пространства имён моделей приложения
namespace VotingSystem.Models;

// Расширение стандартного класса IdentityUser собственными полями пользователя
// IdentityUser уже содержит: Id, Email, UserName, PasswordHash и др.
public class ApplicationUser : IdentityUser
{
    // Имя пользователя (добавлено к стандартным полям Identity)
    public string FirstName { get; set; } = string.Empty;
    // Фамилия пользователя
    public string LastName { get; set; } = string.Empty;
    // Регион проживания (по умолчанию — Владикавказ)
    public string Region { get; set; } = "Владикавказ";
}
