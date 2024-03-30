using contr6;

List<TaskF>? tasks =TasksAvailIdent();
string site = "../../../site";
HttpServer server = new HttpServer(tasks);
await server.RunAsync(site, 8000);

void ToDoneTask(int id)
{
    foreach (var task in tasks)
        if (task.Id == id)
            task.Status = "done";
}

void DeleteTask(int id)
{
    foreach (var task in tasks)
        if (task.Id == id)
            tasks.Remove(task);
}

List<TaskF> TasksAvailIdent()
{
    try
    {
        List<TaskF>? TasksFromFile = Serializer.GetTasks();
        if (TasksFromFile != null && TasksFromFile.Count > 0)
            return TasksFromFile;
        else
            return new List<TaskF>();
    }
    catch (System.Text.Json.JsonException)
    {
        return new List<TaskF>();
    }
    
}