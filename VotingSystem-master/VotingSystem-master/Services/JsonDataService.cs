// Подключение пространства имён для работы с сериализацией JSON (System.Text.Json)
using System.Text.Json;
// Подключение пространства имён с моделями данных (Candidate, Vote)
using VotingSystem.Models;

// Объявление пространства имён сервисов приложения
namespace VotingSystem.Services;

// Сервис для работы с JSON-файлами — загрузка и сохранение кандидатов и голосов
// Используется как дополнительное хранилище данных (помимо SQLite)
public class JsonDataService
{
    // Приватное поле — путь к папке с JSON-файлами (json-data/)
    private readonly string _dataDir;

    // Конструктор сервиса — получает информацию о хосте приложения
    public JsonDataService(IWebHostEnvironment env)
    {
        // Формирование пути к папке json-data в корне проекта
        _dataDir = Path.Combine(env.ContentRootPath, "json-data");
        // Создание папки json-data, если она ещё не существует
        Directory.CreateDirectory(_dataDir);
    }

    // Асинхронный метод загрузки списка кандидатов из JSON-файла
    public async Task<List<Candidate>> LoadCandidatesAsync()
    {
        // Формирование полного пути к файлу candidates.json
        var path = Path.Combine(_dataDir, "candidates.json");
        // Если файл не существует — возвращаем пустой список
        if (!File.Exists(path))
            return new List<Candidate>();

        // Асинхронное чтение содержимого JSON-файла в строку
        var json = await File.ReadAllTextAsync(path);
        // Десериализация JSON-строки в список кандидатов; если null — возвращаем пустой список
        return JsonSerializer.Deserialize<List<Candidate>>(json) ?? new List<Candidate>();
    }

    // Асинхронный метод сохранения списка кандидатов в JSON-файл
    public async Task SaveCandidatesAsync(List<Candidate> candidates)
    {
        // Формирование полного пути к файлу candidates.json
        var path = Path.Combine(_dataDir, "candidates.json");
        // Сериализация списка кандидатов в JSON с форматированием (красивый вывод)
        var json = JsonSerializer.Serialize(candidates, new JsonSerializerOptions { WriteIndented = true });
        // Асинхронная запись JSON-строки в файл (перезапись содержимого)
        await File.WriteAllTextAsync(path, json);
    }

    // Асинхронный метод загрузки списка голосов из JSON-файла
    public async Task<List<Vote>> LoadVotesAsync()
    {
        // Формирование полного пути к файлу votes.json
        var path = Path.Combine(_dataDir, "votes.json");
        // Если файл не существует — возвращаем пустой список
        if (!File.Exists(path))
            return new List<Vote>();

        // Асинхронное чтение содержимого JSON-файла в строку
        var json = await File.ReadAllTextAsync(path);
        // Десериализация JSON-строки в список голосов; если null — возвращаем пустой список
        return JsonSerializer.Deserialize<List<Vote>>(json) ?? new List<Vote>();
    }

    // Асинхронный метод сохранения списка голосов в JSON-файл
    public async Task SaveVotesAsync(List<Vote> votes)
    {
        // Формирование полного пути к файлу votes.json
        var path = Path.Combine(_dataDir, "votes.json");
        // Сериализация списка голосов в JSON с форматированием
        var json = JsonSerializer.Serialize(votes, new JsonSerializerOptions { WriteIndented = true });
        // Асинхронная запись JSON-строки в файл
        await File.WriteAllTextAsync(path, json);
    }
}
