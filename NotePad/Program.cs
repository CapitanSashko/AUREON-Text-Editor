using System;
using System.Windows.Forms;

namespace NotePad
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Каждый запуск создаёт отдельное окно.
            // Путь к файлу Form1 получает самостоятельно.
            Application.Run(new Form1());
        }
    }
}