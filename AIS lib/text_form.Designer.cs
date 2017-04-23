namespace AIS_Decoder
{
    partial class text_form
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
            this.r_t_b = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // r_t_b
            // 
            this.r_t_b.Dock = System.Windows.Forms.DockStyle.Fill;
            this.r_t_b.Location = new System.Drawing.Point(0, 0);
            this.r_t_b.Name = "r_t_b";
            this.r_t_b.Size = new System.Drawing.Size(284, 262);
            this.r_t_b.TabIndex = 0;
            this.r_t_b.Text = "";
            // 
            // text_form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.r_t_b);
            this.Name = "text_form";
            this.Text = "text_form";
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.RichTextBox r_t_b;

    }
}