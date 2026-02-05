using System;
using System.Drawing;
using System.Windows.Forms;
using DotNetEnv;

namespace QuizApp.Forms
{
    /// <summary>
    /// Login form for teacher authentication
    /// </summary>
    public class LoginForm : Form
    {
        private TextBox txtPassword;
        private Button btnLogin;
        
        public LoginForm()
        {
            InitializeComponents();
            AttachEventHandlers();
        }
        
        /// <summary>
        /// Initializes form controls
        /// </summary>
        private void InitializeComponents()
        {
            // Form properties
            Text = "Teacher Login";
            Width = 320;
            Height = 220;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            
            // Password textbox
            txtPassword = new TextBox
            {
                Top = 50,
                Left = 50,
                Width = 200,
                PasswordChar = '*'
            };
            
            // Login button
            btnLogin = new Button
            {
                Text = "Login",
                Top = 90,
                Left = 50,
                Width = 200,
                Height = Constants.ButtonHeight,
                BackColor = Constants.LoginButtonColor
            };
            
            // Label
            var lblPassword = new Label
            {
                Text = "Administrator Password:",
                Top = 25,
                Left = 50,
                Width = 200
            };
            
            // Add controls to form
            Controls.AddRange(new Control[] { lblPassword, txtPassword, btnLogin });
        }
        
        /// <summary>
        /// Attaches event handlers to controls - CRITICAL for button responsiveness
        /// </summary>
        private void AttachEventHandlers()
        {
            // Explicit event handler registration
            btnLogin.Click += BtnLogin_Click;
            txtPassword.KeyPress += TxtPassword_KeyPress;
        }
        
        /// <summary>
        /// Login button click handler
        /// </summary>
        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            string password = txtPassword.Text;
            string? envPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
            
            if (string.IsNullOrWhiteSpace(envPassword))
            {
                MessageBox.Show("Configuration error: Password not found in .env file", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            if (password == envPassword)
            {
                this.Hide();
                var teacherForm = new TeacherForm();
                teacherForm.FormClosed += (s2, e2) => this.Close();
                teacherForm.Show();
            }
            else
            {
                MessageBox.Show("Incorrect Password!", "Security", 
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }
        
        /// <summary>
        /// Enter key support for password field
        /// </summary>
        private void TxtPassword_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char)Keys.Return)
                return;
            
            BtnLogin_Click(sender, EventArgs.Empty);
        }
    }
}
