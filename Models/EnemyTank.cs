namespace TankBattle.Models;

/// <summary>
/// 敌方坦克类型：影响速度、生命值、颜色和分值
/// </summary>
public enum EnemyType
{
    Basic,    // 普通坦克：1命，100分
    Fast,     // 快速坦克：1命，200分，移动快
    Armored   // 装甲坦克：3命，300分，移动慢
}

/// <summary>
/// 敌方坦克 - 继承 Tank，由 EnemyAI 驱动行为
/// </summary>
public class EnemyTank : Tank
{
    public EnemyType EnemyType { get; }

    /// <summary>
    /// 击毁此坦克获得的分数
    /// </summary>
    public int ScoreValue { get; }

    // 用于 AI 的内部帧计时器
    public int AiTimer { get; set; }

    // 是否正在执行 AI 移动（由 EnemyAI 控制）
    public bool IsMoving { get; set; }

    // 当前 AI 移动方向（由 EnemyAI 分配）
    public Direction AiDirection { get; set; }

    public EnemyTank(int gridX, int gridY, EnemyType type, List<Bullet> bulletList)
        : base(gridX, gridY,
            lives:       GetLives(type),
            speed:       GetSpeed(type),
            maxCooldown: GetCooldown(type),
            bulletList)
    {
        EnemyType = type;
        ScoreValue = GetScore(type);
        Direction = Direction.Down;
        AiTimer = 0;
        IsMoving = false;
        AiDirection = Direction.Down;
        InvincibleFrames = 60;
    }

    private static int GetLives(EnemyType type) => type switch
    {
        EnemyType.Basic   => 1,
        EnemyType.Fast    => 1,
        EnemyType.Armored => 3,
        _ => 1
    };

    private static int GetSpeed(EnemyType type) => type switch
    {
        EnemyType.Basic   => 2,
        EnemyType.Fast    => 4,
        EnemyType.Armored => 1,
        _ => 2
    };

    private static int GetCooldown(EnemyType type) => type switch
    {
        EnemyType.Basic   => 60,
        EnemyType.Fast    => 40,
        EnemyType.Armored => 80,
        _ => 60
    };

    private static int GetScore(EnemyType type) => type switch
    {
        EnemyType.Basic   => 100,
        EnemyType.Fast    => 200,
        EnemyType.Armored => 300,
        _ => 100
    };

    /// <summary>
    /// 按 AI 分配的方向移动一步
    /// </summary>
    public void Move()
    {
        Direction = AiDirection;
        switch (Direction)
        {
            case Direction.Up:    Y -= Speed; break;
            case Direction.Down:  Y += Speed; break;
            case Direction.Left:  X -= Speed; break;
            case Direction.Right: X += Speed; break;
        }
    }

    /// <summary>
    /// 撤销移动（碰到障碍时恢复）
    /// </summary>
    public void UndoMove()
    {
        switch (Direction)
        {
            case Direction.Up:    Y += Speed; break;
            case Direction.Down:  Y -= Speed; break;
            case Direction.Left:  X += Speed; break;
            case Direction.Right: X -= Speed; break;
        }
    }

    public override void Update()
    {
        base.Update();
        AiTimer++;
    }

    /// <summary>
    /// 绘制敌方坦克，根据类型使用不同颜色
    /// </summary>
    public override void Draw(Graphics g)
    {
        switch (EnemyType)
        {
            case EnemyType.Basic:
                DrawTankBody(g,
                    bodyColor:   Color.FromArgb(200, 60, 60),   // 红色
                    trackColor:  Color.FromArgb(100, 20, 20),
                    turretColor: Color.FromArgb(160, 30, 30));
                break;

            case EnemyType.Fast:
                DrawTankBody(g,
                    bodyColor:   Color.FromArgb(220, 160, 0),   // 金黄色
                    trackColor:  Color.FromArgb(120, 80, 0),
                    turretColor: Color.FromArgb(180, 120, 0));
                break;

            case EnemyType.Armored:
                DrawTankBody(g,
                    bodyColor:   Color.FromArgb(100, 100, 180), // 蓝紫色
                    trackColor:  Color.FromArgb(50, 50, 100),
                    turretColor: Color.FromArgb(70, 70, 150));
                // 装甲坦克显示血量指示条
                DrawArmorIndicator(g);
                break;
        }
    }

    private void DrawArmorIndicator(Graphics g)
    {
        int barWidth = 28;
        int barHeight = 4;
        int bx = X + 2;
        int by = Y - 6;

        // 灰色背景
        using Brush bgBrush = new SolidBrush(Color.Gray);
        g.FillRectangle(bgBrush, bx, by, barWidth, barHeight);

        // 绿色血条（分3格）
        float ratio = (float)Lives / MaxLives;
        using Brush hpBrush = new SolidBrush(Lives > 1 ? Color.LimeGreen : Color.Red);
        g.FillRectangle(hpBrush, bx, by, (int)(barWidth * ratio), barHeight);

        // 边框
        using Pen borderPen = new Pen(Color.Black, 1);
        g.DrawRectangle(borderPen, bx, by, barWidth, barHeight);
    }
}
