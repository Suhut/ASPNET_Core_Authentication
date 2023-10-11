using System.Text.Json;

namespace DynamicAuthorization;
public class PermissionsDatabase
{
    private readonly string _dbPath;
    public PermissionsDatabase(
        IWebHostEnvironment env
        )
    {
        _dbPath = Path.Combine(env.ContentRootPath, "permissions.json");
    }

    private Dictionary<string, HashSet<string>>? Record => File.Exists(_dbPath)
        ? JsonSerializer.Deserialize<Dictionary<string, HashSet<string>>>(File.ReadAllText(_dbPath))
            : new();

    public bool HasPermission( 
        string userId,
        string permission
        )
    {
        var db = Record;
        return db.ContainsKey(userId) && db[userId].Contains(permission);
    }

    public void AddPermission(
        string userId, 
        string permission
        )
    {
        var db = Record;
        if(!db.ContainsKey(userId))
        {
            db[userId] = new HashSet<string>();
        }

        db[userId].Add(permission);

        File.WriteAllText(_dbPath, JsonSerializer.Serialize(db));
    }

    public void RemovePermission(
       string userId,
       string permission
       )
    {
        var db = Record;
        if (db[userId] == null || !db[userId].Contains(permission)) return;

        db[userId].Remove(permission);
        File.WriteAllText(_dbPath, JsonSerializer.Serialize(db, new JsonSerializerOptions()
        {
            WriteIndented = true,
        }));
    }

}