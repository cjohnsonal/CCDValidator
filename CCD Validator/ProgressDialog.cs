using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CCD_Validator
{
    public partial class ProgressDialog : Form
    {
        public ProgressDialog()
        {
            InitializeComponent();
        }

        public void SetMax(int max)
        {
            progressBar.Maximum = max;
        }

        public void SetProgressLabel(string str)
        {
            progressLabel.Text = str;
        }

        public void SetProgressBar(int i)
        {
            progressBar.Value = i;
            progressBar.Update();
        }

        public event EventHandler<EventArgs> Cancelled;

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            EventHandler<EventArgs> ea = Cancelled;
            if (ea != null)
                ea(this, e);
        }
    }
}
