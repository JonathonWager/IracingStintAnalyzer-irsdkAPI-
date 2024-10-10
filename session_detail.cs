using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace iracingDataIntake
{
    public partial class session_detail : Form
    {
        public string gbl_session_id;
        public int SelectionCount = 0;

        public session_detail(String session_id)
        {
            InitializeComponent();
            load_stints(session_id);
            gbl_session_id = session_id;
        }
        public class stintDetails
        {

            public double TrackTemp { get; set; }

            public double AirTemp { get; set; }

            public double StartingFuel { get; set; }

            public string TrackName { get; set; }

            public string CarName { get; set; }

            public long SessionTime { get; set; }

            public long LapCount { get; set; }

            public string CurrentTime { get; set; }


            public double FastestLap { get; set; }
        }
        public string FormatSesTime(long sesTime)
        {
            string timehour, timemin;
            double timeMath;
            decimal d;
            d = decimal.Parse((sesTime / 60.0000 / 60.00000).ToString().Split('.')[1].Substring(0, 3));
            timeMath = (sesTime / 60.0000 / 60.00000);
            if (timeMath < 12)
            {
                timehour = timeMath.ToString().Split('.')[0];
                timemin = Math.Round(((d / 1000) * 60), 2).ToString() + " AM";
            }
            else
            {
                if (timeMath < 13)
                {
                    timehour = timeMath.ToString().Split('.')[0];
                }
                else
                {
                    timehour = (timeMath - 12).ToString().Split('.')[0];
                }

                timemin = Math.Round(((d / 1000) * 60), 0).ToString() + " PM";
            }
            return timehour + ":" + timemin;
        }
        public string FormatLaptime(double laptime)
        {
            String LapFormated;
            double x = laptime / 60.00;
            int d = (int)x;
            double c = x - d;

            while (c - Math.Floor(c) > 0.0)
            { c *= 10; }
            int length = (int)Math.Floor(Math.Log10(c) + 1);

            c = 60 * (c / Math.Pow(10, length));
            if (d == 0)
            {
                LapFormated = Math.Round(c, 3).ToString() + "s";
            }
            else
            {
                LapFormated = d.ToString() + ":" + Math.Round(c, 3).ToString();
            }

            return LapFormated;
        }
        String RoundAndReturn(double number)
        {
            String returnS = Math.Round(number, 3).ToString();
            return returnS;
        }
        public void load_stints(String session_id)
        {

            ListViewItem item = new ListViewItem(new[] { "Count", "Time", "Type", "LapCount", "FastestTime", "starttime", "tracktemp" , "airtemp" , "fuel"});
            string[] subdirs = Directory.GetFiles(session_id);
            int totalStints = subdirs.Count()-1;
            string test;
            int counter = 0;
            int mhour;
            string hour, min;
            foreach (string subdir in subdirs)
            {
                counter++;
               

                string output  = subdir.Substring(subdir.IndexOf('\\') + 1);
                if (!output.Contains("Stint"))
                {
                   // StintView.Items.Add(output.Substring(output.IndexOf('\\') + 1));
                }
                else if (!output.Contains("Session"))
                {
                    string json = File.ReadAllText(subdir);
                    stintDetails stintDetails = JsonConvert.DeserializeObject<stintDetails>(json);
                    hour = stintDetails.CurrentTime.ToString().Split(':')[0];
                    min = stintDetails.CurrentTime.ToString().Split(':')[1];
                    
                    mhour = int.Parse(hour);
                    if(mhour >= 13)
                    {
                        mhour = mhour - 12;
                        min = ":" + min + "PM";
                    }
                    else
                    {
                        if(mhour == 12)
                        {
                            min = ":" + min + "PM";
                        }
                        else
                        {
                            min = ":" + min + "AM";
                        }
                        
                    }

                    //test = new string(subdir.Split("-")[1].Where(c => char.IsDigit(c)).ToArray());
                    item = new ListViewItem(new[]
                        {
                        "[------->]",
                        new string(subdir.Split('\\')[2].Where(c => char.IsDigit(c)).ToArray()),
                        mhour.ToString() + min,
                        output.Substring(output.IndexOf('\\') + 1).Substring(0,4),
                        stintDetails.LapCount.ToString(),
                        FormatLaptime(stintDetails.FastestLap),
                        FormatSesTime(stintDetails.SessionTime),
                        RoundAndReturn(stintDetails.TrackTemp) + "C",
                        RoundAndReturn(stintDetails.AirTemp) + "C",
                        RoundAndReturn(stintDetails.StartingFuel) + "L",
                        }) ;
                    StintView.Items.Add(item);
                }
            }
            StintView.View = View.List;
            StintView.View = View.Details;
        }
        public ListView generateStintView()
        {
            ListView ListView1;
            ListView1 = StintView;
            ListView1.Location = new System.Drawing.Point(12, 12);
            ListView1.FullRowSelect = true;
            ListView1.Size = new System.Drawing.Size(245, 200);
            Controls.Add(ListView1);
            return ListView1;

        }
        string getStintUrls(int index)
        {
            string[] subdirs = Directory.GetFiles(gbl_session_id);
            int count = 0;
            int total = (subdirs.Length - 1);
            string url = "";
            foreach (string subdir in subdirs)
            {
                if(count  == index * 2)
                {
                    url = subdir + ":";
                }
                if(count  == index*2 + 1)
                {
                    url = url + subdir;
                }
                count++;
            }
            return url;
        }
        private void StintView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (StintView.SelectedItems.Count == 0)
                return;
            else
            {
                    

                    int url = StintView.SelectedItems[0].Index;


                    string selected_key = getStintUrls(url).Split(":")[0];
                    string session_key = getStintUrls(url).Split(":")[1];

                    Form stintView = new Stint(selected_key, session_key);

                    stintView.Show();

                    stintView.Focus();
                    stintView.Activate();
                    stintView.BringToFront();

                
            }
          



        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form Form1 = new Form1();
            Form1.Show();
            
        }
    }
}
