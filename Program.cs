using System;
using System.Windows.Forms;
using DotNetEnv;
using QuizApp.Forms;

namespace QuizApp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Load environment variables once at startup
            if (System.IO.File.Exists(".env"))
            {
                Env.Load();
            }
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }
    }
}