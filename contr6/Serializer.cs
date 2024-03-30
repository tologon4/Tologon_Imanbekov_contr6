using System.Text.Json;

namespace contr6;

public class Serializer
{
    private static List<Task>? Tasks { get; set; }
    
    public static List<Task>? GetTasks()
    {
        if (File.Exists("../../../tasks.json"))
            return Tasks = Tasks ?? JsonSerializer.Deserialize<List<Task>>(File.ReadAllText("../../../tasks.json"));
        else
        {
            Console.WriteLine("Файл считывания не найден!\n Создаю файл ...");
            File.WriteAllText("../../../tasks.json", "");
            return new List<Task>();
        }
    }

    public static void OverrideFile(List<Task> tasks)
    {
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
        File.WriteAllText("../../../tasks.json", JsonSerializer.Serialize(tasks, options));
    }
}