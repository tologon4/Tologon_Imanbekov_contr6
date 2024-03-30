using contr6;

List<TaskF> tasks = new List<TaskF>();
string site = "../../../site";
HttpServer server = new HttpServer(tasks);
await server.RunAsync(site, 8000);