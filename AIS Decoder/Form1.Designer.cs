namespace AIS_Decoder
{
    partial class Form1
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器
        /// 修改這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.browse_button = new System.Windows.Forms.Button();
            this.action_button = new System.Windows.Forms.Button();
            this.path_comboBox = new System.Windows.Forms.ComboBox();
            this.description_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // browse_button
            // 
            this.browse_button.Location = new System.Drawing.Point(197, 11);
            this.browse_button.Name = "browse_button";
            this.browse_button.Size = new System.Drawing.Size(75, 23);
            this.browse_button.TabIndex = 1;
            this.browse_button.Text = "瀏覽";
            this.browse_button.UseVisualStyleBackColor = true;
            this.browse_button.Click += new System.EventHandler(this.browse_button_Click);
            // 
            // action_button
            // 
            this.action_button.Location = new System.Drawing.Point(197, 40);
            this.action_button.Name = "action_button";
            this.action_button.Size = new System.Drawing.Size(75, 23);
            this.action_button.TabIndex = 2;
            this.action_button.Text = "執行";
            this.action_button.UseVisualStyleBackColor = true;
            this.action_button.Click += new System.EventHandler(this.action_button_Click);
            // 
            // path_comboBox
            // 
            this.path_comboBox.FormattingEnabled = true;
            this.path_comboBox.Location = new System.Drawing.Point(13, 13);
            this.path_comboBox.Name = "path_comboBox";
            this.path_comboBox.Size = new System.Drawing.Size(178, 20);
            this.path_comboBox.TabIndex = 3;
            // 
            // description_button
            // 
            this.description_button.Location = new System.Drawing.Point(197, 227);
            this.description_button.Name = "description_button";
            this.description_button.Size = new System.Drawing.Size(75, 23);
            this.description_button.TabIndex = 4;
            this.description_button.Text = "欄位說明";
            this.description_button.UseVisualStyleBackColor = true;
            this.description_button.Click += new System.EventHandler(this.description_button_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.description_button);
            this.Controls.Add(this.path_comboBox);
            this.Controls.Add(this.action_button);
            this.Controls.Add(this.browse_button);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "AIS Decoder";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button browse_button;
        private System.Windows.Forms.Button action_button;
        private System.Windows.Forms.ComboBox path_comboBox;
        private System.Windows.Forms.Button description_button;
    }
}

