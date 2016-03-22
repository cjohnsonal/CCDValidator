using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace CCD_Validator
{
    public partial class Form1 : Form
    {
        #region Variables
        private static int count = 0, len = 0;
        private static Timer timer = new Timer();
        private TreeNodeCollection coll;
        private string[] fnarr;
        private ProgressDialog pd;
        #endregion

        #region Form Init
        public Form1()
        {
            InitializeComponent();

            timer.Interval = 5;
            timer.Tick += new EventHandler(timeEvent);
            treeView1.KeyDown += treeView1_KeyDown;
        }
        #endregion

        #region BackgroundWorker Methods
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            for (int i = 0; i < len; i++)
            {
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }
                ValidateCCD(fnarr[i]);
                worker.ReportProgress(i + 1);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pd.BringToFront();
            pd.SetProgressBar(e.ProgressPercentage);
            pd.SetProgressLabel(e.ProgressPercentage + " / " + len);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            foreach (TreeNode node in coll)
                treeView1.Nodes.Add(node);
            pd.Close();
        }
        #endregion

        #region UI Events
        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            treeView1.CollapseAll();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
            pd.Close();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            ValidateCCDs(openFileDialog1.FileNames);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            count = 0;
            timer.Enabled = true;
        }
        private void treeFalse_CheckedChanged(object sender, EventArgs e)
        {
            if (treeFalse.Checked)
                treeTrue.Checked = false;
        }

        private void treeTrue_CheckedChanged(object sender, EventArgs e)
        {
            if (treeTrue.Checked)
                treeFalse.Checked = false;
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.C))
            {
                if (treeView1.SelectedNode != null)
                {
                    Clipboard.SetText(treeView1.SelectedNode.Text);
                }
                e.SuppressKeyPress = true;
            }
        }
#endregion

        #region Filter Timer
        private void timeEvent(Object obj, EventArgs args)
        {
            if (count++ == 100)
            {
                string text = textBox2.Text;
                if (text != "")
                    treeView1.CollapseAll();
                foreach (string t in text.Split(';'))
                    if (!String.IsNullOrWhiteSpace(t))
                        FilterTree(treeView1.Nodes, t.ToLower().Trim());

                timer.Enabled = false;
            }
        }
        #endregion

        #region Data Manipulation
        private void BuildXML(string filename)
        {
            coll.Add(Helper.ToTreeNode(filename, filename));
        }

        private void FilterTree(TreeNodeCollection parent, string t)
        {
            foreach (TreeNode node in parent)
            {
                if (node.Text.ToLower().Contains(t))
                    node.EnsureVisible();
                if (node.Nodes.Count > 0)
                    FilterTree(node.Nodes, t);
            }
        }
        

        private void ValidateCCD(string fileName)
        {
            TextReader reader = null;
            bool qrda = false;
            try
            {
                XmlRootAttribute xRoot = new XmlRootAttribute();
                xRoot.ElementName = "ClinicalDocument";
                xRoot.IsNullable = true;
                xRoot.Namespace = "urn:hl7-org:v3";

                XmlSerializer deserializer = new XmlSerializer(typeof(ClinicalDocument), xRoot);
                reader = new StreamReader(fileName);
                ClinicalDocument document = (ClinicalDocument)deserializer.Deserialize(reader);
                qrda = document.title.ToLower().Contains("qrda") || document.title.ToLower().Contains("quality");
                Console.WriteLine("File is valid " + (qrda ? "QRDA" : "CCD"));

                reader.Close();

                if (treeTrue.Checked)
                    BuildXML(qrda ? "QRDA: " + fileName : fileName);
                else
                    coll.Add(String.Format((qrda ? "QRDA " : "") + "{0} is valid!", Helper.noPath(fileName)));
            }
            catch (Exception e)
            {
                reader.Close();
                Console.WriteLine("File is not valid!");
                Console.WriteLine(e.StackTrace);
                TreeNode exceptionNode = new TreeNode(String.Format((qrda ? "QRDA " : "") + "{0}: {1} - {2}", Helper.noPath(fileName), e.Message, e.InnerException != null ? e.InnerException.Message : ""));
                exceptionNode.ForeColor = Color.Red;
                coll.Add(exceptionNode);
            }
        }

        private void ValidateCCDs(string[] fileNames)
        {
            openFileDialog1.Dispose();

            var comparer = OrderedComparer.Create(
                ProjectionComparer.Create((TreeNode tn) => tn.Text),
                ProjectionComparer.Create((TreeNode tn) => tn.Text)
            );

            len = fileNames.Length;
            textBox1.Text = String.Join(" ", fileNames);

            fnarr = fileNames;
            coll = new TreeNode().Nodes;

            if (!backgroundWorker1.IsBusy)
            {
                pd = new ProgressDialog();
                pd.SetMax(len);
                pd.Cancelled += new EventHandler<EventArgs>(buttonCancel_Click);
                pd.Show();
                backgroundWorker1.RunWorkerAsync();
            }

            treeView1.TreeViewNodeSorter = comparer;
        }
        #endregion
    }
}