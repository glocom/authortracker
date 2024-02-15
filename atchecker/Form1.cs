using System.Net;
using System.Numerics;
using System.Text.RegularExpressions;

namespace atchecker
{
    public partial class Form1 : Form
    {
        public List<TOTD> totds;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            //TODO put this back
            string playerUid = textBox1.Text.Trim();
            if (string.IsNullOrEmpty(playerUid))
            {
                button1.Enabled = true;
                return;
            }

            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);
            int currentDateNr = currentDate.Day;
            bool firstDateCalc = false;

            try
            {
                WebClient webClient = new WebClient();
                string page = webClient.DownloadString("https://www.author-tracker.com/player/" + playerUid);

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(page);

                var tables = doc.DocumentNode.SelectNodes("//table[@role='table']/tbody/tr");
                if (totds == null)
                {
                    totds = doc.DocumentNode.SelectNodes("//table[@role='table']/tbody/tr")
                            .Where(tr => tr.Elements("td").Count() > 1)
                            .Select(tr =>
                            {
                                TOTD newTotd = new TOTD();
                                var tds = tr.Elements("td").Select(td => td.InnerText.Trim()).ToList();

                                int daynr = int.Parse(tds[0]);
                                newTotd.Day = currentDate;
                                //Remove a Day if new TOTD hasn't released yet
                                if (daynr != currentDateNr && firstDateCalc == false)
                                {
                                    currentDate = currentDate.AddDays(-1);
                                    newTotd.Day = newTotd.Day.AddDays(-1);
                                }
                                currentDate = currentDate.AddDays(-1);

                                newTotd.MapName = Regex.Replace(tds[1], "(?i)(?<!\\$)((?<d>\\$+)\\k<d>)?((?<=\\$)(?!\\$)|(\\$([a-f\\d]{1,3}|[ionmwsztg<>]|[lhp](\\[[^\\]]+\\])?)))", "${d}");
                                newTotd.Author = Regex.Replace(tds[2], "(?i)(?<!\\$)((?<d>\\$+)\\k<d>)?((?<=\\$)(?!\\$)|(\\$([a-f\\d]{1,3}|[ionmwsztg<>]|[lhp](\\[[^\\]]+\\])?)))", "${d}");
                                newTotd.NrOfAts = int.Parse(tds[3].Replace(",", "").Replace("+", ""));
                                newTotd.AtTime = parseStringToTimeOnly(tds[4]);
                                Player player = new Player();
                                player.HasAt = false;
                                if (!string.IsNullOrEmpty(tds[5]))
                                {
                                    player.PbTime = parseStringToTimeOnly(tds[5]);

                                    player.HasAt = player.PbTime.CompareTo(newTotd.AtTime) <= 0;
                                }

                                newTotd.Players = new List<Player>
                                {
                                player
                                };

                                return newTotd;
                            }).ToList();


                    var bindingSource = new BindingSource();
                    bindingSource.DataSource = totds;

                    dataGridView1.DataSource = bindingSource;
                    dataGridView1.Columns[4].DefaultCellStyle.Format = "mm:ss:fff";

                    dataGridView1.Columns.Add("PbTime", "PbTime");
                    dataGridView1.Columns.Add(new DataGridViewCheckBoxColumn());

                    dataGridView1.Columns[dataGridView1.Columns.Count - 2].DefaultCellStyle.Format = "mm:ss:fff";
                    dataGridView1.Columns[dataGridView1.Columns.Count - 1].Name = "HasAT";

                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        dataGridView1[dataGridView1.Columns.Count - 2, i].Value = totds[i].Players[totds[i].Players.Count - 1].PbTime;
                        dataGridView1[dataGridView1.Columns.Count - 1, i].Value = totds[i].Players[totds[i].Players.Count - 1].HasAt;
                    }

                }
                else
                {
                    List<Player> playerData = doc.DocumentNode.SelectNodes("//table[@role='table']/tbody/tr")
                        .Where(tr => tr.Elements("td").Count() > 1)
                        .Select(tr =>
                        {
                            Player player = new Player();
                            player.HasAt = false;
                            var tds = tr.Elements("td").Select(td => td.InnerText.Trim()).ToList();
                            if (!string.IsNullOrEmpty(tds[5]))
                            {
                                player.PbTime = parseStringToTimeOnly(tds[5]);
                            }
                            return player;
                        }).ToList();

                    dataGridView1.Columns.Add("PbTime", "PbTime");
                    dataGridView1.Columns.Add(new DataGridViewCheckBoxColumn());

                    dataGridView1.Columns[dataGridView1.Columns.Count - 2].DefaultCellStyle.Format = "mm:ss:fff";
                    dataGridView1.Columns[dataGridView1.Columns.Count - 1].Name = "HasAT";

                    for (int i = 0; i < totds.Count; i++)
                    {
                        if (playerData[i].PbTime > TimeOnly.MinValue)
                        {
                            playerData[i].HasAt = playerData[i].PbTime.CompareTo(totds[i].AtTime) <= 0;
                        }
                        totds[i].Players.Add(playerData[i]);

                        dataGridView1[dataGridView1.Columns.Count - 2, i].Value = totds[i].Players[totds[i].Players.Count - 1].PbTime;
                        dataGridView1[dataGridView1.Columns.Count - 1, i].Value = totds[i].Players[totds[i].Players.Count - 1].HasAt;
                    }

                    for (int i = 0; i < totds.Count; i++)
                    {
                        int trueCount = 0;
                        foreach (var p in totds[i].Players)
                        {
                            if (p.HasAt)
                                trueCount++;
                        }
                        

                        if (trueCount % totds[i].Players.Count != 0)
                        {
                            dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Orange;
                        }
                    }
                }

                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    column.SortMode = DataGridViewColumnSortMode.Automatic;
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                button1.Enabled = true;
            }
        }

        public TimeOnly parseStringToTimeOnly(string tds)
        {
            tds = "00:0" + tds;
            tds = tds.Replace("*", "");
            int place = tds.LastIndexOf(":");

            if (place == -1)
                return TimeOnly.Parse(tds);

            return TimeOnly.Parse(tds.Remove(place, 1).Insert(place, "."));
        }
    }
}