using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ToeicAudioHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Log("OnStartup: begin");
            base.OnStartup(e);
            Log("OnStartup: after base");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log("OnExit");
            base.OnExit(e);
        }

        private static void Log(string msg)
        {
            try
            {
                var dir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UiKitDebug");
                if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
                var path = System.IO.Path.Combine(dir, "toaud.log");
                System.IO.File.AppendAllText(path, DateTime.Now.ToString("HH:mm:ss.fff ") + msg + Environment.NewLine);
            }
            catch { }
        }
    }
}
