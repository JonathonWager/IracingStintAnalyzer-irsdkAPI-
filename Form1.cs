using Newtonsoft.Json;
using System.Diagnostics;

namespace iracingDataIntake
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            loadSessions();
        }
        public class sessionDetails
        {   
            public string TrackName { get; set; }        
            public string CarName { get; set; }
            public long SessionStartTime { get; set; }
            public double StartTrackTemp { get; set; }
            public double StartAirTemp { get; set; }
        }
        public void loadSessions()
        {
            string[] subdirs = Directory.GetDirectories("Sessions");
            ListViewItem item = new ListViewItem(new[] { "1", "car", "track", "stintcount", "starttime", "starttemp", "startatemp" });

            string date;
            string hour;
            int fCount;
            string json;
            string session_info_path;

            string car,track,timehour, timemin;
            double ttemp, atemp, timeMath;
            long time;
            decimal d;

            int test;
            foreach (string subdir in subdirs)
            {
                session_info_path = subdir + "/SessionInfo.json";
                
                json = File.ReadAllText(session_info_path);
                sessionDetails ses_details = JsonConvert.DeserializeObject<sessionDetails>(json);
                date = subdir.Substring(9, 16);
                hour = date.Substring(11, 2);
                if(int.Parse(hour) >= 12)
                {
                    if(int.Parse(hour) == 12)
                    {
                        hour = (int.Parse(hour)).ToString() + ":" + date.Substring(14, 2) + "PM";
                    }
                    else
                    {
                        hour = (int.Parse(hour) - 12).ToString() + ":" + date.Substring(14, 2) + "PM";
                    }
                   
                    
                }
                else
                {
                    hour = hour + ":" + date.Substring(14, 2) + "AM";
                }
                date = date.Substring(0, 10);
                fCount = (Directory.GetFiles(subdir, "*", SearchOption.AllDirectories).Length-1)/2;

                car = ses_details.CarName;
                track = ses_details.TrackName;
                time = ses_details.SessionStartTime;
                d = decimal.Parse((time / 60.0000 / 60.00000).ToString().Split('.')[1].Substring(0, 3));
                timeMath = (time / 60.0000 / 60.00000);
                if (timeMath <12)
                {
                    timehour = timeMath.ToString().Split('.')[0] ;
                    timemin = Math.Round(((d / 1000) * 60), 2).ToString() + " AM";
                }
                else
                {
                    if(timeMath < 13)
                    {
                        timehour = timeMath.ToString().Split('.')[0];
                    }
                    else
                    {
                        timehour = (timeMath-12).ToString().Split('.')[0];
                    }
                   
                    timemin = Math.Round(((d / 1000) * 60), 0).ToString() + " PM";
                }
                
                ttemp = Math.Round(ses_details.StartTrackTemp, 2);
                atemp = Math.Round(ses_details.StartAirTemp, 2);
                item = new ListViewItem(new[] { "[----------->]", date + "    " + hour, track , car, fCount.ToString(), timehour + ":" + timemin , ttemp.ToString() + "C", atemp.ToString() + "C" });
                
                sessions_view.Items.Add(item);
            }
            sessions_view.Sorting = SortOrder.Descending;
            sessions_view.View = View.List;
            sessions_view.View = View.Details;
        }
        public ListView generateSessionsView()
        {
            ListView ListView1;
            ListView1 = sessions_view;
            ListView1.Location = new System.Drawing.Point(12, 40);
            ListView1.FullRowSelect = true;
            ListView1.View = View.Details;
            ListView1.Size = new System.Drawing.Size(600, 200);
            Controls.Add(ListView1);
            return ListView1;

        }
        string getSessionUrl(int key)
        {
            
            string[] subdirs = Directory.GetDirectories("Sessions");
            int total = (subdirs.Length - 1);
            key = total - key;
            int count = 0;
            foreach (string subdir in subdirs)
            {
                if (count == key)
                {
                    return subdir;
                }
                count++;
            }
            return null;
        }
        private void sessions_view_SelectedIndexChanged(object sender, EventArgs e)
        {
            int url = sessions_view.SelectedItems[0].Index;
          
            Form session_detail = new session_detail(getSessionUrl(url));

            session_detail.Show();
            this.Hide();

        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            sessions_view.Items.Clear();
            loadSessions();
            
        }

        private void btnStartPy_Click(object sender, EventArgs e)
        {
            string strCmdText;
            string file;
            file = "IRJsonList.py";
            strCmdText = "/c py " + file;
            Process.Start("CMD.exe", strCmdText);
         
        }

        private void btnAnalytics_Click(object sender, EventArgs e)
        {
            Form Analytics = new TrackCarAnalyzer();
            Analytics.Show();
        }
    }
}