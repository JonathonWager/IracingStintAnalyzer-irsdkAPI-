using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Newtonsoft.Json;

namespace iracingDataIntake
{
    public partial class Stint : Form
    {
        public List<LapDetail> laps = new List<LapDetail>();

        public Stint(string stint_id, string session_details)
        {
            InitializeComponent();
            readJsonData(stint_id);
            readSessionDetails(session_details);
        }
        
        public class LapDetail
        {
            public int lapNumber { get; set; }
            public float lapTime { get; set; }
            public float fuelUsage { get; set; }
            public float fuelRemaining { get; set; }
            public int lapRemaining { get; set; }
            public float topspeed { get; set; }

            public LapDetail()
            {
                lapNumber = 0;
                lapTime = 0;
                fuelUsage = 0;
                fuelRemaining = 0;
                lapRemaining = 0;
                topspeed = 0;
            }
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
        public class StintLaps
        {
            
            public LapDetail[] Laps { get; set; } = default!;
        }


        public string FormatLaptime(double laptime)
        {
            if(laptime < 0)
            {
                return "INVALID";
            }
            else
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
           
        }
        String RoundAndReturn(double number)
        {
            String returnS = Math.Round(number, 3).ToString();
            return returnS;
        }
        String MpsToKph(float topspeed)
        {
            return Math.Round((topspeed * 3.6),3).ToString() + " KM/H";
        }
        public void readJsonData(string stint_id)
        {
            using (StreamReader r = new StreamReader(stint_id))
            {
                string json = r.ReadToEnd();
                laps= JsonConvert.DeserializeObject<List<LapDetail>>(json);
            }
            ListViewItem item = new ListViewItem(new[] { "1", "car", "track", "stintcount", "starttime", "starttemp", "startatemp" });
            int lapcount = laps.Count();
            int x = 0;
            int y = 0;
            while (x < lapcount)
            {
                item = new ListViewItem(new[] { laps[x].lapNumber.ToString(), FormatLaptime(laps[x].lapTime), RoundAndReturn(laps[x].fuelRemaining) + "L", RoundAndReturn(laps[x].fuelUsage) + "L", laps[x].lapRemaining.ToString(), MpsToKph(laps[x].topspeed) });
                LapsView.Items.Add(item);
                x++;
            }
        }
        String FormatCurTime(stintDetails details)
        {
            int mhour;
            string hour, min;
            hour = details.CurrentTime.ToString().Split(':')[0];
            min = details.CurrentTime.ToString().Split(':')[1];

            mhour = int.Parse(hour);
            if (mhour >= 13)
            {
                mhour = mhour - 12;
                min = ":" + min + "PM";
            }
            else
            {
                if (mhour == 12)
                {
                    min = ":" + min + "PM";
                }
                else
                {
                    min = ":" + min + "AM";
                }

            }
            return mhour.ToString() + min;
        }
        public void readSessionDetails(string session_id)
        {
           
            string json = File.ReadAllText(session_id);
            stintDetails ses_details = JsonConvert.DeserializeObject<stintDetails>(json);

            lblCarName.Text = ses_details.CarName;
            lblTrackName.Text = ses_details.TrackName;
            lblAirTemp.Text = "Air Temp: " + RoundAndReturn(ses_details.AirTemp) + "C";
            lblTrackTemp.Text = "Track Temp: " + RoundAndReturn(ses_details.TrackTemp) + "C";
            lblTime.Text = FormatCurTime(ses_details);
            lblFastest.Text = "Fastest Lap: " + FormatLaptime(ses_details.FastestLap);
        
    
        }

        private void stint_view_Load(object sender, EventArgs e)
        {

        }
    }
}
