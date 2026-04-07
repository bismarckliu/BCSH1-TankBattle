using TankBattle.Persistence;

namespace TankBattle.Forms;

/// <summary>
/// 高分记录查看窗体 - 展示历史最高5条记录
/// </summary>
public class HighScoreForm : Form
{
    private readonly SaveManager _saveManager;

    public HighScoreForm(SaveManager saveManager)
    {
        _saveManager = saveManager;
        InitializeComponent();
        PopulateScores();
    }

    private void InitializeComponent()
    {
        Text = "🏆 Tank Battle - High Scores";
        Size = new Size(400, 380);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(15, 15, 25);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        // 标题标签
        Label titleLabel = new Label
        {
            Text = "🏆 HIGH SCORES",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.Gold,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.None,
            Location = new Point(0, 16),
            Size = new Size(384, 40)
        };

        // 分数列表面板
        Panel scoresPanel = new Panel
        {
            Location = new Point(20, 70),
            Size = new Size(344, 220),
            BackColor = Color.FromArgb(25, 25, 40)
        };

        // 表头
        Label header = new Label
        {
            Text = "  Rank    Score        Date           Level",
            Font = new Font("Consolas", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(150, 200, 255),
            Location = new Point(0, 0),
            Size = new Size(344, 24),
            BackColor = Color.FromArgb(40, 40, 70)
        };
        scoresPanel.Controls.Add(header);

        // 关闭按钮
        Button closeBtn = new Button
        {
            Text = "Close",
            Location = new Point(150, 305),
            Size = new Size(90, 36),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 60, 100),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        closeBtn.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 160);
        closeBtn.Click += (_, _) => Close();

        Controls.Add(titleLabel);
        Controls.Add(scoresPanel);
        Controls.Add(closeBtn);

        Tag = scoresPanel; // 暂存引用，供 PopulateScores 使用
    }

    private void PopulateScores()
    {
        Panel scoresPanel = (Panel)Tag!;
        var data = _saveManager.LoadData();

        if (data.TopScores.Count == 0)
        {
            Label noData = new Label
            {
                Text = "No records yet. Play a game!",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 80),
                Size = new Size(344, 30)
            };
            scoresPanel.Controls.Add(noData);
            return;
        }

        var sorted = data.TopScores.OrderByDescending(s => s.Score).ToList();
        Color[] rankColors = { Color.Gold, Color.Silver, Color.FromArgb(205, 127, 50), Color.White, Color.White };

        for (int i = 0; i < sorted.Count; i++)
        {
            var entry = sorted[i];
            string rankText = (i + 1) switch { 1 => "🥇", 2 => "🥈", 3 => "🥉", _ => $"  {i + 1} " };

            Label row = new Label
            {
                Text = $"  {rankText}   {entry.Score,6}    {entry.Date,-16}  Lv.{entry.LevelReached}",
                Font = new Font("Consolas", 9),
                ForeColor = rankColors[Math.Min(i, rankColors.Length - 1)],
                Location = new Point(0, 28 + i * 36),
                Size = new Size(344, 32),
                BackColor = i % 2 == 0 ? Color.FromArgb(30, 30, 50) : Color.FromArgb(25, 25, 40)
            };
            scoresPanel.Controls.Add(row);
        }
    }
}
