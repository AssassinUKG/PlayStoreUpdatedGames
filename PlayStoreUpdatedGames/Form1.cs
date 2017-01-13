using ProtoBuf;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlayStoreUpdatedGames
{
    public partial class Form1 : Form
    {
        public string DirPath { get; set; }
        private int hour, min;
        private IScheduler sched;
        private IJobDetail iJdetail;
        private ITrigger trigger;

        private Report rp;

        public Form1()
        {
            InitializeComponent();

            hour = 10;
            min = 0;
            DateTime dt1 = DateTime.Now;
            rp= new Report();

           // rp.RaiseMessagetoUI += Rp_RaiseMessagetoUI;

            dateTimePicker1.Value = new DateTime(dt1.Year, dt1.Month, dt1.Day, hour, min, 0);

            ISchedulerFactory fact = new StdSchedulerFactory();
            sched = fact.GetScheduler();
            sched.Start();
            SetJobTime(hour, min);

            dateTimePicker1.CustomFormat = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;

           // rp.Test();
        }

        //private void Rp_RaiseMessagetoUI(object sender, MessageArgsUI e)
        //{
        //    toolStripStatusLabel2.Text = e.Message;
        //}

 




        #region "Saving/Laoding Directory Path"

        private void SetDirectoryPath(string dir)
        {
            Properties.Settings.Default.DirPath = dir;
            Properties.Settings.Default.Save();
        }

        private string LoadPath()
        {
            string path = Properties.Settings.Default.DirPath;
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }
            else { return path; }
        }

        #endregion "Saving/Laoding Directory Path"

        public void SetJobTime(int hour, int min)
        {
            JobKey jKey = new JobKey("MakeReport", "Group1");
            if (sched.CheckExists(jKey))
            {
                sched.DeleteJob(jKey);
                Debug.Print("Jkey Deleted");
            }

            Report.DirectoryPath = LoadPath();

            iJdetail = JobBuilder.Create<Report>().WithIdentity("MakeReport", "Group1").Build();

          // trigger = TriggerBuilder.Create().WithDailyTimeIntervalSchedule(s => s.WithIntervalInHours(24).StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(hour, min)).InTimeZone(TimeZoneInfo.Utc)).Build();

            trigger = TriggerBuilder.Create().WithDailyTimeIntervalSchedule(s => s.StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(hour, min))).Build();

            sched.ScheduleJob(iJdetail, trigger);
            Debug.Print("jKey Started: {0}", trigger.GetNextFireTimeUtc().ToString());
           
        }

        // Start button
        private void button2_Click(object sender, EventArgs e)
        {
            DateTime dT = dateTimePicker1.Value;

            SetJobTime(dT.Hour, dT.Minute);
        }

        //folder browse button
        private void button4_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.ShowNewFolderButton = true;
                
                fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = fbd.SelectedPath;
                    SetDirectoryPath(textBox1.Text);
                    Enable_Disable_Controls(true);
                   // Rp_RaiseMessagetoUI(this, new MessageArgsUI("Directory path set!"));
                }
            }
        }

        private void Enable_Disable_Controls(bool flag)
        {
            dateTimePicker1.Enabled = flag;
            button2.Enabled = flag;
            button3.Enabled = flag;
            button5.Enabled = flag;
            splitContainer1.Enabled = flag;
            linkLabel1.Enabled = flag;
            listBox1.Enabled = flag;
            flowLayoutPanel1.Enabled = flag;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sched != null)
            {
                sched.Shutdown(false);
            }
        }

        //force job now
        private void button5_Click(object sender, EventArgs e)
        {
            if (sched != null)
            {
                sched.TriggerJob(iJdetail.Key, iJdetail.JobDataMap);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(LoadPath()))
            {
                textBox1.Text = LoadPath();
                Enable_Disable_Controls(true);
                LoadBinFiles();
            }
            else { Enable_Disable_Controls(false); }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1) return;
            string filename = shallowFilenNameCopy[listBox1.SelectedIndex].ToString();
            Debug.Print(filename);
            LoadData(filename);
        }

        private string ImageFolderName = "images";

        private bool CheckImageFolder()
        {
            string imagePath = Path.Combine(LoadPath(), ImageFolderName);
            if (Directory.Exists(imagePath)) { return true; }
            return false;
        }

        private void DownloadImages(string[] imagelinks)
        {
            string dirPath = Path.Combine(LoadPath(), ImageFolderName);
            if (!CheckImageFolder()) { Directory.CreateDirectory(dirPath); }

            //downlaod image if not already available

            foreach (string imgLink in imagelinks)
            {
                string filename = Path.GetFileNameWithoutExtension(imgLink);
                filename = filename.Split('=')[0];
                filename = filename + ".png";
                string fullFilenamePath = Path.Combine(dirPath, filename);
                Debug.Print("FullPath: {0}", fullFilenamePath);

                if (!File.Exists(fullFilenamePath))
                {
                    //Download new file save as png
                    using (WebClient Wc = new WebClient())
                    {
                        Wc.DownloadFile(imgLink, fullFilenamePath);
                    }
                }
                else { continue; }

                Debug.Print(filename);
            }
        }

        private string IfImageExistsGivePath(string imgLink)
        {
            string filename = Path.GetFileNameWithoutExtension(imgLink);
            filename = filename.Split('=')[0];
            filename = filename + ".png";
            string dirPath = Path.Combine(LoadPath(), ImageFolderName);
            string fullFilenamePath = Path.Combine(dirPath, filename);

            if (File.Exists(fullFilenamePath))
            {
                return fullFilenamePath;
            }
            return string.Empty;
        }

        private void LoadData(string filenname)
        {
            List<AppStoreClass> asc;

            using (var file = File.OpenRead(filenname))
            {
                asc = Serializer.Deserialize<List<AppStoreClass>>(file);
            }

            flowLayoutPanel1.Controls.Clear();

            // image cache basic
            List<string> imageLinks = new List<string>();
            for (int i = 0; i < asc.Count; i++)
            {
                imageLinks.Add(asc[i].ImageLink);
            }

            Task.Run(() => { DownloadImages(imageLinks.ToArray()); }).Wait();
            Debug.Print("Added links");

            foreach (AppStoreClass ap in asc)
            {
                AppDisplay ad = new AppDisplay();
                ad.aPc = ap;

                //set image cache here
                //TODO: IMAGE CACHE
                string imgCheck = IfImageExistsGivePath(ap.ImageLink);
                if (!string.IsNullOrEmpty(imgCheck))
                {
                    ad.SetImageLocalFile(imgCheck);
                    Debug.Print("Img Loaded from file");
                }
                else
                {
                    ad.SetImage(ap.ImageLink);
                    Debug.Print("Img Loaded from URL");
                }

                ///////////

                ad.LabelText = ap.Name;
                ad.Size = new System.Drawing.Size(75, 85);
                ad.DoubleClick_Clicked += Ad_DoubleClick_Clicked;
                flowLayoutPanel1.Controls.Add(ad);
            }
        }

        private void Ad_DoubleClick_Clicked(AppStoreClass apc)
        {
            if (apc != null)
            {
                label3.Text = string.Format("Name: {0}", apc.Name);
                linkLabel2.Text = string.Format("AppStoreLink: {0}", apc.AppStoreLink);
                linkLabel2.Tag = apc.AppStoreLink;
                textBox2.Text = apc.Description;
                label4.Text = string.Format("Publish Date: {0}", apc.PublishedDate);
                label5.Text = string.Format("Current ver: {0}", string.IsNullOrEmpty(apc.CurrentVersion) ? "N/a" : apc.CurrentVersion);
                label6.Text = string.Format("Price: {0}", apc.Price == "0" ? "Free" : apc.Price);
            }
        }

        private List<string> shallowFilenNameCopy;

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (string.IsNullOrEmpty(linkLabel2.Tag.ToString())) { return; }
            if (!string.IsNullOrEmpty(linkLabel2.Tag.ToString()))
            {
                Process.Start(linkLabel2.Tag.ToString());
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LoadBinFiles();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                int pos = listBox1.SelectedIndex;
                if (File.Exists(shallowFilenNameCopy[pos].ToString()))
                {
                    File.Delete(shallowFilenNameCopy[pos].ToString());
                    shallowFilenNameCopy.RemoveAt(pos);
                    LoadBinFiles();
                }
            }
        }

        
        private void LoadBinFiles()
        {
            string dirPath = LoadPath();
            listBox1.Items.Clear();
            if (string.IsNullOrEmpty(dirPath)) return;
            if (!Directory.Exists(dirPath)) return;

            var files = Directory.GetFiles(dirPath, "*.bin");
            shallowFilenNameCopy = new List<string>();
            shallowFilenNameCopy.AddRange(files);
            foreach (string _file in files)
            {
                FileInfo fi = new FileInfo(_file);
                listBox1.Items.Add(fi.Name);
            }
        }
    }
}