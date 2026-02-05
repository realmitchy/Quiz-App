using System;
using System.IO;
using System.Windows.Forms;

namespace QuizApp.Services
{
    /// <summary>
    /// Service for centralized file operations
    /// </summary>
    public static class FileService
    {
        /// <summary>
        /// Appends quiz result to CSV file
        /// </summary>
        /// <param name="studentName">Student name</param>
        /// <param name="studentClass">Student class</param>
        /// <param name="score">Score achieved</param>
        /// <param name="totalQuestions">Total questions</param>
        public static void SaveResult(string studentName, string studentClass, int score, int totalQuestions)
        {
            try
            {
                string result = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)},{studentName},{studentClass},{score}/{totalQuestions}{Environment.NewLine}";
                File.AppendAllText(Constants.ResultsFileName, result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save results: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
