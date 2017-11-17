using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace WordSolver_GUI
{
    public partial class Form1 : Form
    {
        public BackgroundWorker bw = new BackgroundWorker();

        public WorkerThread workerThread { get; set; }

        public Form1()
        {
            InitializeComponent();
            bw.WorkerReportsProgress = true;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            bw.ProgressChanged += bw_ProgressChanged;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Size = new Size(416, 226);
        }

        private void Button_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            bw.RunWorkerAsync(new WorkerInput(textBox1.Text, domainUpDown1.Text, (int)numericUpDown1.Value));
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Size = new Size(416, 226);
        }

        public void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        public void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            WorkerResult res = e.Result as WorkerResult;
            label5.Text = res.Matches.Count + " words found in " + res.TimeTaken + " seconds";
            this.Size = new Size(416, 627);

            foreach (KeyValuePair<string, int> match in res.Matches)
            {
                listBox1.Items.Add(match.Key);
            }
        }
    }
}
