namespace TankBattle.Models;

/// <summary>
/// 方向枚举 - 用于坦克和子弹的移动方向
/// </summary>
public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

/// <summary>
/// 所有游戏对象的抽象基类，封装位置、尺寸、速度等公共属性
/// </summary>
public abstract class GameObject
{
    // 游戏对象在画布上的位置与尺寸
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; protected set; }
    public int Height { get; protected set; }

    // 移动速度（每 Tick 移动像素数）
    public int Speed { get; protected set; }

    // 当前朝向
    public Direction Direction { get; set; }

    // 是否激活（false 表示已销毁，等待从列表移除）
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 碰撞矩形，基于当前位置和尺寸
    /// </summary>
    public Rectangle Bounds => new Rectangle(X, Y, Width, Height);

    protected GameObject(int x, int y, int width, int height, int speed)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Speed = speed;
    }

    /// <summary>
    /// 每帧更新游戏对象状态（子类重写）
    /// </summary>
    public abstract void Update();

    /// <summary>
    /// 绘制游戏对象（子类重写）
    /// </summary>
    /// <param name="g">Graphics 绘图上下文</param>
    public abstract void Draw(Graphics g);
}
