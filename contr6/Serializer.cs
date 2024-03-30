using System.Text.Json;

namespace contr6;

public class Serializer
{
    private static List<TaskF>? Tasks { get; set; }
    
    public static List<TaskF>? GetTasks()
    {
        if (File.Exists("../../../tasks.json"))
            return Tasks = Tasks ?? JsonSerializer.Deserialize<List<TaskF>>(File.ReadAllText("../../../tasks.json"));
        else
        {
            Console.WriteLine("Файл считывания не найден!\n Создаю файл ...");
            File.WriteAllText("../../../tasks.json", "");
            return new List<TaskF>();
        }
    }

    public static void OverrideFile(List<TaskF> tasks)
    {
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
        File.WriteAllText("../../../tasks.json", JsonSerializer.Serialize(tasks, options));
    }
}