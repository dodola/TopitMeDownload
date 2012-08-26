using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace TopitMeDownload
{
    public partial class Form1 : Form
    {

        private Regex itemRegex = new Regex("<a\\srel=\"lightbox\"\\sid=\"item[-]tip\"\\shref=\"(?<d>.*?)\">");
        private Regex albumItemRegx;
        WebClient client = new WebClient();
        private string albumUrl;
        public Form1()
        {
            InitializeComponent();
            client.DownloadFileCompleted += client_DownloadFileCompleted;
            client.DownloadStringCompleted += client_DownloadStringCompleted;

        }

        private void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {

        }

        private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {

        }

        private string dir;
        private async void button1_Click(object sender, EventArgs e)
        {
            albumUrl = textBox1.Text;

            dir = "d:\\topitme\\" + DateTime.Now.Ticks + "\\";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string u = string.Format("{0}/item/\\d+", albumUrl.TrimEnd('/'));
            albumItemRegx = new Regex(u);
            var lists = await AnalysisAlbulm(albumUrl);
            this.progressBar1.Maximum = lists.Count;
            await DownImageItems(lists);



        }
        private async Task<List<string>> AnalysisAlbulm(string url)
        {
            var items = new List<string>();
            string downloadString = client.DownloadString(url);
            int totalPage = GetTotalPage(downloadString);


            for (int i = 1; i <= totalPage; i++)
            {
                DownloadPages(items, i);
            }

            return items;
        }
        private void DownloadPages(List<string> items, int page)
        {
            string downloadString = client.DownloadString(string.Format("{0}?p={1}", albumUrl, page));
            if (albumItemRegx != null)
                items.AddRange(from Match match in albumItemRegx.Matches(downloadString) select match.Value);
        }

        private int GetTotalPage(string content)
        {
            string u = albumUrl + "[?]p=(?<d>\\d+)";
            Regex r = new Regex(u);
            int maxPage = (from Match match in r.Matches(content) select Convert.ToInt16(match.Groups["d"].Value)).Max();
            return maxPage;
        }
        private async Task DownImageItems(List<string> items)
        {
            if (client != null)
            {
                for (int i = 0; i < items.Count; i++)
                {

                    var currentClient = new WebClient();
                    string content = await currentClient.DownloadStringTaskAsync(items[i]);

                    string itemName = items[i].Substring(items[i].LastIndexOf('/') + 1);

                    string value = itemRegex.Match(content).Groups["d"].Value;

                    await currentClient.DownloadFileTaskAsync(new Uri(value),
                                                         dir + itemName + ".png");
                    this.progressBar1.Value = i;
                }
            }


        }
    }
}
