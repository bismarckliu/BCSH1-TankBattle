using System.Text.Json;
using TankBattle.Logic;

namespace TankBattle.Persistence;

/// <summary>
/// 存档数据结构（序列化为 JSON 文件）
/// </summary>
public class SaveData
{
    public int HighScore     { get; set; } = 0;
    public int UnlockedLevel { get; set; } = 1;
    public List<ScoreEntry> TopScores { get; set; } = new List<ScoreEntry>();
}

/// <summary>
/// 文件持久化管理器 - 使用 .NET 内置 System.Text.Json 读写存档
/// 存档路径：%AppData%\TankBattle\save.json
/// </summary>
public class SaveManager
{
    private readonly string _savePath;

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true
    };

    public SaveManager()
    {
        // 存档目录：%AppData%\TankBattle\
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string folder = Path.Combine(appData, "TankBattle");
        Directory.CreateDirectory(folder);
        _savePath = Path.Combine(folder, "save.json");
    }

    /// <summary>
    /// 读取存档，若不存在则返回默认数据
    /// </summary>
    public SaveData LoadData()
    {
        if (!File.Exists(_savePath))
            return new SaveData();

        try
        {
            string json = File.ReadAllText(_savePath);
            return JsonSerializer.Deserialize<SaveData>(json) ?? new SaveData();
        }
        catch
        {
            // 文件损坏时返回默认值
            return new SaveData();
        }
    }

    /// <summary>
    /// 将存档数据写入 JSON 文件
    /// </summary>
    public void SaveData(SaveData data)
    {
        try
        {
            string json = JsonSerializer.Serialize(data, JsonOptions);
            File.WriteAllText(_savePath, json);
        }
        catch
        {
            // 写入失败时静默忽略（不影响游戏体验）
        }
    }

    /// <summary>
    /// 删除存档（重置游戏时使用）
    /// </summary>
    public void DeleteSave()
    {
        if (File.Exists(_savePath))
            File.Delete(_savePath);
    }
}
