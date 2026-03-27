using TankBattle.Models;

namespace TankBattle.Logic;

/// <summary>
/// 碰撞检测静态工具类 - 使用矩形相交检测所有碰撞关系
/// </summary>
public static class CollisionDetector
{
    /// <summary>
    /// 两矩形是否相交（基础碰撞检测，使用 Rectangle.IntersectsWith）
    /// </summary>
    public static bool IsColliding(Rectangle a, Rectangle b) => a.IntersectsWith(b);

    /// <summary>
    /// 检查指定矩形是否与任意墙壁（阻挡坦克）发生碰撞
    /// </summary>
    public static bool CollidesWithWalls(Rectangle bounds, List<Wall> walls)
    {
        foreach (Wall wall in walls)
        {
            if (wall.IsActive && wall.BlocksTank && IsColliding(bounds, wall.Bounds))
                return true;
        }
        return false;
    }

    /// <summary>
    /// 检查坦克是否即将越界（地图边界）
    /// </summary>
    /// <param name="bounds">坦克边界</param>
    /// <param name="mapWidth">地图像素宽度</param>
    /// <param name="mapHeight">地图像素高度</param>
    public static bool IsOutOfBounds(Rectangle bounds, int mapWidth, int mapHeight)
    {
        return bounds.Left < 0
            || bounds.Top < 0
            || bounds.Right > mapWidth
            || bounds.Bottom > mapHeight;
    }

    /// <summary>
    /// 检测子弹与墙壁碰撞，返回被击中的墙（如果有）
    /// </summary>
    public static Wall? BulletHitsWall(Bullet bullet, List<Wall> walls)
    {
        foreach (Wall wall in walls)
        {
            if (wall.IsActive && wall.BlocksBullets && IsColliding(bullet.Bounds, wall.Bounds))
                return wall;
        }
        return null;
    }

    /// <summary>
    /// 检测子弹是否击中基地
    /// </summary>
    public static bool BulletHitsEagle(Bullet bullet, Eagle eagle)
    {
        return eagle.IsActive && IsColliding(bullet.Bounds, eagle.Bounds);
    }

    /// <summary>
    /// 检测子弹是否击中玩家坦克（仅限敌方子弹）
    /// </summary>
    public static bool BulletHitsPlayer(Bullet bullet, PlayerTank player)
    {
        if (bullet.Owner != BulletOwner.Enemy) return false;
        return player.IsActive && IsColliding(bullet.Bounds, player.Bounds);
    }

    /// <summary>
    /// 检测子弹是否击中某个敌方坦克（仅限玩家子弹）
    /// </summary>
    public static EnemyTank? BulletHitsEnemy(Bullet bullet, List<EnemyTank> enemies)
    {
        if (bullet.Owner != BulletOwner.Player) return null;
        foreach (EnemyTank enemy in enemies)
        {
            if (enemy.IsActive && IsColliding(bullet.Bounds, enemy.Bounds))
                return enemy;
        }
        return null;
    }

    /// <summary>
    /// 检测两坦克是否互相推挤（坦克之间不可重叠）
    /// </summary>
    public static bool TanksOverlap(Rectangle a, Rectangle b) => IsColliding(a, b);
}
