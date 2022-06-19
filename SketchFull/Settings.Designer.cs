namespace SketchFull
{
    partial class Settings
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelFont = new System.Windows.Forms.Label();
            this.comboFonts = new System.Windows.Forms.ComboBox();
            this.comboRebar = new System.Windows.Forms.ComboBox();
            this.checkUseExistTag = new System.Windows.Forms.CheckBox();
            this.checkAligment = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkDeleteGroup = new System.Windows.Forms.CheckBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonUpdate = new System.Windows.Forms.Button();
            this.comboLines = new System.Windows.Forms.ComboBox();
            this.labelLines = new System.Windows.Forms.Label();
            this.buttonUpdateView = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textScale = new System.Windows.Forms.TextBox();
            this.labelScale = new System.Windows.Forms.Label();
            this.labelScale0 = new System.Windows.Forms.Label();
            this.checkBoxLength = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(179, 469);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(120, 30);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Отмена";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // labelFont
            // 
            this.labelFont.AutoSize = true;
            this.labelFont.Location = new System.Drawing.Point(389, 243);
            this.labelFont.Name = "labelFont";
            this.labelFont.Size = new System.Drawing.Size(64, 20);
            this.labelFont.TabIndex = 4;
            this.labelFont.Text = "Шрифт";
            // 
            // comboFonts
            // 
            this.comboFonts.FormattingEnabled = true;
            this.comboFonts.Location = new System.Drawing.Point(101, 235);
            this.comboFonts.Name = "comboFonts";
            this.comboFonts.Size = new System.Drawing.Size(281, 28);
            this.comboFonts.TabIndex = 5;
            this.comboFonts.SelectedIndexChanged += new System.EventHandler(this.ComboFonts_SelectedIndexChanged);
            // 
            // comboRebar
            // 
            this.comboRebar.FormattingEnabled = true;
            this.comboRebar.Location = new System.Drawing.Point(68, 77);
            this.comboRebar.Name = "comboRebar";
            this.comboRebar.Size = new System.Drawing.Size(281, 28);
            this.comboRebar.TabIndex = 7;
            this.comboRebar.SelectedIndexChanged += new System.EventHandler(this.ComboRebar_SelectedIndexChanged);
            // 
            // checkUseExistTag
            // 
            this.checkUseExistTag.AutoSize = true;
            this.checkUseExistTag.Checked = true;
            this.checkUseExistTag.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkUseExistTag.Location = new System.Drawing.Point(68, 36);
            this.checkUseExistTag.Name = "checkUseExistTag";
            this.checkUseExistTag.Size = new System.Drawing.Size(244, 24);
            this.checkUseExistTag.TabIndex = 8;
            this.checkUseExistTag.Text = "Использовать как префикс";
            this.checkUseExistTag.UseVisualStyleBackColor = true;
            this.checkUseExistTag.CheckedChanged += new System.EventHandler(this.CheckUseExistTag_CheckedChanged);
            // 
            // checkAligment
            // 
            this.checkAligment.AutoSize = true;
            this.checkAligment.Checked = true;
            this.checkAligment.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkAligment.Location = new System.Drawing.Point(101, 382);
            this.checkAligment.Name = "checkAligment";
            this.checkAligment.Size = new System.Drawing.Size(235, 24);
            this.checkAligment.TabIndex = 4;
            this.checkAligment.Text = "Выполнить выравнивание";
            this.checkAligment.UseVisualStyleBackColor = true;
            this.checkAligment.CheckedChanged += new System.EventHandler(this.CheckAligment_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBoxLength);
            this.groupBox1.Controls.Add(this.comboRebar);
            this.groupBox1.Controls.Add(this.checkUseExistTag);
            this.groupBox1.Location = new System.Drawing.Point(33, 43);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(420, 168);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Марка для эскиза";
            // 
            // checkDeleteGroup
            // 
            this.checkDeleteGroup.AutoSize = true;
            this.checkDeleteGroup.Checked = true;
            this.checkDeleteGroup.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkDeleteGroup.Location = new System.Drawing.Point(101, 425);
            this.checkDeleteGroup.Name = "checkDeleteGroup";
            this.checkDeleteGroup.Size = new System.Drawing.Size(352, 24);
            this.checkDeleteGroup.TabIndex = 11;
            this.checkDeleteGroup.Text = "Удалять лишние эскизы при обновлении ";
            this.checkDeleteGroup.UseVisualStyleBackColor = true;
            this.checkDeleteGroup.CheckedChanged += new System.EventHandler(this.CheckDeleteGroup_CheckedChanged);
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(42, 469);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(120, 30);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.buttonUpdate.Location = new System.Drawing.Point(308, 516);
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Size = new System.Drawing.Size(167, 30);
            this.buttonUpdate.TabIndex = 13;
            this.buttonUpdate.Text = "Обновить все";
            this.buttonUpdate.UseVisualStyleBackColor = true;
            // 
            // comboLines
            // 
            this.comboLines.FormattingEnabled = true;
            this.comboLines.Location = new System.Drawing.Point(101, 291);
            this.comboLines.Name = "comboLines";
            this.comboLines.Size = new System.Drawing.Size(281, 28);
            this.comboLines.TabIndex = 14;
            this.comboLines.SelectedIndexChanged += new System.EventHandler(this.ComboLines_SelectedIndexChanged);
            // 
            // labelLines
            // 
            this.labelLines.AutoSize = true;
            this.labelLines.Location = new System.Drawing.Point(389, 299);
            this.labelLines.Name = "labelLines";
            this.labelLines.Size = new System.Drawing.Size(86, 20);
            this.labelLines.TabIndex = 15;
            this.labelLines.Text = "Тип линии";
            // 
            // buttonUpdateView
            // 
            this.buttonUpdateView.DialogResult = System.Windows.Forms.DialogResult.No;
            this.buttonUpdateView.Location = new System.Drawing.Point(308, 467);
            this.buttonUpdateView.Name = "buttonUpdateView";
            this.buttonUpdateView.Size = new System.Drawing.Size(167, 30);
            this.buttonUpdateView.TabIndex = 16;
            this.buttonUpdateView.Text = "Обновить вид";
            this.buttonUpdateView.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::SketchFull.Properties.Resources.logo_ar_cadia;
            this.pictureBox1.Location = new System.Drawing.Point(42, 505);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(101, 30);
            this.pictureBox1.TabIndex = 17;
            this.pictureBox1.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.DarkRed;
            this.label2.Location = new System.Drawing.Point(38, 538);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(159, 20);
            this.label2.TabIndex = 18;
            this.label2.Text = "www.ar-cadia.com.ua";
            // 
            // textScale
            // 
            this.textScale.Location = new System.Drawing.Point(324, 339);
            this.textScale.Name = "textScale";
            this.textScale.Size = new System.Drawing.Size(58, 26);
            this.textScale.TabIndex = 19;
            this.textScale.Text = "1,0";
            this.textScale.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textScale.TextChanged += new System.EventHandler(this.TextScale_TextChanged);
            this.textScale.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextScale_KeyPress);
            // 
            // labelScale
            // 
            this.labelScale.AutoSize = true;
            this.labelScale.Location = new System.Drawing.Point(389, 359);
            this.labelScale.Name = "labelScale";
            this.labelScale.Size = new System.Drawing.Size(86, 20);
            this.labelScale.TabIndex = 20;
            this.labelScale.Text = "масштаба";
            // 
            // labelScale0
            // 
            this.labelScale0.AutoSize = true;
            this.labelScale0.ForeColor = System.Drawing.SystemColors.ControlText;
            this.labelScale0.Location = new System.Drawing.Point(389, 334);
            this.labelScale0.Name = "labelScale0";
            this.labelScale0.Size = new System.Drawing.Size(71, 20);
            this.labelScale0.TabIndex = 21;
            this.labelScale0.Text = "Коэфф.";
            // 
            // checkBoxLength
            // 
            this.checkBoxLength.AutoSize = true;
            this.checkBoxLength.Location = new System.Drawing.Point(68, 126);
            this.checkBoxLength.Name = "checkBoxLength";
            this.checkBoxLength.Size = new System.Drawing.Size(194, 24);
            this.checkBoxLength.TabIndex = 22;
            this.checkBoxLength.Text = "Add Ø and total length";
            this.checkBoxLength.UseVisualStyleBackColor = true;
            this.checkBoxLength.CheckedChanged += new System.EventHandler(this.CheckBoxLength_CheckedChanged);
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(496, 590);
            this.Controls.Add(this.labelScale0);
            this.Controls.Add(this.labelScale);
            this.Controls.Add(this.textScale);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.buttonUpdateView);
            this.Controls.Add(this.labelLines);
            this.Controls.Add(this.comboLines);
            this.Controls.Add(this.buttonUpdate);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.checkDeleteGroup);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.checkAligment);
            this.Controls.Add(this.comboFonts);
            this.Controls.Add(this.labelFont);
            this.Controls.Add(this.buttonCancel);
            this.HelpButton = true;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Settings";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Создание эскизов арматуры";
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.Settings_HelpButtonClicked);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelFont;
        private System.Windows.Forms.ComboBox comboFonts;
        private System.Windows.Forms.ComboBox comboRebar;
        private System.Windows.Forms.CheckBox checkUseExistTag;
        private System.Windows.Forms.CheckBox checkAligment;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkDeleteGroup;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonUpdate;
        private System.Windows.Forms.ComboBox comboLines;
        private System.Windows.Forms.Label labelLines;
        private System.Windows.Forms.Button buttonUpdateView;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textScale;
        private System.Windows.Forms.Label labelScale;
        private System.Windows.Forms.Label labelScale0;
        private System.Windows.Forms.CheckBox checkBoxLength;
    }
}