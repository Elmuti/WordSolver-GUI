using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace WordSolver_GUI
{
    public partial class Form1 : Form
    {
        private Stopwatch sw = new Stopwatch();

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
            this.Size = new Size(543, 226);
        }

        private void Button_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            sw.Reset();
            sw.Start();
            bw.RunWorkerAsync(new WorkerInput(textBox1.Text, domainUpDown1.Text, (int)numericUpDown1.Value));
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Size = new Size(543, 226);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string path = saveFileDialog1.FileName;
                List<string> ls = new List<string>();
                foreach (string s in listBox1.Items)
                {
                    ls.Add(s);
                }
                File.WriteAllLines(path, ls.ToArray());
            }
        }

        public void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        public void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            sw.Stop();
            WorkerResult res = e.Result as WorkerResult;
            label5.Text = res.Matches.Count + " words found in " + (((float)sw.ElapsedMilliseconds) / 1000.0f) + " seconds";
            this.Size = new Size(543, 627);

            foreach (string match in res.Matches.Keys)
            {
                if (!listBox1.Items.Contains(match))
                    listBox1.Items.Add(match);
            }
        }
    }
}
