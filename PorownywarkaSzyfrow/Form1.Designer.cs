namespace PorownywarkaSzyfrow
{
    partial class Form1
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
            lblInput = new Label();
            txtInputFile = new TextBox();
            btnBrowseInput = new Button();
            btnBrowseOutput = new Button();
            txtOutputFile = new TextBox();
            lblOutput = new Label();
            lblPassword = new Label();
            txtPassword = new TextBox();
            lblAlgo = new Label();
            cmbAlgorithm = new ComboBox();
            btnEncrypt = new Button();
            btnDecrypt = new Button();
            lblStatus = new Label();
            SuspendLayout();
            // 
            // lblInput
            // 
            lblInput.AutoSize = true;
            lblInput.Location = new Point(114, 56);
            lblInput.Name = "lblInput";
            lblInput.Size = new Size(106, 20);
            lblInput.TabIndex = 0;
            lblInput.Text = "Plik wejściowy:";
            lblInput.Click += lblInput_Click;
            // 
            // txtInputFile
            // 
            txtInputFile.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtInputFile.Location = new Point(278, 53);
            txtInputFile.Name = "txtInputFile";
            txtInputFile.ReadOnly = true;
            txtInputFile.Size = new Size(400, 27);
            txtInputFile.TabIndex = 1;
            // 
            // btnBrowseInput
            // 
            btnBrowseInput.Location = new Point(694, 51);
            btnBrowseInput.Name = "btnBrowseInput";
            btnBrowseInput.Size = new Size(39, 29);
            btnBrowseInput.TabIndex = 2;
            btnBrowseInput.Text = "...";
            btnBrowseInput.UseVisualStyleBackColor = true;
            // 
            // btnBrowseOutput
            // 
            btnBrowseOutput.Location = new Point(694, 122);
            btnBrowseOutput.Name = "btnBrowseOutput";
            btnBrowseOutput.Size = new Size(39, 29);
            btnBrowseOutput.TabIndex = 5;
            btnBrowseOutput.Text = "...";
            btnBrowseOutput.UseVisualStyleBackColor = true;
            // 
            // txtOutputFile
            // 
            txtOutputFile.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtOutputFile.Location = new Point(278, 122);
            txtOutputFile.Name = "txtOutputFile";
            txtOutputFile.ReadOnly = true;
            txtOutputFile.Size = new Size(400, 27);
            txtOutputFile.TabIndex = 4;
            // 
            // lblOutput
            // 
            lblOutput.AutoSize = true;
            lblOutput.Location = new Point(115, 122);
            lblOutput.Name = "lblOutput";
            lblOutput.Size = new Size(105, 20);
            lblOutput.TabIndex = 3;
            lblOutput.Text = "Plik wyjściowy:";
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(144, 187);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(50, 20);
            lblPassword.TabIndex = 6;
            lblPassword.Text = "Hasło:";
            // 
            // txtPassword
            // 
            txtPassword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtPassword.Location = new Point(328, 184);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(300, 27);
            txtPassword.TabIndex = 7;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // lblAlgo
            // 
            lblAlgo.AutoSize = true;
            lblAlgo.Location = new Point(132, 248);
            lblAlgo.Name = "lblAlgo";
            lblAlgo.Size = new Size(74, 20);
            lblAlgo.TabIndex = 8;
            lblAlgo.Text = "Algorytm:";
            // 
            // cmbAlgorithm
            // 
            cmbAlgorithm.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAlgorithm.FormattingEnabled = true;
            cmbAlgorithm.Items.AddRange(new object[] { "Vigenere", "DES", "RC2" });
            cmbAlgorithm.Location = new Point(404, 248);
            cmbAlgorithm.Name = "cmbAlgorithm";
            cmbAlgorithm.Size = new Size(151, 28);
            cmbAlgorithm.TabIndex = 9;
            // 
            // btnEncrypt
            // 
            btnEncrypt.Location = new Point(207, 318);
            btnEncrypt.Name = "btnEncrypt";
            btnEncrypt.Size = new Size(94, 29);
            btnEncrypt.TabIndex = 10;
            btnEncrypt.Text = "Szyfruj";
            btnEncrypt.UseVisualStyleBackColor = true;
            // 
            // btnDecrypt
            // 
            btnDecrypt.Location = new Point(546, 318);
            btnDecrypt.Name = "btnDecrypt";
            btnDecrypt.Size = new Size(94, 29);
            btnDecrypt.TabIndex = 11;
            btnDecrypt.Text = "Odszyfruj";
            btnDecrypt.UseVisualStyleBackColor = true;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(404, 383);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(61, 20);
            lblStatus.TabIndex = 12;
            lblStatus.Text = "Gotowe";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(lblStatus);
            Controls.Add(btnDecrypt);
            Controls.Add(btnEncrypt);
            Controls.Add(cmbAlgorithm);
            Controls.Add(lblAlgo);
            Controls.Add(txtPassword);
            Controls.Add(lblPassword);
            Controls.Add(btnBrowseOutput);
            Controls.Add(txtOutputFile);
            Controls.Add(lblOutput);
            Controls.Add(btnBrowseInput);
            Controls.Add(txtInputFile);
            Controls.Add(lblInput);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Porównywarka szyfrów";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblInput;
        private TextBox txtInputFile;
        private Button btnBrowseInput;
        private Button btnBrowseOutput;
        private TextBox txtOutputFile;
        private Label lblOutput;
        private Label lblPassword;
        private TextBox txtPassword;
        private Label lblAlgo;
        private ComboBox cmbAlgorithm;
        private Button btnEncrypt;
        private Button btnDecrypt;
        private Label lblStatus;
    }
}
