namespace TankBattle.Models;

/// <summary>
/// 基地（老鹰）类 - 玩家需要保护的目标，被摧毁则游戏结束
/// </summary>
public class Eagle : GameObject
{
    private const int EagleSize = 32;

    // 是否已被摧毁
    public bool IsDestroyed { get; private set; }

    public Eagle(int gridX, int gridY)
        : base(gridX * Wall.CellSize, gridY * Wall.CellSize, EagleSize, EagleSize, 0)
    {
        IsDestroyed = false;
    }

    /// <summary>
    /// 基地被击中时触发，标记为已摧毁并通知游戏结束
    /// </summary>
    public void Destroy()
    {
        IsDestroyed = true;
        IsActive = false;
    }

    public override void Update() { /* 基地静止 */ }

    /// <summary>
    /// 绘制基地：未摧毁时绘制老鹰图案，摧毁后绘制残骸
    /// </summary>
    public override void Draw(Graphics g)
    {
        if (IsDestroyed)
        {
            DrawDestroyedEagle(g);
        }
        else
        {
            DrawEagle(g);
        }
    }

    private void DrawEagle(Graphics g)
    {
        // 黑色底座
        using Brush baseBrush = new SolidBrush(Color.Black);
        g.FillRectangle(baseBrush, X, Y, Width, Height);

        // 外边框（金黄色）
        using Pen borderPen = new Pen(Color.Gold, 2);
        g.DrawRectangle(borderPen, X + 1, Y + 1, Width - 3, Height - 3);

        // 老鹰身体（棕色）
        using Brush bodyBrush = new SolidBrush(Color.SaddleBrown);
        g.FillEllipse(bodyBrush, X + 8, Y + 10, 16, 14);

        // 老鹰翅膀（深棕色，左右对称）
        using Brush wingBrush = new SolidBrush(Color.FromArgb(100, 60, 20));
        g.FillEllipse(wingBrush, X + 2,  Y + 8, 10, 10);
        g.FillEllipse(wingBrush, X + 20, Y + 8, 10, 10);

        // 眼睛（红色）
        using Brush eyeBrush = new SolidBrush(Color.Red);
        g.FillEllipse(eyeBrush, X + 11, Y + 13, 4, 4);
        g.FillEllipse(eyeBrush, X + 17, Y + 13, 4, 4);

        // 鸟喙（橙色）
        using Brush beakBrush = new SolidBrush(Color.Orange);
        g.FillPolygon(beakBrush, new Point[]
        {
            new Point(X + 14, Y + 19),
            new Point(X + 18, Y + 23),
            new Point(X + 14, Y + 24),
            new Point(X + 10, Y + 23)
        });
    }

    private void DrawDestroyedEagle(Graphics g)
    {
        // 被摧毁时：灰黑色残骸
        using Brush ashBrush = new SolidBrush(Color.FromArgb(60, 60, 60));
        g.FillRectangle(ashBrush, X, Y, Width, Height);

        using Pen xPen = new Pen(Color.DarkRed, 3);
        g.DrawLine(xPen, X + 4, Y + 4, X + Width - 4, Y + Height - 4);
        g.DrawLine(xPen, X + Width - 4, Y + 4, X + 4, Y + Height - 4);

        using Pen borderPen = new Pen(Color.DarkGray, 2);
        g.DrawRectangle(borderPen, X + 1, Y + 1, Width - 3, Height - 3);
    }
}
