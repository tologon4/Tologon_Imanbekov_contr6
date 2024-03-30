using System.Collections.Specialized;
using System.Net;
using RazorEngine;
using RazorEngine.Templating;
using Encoding = System.Text.Encoding;

namespace contr6;

public class HttpServer
{
    private string _siteDirectory; 
    private HttpListener _listener; 
    private int _port;
    private List<TaskF> Tasks;
    public HttpServer(List<TaskF> tasks)
    {
        Tasks = tasks;
    }
    
    public async Task RunAsync(string path, int port)
    {
        _siteDirectory = path;
        _port = port;
  
        _listener = new HttpListener();
        _listener.Prefixes.Add("http://localhost:" + _port.ToString() + "/");
        _listener.Start();

        Console.WriteLine($"Сервер запущен на порту: {port}");
        Console.WriteLine($"Файлы сайта лежат в папке: {path}");
  
        await ListenAsync();
    }
    
    public void Stop()
    {
        _listener.Abort();
        _listener.Stop();
    }
    private async Task ListenAsync()
    {
        try
        {
            while (true)
            {
                HttpListenerContext context = await _listener.GetContextAsync();
                Process(context);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
    
    async Task AddNewTaskAsync(Stream stream)
    {
        Dictionary<string, string> taskInfo = new Dictionary<string, string>();
        StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
        string? buffer = await streamReader.ReadToEndAsync();
        if (buffer.Contains("delete") || buffer.Contains("todone"))
        {
            string[] actAndId = buffer.Split('=');
            TaskDelDone(actAndId[0], actAndId[actAndId.Length-1]);
        }
        else
        {
            string[] catName = buffer.Split('&');
            foreach (var prop in catName)
            {
                string[] buffer2 = prop.Split('=');
                taskInfo.Add(buffer2[0], string.Join(' ', buffer2[buffer2.Length-1].Split('+')));
            }
            Tasks.Add(new TaskF(taskInfo["heading"],taskInfo["exName"], taskInfo["description"]));
        }
        
        Serializer.OverrideFile(Tasks);
    }

    void TaskDelDone(string taskAct, string id)
    {
        if (taskAct=="delete")
            DeleteTask(int.Parse(id));
        else
            ToDoneTask(int.Parse(id));
    }
    private void Process(HttpListenerContext context)
    {
        if (context.Request.HttpMethod=="POST")
        {
            var stream = context.Request.InputStream;
            AddNewTaskAsync(stream);
            context.Response.Redirect("http://localhost:8000/index.html");
        }
        NameValueCollection query = context.Request.QueryString;
        string filename = context.Request.Url.AbsolutePath;
        Console.WriteLine(filename);
        filename = filename.Substring(1);
        filename = Path.Combine(_siteDirectory, filename);
        if (File.Exists(filename))
        {
            try
            {
                string content = "";
                if (filename.Contains("html"))
                {
                    content = BuildHtml(filename, query["id"]);
                }
                else
                    content = File.ReadAllText(filename);
                
                byte[] htmlBytes = Encoding.UTF8.GetBytes(content);
                Stream fileStream = new MemoryStream(htmlBytes);
                
                context.Response.ContentType = GetContentType(filename);
                context.Response.ContentLength64 = fileStream.Length;
                byte[] buffer = new byte[16 * 1024]; 
                int dataLength;
                do
                {
                    dataLength = fileStream.Read(buffer, 0, buffer.Length);
                    context.Response.OutputStream.Write(buffer, 0, dataLength);
                } while (dataLength > 0);
                fileStream.Close();
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.OutputStream.Flush();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }
        else
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        context.Response.OutputStream.Close();
    }
    private static string GetContentType(string filename)
    {
        var dictionary = new Dictionary<string, string>
        {
            { ".css", "text/css" },
            { ".html", "text/html; charset=utf-8" }, 
            { ".ico", "image/x-icon" },
            { ".js", "application/x-javascript" },
            { ".json", "application/json" },
            { ".png", "image/png" }
        };
        string contentType = "";
        string fileExtension = Path.GetExtension(filename);
        dictionary.TryGetValue(fileExtension, out contentType);
        return contentType;
    }
    void ToDoneTask(int id)
    {
        foreach (var task in Tasks)
            if (task.Id == id)
            {
                task.Status = "done";
                task.DoneDate = DateTime.Today.ToString("dd-MM-yyyy");
            }
        Serializer.OverrideFile(Tasks);
    }

    void DeleteTask(int id)
    {
        foreach (var task in Tasks)
            if (task.Id == id)
                Tasks.Remove(task);
        Serializer.OverrideFile(Tasks);
    }
    private string BuildHtml(string fileName, string? taskId)
    {
        string html = "";
        string layoutPath = "../../../site/layout.html";
        var razorService = Engine.Razor;
        if (!razorService.IsTemplateCached("layout", null))
            razorService.AddTemplate("layout", File.ReadAllText(layoutPath));
        if (!razorService.IsTemplateCached(fileName, null))
        {
            razorService.AddTemplate(fileName, File.ReadAllText(fileName));
            razorService.Compile(fileName);
        }
                
        if (fileName == $"{_siteDirectory}/index.html")
            return razorService.Run(fileName, null, new
            {
                TasksList = Tasks,
            });
        else if (fileName == $"{_siteDirectory}/task.html" )
        {
            return razorService.Run(fileName, null, new
            {
                Task = Tasks.FirstOrDefault(task => task.Id == int.Parse(taskId))
            });
        }
        else
        {
            return razorService.Run(fileName, null, new
            {
                TasksList = Tasks,
            });
        }
        
    }
    
}