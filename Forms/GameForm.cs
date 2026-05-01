using TankBattle.Logic;
using TankBattle.Models;
using TankBattle.Persistence;

namespace TankBattle.Forms;

/// <summary>
/// 游戏状态枚举
/// </summary>
public enum GameState
{
    Playing,     // 游戏进行中
    Paused,      // 已暂停
    LevelCleared,// 关卡通过（显示过关动画）
    GameOver,    // 游戏结束（失败）
    Victory      // 游戏胜利（通过所有关卡）
}

/// <summary>
/// 游戏核心窗体 - 包含双缓冲渲染、游戏主循环（Timer）和所有游戏逻辑协调
/// </summary>
public class GameForm : Form
{
    // ── 依赖 ─────────────────────────────────────────────────────────────────
    private readonly SaveManager    _saveManager;
    private readonly LevelManager   _levelManager;
    private readonly ScoreManager   _scoreManager;
    private readonly EnemyAI        _enemyAI;

    // ── 游戏对象 ──────────────────────────────────────────────────────────────
    private PlayerTank   _player = null!;
    private List<EnemyTank> _enemies = null!;
    private List<Bullet> _bullets = null!;
    private List<Wall>   _walls   = null!;
    private Eagle        _eagle   = null!;

    // ── 输入状态 ──────────────────────────────────────────────────────────────
    // 使用 HashSet 记录所有当前按下的键（支持多键同时按下）
    private readonly HashSet<Keys> _pressedKeys = new HashSet<Keys>();

    // Space 键射击标志（由 ProcessCmdKey 直接设置，绕过 KeyDown 被拦截的问题）
    private bool _shootPressed;

    // ── 游戏状态 ──────────────────────────────────────────────────────────────
    private GameState _state;
    private int _stateTimer; // 状态计时器（帧数），用于动画延迟

    // ── 渲染 ──────────────────────────────────────────────────────────────────
    private readonly System.Windows.Forms.Timer _gameTimer;

    // 游戏区域（地图）大小
    private const int MapWidth   = GameMap.PixelWidth;   // 512
    private const int MapHeight  = GameMap.PixelHeight;  // 512
    private const int HudHeight  = 80;                   // HUD 区高度
    private const int SidePanel  = 0;                    // 预留右侧面板

    // ── 爆炸特效列表 ────────────────────────────────────────────────────────
    private readonly List<(int X, int Y, int Frame)> _explosions = new();

    public GameForm(SaveManager saveManager)
    {
        _saveManager    = saveManager;
        _levelManager   = new LevelManager();
        _scoreManager   = new ScoreManager(saveManager);
        _enemyAI        = new EnemyAI();

        InitializeComponent();

        // 游戏计时器：约 60fps (16ms/Tick)
        _gameTimer = new System.Windows.Forms.Timer { Interval = 16 };
        _gameTimer.Tick += OnGameTick;

        StartLevel(1);
    }

    private void InitializeComponent()
    {
        Text = "Tank Battle";
        ClientSize = new Size(MapWidth, MapHeight + HudHeight);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Color.Black;

        // 双缓冲：消除画面闪烁
        DoubleBuffered = true;

        KeyDown += OnKeyDown;
        KeyUp   += OnKeyUp;
        KeyPreview = true; // 窗体优先截获按键事件
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 关卡初始化
    // ─────────────────────────────────────────────────────────────────────────

    private void StartLevel(int level)
    {
        // 初始化子弹列表（必须先初始化，因为 Tank 构造需要引用它）
        _bullets = new List<Bullet>();
        _enemies = new List<EnemyTank>();

        // 生成玩家坦克（固定出生点：第4列第15行）
        _player = new PlayerTank(4, 15, _bullets);

        // 加载地图（墙壁 + 基地）
        var (walls, eagle) = GameMap.LoadLevel(level);
        _walls = walls;
        _eagle = eagle;

        // 初始化关卡管理器
        _levelManager.LoadLevel(level);

        _state      = GameState.Playing;
        _stateTimer = 0;
        _explosions.Clear();

        _gameTimer.Start();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 游戏主循环（每 16ms 调用）
    // ─────────────────────────────────────────────────────────────────────────

    private void OnGameTick(object? sender, EventArgs e)
    {
        switch (_state)
        {
            case GameState.Playing:
                UpdateGame();
                break;

            case GameState.LevelCleared:
            case GameState.GameOver:
            case GameState.Victory:
                _stateTimer++;
                break;
        }

        Invalidate(); // 触发重绘
    }

    private void UpdateGame()
    {
        // 1. 处理玩家输入
        HandlePlayerInput();

        // 2. 更新玩家坦克
        _player.Update();

        // 3. 生成敌人
        _levelManager.Update(_enemies, _bullets);

        // 4. 更新所有敌人（含 AI）
        foreach (EnemyTank enemy in _enemies)
        {
            enemy.Update();
            _enemyAI.Update(enemy, _player, _walls, MapWidth, MapHeight);
        }

        // 5. 更新所有子弹
        foreach (Bullet bullet in _bullets)
        {
            bullet.Update();
        }

        // 6. 碰撞检测
        ProcessCollisions();

        // 7. 清理失活对象
        _bullets.RemoveAll(b => !b.IsActive);
        _enemies.RemoveAll(e => !e.IsActive);
        _walls.RemoveAll(w => !w.IsActive);

        // 8. 更新爆炸特效
        UpdateExplosions();

        // 9. 检查胜负条件
        CheckGameConditions();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 玩家输入处理
    // ─────────────────────────────────────────────────────────────────────────

    private void HandlePlayerInput()
    {
        if (!_player.IsActive) return;

        // 移动：WASD 或方向键
        Direction? moveDir = null;
        if (_pressedKeys.Contains(Keys.W) || _pressedKeys.Contains(Keys.Up))    moveDir = Direction.Up;
        if (_pressedKeys.Contains(Keys.S) || _pressedKeys.Contains(Keys.Down))  moveDir = Direction.Down;
        if (_pressedKeys.Contains(Keys.A) || _pressedKeys.Contains(Keys.Left))  moveDir = Direction.Left;
        if (_pressedKeys.Contains(Keys.D) || _pressedKeys.Contains(Keys.Right)) moveDir = Direction.Right;

        if (moveDir.HasValue)
        {
            _player.Direction = moveDir.Value;

            // 预测移动后的位置，只有合法时才真正移动（避免卡死问题）
            _player.Move();

            bool blocked = false;

            // 边界检测
            if (CollisionDetector.IsOutOfBounds(_player.Bounds, MapWidth, MapHeight))
                blocked = true;

            // 墙壁碰撞
            if (!blocked && CollisionDetector.CollidesWithWalls(_player.Bounds, _walls))
                blocked = true;

            // 与敌人坦克碰撞（不可穿越）
            if (!blocked)
            {
                foreach (EnemyTank enemy in _enemies)
                {
                    if (enemy.IsActive && CollisionDetector.TanksOverlap(_player.Bounds, enemy.Bounds))
                    {
                        blocked = true;
                        break;
                    }
                }
            }

            if (blocked)
                _player.UndoMove(); // 恢复到移动前的位置
        }

        // 射击：Space 键（每次按下只射一发，松开后才能再射）
        if (_shootPressed)
        {
            _player.TryShoot();
            _shootPressed = false; // 消费掉本次按键，避免持续按住变成自动射击
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 碰撞检测处理
    // ─────────────────────────────────────────────────────────────────────────

    private void ProcessCollisions()
    {
        foreach (Bullet bullet in _bullets)
        {
            if (!bullet.IsActive) continue;

            // 越界销毁
            if (CollisionDetector.IsOutOfBounds(bullet.Bounds, MapWidth, MapHeight))
            {
                bullet.IsActive = false;
                continue;
            }

            // 子弹 - 墙壁
            Wall? hitWall = CollisionDetector.BulletHitsWall(bullet, _walls);
            if (hitWall != null)
            {
                bullet.IsActive = false;
                hitWall.TakeDamage();
                SpawnExplosion(bullet.X, bullet.Y, small: true);
                continue;
            }

            // 子弹 - 基地
            if (CollisionDetector.BulletHitsEagle(bullet, _eagle))
            {
                bullet.IsActive = false;
                _eagle.Destroy();
                SpawnExplosion(_eagle.X + 8, _eagle.Y + 8, small: false);
                _state = GameState.GameOver;
                return;
            }

            // 子弹 - 玩家（敌方子弹）
            if (CollisionDetector.BulletHitsPlayer(bullet, _player))
            {
                bullet.IsActive = false;
                bool destroyed = _player.TakeDamage();
                SpawnExplosion(_player.X + 8, _player.Y + 8, small: false);

                if (destroyed && _player.Lives <= 0)
                {
                    _state = GameState.GameOver;
                    return;
                }
                continue;
            }

            // 子弹 - 敌人（玩家子弹）
            EnemyTank? hitEnemy = CollisionDetector.BulletHitsEnemy(bullet, _enemies);
            if (hitEnemy != null)
            {
                bullet.IsActive = false;
                bool destroyed = hitEnemy.TakeDamage();
                SpawnExplosion(hitEnemy.X + 8, hitEnemy.Y + 8, small: !destroyed);

                if (destroyed)
                    _player.AddScore(hitEnemy.ScoreValue);

                continue;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 胜负条件检测
    // ─────────────────────────────────────────────────────────────────────────

    private void CheckGameConditions()
    {
        if (_state != GameState.Playing) return;

        if (_levelManager.IsLevelComplete(_enemies))
        {
            _scoreManager.Add(0); // flush
            _scoreManager.AddLevelBonus(_levelManager.CurrentLevel);

            _state      = GameState.LevelCleared;
            _stateTimer = 0;

            // 3秒后进入下一关或胜利
            Task.Delay(3000).ContinueWith(_ =>
            {
                if (!IsDisposed)
                    Invoke(() =>
                    {
                        bool hasNext = _levelManager.NextLevel();
                        if (hasNext)
                        {
                            StartLevel(_levelManager.CurrentLevel);
                        }
                        else
                        {
                            _scoreManager.SaveHighScore(_levelManager.CurrentLevel);
                            _state = GameState.Victory;
                        }
                    });
            });
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 爆炸特效
    // ─────────────────────────────────────────────────────────────────────────

    private void SpawnExplosion(int x, int y, bool small)
    {
        // Frame 存储: 高8位=small标志，低8位=当前帧号
        _explosions.Add((x, y, small ? 0x100 : 0x000));
    }

    private void UpdateExplosions()
    {
        for (int i = _explosions.Count - 1; i >= 0; i--)
        {
            var (x, y, frame) = _explosions[i];
            int frameIdx = frame & 0xFF;
            int flags    = frame & ~0xFF;
            frameIdx++;
            if (frameIdx > 12)
                _explosions.RemoveAt(i);
            else
                _explosions[i] = (x, y, flags | frameIdx);
        }
    }

    private void DrawExplosions(Graphics g)
    {
        foreach (var (x, y, frame) in _explosions)
        {
            int frameIdx = frame & 0xFF;
            bool small = (frame & 0x100) != 0;

            float progress = frameIdx / 12f;
            int maxRadius  = small ? 16 : 32;
            int radius     = (int)(maxRadius * Math.Sin(progress * Math.PI));
            int alpha      = (int)(255 * (1 - progress));

            using Brush fireBrush = new SolidBrush(Color.FromArgb(alpha, 255, (int)(150 * (1 - progress)), 0));
            using Brush coreBrush = new SolidBrush(Color.FromArgb(Math.Min(255, alpha + 50), 255, 255, 100));
            g.FillEllipse(fireBrush, x - radius, y - radius, radius * 2, radius * 2);
            g.FillEllipse(coreBrush, x - radius / 2, y - radius / 2, radius, radius);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 绘制 (OnPaint) - 双缓冲，所有绘制在此完成
    // ─────────────────────────────────────────────────────────────────────────

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // ── 地图区域 ─────────────────────────────────────────────────────────
        g.SetClip(new Rectangle(0, HudHeight, MapWidth, MapHeight));
        g.TranslateTransform(0, HudHeight);

        GameMap.DrawBackground(g);

        // 绘制墙壁（草丛最后绘制，覆盖在坦克上方以实现遮挡效果）
        List<Wall> bushes = new List<Wall>();
        foreach (Wall wall in _walls)
        {
            if (wall.IsActive)
            {
                if (wall.WallType == WallType.Bush)
                    bushes.Add(wall);
                else
                    wall.Draw(g);
            }
        }

        // 绘制基地
        _eagle.Draw(g);

        // 绘制玩家坦克
        if (_player.IsActive)
            _player.Draw(g);

        // 绘制敌方坦克
        foreach (EnemyTank enemy in _enemies)
            if (enemy.IsActive) enemy.Draw(g);

        // 绘制子弹
        foreach (Bullet bullet in _bullets)
            if (bullet.IsActive) bullet.Draw(g);

        // 绘制爆炸特效
        DrawExplosions(g);

        // 草丛覆盖坦克（遮挡层）
        foreach (Wall bush in bushes)
            bush.Draw(g);

        g.ResetTransform();
        g.ResetClip();

        // ── HUD 区域 ─────────────────────────────────────────────────────────
        DrawHud(g);

        // ── 覆盖层（暂停/结束/过关）─────────────────────────────────────────
        DrawOverlay(g);
    }

    private void DrawHud(Graphics g)
    {
        // HUD 背景
        using Brush hudBg = new SolidBrush(Color.FromArgb(20, 20, 30));
        g.FillRectangle(hudBg, 0, 0, MapWidth, HudHeight);

        using Pen borderPen = new Pen(Color.FromArgb(60, 80, 140), 2);
        g.DrawLine(borderPen, 0, HudHeight - 2, MapWidth, HudHeight - 2);

        using Font hudFont  = new Font("Segoe UI", 10, FontStyle.Bold);
        using Font valFont  = new Font("Consolas", 14, FontStyle.Bold);
        using Brush labelBr = new SolidBrush(Color.FromArgb(150, 170, 210));
        using Brush valueBr = new SolidBrush(Color.White);
        using Brush scoreBr = new SolidBrush(Color.Gold);
        using Brush lifeBr  = new SolidBrush(Color.FromArgb(80, 230, 80));

        // 分数
        g.DrawString("SCORE", hudFont, labelBr, 20, 8);
        g.DrawString(_scoreManager.CurrentScore.ToString("D6"), valFont, scoreBr, 20, 28);

        // 生命值（心型符号）
        g.DrawString("LIVES", hudFont, labelBr, 160, 8);
        string livesIcons = string.Concat(Enumerable.Repeat("♥ ", Math.Max(0, _player.Lives)));
        g.DrawString(livesIcons, valFont, lifeBr, 160, 28);

        // 关卡
        g.DrawString("LEVEL", hudFont, labelBr, 310, 8);
        g.DrawString($"{_levelManager.CurrentLevel} / {LevelManager.TotalLevels}", valFont, valueBr, 310, 28);

        // 剩余敌人数
        g.DrawString("ENEMIES", hudFont, labelBr, 410, 8);
        g.DrawString(_levelManager.RemainingEnemies.ToString(), valFont, Color.OrangeRed.Name == "OrangeRed"
            ? new SolidBrush(Color.OrangeRed) : valueBr, 420, 28);

        // 提示
        using Font tipFont = new Font("Segoe UI", 7.5f);
        using Brush tipBr  = new SolidBrush(Color.FromArgb(80, 100, 130));
        g.DrawString("P = Pause | ESC = Exit", tipFont, tipBr, 200, 62);
    }

    private void DrawOverlay(Graphics g)
    {
        switch (_state)
        {
            case GameState.Paused:
                DrawCenteredOverlay(g, "⏸  PAUSED", "Press P to continue", Color.FromArgb(120, 100, 220));
                break;

            case GameState.LevelCleared:
                DrawCenteredOverlay(g,
                    $"✔  LEVEL {_levelManager.CurrentLevel - 1} CLEAR!",
                    $"+{(_levelManager.CurrentLevel - 1) * 500} BONUS  →  Lv.{_levelManager.CurrentLevel}",
                    Color.FromArgb(80, 220, 80));
                break;

            case GameState.GameOver:
                _scoreManager.SaveHighScore(_levelManager.CurrentLevel);
                DrawCenteredOverlay(g, "✖  GAME OVER",
                    $"Final Score: {_scoreManager.CurrentScore}   Press R to restart",
                    Color.FromArgb(230, 60, 60));
                break;

            case GameState.Victory:
                DrawCenteredOverlay(g, "🏆  YOU WIN!",
                    $"Final Score: {_scoreManager.CurrentScore}   Press R to play again",
                    Color.Gold);
                break;
        }
    }

    private void DrawCenteredOverlay(Graphics g, string title, string subtitle, Color accent)
    {
        // 半透明黑色遮罩
        int oy = HudHeight;
        using Brush dimBrush = new SolidBrush(Color.FromArgb(180, 0, 0, 0));
        g.FillRectangle(dimBrush, 0, oy, MapWidth, MapHeight);

        // 中心面板
        int px = 60, py = oy + 160, pw = MapWidth - 120, ph = 140;
        using Brush panelBrush = new SolidBrush(Color.FromArgb(230, 15, 15, 30));
        g.FillRectangle(panelBrush, px, py, pw, ph);
        using Pen panelBorder = new Pen(accent, 3);
        g.DrawRectangle(panelBorder, px, py, pw, ph);

        // 标题
        using Font titleFont = new Font("Arial Black", 22, FontStyle.Bold);
        using Brush titleBrush = new SolidBrush(accent);
        SizeF tsz = g.MeasureString(title, titleFont);
        g.DrawString(title, titleFont, titleBrush, (MapWidth - tsz.Width) / 2, py + 20);

        // 副标题
        using Font subFont = new Font("Segoe UI", 10, FontStyle.Bold);
        using Brush subBrush = new SolidBrush(Color.FromArgb(200, 200, 200));
        SizeF ssz = g.MeasureString(subtitle, subFont);
        g.DrawString(subtitle, subFont, subBrush, (MapWidth - ssz.Width) / 2, py + 88);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 键盘事件
    // ─────────────────────────────────────────────────────────────────────────

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        _pressedKeys.Add(e.KeyCode);

        // 暂停
        if (e.KeyCode == Keys.P)
        {
            if (_state == GameState.Playing)
            {
                _state = GameState.Paused;
                _gameTimer.Stop();
            }
            else if (_state == GameState.Paused)
            {
                _state = GameState.Playing;
                _gameTimer.Start();
            }
        }

        // 重新开始
        if (e.KeyCode == Keys.R && (_state == GameState.GameOver || _state == GameState.Victory))
        {
            _scoreManager.Reset();
            StartLevel(1);
        }

        // 退出游戏
        if (e.KeyCode == Keys.Escape)
        {
            _scoreManager.SaveHighScore(_levelManager.CurrentLevel);
            Close();
        }
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        _pressedKeys.Remove(e.KeyCode);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        const int WM_KEYDOWN = 0x0100;
        const int WM_KEYUP   = 0x0101;

        // 防止方向键触发窗体默认焦点切换行为，并在此处直接处理 Space 射击
        if (keyData == Keys.Up || keyData == Keys.Down ||
            keyData == Keys.Left || keyData == Keys.Right)
        {
            return true; // 已处理，阻止焦点移动
        }

        // Space 键：在此处拦截并设置射击标志，不依赖 KeyDown 事件
        if (keyData == Keys.Space)
        {
            if (msg.Msg == WM_KEYDOWN)
                _shootPressed = true;
            else if (msg.Msg == WM_KEYUP)
                _shootPressed = false;
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _gameTimer.Stop();
            _gameTimer.Dispose();
        }
        base.Dispose(disposing);
    }
}
