namespace TankBattle.Models;

/// <summary>
/// 墙壁类型枚举
/// </summary>
public enum WallType
{
    Brick,  // 砖墙：可被摧毁，需2发子弹
    Steel,  // 钢铁墙：不可摧毁
    Bush,   // 草丛：视觉遮挡，子弹和坦克可穿过
    Water   // 水域：子弹可穿过，坦克不能通过
}

/// <summary>
/// 墙壁/障碍物类 - 定义地图中的固定障碍
/// </summary>
public class Wall : GameObject
{
    // 地图格子大小 32x32 像素
    public const int CellSize = 32;

    public WallType WallType { get; }

    // 砖墙耐久度（2发子弹摧毁）
    private int _durability;

    public Wall(int gridX, int gridY, WallType wallType)
        : base(gridX * CellSize, gridY * CellSize, CellSize, CellSize, 0)
    {
        WallType = wallType;
        _durability = wallType == WallType.Brick ? 2 : int.MaxValue;
    }

    /// <summary>
    /// 接受子弹伤害，砖墙两发摧毁，钢铁墙无敌
    /// </summary>
    /// <returns>true 表示已被摧毁</returns>
    public bool TakeDamage()
    {
        if (WallType == WallType.Steel || WallType == WallType.Bush || WallType == WallType.Water)
            return false;

        _durability--;
        if (_durability <= 0)
        {
            IsActive = false;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 是否阻挡子弹运动
    /// </summary>
    public bool BlocksBullets => WallType == WallType.Brick || WallType == WallType.Steel;

    /// <summary>
    /// 是否阻挡坦克移动
    /// </summary>
    public bool BlocksTank => WallType == WallType.Brick || WallType == WallType.Steel || WallType == WallType.Water;

    public override void Update() { /* 墙壁静止不动 */ }

    /// <summary>
    /// 根据墙壁类型用不同颜色绘制
    /// </summary>
    public override void Draw(Graphics g)
    {
        switch (WallType)
        {
            case WallType.Brick:
                DrawBrickWall(g);
                break;
            case WallType.Steel:
                DrawSteelWall(g);
                break;
            case WallType.Bush:
                DrawBush(g);
                break;
            case WallType.Water:
                DrawWater(g);
                break;
        }
    }

    private void DrawBrickWall(Graphics g)
    {
        // 砖墙：橙红色底色+砖块纹理
        using Brush baseBrush = new SolidBrush(Color.FromArgb(180, 80, 20));
        g.FillRectangle(baseBrush, X, Y, Width, Height);

        using Pen brickPen = new Pen(Color.FromArgb(120, 40, 0), 1);
        // 横线
        g.DrawLine(brickPen, X, Y + 8,  X + Width, Y + 8);
        g.DrawLine(brickPen, X, Y + 16, X + Width, Y + 16);
        g.DrawLine(brickPen, X, Y + 24, X + Width, Y + 24);
        // 竖线（交错砖块样式）
        g.DrawLine(brickPen, X + 16, Y,      X + 16, Y + 8);
        g.DrawLine(brickPen, X + 8,  Y + 8,  X + 8,  Y + 16);
        g.DrawLine(brickPen, X + 24, Y + 8,  X + 24, Y + 16);
        g.DrawLine(brickPen, X + 16, Y + 16, X + 16, Y + 24);
        g.DrawLine(brickPen, X + 8,  Y + 24, X + 8,  Y + 32);
        g.DrawLine(brickPen, X + 24, Y + 24, X + 24, Y + 32);

        // 耐久度低时显示裂缝效果（第一发打到时）
        if (_durability == 1)
        {
            using Pen crackPen = new Pen(Color.Black, 1);
            g.DrawLine(crackPen, X + 4,  Y + 4,  X + 12, Y + 14);
            g.DrawLine(crackPen, X + 20, Y + 18, X + 28, Y + 28);
        }
    }

    private void DrawSteelWall(Graphics g)
    {
        // 钢铁墙：银灰色+金属高光
        using Brush baseBrush = new SolidBrush(Color.FromArgb(160, 160, 160));
        g.FillRectangle(baseBrush, X, Y, Width, Height);

        using Brush highlightBrush = new SolidBrush(Color.FromArgb(220, 220, 220));
        g.FillRectangle(highlightBrush, X + 2, Y + 2, Width / 2 - 3, Height / 2 - 3);
        g.FillRectangle(highlightBrush, X + Width / 2 + 2, Y + Height / 2 + 2, Width / 2 - 4, Height / 2 - 4);

        using Pen borderPen = new Pen(Color.FromArgb(80, 80, 80), 1);
        g.DrawRectangle(borderPen, X, Y, Width - 1, Height - 1);
    }

    private void DrawBush(Graphics g)
    {
        // 草丛：深绿色纹理
        using Brush bushBrush = new SolidBrush(Color.FromArgb(0, 140, 0));
        g.FillRectangle(bushBrush, X, Y, Width, Height);

        using Brush leafBrush = new SolidBrush(Color.FromArgb(0, 180, 0));
        for (int i = 0; i < 5; i++)
        {
            int lx = X + (i * 7) % 24;
            int ly = Y + (i * 5) % 20;
            g.FillEllipse(leafBrush, lx, ly, 10, 8);
        }
    }

    private void DrawWater(Graphics g)
    {
        // 水域：蓝色+波纹
        using Brush waterBrush = new SolidBrush(Color.FromArgb(30, 100, 200));
        g.FillRectangle(waterBrush, X, Y, Width, Height);

        using Pen wavePen = new Pen(Color.FromArgb(100, 180, 255), 1);
        g.DrawLine(wavePen, X + 2,  Y + 10, X + 14, Y + 10);
        g.DrawLine(wavePen, X + 18, Y + 10, X + 30, Y + 10);
        g.DrawLine(wavePen, X + 2,  Y + 20, X + 14, Y + 20);
        g.DrawLine(wavePen, X + 18, Y + 20, X + 30, Y + 20);
    }
}
