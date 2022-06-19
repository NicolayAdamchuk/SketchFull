namespace SketchFull
{
    partial class AreaReinforcementDialog
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
            this.groupDirection = new System.Windows.Forms.GroupBox();
            this.radioSecond = new System.Windows.Forms.RadioButton();
            this.radioMain = new System.Windows.Forms.RadioButton();
            this.groupLayer = new System.Windows.Forms.GroupBox();
            this.radioDown = new System.Windows.Forms.RadioButton();
            this.radioUp = new System.Windows.Forms.RadioButton();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupDirection.SuspendLayout();
            this.groupLayer.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupDirection
            // 
            this.groupDirection.Controls.Add(this.radioSecond);
            this.groupDirection.Controls.Add(this.radioMain);
            this.groupDirection.Location = new System.Drawing.Point(29, 25);
            this.groupDirection.Name = "groupDirection";
            this.groupDirection.Size = new System.Drawing.Size(250, 100);
            this.groupDirection.TabIndex = 0;
            this.groupDirection.TabStop = false;
            this.groupDirection.Text = "Направление армирования";
            // 
            // radioSecond
            // 
            this.radioSecond.AutoSize = true;
            this.radioSecond.Location = new System.Drawing.Point(6, 66);
            this.radioSecond.Name = "radioSecond";
            this.radioSecond.Size = new System.Drawing.Size(161, 24);
            this.radioSecond.TabIndex = 1;
            this.radioSecond.TabStop = true;
            this.radioSecond.Text = "Второстепенное";
            this.radioSecond.UseVisualStyleBackColor = true;
            this.radioSecond.CheckedChanged += new System.EventHandler(this.RadioSecond_CheckedChanged);
            // 
            // radioMain
            // 
            this.radioMain.AutoSize = true;
            this.radioMain.Checked = true;
            this.radioMain.Location = new System.Drawing.Point(6, 36);
            this.radioMain.Name = "radioMain";
            this.radioMain.Size = new System.Drawing.Size(108, 24);
            this.radioMain.TabIndex = 0;
            this.radioMain.TabStop = true;
            this.radioMain.Text = "Основное";
            this.radioMain.UseVisualStyleBackColor = true;
            this.radioMain.CheckedChanged += new System.EventHandler(this.RadioMain_CheckedChanged);
            // 
            // groupLayer
            // 
            this.groupLayer.Controls.Add(this.radioDown);
            this.groupLayer.Controls.Add(this.radioUp);
            this.groupLayer.Location = new System.Drawing.Point(302, 25);
            this.groupLayer.Name = "groupLayer";
            this.groupLayer.Size = new System.Drawing.Size(250, 100);
            this.groupLayer.TabIndex = 1;
            this.groupLayer.TabStop = false;
            this.groupLayer.Text = "Слой армирования";
            // 
            // radioDown
            // 
            this.radioDown.AutoSize = true;
            this.radioDown.Location = new System.Drawing.Point(6, 66);
            this.radioDown.Name = "radioDown";
            this.radioDown.Size = new System.Drawing.Size(93, 24);
            this.radioDown.TabIndex = 1;
            this.radioDown.TabStop = true;
            this.radioDown.Text = "Нижний";
            this.radioDown.UseVisualStyleBackColor = true;
            this.radioDown.CheckedChanged += new System.EventHandler(this.RadioDown_CheckedChanged);
            // 
            // radioUp
            // 
            this.radioUp.AutoSize = true;
            this.radioUp.Checked = true;
            this.radioUp.Location = new System.Drawing.Point(6, 36);
            this.radioUp.Name = "radioUp";
            this.radioUp.Size = new System.Drawing.Size(97, 24);
            this.radioUp.TabIndex = 0;
            this.radioUp.TabStop = true;
            this.radioUp.Text = "Верхний";
            this.radioUp.UseVisualStyleBackColor = true;
            this.radioUp.CheckedChanged += new System.EventHandler(this.RadioUp_CheckedChanged);
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(29, 167);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(250, 36);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(302, 167);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(250, 36);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Отмена";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // AreaReinforcementDialog
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(578, 244);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupLayer);
            this.Controls.Add(this.groupDirection);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AreaReinforcementDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Армирование по площади";
            this.Load += new System.EventHandler(this.AreaReinforcementDialog_Load);
            this.groupDirection.ResumeLayout(false);
            this.groupDirection.PerformLayout();
            this.groupLayer.ResumeLayout(false);
            this.groupLayer.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupDirection;
        private System.Windows.Forms.RadioButton radioSecond;
        private System.Windows.Forms.RadioButton radioMain;
        private System.Windows.Forms.GroupBox groupLayer;
        private System.Windows.Forms.RadioButton radioDown;
        private System.Windows.Forms.RadioButton radioUp;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}