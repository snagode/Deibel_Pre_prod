using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SageIntactService
{
    public partial class Service1 : ServiceBase
    {
        Timer timer = new Timer();
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            WriteToFile("Service is started at " + DateTime.Now);
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 5000; //number in milisecinds  
            timer.Enabled = true;
            Launch();
        }

        protected override void OnStop()
        {
            WriteToFile("Service is stopped at " + DateTime.Now);
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            WriteToFile("Service is recall at " + DateTime.Now);
        }
        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }

        public void Launch()
        {
            try
            {
                var path = AppDomain.CurrentDomain.BaseDirectory + "\\";
                DirectoryInfo d = new DirectoryInfo(@"E:\Movies");
                var files = d.GetFiles(path);
                var fileCount = (from file in Directory.EnumerateFiles(@"H:\iPod_Control\Music", "*.mp3", SearchOption.AllDirectories)
                                 select file).Count();
                if (files.Length == 0)
                {
                    WriteToFile($"{DateTime.Now}:There is no file to upload.");
                    return;
                }

                DoEverything(files);
            }
            catch (Exception e) { WriteToFile(e.Message + e.StackTrace); }
        }
        void DoEverything(IList<FileInfo> files)
        {
            foreach (var file in files)
            {
                using (var sr = new StreamReader(file.FullName))
                {
                    var headers = sr.ReadLine();
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (string.IsNullOrEmpty(line))
                            continue;
                        var v = line.Replace("\"", "").Split(',').ToList();
                    }
                }
            }
        }

        async static void PostRequest(string url)
        {
            IEnumerable<KeyValuePair<string, string>> queries = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("",""),
                new KeyValuePair<string, string>("","")
            };

            HttpContent q = new FormUrlEncodedContent(queries);
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.PostAsync(url,q))
                {
                    using (HttpContent content = response.Content)
                    {
                        HttpContentHeaders headers = content.Headers;
                    }
            }
            }
        
        }
        }
}
