namespace WinFormsMogwai
{
    partial class FormMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            splitContainer1 = new SplitContainer();
            tableLayoutPanel1 = new TableLayoutPanel();
            ExecuteButton = new Button();
            HaltButton = new Button();
            label2 = new Label();
            label1 = new Label();
            SamplesComboBox = new ComboBox();
            CodeTextBox = new TextBox();
            splitContainer2 = new SplitContainer();
            label3 = new Label();
            DrawTurtlePictureBox = new PictureBox();
            OutputTextBox = new ConsoleDisplay();
            label4 = new Label();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)DrawTurtlePictureBox).BeginInit();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.BorderStyle = BorderStyle.FixedSingle;
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Margin = new Padding(4);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tableLayoutPanel1);
            splitContainer1.Panel1.Controls.Add(label2);
            splitContainer1.Panel1.Controls.Add(label1);
            splitContainer1.Panel1.Controls.Add(SamplesComboBox);
            splitContainer1.Panel1.Controls.Add(CodeTextBox);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new Size(1807, 1039);
            splitContainer1.SplitterDistance = 863;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 4;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(ExecuteButton, 0, 0);
            tableLayoutPanel1.Controls.Add(HaltButton, 1, 0);
            tableLayoutPanel1.Location = new Point(13, 951);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new Size(834, 74);
            tableLayoutPanel1.TabIndex = 9;
            // 
            // ExecuteButton
            // 
            ExecuteButton.Dock = DockStyle.Fill;
            ExecuteButton.Location = new Point(10, 10);
            ExecuteButton.Margin = new Padding(10);
            ExecuteButton.Name = "ExecuteButton";
            ExecuteButton.Size = new Size(397, 54);
            ExecuteButton.TabIndex = 5;
            ExecuteButton.Text = "Execute";
            ExecuteButton.UseVisualStyleBackColor = true;
            ExecuteButton.Click += ExecuteButton_Click;
            // 
            // HaltButton
            // 
            HaltButton.Dock = DockStyle.Fill;
            HaltButton.Enabled = false;
            HaltButton.Location = new Point(427, 10);
            HaltButton.Margin = new Padding(10);
            HaltButton.Name = "HaltButton";
            HaltButton.Size = new Size(397, 54);
            HaltButton.TabIndex = 6;
            HaltButton.Text = "Stop";
            HaltButton.UseVisualStyleBackColor = true;
            HaltButton.Click += HaltButton_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(11, 72);
            label2.Name = "label2";
            label2.Size = new Size(131, 23);
            label2.TabIndex = 8;
            label2.Text = "MOGWAI code";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(9, 8);
            label1.Name = "label1";
            label1.Size = new Size(98, 23);
            label1.TabIndex = 7;
            label1.Text = "Examples";
            // 
            // SamplesComboBox
            // 
            SamplesComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            SamplesComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            SamplesComboBox.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            SamplesComboBox.FormattingEnabled = true;
            SamplesComboBox.Items.AddRange(new object[] { "Downloading resources (http.get)", "Additional functions (turtle)", "Using tasks", "The mystery number" });
            SamplesComboBox.Location = new Point(11, 33);
            SamplesComboBox.Name = "SamplesComboBox";
            SamplesComboBox.Size = new Size(836, 30);
            SamplesComboBox.TabIndex = 6;
            SamplesComboBox.SelectedIndexChanged += SamplesComboBox_SelectedIndexChanged;
            // 
            // CodeTextBox
            // 
            CodeTextBox.AcceptsReturn = true;
            CodeTextBox.AcceptsTab = true;
            CodeTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            CodeTextBox.BorderStyle = BorderStyle.FixedSingle;
            CodeTextBox.Font = new Font("Consolas", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            CodeTextBox.ForeColor = Color.Blue;
            CodeTextBox.Location = new Point(13, 97);
            CodeTextBox.Margin = new Padding(4, 2, 4, 2);
            CodeTextBox.Multiline = true;
            CodeTextBox.Name = "CodeTextBox";
            CodeTextBox.ScrollBars = ScrollBars.Both;
            CodeTextBox.Size = new Size(836, 829);
            CodeTextBox.TabIndex = 4;
            CodeTextBox.WordWrap = false;
            // 
            // splitContainer2
            // 
            splitContainer2.BorderStyle = BorderStyle.FixedSingle;
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(label3);
            splitContainer2.Panel1.Controls.Add(DrawTurtlePictureBox);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(OutputTextBox);
            splitContainer2.Panel2.Controls.Add(label4);
            splitContainer2.Size = new Size(939, 1039);
            splitContainer2.SplitterDistance = 518;
            splitContainer2.TabIndex = 2;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(14, 8);
            label3.Name = "label3";
            label3.Size = new Size(131, 23);
            label3.TabIndex = 8;
            label3.Text = "Turtle area";
            // 
            // DrawTurtlePictureBox
            // 
            DrawTurtlePictureBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            DrawTurtlePictureBox.BackColor = Color.Blue;
            DrawTurtlePictureBox.BorderStyle = BorderStyle.FixedSingle;
            DrawTurtlePictureBox.Location = new Point(19, 34);
            DrawTurtlePictureBox.Name = "DrawTurtlePictureBox";
            DrawTurtlePictureBox.Size = new Size(907, 466);
            DrawTurtlePictureBox.TabIndex = 5;
            DrawTurtlePictureBox.TabStop = false;
            DrawTurtlePictureBox.Paint += DrawTurtlePictureBox_Paint;
            // 
            // OutputTextBox
            // 
            OutputTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            OutputTextBox.BackColor = Color.Black;
            OutputTextBox.BorderStyle = BorderStyle.FixedSingle;
            OutputTextBox.Font = new Font("Consolas", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            OutputTextBox.ForeColor = Color.Lime;
            OutputTextBox.Location = new Point(14, 44);
            OutputTextBox.Multiline = true;
            OutputTextBox.Name = "OutputTextBox";
            OutputTextBox.ReadOnly = true;
            OutputTextBox.ScrollBars = ScrollBars.Vertical;
            OutputTextBox.Size = new Size(907, 459);
            OutputTextBox.TabIndex = 10;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(14, 18);
            label4.Name = "label4";
            label4.Size = new Size(76, 23);
            label4.TabIndex = 9;
            label4.Text = "Output";
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(144F, 144F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1807, 1039);
            Controls.Add(splitContainer1);
            DoubleBuffered = true;
            Font = new Font("Consolas", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 2, 4, 2);
            Name = "FormMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "MOGWAI WinForms integration";
            WindowState = FormWindowState.Maximized;
            SizeChanged += FormMain_SizeChanged;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel1.PerformLayout();
            splitContainer2.Panel2.ResumeLayout(false);
            splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)DrawTurtlePictureBox).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private TextBox CodeTextBox;
        private Button ExecuteButton;
        private SplitContainer splitContainer2;
        private PictureBox DrawTurtlePictureBox;
        private Label label1;
        private ComboBox SamplesComboBox;
        private Label label2;
        private Label label3;
        private Label label4;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button HaltButton;
        private TableLayoutPanel tableLayoutPanel1;
        private ConsoleDisplay OutputTextBox;
    }
}
