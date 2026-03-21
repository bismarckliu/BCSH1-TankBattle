namespace TankBattle.Models;

/// <summary>
/// 子弹类型：区分玩家子弹和敌方子弹
/// </summary>
public enum BulletOwner
{
    Player,
    Enemy
}

/// <summary>
/// 子弹类 - 继承 GameObject，每帧沿朝向匀速移动
/// </summary>
public class Bullet : GameObject
{
    // 子弹宽高（像素）
    private const int BulletSize = 8;

    // 子弹速度（每 Tick 像素数），比坦克快
    private const int BulletSpeed = 8;

    /// <summary>
    /// 子弹所属方（玩家 or 敌人）
    /// </summary>
    public BulletOwner Owner { get; }

    public Bullet(int x, int y, Direction direction, BulletOwner owner)
        : base(x, y, BulletSize, BulletSize, BulletSpeed)
    {
        Direction = direction;
        Owner = owner;
    }

    /// <summary>
    /// 每帧根据朝向移动子弹
    /// </summary>
    public override void Update()
    {
        switch (Direction)
        {
            case Direction.Up:    Y -= Speed; break;
            case Direction.Down:  Y += Speed; break;
            case Direction.Left:  X -= Speed; break;
            case Direction.Right: X += Speed; break;
        }
    }

    /// <summary>
    /// 绘制子弹 - 玩家子弹黄色，敌方子弹白色
    /// </summary>
    /// <param name="g">Graphics 绘图上下文</param>
    public override void Draw(Graphics g)
    {
        Color bulletColor = Owner == BulletOwner.Player ? Color.Yellow : Color.White;
        using Brush brush = new SolidBrush(bulletColor);
        g.FillEllipse(brush, X, Y, Width, Height);

        // 黑色边框使子弹更清晰
        using Pen pen = new Pen(Color.Black, 1);
        g.DrawEllipse(pen, X, Y, Width - 1, Height - 1);
    }
}
