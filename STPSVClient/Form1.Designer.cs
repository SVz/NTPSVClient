namespace STPSVClient;

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
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
        button1 = new Button();
        checkBox1 = new CheckBox();
        richTextBox1 = new RichTextBox();
        textBox1 = new TextBox();
        SuspendLayout();
        // 
        // button1
        // 
        button1.Location = new Point(12, 12);
        button1.Name = "button1";
        button1.Size = new Size(103, 36);
        button1.TabIndex = 0;
        button1.Text = "Get Time";
        button1.UseVisualStyleBackColor = true;
        button1.Click += button1_Click;
        // 
        // checkBox1
        // 
        checkBox1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        checkBox1.Location = new Point(350, 20);
        checkBox1.Name = "checkBox1";
        checkBox1.Size = new Size(73, 24);
        checkBox1.TabIndex = 1;
        checkBox1.Text = "Sync PC";
        checkBox1.UseVisualStyleBackColor = true;
        // 
        // richTextBox1
        // 
        richTextBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        richTextBox1.BackColor = Color.Ivory;
        richTextBox1.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
        richTextBox1.Location = new Point(12, 60);
        richTextBox1.Name = "richTextBox1";
        richTextBox1.Size = new Size(411, 92);
        richTextBox1.TabIndex = 2;
        richTextBox1.Text = "";
        // 
        // textBox1
        // 
        textBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        textBox1.Location = new Point(121, 20);
        textBox1.Name = "textBox1";
        textBox1.Size = new Size(191, 23);
        textBox1.TabIndex = 3;
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(435, 164);
        Controls.Add(textBox1);
        Controls.Add(richTextBox1);
        Controls.Add(checkBox1);
        Controls.Add(button1);
        Icon = (Icon)resources.GetObject("$this.Icon");
        MinimumSize = new Size(451, 203);
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "NTP Client V1.0 by SVz (2o25)";
        Load += Form1_Load;
        ResumeLayout(false);
        PerformLayout();
    }

    private System.Windows.Forms.TextBox textBox1;

    private System.Windows.Forms.RichTextBox richTextBox1;

    private System.Windows.Forms.CheckBox checkBox1;

    private System.Windows.Forms.Button button1;

    #endregion
}