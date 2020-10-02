using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace CommonUtils
{
    public partial class Download : Form
    {
        private static Uri uri;
        private static string patch = string.Empty;
        private WebClient webClient = null;
        private bool downloaded = false;

        public Download(Uri _uri, string _patch)
        {
            InitializeComponent();
            uri = _uri;
            patch = _patch;
            CheckForIllegalCrossThreadCalls = false;
        }

        private void Download_Load(object sender, EventArgs e)
        {
            webClient = new WebClient();
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(Wc_DownloadProgressChanged);
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Wc_DownloadFileCompleted);

            if (uri != null && patch != null) {
                Thread thread = new Thread(() => {
                    webClient.DownloadFileAsync(uri, patch);
                }) { IsBackground = true };
                thread.Start();
            }
        }

        private void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Minimum = 0;
            double receive = double.Parse(e.BytesReceived.ToString());
            double total = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = receive / total * 100;
            lbStatus.Text = $"Downloaded {string.Format("{0:0.##}", percentage)}%";
            progressBar.Value = int.Parse(Math.Truncate(percentage).ToString());
        }

        private void Wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            downloaded = true;
            Close();

            if (e.Error == null) {
                Process.Start(patch);

                foreach (var process in Process.GetProcessesByName("Revit")) {
                    process.Kill();
                }
            }
            else if (e.Cancelled)
                MessageBox.Show(new Form { TopMost = true, StartPosition = FormStartPosition.CenterScreen },
                                "The update process is canceled. Please try again!", "Download Canceled",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
                MessageBox.Show(new Form { TopMost = true, StartPosition = FormStartPosition.CenterScreen },
                                "Unable to download exe. Please check your connection!", "Download failed",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void Download_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!downloaded && webClient != null)
                webClient.CancelAsync();
        }
    }
}