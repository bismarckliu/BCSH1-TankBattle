# 🎮 Tank Battle — Battle City Style

**Semestrální práce** | BCSH1 | Varianta (b) — Jednoduchá počítačová hra

---

## Popis hry / 游戏说明

| 🇨🇿 Česky | 🇨🇳 中文 |
|-----------|---------|
| 2D tanková hra ve stylu Battle City realizovaná v technologii **Windows Forms + C# + .NET 8**. Hráč ovládá zelený tank a cílem je zničit všechny nepřátelské tanky a zároveň ochránit základnu (orla) před zničením. Hra obsahuje 3 úrovně — čím vyšší úroveň, tím více nepřátel a silnější typy. | 玩家操控一辆绿色坦克，目标是消灭所有敌方坦克，同时保护基地（老鹰）不被摧毁。共3个关卡，关卡越高敌人越多、类型越强。 |

---

## Herní mechaniky / 游戏功能

| Funkce / 功能 | 🇨🇿 Česky | 🇨🇳 中文 |
|--------------|-----------|---------|
| **Ovládání hráče** / **玩家操控** | WASD / šipky pro pohyb, mezerník pro střelbu | WASD / 方向键移动，空格键射击 |
| **3 typy nepřátel** / **3种敌方坦克** | Základní (červený·100 b.) / Rychlý (zlatý·200 b.) / Obrněný (modrý·300 b., 3 životy) | 普通（红·100分）/ 快速（金·200分）/ 装甲（蓝·300分，3血） |
| **3 úrovně** / **3个关卡** | Různé rozvržení mapy (cihly/ocel/keře/voda) | 不同地图布局（砖墙/钢铁墙/草丛/水域） |
| **AI systém** / **AI 系统** | Náhodná hlídka + přímočaré sledování + střelba ve stejném řádku/sloupci | 随机巡逻 + 直线追踪 + 同行/列时瞄准射击 |
| **Výbuchové efekty** / **爆炸特效** | Kruhová animace výbuchu při zničení tanku | 消灭坦克时显示圆形爆炸动画 |
| **Krytí keřů** / **草丛遮挡** | Vrstva keřů překrývá tanky | 草丛层覆盖于坦克之上 |
| **Ochrana základny** / **基地保护** | Zničení orla = prohra | 老鹰被摧毁即游戏失败 |
| **Ukládání skóre** / **高分存档** | Top 5 výsledků uloženo v `%AppData%\TankBattle\save.json` | 历史最高分5条，保存至 `%AppData%\TankBattle\save.json` |
| **Pauza / Restart** / **暂停/重开** | P = pauza, R = restart, ESC = konec | P 键暂停，R 键重开，ESC 退出 |

---

## Ovládání / 操作说明

| Klávesa / 按键 | 🇨🇿 Akce | 🇨🇳 动作 |
|---------------|---------|---------|
| `W` / `↑` | Pohyb nahoru | 上移 |
| `S` / `↓` | Pohyb dolů | 下移 |
| `A` / `←` | Pohyb doleva | 左移 |
| `D` / `→` | Pohyb doprava | 右移 |
| `SPACE` | Střelba | 射击 |
| `P` | Pauza / Pokračovat | 暂停 / 继续 |
| `R` | Restart (po konci hry) | 重新开始（游戏结束后） |
| `ESC` | Ukončit hru | 退出游戏 |

---

## Architektura / 项目架构

```
TankBattle/
├── Program.cs                  # Vstupní bod / 入口：Application.Run(new MainForm())
├── Models/
│   ├── GameObject.cs           # Abstraktní základní třída / 抽象基类（Rectangle Bounds, Direction, Speed）
│   ├── Tank.cs                 # Základní třída tanku / 坦克基类（射击冷却, 无敌帧, 炮管绘制）
│   ├── PlayerTank.cs           # Tank hráče / 玩家坦克（绿色，含分数）
│   ├── EnemyTank.cs            # Nepřátelský tank / 敌方坦克（3种类型 + 血条）
│   ├── Bullet.cs               # Střela / 子弹（方向速度 + 归属）
│   ├── Wall.cs                 # Zeď / 墙壁（砖墙/钢铁/草丛/水域，含耐久度）
│   └── Eagle.cs                # Základna / 基地（被摧毁触发失败）
├── Logic/
│   ├── GameMap.cs              # Mapy 3 úrovní / 3关地图（16×16 二维数组 int[,]）
│   ├── CollisionDetector.cs    # Detekce kolizí / 碰撞检测（Rectangle.IntersectsWith）
│   ├── EnemyAI.cs              # AI nepřítele / 敌方AI（巡逻状态机 + 直线射击）
│   ├── LevelManager.cs         # Správa úrovní / 关卡管理（敌人队列, 分批出场）
│   └── ScoreManager.cs         # Správa skóre / 分数管理（击杀加分, 过关奖励）
├── Persistence/
│   └── SaveManager.cs          # Ukládání souborů / 文件存档（System.Text.Json, JSON 格式）
└── Forms/
    ├── MainForm.cs             # Hlavní menu / 主菜单（标题浮动动画, 小坦克装饰）
    ├── GameForm.cs             # Herní formulář / 游戏主窗体（双缓冲, 60fps Timer, 状态机）
    └── HighScoreForm.cs        # Žebříček skóre / 高分榜窗体（排行前5条）
```

---

## Použité C# techniky / 使用的 C# 技术

| Technika / 技术 | 🇨🇿 Použití | 🇨🇳 用途 |
|----------------|------------|---------|
| `System.Windows.Forms.Timer` | Herní smyčka (~60fps, 16ms/Tick) | 游戏主循环 (~60fps, 16ms/Tick) |
| `DoubleBuffered = true` + `OnPaint` | Vykreslování bez blikání | 无闪烁双缓冲渲染 |
| `KeyDown` / `KeyUp` + `HashSet<Keys>` | Detekce více současně stisknutých kláves | 多键同时按下检测 |
| `Rectangle.IntersectsWith()` | Detekce obdélníkových kolizí | 矩形碰撞检测 |
| Dědičnost (`Tank : GameObject`) | Objektově orientovaný polymorfní design | 面向对象多态设计 |
| `enum` | `Direction`, `WallType`, `EnemyType`, `GameState` | `Direction`, `WallType`, `EnemyType`, `GameState` |
| `List<T>` / `Queue<T>` | Správa nepřátel, střel a výbuchů | 敌人、子弹、爆炸特效管理 |
| `System.Text.Json` | JSON ukládání (vestavěné v .NET, bez externích knihoven) | JSON 存档（.NET 内置，无外部库） |
| `System.Random` | Náhodné rozhodování AI | AI 随机决策 |
| `Task.Delay` | Zpoždění animace přechodu úrovně | 过关动画延迟 |
| `LinearGradientBrush` | Přechodový nadpis hlavního menu | 主菜单渐变标题 |
| `record` typ | Konfigurace úrovně `LevelConfig` | `LevelConfig` 关卡配置 |

---

## Spuštění / 运行方法

| 🇨🇿 Postup | 🇨🇳 步骤 |
|-----------|---------|
| Otevřete terminál a přejděte do složky projektu. | 打开终端，切换到项目目录。 |
| Spusťte příkaz níže: | 执行以下命令： |

```powershell
cd "C:\Users\liu\Desktop\program\C#\Semestralni prace"
dotnet run
```

---

## Dokumentace assetů a zdrojů / 资源与来源文档

> ⚠️ **Povinná součást semestrální práce dle požadavků BCSH1** — nedokumentované assety nebo tutoriály jsou důvodem k hodnocení „fail".  
> 以下为课程 BCSH1 强制要求的资源声明章节，未注明来源的资源或教程将导致不合格。

---

### 🎨 Grafické assety / 图形资源

| Asset / 资源 | Popis / 描述 | Zdroj / 来源 |
|-------------|-------------|-------------|
| Hráčský tank | Zelený tank — `FillRectangle` + `DrawEllipse` (炮管) | ✍️ **Autor** — Jialong Liu |
| Nepřátelský tank — základní | Červený tank kreslený kódem | ✍️ **Autor** — Jialong Liu |
| Nepřátelský tank — rychlý | Zlatý tank kreslený kódem | ✍️ **Autor** — Jialong Liu |
| Nepřátelský tank — obrněný | Modrý tank s ukazatelem zdraví | ✍️ **Autor** — Jialong Liu |
| Střela (Bullet) | Malý žlutý obdélník | ✍️ **Autor** — Jialong Liu |
| Zeď — cihly (Brick) | Červený obdélník s tmavou mřížkou | ✍️ **Autor** — Jialong Liu |
| Zeď — ocel (Steel) | Šedý obdélník s kříži | ✍️ **Autor** — Jialong Liu |
| Keře (Bush) | Zelené tečky na průhledné vrstvě | ✍️ **Autor** — Jialong Liu |
| Voda (Water) | Modrý obdélník s vlnkami | ✍️ **Autor** — Jialong Liu |
| Základna — orel (Eagle) | Hnědý symbol | ✍️ **Autor** — Jialong Liu |
| Animace výbuchu | Expandující kružnice po zničení objektu | ✍️ **Autor** — Jialong Liu |
| Přechodový titulek (menu) | `LinearGradientBrush` — zlato-červený gradient | ✍️ **Autor** — Jialong Liu |

> 📌 Projekt **nepoužívá žádné externí obrázky, sprity ani zvukové soubory**.  
> Veškerá grafika je generována výhradně kódem přes `System.Drawing.Graphics`.  
> 本项目**不包含任何外部图片、精灵图或音效文件**，全部图形均通过 `System.Drawing.Graphics` 以代码绘制。

---

### 🎬 Použité tutoriály a výukové materiály / 参考教程与学习资料

| # | Typ / 类型 | Název / 标题 | Odkaz / 链接 | Použití / 用途 |
|---|-----------|-------------|-------------|--------------|
| 1 | 🎬 Bilibili 视频 | Battle City 坦克大战实现思路 | [bilibili.com/video/BV1ne4y1U7Kx](https://www.bilibili.com/video/BV1ne4y1U7Kx/?spm_id_from=333.337.search-card.all.click&vd_source=a629db31ff833a5bed413d5f4cc95c2b) | 游戏整体架构、敌方AI状态机、地图设计思路 |
| 2 | 📝 CSDN 博客 | C# WinForms 坦克大战实现参考 | [blog.csdn.net/weixin_42388898](https://blog.csdn.net/weixin_42388898/article/details/150621629) | 碰撞检测、双缓冲渲染、游戏循环实现 |
| 3 | 📖 官方文档 | Microsoft Docs — Windows Forms | [learn.microsoft.com/dotnet/desktop/winforms](https://learn.microsoft.com/cs-cz/dotnet/desktop/winforms/) | Timer、DoubleBuffered、KeyDown/KeyUp、Paint 事件 |
| 4 | 📖 官方文档 | Microsoft Docs — System.Drawing | [learn.microsoft.com/dotnet/api/system.drawing](https://learn.microsoft.com/cs-cz/dotnet/api/system.drawing) | `Graphics`、`Brush`、`Pen`、`Rectangle` API |
| 5 | 📖 官方文档 | Microsoft Docs — System.Text.Json | [learn.microsoft.com/dotnet/api/system.text.json](https://learn.microsoft.com/cs-cz/dotnet/api/system.text.json) | JSON 存档读写（`JsonSerializer`） |

---

### 🤖 Použité AI nástroje (Generativní AI) / 生成式 AI 工具声明

| Nástroj / 工具 | Poskytovatel / 提供方 | Verze / 版本 | Použití / 用途 |
|---------------|----------------------|-------------|--------------|
| **Google Gemini** | Google DeepMind | Gemini 2.5 Pro | 代码调试、逻辑优化、文档编写辅助 |
| **Anthropic Claude** | Anthropic | Claude Sonnet 4.5 | 代码生成辅助、架构建议、README 文档编写 |

| 🇨🇿 Prohlášení | 🇨🇳 声明 |
|--------------|---------|
| Nástroje generativní AI byly využity jako asistenti při psaní kódu, ladění chyb a generování dokumentace. Veškerá herní logika, architektura systému a designová rozhodnutí byla navržena, zkontrolována a schválena autorem. Žádný asset nebyl generován AI bez autorovy kontroly. | 生成式 AI 工具仅作为辅助工具，用于代码编写、错误调试与文档生成。所有游戏逻辑、系统架构与设计决策均由作者本人独立设计并审核确认，未有任何资源由 AI 独立生成并在未经作者审查的情况下使用。 |

---

## Autor / 作者

- **Jméno / 姓名**: Jialong Liu  
- **Email**: liujialong630@gmail.com  
- **Repozitář / 仓库**: https://github.com/bismarckliu/BCSH1-TankBattle  
- **Datum / 日期**: 2026-03
