namespace TankBattle.Models;

/// <summary>
/// 坦克基类 - 包含玩家和敌方坦克的公共属性与行为
/// </summary>
public abstract class Tank : GameObject
{
    // 坦克格子对齐尺寸（32x32 像素）
    public const int TankSize = 32;

    // 最大生命值（玩家3命，敌人1命或更多）
    public int MaxLives { get; protected set; }

    // 当前生命值
    public int Lives { get; protected set; }

    // 射击冷却计数器（帧数），防止连射
    protected int ShootCooldown;

    // 每次射击的冷却帧数
    protected int MaxCooldown;

    // 当前存活子弹列表（由 GameForm 统一管理，此处为引用）
    protected List<Bullet> BulletList;

    // 无敌帧计数（出生后短暂无敌）
    protected int InvincibleFrames;

    protected Tank(int gridX, int gridY, int lives, int speed, int maxCooldown, List<Bullet> bulletList)
        : base(gridX * Wall.CellSize, gridY * Wall.CellSize, TankSize, TankSize, speed)
    {
        MaxLives = lives;
        Lives = lives;
        MaxCooldown = maxCooldown;
        ShootCooldown = 0;
        BulletList = bulletList;
        InvincibleFrames = 60; // 出生后60帧无敌
    }

    /// <summary>
    /// 尝试射击：冷却结束后生成子弹
    /// </summary>
    /// <returns>true 表示本次成功射击</returns>
    public virtual bool TryShoot()
    {
        if (ShootCooldown > 0)
            return false;

        // 计算子弹初始位置（坦克中心偏移）
        int bx = X + TankSize / 2 - 4;
        int by = Y + TankSize / 2 - 4;

        // 修正到炮口
        switch (Direction)
        {
            case Direction.Up:    by = Y - 4;          break;
            case Direction.Down:  by = Y + TankSize - 4; break;
            case Direction.Left:  bx = X - 4;          break;
            case Direction.Right: bx = X + TankSize - 4; break;
        }

        BulletOwner owner = this is PlayerTank ? BulletOwner.Player : BulletOwner.Enemy;
        BulletList.Add(new Bullet(bx, by, Direction, owner));
        ShootCooldown = MaxCooldown;
        return true;
    }

    /// <summary>
    /// 接受伤害，减少生命值
    /// </summary>
    /// <returns>true 表示坦克已被摧毁</returns>
    public virtual bool TakeDamage()
    {
        if (InvincibleFrames > 0)
            return false;

        Lives--;
        if (Lives <= 0)
        {
            IsActive = false;
            return true;
        }

        InvincibleFrames = 120; // 受伤后短暂无敌
        return false;
    }

    public override void Update()
    {
        if (ShootCooldown > 0)
            ShootCooldown--;

        if (InvincibleFrames > 0)
            InvincibleFrames--;
    }

    /// <summary>
    /// 绘制无敌护盾闪烁效果
    /// </summary>
    protected void DrawShield(Graphics g)
    {
        if (InvincibleFrames > 0 && (InvincibleFrames / 5) % 2 == 0)
        {
            using Pen shieldPen = new Pen(Color.Cyan, 2);
            g.DrawEllipse(shieldPen, X - 3, Y - 3, TankSize + 6, TankSize + 6);
        }
    }

    /// <summary>
    /// 绘制坦克核心图形（子类调用，传入坦克主色）
    /// </summary>
    protected void DrawTankBody(Graphics g, Color bodyColor, Color trackColor, Color turretColor)
    {
        // 履带（两侧矩形）
        using Brush trackBrush = new SolidBrush(trackColor);
        g.FillRectangle(trackBrush, X, Y + 4, 6, TankSize - 8);         // 左履带
        g.FillRectangle(trackBrush, X + TankSize - 6, Y + 4, 6, TankSize - 8); // 右履带

        // 履带分格纹
        using Pen trackPen = new Pen(Color.FromArgb(40, 40, 40), 1);
        for (int i = 0; i < 4; i++)
        {
            int ty = Y + 6 + i * 5;
            g.DrawLine(trackPen, X, ty, X + 6, ty);
            g.DrawLine(trackPen, X + TankSize - 6, ty, X + TankSize, ty);
        }

        // 坦克主体
        using Brush bodyBrush = new SolidBrush(bodyColor);
        g.FillRectangle(bodyBrush, X + 6, Y + 4, TankSize - 12, TankSize - 8);

        // 炮塔（中心小圆）
        using Brush turretBrush = new SolidBrush(turretColor);
        g.FillEllipse(turretBrush, X + 10, Y + 10, 12, 12);

        // 炮管（根据朝向绘制）
        using Pen gunPen = new Pen(turretColor, 4);
        DrawGun(g, gunPen);

        DrawShield(g);
    }

    /// <summary>
    /// 根据当前朝向绘制炮管
    /// </summary>
    private void DrawGun(Graphics g, Pen gunPen)
    {
        int cx = X + TankSize / 2;
        int cy = Y + TankSize / 2;

        switch (Direction)
        {
            case Direction.Up:    g.DrawLine(gunPen, cx, cy, cx, Y);              break;
            case Direction.Down:  g.DrawLine(gunPen, cx, cy, cx, Y + TankSize);   break;
            case Direction.Left:  g.DrawLine(gunPen, cx, cy, X, cy);              break;
            case Direction.Right: g.DrawLine(gunPen, cx, cy, X + TankSize, cy);   break;
        }
    }
}
