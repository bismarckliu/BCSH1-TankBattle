using TankBattle.Models;

namespace TankBattle.Logic;

/// <summary>
/// 敌方 AI 状态类型
/// </summary>
public enum AiState
{
    Patrol,   // 巡逻：随机移动
    Chase,    // 追踪：直线冲向玩家或基地
    Shoot     // 射击：瞄准玩家开火
}

/// <summary>
/// 敌方 AI 控制器 - 使用简单状态机驱动敌方坦克行为
/// 状态：巡逻 → 发现玩家 → 射击/追踪
/// </summary>
public class EnemyAI
{
    private static readonly Random Rng = new Random();

    // 每隔多少帧重新决策移动方向（约 2 秒）
    private const int PatrolInterval = 80;

    // 发现玩家的最大直线距离（像素）
    private const int DetectionRange = 320;

    /// <summary>
    /// 更新单个敌方坦克的 AI 行为
    /// </summary>
    /// <param name="enemy">敌方坦克</param>
    /// <param name="player">玩家坦克</param>
    /// <param name="walls">墙壁列表</param>
    /// <param name="mapWidth">地图宽度</param>
    /// <param name="mapHeight">地图高度</param>
    public void Update(EnemyTank enemy, PlayerTank player, List<Wall> walls, int mapWidth, int mapHeight)
    {
        // --- 移动决策 ---
        if (enemy.AiTimer % PatrolInterval == 0)
        {
            DecideDirection(enemy, player, mapWidth, mapHeight);
        }

        // 移动并检查碰撞
        enemy.Move();

        Rectangle newBounds = enemy.Bounds;
        bool hitWall = CollisionDetector.CollidesWithWalls(newBounds, walls);
        bool outOfBounds = CollisionDetector.IsOutOfBounds(newBounds, mapWidth, mapHeight);

        if (hitWall || outOfBounds)
        {
            enemy.UndoMove();
            // 碰撞后立即换方向
            enemy.AiDirection = PickRandomDirection(enemy.AiDirection);
        }

        // --- 射击决策 ---
        TryAiShoot(enemy, player);
    }

    /// <summary>
    /// 决定 AI 移动方向：如果玩家在直线视野内则追踪，否则随机巡逻
    /// </summary>
    private void DecideDirection(EnemyTank enemy, PlayerTank player, int mapWidth, int mapHeight)
    {
        if (!player.IsActive) 
        {
            enemy.AiDirection = PickRandomDirection(enemy.AiDirection);
            return;
        }

        int dx = player.X - enemy.X;
        int dy = player.Y - enemy.Y;
        double dist = Math.Sqrt(dx * dx + dy * dy);

        // 在范围内：大概率朝玩家方向移动
        if (dist < DetectionRange && Rng.Next(100) < 60)
        {
            // 选择水平或垂直中距离更短的轴移动
            if (Math.Abs(dx) > Math.Abs(dy))
                enemy.AiDirection = dx > 0 ? Direction.Right : Direction.Left;
            else
                enemy.AiDirection = dy > 0 ? Direction.Down : Direction.Up;
        }
        else
        {
            // 随机巡逻
            enemy.AiDirection = PickRandomDirection(enemy.AiDirection);
        }
    }

    /// <summary>
    /// 随机选择一个方向（避免重复相同方向导致卡墙）
    /// </summary>
    private Direction PickRandomDirection(Direction current)
    {
        Direction[] directions = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

        // 70% 概率选不同方向
        if (Rng.Next(100) < 70)
        {
            Direction newDir;
            do
            {
                newDir = directions[Rng.Next(directions.Length)];
            }
            while (newDir == current);
            return newDir;
        }

        return directions[Rng.Next(directions.Length)];
    }

    /// <summary>
    /// AI 射击判断：若玩家与坦克同行或同列，尝试开火
    /// </summary>
    private void TryAiShoot(EnemyTank enemy, PlayerTank player)
    {
        if (!player.IsActive) return;

        int ex = enemy.X + 16; // 敌坦克中心
        int ey = enemy.Y + 16;
        int px = player.X + 16;
        int py = player.Y + 16;

        bool sameColumn = Math.Abs(ex - px) < 20; // 同列（水平误差<20px）
        bool sameRow    = Math.Abs(ey - py) < 20; // 同行（垂直误差<20px）

        if (sameColumn)
        {
            // 调整炮口朝向玩家（上/下）
            enemy.AiDirection = ey > py ? Direction.Up : Direction.Down;
            enemy.TryShoot();
        }
        else if (sameRow)
        {
            // 调整炮口朝向玩家（左/右）
            enemy.AiDirection = ex > px ? Direction.Left : Direction.Right;
            enemy.TryShoot();
        }
        else
        {
            // 不在视线内，以 30% 概率随机射击
            if (Rng.Next(100) < 3) // 每帧3%概率 ≈ 约2秒一次
                enemy.TryShoot();
        }
    }
}
