using TankBattle.Models;

namespace TankBattle.Logic;

/// <summary>
/// 地图数据定义，使用二维整数数组编码地形
/// 格子编码：0=空地, 1=砖墙, 2=钢铁, 3=草丛, 4=水域
/// </summary>
public static class GameMap
{
    public const int MapCols = 16;  // 地图列数（格子）
    public const int MapRows = 16;  // 地图行数（格子）
    public const int PixelWidth  = MapCols * Wall.CellSize;  // 512px
    public const int PixelHeight = MapRows * Wall.CellSize;  // 512px

    // ─── 关卡 1：经典对称布局 ──────────────────────────────────────────────
    // 基地位于第15行第8列（中下方）
    private static readonly int[,] Level1Map =
    {
        // 0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15
        {  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, // 0
        {  0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0 }, // 1
        {  0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0 }, // 2
        {  0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0 }, // 3
        {  0, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 0 }, // 4
        {  0, 1, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0 }, // 5
        {  0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0 }, // 6
        {  1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 1, 1 }, // 7
        {  0, 0, 0, 0, 0, 1, 0, 4, 4, 0, 1, 0, 0, 0, 0, 0 }, // 8
        {  0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0 }, // 9
        {  0, 0, 0, 0, 2, 0, 0, 3, 3, 0, 0, 2, 0, 0, 0, 0 }, // 10
        {  0, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 1, 0 }, // 11
        {  0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0 }, // 12
        {  0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0 }, // 13
        {  0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0 }, // 14
        {  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, // 15（基地行）
    };

    // ─── 关卡 2：更多钢铁墙，水域地形 ─────────────────────────────────────
    private static readonly int[,] Level2Map =
    {
        // 0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15
        {  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, // 0
        {  2, 0, 1, 1, 0, 0, 2, 2, 2, 2, 0, 0, 1, 1, 0, 2 }, // 1
        {  0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0 }, // 2
        {  0, 1, 0, 0, 0, 2, 0, 1, 1, 0, 2, 0, 0, 0, 1, 0 }, // 3
        {  0, 1, 0, 4, 4, 0, 0, 0, 0, 0, 0, 4, 4, 0, 1, 0 }, // 4
        {  0, 0, 0, 4, 0, 0, 1, 0, 0, 1, 0, 0, 4, 0, 0, 0 }, // 5
        {  2, 0, 0, 0, 0, 0, 2, 0, 0, 2, 0, 0, 0, 0, 0, 2 }, // 6
        {  1, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1 }, // 7
        {  0, 0, 0, 0, 0, 0, 0, 4, 4, 0, 0, 0, 0, 0, 0, 0 }, // 8
        {  0, 2, 1, 0, 0, 3, 3, 0, 0, 3, 3, 0, 0, 1, 2, 0 }, // 9
        {  0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0 }, // 10
        {  1, 0, 0, 2, 0, 0, 1, 1, 1, 1, 0, 0, 2, 0, 0, 1 }, // 11
        {  0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0 }, // 12
        {  0, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 1, 1, 0 }, // 13
        {  0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0 }, // 14
        {  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, // 15
    };

    // ─── 关卡 3：复杂迷宫，大量钢铁+水域 ─────────────────────────────────
    private static readonly int[,] Level3Map =
    {
        // 0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15
        {  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, // 0
        {  2, 2, 0, 2, 2, 0, 1, 0, 0, 1, 0, 2, 2, 0, 2, 2 }, // 1
        {  2, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 2 }, // 2
        {  0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0 }, // 3
        {  0, 2, 0, 0, 0, 4, 4, 0, 0, 4, 4, 0, 0, 0, 2, 0 }, // 4
        {  0, 2, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 2, 0 }, // 5
        {  0, 0, 0, 1, 0, 2, 0, 1, 1, 0, 2, 0, 1, 0, 0, 0 }, // 6
        {  1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }, // 7
        {  4, 4, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 4, 4 }, // 8
        {  0, 0, 0, 2, 0, 0, 3, 3, 3, 3, 0, 0, 2, 0, 0, 0 }, // 9
        {  0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0 }, // 10
        {  0, 1, 2, 0, 4, 4, 0, 0, 0, 0, 4, 4, 0, 2, 1, 0 }, // 11
        {  0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0 }, // 12
        {  2, 0, 1, 1, 0, 0, 2, 0, 0, 2, 0, 0, 1, 1, 0, 2 }, // 13
        {  0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0 }, // 14
        {  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, // 15
    };

    /// <summary>
    /// 根据关卡索引（1-3）加载地图，返回墙壁列表和基地对象
    /// </summary>
    public static (List<Wall> Walls, Eagle Eagle) LoadLevel(int level)
    {
        int[,] map = level switch
        {
            1 => Level1Map,
            2 => Level2Map,
            3 => Level3Map,
            _ => Level1Map
        };

        List<Wall> walls = new List<Wall>();

        // 基地固定在第8列第14行（画面中下偏右）
        Eagle eagle = new Eagle(8, 14);

        for (int row = 0; row < MapRows; row++)
        {
            for (int col = 0; col < MapCols; col++)
            {
                if (row == 14 && col == 8) continue; // 基地占用，跳过

                int cell = map[row, col];
                switch (cell)
                {
                    case 1: walls.Add(new Wall(col, row, WallType.Brick));  break;
                    case 2: walls.Add(new Wall(col, row, WallType.Steel));  break;
                    case 3: walls.Add(new Wall(col, row, WallType.Bush));   break;
                    case 4: walls.Add(new Wall(col, row, WallType.Water));  break;
                }
            }
        }

        // 在基地左右各放一块砖墙保护
        walls.Add(new Wall(7, 14, WallType.Brick));
        walls.Add(new Wall(9, 14, WallType.Brick));
        walls.Add(new Wall(7, 13, WallType.Brick));
        walls.Add(new Wall(8, 13, WallType.Brick));
        walls.Add(new Wall(9, 13, WallType.Brick));

        return (walls, eagle);
    }

    /// <summary>
    /// 绘制深色背景格子网格
    /// </summary>
    public static void DrawBackground(Graphics g)
    {
        using Brush bgBrush = new SolidBrush(Color.FromArgb(20, 20, 20));
        g.FillRectangle(bgBrush, 0, 0, PixelWidth, PixelHeight);

        // 轻微网格线辅助视觉
        using Pen gridPen = new Pen(Color.FromArgb(30, 30, 30), 1);
        for (int x = 0; x <= PixelWidth; x += Wall.CellSize)
            g.DrawLine(gridPen, x, 0, x, PixelHeight);
        for (int y = 0; y <= PixelHeight; y += Wall.CellSize)
            g.DrawLine(gridPen, 0, y, PixelWidth, y);
    }
}
