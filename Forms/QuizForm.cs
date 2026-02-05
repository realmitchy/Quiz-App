using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuizApp.Models;
using QuizApp.Services;

namespace QuizApp.Forms
{
    /// <summary>
    /// Quiz form for taking exams
    /// </summary>
    public class QuizForm : Form
    {
        private string studentName;
        private string studentClass;
        private Label lblQuestion, lblTimer, lblTotalTime;
        private ProgressBar prgTime;
        private RadioButton[] options;
        private Button btnNext, btnPrev, btnSubmit;
        private CheckBox chkFlag;
        private FlowLayoutPanel navPanel;
        private System.Windows.Forms.Timer? quizTimer;
        private int currentIndex = 0;
        private int timeRemaining;
        private List<Question> questions;
        private int[] studentAnswers;
        private bool[] flagged;
        private Button[] navButtons;
        private QuizSet quizSet;
        
        public QuizForm(QuizSet selectedQuizSet, string name, string classGrade)
        {
            studentName = name;
            studentClass = classGrade ?? string.Empty;
            quizSet = selectedQuizSet;
            
            // Shuffle questions
            questions = quizSet.Questions.OrderBy(x => Guid.NewGuid()).ToList();
            studentAnswers = Enumerable.Repeat(-1, questions.Count).ToArray();
            flagged = new bool[questions.Count];
            timeRemaining = quizSet.TimeLimitMinutes * 60;
            
            // Shuffle options for each question
            foreach (var q in questions)
            {
                q.ShuffledOrder = Enumerable.Range(0, 4).OrderBy(x => Guid.NewGuid()).ToList();
            }
            
            InitializeComponents();
            AttachEventHandlers();
            StartQuiz();
        }
        
        /// <summary>
        /// Initializes form controls
        /// </summary>
        private void InitializeComponents()
        {
            // Form properties
            Text = $"Exam in Progress: {studentName}";
            Width = 900;
            Height = 580;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            
            // Total time label (new requirement)
            lblTotalTime = new Label
            {
                Text = $"Total Time: {quizSet.TimeLimitMinutes} minutes ({quizSet.TimeLimitMinutes * 60} seconds)",
                Top = 30,
                Left = Constants.StandardSpacing,
                Width = 610,
                Font = new Font(Constants.DefaultFontFamily, Constants.DefaultFontSize, FontStyle.Italic),
                ForeColor = Color.DarkBlue
            };
            
            // Progress bar
            prgTime = new ProgressBar
            {
                Top = 10,
                Left = Constants.StandardSpacing,
                Width = 610,
                Height = 15,
                Maximum = timeRemaining,
                Value = timeRemaining
            };
            
            // Timer label
            lblTimer = new Label
            {
                Top = 10,
                Left = 560,
                Width = 70,
                ForeColor = Constants.TimerTextColor,
                Font = new Font(Constants.DefaultFontFamily, Constants.SmallFontSize, FontStyle.Bold)
            };
            
            // Question label
            lblQuestion = new Label
            {
                Top = 55,
                Left = Constants.StandardSpacing,
                Width = 600,
                Height = 90,
                Font = new Font(Constants.DefaultFontFamily, Constants.LargeFontSize, FontStyle.Bold)
            };
            
            // Radio button options
            options = new RadioButton[4];
            for (int i = 0; i < 4; i++)
            {
                options[i] = new RadioButton
                {
                    Top = 160 + i * 50,
                    Left = 40,
                    Width = 580,
                    Font = new Font(Constants.DefaultFontFamily, 11F)
                };
                Controls.Add(options[i]);
            }
            
            // Flag checkbox
            chkFlag = new CheckBox
            {
                Text = "Flag for Review",
                Top = 370,
                Left = 40,
                Width = 150
            };
            
            // Navigation buttons
            btnPrev = new Button
            {
                Text = "← Back",
                Top = 420,
                Left = 40,
                Width = 110,
                Height = 45
            };
            
            btnNext = new Button
            {
                Text = "Next →",
                Top = 420,
                Left = 160,
                Width = 110,
                Height = 45
            };
            
            btnSubmit = new Button
            {
                Text = "Submit",
                Top = 420,
                Left = 460,
                Width = 150,
                Height = 45,
                BackColor = Constants.SubmitButtonColor,
                ForeColor = Color.White,
                Font = new Font(Constants.DefaultFontFamily, Constants.DefaultFontSize, FontStyle.Bold)
            };
            
            // Navigation panel
            navPanel = new FlowLayoutPanel
            {
                Top = 10,
                Left = 650,
                Width = 220,
                Height = 450,
                AutoScroll = true,
                BackColor = Constants.NavPanelBackColor
            };
            
            // Create navigation buttons
            navButtons = new Button[questions.Count];
            for (int i = 0; i < questions.Count; i++)
            {
                int index = i; // Capture for lambda
                navButtons[i] = new Button
                {
                    Text = (i + 1).ToString(),
                    Width = 45,
                    Height = 45,
                    BackColor = Constants.UnansweredQuestionColor
                };
                // Event handler will be attached in AttachEventHandlers
                navPanel.Controls.Add(navButtons[i]);
            }
            
            // Add controls
            Controls.AddRange(new Control[]
            {
                prgTime, lblTotalTime, lblTimer, lblQuestion,
                btnPrev, btnNext, btnSubmit, navPanel, chkFlag
            });
        }
        
        /// <summary>
        /// Attaches event handlers - CRITICAL for button responsiveness
        /// </summary>
        private void AttachEventHandlers()
        {
            btnPrev.Click += BtnPrev_Click;
            btnNext.Click += BtnNext_Click;
            btnSubmit.Click += BtnSubmit_Click;
            chkFlag.CheckedChanged += ChkFlag_CheckedChanged;
            FormClosing += QuizForm_FormClosing;
            
            // Attach navigation button handlers
            for (int i = 0; i < navButtons.Length; i++)
            {
                int index = i; // Capture for lambda
                navButtons[i].Click += (s, e) =>
                {
                    SaveState();
                    currentIndex = index;
                    ShowQuestion();
                };
            }
        }
        
        /// <summary>
        /// Starts the quiz timer and shows first question
        /// </summary>
        private void StartQuiz()
        {
            quizTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            quizTimer.Tick += QuizTimer_Tick;
            quizTimer.Start();
            ShowQuestion();
        }
        
        /// <summary>
        /// Timer tick event handler
        /// </summary>
        private void QuizTimer_Tick(object? sender, EventArgs e)
        {
            timeRemaining--;
            
            if (timeRemaining < 0)
            {
                FinishQuiz();
                return;
            }
            
            prgTime.Value = timeRemaining;
            var timeSpan = TimeSpan.FromSeconds(timeRemaining);
            lblTimer.Text = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
        
        /// <summary>
        /// Shows the current question
        /// </summary>
        private void ShowQuestion()
        {
            var q = questions[currentIndex];
            lblQuestion.Text = $"Question {currentIndex + 1} of {questions.Count}:\n{q.Text}";
            
            if (q.ShuffledOrder != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    options[i].Text = q.Options[q.ShuffledOrder[i]];
                    options[i].Checked = (studentAnswers[currentIndex] == i);
                }
            }
            
            chkFlag.Checked = flagged[currentIndex];
            UpdateNavColor();
        }
        
        /// <summary>
        /// Saves the current answer state
        /// </summary>
        private void SaveState()
        {
            studentAnswers[currentIndex] = -1;
            for (int i = 0; i < 4; i++)
            {
                if (options[i].Checked)
                {
                    studentAnswers[currentIndex] = i;
                }
            }
            UpdateNavColor();
        }
        
        /// <summary>
        /// Updates navigation button colors based on answer status
        /// </summary>
        private void UpdateNavColor()
        {
            for (int i = 0; i < navButtons.Length; i++)
            {
                // Set background color
                if (flagged[i])
                    navButtons[i].BackColor = Constants.FlaggedQuestionColor;
                else if (studentAnswers[i] != -1)
                    navButtons[i].BackColor = Constants.AnsweredQuestionColor;
                else
                    navButtons[i].BackColor = Constants.UnansweredQuestionColor;
                
                // Highlight current question
                if (i == currentIndex)
                {
                    navButtons[i].FlatStyle = FlatStyle.Flat;
                    navButtons[i].FlatAppearance.BorderSize = 3;
                    navButtons[i].FlatAppearance.BorderColor = Constants.ActiveQuestionBorderColor;
                }
                else
                {
                    navButtons[i].FlatStyle = FlatStyle.Standard;
                    navButtons[i].FlatAppearance.BorderSize = 1;
                }
            }
        }
        
        /// <summary>
        /// Previous button click handler
        /// </summary>
        private void BtnPrev_Click(object? sender, EventArgs e)
        {
            SaveState();
            if (currentIndex > 0)
            {
                currentIndex--;
                ShowQuestion();
            }
        }
        
        /// <summary>
        /// Next button click handler
        /// </summary>
        private void BtnNext_Click(object? sender, EventArgs e)
        {
            SaveState();
            if (currentIndex < questions.Count - 1)
            {
                currentIndex++;
                ShowQuestion();
            }
        }
        
        /// <summary>
        /// Submit button click handler
        /// </summary>
        private void BtnSubmit_Click(object? sender, EventArgs e)
        {
            var result = MessageBox.Show("Finalize and Submit?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                FinishQuiz();
            }
        }
        
        /// <summary>
        /// Flag checkbox changed handler
        /// </summary>
        private void ChkFlag_CheckedChanged(object? sender, EventArgs e)
        {
            flagged[currentIndex] = chkFlag.Checked;
            UpdateNavColor();
        }
        
        /// <summary>
        /// Form closing event handler with confirmation
        /// </summary>
        private void QuizForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (quizTimer != null && quizTimer.Enabled)
            {
                var result = MessageBox.Show("Are you sure you want to exit? Your progress will be lost.",
                    "Exit Exam", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }
        
        /// <summary>
        /// Finishes the quiz, calculates score, and saves results
        /// </summary>
        private void FinishQuiz()
        {
            if (quizTimer != null)
            {
                quizTimer.Stop();
                quizTimer.Dispose();
                quizTimer = null;
            }
            
            SaveState();
            
            // Calculate score
            int score = 0;
            for (int i = 0; i < questions.Count; i++)
            {
                if (studentAnswers[i] != -1 && questions[i].ShuffledOrder != null)
                {
                    if (questions[i].ShuffledOrder[studentAnswers[i]] == questions[i].CorrectIndex)
                    {
                        score++;
                    }
                }
            }
            
            // Show results
            MessageBox.Show($"Test Completed!\nStudent: {studentName}\nScore: {score}/{questions.Count}",
                "Quiz Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            // Save results using FileService
            FileService.SaveResult(studentName, studentClass, score, questions.Count);
            
            Application.Exit();
        }
        
        /// <summary>
        /// Disposes resources including timer
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                base.Dispose(disposing);
                return;
            }
            
            if (quizTimer != null)
            {
                quizTimer.Stop();
                quizTimer.Dispose();
                quizTimer = null;
            }
            
            base.Dispose(disposing);
        }
    }
}
