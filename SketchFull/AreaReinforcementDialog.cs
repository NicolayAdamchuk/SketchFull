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
    
    public partial class AreaReinforcementDialog : Form
    {
        // dataManager m_data = new dataManager();
        public AreaReinforcementDialog()
        {
            // m_data = data;
            InitializeComponent();

            this.Text = SketchFull.Resourses.Strings.Texts.Title2;
            this.groupDirection.Text = SketchFull.Resourses.Strings.Texts.Dialog1;
            this.groupLayer.Text = SketchFull.Resourses.Strings.Texts.Dialog2;
            this.radioMain.Text = SketchFull.Resourses.Strings.Texts.Dialog3;
            this.radioSecond.Text = SketchFull.Resourses.Strings.Texts.Dialog4;
            this.radioUp.Text = SketchFull.Resourses.Strings.Texts.Dialog5;
            this.radioDown.Text = SketchFull.Resourses.Strings.Texts.Dialog6;
            this.buttonCancel.Text = SketchFull.Resourses.Strings.Texts.Cancel;

            if (SketchFullApp.areaLayer==AreaLayer.Up) radioUp.Checked = true;            
            else radioDown.Checked = true;

            if (SketchFullApp.areaDirect == AreaDirect.Main)  radioMain.Checked = true;
            else radioSecond.Checked = true;
        }

        private void RadioMain_CheckedChanged(object sender, EventArgs e)
        {
            if (radioMain.Checked) SketchFullApp.areaDirect = AreaDirect.Main;
            else SketchFullApp.areaDirect = AreaDirect.Second;
        }

        private void RadioSecond_CheckedChanged(object sender, EventArgs e)
        {
            if (radioSecond.Checked) SketchFullApp.areaDirect = AreaDirect.Second;
            else SketchFullApp.areaDirect = AreaDirect.Main;
        }

        private void RadioUp_CheckedChanged(object sender, EventArgs e)
        {
            if (radioUp.Checked) SketchFullApp.areaLayer = AreaLayer.Up;
            else SketchFullApp.areaLayer = AreaLayer.Down;
        }

        private void RadioDown_CheckedChanged(object sender, EventArgs e)
        {
            if (radioDown.Checked) SketchFullApp.areaLayer = AreaLayer.Down;
            else SketchFullApp.areaLayer = AreaLayer.Up;
        }

        private void AreaReinforcementDialog_Load(object sender, EventArgs e)
        {

        }
    }
}
