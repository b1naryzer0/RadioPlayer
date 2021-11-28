using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using WMPLib;

namespace RadioPlayer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        String[,] URLs =
        {
            { "Bagel 128k AAC", "https://ais-sa3.cdnstream1.com/2606_128.aac" },
            { "Bagel 128k MP3", "https://ais-sa3.cdnstream1.com/2606_128.mp3" },
            { "Bagel 64k AAC", "https://ais-sa3.cdnstream1.com/2606_64.aac" },
            { "Bagel 32k AAC", "https://ais-sa3.cdnstream1.com/2606_32.aac" }
        };

        private void Form1_Load(object sender, EventArgs e)
        {
            cbSender.Text = "Bagel 128k AAC";

            for (int i = 0; i < URLs.Length / 2; i++)
            {
                cbSender.Items.Add(URLs[i, 0]);
            }

            // wmp.settings.mute = true;
            wmp.settings.volume = 100;
            wmp.URL = URLs[1, 1];
            wmp.uiMode = "none"; // none, mini, full, invisible
            
            /*
            if (wmp.currentMedia != null)
            {
                // Display the name of the current media item. 
                // cbSender.Text = wmp.currentMedia.name; // KLAPPT NICHT

                // DAS HIER KLAPPT, ABER LOGISCHERWEISE NUR GENAU EINMAL
                // Task<string> task = GetMetaDataFromIceCastStream("https://ais-sa3.cdnstream1.com/2606_128.aac");
            }
            */
        }

        private void cbSender_SelectedIndexChanged(object sender, EventArgs e)
        {
            wmp.URL = URLs[cbSender.SelectedIndex, 1];
        }

        /*
        private void ni1_BalloonTipClicked(object sender, EventArgs e)
        {
            this.Show();
        }

        //Wenn das Fenster geschlossen wird, wird nachgefragt, ob der Player im Hintergrund laufen soll. 
        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Soll das Programm im Hintergrund ausgeführt werden?", "Radioplayer", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    this.Hide();
                    ni1.Visible = true;     //TrayIcon anzeigen 
                    ni1.ShowBalloonTip(1, "Webradio", "Wiedergabe erfolgt im Hintergrund", ToolTipIcon.Info);
                    e.Cancel = true;
                }
            }
        }
        */

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public async Task<String> GetMetaDataFromIceCastStream(string iceCast)
        {
            HttpClient m_httpClient = new HttpClient();

            m_httpClient.DefaultRequestHeaders.Add("Icy-MetaData", "1");
            var response = await m_httpClient.GetAsync(iceCast, HttpCompletionOption.ResponseHeadersRead);
            m_httpClient.DefaultRequestHeaders.Remove("Icy-MetaData");
            if (response.IsSuccessStatusCode)
            {
                IEnumerable<string> headerValues;
                if (response.Headers.TryGetValues("icy-metaint", out headerValues))
                {
                    string metaIntString = headerValues.First();
                    if (!string.IsNullOrEmpty(metaIntString))
                    {
                        int metadataInterval = int.Parse(metaIntString);
                        byte[] buffer = new byte[metadataInterval];
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            int numBytesRead = 0;
                            int numBytesToRead = metadataInterval;
                            do
                            {
                                int n = stream.Read(buffer, numBytesRead, 10);
                                numBytesRead += n;
                                numBytesToRead -= n;
                            } while (numBytesToRead > 0);

                            int lengthOfMetaData = stream.ReadByte();
                            int metaBytesToRead = lengthOfMetaData * 16;
                            byte[] metadataBytes = new byte[metaBytesToRead];
                            var bytesRead = await stream.ReadAsync(metadataBytes, 0, metaBytesToRead);
                            var metaDataString = System.Text.Encoding.UTF8.GetString(metadataBytes);

                            string metaFormattedString = metaDataString.Replace("StreamTitle='", "");
                            metaFormattedString = metaFormattedString.Replace("';", "");
                            cbSender.Text = metaFormattedString;
                            // string metaDataStringFormatted = metaDataString.Replace("StreamTitle='", "");
                            // return metaDataString;
                        }
                    }
                }
            }
            return "";
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            Task<string> task = GetMetaDataFromIceCastStream(wmp.currentMedia.sourceURL);
        }
    }
}
