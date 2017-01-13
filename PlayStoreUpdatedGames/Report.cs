using ProtoBuf;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace PlayStoreUpdatedGames
{
     class Report : IJob
    {
        public static string DirectoryPath { get; set; }

        private string baseurl = "https://play.google.com/store/apps/collection/promotion_3000791_new_releases_games?authuser=0";

        public void Execute(IJobExecutionContext context)
        {
            //run parsing and saving here.
            if (string.IsNullOrEmpty(DirectoryPath)) { Debug.Print("directory not set"); return; }
            if (!Directory.Exists(DirectoryPath)) { Debug.Print("Directory not exist"); return; }
            //this.RaiseMessagetoUI -= RaiseMessagetoUI;

            SaveFileToDirectory();
            Debug.Print("*** JOB HAS RUN ***");
        }

        //public delegate void RaiseMessageEventHandler(object sender, MessageArgsUI e);

        //public event RaiseMessageEventHandler RaiseMessagetoUI;

        //public virtual void OnRaiseMessageToUI(object sender, MessageArgsUI e)
        //{
        //    // RaiseMessagetoUI?.Invoke(this,message);
        //    RaiseMessageEventHandler handler = RaiseMessagetoUI;
        //    if (handler != null)
        //    {
        //        handler(sender, e);
        //    }
            
        //}

        //public void Test()
        //{
        //    OnRaiseMessageToUI(this, new MessageArgsUI("test message"));
           
        //}


        public void SaveFileToDirectory()
        {
            //MessageArgsUI me = new MessageArgsUI("Saving file to disk...");
            //OnRaiseMessageToUI(this, me);
            Debug.Print("Dir: {0}", DirectoryPath);

            string storePage = GetStorePage();
            List<AppStoreClass> listOfApplications = new List<AppStoreClass>();

            string splitone = new Regex("<div class=\"cluster-heading\">(.*)</div>").Match(storePage).Value;

            //regex split two <div class="card no-rationale square-cover apps small"  - cuts into app sections.
            string[] appParts = splitone.Split(new string[] { "<div class=\"card no-rationale square-cover apps small\"" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in appParts)
            {
                if (!part.Contains("card no-rationale square-cover apps small"))
                {
                    continue;
                }

                //split into parts
                listOfApplications.Add(splitSections(part));
            }

            //// Save List<AppstoreClass>> to file

            using (var file = File.Create(Path.Combine(DirectoryPath, string.Format("psUpdates-{0:dd-MM-yyy-_hh-mm-ss-tt}.bin", DateTime.Now))))
            {
                Serializer.Serialize(file, listOfApplications);
                Debug.Print("Seralized file");
             //  OnRaiseMessageToUI(this, new MessageArgsUI("Save Fnished"));
            }
           
        }

        private AppStoreClass splitSections(string section)
        {
            //class="title" href="(.*?)" title="(.*?)" aria    << Gives Name and Href

            Regex R = new Regex("class=\"title\" href=\"(.*?)\" title=\"(.*?)\" aria");
            Match M = R.Match(section);

            string Name = M.Groups[2].Value;
           // OnRaiseMessageToUI(this, new MessageArgsUI(string.Format("Parsing app {0} .", Name)));
            string Href = "https://play.google.com" + M.Groups[1].Value;

            //Debug.Print("Name: {0}, Link:{1}", Name, Href);
            //Debug.Indent();
            // <div class="description">(.*?)<span     << Description
            R = new Regex("<div class=\"description\">(.*?)<span");
            Match M1 = R.Match(section);

            string imgLink;
            R = new Regex("data-cover-small=\"(.*?)\"");
            Match M2 = R.Match(section);
            imgLink = "http:" + M2.Groups[1].Value;

            string description = M1.Groups[1].Value;
           // Debug.Print("Desc: {0}", description);
            string extrainfos = GetExtraStorePage(Href);

            string publishedDate = GetDatePublished(extrainfos);
            string currVersion = GetCurrentVersion(extrainfos);

            string price = GetPrice(extrainfos);

          //  Debug.Print("PublishDate: {0}", publishedDate);
          //  Debug.Unindent();

            return new AppStoreClass() { Name = Name, AppStoreLink = Href, PublishedDate = publishedDate, Description = description, ImageLink = imgLink, CurrentVersion = currVersion, Price = price };
        }

        public string GetExtraStorePage(string link)
        {
            HttpWebRequest hr = (HttpWebRequest)WebRequest.Create(link);
            hr.UserAgent = "PublishDataInfo";
            HttpWebResponse wr = (HttpWebResponse)hr.GetResponse();
            if (wr.StatusCode == HttpStatusCode.OK)
            {
                using (StreamReader sr = new StreamReader(wr.GetResponseStream()))
                {
                    return sr.ReadToEnd();
                }
            }
            return string.Empty;
        }

        public string GetDatePublished(string page)
        {
            //Updated</div> <div class="content" itemprop="datePublished">20 July 2016</div>

            //<div class="content" itemprop="softwareVersion"> 4.6.3  </div>

            string publishDate = new Regex("<div class=\"content\" itemprop=\"datePublished\">(.*?)</div>").Match(page).Groups[1].Value;
            return publishDate;
        }

        public string GetPrice(string page)
        {
            Match M = new Regex("l\"> <meta content=\"(.*?)\" itemprop=\"price\">").Match(page);
            string price = M.Groups[1].Value;
            return !string.IsNullOrEmpty(price) ? price : string.Empty;
        }

        public string GetCurrentVersion(string page)
        {
            //Updated</div> <div class="content" itemprop="datePublished">20 July 2016</div>

            //<div class="content" itemprop="softwareVersion"> 4.6.3  </div>

            string currVersion = new Regex("<div class=\"content\" itemprop=\"softwareVersion\">(.*?)</div>").Match(page).Groups[1].Value;
            return currVersion;
        }

        public string GetStorePage()
        {
            HttpWebRequest hr = (HttpWebRequest)WebRequest.Create(baseurl);

            hr.UserAgent = "NewGamesCheck";

            HttpWebResponse resp = (HttpWebResponse)hr.GetResponse();

            if (resp.StatusCode == HttpStatusCode.OK)
            {
                using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                {
                    return sr.ReadToEnd();
                }
            }
            return string.Empty;
        }
    }
}