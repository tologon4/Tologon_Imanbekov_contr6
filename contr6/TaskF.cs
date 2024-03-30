namespace contr6;

public class TaskF
{
    public int Id { get; set; }
    public string Heading { get; set; }
    public string ExecutorName { get; set; }
    public string CreatedDate { get; set; } 
    public string Status { get; set; }
    public string Description { get; set; }
    private static int tick = 0;
    public string DoneDate { get; set; }

    public TaskF(string heading, string executorName, string description)
    {
        Id = Interlocked.Increment(ref tick);
        Heading = heading;
        ExecutorName = executorName;
        Description = description;
        CreatedDate = DateTime.Today.ToString("dd-MM-yyyy");
        Status = "new";
    }
    
}