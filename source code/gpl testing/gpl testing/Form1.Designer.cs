namespace GPL_Testing
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button_PERMIS_xml = new System.Windows.Forms.Button();
            this.button_LoadFile = new System.Windows.Forms.Button();
            this.TextBox_UserName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_Compile = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.TextBoxGPL_Error = new System.Windows.Forms.TextBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.TextBoxGPL_Code = new FastColoredTextBoxNS.FastColoredTextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button_PERMIS_xml);
            this.panel1.Controls.Add(this.button_LoadFile);
            this.panel1.Controls.Add(this.TextBox_UserName);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.button_Compile);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(752, 71);
            this.panel1.TabIndex = 1;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // button_PERMIS_xml
            // 
            this.button_PERMIS_xml.Location = new System.Drawing.Point(629, 12);
            this.button_PERMIS_xml.Name = "button_PERMIS_xml";
            this.button_PERMIS_xml.Size = new System.Drawing.Size(74, 42);
            this.button_PERMIS_xml.TabIndex = 4;
            this.button_PERMIS_xml.Text = "PERMIS xml";
            this.button_PERMIS_xml.UseVisualStyleBackColor = true;
            this.button_PERMIS_xml.Click += new System.EventHandler(this.button_PERMIS_xml_Click);
            // 
            // button_LoadFile
            // 
            this.button_LoadFile.Location = new System.Drawing.Point(12, 12);
            this.button_LoadFile.Name = "button_LoadFile";
            this.button_LoadFile.Size = new System.Drawing.Size(74, 42);
            this.button_LoadFile.TabIndex = 3;
            this.button_LoadFile.Text = "Load File";
            this.button_LoadFile.UseVisualStyleBackColor = true;
            this.button_LoadFile.Click += new System.EventHandler(this.button_LoadFile_Click);
            // 
            // TextBox_UserName
            // 
            this.TextBox_UserName.Location = new System.Drawing.Point(328, 21);
            this.TextBox_UserName.Name = "TextBox_UserName";
            this.TextBox_UserName.Size = new System.Drawing.Size(170, 20);
            this.TextBox_UserName.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(213, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "User/Object Name";
            // 
            // button_Compile
            // 
            this.button_Compile.Location = new System.Drawing.Point(523, 12);
            this.button_Compile.Name = "button_Compile";
            this.button_Compile.Size = new System.Drawing.Size(100, 42);
            this.button_Compile.TabIndex = 0;
            this.button_Compile.Text = "Compile and Test Policy";
            this.button_Compile.UseVisualStyleBackColor = true;
            this.button_Compile.Click += new System.EventHandler(this.button_Compile_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.TextBoxGPL_Error);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 493);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(752, 130);
            this.panel2.TabIndex = 3;
            // 
            // TextBoxGPL_Error
            // 
            this.TextBoxGPL_Error.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextBoxGPL_Error.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TextBoxGPL_Error.Font = new System.Drawing.Font("Courier New", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxGPL_Error.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.TextBoxGPL_Error.Location = new System.Drawing.Point(0, 0);
            this.TextBoxGPL_Error.Multiline = true;
            this.TextBoxGPL_Error.Name = "TextBoxGPL_Error";
            this.TextBoxGPL_Error.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.TextBoxGPL_Error.Size = new System.Drawing.Size(752, 130);
            this.TextBoxGPL_Error.TabIndex = 1;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.TextBoxGPL_Code);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 71);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(752, 422);
            this.panel3.TabIndex = 4;
            // 
            // TextBoxGPL_Code
            // 
            this.TextBoxGPL_Code.AutoScrollMinSize = new System.Drawing.Size(25, 15);
            this.TextBoxGPL_Code.BackBrush = null;
            this.TextBoxGPL_Code.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TextBoxGPL_Code.ChangedLineColor = System.Drawing.Color.White;
            this.TextBoxGPL_Code.CurrentLineColor = System.Drawing.Color.SkyBlue;
            this.TextBoxGPL_Code.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.TextBoxGPL_Code.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.TextBoxGPL_Code.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TextBoxGPL_Code.HighlightingRangeType = FastColoredTextBoxNS.HighlightingRangeType.AllTextRange;
            this.TextBoxGPL_Code.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.TextBoxGPL_Code.Language = FastColoredTextBoxNS.Language.CSharp;
            this.TextBoxGPL_Code.LineNumberStartValue = ((uint)(0u));
            this.TextBoxGPL_Code.Location = new System.Drawing.Point(0, 0);
            this.TextBoxGPL_Code.Name = "TextBoxGPL_Code";
            this.TextBoxGPL_Code.Paddings = new System.Windows.Forms.Padding(0);
            this.TextBoxGPL_Code.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(0)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.TextBoxGPL_Code.Size = new System.Drawing.Size(752, 422);
            this.TextBoxGPL_Code.TabIndex = 3;
            this.TextBoxGPL_Code.Load += new System.EventHandler(this.fastColoredTextBox_Source_Load);
            this.TextBoxGPL_Code.TextChanged += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.TextBoxGPL_Code_TextChanged);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "\"GPL files|*.gpl|Text files|*.txt\"";
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(752, 623);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "FormMain";
            this.Text = "GPL Demo";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox TextBox_UserName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_Compile;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button button_LoadFile;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TextBox TextBoxGPL_Error;
        protected FastColoredTextBoxNS.FastColoredTextBox TextBoxGPL_Code;
        private System.Windows.Forms.Button button_PERMIS_xml;
    }
}

