namespace Stateful;

public class Database : Dictionary<string, UserInfo>  
{
    public Database()
    {
        this["one"] = new UserInfo() { Name = "User One" };
        this["two"] = new UserInfo() { Name = "User Two" };
    }
}

public class UserInfo
{
    public string Name { get; set; }
    public string Impersonating { get; set; }
}