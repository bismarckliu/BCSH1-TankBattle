using TankBattle.Models;

namespace TankBattle.Logic;

/// <summary>
/// 关卡配置，定义每关出现的敌人
/// </summary>
public record LevelConfig(int Level, List<(int GridX, int GridY, EnemyType Type)> Enemies);

/// <summary>
/// 关卡管理器 - 管理关卡加载、敌人生成和过关判断
/// </summary>
public class LevelManager
{
    public int CurrentLevel { get; private set; }
    public const int TotalLevels = 3;

    // 当前关卡剩余待出场敌人队列
    private Queue<(int GridX, int GridY, EnemyType Type)> _spawnQueue;

    // 出场计时器（避免所有敌人同时刷新）
    private int _spawnTimer;
    private const int SpawnInterval = 120; // 每2秒出一辆敌人

    // 场上最大同时存在敌人数
    private const int MaxEnemiesOnField = 4;

    // 每关敌人出生点（随机从上方3个出生点出发）
    private static readonly (int GridX, int GridY)[] SpawnPoints =
    {
        (0, 0), (8, 0), (15, 0)
    };

    private static readonly Random Rng = new Random();

    public LevelManager()
    {
        CurrentLevel = 1;
        _spawnQueue = new Queue<(int, int, EnemyType)>();
        _spawnTimer = 0;
    }

    /// <summary>
    /// 根据关卡索引生成敌人配置
    /// </summary>
    public void LoadLevel(int level)
    {
        CurrentLevel = Math.Clamp(level, 1, TotalLevels);
        _spawnQueue.Clear();
        _spawnTimer = SpawnInterval; // 第一个敌人马上出现

        List<(int, int, EnemyType)> enemies = BuildEnemyList(CurrentLevel);
        foreach (var e in enemies)
            _spawnQueue.Enqueue(e);
    }

    /// <summary>
    /// 构建当前关卡的敌人列表（关卡越高越难）
    /// </summary>
    private List<(int, int, EnemyType)> BuildEnemyList(int level)
    {
        List<(int, int, EnemyType)> list = new List<(int, int, EnemyType)>();

        int basicCount   = level switch { 1 => 6, 2 => 5, 3 => 4, _ => 6 };
        int fastCount    = level switch { 1 => 2, 2 => 4, 3 => 4, _ => 2 };
        int armoredCount = level switch { 1 => 0, 2 => 2, 3 => 4, _ => 0 };

        for (int i = 0; i < basicCount;   i++) list.Add((0, 0, EnemyType.Basic));
        for (int i = 0; i < fastCount;    i++) list.Add((0, 0, EnemyType.Fast));
        for (int i = 0; i < armoredCount; i++) list.Add((0, 0, EnemyType.Armored));

        // 打乱顺序
        return list.OrderBy(_ => Rng.Next()).ToList();
    }

    /// <summary>
    /// 每帧调用：按时间间隔从队列中出场敌人
    /// </summary>
    /// <param name="activeEnemies">当前场上的敌人列表</param>
    /// <param name="bullets">子弹列表（传给新生成的敌人）</param>
    public void Update(List<EnemyTank> activeEnemies, List<Bullet> bullets)
    {
        if (_spawnQueue.Count == 0) return;
        if (activeEnemies.Count >= MaxEnemiesOnField) return;

        _spawnTimer++;
        if (_spawnTimer < SpawnInterval) return;

        _spawnTimer = 0;
        var (_, _, type) = _spawnQueue.Dequeue();
        var (spawnX, spawnY) = SpawnPoints[Rng.Next(SpawnPoints.Length)];
        activeEnemies.Add(new EnemyTank(spawnX, spawnY, type, bullets));
    }

    /// <summary>
    /// 当前关卡是否已经通关（场上无敌人且队列为空）
    /// </summary>
    public bool IsLevelComplete(List<EnemyTank> activeEnemies)
        => _spawnQueue.Count == 0 && activeEnemies.Count == 0;

    /// <summary>
    /// 剩余待出场敌人数量（用于 HUD 显示）
    /// </summary>
    public int RemainingEnemies => _spawnQueue.Count;

    /// <summary>
    /// 切换到下一关，返回是否还有下一关
    /// </summary>
    public bool NextLevel()
    {
        if (CurrentLevel >= TotalLevels)
            return false;
        LoadLevel(CurrentLevel + 1);
        return true;
    }
}
