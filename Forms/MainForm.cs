using TankBattle.Persistence;

namespace TankBattle.Forms;

/// <summary>
/// 主菜单窗体 - 游戏入口，包含开始/高分/退出选项及标题动画
/// </summary>
public class MainForm : Form
{
    private readonly SaveManager _saveManager;

    // 标题动画字母偏移
    private int _titleAnimOffset;
    private bool _titleAnimGoingUp = true;
    private readonly System.Windows.Forms.Timer _animTimer;

    // 闪烁提示文字
    private int _blinkCounter;
    private bool _blinkVisible = true;

    public MainForm()
    {
        _saveManager = new SaveManager();
        InitializeComponent();

        // 标题动画计时器（~30fps）
        _animTimer = new System.Windows.Forms.Timer { Interval = 33 };
        _animTimer.Tick += OnAnimTick;
        _animTimer.Start();
    }

    private void InitializeComponent()
    {
        Text = "Tank Battle";
        Size = new Size(560, 640);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(10, 10, 20);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        // 使用 Paint 事件自定义绘制标题
        this.Paint += OnPaint;

        // 菜单按钮组
        AddMenuButton("▶  Start Game",  new Point(180, 320), StartGame);
        AddMenuButton("🏆  High Score",  new Point(180, 390), ShowHighScore);
        AddMenuButton("✖  Exit",         new Point(180, 460), ExitGame);

        // 底部提示
        Label tipLabel = new Label
        {
            Text = "WASD / Arrow Keys to move  |  SPACE to fire",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(100, 120, 160),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(0, 555),
            Size = new Size(544, 24),
            BackColor = Color.Transparent
        };
        Controls.Add(tipLabel);
    }

    private void AddMenuButton(string text, Point location, EventHandler clickHandler)
    {
        Button btn = new Button
        {
            Text = text,
            Location = location,
            Size = new Size(200, 48),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(30, 40, 70),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderColor = Color.FromArgb(80, 100, 180);
        btn.FlatAppearance.BorderSize = 2;
        btn.MouseEnter += (_, _) =>
        {
            btn.BackColor = Color.FromArgb(50, 70, 130);
            btn.FlatAppearance.BorderColor = Color.Gold;
        };
        btn.MouseLeave += (_, _) =>
        {
            btn.BackColor = Color.FromArgb(30, 40, 70);
            btn.FlatAppearance.BorderColor = Color.FromArgb(80, 100, 180);
        };
        btn.Click += clickHandler;
        Controls.Add(btn);
    }

    private void OnAnimTick(object? sender, EventArgs e)
    {
        // 标题上下浮动动画
        if (_titleAnimGoingUp)
        {
            _titleAnimOffset--;
            if (_titleAnimOffset <= -8) _titleAnimGoingUp = false;
        }
        else
        {
            _titleAnimOffset++;
            if (_titleAnimOffset >= 8) _titleAnimGoingUp = true;
        }

        _blinkCounter++;
        if (_blinkCounter >= 20) { _blinkVisible = !_blinkVisible; _blinkCounter = 0; }

        Invalidate(new Rectangle(0, 60, 560, 260)); // 只刷新标题区域
    }

    private void OnPaint(object? sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        DrawTitle(g);
        DrawTankDecoration(g);
    }

    private void DrawTitle(Graphics g)
    {
        // 标题阴影
        using Font titleFont = new Font("Arial Black", 32, FontStyle.Bold);
        string title = "TANK BATTLE";

        SizeF sz = g.MeasureString(title, titleFont);
        float tx = (Width - sz.Width) / 2f;
        float ty = 80 + _titleAnimOffset;

        // 橙黄色渐变标题
        using System.Drawing.Drawing2D.LinearGradientBrush titleBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
            new PointF(tx, ty), new PointF(tx, ty + sz.Height),
            Color.Yellow, Color.OrangeRed);

        // 黑色阴影
        using Brush shadowBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0));
        g.DrawString(title, titleFont, shadowBrush, tx + 3, ty + 3);
        g.DrawString(title, titleFont, titleBrush, tx, ty);

        // 副标题
        using Font subFont = new Font("Segoe UI", 11, FontStyle.Italic);
        using Brush subBrush = new SolidBrush(Color.FromArgb(180, 200, 255));
        string sub = "Battle City Style — Windows Forms C#";
        SizeF subSz = g.MeasureString(sub, subFont);
        g.DrawString(sub, subFont, subBrush, (Width - subSz.Width) / 2f, 155 + _titleAnimOffset);

        // 闪烁提示
        if (_blinkVisible)
        {
            using Font blinkFont = new Font("Segoe UI", 9, FontStyle.Bold);
            using Brush blinkBrush = new SolidBrush(Color.FromArgb(120, 255, 120));
            string hint = "— SELECT AN OPTION —";
            SizeF hintSz = g.MeasureString(hint, blinkFont);
            g.DrawString(hint, blinkFont, blinkBrush, (Width - hintSz.Width) / 2f, 292);
        }
    }

    private void DrawTankDecoration(Graphics g)
    {
        // 左侧小坦克装饰
        DrawMiniTank(g, 40, 200, Color.FromArgb(60, 160, 60), facing: "right");
        // 右侧敌方坦克
        DrawMiniTank(g, 460, 200, Color.FromArgb(200, 60, 60), facing: "left");
    }

    private void DrawMiniTank(Graphics g, int x, int y, Color color, string facing)
    {
        int w = 40, h = 32;
        using Brush bodyBrush = new SolidBrush(color);
        using Brush trackBrush = new SolidBrush(Color.FromArgb(color.R / 3, color.G / 3, color.B / 3));
        using Pen gunPen = new Pen(color.Darken(0.3f), 3);

        g.FillRectangle(trackBrush, x, y + 4, 6, h - 8);
        g.FillRectangle(trackBrush, x + w - 6, y + 4, 6, h - 8);
        g.FillRectangle(bodyBrush, x + 6, y + 4, w - 12, h - 8);
        g.FillEllipse(new SolidBrush(color.Darken(0.2f)), x + 13, y + 13, 14, 14);

        int cx = x + w / 2, cy = y + h / 2;
        if (facing == "right")
            g.DrawLine(gunPen, cx, cy, x + w, cy);
        else
            g.DrawLine(gunPen, cx, cy, x, cy);
    }

    private void StartGame(object? sender, EventArgs e)
    {
        _animTimer.Stop();
        Hide();
        using GameForm gameForm = new GameForm(_saveManager);
        gameForm.ShowDialog();
        Show();
        _animTimer.Start();
    }

    private void ShowHighScore(object? sender, EventArgs e)
    {
        using HighScoreForm hsForm = new HighScoreForm(_saveManager);
        hsForm.ShowDialog(this);
    }

    private void ExitGame(object? sender, EventArgs e) => Application.Exit();

    protected override void Dispose(bool disposing)
    {
        if (disposing) _animTimer.Dispose();
        base.Dispose(disposing);
    }
}

// 扩展：Color 加深辅助方法
internal static class ColorExtensions
{
    internal static Color Darken(this Color c, float factor)
    {
        return Color.FromArgb(
            c.A,
            (int)(c.R * (1 - factor)),
            (int)(c.G * (1 - factor)),
            (int)(c.B * (1 - factor)));
    }
}
