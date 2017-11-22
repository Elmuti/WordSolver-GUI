using System;
using System.ComponentModel;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml.Linq;
using System.Diagnostics;
using System.Text;

namespace WordSolver_GUI
{
    public class WorkerInput
    {
        public string InputString;
        public string Language;
        public int MinChars;
        public int MaxChars;
        public bool MustContainAll;

        public WorkerInput(string i, string l, int mm, int m, bool mca)
        {
            InputString = i;
            Language = l;
            MinChars = mm;
            MaxChars = m;
            MustContainAll = mca;
        }
    }

    public class WorkerResult
    {
        public Dictionary<string, int> Matches;
        public float TimeTaken;


        public WorkerResult(Dictionary<string, int> matches, float timeTaken)
        {
            Matches = matches;
            TimeTaken = timeTaken;
        }
    }

    public class WorkerThread
    {
        private BackgroundWorker m_Worker;

        private static StreamReader GenerateStreamFromString(string s)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(s);
            MemoryStream stream = new MemoryStream(byteArray);
            StreamReader reader = new StreamReader(stream);
            return reader;
        }


        public static List<string> GetWordsFromList(string language, int minLength, int maxLength)
        {
            string file = "";

            switch (language)
            {
                case "English":
                    file = Properties.Resources.English;
                    break;
                case "German":
                    file = Properties.Resources.German;
                    break;
                case "Finnish":
                    file = Properties.Resources.Finnish;
                    break;
                case "Spanish":
                    file = Properties.Resources.Spanish;
                    break;
                default:
                    break;
            }

            List<string> list = new List<string>();
            foreach (XElement ele in XElement.Load(GenerateStreamFromString(file)).Elements("st"))
            {
                string word = ele.Element("s").Value;
                if (word.Length <= maxLength && word.Length >= minLength)
                    list.Add(word);
            }
            return list;
        }

        /// <summary>
        /// Gets words matching dictionary, enforces character limit
        /// </summary>
        /// <param name="letters"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public Dictionary<string, int> GetWordsMatchingDictionary(string letters, List<string> dictionary)
        {
            Dictionary<string, int> list = new Dictionary<string, int>();
            int dcount = dictionary.Count;
            int i = 0;
            bool correct = false;
            string original = letters;
            int totalLetters = letters.Length;

            foreach (string m in dictionary)
            {
                letters = original;
                for (int f = 0; f < m.Length; f++)
                {

                    correct = false;
                    if (totalLetters < m.Length)
                    {
                        break;
                    }
                    if (letters.Contains(m[f]))
                    {
                        letters = letters.Remove(letters.IndexOf(m[f]), 1);
                    }
                    else
                    {
                        break;
                    }
                    correct = true;
                }
                if (correct)
                {
                    if (!list.ContainsKey(m)) //avoid duplicates
                    {
                        list.Add(m, 1);
                    }
                }

                float frac = ((float)i / (float)dcount);
                m_Worker.ReportProgress((int)(frac * 100.0f));
                i++;
            }

            m_Worker.ReportProgress(100);
            return list;
        }

        /// <summary>
        /// Gets words matching dictionary, doesnt enforce character limit
        /// </summary>
        /// <param name="letters"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public Dictionary<string, int> GetWordsMatchingDictionary(char[] letters, List<string> dictionary)
        {
            Dictionary<string, int> list = new Dictionary<string, int>();
            int dcount = dictionary.Count;
            int i = 0;
            foreach(string s in dictionary)
            {
                bool correct = true;
                foreach (char c in letters)
                {
                    if (!s.Contains(c))
                        correct = false;
                }

                if (correct)
                    list[s] = 1;


                float frac = ((float)i / (float)dcount);
                m_Worker.ReportProgress((int)(frac * 100.0f));
            }
            return list;
        }

        public Dictionary<string, int> GenerateWords(string letters, string language, int minLetters, int maxLetters, bool wordMustContainAllLetters)
        {
            List<string> wordlist = GetWordsFromList(language, minLetters, maxLetters);
            Dictionary<string, int> matches;

            if (!wordMustContainAllLetters)
            {
                matches = GetWordsMatchingDictionary(letters.ToCharArray(), wordlist);
            }
            else
            {
                matches = GetWordsMatchingDictionary(letters, wordlist);
            }

            return matches;
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            if (e != DoWorkEventArgs.Empty)
            {
                WorkerInput input = e.Argument as WorkerInput;
                Dictionary<string, int> matches = GenerateWords(input.InputString, input.Language, input.MinChars, input.MaxChars, input.MustContainAll);
                e.Result = new WorkerResult(matches, 0.0f);
            }
        }

        public WorkerThread(Form1 f)
        {
            m_Worker = f.BackgroundWorker;
            m_Worker.DoWork += new DoWorkEventHandler(bw_DoWork);
            m_Worker.ProgressChanged += new ProgressChangedEventHandler(f.bw_ProgressChanged);
            m_Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(f.bw_RunWorkerCompleted);
        }
    }

}
