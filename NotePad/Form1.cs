using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace NotePad
{
    public partial class Form1 : Form
    {
        // ═════════════ Состояние документа ═════════════
        private string currentFilePath;
        private Encoding currentEncoding = new UTF8Encoding(false);
        private bool isTextChanged;
        private bool isLoadingText;      // true, пока текст ставится программно
        private bool isZenMode;
        private bool statusVisible = true;

        // ═════════════ Печать ═════════════
        private PageSetupDialog pageSetupDialog;
        private PageSettings pageSettings;
        private PrintDocument printDocument;
        private int printCharIndex;

        // ═════════════ Статус-бар ═════════════
        private ToolStripStatusLabel positionLabel;
        private ToolStripStatusLabel selectionLabel;
        private ToolStripStatusLabel wordsLabel;
        private ToolStripStatusLabel zoomLabel;
        private ToolStripStatusLabel encodingLabel;

        private Timer countTimer;        // подсчёт слов с задержкой
        private Timer autosaveTimer;     // резервная копия на случай сбоя

        // ═════════════ Сохранённые настройки ═════════════
        private string fontName;
        private float fontSize = 13f;
        private float zoom = 1f;
        private bool wordWrap = true;
        private Size savedSize = Size.Empty;
        private bool savedMaximized;

        // ═════════════ Конструктор ═════════════
        public Form1()
        {
            InitializeComponent();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            LoadSettings();
            ResolveSkin();

            FormBorderStyle = FormBorderStyle.None;
            KeyPreview = true;
            AllowDrop = true;
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw, true);
            MinimumSize = new Size(640, 600);
            StartPosition = FormStartPosition.CenterScreen;
            if (savedSize.Width >= 900 && savedSize.Height >= 600)
                Size = savedSize;

            ConfigureEditor();
            BuildInterface();
            InitializeMenus();
            InitializeStatusStrip();
            InitializePrinting();
            InitializeTimers();

            ApplyTheme();
            UpdateWindowTitle();
            RefreshStatus();

            DragEnter += Form1_DragEnter;
            DragDrop += Form1_DragDrop;
            Shown += Form1_Shown;
            FormClosed += (s, e) =>
            {
                SaveSettings();
                DeleteRecovery();
            };
        }

        // Путь к файлу, который передала Windows (двойной щелчок / «Открыть с помощью»)
        private static string GetStartupFile()
        {
            string[] args = Environment.GetCommandLineArgs();

            // args[0] — сам exe, файл идёт дальше
            for (int i = 1; i < args.Length; i++)
            {
                string a = args[i].Trim('"');
                if (File.Exists(a))
                    return Path.GetFullPath(a);
            }
            return null;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (savedMaximized)
                WindowState = FormWindowState.Maximized;

            string startupFile = GetStartupFile();

            // Если Windows запустила AUREON для конкретного файла,
            // открываем его только в этом новом экземпляре программы.
            // Восстановление несохранённого текста показываем только
            // при обычном запуске без переданного файла.
            if (!string.IsNullOrEmpty(startupFile))
                LoadFile(startupFile);
            else
                TryRestoreRecovery();

            TextBox.Focus();
        }

        // ═════════════ Редактор ═════════════
        private void ConfigureEditor()
        {
            TextBox.HandleCreated += (s, e) =>
            {
                ApplyScrollBarTheme();
            };

            string family = fontName ??
                (FontFamilyExists("Cascadia Code") ? "Cascadia Code" : "Consolas");

            TextBox.Font = new Font(family, fontSize);
            TextBox.WordWrap = wordWrap;
            TextBox.AcceptsTab = true;
            TextBox.DetectUrls = false;
            TextBox.HideSelection = false;
            TextBox.BorderStyle = BorderStyle.None;
            TextBox.AllowDrop = true;
            TextBox.ZoomFactor = Math.Max(0.5f, Math.Min(5f, zoom));

            TextBox.DragEnter += Form1_DragEnter;
            TextBox.DragDrop += Form1_DragDrop;
            TextBox.SelectionChanged += UpdatePosition;
            TextBox.KeyDown += TextBox_KeyDown;
            TextBox.MouseWheel += (s, e) => BeginInvoke(new Action(UpdateZoomLabel));
        }

        // RichTextBox по умолчанию вставляет форматированный текст и реагирует на
        // Ctrl+B/I/U — в простом блокноте это не нужно.
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Control && e.KeyCode == Keys.V) || (e.Shift && e.KeyCode == Keys.Insert))
            {
                if (Clipboard.ContainsText())
                    TextBox.Paste(DataFormats.GetFormat(DataFormats.UnicodeText));
                e.SuppressKeyPress = true;
                e.Handled = true;
                return;
            }

            if (e.Control && !e.Alt)
            {
                switch (e.KeyCode)
                {
                    case Keys.B:
                    case Keys.I:
                    case Keys.U:
                    case Keys.L:
                    case Keys.R:
                    case Keys.J:
                    case Keys.D1:
                    case Keys.D2:
                    case Keys.D5:
                        e.SuppressKeyPress = true;
                        e.Handled = true;
                        break;
                }
            }
        }

        private void SetEditorText(string text)
        {
            isLoadingText = true;
            try
            {
                TextBox.Text = text;
                TextBox.ClearUndo();
                TextBox.Select(0, 0);
            }
            finally
            {
                isLoadingText = false;
            }

            UpdateCounts();
            UpdatePosition(null, null);
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            if (isLoadingText)
                return;

            isTextChanged = true;
            UpdateWindowTitle();

            if (countTimer != null)
            {
                countTimer.Stop();
                countTimer.Start();
            }
        }

        private void UpdateWindowTitle()
        {
            string name = string.IsNullOrEmpty(currentFilePath)
                ? "Без назви"
                : Path.GetFileName(currentFilePath);

            Text = $"{name}{(isTextChanged ? " •" : "")} — AUREON";

            if (fileLabel != null)
                fileLabel.Text = name + (isTextChanged ? "  •" : "");
        }

        // ═════════════ Строка состояния ═════════════
        private void InitializeStatusStrip()
        {
            statusStrip1.Items.Clear();
            statusStrip1.SizingGrip = false;
            statusStrip1.AutoSize = false;
            statusStrip1.Font = new Font("Segoe UI", 9F);
            statusStrip1.Renderer = new AureonMenuRenderer();

            positionLabel = new ToolStripStatusLabel
            {
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(14, 3, 0, 2)
            };
            selectionLabel = new ToolStripStatusLabel
            {
                Margin = new Padding(16, 3, 0, 2),
                Visible = false
            };
            wordsLabel = new ToolStripStatusLabel
            {
                Spring = true,
                TextAlign = ContentAlignment.MiddleRight
            };
            zoomLabel = new ToolStripStatusLabel { Margin = new Padding(18, 3, 0, 2) };
            encodingLabel = new ToolStripStatusLabel { Margin = new Padding(18, 3, 14, 2) };

            statusStrip1.Items.AddRange(new ToolStripItem[]
            {
                positionLabel, selectionLabel, wordsLabel, zoomLabel, encodingLabel
            });
        }

        private void RefreshStatus()
        {
            UpdatePosition(null, null);
            UpdateCounts();
            UpdateZoomLabel();
            UpdateEncodingLabel();
        }

        private void UpdatePosition(object sender, EventArgs e)
        {
            if (positionLabel == null)
                return;

            int pos = TextBox.SelectionStart;
            int line, col;

            if (TextBox.TextLength <= 200000)
            {
                // Логические строки (не зависят от переноса по словам).
                string t = TextBox.Text;
                int end = Math.Min(pos, t.Length);
                int lastNl = -1;
                line = 1;
                for (int i = 0; i < end; i++)
                {
                    if (t[i] == '\n')
                    {
                        line++;
                        lastNl = i;
                    }
                }
                col = pos - lastNl;
            }
            else
            {
                line = TextBox.GetLineFromCharIndex(pos) + 1;
                col = pos - TextBox.GetFirstCharIndexOfCurrentLine() + 1;
            }

            positionLabel.Text = $"Рядок {line}, стовпець {col}";

            selectionLabel.Visible = TextBox.SelectionLength > 0;
            selectionLabel.Text = $"Виділено: {TextBox.SelectionLength}";

            UpdateZoomLabel();
        }

        private void UpdateCounts()
        {
            if (wordsLabel == null)
                return;

            string text = TextBox.Text;
            int words = 0;
            bool inWord = false;

            foreach (char c in text)
            {
                if (char.IsWhiteSpace(c))
                    inWord = false;
                else if (!inWord)
                {
                    inWord = true;
                    words++;
                }
            }

            wordsLabel.Text = $"Слів: {words}   ·   Символів: {text.Length}";
        }

        private void UpdateZoomLabel()
        {
            if (zoomLabel != null)
                zoomLabel.Text = $"{Math.Round(TextBox.ZoomFactor * 100)}%";
        }

        private void UpdateEncodingLabel()
        {
            if (encodingLabel != null)
                encodingLabel.Text = EncodingName(currentEncoding);
        }

        private static string EncodingName(Encoding enc)
        {
            if (enc is UTF8Encoding)
                return enc.GetPreamble().Length > 0 ? "UTF-8 BOM" : "UTF-8";
            return enc.WebName.ToUpperInvariant();
        }

        private static bool FontFamilyExists(string name)
        {
            foreach (FontFamily f in FontFamily.Families)
            {
                if (string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        // ═════════════ Таймеры, автосохранение ═════════════
        private void InitializeTimers()
        {
            countTimer = new Timer { Interval = 180 };
            countTimer.Tick += (s, e) =>
            {
                countTimer.Stop();
                UpdateCounts();
            };

            autosaveTimer = new Timer { Interval = 30000 };
            autosaveTimer.Tick += (s, e) => AutosaveRecovery();
            autosaveTimer.Start();
        }

        private string SettingsDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NotePadPro");

        private string SettingsPath => Path.Combine(SettingsDir, "settings.ini");

        // У каждого запущенного окна свой файл восстановления.
        // Поэтому закрытие одного экземпляра не удаляет резервную копию другого.
        private string RecoveryPath => Path.Combine(
            SettingsDir, $"recovery_{Environment.ProcessId}.txt");

        private void AutosaveRecovery()
        {
            if (!isTextChanged)
                return;

            try
            {
                Directory.CreateDirectory(SettingsDir);
                File.WriteAllText(RecoveryPath, TextBox.Text, Encoding.UTF8);
            }
            catch
            {
                // Резервная копия — не критична.
            }
        }

        // Удаляет только резервную копию текущего процесса.
        private void DeleteRecovery()
        {
            try
            {
                if (File.Exists(RecoveryPath))
                    File.Delete(RecoveryPath);
            }
            catch { }
        }

        private void TryRestoreRecovery()
        {
            string recoveryFile = null;

            try
            {
                if (!Directory.Exists(SettingsDir))
                    return;

                // Находим самую свежую резервную копию от аварийно завершённого окна.
                string[] allBackups = Directory.GetFiles(SettingsDir, "recovery_*.txt");
                var backups = new List<string>();

                foreach (string backup in allBackups)
                {
                    string fileName = Path.GetFileNameWithoutExtension(backup);
                    string processIdText = fileName.Substring("recovery_".Length);

                    // Не предлагаем резервную копию, если соответствующее окно
                    // AUREON всё ещё работает и само владеет этим файлом.
                    if (int.TryParse(processIdText, out int processId) &&
                        IsAureonProcessRunning(processId))
                    {
                        continue;
                    }

                    backups.Add(backup);
                }

                if (backups.Count == 0)
                    return;

                backups.Sort((a, b) =>
                    File.GetLastWriteTimeUtc(b).CompareTo(File.GetLastWriteTimeUtc(a)));

                recoveryFile = backups[0];
                string text = File.ReadAllText(recoveryFile, Encoding.UTF8);

                if (text.Length == 0)
                {
                    DeleteRecoveryFile(recoveryFile);
                    return;
                }

                DialogResult r = ShowAureonDialog(
                    "Відновити текст?",
                    "Знайдено незбережений текст із попереднього сеансу.\nВідновити його?",
                    "?", DialogResult.No,
                    new DlgButton("Відкинути", DialogResult.No),
                    new DlgButton("Відновити", DialogResult.Yes, 1));

                if (r == DialogResult.Yes)
                {
                    SetEditorText(text);
                    isTextChanged = true;
                    UpdateWindowTitle();

                    // Переносим копию в файл нового процесса до удаления старой.
                    Directory.CreateDirectory(SettingsDir);
                    File.WriteAllText(RecoveryPath, text, Encoding.UTF8);
                }

                // Удаляем только ту старую копию, которую предложили восстановить.
                if (!string.Equals(recoveryFile, RecoveryPath, StringComparison.OrdinalIgnoreCase))
                    DeleteRecoveryFile(recoveryFile);
                else if (r != DialogResult.Yes)
                    DeleteRecoveryFile(recoveryFile);
            }
            catch
            {
                // Ошибка восстановления не должна мешать запуску редактора.
            }
        }

        private static bool IsAureonProcessRunning(int processId)
        {
            try
            {
                using Process candidate = Process.GetProcessById(processId);
                using Process current = Process.GetCurrentProcess();

                return string.Equals(
                    candidate.ProcessName,
                    current.ProcessName,
                    StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static void DeleteRecoveryFile(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    File.Delete(path);
            }
            catch { }
        }

        // ═════════════ Настройки ═════════════
        private static int ParseIndex(string value, int count)
        {
            return int.TryParse(value, out int i)
                ? Math.Max(0, Math.Min(count - 1, i))
                : 0;
        }

        private void LoadSettings()
        {
            try
            {
                if (!File.Exists(SettingsPath))
                    return;

                int w = 0, h = 0;

                foreach (string line in File.ReadAllLines(SettingsPath))
                {
                    int eq = line.IndexOf('=');
                    if (eq <= 0)
                        continue;

                    string key = line.Substring(0, eq).Trim();
                    string val = line.Substring(eq + 1).Trim();

                    switch (key)
                    {
                        case "preset": presetIndex = ParseIndex(val, presets.Length); break;
                        case "accent": accentIndex = ParseIndex(val, accents.Length); break;
                        case "text": textColorIndex = ParseIndex(val, textColors.Length + 1); break;
                        case "customText":
                            if (int.TryParse(val, out int argb))
                                customTextColor = Color.FromArgb(argb);
                            break;
                        case "font":
                            if (val.Length > 0 && FontFamilyExists(val)) fontName = val;
                            break;
                        case "fontSize":
                            if (float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out float fs) && fs >= 6 && fs <= 72)
                                fontSize = fs;
                            break;
                        case "zoom":
                            if (float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                                zoom = z;
                            break;
                        case "wrap": bool.TryParse(val, out wordWrap); break;
                        case "status": bool.TryParse(val, out statusVisible); break;
                        case "width": int.TryParse(val, out w); break;
                        case "height": int.TryParse(val, out h); break;
                        case "maximized": bool.TryParse(val, out savedMaximized); break;
                    }
                }

                if (w > 0 && h > 0)
                    savedSize = new Size(w, h);
            }
            catch
            {
                // Повреждённый файл — стартуем с настройками по умолчанию.
            }
        }

        private void SaveSettings()
        {
            try
            {
                Directory.CreateDirectory(SettingsDir);

                Rectangle b = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;

                File.WriteAllLines(SettingsPath, new[]
                {
                    "preset=" + presetIndex,
                    "accent=" + accentIndex,
                    "text=" + textColorIndex,
                    "customText=" + (customTextColor.HasValue
                        ? customTextColor.Value.ToArgb().ToString(CultureInfo.InvariantCulture)
                        : ""),
                    "font=" + TextBox.Font.Name,
                    "fontSize=" + TextBox.Font.Size.ToString(CultureInfo.InvariantCulture),
                    "zoom=" + TextBox.ZoomFactor.ToString(CultureInfo.InvariantCulture),
                    "wrap=" + TextBox.WordWrap,
                    "status=" + statusVisible,
                    "width=" + b.Width,
                    "height=" + b.Height,
                    "maximized=" + (WindowState == FormWindowState.Maximized)
                });
            }
            catch
            {
                // Ошибка сохранения настроек не должна закрывать редактор.
            }
        }

        // ═════════════ Перетаскивание файлов ═════════════
        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop)
                ? DragDropEffects.Copy
                : DragDropEffects.None;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files == null || files.Length == 0)
                return;

            string path = files[0];

            // Откладываем, чтобы не держать источник перетаскивания во время диалога.
            BeginInvoke(new Action(() =>
            {
                if (ConfirmUnsavedChanges())
                    LoadFile(path);
            }));
        }

        // ═════════════ Файлы ═════════════
        private static string ReadTextSmart(string path, out Encoding enc)
        {
            byte[] bytes = File.ReadAllBytes(path);

            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            {
                enc = new UTF8Encoding(true);
                return enc.GetString(bytes, 3, bytes.Length - 3);
            }
            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            {
                enc = Encoding.Unicode;
                return enc.GetString(bytes, 2, bytes.Length - 2);
            }
            if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            {
                enc = Encoding.BigEndianUnicode;
                return enc.GetString(bytes, 2, bytes.Length - 2);
            }

            try
            {
                string s = new UTF8Encoding(false, true).GetString(bytes);
                enc = new UTF8Encoding(false);
                return s;
            }
            catch (DecoderFallbackException)
            {
                // Не UTF-8 — вероятно старый файл в кириллической кодировке.
                enc = Encoding.GetEncoding(1251);
                return enc.GetString(bytes);
            }
        }

        private void LoadFile(string path)
        {
            try
            {
                string text = ReadTextSmart(path, out Encoding enc);

                currentFilePath = path;
                currentEncoding = enc;
                SetEditorText(text);
                isTextChanged = false;
                DeleteRecovery();

                UpdateWindowTitle();
                UpdateEncodingLabel();
            }
            catch (Exception ex)
            {
                ShowInfo("Не вдалося відкрити файл", ex.Message);
            }
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ConfirmUnsavedChanges())
                return;

            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Текстові файли|*.txt;*.md;*.log;*.cs;*.json;*.xml;*.ini|Усі файли (*.*)|*.*";
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (dlg.ShowDialog(this) == DialogResult.OK)
                    LoadFile(dlg.FileName);
            }
        }

        private bool Save()
        {
            return string.IsNullOrEmpty(currentFilePath)
                ? SaveAsNewFile()
                : SaveToFile(currentFilePath);
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e) => Save();

        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e) => SaveAsNewFile();

        private void сохранитьВсёToolStripMenuItem_Click(object sender, EventArgs e) => Save();

        private bool SaveToFile(string path)
        {
            try
            {
                // RichTextBox отдаёт переводы строк как \n — приводим к Windows-формату.
                string text = TextBox.Text.Replace("\r\n", "\n").Replace("\n", "\r\n");
                File.WriteAllText(path, text, currentEncoding);

                currentFilePath = path;
                isTextChanged = false;
                DeleteRecovery();
                UpdateWindowTitle();
                return true;
            }
            catch (Exception ex)
            {
                ShowInfo("Не вдалося зберегти файл", ex.Message);
                return false;
            }
        }

        private bool SaveAsNewFile()
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "Текстові файли (*.txt)|*.txt|Усі файли (*.*)|*.*";
                dlg.DefaultExt = "txt";
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (!string.IsNullOrEmpty(currentFilePath))
                    dlg.FileName = Path.GetFileName(currentFilePath);

                return dlg.ShowDialog(this) == DialogResult.OK && SaveToFile(dlg.FileName);
            }
        }

        private void ResetEditor()
        {
            SetEditorText("");
            currentFilePath = null;
            currentEncoding = new UTF8Encoding(false);
            isTextChanged = false;
            DeleteRecovery();
            UpdateWindowTitle();
            UpdateEncodingLabel();
        }

        private void новыйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ConfirmUnsavedChanges())
                return;

            ResetEditor();
        }

        private bool ConfirmUnsavedChanges()
        {
            if (!isTextChanged)
                return true;

            DialogResult result = ShowAureonDialog(
                "Незбережені зміни",
                "У документі є зміни, які ще не збережені.\nБажаєте зберегти їх, перш ніж продовжити?",
                "!", DialogResult.Cancel,
                new DlgButton("Скасувати", DialogResult.Cancel),
                new DlgButton("Не зберігати", DialogResult.No, 2),
                new DlgButton("Зберегти", DialogResult.Yes, 1));

            if (result == DialogResult.Yes)
                return Save();           // false, если сохранение отменено или не удалось

            return result == DialogResult.No;
        }

        private void выйтиToolStripMenuItem_Click(object sender, EventArgs e) => Close();

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ConfirmUnsavedChanges())
                e.Cancel = true;
        }

        // ═════════════ Печать ═════════════
        private void InitializePrinting()
        {
            pageSetupDialog = new PageSetupDialog();
            printDocument = new PrintDocument();
            pageSetupDialog.Document = printDocument;
            pageSettings = printDocument.DefaultPageSettings;

            printDocument.BeginPrint += (s, e) => printCharIndex = 0;
            printDocument.PrintPage += PrintDocument_PrintPage;
        }

        private void параметрыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pageSetupDialog.ShowDialog(this) == DialogResult.OK)
            {
                pageSettings = pageSetupDialog.PageSettings;
                printDocument.DefaultPageSettings = pageSettings;
                printDocument.PrinterSettings = pageSetupDialog.PrinterSettings;
            }
        }

        private void друкToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                printDialog1.Document = printDocument;
                if (printDialog1.ShowDialog(this) == DialogResult.OK)
                    printDocument.Print();
            }
            catch (Exception ex)
            {
                ShowInfo("Помилка друку", ex.Message);
            }
        }

        // Многостраничная печать: каждая страница берёт столько символов, сколько влезает.
        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            string text = TextBox.Text;
            if (printCharIndex >= text.Length)
            {
                e.HasMorePages = false;
                return;
            }

            string rest = text.Substring(printCharIndex);
            using (var fmt = new StringFormat(StringFormatFlags.LineLimit))
            {
                e.Graphics.MeasureString(rest, TextBox.Font, e.MarginBounds.Size, fmt,
                    out int charsFitted, out int linesFilled);

                if (charsFitted <= 0)
                    charsFitted = rest.Length;

                e.Graphics.DrawString(rest.Substring(0, charsFitted), TextBox.Font,
                    Brushes.Black, e.MarginBounds, fmt);

                printCharIndex += charsFitted;
            }

            e.HasMorePages = printCharIndex < text.Length;
        }

        // ═════════════ Редактирование ═════════════
        private void отменитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TextBox.CanUndo) TextBox.Undo();
        }

        private void вырезатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TextBox.SelectionLength > 0) TextBox.Cut();
        }

        private void копироватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TextBox.SelectionLength > 0) TextBox.Copy();
        }

        private void вставитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
                TextBox.Paste(DataFormats.GetFormat(DataFormats.UnicodeText));
        }

        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TextBox.SelectionLength > 0)
                TextBox.SelectedText = string.Empty;
        }

        private void выбратьВсёToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TextBox.SelectAll();
        }

        private void DuplicateLine()
        {
            string t = TextBox.Text;
            int pos = TextBox.SelectionStart;

            int start = pos == 0 ? 0 : t.LastIndexOf('\n', pos - 1) + 1;
            int end = t.IndexOf('\n', pos);
            if (end < 0) end = t.Length;

            string line = t.Substring(start, end - start);

            TextBox.Select(end, 0);
            TextBox.SelectedText = "\n" + line;
            TextBox.Select(pos + line.Length + 1, 0);
        }

        // ═════════════ Поиск и замена ═════════════
        private bool matchCase;

        private StringComparison SearchComparison =>
            matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        private void поискToolStripMenuItem_Click(object sender, EventArgs e) => ShowFindBar(false);

        private void заменитьToolStripMenuItem_Click(object sender, EventArgs e) => ShowFindBar(true);

        private void найтиДалееToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (findBox.Text.Length == 0 && TextBox.SelectionLength > 0)
                findBox.Text = TextBox.SelectedText;

            if (findBox.Text.Length == 0)
            {
                ShowFindBar(false);
                return;
            }

            FindNext(true);
        }

        private void найтиПредыдущееToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (findBox.Text.Length == 0 && TextBox.SelectionLength > 0)
                findBox.Text = TextBox.SelectedText;

            if (findBox.Text.Length == 0)
            {
                ShowFindBar(false);
                return;
            }

            FindNext(false);
        }

        private void SelectMatch(int index, int length)
        {
            TextBox.Select(index, length);
            TextBox.ScrollToCaret();
        }

        private void FindIncremental()
        {
            string q = findBox.Text;

            if (q.Length > 0)
            {
                string t = TextBox.Text;
                int idx = t.IndexOf(q, Math.Min(TextBox.SelectionStart, t.Length), SearchComparison);
                if (idx < 0)
                    idx = t.IndexOf(q, 0, SearchComparison);
                if (idx >= 0)
                    SelectMatch(idx, q.Length);
            }

            UpdateMatchInfo();
        }

        private bool FindNext(bool forward)
        {
            string q = findBox.Text;
            if (q.Length == 0)
                return false;

            string t = TextBox.Text;
            if (t.Length == 0)
            {
                UpdateMatchInfo();
                return false;
            }

            int idx;
            if (forward)
            {
                int start = TextBox.SelectionStart + TextBox.SelectionLength;
                idx = start <= t.Length ? t.IndexOf(q, start, SearchComparison) : -1;
                if (idx < 0)
                    idx = t.IndexOf(q, 0, SearchComparison);        // по кругу
            }
            else
            {
                int from = TextBox.SelectionStart + q.Length - 2;
                idx = (TextBox.SelectionStart > 0 && from >= 0)
                    ? t.LastIndexOf(q, Math.Min(from, t.Length - 1), SearchComparison)
                    : -1;
                if (idx < 0)
                    idx = t.LastIndexOf(q, t.Length - 1, SearchComparison);
            }

            if (idx >= 0)
                SelectMatch(idx, q.Length);

            UpdateMatchInfo();
            return idx >= 0;
        }

        private void UpdateMatchInfo()
        {
            if (findBar == null || !findBar.Visible)
                return;

            string q = findBox.Text;
            if (q.Length == 0)
            {
                matchLabel.Text = "";
                findBox.BackColor = Skin.EditorTint;
                return;
            }

            string t = TextBox.Text;
            int total = 0, current = 0, i = 0;
            int selStart = TextBox.SelectionStart;

            while ((i = t.IndexOf(q, i, SearchComparison)) >= 0)
            {
                total++;
                if (TextBox.SelectionLength > 0 && i <= selStart)
                    current = total;
                i += Math.Max(1, q.Length);
            }

            matchLabel.Text = total == 0
                ? "Немає збігів"
                : (current > 0 ? $"{current} з {total}" : $"Збігів: {total}");

            findBox.BackColor = total == 0
                ? Blend(Skin.EditorTint, Color.Red, 0.25f)
                : Skin.EditorTint;
        }

        private void ReplaceOne()
        {
            string q = findBox.Text;
            if (q.Length == 0)
                return;

            if (TextBox.SelectionLength > 0 &&
                string.Equals(TextBox.SelectedText, q, SearchComparison))
            {
                TextBox.SelectedText = replaceBox.Text;
            }

            FindNext(true);
        }

        private void ReplaceAll()
        {
            string q = findBox.Text;
            if (q.Length == 0)
                return;

            string rep = replaceBox.Text;
            RegexOptions opts = matchCase
                ? RegexOptions.None
                : RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

            int count = 0;
            string result = Regex.Replace(TextBox.Text, Regex.Escape(q),
                m => { count++; return rep; }, opts);

            if (count == 0)
            {
                matchLabel.Text = "Немає збігів";
                return;
            }

            int caret = TextBox.SelectionStart;

            // Через SelectedText, щоб Ctrl+Z міг скасувати заміну.
            TextBox.SelectAll();
            TextBox.SelectedText = result;
            TextBox.Select(Math.Min(caret, TextBox.TextLength), 0);

            matchLabel.Text = $"Замінено: {count}";
        }

        private void перейтиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string t = TextBox.Text;
            int total = 1;
            foreach (char c in t)
                if (c == '\n') total++;

            string input = AureonPrompt("Перейти до рядка", $"Номер рядка (1–{total}):");
            if (input == null || !int.TryParse(input.Trim(), out int n))
                return;

            n = Math.Max(1, Math.Min(n, total));

            int idx = 0;
            for (int l = 1; l < n; l++)
            {
                int nl = t.IndexOf('\n', idx);
                if (nl < 0) break;
                idx = nl + 1;
            }

            TextBox.Select(idx, 0);
            TextBox.ScrollToCaret();
            TextBox.Focus();
        }

        // ═════════════ Шрифт / цвет ═════════════
        private void шрифтToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new FontDialog())
            {
                dlg.Font = TextBox.Font;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    TextBox.Font = dlg.Font;
            }
        }

        private void цветToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChooseCustomTextColor();
        }

        // ═════════════ Масштаб / вид ═════════════
        private void ChangeZoom(float delta)
        {
            float z = (float)Math.Round(TextBox.ZoomFactor + delta, 2);
            TextBox.ZoomFactor = Math.Max(0.5f, Math.Min(5f, z));
            UpdateZoomLabel();
        }

        private void увеличитьToolStripMenuItem_Click(object sender, EventArgs e) => ChangeZoom(0.1f);

        private void уменьшитьToolStripMenuItem_Click(object sender, EventArgs e) => ChangeZoom(-0.1f);

        private void переносПоСловамToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TextBox.WordWrap = !TextBox.WordWrap;
            переносПоСловамToolStripMenuItem.Checked = TextBox.WordWrap;
            UpdateWrapButton();
        }

        private void строкаСостоянияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusVisible = !statusVisible;
            строкаСостоянияToolStripMenuItem.Checked = statusVisible;
            UpdateLayoutRows();
        }

        // ═════════════ Клавиши ═════════════
        // Подписан в Designer: KeyDown += Form1_KeyDown_1
        private void Form1_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                ToggleZenMode();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                if (findBar != null && findBar.Visible)
                {
                    CloseFindBar();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
                else if (isZenMode)
                {
                    ToggleZenMode();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }
        }



    }
}