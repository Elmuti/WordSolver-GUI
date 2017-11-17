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
        public int MaxChars;

        public WorkerInput(string i, string l, int m)
        {
            InputString = i;
            Language = l;
            MaxChars = m;
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
        private static List<string> words = new List<string>();
        private BackgroundWorker worker;

        private static StreamReader GenerateStreamFromString(string s)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(s);
            MemoryStream stream = new MemoryStream(byteArray);
            StreamReader reader = new StreamReader(stream);
            return reader;
        }


        public static List<string> GetWordsFromList(string language)
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
                default:
                    break;
            }

            List<string> list = new List<string>();
            foreach (XElement ele in XElement.Load(GenerateStreamFromString(file)).Elements("st"))
            {
                list.Add(ele.Element("s").Value);
            }
            return list;
        }

        private static void GetStringPermutations(string word, string currentWord, int minLength, int maxLength)
        {
            string stat = currentWord;
            for (int i = 0; i < word.Length; i++)
            {
                currentWord = stat + word.ElementAt(i);

                if (currentWord.Length >= minLength && currentWord.Length <= maxLength)
                {
                    words.Add(currentWord);
                }

                GetStringPermutations(word.Remove(i, 1), currentWord, minLength, maxLength);
            }
        }

        public Dictionary<string, int> GetWordsMatchingDictionary(List<string> words, List<string> dictionary)
        {
            Dictionary<string, int> list = new Dictionary<string, int>();
            int dcount = dictionary.Count;
            int i = 0;

            foreach (string s in words)
            {
                foreach (string m in dictionary)
                {
                    if (s == m)
                    {
                        if (list.ContainsKey(s)) //avoid duplicates
                            list[s]++;
                        else
                            list.Add(s, 1);
                    }
                }
                float frac = ((float)i / (float)dcount);
                worker.ReportProgress((int)(frac * 100.0f));
                i++;
            }

            worker.ReportProgress(100);
            return list;
        }

        public Dictionary<string, int> GenerateWords(string letters, string language, int maxLetters)
        {
            //this is a change
            words = new List<string>();

            GetStringPermutations(letters, "", 3, maxLetters);
            List<string> wordlist = GetWordsFromList(language);
            Dictionary<string, int> matches = GetWordsMatchingDictionary(words, wordlist);

            return matches;
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            if (e != DoWorkEventArgs.Empty)
            {
                WorkerInput input = e.Argument as WorkerInput;
                //Stopwatch sw = new Stopwatch();
                //sw.Start();
                Dictionary<string, int> matches = GenerateWords(input.InputString, input.Language, input.MaxChars);
                //sw.Stop();
                //float timeTaken = ((float)sw.Elapsed.Milliseconds) / 1000.0f;

                e.Result = new WorkerResult(matches, 0.0f);
            }
        }

        public WorkerThread(Form1 f)
        {
            worker = f.bw;
            worker.DoWork += new DoWorkEventHandler(bw_DoWork);
            worker.ProgressChanged += new ProgressChangedEventHandler(f.bw_ProgressChanged);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(f.bw_RunWorkerCompleted);
        }
    }

}
