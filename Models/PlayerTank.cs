namespace TankBattle.Models;

/// <summary>
/// 玩家控制的坦克 - 绿色外观，响应键盘输入
/// </summary>
public class PlayerTank : Tank
{
    // 玩家分数
    public int Score { get; private set; }

    public PlayerTank(int gridX, int gridY, List<Bullet> bulletList)
        : base(gridX, gridY, lives: 3, speed: 3, maxCooldown: 20, bulletList)
    {
        Direction = Direction.Up;
        Score = 0;
    }

    /// <summary>
    /// 增加分数（击毁敌方坦克时调用）
    /// </summary>
    public void AddScore(int points) => Score += points;

    /// <summary>
    /// 重置玩家位置（过关后重新出场）
    /// </summary>
    public void Respawn(int gridX, int gridY)
    {
        X = gridX * Wall.CellSize;
        Y = gridY * Wall.CellSize;
        Direction = Direction.Up;
        InvincibleFrames = 120;
        if (Lives <= 0) Lives = 1; // 最少保留1命继续显示
    }

    /// <summary>
    /// 按当前方向尝试移动（碰撞由外部检测后调用）
    /// </summary>
    public void Move()
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
    /// 撤销上一次移动（碰撞检测后恢复位置用）
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
    }

    /// <summary>
    /// 绘制玩家坦克（绿色系）
    /// </summary>
    public override void Draw(Graphics g)
    {
        DrawTankBody(g,
            bodyColor:   Color.FromArgb(60, 160, 60),   // 橄榄绿主体
            trackColor:  Color.FromArgb(30, 80, 30),    // 深绿履带
            turretColor: Color.FromArgb(40, 120, 40));  // 中绿炮塔
    }
}
