using System;
using System.Drawing;
using System.Windows.Forms;
using QuizApp.Models;

namespace QuizApp.Forms
{
    /// <summary>
    /// Student information registration form
    /// </summary>
    public class StudentInfoForm : Form
    {
        private TextBox txtName, txtClass;
        private Button btnStartExam;
        private Label lblQuizInfo;
        private QuizSet quizSet;
        
        public StudentInfoForm(QuizSet selectedQuizSet)
        {
            quizSet = selectedQuizSet;
            InitializeComponents();
            AttachEventHandlers();
        }
        
        /// <summary>
        /// Initializes form controls
        /// </summary>
        private void InitializeComponents()
        {
            // Form properties
            Text = "Student Registration";
            Width = 400;
            Height = 320;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            
            // Quiz info label
            lblQuizInfo = new Label
            {
                Text = $"Quiz: {quizSet.Title}\nQuestions: {quizSet.Questions.Count}\nTime Limit: {quizSet.TimeLimitMinutes} minutes",
                Top = Constants.StandardSpacing,
                Left = Constants.StandardSpacing,
                Width = 350,
                Height = 60,
                Font = new Font(Constants.DefaultFontFamily, Constants.DefaultFontSize, FontStyle.Bold)
            };
            
            // Name label and textbox
            var lblName = new Label
            {
                Text = "Full Name:",
                Top = 90,
                Left = Constants.StandardSpacing
            };
            
            txtName = new TextBox
            {
                Top = 110,
                Left = Constants.StandardSpacing,
                Width = 340
            };
            
            // Class label and textbox
            var lblClass = new Label
            {
                Text = "Class/Grade:",
                Top = 150,
                Left = Constants.StandardSpacing
            };
            
            txtClass = new TextBox
            {
                Top = 170,
                Left = Constants.StandardSpacing,
                Width = 340
            };
            
            // Start exam button
            btnStartExam = new Button
            {
                Text = "Start Exam",
                Top = 220,
                Left = Constants.StandardSpacing,
                Width = 340,
                Height = 40,
                BackColor = Constants.StartButtonColor,
                Font = new Font(Constants.DefaultFontFamily, Constants.DefaultFontSize, FontStyle.Bold)
            };
            
            // Add controls
            Controls.AddRange(new Control[] 
            {
                lblQuizInfo, lblName, txtName, lblClass, txtClass, btnStartExam
            });
        }
        
        /// <summary>
        /// Attaches event handlers - CRITICAL for button responsiveness
        /// </summary>
        private void AttachEventHandlers()
        {
            btnStartExam.Click += BtnStartExam_Click;
            txtName.KeyPress += TxtName_KeyPress;
            txtClass.KeyPress += TxtClass_KeyPress;
        }
        
        /// <summary>
        /// Start exam button click handler
        /// </summary>
        private void BtnStartExam_Click(object? sender, EventArgs e)
        {
            // Validate name
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter your name.", "Required Field", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }
            
            // Start the quiz
            this.Hide();
            var quizForm = new QuizForm(quizSet, txtName.Text, txtClass.Text);
            quizForm.FormClosed += (s2, e2) => this.Close();
            quizForm.Show();
        }
        
        /// <summary>
        /// Enter key support for name field
        /// </summary>
        private void TxtName_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char)Keys.Return)
                return;
            
            txtClass.Focus();
        }
        
        /// <summary>
        /// Enter key support for class field
        /// </summary>
        private void TxtClass_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char)Keys.Return)
                return;
            
            BtnStartExam_Click(sender, EventArgs.Empty);
        }
    }
}
