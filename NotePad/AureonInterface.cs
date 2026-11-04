using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EditBox = System.Windows.Forms.TextBox;

namespace NotePad
{
    public partial class Form1
    {
        // ═════════════════════════════════════════════════════════════
        //  ПАЛИТРЫ
        // ═════════════════════════════════════════════════════════════
        private sealed class Preset
        {
            public string Name;
            public bool Light;
            public Color Bg, Panel, Editor, Hover, Border, Text;
        }

        private static Color Hex(int rgb) =>
            Color.FromArgb((rgb >> 16) & 255, (rgb >> 8) & 255, rgb & 255);

        private static Preset MakePreset(string name, bool light,
            int bg, int panel, int editor, int hover, int border, int text) =>
            new Preset
            {
                Name = name,
                Light = light,
                Bg = Hex(bg),
                Panel = Hex(panel),
                Editor = Hex(editor),
                Hover = Hex(hover),
                Border = Hex(border),
                Text = Hex(text)
            };

        private static readonly Preset[] presets =
        {
            MakePreset("Північ",  false, 0x151923, 0x202736, 0x181D29, 0x2B3548, 0x303B4E, 0xE7ECF5),
            MakePreset("Графіт",  false, 0x1C1D21, 0x2B2D33, 0x202227, 0x3A3D45, 0x4B4E57, 0xE4E6EB),
            MakePreset("Океан",   false, 0x121B2B, 0x1D2B41, 0x152236, 0x273953, 0x354B69, 0xE2EAF6),
            MakePreset("Ліс",     false, 0x121F1D, 0x1D302C, 0x152723, 0x28413A, 0x37564C, 0xE0F0EA),
            MakePreset("Nord",    false, 0x242933, 0x2E3440, 0x282E3A, 0x3B4252, 0x434C5E, 0xD8DEE9),
            MakePreset("Папір",   true,  0xDCE0E6, 0xEBEEF2, 0xF8F9FB, 0xD2D8E0, 0xB9C0CA, 0x232B3A),
            MakePreset("Сепія",   true,  0xE2D7C2, 0xEEE4D0, 0xF7F0E0, 0xDCCEB4, 0xC4B496, 0x463424)
        };

        private static readonly Color[] accents =
        {
            Color.FromArgb(76, 141, 255),
            Color.FromArgb(139, 124, 255),
            Color.FromArgb(236, 100, 170),
            Color.FromArgb(255, 107, 107),
            Color.FromArgb(243, 156, 18),
            Color.FromArgb(85, 184, 138),
            Color.FromArgb(38, 198, 218),
            Color.FromArgb(137, 147, 164)
        };

        private static readonly string[] accentNames =
        {
            "Синій", "Фіолетовий", "Рожевий", "Кораловий",
            "Помаранчевий", "Зелений", "Бірюзовий", "Сірий"
        };

        private static readonly Color[] textColors =
        {
            Color.White,
            Color.FromArgb(220, 220, 220),
            Color.FromArgb(160, 160, 160),
            Color.FromArgb(95, 95, 95),
            Color.Black,
            Color.FromArgb(231, 76, 60),
            Color.FromArgb(52, 152, 219),
            Color.FromArgb(46, 204, 113),
            Color.FromArgb(241, 196, 15),
            Color.FromArgb(155, 89, 182),
            Color.FromArgb(243, 156, 18)
        };

        private static readonly string[] textColorNames =
        {
            "Білий", "Світло-сірий", "Сірий", "Темно-сірий", "Чорний", "Червоний",
            "Блакитний", "Зелений", "Жовтий", "Фіолетовий", "Помаранчевий"
        };

        // Текущий выбор (сохраняется в settings.ini)
        private int presetIndex;
        private int accentIndex;
        private int textColorIndex;          // 0 = авто (цвет из темы), 1..N = из списка
        private Color? customTextColor;      // «свой» цвет, перекрывает всё остальное

        // Активные цвета, из которых рисуется весь интерфейс
        private static class Skin
        {
            public static Color Background, Panel, Editor, Hover, Border;
            public static Color Accent, Text, Muted;
            public static bool IsLight;

            // Лёгкий оттенок акцента на рабочем поле
            public static Color EditorTint => Blend(Editor, Accent, 0.05f);

            // На светлых темах акцент для текста чуть темнее — иначе плохо читается
            public static Color AccentText => IsLight ? Blend(Accent, Color.Black, 0.30f) : Accent;

            // Цвет текста поверх акцентной заливки
            public static Color OnAccent => Luma(Accent) > 0.62 ? Color.Black : Color.White;
        }

        private static Color Blend(Color a, Color b, float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            return Color.FromArgb(
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));
        }

        private static double Luma(Color c) =>
            (0.2126 * c.R + 0.7152 * c.G + 0.0722 * c.B) / 255.0;

        private void ResolveSkin()
        {
            Preset p = presets[presetIndex];

            Skin.Background = p.Bg;
            Skin.Panel = p.Panel;
            Skin.Editor = p.Editor;
            Skin.Hover = p.Hover;
            Skin.Border = p.Border;
            Skin.IsLight = p.Light;

            Skin.Accent = accents[accentIndex];
            Skin.Text = customTextColor
                ?? (textColorIndex > 0 ? textColors[textColorIndex - 1] : p.Text);
            Skin.Muted = Blend(Skin.Text, Skin.Panel, 0.42f);
        }

        private void ApplyAppearance()
        {
            ResolveSkin();
            ApplyTheme();
            SaveSettings();
        }

        // ═════════════════════════════════════════════════════════════
        //  ЭЛЕМЕНТЫ ИНТЕРФЕЙСА
        // ═════════════════════════════════════════════════════════════
        private const int HeaderHeight = 52;
        private const int MenuHeight = 30;
        private const int StatusHeight = 28;
        private const int SidebarWidth = 58;
        private const int ResizeBorder = 7;

        private static readonly bool HasMdl2 = FontFamilyExists("Segoe MDL2 Assets");

        private TableLayoutPanel root;
        private Panel header, workspace, sidebar, sideEdge, editorHost, sideSeparator;
        private FlowLayoutPanel topFlow, bottomFlow;

        private Label logoLabel, subtitleLabel, fileLabel;
        private Button minimizeButton, maximizeButton, closeButton;
        private Button newButton, openButton, saveButton, saveAsButton, printButton;
        private Button findButton, gotoButton, wrapButton;
        private Button appearanceButton, zenButton;

        private ToolTip toolTip;
        private Font iconFont;

        // Панель поиска / замены
        private Panel findBar;
        private EditBox findBox, replaceBox;
        private Button matchCaseButton, findPrevButton, findNextButton;
        private Button replaceButton, replaceAllButton, findCloseButton;
        private Label matchLabel;

        // ─────────── Сборка ───────────
        private void BuildInterface()
        {
            SuspendLayout();

            // Забираем элементы из Designer: меню, редактор и строку состояния
            Controls.Remove(menuStrip1);
            Controls.Remove(TextBox);
            Controls.Remove(statusStrip1);

            // Рамка вокруг окна: именно в ней ловятся края для изменения размера
            Padding = new Padding(ResizeBorder);

            toolTip = new ToolTip { InitialDelay = 400 };
            iconFont = new Font(HasMdl2 ? "Segoe MDL2 Assets" : "Segoe UI Symbol", HasMdl2 ? 13F : 15F);

            root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, HeaderHeight));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, MenuHeight));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, StatusHeight));

            BuildHeader();

            menuStrip1.Dock = DockStyle.Fill;
            menuStrip1.Margin = Padding.Empty;

            workspace = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };

            BuildSidebar();

            editorHost = new Panel { Dock = DockStyle.Fill };
            editorHost.SizeChanged += (s, e) => UpdateEditorPadding();

            TextBox.Dock = DockStyle.Fill;
            TextBox.Margin = Padding.Empty;
            editorHost.Controls.Add(TextBox);

            BuildFindBar();

            // Порядок важен: сначала Fill, потом остальные «края»
            workspace.Controls.Add(editorHost);
            workspace.Controls.Add(findBar);
            workspace.Controls.Add(sidebar);

            statusStrip1.Dock = DockStyle.Fill;
            statusStrip1.Margin = Padding.Empty;

            root.Controls.Add(header, 0, 0);
            root.Controls.Add(menuStrip1, 0, 1);
            root.Controls.Add(workspace, 0, 2);
            root.Controls.Add(statusStrip1, 0, 3);

            Controls.Add(root);
            MainMenuStrip = menuStrip1;

            UpdateLayoutRows();
            ResumeLayout(true);
        }

        // ─────────── Верхняя панель ───────────
        private void BuildHeader()
        {
            header = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };

            logoLabel = new Label
            {
                Text = "A U R E O N",
                Font = new Font("Segoe UI Semibold", 12.5F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 8),
                BackColor = Color.Transparent
            };

            subtitleLabel = new Label
            {
                Text = "TEXT EDITOR",
                Font = new Font("Segoe UI", 7F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(21, 30),
                BackColor = Color.Transparent
            };

            fileLabel = new Label
            {
                Font = new Font("Segoe UI", 10F),
                AutoSize = true,
                Location = new Point(190, 17),
                BackColor = Color.Transparent
            };

            header.Controls.Add(logoLabel);
            header.Controls.Add(subtitleLabel);
            header.Controls.Add(fileLabel);

            foreach (Control c in new Control[] { header, logoLabel, subtitleLabel, fileLabel })
                c.MouseDown += Header_MouseDown;

            minimizeButton = CreateWindowButton("\uE921", "—",
                (s, e) => WindowState = FormWindowState.Minimized);
            maximizeButton = CreateWindowButton("\uE922", "□",
                (s, e) => ToggleMaximize());
            closeButton = CreateWindowButton("\uE8BB", "×",
                (s, e) => Close());

            closeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(196, 43, 28);
            closeButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(160, 30, 20);
            closeButton.MouseEnter += (s, e) => closeButton.ForeColor = Color.White;
            closeButton.MouseLeave += (s, e) => closeButton.ForeColor = Skin.Text;

            // Последний добавленный Dock.Right оказывается самым правым
            header.Controls.Add(minimizeButton);
            header.Controls.Add(maximizeButton);
            header.Controls.Add(closeButton);
        }

        private Button CreateWindowButton(string glyph, string fallback, EventHandler click)
        {
            var b = new Button
            {
                Text = HasMdl2 ? glyph : fallback,
                Font = HasMdl2 ? new Font("Segoe MDL2 Assets", 9F) : new Font("Segoe UI", 12F),
                Dock = DockStyle.Right,
                Width = 48,
                FlatStyle = FlatStyle.Flat,
                TabStop = false,
                Cursor = Cursors.Default
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += click;
            return b;
        }

        private void Header_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (e.Clicks == 2)
            {
                ToggleMaximize();
                return;
            }

            if (WindowState == FormWindowState.Maximized)
                return;

            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
        }

        private void ToggleMaximize()
        {
            WindowState = WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal
                : FormWindowState.Maximized;
            UpdateMaxButton();
        }

        private void UpdateMaxButton()
        {
            if (maximizeButton == null)
                return;

            bool max = WindowState == FormWindowState.Maximized;
            maximizeButton.Text = HasMdl2
                ? (max ? "\uE923" : "\uE922")
                : (max ? "❐" : "□");
        }

        // ─────────── Боковая панель ───────────
        private void BuildSidebar()
        {
            sidebar = new Panel { Dock = DockStyle.Left, Width = SidebarWidth, Margin = Padding.Empty };
            sideEdge = new Panel { Dock = DockStyle.Right, Width = 1 };

            topFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(9, 12, 0, 0),
                Margin = Padding.Empty
            };

            bottomFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(9, 0, 0, 8),
                Margin = Padding.Empty
            };

            newButton = CreateSideButton("\uE710", "＋", "Новий файл  (Ctrl+Shift+N)",
                (s, e) => { новыйToolStripMenuItem_Click(s, e); TextBox.Focus(); });

            openButton = CreateSideButton("\uE8E5", "↗", "Відкрити  (Ctrl+O)",
                (s, e) => { открытьToolStripMenuItem_Click(s, e); TextBox.Focus(); });

            saveButton = CreateSideButton("\uE74E", "↓", "Зберегти  (Ctrl+S)",
                (s, e) => { сохранитьToolStripMenuItem_Click(s, e); TextBox.Focus(); });

            saveAsButton = CreateSideButton("\uE792", "⇩", "Зберегти як…  (Ctrl+Shift+S)",
                (s, e) => { сохранитьКакToolStripMenuItem_Click(s, e); TextBox.Focus(); });

            printButton = CreateSideButton("\uE749", "⎙", "Друк  (Ctrl+P)",
                (s, e) => { друкToolStripMenuItem_Click(s, e); TextBox.Focus(); });

            findButton = CreateSideButton("\uE721", "⌕", "Пошук і заміна  (Ctrl+F / Ctrl+H)",
                (s, e) => ShowFindBar(false));

            gotoButton = CreateSideButton("\uE7C3", "№", "Перейти до рядка  (Ctrl+G)",
                (s, e) => перейтиToolStripMenuItem_Click(s, e));

            wrapButton = CreateSideButton("\uE751", "↵", "Перенос по словах",
                (s, e) => { переносПоСловамToolStripMenuItem_Click(s, e); TextBox.Focus(); });

            appearanceButton = CreateSideButton("\uE790", "◐", "Оформлення  (Ctrl+T)",
                (s, e) => ShowAppearancePopup());

            zenButton = CreateSideButton("\uE740", "◉", "Zen-режим  (F11)",
                (s, e) => ToggleZenMode());

            sideSeparator = new Panel
            {
                Size = new Size(32, 1),
                Margin = new Padding(4, 4, 0, 10)
            };

            topFlow.Controls.Add(newButton);
            topFlow.Controls.Add(openButton);
            topFlow.Controls.Add(saveButton);
            topFlow.Controls.Add(saveAsButton);
            topFlow.Controls.Add(printButton);
            topFlow.Controls.Add(sideSeparator);
            topFlow.Controls.Add(findButton);
            topFlow.Controls.Add(gotoButton);
            topFlow.Controls.Add(wrapButton);

            bottomFlow.Controls.Add(appearanceButton);
            bottomFlow.Controls.Add(zenButton);

            sidebar.Controls.Add(topFlow);
            sidebar.Controls.Add(bottomFlow);
            sidebar.Controls.Add(sideEdge);
        }

        private Button CreateSideButton(string glyph, string fallback, string tip, EventHandler click)
        {
            var b = new Button
            {
                Text = HasMdl2 ? glyph : fallback,
                Font = iconFont,
                Size = new Size(40, 40),
                Margin = new Padding(0, 0, 0, 2),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TabStop = false
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += click;
            toolTip.SetToolTip(b, tip);
            return b;
        }

        // Подсветка кнопки «Перенос по словах» акцентом, когда режим включён
        private void UpdateWrapButton()
        {
            if (wrapButton == null)
                return;

            bool on = TextBox.WordWrap;
            wrapButton.BackColor = on ? Blend(Skin.Panel, Skin.Accent, 0.45f) : Skin.Panel;
            wrapButton.ForeColor = on ? Skin.OnAccent : Skin.Text;
        }

        // ─────────── Панель поиска / замены ───────────
        private void BuildFindBar()
        {
            findBar = new Panel { Dock = DockStyle.Top, Height = 46, Visible = false };

            var inputFont = new Font("Segoe UI", 10.5F);

            findBox = new EditBox
            {
                Location = new Point(16, 9),
                Width = 270,
                Font = inputFont,
                BorderStyle = BorderStyle.FixedSingle
            };

            replaceBox = new EditBox
            {
                Location = new Point(16, 47),
                Width = 270,
                Font = inputFont,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };

            matchCaseButton = CreateBarButton("Aa", 36, 294, 8, "Враховувати регістр");
            findPrevButton = CreateBarButton(HasMdl2 ? "\uE70E" : "▲", 32, 334, 8, "Попереднє  (Shift+F3)");
            findNextButton = CreateBarButton(HasMdl2 ? "\uE70D" : "▼", 32, 370, 8, "Наступне  (F3)");

            matchLabel = new Label
            {
                AutoSize = true,
                Location = new Point(412, 14),
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.Transparent
            };

            replaceButton = CreateBarButton("Замінити", 84, 294, 46, "Замінити поточне");
            replaceAllButton = CreateBarButton("Замінити все", 104, 384, 46, "Замінити всі збіги");
            replaceButton.Visible = false;
            replaceAllButton.Visible = false;

            findCloseButton = new Button
            {
                Text = HasMdl2 ? "\uE8BB" : "×",
                Font = HasMdl2 ? new Font("Segoe MDL2 Assets", 9F) : new Font("Segoe UI", 12F),
                Dock = DockStyle.Right,
                Width = 44,
                FlatStyle = FlatStyle.Flat,
                TabStop = false,
                Cursor = Cursors.Hand
            };
            findCloseButton.FlatAppearance.BorderSize = 0;
            toolTip.SetToolTip(findCloseButton, "Закрити  (Esc)");

            findBox.TextChanged += (s, e) => FindIncremental();
            findBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    FindNext(!e.Shift);
                    e.SuppressKeyPress = true;
                }
            };
            replaceBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    ReplaceOne();
                    e.SuppressKeyPress = true;
                }
            };

            matchCaseButton.Click += (s, e) =>
            {
                matchCase = !matchCase;
                StyleBarButton(matchCaseButton, matchCase);
                FindIncremental();
            };
            findPrevButton.Click += (s, e) => FindNext(false);
            findNextButton.Click += (s, e) => FindNext(true);
            replaceButton.Click += (s, e) => ReplaceOne();
            replaceAllButton.Click += (s, e) => ReplaceAll();
            findCloseButton.Click += (s, e) => CloseFindBar();

            findBar.Controls.Add(findBox);
            findBar.Controls.Add(replaceBox);
            findBar.Controls.Add(matchCaseButton);
            findBar.Controls.Add(findPrevButton);
            findBar.Controls.Add(findNextButton);
            findBar.Controls.Add(matchLabel);
            findBar.Controls.Add(replaceButton);
            findBar.Controls.Add(replaceAllButton);
            findBar.Controls.Add(findCloseButton);
        }

        private Button CreateBarButton(string text, int width, int x, int y, string tip)
        {
            bool glyph = text.Length == 1 && text[0] >= '\uE000' && HasMdl2;

            var b = new Button
            {
                Text = text,
                Font = glyph ? new Font("Segoe MDL2 Assets", 9F) : new Font("Segoe UI", 9.5F),
                Size = new Size(width, 30),
                Location = new Point(x, y),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TabStop = false
            };
            b.FlatAppearance.BorderSize = 0;
            toolTip.SetToolTip(b, tip);
            return b;
        }

        private void StyleBarButton(Button b, bool active)
        {
            b.BackColor = active ? Blend(Skin.Panel, Skin.Accent, 0.45f) : Skin.Hover;
            b.ForeColor = Skin.Text;
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Blend(b.BackColor, Skin.Accent, 0.25f);
            b.FlatAppearance.MouseDownBackColor = Blend(b.BackColor, Skin.Accent, 0.45f);
        }

        private void StyleFindBar()
        {
            findBar.BackColor = Skin.Panel;

            foreach (EditBox tb in new[] { findBox, replaceBox })
            {
                tb.BackColor = Skin.EditorTint;
                tb.ForeColor = Skin.Text;
            }

            StyleBarButton(matchCaseButton, matchCase);
            StyleBarButton(findPrevButton, false);
            StyleBarButton(findNextButton, false);
            StyleBarButton(replaceButton, false);
            StyleBarButton(replaceAllButton, false);

            matchLabel.ForeColor = Skin.Muted;

            findCloseButton.BackColor = Skin.Panel;
            findCloseButton.ForeColor = Skin.Text;
            findCloseButton.FlatAppearance.MouseOverBackColor = Skin.Hover;
            findCloseButton.FlatAppearance.MouseDownBackColor = Blend(Skin.Hover, Skin.Accent, 0.35f);
        }

        private void ShowFindBar(bool replace)
        {
            findBar.Height = replace ? 84 : 46;
            replaceBox.Visible = replace;
            replaceButton.Visible = replace;
            replaceAllButton.Visible = replace;

            if (TextBox.SelectionLength > 0 && TextBox.SelectionLength < 200 &&
                !TextBox.SelectedText.Contains("\n"))
            {
                findBox.Text = TextBox.SelectedText;
            }

            findBar.Visible = true;
            StyleFindBar();

            findBox.Focus();
            findBox.SelectAll();
            UpdateMatchInfo();
        }

        private void CloseFindBar()
        {
            findBar.Visible = false;
            TextBox.Focus();
        }

        // ─────────── Меню ───────────
        private void InitializeMenus()
        {
            menuStrip1.Renderer = new AureonMenuRenderer();
            menuStrip1.Font = new Font("Segoe UI", 9.5F);
            menuStrip1.GripStyle = ToolStripGripStyle.Hidden;
            menuStrip1.AutoSize = false;
            menuStrip1.Padding = new Padding(8, 3, 8, 3);

            // Стандартные клавиши (Ctrl+Z/X/C/V/A, Del) обрабатывает сам RichTextBox.
            // Если оставить их горячими клавишами меню, они перехватываются и ломают
            // ввод в поле поиска (например, Delete или Ctrl+V).
            SetDisplayOnly(отменитьToolStripMenuItem, "Ctrl+Z");
            SetDisplayOnly(вырезатьToolStripMenuItem, "Ctrl+X");
            SetDisplayOnly(копироватьToolStripMenuItem, "Ctrl+C");
            SetDisplayOnly(вставитьToolStripMenuItem, "Ctrl+V");
            SetDisplayOnly(удалитьToolStripMenuItem, "Del");
            SetDisplayOnly(выбратьВсёToolStripMenuItem, "Ctrl+A");

            // В Designer масштаб висел на Ctrl+N / Ctrl+M — это неудобно и конфликтует с «Новий файл».
            увеличитьToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Oemplus;
            увеличитьToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl++";
            уменьшитьToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.OemMinus;
            уменьшитьToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+-";

            var resetZoom = new ToolStripMenuItem("Скинути масштаб") { ShortcutKeys = Keys.Control | Keys.D0 };
            resetZoom.Click += (s, e) =>
            {
                TextBox.ZoomFactor = 1f;
                UpdateZoomLabel();
            };
            масштабToolStripMenuItem.DropDownItems.Add(resetZoom);

            var redo = new ToolStripMenuItem("Повторити") { ShortcutKeyDisplayString = "Ctrl+Y" };
            redo.Click += (s, e) => { if (TextBox.CanRedo) TextBox.Redo(); };
            редактироватьToolStripMenuItem.DropDownItems.Insert(1, redo);

            var duplicate = new ToolStripMenuItem("Дублювати рядок") { ShortcutKeys = Keys.Control | Keys.D };
            duplicate.Click += (s, e) => DuplicateLine();

            var dateTime = new ToolStripMenuItem("Дата й час") { ShortcutKeys = Keys.F5 };
            dateTime.Click += (s, e) => TextBox.SelectedText = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

            int at = редактироватьToolStripMenuItem.DropDownItems.IndexOf(выбратьВсёToolStripMenuItem) + 1;
            редактироватьToolStripMenuItem.DropDownItems.Insert(at, duplicate);
            редактироватьToolStripMenuItem.DropDownItems.Insert(at + 1, dateTime);

            var appearance = new ToolStripMenuItem("Оформлення…") { ShortcutKeys = Keys.Control | Keys.T };
            appearance.Click += (s, e) => ShowAppearancePopup();

            var zen = new ToolStripMenuItem("Zen-режим") { ShortcutKeyDisplayString = "F11" };
            zen.Click += (s, e) => ToggleZenMode();

            просмотретьToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
            просмотретьToolStripMenuItem.DropDownItems.Add(appearance);
            просмотретьToolStripMenuItem.DropDownItems.Add(zen);

            переносПоСловамToolStripMenuItem.Checked = TextBox.WordWrap;
            строкаСостоянияToolStripMenuItem.Checked = statusVisible;

            // Горячие клавиши для действий, вынесенных на боковую панель
            перейтиToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.G;
            сохранитьКакToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
            друкToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.P;

            UpdateWrapButton();
        }

        private static void SetDisplayOnly(ToolStripMenuItem item, string text)
        {
            item.ShortcutKeys = Keys.None;
            item.ShortcutKeyDisplayString = text;
        }

        private void StyleMenuItems(ToolStripItemCollection items)
        {
            foreach (ToolStripItem item in items)
            {
                item.ForeColor = Skin.Text;

                if (item is ToolStripMenuItem mi && mi.HasDropDownItems)
                {
                    mi.DropDown.BackColor = Skin.Panel;
                    mi.DropDown.ForeColor = Skin.Text;
                    StyleMenuItems(mi.DropDownItems);
                }
            }
        }

        // ─────────── Раскладка / Zen ───────────
        private void UpdateLayoutRows()
        {
            if (root == null)
                return;

            // Меню остаётся Visible (просто со строкой высотой 0), иначе в Zen-режиме
            // перестанут работать его горячие клавиши (Ctrl+S, Ctrl+O и т.д.).
            root.RowStyles[0].Height = isZenMode ? 0 : HeaderHeight;
            root.RowStyles[1].Height = isZenMode ? 0 : MenuHeight;
            root.RowStyles[3].Height = (isZenMode || !statusVisible) ? 0 : StatusHeight;

            header.Visible = !isZenMode;
            sidebar.Visible = !isZenMode;
            statusStrip1.Visible = !isZenMode && statusVisible;

            UpdateEditorPadding();
        }

        private void UpdateEditorPadding()
        {
            if (editorHost == null)
                return;

            Padding target;
            if (isZenMode)
            {
                int side = Math.Max(48, (editorHost.ClientSize.Width - 880) / 2);
                target = new Padding(side, 48, side, 40);
            }
            else
            {
                target = new Padding(18, 14, 10, 8);
            }

            if (editorHost.Padding != target)
                editorHost.Padding = target;
        }

        private void ToggleZenMode()
        {
            isZenMode = !isZenMode;
            UpdateLayoutRows();
            TextBox.Focus();
        }

        // ═════════════════════════════════════════════════════════════
        //  ПРИМЕНЕНИЕ ТЕМЫ
        // ═════════════════════════════════════════════════════════════
        private void ApplyTheme()
        {
            if (root == null)
                return;

            SuspendLayout();

            BackColor = Skin.Background;
            root.BackColor = Skin.Background;
            workspace.BackColor = Skin.Background;

            // Верхняя панель
            header.BackColor = Skin.Panel;
            logoLabel.ForeColor = Skin.AccentText;
            subtitleLabel.ForeColor = Skin.Muted;
            fileLabel.ForeColor = Skin.Text;

            foreach (Button b in new[] { minimizeButton, maximizeButton, closeButton })
            {
                b.BackColor = Skin.Panel;
                b.ForeColor = Skin.Text;
                if (b != closeButton)
                {
                    b.FlatAppearance.MouseOverBackColor = Skin.Hover;
                    b.FlatAppearance.MouseDownBackColor = Blend(Skin.Hover, Skin.Accent, 0.35f);
                }
            }

            // Боковая панель
            sidebar.BackColor = Skin.Panel;
            topFlow.BackColor = Skin.Panel;
            bottomFlow.BackColor = Skin.Panel;
            sideEdge.BackColor = Skin.Border;
            sideSeparator.BackColor = Skin.Border;

            foreach (Button b in new[]
            {
                newButton, openButton, saveButton, saveAsButton, printButton,
                findButton, gotoButton, wrapButton, appearanceButton, zenButton
            })
            {
                b.BackColor = Skin.Panel;
                b.ForeColor = Skin.Text;
                b.FlatAppearance.MouseOverBackColor =
                    Blend(Skin.Panel, Skin.Accent, Skin.IsLight ? 0.20f : 0.28f);
                b.FlatAppearance.MouseDownBackColor =
                    Blend(Skin.Panel, Skin.Accent, Skin.IsLight ? 0.32f : 0.42f);
            }

            UpdateWrapButton();

            // Редактор
            editorHost.BackColor = Skin.EditorTint;
            TextBox.BackColor = Skin.EditorTint;
            TextBox.ForeColor = Skin.Text;

            // Поиск
            StyleFindBar();
            UpdateMatchInfo();

            // Меню и строка состояния
            menuStrip1.BackColor = Skin.Panel;
            menuStrip1.ForeColor = Skin.Text;
            StyleMenuItems(menuStrip1.Items);

            statusStrip1.BackColor = Blend(Skin.Panel, Skin.Accent, 0.08f);
            statusStrip1.ForeColor = Skin.Muted;
            foreach (ToolStripItem item in statusStrip1.Items)
                item.ForeColor = Skin.Muted;
            if (positionLabel != null)
                positionLabel.ForeColor = Skin.AccentText;

            UpdateMaxButton();

            ResumeLayout(true);
            menuStrip1.Invalidate(true);
            statusStrip1.Invalidate(true);
            Invalidate(true);
        }

        // ═════════════════════════════════════════════════════════════
        //  ПОПАП «ОФОРМЛЕННЯ»
        // ═════════════════════════════════════════════════════════════
        private Form appearancePopup;
        private DateTime popupClosedAt = DateTime.MinValue;

        private void ShowAppearancePopup()
        {
            if (isZenMode)
                return;

            if (appearancePopup != null && !appearancePopup.IsDisposed)
            {
                appearancePopup.Close();
                return;
            }

            // Клик по кнопке закрывает попап через Deactivate — не открываем его заново тут же.
            if ((DateTime.UtcNow - popupClosedAt).TotalMilliseconds < 300)
                return;

            var popup = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                Width = 300,
                KeyPreview = true
            };
            appearancePopup = popup;

            popup.Deactivate += (s, e) => popup.Close();
            popup.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) popup.Close(); };
            popup.FormClosed += (s, e) =>
            {
                popupClosedAt = DateTime.UtcNow;
                appearancePopup = null;
            };

            Action rebuild = null;
            rebuild = () =>
            {
                if (!popup.IsDisposed)
                    BuildAppearanceContent(popup, rebuild);
            };
            rebuild();

            Point pt = appearanceButton.PointToScreen(new Point(appearanceButton.Width + 10, appearanceButton.Height));
            Rectangle wa = Screen.FromControl(this).WorkingArea;
            int y = Math.Max(wa.Top + 8, Math.Min(pt.Y - popup.Height, wa.Bottom - popup.Height - 8));
            popup.Location = new Point(pt.X, y);

            popup.Show(this);
        }

        private void BuildAppearanceContent(Form popup, Action rebuild)
        {
            popup.SuspendLayout();

            while (popup.Controls.Count > 0)
                popup.Controls[0].Dispose();

            popup.BackColor = Skin.Border;
            popup.Padding = new Padding(1);

            var inner = new Panel { Dock = DockStyle.Fill, BackColor = Skin.Panel };
            popup.Controls.Add(inner);

            const int left = 16;
            const int contentWidth = 266;
            int y = 14;

            void AddSection(string text)
            {
                inner.Controls.Add(new Label
                {
                    Text = text,
                    AutoSize = true,
                    Location = new Point(left, y),
                    Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                    ForeColor = Skin.Muted,
                    BackColor = Color.Transparent
                });
                y += 22;
            }

            void AddSwatch(Color color, bool selected, string tip, int sx, int sy, Action click, string glyph = null)
            {
                var sw = new Panel
                {
                    Location = new Point(sx, sy),
                    Size = new Size(30, 30),
                    Cursor = Cursors.Hand,
                    BackColor = Skin.Panel
                };

                sw.Paint += (s, e) =>
                {
                    Graphics g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    using (var br = new SolidBrush(color))
                        g.FillEllipse(br, 5, 5, 20, 20);
                    using (var pen = new Pen(Skin.Border, 1f))
                        g.DrawEllipse(pen, 5, 5, 20, 20);

                    if (selected)
                    {
                        using (var ring = new Pen(Skin.Text, 2f))
                            g.DrawEllipse(ring, 1, 1, 27, 27);
                    }

                    if (glyph != null)
                    {
                        Color gc = Luma(color) > 0.5 ? Color.Black : Color.White;
                        TextRenderer.DrawText(g, glyph, Font, new Rectangle(0, 0, 30, 30), gc,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                };

                sw.Click += (s, e) => click();
                toolTip.SetToolTip(sw, tip);
                inner.Controls.Add(sw);
            }

            // ── Тема ──
            AddSection("ТЕМА");
            for (int i = 0; i < presets.Length; i++)
            {
                int index = i;
                Preset p = presets[i];
                bool selected = i == presetIndex;

                var row = new Panel
                {
                    Location = new Point(left, y),
                    Size = new Size(contentWidth, 30),
                    Cursor = Cursors.Hand,
                    BackColor = selected ? Blend(Skin.Panel, Skin.Accent, 0.22f) : Skin.Panel
                };

                row.Paint += (s, e) =>
                {
                    Graphics g = e.Graphics;
                    int x = 8;
                    foreach (Color c in new[] { p.Bg, p.Panel, p.Editor })
                    {
                        using (var br = new SolidBrush(c))
                            g.FillRectangle(br, x, 8, 14, 14);
                        x += 14;
                    }
                    using (var pen = new Pen(Skin.Border))
                        g.DrawRectangle(pen, 8, 8, 41, 13);

                    TextRenderer.DrawText(g, p.Name, Font,
                        new Rectangle(60, 0, contentWidth - 64, 30),
                        selected ? Skin.AccentText : Skin.Text,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                };

                row.Click += (s, e) =>
                {
                    presetIndex = index;
                    ApplyAppearance();
                    popup.BeginInvoke(rebuild);
                };

                inner.Controls.Add(row);
                y += 32;
            }

            // ── Акцент ──
            y += 6;
            AddSection("АКЦЕНТ");
            for (int i = 0; i < accents.Length; i++)
            {
                int index = i;
                AddSwatch(accents[i], i == accentIndex, accentNames[i],
                    left + i * 33, y, () =>
                    {
                        accentIndex = index;
                        ApplyAppearance();
                        popup.BeginInvoke(rebuild);
                    });
            }
            y += 40;

            // ── Цвет текста ──
            AddSection("КОЛІР ТЕКСТУ");
            int slot = 0;

            void NextPos(out int px, out int py)
            {
                px = left + (slot % 8) * 33;
                py = y + (slot / 8) * 33;
                slot++;
            }

            NextPos(out int ax, out int ay);
            AddSwatch(presets[presetIndex].Text, customTextColor == null && textColorIndex == 0,
                "Авто (колір теми)", ax, ay, () =>
                {
                    textColorIndex = 0;
                    customTextColor = null;
                    ApplyAppearance();
                    popup.BeginInvoke(rebuild);
                }, "A");

            for (int i = 0; i < textColors.Length; i++)
            {
                int index = i + 1;
                NextPos(out int px, out int py);
                AddSwatch(textColors[i], customTextColor == null && textColorIndex == index,
                    textColorNames[i], px, py, () =>
                    {
                        textColorIndex = index;
                        customTextColor = null;
                        ApplyAppearance();
                        popup.BeginInvoke(rebuild);
                    });
            }

            NextPos(out int cx, out int cy);
            AddSwatch(customTextColor ?? Skin.Panel, customTextColor != null,
                "Свій колір…", cx, cy, () =>
                {
                    popup.Close();
                    BeginInvoke(new Action(ChooseCustomTextColor));
                }, "+");

            y += ((slot - 1) / 8 + 1) * 33 + 10;

            popup.Height = y + 2;
            popup.ResumeLayout(true);
        }

        private void ChooseCustomTextColor()
        {
            using (var dlg = new ColorDialog { Color = Skin.Text, FullOpen = true })
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    customTextColor = dlg.Color;
                    ApplyAppearance();
                }
            }
        }

        // ═════════════════════════════════════════════════════════════
        //  ДИАЛОГИ В СТИЛЕ AUREON
        // ═════════════════════════════════════════════════════════════
        private sealed class DlgButton
        {
            public string Text;
            public DialogResult Result;
            public int Kind;            // 0 — обычная, 1 — основная (акцент), 2 — опасная

            public DlgButton(string text, DialogResult result, int kind = 0)
            {
                Text = text;
                Result = result;
                Kind = kind;
            }
        }

        private Form CreateDialogShell(string title, int width, int height,
            out Panel body, out FlowLayoutPanel footer)
        {
            var dlg = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(width, height),
                BackColor = Skin.Border,           // 1px рамка
                Padding = new Padding(1),
                ShowInTaskbar = false,
                KeyPreview = true
            };

            var inner = new Panel { Dock = DockStyle.Fill, BackColor = Skin.Background };
            var bodyPanel = new Panel { Dock = DockStyle.Fill, BackColor = Skin.Background };

            var footerPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 62,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(16, 12, 16, 12),
                BackColor = Skin.Panel
            };

            var bar = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Skin.Panel };

            var caption = new Label
            {
                Text = title,
                AutoSize = true,
                Location = new Point(18, 13),
                Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold),
                ForeColor = Skin.AccentText,
                BackColor = Color.Transparent
            };

            var close = new Button
            {
                Text = HasMdl2 ? "\uE8BB" : "×",
                Font = HasMdl2 ? new Font("Segoe MDL2 Assets", 9F) : new Font("Segoe UI", 13F),
                Dock = DockStyle.Right,
                Width = 46,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Skin.Text,
                BackColor = Skin.Panel,
                TabStop = false
            };
            close.FlatAppearance.BorderSize = 0;
            close.FlatAppearance.MouseOverBackColor = Color.FromArgb(196, 43, 28);
            close.Click += (s, e) => dlg.DialogResult = DialogResult.Cancel;

            MouseEventHandler drag = (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(dlg.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
                }
            };
            bar.MouseDown += drag;
            caption.MouseDown += drag;

            bar.Controls.Add(caption);
            bar.Controls.Add(close);

            inner.Controls.Add(bodyPanel);
            inner.Controls.Add(footerPanel);
            inner.Controls.Add(bar);
            dlg.Controls.Add(inner);

            body = bodyPanel;
            footer = footerPanel;
            return dlg;
        }

        private Button CreateDialogButton(string text, int kind)
        {
            Color back, fore;

            if (kind == 1)
            {
                back = Skin.Accent;
                fore = Skin.OnAccent;
            }
            else if (kind == 2)
            {
                back = Skin.IsLight ? Hex(0xF3D0D0) : Hex(0x452D37);
                fore = Skin.IsLight ? Hex(0xA32030) : Hex(0xFF828C);
            }
            else
            {
                back = Skin.Hover;
                fore = Skin.Text;
            }

            var font = new Font("Segoe UI Semibold", 9.5F);
            int width = Math.Max(96, TextRenderer.MeasureText(text, font).Width + 36);

            var b = new Button
            {
                Text = text,
                Font = font,
                Size = new Size(width, 38),
                FlatStyle = FlatStyle.Flat,
                BackColor = back,
                ForeColor = fore,
                Cursor = Cursors.Hand,
                Margin = new Padding(8, 0, 0, 0)
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor =
                Blend(back, Skin.IsLight ? Color.Black : Color.White, 0.10f);
            return b;
        }

        private DialogResult ShowAureonDialog(string heading, string message, string glyph,
            DialogResult escapeResult, params DlgButton[] buttons)
        {
            Panel body;
            FlowLayoutPanel footer;

            using (Form dlg = CreateDialogShell("AUREON", 500, 230, out body, out footer))
            {
                body.Controls.Add(new Label
                {
                    Text = glyph,
                    Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                    ForeColor = Skin.AccentText,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Size = new Size(48, 48),
                    Location = new Point(24, 22),
                    BackColor = Color.Transparent
                });

                body.Controls.Add(new Label
                {
                    Text = heading,
                    Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold),
                    ForeColor = Skin.Text,
                    AutoSize = true,
                    Location = new Point(84, 22),
                    BackColor = Color.Transparent
                });

                body.Controls.Add(new Label
                {
                    Text = message,
                    Font = new Font("Segoe UI", 10F),
                    ForeColor = Skin.Muted,
                    AutoSize = false,
                    Size = new Size(380, 60),
                    Location = new Point(86, 56),
                    BackColor = Color.Transparent
                });

                // RightToLeft: первый добавленный — самый правый, поэтому идём с конца
                for (int i = buttons.Length - 1; i >= 0; i--)
                {
                    DlgButton spec = buttons[i];
                    Button b = CreateDialogButton(spec.Text, spec.Kind);
                    b.DialogResult = spec.Result;
                    footer.Controls.Add(b);

                    if (spec.Kind == 1)
                        dlg.AcceptButton = b;
                }

                dlg.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Escape)
                        dlg.DialogResult = escapeResult;
                };

                return dlg.ShowDialog(this);
            }
        }

        private void ShowInfo(string heading, string message)
        {
            ShowAureonDialog(heading, message, "i", DialogResult.OK,
                new DlgButton("OK", DialogResult.OK, 1));
        }

        // Диалог ввода строки. Возвращает null, если нажали «Скасувати».
        private string AureonPrompt(string title, string label, string initial = "")
        {
            Panel body;
            FlowLayoutPanel footer;

            using (Form dlg = CreateDialogShell(title, 420, 190, out body, out footer))
            {
                body.Controls.Add(new Label
                {
                    Text = label,
                    Font = new Font("Segoe UI", 10F),
                    ForeColor = Skin.Muted,
                    AutoSize = true,
                    Location = new Point(24, 16),
                    BackColor = Color.Transparent
                });

                var input = new EditBox
                {
                    Location = new Point(24, 44),
                    Width = 372,
                    Font = new Font("Segoe UI", 11F),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Skin.EditorTint,
                    ForeColor = Skin.Text,
                    Text = initial
                };
                body.Controls.Add(input);

                Button ok = CreateDialogButton("OK", 1);
                ok.DialogResult = DialogResult.OK;
                Button cancel = CreateDialogButton("Скасувати", 0);
                cancel.DialogResult = DialogResult.Cancel;

                footer.Controls.Add(ok);
                footer.Controls.Add(cancel);
                dlg.AcceptButton = ok;

                dlg.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Escape)
                        dlg.DialogResult = DialogResult.Cancel;
                };

                dlg.Shown += (s, e) =>
                {
                    input.Focus();
                    input.SelectAll();
                };

                return dlg.ShowDialog(this) == DialogResult.OK ? input.Text : null;
            }
        }

        // ═════════════════════════════════════════════════════════════
        //  БЕЗРАМОЧНОЕ ОКНО: рамка, тень, изменение размера, максимизация
        // ═════════════════════════════════════════════════════════════
        private const int WM_NCHITTEST = 0x0084;
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int WM_GETMINMAXINFO = 0x0024;

        private const int HTCLIENT = 1;
        private const int HTCAPTION = 2;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;


        [DllImport(
    "uxtheme.dll",
    CharSet = CharSet.Unicode,
    ExactSpelling = true)]
        private static extern int SetWindowTheme(
    IntPtr hWnd,
    string pszSubAppName,
    string pszSubIdList);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= 0x20000;        // WS_MINIMIZEBOX — сворачивание кликом по значку в панели задач
                cp.ClassStyle |= 0x20000;   // CS_DROPSHADOW — тень у безрамочного окна
                return cp;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            Padding wanted = WindowState == FormWindowState.Maximized
                ? Padding.Empty
                : new Padding(ResizeBorder);

            if (Padding != wanted)
                Padding = wanted;

            UpdateMaxButton();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (WindowState == FormWindowState.Normal)
            {
                using (var pen = new Pen(Skin.Border))
                    e.Graphics.DrawRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_GETMINMAXINFO)
            {
                // Без этого безрамочное окно при максимизации перекрывает панель задач.
                base.WndProc(ref m);

                var mmi = Marshal.PtrToStructure<MINMAXINFO>(m.LParam);
                Screen screen = Screen.FromHandle(Handle);
                Rectangle wa = screen.WorkingArea;
                Rectangle sb = screen.Bounds;

                mmi.ptMaxPosition.x = wa.Left - sb.Left;
                mmi.ptMaxPosition.y = wa.Top - sb.Top;
                mmi.ptMaxSize.x = wa.Width;
                mmi.ptMaxSize.y = wa.Height;

                Marshal.StructureToPtr(mmi, m.LParam, true);
                return;
            }

            if (m.Msg == WM_NCHITTEST && WindowState == FormWindowState.Normal)
            {
                base.WndProc(ref m);

                if ((int)m.Result == HTCLIENT)
                {
                    long lp = m.LParam.ToInt64();
                    int x = unchecked((short)(lp & 0xFFFF));
                    int y = unchecked((short)((lp >> 16) & 0xFFFF));

                    Point p = PointToClient(new Point(x, y));

                    bool left = p.X <= ResizeBorder;
                    bool right = p.X >= ClientSize.Width - ResizeBorder;
                    bool top = p.Y <= ResizeBorder;
                    bool bottom = p.Y >= ClientSize.Height - ResizeBorder;

                    if (top && left) m.Result = (IntPtr)HTTOPLEFT;
                    else if (top && right) m.Result = (IntPtr)HTTOPRIGHT;
                    else if (bottom && left) m.Result = (IntPtr)HTBOTTOMLEFT;
                    else if (bottom && right) m.Result = (IntPtr)HTBOTTOMRIGHT;
                    else if (left) m.Result = (IntPtr)HTLEFT;
                    else if (right) m.Result = (IntPtr)HTRIGHT;
                    else if (top) m.Result = (IntPtr)HTTOP;
                    else if (bottom) m.Result = (IntPtr)HTBOTTOM;
                }
                return;
            }

            base.WndProc(ref m);
        }

        // ═════════════════════════════════════════════════════════════
        //  РЕНДЕРЕР МЕНЮ
        // ═════════════════════════════════════════════════════════════
        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private sealed class AureonMenuRenderer : ToolStripProfessionalRenderer
        {
            public AureonMenuRenderer() : base(new AureonColorTable())
            {
                RoundedEdges = false;
            }

            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                if (e.ToolStrip is MenuStrip || e.ToolStrip is StatusStrip)
                {
                    using (var brush = new SolidBrush(e.ToolStrip.BackColor))
                        e.Graphics.FillRectangle(brush, e.AffectedBounds);

                    // Тонкая линия: снизу у меню, сверху у строки состояния
                    int lineY = e.ToolStrip is MenuStrip ? e.ToolStrip.Height - 1 : 0;
                    using (var pen = new Pen(Skin.Border))
                        e.Graphics.DrawLine(pen, 0, lineY, e.ToolStrip.Width, lineY);
                    return;
                }

                base.OnRenderToolStripBackground(e);
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                if (e.ToolStrip is MenuStrip || e.ToolStrip is StatusStrip)
                    return;

                base.OnRenderToolStripBorder(e);
            }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                if (!e.Item.Selected && !e.Item.Pressed)
                    return;

                var rect = new Rectangle(2, 1, Math.Max(1, e.Item.Width - 4), Math.Max(1, e.Item.Height - 2));
                Color fill = Blend(Skin.Hover, Skin.Accent, 0.30f);

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundedRect(rect, 5))
                using (var brush = new SolidBrush(fill))
                    e.Graphics.FillPath(brush, path);
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = e.Item.Enabled ? Skin.Text : Skin.Muted;
                base.OnRenderItemText(e);
            }

            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
            {
                e.ArrowColor = Skin.Muted;
                base.OnRenderArrow(e);
            }

            protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle r = e.ImageRectangle;
                Rectangle box = new Rectangle(r.X, r.Y, r.Width - 1, r.Height - 1);

                using (GraphicsPath path = RoundedRect(box, 3))
                using (var brush = new SolidBrush(Blend(Skin.Hover, Skin.Accent, 0.55f)))
                    g.FillPath(brush, path);

                using (var pen = new Pen(Skin.OnAccent, 1.8f))
                {
                    g.DrawLines(pen, new[]
                    {
                        new Point(r.Left + 4, r.Top + r.Height / 2),
                        new Point(r.Left + r.Width / 2 - 1, r.Bottom - 5),
                        new Point(r.Right - 5, r.Top + 4)
                    });
                }
            }
        }

        private sealed class AureonColorTable : ProfessionalColorTable
        {
            public AureonColorTable()
            {
                UseSystemColors = false;
            }

            public override Color ToolStripDropDownBackground => Skin.Panel;
            public override Color ImageMarginGradientBegin => Skin.Panel;
            public override Color ImageMarginGradientMiddle => Skin.Panel;
            public override Color ImageMarginGradientEnd => Skin.Panel;
            public override Color MenuBorder => Skin.Border;
            public override Color MenuItemBorder => Color.Transparent;
            public override Color MenuItemSelected => Skin.Hover;
            public override Color MenuItemSelectedGradientBegin => Skin.Hover;
            public override Color MenuItemSelectedGradientEnd => Skin.Hover;
            public override Color MenuItemPressedGradientBegin => Skin.Hover;
            public override Color MenuItemPressedGradientMiddle => Skin.Hover;
            public override Color MenuItemPressedGradientEnd => Skin.Hover;
            public override Color MenuStripGradientBegin => Skin.Panel;
            public override Color MenuStripGradientEnd => Skin.Panel;
            public override Color StatusStripGradientBegin => Skin.Panel;
            public override Color StatusStripGradientEnd => Skin.Panel;
            public override Color ToolStripBorder => Skin.Border;
            public override Color SeparatorDark => Skin.Border;
            public override Color SeparatorLight => Skin.Border;
        }

        private void ApplyScrollBarTheme()
        {
            if (TextBox == null ||
                TextBox.IsDisposed ||
                !TextBox.IsHandleCreated)
                return;

            try
            {
                if (Skin.IsLight)
                {
                    // Светлая тема
                    SetWindowTheme(
                        TextBox.Handle,
                        "Explorer",
                        "ScrollBar");
                }
                else
                {
                    // Тёмная тема
                    SetWindowTheme(
                        TextBox.Handle,
                        "DarkMode_Explorer",
                        "ScrollBar");
                }

                TextBox.Invalidate();
            }
            catch (DllNotFoundException)
            {
                // Продолжаем работать со стандартным оформлением.
            }
            catch (EntryPointNotFoundException)
            {
                // API недоступен — оставляем системную полосу.
            }
        }
    }
}