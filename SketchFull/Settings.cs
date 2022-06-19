using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SketchFull
{
    public partial class Settings : Form
    {
        public string separator;
        public bool IsPutSep = false;
        dataManager m_data = new dataManager();
        public Settings(dataManager data)
        {
            m_data = data;
            InitializeComponent();            

            System.Globalization.NumberFormatInfo formatInfo = System.Globalization.NumberFormatInfo.CurrentInfo;                        
            separator = formatInfo.NumberDecimalSeparator;
           

            this.Text = SketchFull.Resourses.Strings.Texts.Title3;
            this.groupBox1.Text = SketchFull.Resourses.Strings.Texts.NameBox1;
            this.checkUseExistTag.Text = SketchFull.Resourses.Strings.Texts.Label1;
            this.labelFont.Text = SketchFull.Resourses.Strings.Texts.Font;
            this.labelLines.Text = SketchFull.Resourses.Strings.Texts.Lines;
            this.checkAligment.Text = SketchFull.Resourses.Strings.Texts.Aligment;
            this.checkDeleteGroup.Text = SketchFull.Resourses.Strings.Texts.DeleteGroup;
            this.buttonCancel.Text = SketchFull.Resourses.Strings.Texts.Cancel;
            this.buttonUpdate.Text = SketchFull.Resourses.Strings.Texts.Update;
            this.buttonUpdateView.Text = SketchFull.Resourses.Strings.Texts.UpdateView;
            this.labelScale.Text = SketchFull.Resourses.Strings.Texts.LabelScale;
            this.labelScale0.Text = SketchFull.Resourses.Strings.Texts.LabelScale0;
            this.checkBoxLength.Text = SketchFull.Resourses.Strings.Texts.TotalLength;
            this.textScale.Text = data.Scale.ToString();
            this.checkBoxLength.Checked = data.IsTotalLength;


            if (m_data.Tags.Count == 0)
            {
                checkUseExistTag.Enabled = false;
                m_data.UsePrefix = false;
            }


            checkDeleteGroup.Checked = m_data.Clear;

            foreach (Autodesk.Revit.DB.TextNoteType tnt in m_data.Fonts)
            {
                comboFonts.Items.Add(tnt.Name);
            }

            comboFonts.SelectedIndex = m_data.Font_default;


            foreach (Autodesk.Revit.DB.GraphicsStyle tnt in m_data.Line_types)
            {
                comboLines.Items.Add(tnt.Name);
            }

            comboLines.SelectedIndex = m_data.Line_types_default;


            if (m_data.Tags.Count > 0)
            {
                foreach (Autodesk.Revit.DB.FamilySymbol tnt in m_data.Tags)
                {
                    comboRebar.Items.Add(tnt.Name);
                }

                comboRebar.SelectedIndex = m_data.Tag_default;
            }
            else
            {

                comboRebar.Enabled = false;
            }

            checkUseExistTag.Checked = m_data.UsePrefix;
            if (checkUseExistTag.Checked)
            {

                comboRebar.Enabled = true;
            }
            else
            {

                comboRebar.Enabled = false;
            }

            checkAligment.Checked = m_data.Aligment;

            
        }

        private void ComboFonts_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_data.Font_default = comboFonts.SelectedIndex;
        }

        private void ComboRebar_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_data.Tag_default = comboRebar.SelectedIndex;
        }

        private void CheckUseExistTag_CheckedChanged(object sender, EventArgs e)
        {
            if(m_data.Tags.Count==0)
            {
                m_data.UsePrefix = false;
                comboRebar.Enabled = false;
            }
            if(checkUseExistTag.Checked)
            {
                 
                comboRebar.Enabled = true;
                m_data.UsePrefix = true;
                m_data.Tag_default = comboRebar.SelectedIndex;
            }
            else
            {                 
                comboRebar.Enabled = false;
                m_data.UsePrefix = false;
                m_data.Tag_default = comboRebar.SelectedIndex;
            }
        }
 
        private void CheckAligment_CheckedChanged(object sender, EventArgs e)
        {
            m_data.Aligment = checkAligment.Checked;             
        }

        private void CheckDeleteGroup_CheckedChanged(object sender, EventArgs e)
        {
            m_data.Clear = checkDeleteGroup.Checked;
        }

        private void ComboLines_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_data.Line_types_default = comboLines.SelectedIndex;
        }

        private void Settings_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            Help.ShowHelp(this, SketchFull.Resourses.Strings.Texts.PathToHelpFile, SketchFull.Resourses.Strings.Texts.HelpSection);
        }

        private void TextScale_TextChanged(object sender, EventArgs e)
        {
            if (this.textScale.Text == "." || this.textScale.Text == ",")
            {
                this.textScale.Text = "0"+separator+"0";
                this.textScale.SelectionStart = 2;
            }
            if (this.textScale.Text.Contains("."))
            {
                this.textScale.Text = this.textScale.Text.Replace(".",separator);
                // this.textScale.SelectionStart = this.textScale.Text.IndexOf(separator) + 1;
            }
            if (this.textScale.Text.Contains(","))
            {
                this.textScale.Text = this.textScale.Text.Replace(",", separator);
                // this.textScale.SelectionStart = this.textScale.Text.IndexOf(separator) + 1;
            }

            if (this.textScale.Text.Count(x => x.ToString() == separator) > 1)
            {
                int del = this.textScale.Text.LastIndexOf(separator);
                this.textScale.Text = this.textScale.Text.Remove(del, 1);
                // this.textScale.SelectionStart = this.textScale.Text.IndexOf(separator) + 1;
            }
            if (this.textScale.Text.IndexOf(separator) == 0)
            {
                this.textScale.Text = "0" + this.textScale.Text;
                this.textScale.SelectionStart = 2;
            }
            if (this.textScale.Text == "")
            {
                this.textScale.Text = "0";
                // this.textScale.SelectionStart = 2;
            }

            // string s = this.textScale.Text.Replace(".","'");
            // m_data.Scale = Convert.ToDouble(this.textScale.Text);
            m_data.Scale = double.Parse(this.textScale.Text);
            
            if (m_data.Scale == 0)
            {
                this.textScale.SelectionStart =
                                   (this.textScale.Text.IndexOf(separator) + 1) < this.textScale.Text.Length ? this.textScale.Text.Length : this.textScale.Text.Length + 1;
            }

            if(IsPutSep)
            {
                this.textScale.SelectionStart = this.textScale.Text.IndexOf(separator) + 1;
            }
            //else
            //{
            //    if (m_data.Scale < 1)
            //        this.textScale.SelectionStart = this.textScale.Text.Length;
            //}
        }

        private void TextScale_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if ((e.KeyChar <= 47 || e.KeyChar >= 58) && number != 8 && number != 44 && number != 46)
            {
                e.Handled = true;
            }

            if (number == 44 || number == 46)
            {
                IsPutSep = true;
            }
            else
            {
                IsPutSep = false;
            }

        }

        private void CheckBoxLength_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxLength.Checked) m_data.IsTotalLength = true;
            else m_data.IsTotalLength = false;
        }
    }
}
