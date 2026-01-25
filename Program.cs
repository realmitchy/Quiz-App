using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using System.Linq;
using System.Media;
using System.Drawing;

namespace QuizApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }
    }

    public class QuizData
    {
        public int TimeLimitMinutes { get; set; } = 5;
        public List<Question> Questions { get; set; } = new List<Question>();
    }

    public class Question
    {
        public required string Text { get; set; }
        public required string[] Options { get; set; }
        public int CorrectIndex { get; set; }
        [System.Text.Json.Serialization.JsonIgnore] 
        public List<int>? ShuffledOrder { get; set; } 
    }

    // ================= 1. LOGIN FORM =================
    public class LoginForm : Form
    {
        private TextBox txtPassword;
        private Button btnLogin;
        public LoginForm()
        {
            Text = "Teacher Login"; Width = 320; Height = 220; StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
            txtPassword = new TextBox { Top = 50, Left = 50, Width = 200, PasswordChar = '*' };
            btnLogin = new Button { Text = "Login", Top = 90, Left = 50, Width = 200, Height = 35, BackColor = Color.AliceBlue };
            btnLogin.Click += (s, e) => {
                if (txtPassword.Text == "1234") { 
                    this.Hide(); 
                    var tf = new TeacherForm(); 
                    tf.FormClosed += (s2, e2) => this.Close(); 
                    tf.Show(); 
                }
                else MessageBox.Show("Incorrect Password!", "Security", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            };
            Controls.AddRange(new Control[] { new Label { Text = "Administrator Password:", Top = 25, Left = 50, Width = 200 }, txtPassword, btnLogin });
        }
    }

    // ================= 2. TEACHER PANEL =================
    public class TeacherForm : Form
    {
        private TextBox txtQuestion, optA, optB, optC, optD;
        private NumericUpDown numTime;
        private ComboBox cmbCorrect;
        private Button btnAdd, btnSave, btnLoad, btnStart;
        public static QuizData Data = new QuizData();

        public TeacherForm()
        {
            Text = "Teacher Dashboard"; Width = 520; Height = 620; StartPosition = FormStartPosition.CenterScreen;
            txtQuestion = new TextBox { Top = 30, Left = 20, Width = 460 };
            optA = new TextBox { Top = 75, Left = 20, Width = 460 };
            optB = new TextBox { Top = 120, Left = 20, Width = 460 };
            optC = new TextBox { Top = 165, Left = 20, Width = 460 };
            optD = new TextBox { Top = 210, Left = 20, Width = 460 };
            cmbCorrect = new ComboBox { Top = 255, Left = 20, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCorrect.Items.AddRange(new[] { "Option A", "Option B", "Option C", "Option D" }); cmbCorrect.SelectedIndex = 0;
            numTime = new NumericUpDown { Top = 305, Left = 20, Minimum = 1, Maximum = 300, Value = 5 };
            
            btnAdd = new Button { Text = "Add Question", Top = 345, Left = 20, Width = 140 };
            btnSave = new Button { Text = "Save to File", Top = 345, Left = 180, Width = 140 };
            btnLoad = new Button { Text = "Load from File", Top = 345, Left = 340, Width = 140 };
            btnStart = new Button { Text = "LAUNCH EXAM MODE", Top = 400, Left = 20, Width = 460, Height = 60, BackColor = Color.PaleGreen, Font = new Font(Font, FontStyle.Bold) };

            btnAdd.Click += (s, e) => {
                if(string.IsNullOrWhiteSpace(txtQuestion.Text)) return;
                Data.Questions.Add(new Question { 
                    Text = txtQuestion.Text, 
                    Options = new[] { optA.Text, optB.Text, optC.Text, optD.Text }, 
                    CorrectIndex = cmbCorrect.SelectedIndex 
                });
                txtQuestion.Clear(); optA.Clear(); optB.Clear(); optC.Clear(); optD.Clear();
                MessageBox.Show("Question Added.");
            };
            btnSave.Click += (s, e) => { 
                Data.TimeLimitMinutes = (int)numTime.Value; 
                File.WriteAllText("quiz_data.json", JsonSerializer.Serialize(Data)); 
                MessageBox.Show("Data Saved!"); 
            };
            btnLoad.Click += (s, e) => { 
                if (File.Exists("quiz_data.json")) { 
                    Data = JsonSerializer.Deserialize<QuizData>(File.ReadAllText("quiz_data.json")) ?? new QuizData();
                    numTime.Value = Data.TimeLimitMinutes;
                    MessageBox.Show("Data Loaded!"); 
                } 
            };
            btnStart.Click += (s, e) => { if (Data.Questions.Count > 0) { this.Hide(); new StudentInfoForm(Data).Show(); } };
            
            Controls.AddRange(new Control[] { 
                new Label{Text="Question:",Top=10,Left=20}, txtQuestion, 
                new Label{Text="A:",Top=55,Left=20}, optA, new Label{Text="B:",Top=100,Left=20}, optB,
                new Label{Text="C:",Top=145,Left=20}, optC, new Label{Text="D:",Top=190,Left=20}, optD,
                new Label{Text="Correct Answer:",Top=235,Left=20}, cmbCorrect,
                new Label{Text="Time (Mins):",Top=285,Left=20}, numTime, btnAdd, btnSave, btnLoad, btnStart 
            });
        }
    }

    // ================= 3. STUDENT INFO =================
    public class StudentInfoForm : Form
    {
        private TextBox txtName, txtClass;
        public StudentInfoForm(QuizData d)
        {
            Text = "Student Registration"; Width = 350; Height = 250; StartPosition = FormStartPosition.CenterScreen;
            this.FormClosed += (s, e) => Application.Exit();
            txtName = new TextBox { Top = 40, Left = 20, Width = 280 };
            txtClass = new TextBox { Top = 95, Left = 20, Width = 280 };
            var btn = new Button { Text = "Start Exam", Top = 140, Left = 20, Width = 280, Height = 40, BackColor = Color.LightBlue };
            btn.Click += (s, e) => { 
                if (!string.IsNullOrWhiteSpace(txtName.Text)) { 
                    QuizForm.StudentName = txtName.Text; 
                    QuizForm.StudentClass = txtClass.Text; 
                    this.Hide(); 
                    new QuizForm(d).Show(); 
                } 
            };
            Controls.AddRange(new Control[] { new Label{Text="Full Name:",Top=20,Left=20}, txtName, new Label{Text="Class/Grade:",Top=75,Left=20}, txtClass, btn });
        }
    }

    // ================= 4. THE MASTER QUIZ ENGINE =================
    public class QuizForm : Form
    {
        public static string StudentName = "";
        public static string StudentClass = "";
        private Label lblQuestion, lblTimer; 
        private ProgressBar prgTime; 
        private RadioButton[] options;
        private Button btnNext, btnPrev, btnSubmit; 
        private CheckBox chkFlag;
        private FlowLayoutPanel navPanel;
        private System.Windows.Forms.Timer quizTimer; 
        private int index = 0, timeRemaining;
        private List<Question> questions;
        private int[] studentAnswers; 
        private bool[] flagged;
        private Button[] navButtons;

        public QuizForm(QuizData d)
        {
            questions = d.Questions.OrderBy(x => Guid.NewGuid()).ToList();
            studentAnswers = Enumerable.Repeat(-1, questions.Count).ToArray();
            flagged = new bool[questions.Count];
            timeRemaining = d.TimeLimitMinutes * 60;

            Text = "Exam in Progress: " + StudentName; Width = 900; Height = 580; StartPosition = FormStartPosition.CenterScreen;
            this.FormClosed += (s, e) => Application.Exit();

            navPanel = new FlowLayoutPanel { Top = 10, Left = 650, Width = 220, Height = 450, AutoScroll = true, BackColor = Color.FromArgb(240,240,240) };
            navButtons = new Button[questions.Count];
            for (int i = 0; i < questions.Count; i++) {
                int cap = i;
                navButtons[i] = new Button { Text = (i + 1).ToString(), Width = 45, Height = 45, BackColor = Color.White };
                navButtons[i].Click += (s, e) => { SaveState(); index = cap; ShowQuestion(); };
                navPanel.Controls.Add(navButtons[i]);
            }

            prgTime = new ProgressBar { Top = 10, Left = 20, Width = 610, Height = 15, Maximum = timeRemaining, Value = timeRemaining };
            lblQuestion = new Label { Top = 50, Left = 20, Width = 600, Height = 90, Font = new Font("Arial", 12, FontStyle.Bold) };
            lblTimer = new Label { Top = 10, Left = 560, Width = 70, ForeColor = Color.Red, Font = new Font("Arial", 10, FontStyle.Bold) };

            options = new RadioButton[4];
            for (int i = 0; i < 4; i++) {
                options[i] = new RadioButton { Top = 160 + i * 50, Left = 40, Width = 580, Font = new Font("Arial", 11) };
                Controls.Add(options[i]);
            }

            chkFlag = new CheckBox { Text = "Flag for Review", Top = 370, Left = 40, Width = 150 };
            chkFlag.CheckedChanged += (s, e) => { flagged[index] = chkFlag.Checked; UpdateNavColor(); };

            btnPrev = new Button { Text = "← Back", Top = 420, Left = 40, Width = 110, Height = 45 };
            btnNext = new Button { Text = "Next →", Top = 420, Left = 160, Width = 110, Height = 45 };
            btnSubmit = new Button { Text = "FINISH EXAM", Top = 420, Left = 460, Width = 150, Height = 45, BackColor = Color.Crimson, ForeColor = Color.White };

            btnPrev.Click += (s, e) => { SaveState(); if (index > 0) { index--; ShowQuestion(); } };
            btnNext.Click += (s, e) => { SaveState(); if (index < questions.Count - 1) { index++; ShowQuestion(); } };
            btnSubmit.Click += (s, e) => { if (MessageBox.Show("Finalize and Submit?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes) FinishQuiz(); };

            quizTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            quizTimer.Tick += (s, e) => {
                timeRemaining--;
                if (timeRemaining >= 0) { prgTime.Value = timeRemaining; lblTimer.Text = $"{timeRemaining / 60:D2}:{timeRemaining % 60:D2}"; }
                else FinishQuiz();
            };

            foreach(var q in questions) q.ShuffledOrder = Enumerable.Range(0, 4).OrderBy(x => Guid.NewGuid()).ToList();
            
            Controls.AddRange(new Control[] { prgTime, lblQuestion, lblTimer, btnPrev, btnNext, btnSubmit, navPanel, chkFlag });
            quizTimer.Start(); ShowQuestion();
        }

        void ShowQuestion()
        {
            var q = questions[index];
            lblQuestion.Text = $"Question {index + 1} of {questions.Count}:\n{q.Text}";
            if (q.ShuffledOrder != null) {
                for (int i = 0; i < 4; i++) {
                    options[i].Text = q.Options[q.ShuffledOrder[i]];
                    options[i].Checked = (studentAnswers[index] == i);
                }
            }
            chkFlag.Checked = flagged[index];
            UpdateNavColor();
        }

        void SaveState()
        {
            studentAnswers[index] = -1;
            for (int i = 0; i < 4; i++) if (options[i].Checked) studentAnswers[index] = i;
            UpdateNavColor();
        }

        void UpdateNavColor()
        {
            for (int i = 0; i < navButtons.Length; i++)
            {
                if (flagged[i]) navButtons[i].BackColor = Color.Yellow;
                else if (studentAnswers[i] != -1) navButtons[i].BackColor = Color.LightGreen;
                else navButtons[i].BackColor = Color.White;

                if (i == index) 
                {
                    navButtons[i].FlatStyle = FlatStyle.Flat;
                    navButtons[i].FlatAppearance.BorderSize = 3;
                    navButtons[i].FlatAppearance.BorderColor = Color.Blue;
                }
                else 
                {
                    navButtons[i].FlatStyle = FlatStyle.Standard;
                    navButtons[i].FlatAppearance.BorderSize = 1;
                }
            }
        }

        void FinishQuiz()
        {
            quizTimer.Stop(); SaveState();
            int score = 0;
            for (int i = 0; i < questions.Count; i++) {
                if (studentAnswers[i] != -1 && questions[i].ShuffledOrder != null) {
                    if (questions[i].ShuffledOrder![studentAnswers[i]] == questions[i].CorrectIndex) score++;
                }
            }
            MessageBox.Show($"Test Completed!\nStudent: {StudentName}\nScore: {score}/{questions.Count}");
            try { File.AppendAllText("results.csv", $"{DateTime.Now},{StudentName},{StudentClass},{score}/{questions.Count}\n"); } catch { }
            Application.Exit();
        }
    }
}