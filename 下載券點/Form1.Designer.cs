namespace 下載券點
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
		private void InitializeComponent() {
		btn下載 = new Button();
		btnTest = new Button();
		txtYMD = new TextBox();
		SuspendLayout();
		// 
		// btn下載
		// 
		btn下載.Location = new Point(1, 3);
		btn下載.Margin = new Padding(4);
		btn下載.Name = "btn下載";
		btn下載.Size = new Size(127, 29);
		btn下載.TabIndex = 0;
		btn下載.Text = "下載";
		btn下載.UseVisualStyleBackColor = true;
		// 
		// btnTest
		// 
		btnTest.Location = new Point(231, 1);
		btnTest.Margin = new Padding(4);
		btnTest.Name = "btnTest";
		btnTest.Size = new Size(42, 29);
		btnTest.TabIndex = 1;
		btnTest.Text = "test";
		btnTest.UseVisualStyleBackColor = true;
		btnTest.Click += btnTest_Click;
		// 
		// txtYMD
		// 
		txtYMD.Location = new Point(135, 3);
		txtYMD.Name = "txtYMD";
		txtYMD.Size = new Size(89, 27);
		txtYMD.TabIndex = 2;
		txtYMD.Text = "20260126";
		// 
		// Form1
		// 
		AutoScaleDimensions = new SizeF(9F, 19F);
		AutoScaleMode = AutoScaleMode.Font;
		ClientSize = new Size(271, 30);
		Controls.Add(txtYMD);
		Controls.Add(btnTest);
		Controls.Add(btn下載);
		Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 136);
		Margin = new Padding(4);
		Name = "Form1";
		Text = "券點";
		ResumeLayout(false);
		PerformLayout();
		}

		#endregion

		private Button btn下載;
		private Button btnTest;
		private TextBox txtYMD;
	}
}
