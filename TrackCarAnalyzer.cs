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
    public partial class TrackCarAnalyzer : Form
    {
        public List<LapDetail> laps = new List<LapDetail>();
        public TrackCarAnalyzer()
        {
            InitializeComponent();
            GetTracks();
            CarsListBox.Hide();
            RacePannel.Hide();
            QualPannel.Hide();
            pnlSessionStats.Hide();
            FuelPannel.Hide();
            this.Height = 164;
            this.Width = 465;
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
        public class sessionDetails
        {
            public string TrackName { get; set; }
            public string CarName { get; set; }
            public long SessionStartTime { get; set; }
            public double StartTrackTemp { get; set; }
            public double StartAirTemp { get; set; }
        }
        public class StintLaps
        {

            public LapDetail[] Laps { get; set; } = default!;
        }


        public void GetTracks()
        {
            string[] subdirs = Directory.GetDirectories("Sessions");
            string session_info_path = "";
            string json = "";
            List<string> Tracks = new List<string>();
            foreach (string subdir in subdirs)
            {
                session_info_path = subdir + "/SessionInfo.json";
                json = File.ReadAllText(session_info_path);
                sessionDetails ses_details = JsonConvert.DeserializeObject<sessionDetails>(json);
                if (!Tracks.Contains(ses_details.TrackName))
                {
                    Tracks.Add(ses_details.TrackName);
                }
            }
            Tracks.Reverse();
            PopulateTrackList(Tracks);
        }
        public void GetCars(String Track)
        {
            string[] subdirs = Directory.GetDirectories("Sessions");
            string session_info_path = "";
            string json = "";
            List<string> Cars = new List<string>();
            foreach (string subdir in subdirs)
            {
                session_info_path = subdir + "/SessionInfo.json";
                json = File.ReadAllText(session_info_path);
                sessionDetails ses_details = JsonConvert.DeserializeObject<sessionDetails>(json);
                if (ses_details.TrackName == Track)
                {
                    if (!Cars.Contains(ses_details.CarName))
                    {
                        Cars.Add(ses_details.CarName);
                    }
                }
            }
            Cars.Reverse();
            PopulateCarList(Cars);
        }
        public void PopulateTrackList(List<string> Tracks)
        {
            foreach (String Track in Tracks)
            {
                TrackListBox.Items.Add(Track);
            }
        }
        public void PopulateCarList(List<string> Cars)
        {
            CarsListBox.Show();
            foreach (String Car in Cars)
            {
                CarsListBox.Items.Add(Car);
            }
        }
        private void TrackListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            String track = TrackListBox.GetItemText(TrackListBox.SelectedItem);
            if(CarsListBox.GetItemText(CarsListBox.SelectedItem) != "")
            {
                CarsListBox.Items.Clear();
                GetCars(track);
                DataPullMaster(track, CarsListBox.GetItemText(CarsListBox.SelectedItem));
            }
            else
            {
                GetCars(track);
            }
           
        }
        private void CarListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            String car = CarsListBox.GetItemText(CarsListBox.SelectedItem);
            String track = TrackListBox.GetItemText(TrackListBox.SelectedItem);
            DataPullMaster(track, car);

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

            c = 60 * (c / Math.Pow(10,length));
            if(d == 0)
            {
                LapFormated = Math.Round(c, 3).ToString() + "s";
            }
            else
            {
                LapFormated = d.ToString() + ":" + Math.Round(c, 3).ToString();
            }

            return LapFormated;
        }
        String MpsToKph(float topspeed)
        {
            return Math.Round((topspeed * 3.6), 3).ToString() + " KM/H";
        }
        public void GetAvg(List<double> raceLaps, List<double> fuelUsages,List <float>topSpeeds, List<double> tTemps, List<double> aTemps, double fastestLap, bool Race)
        {
            double avgCalc = 0;
            double operatingWindow;
            int lapCount = 0;
            int outSideWindowCount = 0;
            operatingWindow = fastestLap * 1.1;

            float topSpeedAvg = 0;
            double fuelAvg = 0;
            int fullLapcount = 0;

            double aTempCalc = 0;
            double tTempCalc = 0;
            int tempCounts = 0;
            foreach(double aTemp in aTemps)
            {
                aTempCalc = aTempCalc + aTemp;
                tempCounts++;
            }
            foreach (double tTemp in tTemps)
            {
                tTempCalc = tTempCalc + tTemp;
            }
            aTempCalc /= tempCounts;
            tTempCalc /= tempCounts;
            foreach (double lap in raceLaps)
            {
              
                if(lap < operatingWindow && lap > 0)
                {
                    fuelAvg = fuelAvg + fuelUsages[fullLapcount];
                    topSpeedAvg = topSpeedAvg + topSpeeds[fullLapcount];
                    avgCalc = avgCalc + lap;
                    lapCount++;
                }
                else
                {
                    outSideWindowCount++;
                }
                fullLapcount++;
            }
            avgCalc /= lapCount;
            fuelAvg /= lapCount;
            topSpeedAvg /= lapCount;
            double fulltankavg = 100 / fuelAvg;


            lbl100lAvg.Text = Math.Round(fulltankavg, 0).ToString();
            lblAvgFuelUse.Text = RoundAndReturn(fuelAvg) + "L";
            if (Race)
            {
                lblAvgRace.Text = FormatLaptime(avgCalc);
                lblTotalRaceLaps.Text = lapCount.ToString();
                lblOffPaceRace.Text = outSideWindowCount.ToString();

                lblAvgAirTemp.Text = RoundAndReturn(aTempCalc) + "C";
                lblAvgTrackTemp.Text = RoundAndReturn(tTempCalc) + "C";

                lblAvgTopSpeed.Text = MpsToKph(topSpeedAvg);
            }
            else
            {
                lblAvgQualLap.Text = FormatLaptime(avgCalc);
                lblTotalQualLaps.Text = lapCount.ToString();
                lblQualOffPace.Text = outSideWindowCount.ToString();

                lblAvgQATemp.Text = RoundAndReturn(aTempCalc) + "C";
                lblAvgQTTemp.Text = RoundAndReturn(tTempCalc) + "C";
                
                lblTopSpeedQual.Text = MpsToKph(topSpeedAvg)  ;
            }
           
        }
        String RoundAndReturn(double number)
        {
            String returnS = Math.Round(number,3).ToString();
            return returnS;
        }
        public void DataPullMaster(String track, String car)
        {
            RacePannel.Hide();
            QualPannel.Hide();
            FuelPannel.Hide();
            bool RaceData = false;
            bool QualData = false;

            List<double> fuelUsages = new List<double>();

            List<float> raceTopSpeeds = new List<float>();
            List<float> qualTopSpeeds = new List<float>();

            List<double> raceLaps = new List<double>();
            List<double> racetTemps = new List<double>();
            List<double> raceaTemps = new List<double>();

            List<double> qualLaps = new List<double>();
            List<double> qualtTemps = new List<double>();
            List<double> qualaTemps = new List<double>();

            int TotalSessions = 0, TotalStints = 0, RaceStints = 0, QualStints = 0;
            double FastestRaceLap = 99999999, RacelapFuelRem = 0 , RaceLapFuelUsed= 0, RaceFastestLapATemp = 0, RaceFastestLapTTemp = 0;
            int FastestRaceLapNumber = 0;
            double FastestQualLap = 99999999, QuallapFuelRem = 0, QualLapFuelUsed = 0, QualFastestLapATemp = 0, QualFastestLapTTemp = 0;
            int FastestQualLapNumber = 0;

            string[] subdirs = Directory.GetDirectories("Sessions");
            string session_info_path = "";
            string json = "",json2 = "";
            foreach (string subdir in subdirs)
            {
                session_info_path = subdir + "/SessionInfo.json";
                json = File.ReadAllText(session_info_path);
                sessionDetails ses_details = JsonConvert.DeserializeObject<sessionDetails>(json);
                if (ses_details.CarName == car && ses_details.TrackName == track)
                {
                    TotalSessions++;
                    string[] stints = Directory.GetFiles(subdir);
                    foreach (string stint in stints)
                    {
                        if (stint.Contains("Race") && stint.Contains("Stint"))
                        {
                            RaceData = true;
                            json2 = File.ReadAllText(stint);
                            stintDetails stint_details = JsonConvert.DeserializeObject<stintDetails>(json2);
                            if (stint_details.FastestLap <= FastestRaceLap && stint_details.FastestLap > 0)
                            {
                                FastestRaceLap = stint_details.FastestLap;
                                RaceFastestLapTTemp = stint_details.TrackTemp;
                                RaceFastestLapATemp = stint_details.AirTemp;
                                raceaTemps.Add(stint_details.AirTemp);
                                racetTemps.Add(stint_details.TrackTemp);
                            }
                        }
                        if (stint.Contains("Race") && !stint.Contains("Stint"))
                        {
                            RaceData = true;
                            RaceStints++;
                            TotalStints++;
                            json2 = File.ReadAllText(stint);
                            laps = JsonConvert.DeserializeObject<List<LapDetail>>(json2);
                            int lapcount = laps.Count();
                            int x = 0;
                            while (x < lapcount)
                            {
                                if (laps[x].lapTime > 0)
                                {
                                    raceLaps.Add(laps[x].lapTime);
                                    fuelUsages.Add(laps[x].fuelUsage);
                                    raceTopSpeeds.Add(laps[x].topspeed);



                                }
                                if (laps[x].lapTime <= FastestRaceLap && laps[x].lapTime > 0)
                                {
                                    FastestRaceLap = laps[x].lapTime;
                                    RacelapFuelRem = laps[x].fuelRemaining;
                                    RaceLapFuelUsed = laps[x].fuelUsage;
                                    FastestRaceLapNumber = x + 1;
                                }
                                x++;
                            }

                        }
                        if (stint.Contains("Qual") && stint.Contains("Stint"))
                        {
                            QualData = true;
                            json2 = File.ReadAllText(stint);
                            stintDetails stint_details = JsonConvert.DeserializeObject<stintDetails>(json2);
                            if (stint_details.FastestLap <= FastestQualLap && stint_details.FastestLap > 0)
                            {
                                FastestQualLap = stint_details.FastestLap;
                                QualFastestLapTTemp = stint_details.TrackTemp;
                                QualFastestLapATemp = stint_details.AirTemp;
                                qualaTemps.Add(stint_details.AirTemp);
                                qualtTemps.Add(stint_details.TrackTemp);

                            }

                        }
                        if (stint.Contains("Qual") && !stint.Contains("Stint"))
                        {
                            QualData = true;
                            QualStints++;
                            TotalStints++;
                            json2 = File.ReadAllText(stint);
                            laps = JsonConvert.DeserializeObject<List<LapDetail>>(json2);
                            int lapcount = laps.Count();
                            int x = 0;
                            while (x < lapcount)
                            {
                                if (laps[x].lapTime > 0)
                                {
                                    qualLaps.Add(laps[x].lapTime);
                                    fuelUsages.Add(laps[x].fuelUsage);
                                    qualTopSpeeds.Add(laps[x].topspeed);

                                }
                                if (laps[x].lapTime <= FastestQualLap && laps[x].lapTime > 0)
                                {
                                    FastestQualLap = laps[x].lapTime;
                                    QuallapFuelRem = laps[x].fuelRemaining;
                                    QualLapFuelUsed = laps[x].fuelUsage;
                                    FastestQualLapNumber = x + 1;

                                }
                                x++;
                            }

                        }

                    }
                }
            }
            if (RaceData)
            {
                GetAvg(raceLaps, fuelUsages,raceTopSpeeds, racetTemps, raceaTemps, FastestRaceLap, true);
                RacePannel.Show();

                lblFastestRaceLap.Text = FormatLaptime(FastestRaceLap);
                lblRaceATemp.Text = RoundAndReturn(RaceFastestLapATemp) + "C";
                lblRaceTTemp.Text = RoundAndReturn(RaceFastestLapTTemp) + "C";
                lblRaceFuelLvl.Text = RoundAndReturn(RacelapFuelRem) + "L";
                lblRaceFuelUse.Text = RoundAndReturn(RaceLapFuelUsed) + "L";
                lblRaceLapNum.Text = FastestRaceLapNumber.ToString();
            }
            if (QualData)
            {
                GetAvg(qualLaps, fuelUsages, qualTopSpeeds, qualtTemps, qualaTemps, FastestQualLap, false);
                QualPannel.Show();

                lblFastestQualLap.Text = FormatLaptime(FastestQualLap);
                lblQualLapNumber.Text = FastestQualLapNumber.ToString();
                lblQualtTemp.Text = RoundAndReturn(QualFastestLapTTemp) + "C";
                lblQualATemp.Text = RoundAndReturn(QualFastestLapATemp) + "C";
                lblQualFuelUse.Text = RoundAndReturn(QualLapFuelUsed) + "L";
                lblQualFuel.Text = RoundAndReturn(QuallapFuelRem) + "L";
            }
           
           
            this.Height = 725;
            this.Width = 1169;

            Form1 homeForm = (Form1)Application.OpenForms["Form1"];
            homeForm.Hide();
            pnlSessionStats.Show();
            FuelPannel.Show();

            lblTtlSessions.Text = TotalSessions.ToString();
            lblTtlStints.Text = TotalStints.ToString();
            lblRaceStints.Text = RaceStints.ToString();
            lblQualStints.Text = QualStints.ToString();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            Form Home = new Form1();
            Home.Show();
            this.Hide();
        }

      
    }
}
