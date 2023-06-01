/*
 * 
 * Emotion Sensing Platform Ver 0.1
 * Mitsutoshi Makihata Ph.D (Since 2020-3)
 * 
 * Version history:
 * 0.1b <= current
 * + Controled motion stimulation by using WAV file
 * + Long term data recording (saving file)
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Media;
using System.Threading;

using PS2000Imports;
using MDataplot;
using ECG_DSP;

//Faster sound effects.
using SharpDX.IO;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace Emotion_beat
{
    public partial class Form1 : Form
    {
        PicoECG picoeeg = new PicoECG();

        //This class should be static class!?
        ECG_DSP.ECG_DSP dsp = new ECG_DSP.ECG_DSP();
        
        //Beat generator thread
        BeatLock bll = new BeatLock();
        Thread thr;

        DATAPLOT dp_main;   //main waveform monitor
        DATAPLOT dp_lorenz; //Lorenz plot area
        DATAPLOT dp_spectrum; //FFT plot area

        //Indice for Data plot
        int _WaveIndexChA;
        int _WaveIndexChB;
        int _WaveIndexRR;
        int _WaveIndexFFT;

        List<WAVE> MainMemory = new List<WAVE>();
        WAVE FFT_buffer; //storing all measured data to process FFT.
        WAVE RR_data;

        WAVE[] last_wave = new WAVE[2];

        //File names
        string out_filename = string.Empty;
        string in_filename = string.Empty;

        Boolean _Running = false;

        //Making this form public!
        public static Form1 _Form1;

        //Enable window dragging.
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        private void Form1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        //sound play -SharpDX
        XAudio2 xaudio;
        WaveFormat waveFormat;
        AudioBuffer buffer;
        SoundStream soundstream;

        //Stopwatch to control program timings.
        Stopwatch Global_sw = new Stopwatch();

        //=======================================================

        public Form1()
        {
            InitializeComponent();
            _Form1 = this;              //make this form accessible from different classes.
        }

        public void LogAppend(string line)
        {
            textBox1.AppendText(line);
            Form1._Form1.Update();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Dataplot 
            dp_main = new DATAPLOT(pictureBox1.Width, pictureBox1.Height, "Time", "Voltage");
            dp_main.ColorSetting(this.BackColor, Color.FromArgb(0xFF, 0xFF, 0xFF));
            dp_main.drawstyle = 0; //line style
            pictureBox1.Image = dp_main.Plot();
            pictureBox1.Refresh();
           
            dp_lorenz = new DATAPLOT(pictureBox2.Width, pictureBox2.Height, "RR(n)", "RR(n+1)");
            dp_lorenz.ColorSetting(this.BackColor, Color.FromArgb(0xFF, 0xFF, 0xFF));
            dp_lorenz.drawstyle = 1; //dot style
            pictureBox2.Image = dp_lorenz.Plot();
            pictureBox2.Refresh();

            dp_spectrum = new DATAPLOT(pictureBox3.Width, pictureBox3.Height, "Frequency", "Amplitude");
            dp_spectrum.ColorSetting(this.BackColor, Color.FromArgb(0xFF, 0xFF, 0xFF));
            dp_spectrum.drawstyle = 2; //vertical line style
            pictureBox3.Image = dp_spectrum.Plot();
            pictureBox3.Refresh();

            CMBX_fmpl_unit.Items.Clear();
            foreach (String name in Enum.GetNames(typeof(PS2000_Const.FreqUnit)))
            {
                CMBX_fmpl_unit.Items.Add(name);
            }
            CMBX_fmpl_unit.SelectedIndex = (int)PS2000_Const.FreqUnit.kHz; //Default is kHz.

            // Total recording time for sigle shot streaming.
            CMBX_recordtime_unit.Items.Clear();
            foreach (String name in Enum.GetNames(typeof(PS2000_Const.TimeUnit)))
            {
                CMBX_recordtime_unit.Items.Add(name);
            }
            CMBX_recordtime_unit.SelectedIndex = (int)PS2000_Const.TimeUnit.sec;

            // Interval to refresh graph for continuous streaming mode
            CMBX_interval_unit.Items.Clear();
            foreach (String name in Enum.GetNames(typeof(PS2000_Const.TimeUnit)))
            {
                CMBX_interval_unit.Items.Add(name);
            }
            CMBX_interval_unit.SelectedIndex = (int)PS2000_Const.TimeUnit.ms;

            //Trigger channel combo box
            CMBOX_trg_ch.Items.Clear();
            foreach (String name in Enum.GetNames(typeof(PS2000_Const.TrigChannel)))
            {
                CMBOX_trg_ch.Items.Add(name);
            }
            CMBOX_trg_ch.SelectedIndex = 2; //Default is none

            //Setting range combo box.
            CMBOX_inputrange_A.Items.Clear();
            CMBOX_inputrange_B.Items.Clear();
            foreach (String name in Enum.GetNames(typeof(PS2000_Const.RangeCode)))
            {
                CMBOX_inputrange_A.Items.Add(name);
                CMBOX_inputrange_B.Items.Add(name);
            }
            CMBOX_inputrange_A.SelectedIndex = (int)PS2000_Const.RangeCode.Range_1V; //Default is 1V
            CMBOX_inputrange_B.SelectedIndex = (int)PS2000_Const.RangeCode.Range_1V;

            //Coupling setting (DC is default)
            CMBOX_A_ACDC.SelectedIndex = 1;
            CMBOX_B_ACDC.SelectedIndex = 1;

            //Trigger mode
            CMBOX_trig_direction.Items.Clear();
            foreach (String name in Enum.GetNames(typeof(PS2000_Const.TrigDirection)))
            {
                CMBOX_trig_direction.Items.Add(name);
            }
            CMBOX_trig_direction.SelectedIndex = 0; //Default is rising.
        }

        //Single-shot reading (Testing, may contain old code...)
        private void BTN_Single_clicked(object sender, EventArgs e)
        {
            //Open Picoscope
            picoeeg.InitPico();

            dp_main.AllWaveClear();

            if ((Imports.Channel)Enum.Parse(typeof(PS2000_Const.TrigChannel), CMBOX_trg_ch.Text) != Imports.Channel.None)
            {
                dp_main.SetMarker(DATAPLOT.MARKERTYPE.X, 0, Color.Green);
            }

            //update setting.
            picoeeg.setting = Forms2Setting();
            //picoeeg.setting.RecordTime = 3600;//5min

            //Setting of bufferes. (size depends on sampling rate)
            FFT_buffer = new WAVE((int)(picoeeg.setting.SampleFreqHz * picoeeg.setting.RecordTime));          //1hour memory
            RR_data = new WAVE((int)picoeeg.setting.RecordTime);                                              //~1hour memory

            //Obtain wave indice
            //Temporal data (main monitoring plot) is stored in MDPlot class.
            _WaveIndexRR = dp_lorenz.CreateWave(RR_data.Length, "RR", "RR");
            _WaveIndexFFT = dp_spectrum.CreateWave(FFT_buffer.Length / 2, "Freq", "Amp");
            _WaveIndexChA = dp_main.CreateWave((int)(picoeeg.setting.SampleFreqHz * 5), "Time", "ChA");
            _WaveIndexChB = dp_main.CreateWave((int)(picoeeg.setting.SampleFreqHz * 5), "Time", "ChB");


            int rangeA = picoeeg.setting.channelSettings[0].enabled * (int)Enum.Parse(typeof(PS2000_Const.Rangemv), CMBOX_inputrange_A.Text);
            int rangeB = picoeeg.setting.channelSettings[1].enabled * (int)Enum.Parse(typeof(PS2000_Const.Rangemv), CMBOX_inputrange_B.Text);
            dp_main.Y_end = rangeA > rangeB ? rangeA : rangeB;
            dp_main.Y_start = -1 * dp_main.Y_end;
            dp_main.X_end = 5;
            dp_main.X_start = 0;
            //dp.FittingView(true, false);

            picoeeg.StartStreaming();

            //data recording and graphing without timer interrupt.
            while (picoeeg.CurrentTimeStamp < picoeeg.setting.RecordTime)
            {
                Boolean monitorrefresh = false;
                //Receive available data in streaming buffer of Picoscope.
                WAVE[] results = picoeeg.ReveiveAvailableData();
                if (results.Length > 0)
                {
                    for (int i = 0; i < results[0].ValidLength; i++) //datapoints
                    {
                        for (int j = 0; j < results.Length; j++) //channel
                        {
                            if (results[j][i].X <= picoeeg.setting.RecordTime)
                            {
                                dp_main.AddPoint(_WaveIndexChA + j, results[j][i].X, results[j][i].Y);
                            }
                            if (results[j][i].X % 5 == 0 && results[j][i].X > 0) //detect interval end.
                            {
                                monitorrefresh = true;
                            }

                        }
                    }

                    if (monitorrefresh)
                    { //called every 5sec.
                        monitorrefresh = false;
                        WAVE temp = dp_main.Getwave(_WaveIndexChA); //so far chA is assigned to ECG.
                                                                    //WAVE first = dsp.LowPassFilter(ShortBuf_A, (int)picoeeg.setting.SampleFreqHz, 50);
                                                                    //double[] peaks = dsp.PeakDetection(first, (int)picoeeg.setting.SampleFreqHz);

                        double[] peaks = dsp.PeakDetection(temp, (int)picoeeg.setting.SampleFreqHz);
                        dp_main.AllMarkerClear();
                        for (int k = 0; k < peaks.Length; k++)
                        {
                            dp_main.SetMarker(DATAPLOT.MARKERTYPE.X, peaks[k]);
                        }
                        //pictureBox1.Image = dp_main.Plot();
                        //pictureBox1.Refresh();

                        double total_ms = 0;
                        for (int k = 2; k < peaks.Length; k++)
                        {
                            total_ms += (peaks[k] - peaks[k - 1]) * 1000;
                        }

                        //Update beat oscillator
                        double newinterval = (total_ms / (peaks.Length - 2));
                        bll.Interval = (int)newinterval;

                        //this adjustment is done at dp_main.X_end.
                        int offset_ms = (int)((dp_main.X_end - peaks[peaks.Length - 1]) * 1000);
                        bll.PhaseAdjust(-1 * offset_ms);

                        for (int k = 2; k < peaks.Length; k++)
                        {
                            dp_lorenz.AddPoint(_WaveIndexRR, peaks[k] - peaks[k - 1], peaks[k - 1] - peaks[k - 2]);
                        }

                        pictureBox2.Image = dp_lorenz.Plot();
                        pictureBox2.Refresh();
                        //scroll every 5sec. (suitable for signal processing)
                        dp_main.X_start += 5;
                        dp_main.X_end += 5;
                    }


                    pictureBox1.Image = dp_main.Plot();
                    pictureBox1.Refresh();
                    Application.DoEvents();
                }
            }
            picoeeg.StopStreaming();

            //not good....
            if      (picoeeg.setting.channelSettings[0].enabled > 0) { last_wave[0] = dp_main.Getwave(_WaveIndexChA); }
            else if (picoeeg.setting.channelSettings[1].enabled > 0) { last_wave[1] = dp_main.Getwave(_WaveIndexChB); }
            
            //TEST CODE

            RRanalysis(last_wave[0]);
            
            WAVE first = dsp.FFT(last_wave[0], 2000);
            dp_spectrum.Addwave(first);
            dp_spectrum.FittingView(true, true);
            dp_spectrum.X_start = 0;
            dp_spectrum.X_end = 2;
            pictureBox3.Image = dp_spectrum.Plot();
            pictureBox3.Refresh();

            //Clear all plot..
            dp_main.AllWaveClear();
            dp_lorenz.AllWaveClear();
            dp_spectrum.AllWaveClear();

            //Open Picoscope
            picoeeg.Close();

        }

        //Update setting file by reading forms.
        private PicoSetting Forms2Setting()
        {
            PicoSetting set = new PicoSetting();
            //channelA
            set.channelSettings[0].enabled = (short)(CHKBOX_en_A.Checked ? 1 : 0);
            set.channelSettings[0].DCcoupled = (short)CMBOX_A_ACDC.SelectedIndex;
            set.channelSettings[0].range = (Imports.Range)Enum.Parse(typeof(PS2000_Const.RangeCode), CMBOX_inputrange_A.Text); ;
            //channelB
            set.channelSettings[1].enabled = (short)(CHKBOX_en_B.Checked ? 1 : 0);
            set.channelSettings[1].DCcoupled = (short)CMBOX_B_ACDC.SelectedIndex;
            set.channelSettings[1].range = (Imports.Range)Enum.Parse(typeof(PS2000_Const.RangeCode), CMBOX_inputrange_B.Text); ;

            //sampling frequency
            double factor = 1;
            if (CMBX_fmpl_unit.SelectedIndex == (int)PS2000_Const.FreqUnit.mHz)      factor = 1/1000;
            else if (CMBX_fmpl_unit.SelectedIndex == (int)PS2000_Const.FreqUnit.kHz) factor = 1000;
            else if (CMBX_fmpl_unit.SelectedIndex == (int)PS2000_Const.FreqUnit.MHz) factor = 1000000;
            set.SampleFreqHz = double.Parse(TBOX_fsmpl.Text)* factor;

            //Recording time 

            if (CMBX_recordtime_unit.SelectedIndex == (int)PS2000_Const.TimeUnit.sec) factor = 1.0;
            else if (CMBX_recordtime_unit.SelectedIndex == (int)PS2000_Const.TimeUnit.ms) factor = 1.0e-3;
            else if (CMBX_recordtime_unit.SelectedIndex == (int)PS2000_Const.TimeUnit.us) factor = 1.0E-6;
            set.RecordTime = double.Parse(TBOX_sample_time.Text) * factor;

            set.noOfSamplesPerAggregate = 1;
            set.TrigLevelmv = short.Parse(TBOX_trg_level.Text);
            set.TrigDelay = trackBar1.Value;
            set.TrigSource = (Imports.Channel)Enum.Parse(typeof(PS2000_Const.TrigChannel), CMBOX_trg_ch.Text);
            set.TrigDirection = (Imports.ThresholdDirection)CMBOX_trig_direction.SelectedIndex;

            return set;
        }

        private Double ToSec(int time, short timeUnit)
        {
            switch (timeUnit)
            {
                case (short)Imports.ReportedTimeUnits.FemtoSeconds:
                    return (Double)time * 1.0e-15;
                case (short)Imports.ReportedTimeUnits.PicoSeconds:
                    return (Double)time * 1.0e-12;
                case (short)Imports.ReportedTimeUnits.NanoSeconds:
                    return (Double)time * 1.0e-9;
                case (short)Imports.ReportedTimeUnits.MicroSeconds:
                    return (Double)time * 1.0e-6;
                case (short)Imports.ReportedTimeUnits.MilliSeconds:
                    return (Double)time * 1.0e-3;
                case (short)Imports.ReportedTimeUnits.Seconds:
                    return (Double)time;
                default:
                    return (Double)time;
            }
        }

        private string nano2other(Double nanovalue)
        {
            if (nanovalue < 1e3) return string.Format("{0:###.000} ns", nanovalue);
            else if (nanovalue >= 1e3 && nanovalue < 1e6) return string.Format("{0:###.000} us", nanovalue / 1e3);
            else if (nanovalue >= 1e6 && nanovalue < 1e9) return string.Format("{0:###.000} ms", nanovalue / 1e6);
            else return string.Format("{0:###.000}  s", nanovalue / 1.0e9);
        }


        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void BTN_start_Click(object sender, EventArgs e)
        {
            //Open Picoscope
            picoeeg.InitPico();

            dp_main.AllWaveClear();

            if ((Imports.Channel)Enum.Parse(typeof(PS2000_Const.TrigChannel), CMBOX_trg_ch.Text) != Imports.Channel.None)
            {
                dp_main.SetMarker(DATAPLOT.MARKERTYPE.X, 0, Color.Green);
            }

            //update setting.
            picoeeg.setting = Forms2Setting();
            picoeeg.setting.RecordTime = 3600;//5min
            
            //Setting of bufferes. (size depends on sampling rate)
            FFT_buffer =     new WAVE((int)(picoeeg.setting.SampleFreqHz * picoeeg.setting.RecordTime));          //1hour memory
            RR_data    =     new WAVE((int)picoeeg.setting.RecordTime);                                              //~1hour memory

            //Obtain wave indice
            //Temporal data (main monitoring plot) is stored in MDPlot class.
            _WaveIndexRR  = dp_lorenz.CreateWave(RR_data.Length, "RR", "RR");
            _WaveIndexFFT = dp_spectrum.CreateWave(FFT_buffer.Length/2,"Freq","Amp");
            _WaveIndexChA = dp_main.CreateWave((int)(picoeeg.setting.SampleFreqHz * 5), "Time", "ChA");
            _WaveIndexChB = dp_main.CreateWave((int)(picoeeg.setting.SampleFreqHz * 5), "Time", "ChB");
            
            double factor = 1.0;
            if (CMBX_interval_unit.SelectedIndex == (int)PS2000_Const.TimeUnit.sec) factor = 1.0;
            else if (CMBX_interval_unit.SelectedIndex == (int)PS2000_Const.TimeUnit.ms) factor = 1.0e-3;
            else if (CMBX_interval_unit.SelectedIndex == (int)PS2000_Const.TimeUnit.us) factor = 1.0e-6;

            int rangeA = picoeeg.setting.channelSettings[0].enabled * (int)Enum.Parse(typeof(PS2000_Const.Rangemv), CMBOX_inputrange_A.Text);
            int rangeB = picoeeg.setting.channelSettings[1].enabled * (int)Enum.Parse(typeof(PS2000_Const.Rangemv), CMBOX_inputrange_B.Text);

            dp_main.Y_end = rangeA > rangeB ? rangeA : rangeB;
            dp_main.Y_start = -1 * dp_main.Y_end;
            dp_main.X_end = 5;
            dp_main.X_start = 0;

            dp_lorenz.Y_end = 1.2;
            dp_lorenz.Y_start = 0.5;
            dp_lorenz.X_end = 1.2;
            dp_lorenz.X_start = 0.5;

            //Start Beat Generator
            bll.Start();
            thr = new Thread(new ThreadStart(bll.beat));
            thr.Start();
            bll.Interval = 1000;

            //Start streaming
            picoeeg.StartStreaming();

            //Start timer interrupt
            _Running = true;

            Global_sw.Start();
        }

        private void BTN_stop_Click(object sender, EventArgs e)
        {
            picoeeg.StopStreaming();
            _Running = false;

            //stop and reset the stop watch
            Global_sw.Reset();

            RR_data = dp_lorenz.Getwave(_WaveIndexRR);
            //Close Picoscope
            picoeeg.Close();

            //stop Beat Generator (BLL)
            thr.Abort();
        }

        //Timer interrupt 
        private void timer1_Tick(object sender, EventArgs e)
        {
            
            if (_Running)
            {
                //show elapsed time
                TimeSpan ts = Global_sw.Elapsed;
                Label_time.ForeColor = Color.GreenYellow;               
                Label_time.Text = ts.ToString("h\\:mm\\:ss\\.ff");

                //Receive available data in streaming buffer of Picoscope.
                WAVE[] results = picoeeg.ReveiveAvailableData();

                bool monitorrefresh = false;
                bool FFTready = false;

                if (results != null && results[0] != null)
                {
                    for (int i = 0; i < results[0].ValidLength; i++) //datapoints
                    {
                        for (int j = 0; j < results.Length; j++) //channel
                        {

                            int windex = 0;
                            if (j == 0) //channel #A
                            {
                                dp_main.AddPoint(_WaveIndexChA, results[j][i].X, results[j][i].Y);
                                FFT_buffer.AddPoint(results[j][i].X, results[j][i].Y);

                                if (results[j][i].X % 30 == 0 && results[j][i].X > 0) //detect FFT interval end.
                                {
                                    FFTready = true;
                                }
                            }
                            else if (j == 1)
                            {
                                // dp_main.wavelist[_WaveIndexChB].AddPoint(results[j][i].X, results[j][i].Y);
                                dp_main.AddPoint(_WaveIndexChB, results[j][i].X, results[j][i].Y);
                            }

                            if (results[j][i].X % 5 == 0 && results[j][i].X > 0) //detect interval end.
                            {
                                monitorrefresh = true;
                            }

                        }
                        //Scrolling(suitable for monitoring)
                        if (results[0][results[0].Length - 1].X >= 5)
                        {
                            dp_main.X_end = results[0][results[0].Length - 1].X;
                            dp_main.X_start = dp_main.X_end - 5;
                        }

                        if (monitorrefresh)
                        { //called every 5sec.
                            monitorrefresh = false;
                            WAVE temp = dp_main.Getwave(_WaveIndexChA); //so far chA is assigned to ECG.
                            temp = dsp.LowPassFilter(temp, (int)picoeeg.setting.SampleFreqHz, 60);
                            double[] peaks = dsp.PeakDetection(temp, (int)picoeeg.setting.SampleFreqHz);
                            dp_main.AllMarkerClear();
                            for (int k = 0; k < peaks.Length; k++)
                            {
                                dp_main.SetMarker(DATAPLOT.MARKERTYPE.X, peaks[k]);
                            }
                            //pictureBox1.Image = dp_main.Plot();
                            //pictureBox1.Refresh();

                            double total_ms = 0;
                            for (int k = 2; k < peaks.Length; k++)
                            {
                                total_ms += (peaks[k] - peaks[k - 1]) * 1000;
                            }

                            ////Update beat oscillator
                            //double newinterval = (total_ms / (peaks.Length - 2));
                            //bll.Interval = (int)newinterval;

                            ////this adjustment is done at dp_main.X_end.
                            //int offset_ms = (int)((dp_main.X_end - peaks[peaks.Length - 1]) * 1000);
                            //bll.PhaseAdjust(-1 * offset_ms);

                            for (int k = 2; k < peaks.Length; k++)
                            {
                                dp_lorenz.AddPoint(_WaveIndexRR, peaks[k] - peaks[k - 1], peaks[k - 1] - peaks[k - 2]);
                            }

                            pictureBox2.Image = dp_lorenz.Plot();
                            pictureBox2.Refresh();
                            //scroll every 5sec. (suitable for signal processing)
                            //dp_main.X_start += 5;
                            //dp_main.X_end += 5;
                        }
                        if (FFTready)
                        {
                            FFTready = false;
                            WAVE temp = dsp.FFT(FFT_buffer, (int)picoeeg.setting.SampleFreqHz);
                            dp_spectrum.Replacewave(temp, _WaveIndexFFT);
                            dp_spectrum.X_end = 0.2;
                            dp_spectrum.FittingView(false, true);
                            pictureBox3.Image = dp_spectrum.Plot();
                            pictureBox3.Refresh();

                            //Analysis
                            if (temp[1].X < 0.01) {
                                Double HF = 0, LF = 0;
                                for (int j = 0; j < temp.ValidLength; j++)
                                {
                                    if (temp[j].X >= 0.04 && temp[j].X <= 0.15) LF += temp[j].Y;
                                    else if (temp[j].X > 0.15 && temp[j].X <= 0.4) HF += temp[j].Y;
                                }
                                LBL_FFT.Text = String.Format("HF {0}  LF {1} , LF/HF {2}", HF, LF, LF/HF);
                            }
                        }

                    } //for (i) data point

                    //dp_main.FittingView(true,true);
                    pictureBox1.Image = dp_main.Plot();
                    pictureBox1.Refresh();
                }
            }
            else
            {
                Label_time.ForeColor = Color.Yellow;
                Label_time.Text = DateTime.Now.ToString("h:mm:ss tt");

                pictureBox1.Image = dp_main.Plot();
                pictureBox1.Refresh();
            }


        }



        //Save last-scan
        private void SaveLastWave()
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            StreamWriter sw;

            //Determine filepath to export.
            saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            saveFileDialog1.Filter = "csv files (*.csv) | *.csv| All files (*.*) | *.*";
            saveFileDialog1.Title = "Saving csv file";
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                out_filename = saveFileDialog1.FileName;
                sw = new StreamWriter(out_filename, false, System.Text.Encoding.Default);

                //Header creation
                sw.WriteLine("TimeA, ChannelA, TimeB, ChannelB");

                int datapoint = Math.Max(last_wave[0].ValidLength, last_wave[1].ValidLength);

                for (int i = 0; i < datapoint; i++)
                {
                    string buf = String.Empty;
                    buf += last_wave[0][i].X.ToString() + "," + last_wave[0][i].Y.ToString();
                    buf += "," + String.Format("{0:e},{1:e}", last_wave[1][i].X, last_wave[1][i].Y);
                    sw.WriteLine(buf);
                }
                LogAppend(String.Format("file({0}) is saved.", out_filename));
                sw.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveLastWave();
        }


        private void BTN_Load_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            StreamReader sr;

            dp_main.AllWaveClear();

            //Determine filepath to export.
            openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDialog1.Filter = "csv files (*.csv) | *.csv| All files (*.*) | *.*";
            openFileDialog1.Title = "Open wave file";
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                in_filename = openFileDialog1.FileName;
                sr = new StreamReader(in_filename);

                string data;
                string[] read;
                char[] seperators = { ',' };

                List<string> all_line = new List<string>();

                while ((data = sr.ReadLine()) != null)
                {
                    read = data.Split(seperators, StringSplitOptions.None);
                    if (read.Length >= 2) { all_line.Add(data); }
                }

                if (all_line.Count > 1)
                {
                    // Read header
                    read = all_line[0].Split(seperators, StringSplitOptions.None); //Header
                    int wavecount = (int)(read.Length / 2);
                    int[] waveindex = new int[wavecount];

                    for (int i = 0; i < wavecount; i++)
                    {
                        WAVE temp = new WAVE(all_line.Count - 1);
                        temp.x_name = read[0];
                        temp.y_name = read[1];

                        waveindex[i] = dp_main.Addwave(temp);
                    }

                    for (int i = 1; i < all_line.Count; i++)
                    {
                        read = all_line[i].Split(seperators, StringSplitOptions.None);
                        for (int j = 0; j < wavecount; j++)
                        {
                            dp_main.AddPoint(waveindex[j], double.Parse(read[j]), double.Parse(read[j + 1]));
                        }
                    }

                    //update last_wave
                    last_wave = new WAVE[wavecount];

                    for (int i = 0; i < wavecount; i++)
                    {
                        last_wave[i] = dp_main.Getwave(waveindex[i]);
                    }
                }

            }
            dp_main.FittingView(true, true);
            dp_main.Y_end = 1.5e3;
            dp_main.Y_start = -1.5e3;
            pictureBox1.Image = dp_main.Plot();
            pictureBox1.Refresh();
        }

        private void RRanalysis(WAVE inwave)
        {
            int smp_freq = (int)picoeeg.setting.SampleFreqHz;

            WAVE first = dsp.LowPassFilter(inwave, smp_freq, 50);

            dp_main.Addwave(first);
            double[] peaks = dsp.PeakDetection(first, smp_freq);

            dp_main.AllMarkerClear();
            for (int i = 0; i < peaks.Length; i++)
            {
                dp_main.SetMarker(DATAPLOT.MARKERTYPE.X, peaks[i]);
            }
            pictureBox1.Image = dp_main.Plot();
            pictureBox1.Refresh();


            for (int i = 2; i < peaks.Length; i++)
            {
                dp_lorenz.AddPoint(_WaveIndexRR, peaks[i] - peaks[i - 1], peaks[i - 1] - peaks[i - 2]);
            }

            //dp_lorenz.Y_end = 1.2;
            //dp_lorenz.Y_start = 0.5;
            //dp_lorenz.X_end = 1.2;
            //dp_lorenz.X_start = 0.5;
            dp_lorenz.FittingView(true, true);

            pictureBox2.Image = dp_lorenz.Plot();
            pictureBox2.Refresh();



        }

        private void BTN_dsp_Click(object sender, EventArgs e)
        {   
            
            // === quasi parameter setting for simulated condition.
            picoeeg.setting = Forms2Setting();
            picoeeg.setting.SampleFreqHz = 2000;

            //Setting of bufferes. (size depends on sampling rate)
            FFT_buffer = new WAVE((int)(picoeeg.setting.SampleFreqHz * picoeeg.setting.RecordTime));          //1hour memory
            RR_data = new WAVE((int)picoeeg.setting.RecordTime);                                              //~1hour memory

            //Obtain wave indice
            //Temporal data (main monitoring plot) is stored in MDPlot class.
            _WaveIndexRR = dp_lorenz.CreateWave(RR_data.Length, "RR", "RR");
            _WaveIndexFFT = dp_spectrum.CreateWave(FFT_buffer.Length / 2, "Freq", "Amp");
            _WaveIndexChA = dp_main.CreateWave((int)(picoeeg.setting.SampleFreqHz * 5), "Time", "ChA");
            _WaveIndexChB = dp_main.CreateWave((int)(picoeeg.setting.SampleFreqHz * 5), "Time", "ChB");

            // ========================================================

            RRanalysis(last_wave[0]);
            ECG_DSP.ECG_DSP dsp = new ECG_DSP.ECG_DSP();
            WAVE first = dsp.FFT(last_wave[0],2000);
            dp_spectrum.Addwave(first);

            dp_spectrum.FittingView(true, true);
            //dp_spectrum.X_start = 0;
            //dp_spectrum.X_end = 100;
            pictureBox3.Image = dp_spectrum.Plot();
            pictureBox3.Refresh();
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ShowItClickable(object sender, MouseEventArgs e)
        {
            Cursor.Current = Cursors.Hand;
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            Form2 about = new Form2();
            about.Show();
        }

        private void BeatTimer_Tick(object sender, EventArgs e)
        {
            if (_Running)
            {

            }
        }
        //Music Play test.
        private void button1_Click_1(object sender, EventArgs e)
        {

            //Setting of music.(sharpDX)
            xaudio = new XAudio2();
            var masteringsound = new MasteringVoice(xaudio);
            var nativefilestream = new NativeFileStream(@"C:\Users\miz\Music\emo-shuffle\Sad_Kazuki.wav", NativeFileMode.Open, NativeFileAccess.Read, NativeFileShare.Read);
            soundstream = new SoundStream(nativefilestream);
            waveFormat = soundstream.Format;
            buffer = new AudioBuffer
            {
                Stream = soundstream.ToDataStream(),
                AudioBytes = (int)soundstream.Length,
                Flags = BufferFlags.EndOfStream
            };
            SourceVoice sourceVoice = new SourceVoice(xaudio, waveFormat, true);
            sourceVoice.SubmitSourceBuffer(buffer, soundstream.DecodedPacketsInfo);
            sourceVoice.Start();
        }

        private void MusicEnd(object sender, EventArgs e)
        {
            System.Media.SystemSounds.Beep.Play();
        }

    }

    public class BeatLock
    {
        Stopwatch sw = new Stopwatch();

        //sound play -SharpDX
        XAudio2 xaudio;
        WaveFormat waveFormat;
        AudioBuffer buffer;
        SoundStream soundstream;

        int last_beat; //ms
        Boolean _adjust = false;

        int BeatNum = 1;
        //ms
        public int Interval = 1000;
        int Phase = 10;
        
        public void Start()
        {
            //Setting of music.(sharpDX)
            xaudio = new XAudio2();
            var masteringsound = new MasteringVoice(xaudio);
            var nativefilestream = new NativeFileStream(@"..\..\Resources\Metronom.wav", NativeFileMode.Open, NativeFileAccess.Read, NativeFileShare.Read);
            soundstream = new SoundStream(nativefilestream);
            waveFormat = soundstream.Format;
            buffer = new AudioBuffer
            {
                Stream = soundstream.ToDataStream(),
                AudioBytes = (int)soundstream.Length,
                Flags = BufferFlags.EndOfStream
            };

            //Start Stopwatch
            sw.Start();
        }

        public void beat()
        {
            SourceVoice sourceVoice;


            while (true)
            {
                for (int i = 0; i < BeatNum; i++)
                {
                    sourceVoice = new SourceVoice(xaudio, waveFormat, true);
                    sourceVoice.SubmitSourceBuffer(buffer, soundstream.DecodedPacketsInfo);
                    sourceVoice.Start();
                    last_beat = (int)((double)sw.ElapsedTicks / Stopwatch.Frequency * 1000);
                    System.Threading.Thread.Sleep(Interval/BeatNum);
                }
                //last_beat = (int)((double)sw.ElapsedTicks / Stopwatch.Frequency * 1000);
                //System.Threading.Thread.Sleep(Interval);
            }
        }

        //phase adjust accoding to the time this function is called.
        //offset_ms enable to adjust for past event.
        public void PhaseAdjust(int offset_ms)
        {
            int current = (int)((double)sw.ElapsedTicks / Stopwatch.Frequency * 1000) - offset_ms;
            Phase = (current - last_beat) % (Interval/BeatNum);
            //if (Phase < 0) Phase += (Interval / BeatNum);
            _adjust = true;
        }
    }
}
