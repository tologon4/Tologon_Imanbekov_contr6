using contr6;

List<TaskF>? tasks =TasksAvailIdent();
string site = "../../../site";
HttpServer server = new HttpServer(tasks);
await server.RunAsync(site, 8000);

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