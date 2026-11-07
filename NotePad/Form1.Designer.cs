// Дизайн проекта
namespace NotePad
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            menuStrip1 = new MenuStrip();
            файлToolStripMenuItem = new ToolStripMenuItem();
            новыйToolStripMenuItem = new ToolStripMenuItem();
            открытьToolStripMenuItem = new ToolStripMenuItem();
            сохранитьToolStripMenuItem = new ToolStripMenuItem();
            сохранитьКакToolStripMenuItem = new ToolStripMenuItem();
            сохранитьВсёToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            параметрыToolStripMenuItem = new ToolStripMenuItem();
            друкToolStripMenuItem = new ToolStripMenuItem();
            выйтиToolStripMenuItem = new ToolStripMenuItem();
            редактироватьToolStripMenuItem = new ToolStripMenuItem();
            отменитьToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            вырезатьToolStripMenuItem = new ToolStripMenuItem();
            копироватьToolStripMenuItem = new ToolStripMenuItem();
            вставитьToolStripMenuItem = new ToolStripMenuItem();
            удалитьToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            поискToolStripMenuItem = new ToolStripMenuItem();
            найтиДалееToolStripMenuItem = new ToolStripMenuItem();
            найтиПредыдущееToolStripMenuItem = new ToolStripMenuItem();
            заменитьToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            перейтиToolStripMenuItem = new ToolStripMenuItem();
            выбратьВсёToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            шрифтToolStripMenuItem = new ToolStripMenuItem();
            цветToolStripMenuItem = new ToolStripMenuItem();
            просмотретьToolStripMenuItem = new ToolStripMenuItem();
            масштабToolStripMenuItem = new ToolStripMenuItem();
            увеличитьToolStripMenuItem = new ToolStripMenuItem();
            уменьшитьToolStripMenuItem = new ToolStripMenuItem();
            строкаСостоянияToolStripMenuItem = new ToolStripMenuItem();
            переносПоСловамToolStripMenuItem = new ToolStripMenuItem();
            TextBox = new RichTextBox();
            openFileDialog1 = new OpenFileDialog();
            saveFileDialog1 = new SaveFileDialog();
            fontDialog1 = new FontDialog();
            colorDialog1 = new ColorDialog();
            pageSetupDialog1 = new PageSetupDialog();
            printDialog1 = new PrintDialog();
            statusStrip1 = new StatusStrip();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.BackColor = Color.FromArgb(24, 24, 37);
            menuStrip1.ForeColor = Color.FromArgb(228, 228, 240);
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { файлToolStripMenuItem, редактироватьToolStripMenuItem, просмотретьToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.RenderMode = ToolStripRenderMode.Professional;
            menuStrip1.Size = new Size(800, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // файлToolStripMenuItem
            // 
            файлToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { новыйToolStripMenuItem, открытьToolStripMenuItem, сохранитьToolStripMenuItem, сохранитьКакToolStripMenuItem, сохранитьВсёToolStripMenuItem, toolStripSeparator1, параметрыToolStripMenuItem, друкToolStripMenuItem, выйтиToolStripMenuItem });
            файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            файлToolStripMenuItem.Size = new Size(48, 20);
            файлToolStripMenuItem.Text = "Файл";
            // 
            // новыйToolStripMenuItem
            // 
            новыйToolStripMenuItem.Name = "новыйToolStripMenuItem";
            новыйToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.N;
            новыйToolStripMenuItem.Size = new Size(217, 22);
            новыйToolStripMenuItem.Text = "Новий файл";
            новыйToolStripMenuItem.Click += новыйToolStripMenuItem_Click;
            // 
            // открытьToolStripMenuItem
            // 
            открытьToolStripMenuItem.Name = "открытьToolStripMenuItem";
            открытьToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.O;
            открытьToolStripMenuItem.Size = new Size(217, 22);
            открытьToolStripMenuItem.Text = "Відкрити";
            открытьToolStripMenuItem.Click += открытьToolStripMenuItem_Click;
            // 
            // сохранитьToolStripMenuItem
            // 
            сохранитьToolStripMenuItem.Name = "сохранитьToolStripMenuItem";
            сохранитьToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S;
            сохранитьToolStripMenuItem.Size = new Size(217, 22);
            сохранитьToolStripMenuItem.Text = "Зберегти";
            сохранитьToolStripMenuItem.Click += сохранитьToolStripMenuItem_Click;
            // 
            // сохранитьКакToolStripMenuItem
            // 
            сохранитьКакToolStripMenuItem.Name = "сохранитьКакToolStripMenuItem";
            сохранитьКакToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
            сохранитьКакToolStripMenuItem.Size = new Size(217, 22);
            сохранитьКакToolStripMenuItem.Text = "Зберегти як";
            сохранитьКакToolStripMenuItem.Click += сохранитьКакToolStripMenuItem_Click;
            // 
            // сохранитьВсёToolStripMenuItem
            // 
            сохранитьВсёToolStripMenuItem.Name = "сохранитьВсёToolStripMenuItem";
            сохранитьВсёToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Alt | Keys.S;
            сохранитьВсёToolStripMenuItem.Size = new Size(217, 22);
            сохранитьВсёToolStripMenuItem.Text = "Зберегти все";
            сохранитьВсёToolStripMenuItem.Click += сохранитьВсёToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(214, 6);
            // 
            // параметрыToolStripMenuItem
            // 
            параметрыToolStripMenuItem.Name = "параметрыToolStripMenuItem";
            параметрыToolStripMenuItem.Size = new Size(217, 22);
            параметрыToolStripMenuItem.Text = "Параметри сторінки";
            параметрыToolStripMenuItem.Click += параметрыToolStripMenuItem_Click;
            // 
            // друкToolStripMenuItem
            // 
            друкToolStripMenuItem.Name = "друкToolStripMenuItem";
            друкToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.P;
            друкToolStripMenuItem.Size = new Size(217, 22);
            друкToolStripMenuItem.Text = "Друк";
            друкToolStripMenuItem.Click += друкToolStripMenuItem_Click;
            // 
            // выйтиToolStripMenuItem
            // 
            выйтиToolStripMenuItem.Name = "выйтиToolStripMenuItem";
            выйтиToolStripMenuItem.Size = new Size(217, 22);
            выйтиToolStripMenuItem.Text = "Вийти";
            выйтиToolStripMenuItem.Click += выйтиToolStripMenuItem_Click;
            // 
            // редактироватьToolStripMenuItem
            // 
            редактироватьToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { отменитьToolStripMenuItem, toolStripSeparator5, вырезатьToolStripMenuItem, копироватьToolStripMenuItem, вставитьToolStripMenuItem, удалитьToolStripMenuItem, toolStripSeparator4, поискToolStripMenuItem, найтиДалееToolStripMenuItem, найтиПредыдущееToolStripMenuItem, заменитьToolStripMenuItem, toolStripSeparator3, перейтиToolStripMenuItem, выбратьВсёToolStripMenuItem, toolStripSeparator2, шрифтToolStripMenuItem, цветToolStripMenuItem });
            редактироватьToolStripMenuItem.Name = "редактироватьToolStripMenuItem";
            редактироватьToolStripMenuItem.Size = new Size(79, 20);
            редактироватьToolStripMenuItem.Text = "Редагувати";
            // 
            // отменитьToolStripMenuItem
            // 
            отменитьToolStripMenuItem.Name = "отменитьToolStripMenuItem";
            отменитьToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Z;
            отменитьToolStripMenuItem.Size = new Size(226, 22);
            отменитьToolStripMenuItem.Text = "Скасувати";
            отменитьToolStripMenuItem.Click += отменитьToolStripMenuItem_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(223, 6);
            // 
            // вырезатьToolStripMenuItem
            // 
            вырезатьToolStripMenuItem.Name = "вырезатьToolStripMenuItem";
            вырезатьToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.X;
            вырезатьToolStripMenuItem.Size = new Size(226, 22);
            вырезатьToolStripMenuItem.Text = "Обрізати";
            вырезатьToolStripMenuItem.Click += вырезатьToolStripMenuItem_Click;
            // 
            // копироватьToolStripMenuItem
            // 
            копироватьToolStripMenuItem.Name = "копироватьToolStripMenuItem";
            копироватьToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.C;
            копироватьToolStripMenuItem.Size = new Size(226, 22);
            копироватьToolStripMenuItem.Text = "Копіювати";
            копироватьToolStripMenuItem.Click += копироватьToolStripMenuItem_Click;
            // 
            // вставитьToolStripMenuItem
            // 
            вставитьToolStripMenuItem.Name = "вставитьToolStripMenuItem";
            вставитьToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.V;
            вставитьToolStripMenuItem.Size = new Size(226, 22);
            вставитьToolStripMenuItem.Text = "Вставити";
            вставитьToolStripMenuItem.Click += вставитьToolStripMenuItem_Click;
            // 
            // удалитьToolStripMenuItem
            // 
            удалитьToolStripMenuItem.Name = "удалитьToolStripMenuItem";
            удалитьToolStripMenuItem.ShortcutKeys = Keys.Delete;
            удалитьToolStripMenuItem.Size = new Size(226, 22);
            удалитьToolStripMenuItem.Text = "Видалити";
            удалитьToolStripMenuItem.Click += удалитьToolStripMenuItem_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(223, 6);
            // 
            // поискToolStripMenuItem
            // 
            поискToolStripMenuItem.Name = "поискToolStripMenuItem";
            поискToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.F;
            поискToolStripMenuItem.Size = new Size(226, 22);
            поискToolStripMenuItem.Text = "Пошук";
            поискToolStripMenuItem.Click += поискToolStripMenuItem_Click;
            // 
            // найтиДалееToolStripMenuItem
            // 
            найтиДалееToolStripMenuItem.Name = "найтиДалееToolStripMenuItem";
            найтиДалееToolStripMenuItem.ShortcutKeys = Keys.F3;
            найтиДалееToolStripMenuItem.Size = new Size(226, 22);
            найтиДалееToolStripMenuItem.Text = "Знайти";
            найтиДалееToolStripMenuItem.Click += найтиДалееToolStripMenuItem_Click;
            // 
            // найтиПредыдущееToolStripMenuItem
            // 
            найтиПредыдущееToolStripMenuItem.Name = "найтиПредыдущееToolStripMenuItem";
            найтиПредыдущееToolStripMenuItem.ShortcutKeys = Keys.Shift | Keys.F3;
            найтиПредыдущееToolStripMenuItem.Size = new Size(226, 22);
            найтиПредыдущееToolStripMenuItem.Text = "Знайти попереднє";
            найтиПредыдущееToolStripMenuItem.Click += найтиПредыдущееToolStripMenuItem_Click;
            // 
            // заменитьToolStripMenuItem
            // 
            заменитьToolStripMenuItem.Name = "заменитьToolStripMenuItem";
            заменитьToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.H;
            заменитьToolStripMenuItem.Size = new Size(226, 22);
            заменитьToolStripMenuItem.Text = "Замінити";
            заменитьToolStripMenuItem.Click += заменитьToolStripMenuItem_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(223, 6);
            // 
            // перейтиToolStripMenuItem
            // 
            перейтиToolStripMenuItem.Name = "перейтиToolStripMenuItem";
            перейтиToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.G;
            перейтиToolStripMenuItem.Size = new Size(226, 22);
            перейтиToolStripMenuItem.Text = "Перейти";
            перейтиToolStripMenuItem.Click += перейтиToolStripMenuItem_Click;
            // 
            // выбратьВсёToolStripMenuItem
            // 
            выбратьВсёToolStripMenuItem.Name = "выбратьВсёToolStripMenuItem";
            выбратьВсёToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.A;
            выбратьВсёToolStripMenuItem.Size = new Size(226, 22);
            выбратьВсёToolStripMenuItem.Text = "Вибрати все";
            выбратьВсёToolStripMenuItem.Click += выбратьВсёToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(223, 6);
            // 
            // шрифтToolStripMenuItem
            // 
            шрифтToolStripMenuItem.Name = "шрифтToolStripMenuItem";
            шрифтToolStripMenuItem.Size = new Size(226, 22);
            шрифтToolStripMenuItem.Text = "Шрифт";
            шрифтToolStripMenuItem.Click += шрифтToolStripMenuItem_Click;
            // 
            // цветToolStripMenuItem
            // 
            цветToolStripMenuItem.Name = "цветToolStripMenuItem";
            цветToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.E;
            цветToolStripMenuItem.Size = new Size(226, 22);
            цветToolStripMenuItem.Text = "Колір";
            цветToolStripMenuItem.Click += цветToolStripMenuItem_Click;
            // 
            // просмотретьToolStripMenuItem
            // 
            просмотретьToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { масштабToolStripMenuItem, строкаСостоянияToolStripMenuItem, переносПоСловамToolStripMenuItem });
            просмотретьToolStripMenuItem.Name = "просмотретьToolStripMenuItem";
            просмотретьToolStripMenuItem.Size = new Size(90, 20);
            просмотретьToolStripMenuItem.Text = "Переглянути";
            // 
            // масштабToolStripMenuItem
            // 
            масштабToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { увеличитьToolStripMenuItem, уменьшитьToolStripMenuItem });
            масштабToolStripMenuItem.Name = "масштабToolStripMenuItem";
            масштабToolStripMenuItem.Size = new Size(212, 22);
            масштабToolStripMenuItem.Text = "Масштаб";
            // 
            // увеличитьToolStripMenuItem
            // 
            увеличитьToolStripMenuItem.Name = "увеличитьToolStripMenuItem";
            увеличитьToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.N;
            увеличитьToolStripMenuItem.Size = new Size(178, 22);
            увеличитьToolStripMenuItem.Text = "Збільшити";
            увеличитьToolStripMenuItem.Click += увеличитьToolStripMenuItem_Click;
            // 
            // уменьшитьToolStripMenuItem
            // 
            уменьшитьToolStripMenuItem.Name = "уменьшитьToolStripMenuItem";
            уменьшитьToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.M;
            уменьшитьToolStripMenuItem.Size = new Size(178, 22);
            уменьшитьToolStripMenuItem.Text = "Зменшити";
            уменьшитьToolStripMenuItem.Click += уменьшитьToolStripMenuItem_Click;
            // 
            // строкаСостоянияToolStripMenuItem
            // 
            строкаСостоянияToolStripMenuItem.Name = "строкаСостоянияToolStripMenuItem";
            строкаСостоянияToolStripMenuItem.Size = new Size(212, 22);
            строкаСостоянияToolStripMenuItem.Text = "Рядок стану";
            строкаСостоянияToolStripMenuItem.Click += строкаСостоянияToolStripMenuItem_Click;
            // 
            // переносПоСловамToolStripMenuItem
            // 
            переносПоСловамToolStripMenuItem.Name = "переносПоСловамToolStripMenuItem";
            переносПоСловамToolStripMenuItem.Size = new Size(212, 22);
            переносПоСловамToolStripMenuItem.Text = "Перенесення за словами";
            переносПоСловамToolStripMenuItem.Click += переносПоСловамToolStripMenuItem_Click;
            // 
            // TextBox
            // 
            TextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            TextBox.BackColor = Color.FromArgb(37, 37, 54);
            TextBox.BorderStyle = BorderStyle.None;
            TextBox.Font = new Font("Cascadia Code", 13F);
            TextBox.ForeColor = Color.FromArgb(228, 228, 240);
            TextBox.Location = new Point(0, 27);
            TextBox.Name = "TextBox";
            TextBox.Size = new Size(800, 397);
            TextBox.TabIndex = 1;
            TextBox.Text = "";
            TextBox.TextChanged += TextBox_TextChanged;
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            // 
            // printDialog1
            // 
            printDialog1.UseEXDialog = true;
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = Color.FromArgb(24, 24, 37);
            statusStrip1.ForeColor = Color.FromArgb(166, 166, 192);
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Location = new Point(0, 428);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(800, 22);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 30, 46);
            ClientSize = new Size(800, 450);
            Controls.Add(statusStrip1);
            Controls.Add(TextBox);
            Controls.Add(menuStrip1);
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            Name = "Form1";
            Text = "NotePad Pro";
            FormClosing += Form1_FormClosing;
            KeyDown += Form1_KeyDown_1;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem файлToolStripMenuItem;
        private ToolStripMenuItem редактироватьToolStripMenuItem;
        private ToolStripMenuItem просмотретьToolStripMenuItem;
        private RichTextBox TextBox;
        private ToolStripMenuItem открытьToolStripMenuItem;
        private ToolStripMenuItem сохранитьToolStripMenuItem;
        private ToolStripMenuItem сохранитьКакToolStripMenuItem;
        private ToolStripMenuItem сохранитьВсёToolStripMenuItem;
        private ToolStripMenuItem параметрыToolStripMenuItem;
        private ToolStripMenuItem друкToolStripMenuItem;
        private ToolStripMenuItem выйтиToolStripMenuItem;
        private ToolStripMenuItem отменитьToolStripMenuItem;
        private ToolStripMenuItem вырезатьToolStripMenuItem;
        private ToolStripMenuItem копироватьToolStripMenuItem;
        private ToolStripMenuItem вставитьToolStripMenuItem;
        private ToolStripMenuItem удалитьToolStripMenuItem;
        private ToolStripMenuItem поискToolStripMenuItem;
        private ToolStripMenuItem найтиДалееToolStripMenuItem;
        private ToolStripMenuItem найтиПредыдущееToolStripMenuItem;
        private ToolStripMenuItem заменитьToolStripMenuItem;
        private ToolStripMenuItem перейтиToolStripMenuItem;
        private ToolStripMenuItem выбратьВсёToolStripMenuItem;
        private ToolStripMenuItem шрифтToolStripMenuItem;
        private ToolStripMenuItem масштабToolStripMenuItem;
        private ToolStripMenuItem увеличитьToolStripMenuItem;
        private ToolStripMenuItem уменьшитьToolStripMenuItem;
        private ToolStripMenuItem строкаСостоянияToolStripMenuItem;
        private ToolStripMenuItem переносПоСловамToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem цветToolStripMenuItem;
        private OpenFileDialog openFileDialog1;
        private SaveFileDialog saveFileDialog1;
        private FontDialog fontDialog1;
        private ColorDialog colorDialog1;
        private PageSetupDialog pageSetupDialog1;
        private PrintDialog printDialog1;
        private StatusStrip statusStrip1;
        private ToolStripMenuItem новыйToolStripMenuItem;
    }
}