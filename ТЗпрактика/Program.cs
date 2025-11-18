using System.Text.Json;
class Program
{
    static void Main()
    {
        var repository = new RequestRepository();
        var service = new RequestService(repository);

        
        repository.LoadFromFile("requests.json");

        Console.WriteLine("=== Система учёта заявок ИТ-отдела ===");

        bool running = true;
        while (running)
        {
            DisplayMenu();
            string choice = Console.ReadLine() ?? "";

            switch (choice)
            {
                case "1":
                    CreateNewRequest(service);
                    break;
                case "2":
                    service.DisplayAllRequests();
                    break;
                case "3":
                    UpdateRequestStatus(service);
                    break;
                case "4":
                    SearchByApplicant(service);
                    break;
                case "5":
                    FilterByStatus(service);
                    break;
                case "6":
                    repository.SaveToFile("requests.json");
                    Console.WriteLine("Данные сохранены.");
                    break;
                case "0":
                    repository.SaveToFile("requests.json");
                    Console.WriteLine("Данные сохранены. Выход...");
                    running = false;
                    break;
                default:
                    Console.WriteLine("Неверный выбор. Попробуйте снова.");
                    break;
            }

            Console.WriteLine();
        }
    }

    static void DisplayMenu()
    {
        Console.WriteLine("1. Создать новую заявку");
        Console.WriteLine("2. Показать все заявки");
        Console.WriteLine("3. Изменить статус заявки");
        Console.WriteLine("4. Найти заявки по заявителю");
        Console.WriteLine("5. Фильтровать заявки по статусу");
        Console.WriteLine("6. Сохранить данные");
        Console.WriteLine("0. Выход");
        Console.Write("Выберите действие: ");
    }

    static void CreateNewRequest(RequestService service)
    {
        Console.Write("ФИО заявителя: ");
        string name = Console.ReadLine() ?? "";
        Console.Write("Категория (Техника/ПО/Сеть): ");
        string category = Console.ReadLine() ?? "";
        Console.Write("Описание проблемы: ");
        string description = Console.ReadLine() ?? "";

        service.CreateRequest(name, description, category);
    }

    static void UpdateRequestStatus(RequestService service)
    {
        Console.Write("ID заявки: ");
        if (int.TryParse(Console.ReadLine(), out int id))
        {
            Console.Write("Новый статус (Новая/В работе/Выполнена/Отменена): ");
            string status = Console.ReadLine() ?? "";
            Console.Write("Исполнитель: ");
            string executor = Console.ReadLine() ?? "";
            Console.Write("Комментарий: ");
            string comment = Console.ReadLine() ?? "";

            service.UpdateRequestStatus(id, status, executor, comment);
        }
        else
        {
            Console.WriteLine("Неверный ID.");
        }
    }

    static void SearchByApplicant(RequestService service)
    {
        Console.Write("ФИО заявителя для поиска: ");
        string name = Console.ReadLine() ?? "";
        service.SearchByApplicant(name);
    }

    static void FilterByStatus(RequestService service)
    {
        Console.Write("Статус для фильтрации: ");
        string status = Console.ReadLine() ?? "";
        service.FilterByStatus(status);
    }
}




public class Request
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = "Новая";
    public string ExecutorName { get; set; } = string.Empty;
    public DateTime? CompletedDate { get; set; }
    public string ExecutorComment { get; set; } = string.Empty;
}



public class RequestRepository
{
    private List<Request> _requests = new List<Request>();
    private int _nextId = 1;

    public List<Request> GetAll()
    {
        return _requests;
    }

    public void Add(Request request)
    {
        request.Id = _nextId++;
        request.CreatedDate = DateTime.Now;
        request.Status = "Новая";
        _requests.Add(request);
    }

    public Request? GetById(int id)
    {
        return _requests.FirstOrDefault(r => r.Id == id);
    }

    public List<Request> FindByStatus(string status)
    {
        return _requests
            .Where(r => r.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public List<Request> FindByApplicant(string applicantName)
    {
        return _requests
            .Where(r => r.ApplicantName.Contains(applicantName, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public bool Update(Request request)
    {
        var existing = GetById(request.Id);
        if (existing != null)
        {
            existing.Status = request.Status;
            existing.ExecutorName = request.ExecutorName;
            existing.CompletedDate = request.CompletedDate;
            existing.ExecutorComment = request.ExecutorComment;
            return true;
        }
        return false;
    }

    public void SaveToFile(string filename)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(_requests, options);
        File.WriteAllText(filename, json);
    }

    public void LoadFromFile(string filename)
    {
        if (File.Exists(filename))
        {
            var json = File.ReadAllText(filename);
            var requests = JsonSerializer.Deserialize<List<Request>>(json);
            if (requests != null)
            {
                _requests = requests;
                _nextId = _requests.Count > 0 ? _requests.Max(r => r.Id) + 1 : 1;
            }
        }
    }
}
public class RequestService
{
    private readonly RequestRepository _repository;

    public RequestService(RequestRepository repository)
    {
        _repository = repository;
    }

    public void CreateRequest(string applicantName, string description, string category)
    {
        var request = new Request
        {
            ApplicantName = applicantName,
            Description = description,
            Category = category
        };
        _repository.Add(request);
        Console.WriteLine($"Заявка #{request.Id} создана успешно!");
    }

    public void DisplayAllRequests()
    {
        var requests = _repository.GetAll();
        if (!requests.Any())
        {
            Console.WriteLine("Заявок нет.");
            return;
        }

        Console.WriteLine("\n=== Список всех заявок ===");
        foreach (var request in requests)
        {
            DisplayRequest(request);
        }
    }

    public void DisplayRequest(Request request)
    {
        Console.WriteLine($"ID: {request.Id}");
        Console.WriteLine($"Дата: {request.CreatedDate:dd.MM.yyyy HH:mm}");
        Console.WriteLine($"Заявитель: {request.ApplicantName}");
        Console.WriteLine($"Категория: {request.Category}");
        Console.WriteLine($"Описание: {request.Description}");
        Console.WriteLine($"Статус: {request.Status}");
        Console.WriteLine($"Исполнитель: {request.ExecutorName}");
        if (!string.IsNullOrEmpty(request.ExecutorComment))
            Console.WriteLine($"Комментарий: {request.ExecutorComment}");
        if (request.CompletedDate.HasValue)
            Console.WriteLine($"Дата выполнения: {request.CompletedDate:dd.MM.yyyy}");
        Console.WriteLine("---");
    }

    public void UpdateRequestStatus(int id, string status, string executorName = "", string comment = "")
    {
        var request = _repository.GetById(id);
        if (request != null)
        {
            request.Status = status;
            request.ExecutorName = executorName;
            request.ExecutorComment = comment;

            if (status == "Выполнена")
                request.CompletedDate = DateTime.Now;

            _repository.Update(request);
            Console.WriteLine($"Статус заявки #{id} изменен на '{status}'");
        }
        else
        {
            Console.WriteLine($"Заявка с ID {id} не найдена.");
        }
    }

    public void SearchByApplicant(string applicantName)
    {
        var requests = _repository.FindByApplicant(applicantName);
        if (requests.Any())
        {
            Console.WriteLine($"\n=== Найдены заявки для '{applicantName}' ===");
            foreach (var request in requests)
            {
                DisplayRequest(request);
            }
        }
        else
        {
            Console.WriteLine($"Заявки для '{applicantName}' не найдены.");
        }
    }

    public void FilterByStatus(string status)
    {
        var requests = _repository.FindByStatus(status);
        if (requests.Any())
        {
            Console.WriteLine($"\n=== Заявки со статусом '{status}' ===");
            foreach (var request in requests)
            {
                DisplayRequest(request);
            }
        }
        else
        {
            Console.WriteLine($"Заявки со статусом '{status}' не найдены.");
        }
    }
}

