using System;
using AutoGestPro.src.UI.Windows;
using Gtk;

namespace AutoGestPro.src
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // Manejo global de excepciones
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Console.WriteLine($"Unhandled Exception: {e.ExceptionObject}");
            };

            try
            {
                Application.Init();

                var app = new Application("org.AutoGestPro.AutoGestPro", GLib.ApplicationFlags.None);
                app.Register(GLib.Cancellable.Current);

                var win = new MainWindow();
                app.AddWindow(win);

                win.Show();
                Application.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal Error: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                Console.WriteLine("Application Exited.");
            }
        }
    }
}
