namespace TankBattle.Logic;

/// <summary>
/// 独立高分记录条目
/// </summary>
public class ScoreEntry
{
    public int Score { get; set; }
    public string Date { get; set; } = string.Empty;
    public int LevelReached { get; set; }
}

/// <summary>
/// 分数管理器 - 管理当前局分数并与存档交互
/// </summary>
public class ScoreManager
{
    private readonly Persistence.SaveManager _saveManager;

    public int CurrentScore { get; private set; }
    public int HighScore    { get; private set; }

    public ScoreManager(Persistence.SaveManager saveManager)
    {
        _saveManager = saveManager;
        var data = saveManager.LoadData();
        HighScore = data.HighScore;
        CurrentScore = 0;
    }

    /// <summary>
    /// 增加分数，若超过历史最高分则同步更新
    /// </summary>
    public void Add(int points)
    {
        CurrentScore += points;
        if (CurrentScore > HighScore)
            HighScore = CurrentScore;
    }

    /// <summary>
    /// 过关奖励：当前关卡号 × 500 分
    /// </summary>
    public void AddLevelBonus(int level) => Add(level * 500);

    /// <summary>
    /// 保存高分到文件
    /// </summary>
    public void SaveHighScore(int levelReached)
    {
        var data = _saveManager.LoadData();
        if (CurrentScore > data.HighScore)
            data.HighScore = CurrentScore;
        data.UnlockedLevel = levelReached;

        // 添加到历史记录（最多保留5条）
        data.TopScores.Add(new ScoreEntry
        {
            Score = CurrentScore,
            Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            LevelReached = levelReached
        });

        if (data.TopScores.Count > 5)
            data.TopScores = data.TopScores.OrderByDescending(s => s.Score).Take(5).ToList();

        _saveManager.SaveData(data);
    }

    /// <summary>
    /// 重置当前局分数（新游戏时调用）
    /// </summary>
    public void Reset() => CurrentScore = 0;
}
