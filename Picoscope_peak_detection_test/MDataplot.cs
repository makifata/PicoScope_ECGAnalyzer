/* 
 * MDataplot Ver. 1.1.2
 * Copyright 2017 - 2020, Mitsutoshi Makihata. All rights received
 * 
 * Ver 1.0.0  170906 2D plot is implemented. Converted old code since 2012 into Class.
 * Ver 1.0.1  190116 Improve the Autotick function
 * Ver 1.0.2  200318 Bugfix in fittingview(). C and Y can not automaticaly fit simultaneously..
 * Ver 1.0.3  200330 Bugfix in AutoTicking of Y-axis.
 * Ver 1.0.4  2020-04-01 Improve Auto Ticking and Engineering notation
 * Ver 1.1.0  2020-04-06 Introduce WAVE class instead of SIGNAL[].
 * Ver 1.1.1  2020-05-13 RDP algorithm to simplify plot (https://rosettacode.org/wiki/Ramer-Douglas-Peucker_line_simplification) not finished yet.
 * Ver 1.1.2  2020-06-09 Implemented Circular buffer for large number of data. (faster plot)
 * 
 * Next update
 * Introdution of several preset of graph stype) - Tiny plot (no label, axis), etc.
 * Update 2D plot for Spectrum View
 * 
 */


using System;
using System.Collections.Generic;//List
using System.Linq;
using System.Text;
using System.Drawing;

namespace MDataplot
{
    //Single 1D data

    public class WAVE
    {
        public struct SIGNAL
        {
            public Double X;
            public Double Y;

            public SIGNAL(Double setX, Double setY)
            {
                X = setX;
                Y = setY;
            }
        }

        private SIGNAL[] wave; // list or determined size? (to be considered.., 2020.5)
        public string x_name; // temporal?
        public string y_name; // temporal?
        int head = 0; // start pointer
        int datalength = 0;

        //create signal array.
        public WAVE(int size)
        {
            wave = new SIGNAL[size];
            head = 0;
            datalength = 0;

            for (int i = 0; i < size; i++)
            {
                wave[i] = new SIGNAL();
                wave[i].X = 0;
                wave[i].Y = 0;
            }
        }

        // Clear wave
        public void Clear()
        {
            head = 0;
            datalength = 0;
        }
        //"Indexer or Smart array
        public SIGNAL this[int index]
        {
            get {
                if (index > wave.Length - 1)
                {
                    throw new IndexOutOfRangeException();
                }
                else return wave[(head + index) % wave.Length];
            }
            set {
                if (index > wave.Length - 1)
                {
                    throw new IndexOutOfRangeException();
                }
                else
                {
                    wave[(head + index) % wave.Length] = value;
                }
            }
        }

        //Size of wave buffer.
        public int Length
        {
            get {
                return wave.Length;
            }
        }
        //return number of valid data
        public int ValidLength
        {
            get
            {
                return datalength;
            }
        }

        //adding point
        public void AddPoint(Double newx, Double newy)
        {
            wave[(head + datalength) % wave.Length].X = newx;
            wave[(head + datalength) % wave.Length].Y = newy;
            if (datalength < wave.Length) datalength++;
            else head = (head + 1) % wave.Length;
        }

    }

    class DATAPLOT
    {

        public struct Dpoint
        {
            public Double X;
            public Double Y;

            public Dpoint(Double setX, Double setY)
            {
                X = setX;
                Y = setY;
            }
        }

        //Consts
        private static readonly Font fnt = new Font("Arial", 10);
        private static readonly Font title_fnt = new Font("Aliquam", 12);
        private const int MAX_PLOT = 6;
        private const int TICK_X_NUM = 5;
        private const int TICK_Y_NUM = 5;
        private const int marginX1 = 90;
        private const int marginX2 = 40;
        private const int marginY1 = 50;
        private const int marginY2 = 40;
        private const float PLOT_LINE_WIDTH = 1.0F;
        private const float PLOT_DOT_SIZE = 2;
        private const float AXIS_LINE_WIDTH = 1.5F;

        //Final result of DATAPLOT.
        public Bitmap canvas;

        //List of traces
        public List<WAVE> wavelist = new List<WAVE>();             //List of waves
        private List<bool> plotflag  = new List<bool>();            //show or hide of each wave.

        //Markers
        private List<Double> X_marker =  new List<Double>();        //Vertical Coursol ?(Global)
        private List<Double> Y_marker = new List<Double>();         //Horizontal Coursol ?(Global)
        private List<Color> XMarker_color = new List<Color>();      //Color of Vertical Coursol
        private List<Color> YMarker_color = new List<Color>();      //Color of Horizontal Coursol

        //public struct PlotSettings
        //{

        //}

        //Plot setting
        private String TitleX;
        private String TitleY;
        private String UnitX;
        private String UnitY;
        public Boolean Auto_tick = true;  //True: Auto (5 ticks) False: Manual (use tick_inc)
        
        public Brush Plot_Background = Brushes.Black;
        public Pen Plot_AxisPen = Pens.Yellow;
        public Brush Plot_FontColor = Brushes.White;

        //Plot area
        public Double X_start = 0;
        public Double X_end = 1;
        public Double Y_start = 0;
        public Double Y_end = 1;
        public int Tick_num_X = 1;
        public int Tick_num_Y = 1;
        public Double Tick_inc_X = 1;
        public Double Tick_inc_Y = 1;
        public int Exponent_X = 0;  //10^{??}
        public int Exponent_Y = 0;  //10^{??}
        public int Digits_after_dp_X;
        public int Digits_after_dp_Y;

        public byte drawstyle = 0;   //0 is line, 1 is dot, 2 is vertical line (future)

        public DATAPLOT(Int32 pbox_w, Int32 pbox_h)
        {
            TitleX = "X";
            TitleY = "Y";
            canvas = new Bitmap(pbox_w, pbox_h);
        }

        public DATAPLOT(Int32 pbox_w, Int32 pbox_h, String Xtitle, String Ytitle)
        {
            TitleX = Xtitle;
            TitleY = Ytitle;
            canvas = new Bitmap(pbox_w, pbox_h);
        }

        public void ColorSetting(Color bg, Color axis)
        {
            Plot_Background = new SolidBrush(bg);
            Plot_FontColor = new SolidBrush(axis);
            Plot_AxisPen = new Pen(axis, AXIS_LINE_WIDTH);
        }

        public void AddPoint(int waveindex, double x, double y)
        {
            wavelist[waveindex].AddPoint(x, y);
        }

        public void JointWave(int waveindex, WAVE newwave)
        {
            for(int i = 0; i < newwave.ValidLength; i++)
            {
                wavelist[waveindex].AddPoint(newwave[i].X, newwave[i].Y);
            }
        }

        //clear waves.
        public void AllWaveClear()
        {
            wavelist.Clear();
            plotflag.Clear();
            X_marker.Clear();
            Y_marker.Clear();
            XMarker_color.Clear();
            YMarker_color.Clear();
        }

        public void AllMarkerClear()
        {
            X_marker.Clear();
            Y_marker.Clear();
            //XMarker_color.Clear();
            //YMarker_color.Clear();
        }

        //Clear specific wave using waveindex
        public bool ClearWave(Int32 waveindex)
        {
            if (waveindex < 0 || waveindex >= wavelist.Count)
            {
                return false;
            }
            else
            {
                wavelist[waveindex].Clear();
                return true;
            }
        }


        public int CreateWave(int size, string xname, string yname)
        {
            WAVE temp = new WAVE(size);
            temp.x_name = xname;
            temp.y_name = yname;
            wavelist.Add(temp);
            plotflag.Add(true);
            return wavelist.Count - 1;
        }

        //Add wave, fitting, and return waveindex.
        public int Addwave(WAVE newwave)
        {
            //Add new wave.
            wavelist.Add(newwave);
            plotflag.Add(true);
            return wavelist.Count - 1;
        }

        public WAVE Getwave(Int32 waveindex)
        {
            return wavelist[waveindex];
        }

        //Remove waveform.
        public void Removewave(int index)
        {
            wavelist.RemoveAt(index);
            plotflag.RemoveAt(index);
        }

        //Replace waveform.
        public void Replacewave(WAVE wave, int index)
        {
            wavelist[index] = wave;
        }

        //make a wave visible
        public void ShowWave(int index)
        {
            plotflag[index] = true;
        }

        //make a wave invisible (data exist)
        public void HideWave(int index)
        {
            plotflag[index] = false;
        }

        //Direct updating of X and Y limit
        public void UpdateRanges(Double x_start, Double x_end, Double y_start, Double y_end)
        {
            X_start = x_start;
            X_end   = x_end; 
            Y_start = y_start;
            Y_end   = y_end;
        }

        //Convert position data in a picturebox into data range.
        //This is for zooming function.
        public void UpdateView(Point P1, Point P2)
        {

            Double scaleX = (X_end - X_start) / (canvas.Width - marginX1 - marginX2);
            Double scaleY = (Y_end - Y_start) / (canvas.Height - marginY2 - marginY1);

            X_end = X_start + (P2.X - marginX1) * scaleX; //Do this first
            X_start += (P1.X - marginX1) * scaleX;

            Y_start = Y_end - (P2.Y - marginY2) * scaleY;//Do this first
            Y_end -= (P1.Y - marginY2) * scaleY;
        }

        public void UpdateView(Double X1, Double X2)
        {
            X_start = X1;
            X_end = X2;

            List<Double> testarray = new List<Double>();
            for (Int32 i = 0; i < wavelist.Count; i++)
            {
                if (plotflag[i])
                {
                    for (Int32 j = 0; j < wavelist[i].ValidLength; j++)
                    {
                        if (wavelist[i][j].X > X1 && wavelist[i][j].X < X2)
                        {
                            testarray.Add(wavelist[i][j].Y);
                        }
                    }
                }
            }
            Y_start  = testarray.Min() - (testarray.Max() - testarray.Min()) * 0.01;
            Y_end    = testarray.Max() + (testarray.Max() - testarray.Min()) * 0.01;
            
        }

        //Update the view setting so that all existing wave can be fit in plot area.
        public void FittingView(Boolean X, Boolean Y)
        {
            Int32 i, j;

            Double temp_XMIN = Double.MaxValue;
            Double temp_XMAX = Double.MinValue;
            Double temp_YMIN = Double.MaxValue;
            Double temp_YMAX = Double.MinValue;

            for (i = 0; i < wavelist.Count; i++)
            {
                if (plotflag[i])
                {
                    for (j = 0; j < wavelist[i].ValidLength; j++)
                    {
                            temp_XMIN = Math.Min(temp_XMIN, wavelist[i][j].X);
                            temp_XMAX = Math.Max(temp_XMAX, wavelist[i][j].X);
                            temp_YMIN = Math.Min(temp_YMIN, wavelist[i][j].Y);
                            temp_YMAX = Math.Max(temp_YMAX, wavelist[i][j].Y);
                    }
                }
            }

            if (X)
            {
                X_start = temp_XMIN;
                X_end = temp_XMAX;
            }

            if (Y)
            {
                Y_start = temp_YMIN;
                Y_end = temp_YMAX;
            }
        }

        public enum MARKERTYPE : byte
        {
            X = 0,
            Y = 1,
            XY = 2
        }

        public void SetMarker(MARKERTYPE MarkerType, double value)
        {
            if (MarkerType == MARKERTYPE.X)
            {
                X_marker.Add(value);
                XMarker_color.Add(Color.Green);
            }
            else
            {
                Y_marker.Add(value);
                YMarker_color.Add(Color.Green);
            }
        }

        private Double pow10(Double input)
        {
            return Math.Pow(10, input);
        }

        public void SetMarker(MARKERTYPE MarkerType, double value,Color color)
        {
            if (MarkerType == MARKERTYPE.X)
            {
                X_marker.Add(value);
                XMarker_color.Add(color);
            }
            else
            {
                Y_marker.Add(value);
                YMarker_color.Add(color);
            }
        }

        /*     /(0,0) in picture box
         *   *_____________________________
         *   |  ____________Y2___________  |
         *   |  |                       |  |
         *   |  |                       |  |
         *   |  |                       |  |
         *   |X1|                       |X2| canvas.Height
         *   |  |                       |  |
         *   |  | /(data origin)        |  |
         *   |  *_______________________|  |
         *   |______________Y1_____________|
         *              canvas.Width
         */
        
        
        public Bitmap Plot()
        {
            int i, j;
            int draw_width = canvas.Width - marginX1 - marginX2;
            int draw_height = canvas.Height - marginY2 - marginY1;
            int draw_centerX = draw_width / 2 + marginX1;
            int draw_centerY = draw_height / 2 + marginY2;

            //Create a Graphics Object
            Graphics g = Graphics.FromImage(canvas);

            //Reflesh Graph
            g.FillRectangle(Plot_Background, 0, 0, canvas.Width, canvas.Height);

            //Auto Ticking (Calculation)
            if (Auto_tick)
            {
                //###Determin Suitable number of tick for X.##########################
                //Find enginnering notation.
                Exponent_X = (int)Math.Log10(X_end - X_start);
                if (Exponent_X > 0) Exponent_X = (Exponent_X / 3) * 3;
                else Exponent_X = (-Exponent_X + 3) / 3 * (-3);

                //Find digits after decimap point (dp)
                if ((X_end-X_start) * Math.Pow(10, -Exponent_X) >= 1000) { Exponent_X += 3; Digits_after_dp_X = 3; }
                else if ((X_end - X_start) * Math.Pow(10, -Exponent_X) >= 100) Digits_after_dp_X = 1;
                else if ((X_end - X_start) * Math.Pow(10, -Exponent_X) >= 10) Digits_after_dp_X = 2;
                else Digits_after_dp_X = 3;

                //Number of tick (space)
                Double err_min = 0;
                for (i = -2; i <= 2; i++)   //Look for near the target value (TICK_X_NUM, Constant this case)
                {
                    Double Tick_log = (X_end - X_start) / (i + TICK_X_NUM) * Math.Pow(10, -Exponent_X); //Pick tick number can divide a range with minimum error.
                    Double temp = Math.Abs(Math.Round(Tick_log, Digits_after_dp_X) - Tick_log);
                    if (i == -2 || err_min > temp)
                    {
                        err_min = temp;
                        Tick_num_X = i + TICK_X_NUM;
                    }
                }
                Tick_inc_X = Math.Abs(X_end - X_start) / Tick_num_X;

                //###Determin Suitable number of tick for Y.##########################
                //Find enginnering notation.
                Exponent_Y = (int)Math.Log10(Y_end-Y_start);
                if (Exponent_Y > 0) Exponent_Y = (Exponent_Y / 3) * 3;
                else Exponent_Y = (-Exponent_Y + 3) / 3 * (-3);

                //Find digits
                if ((Y_end - Y_start) * Math.Pow(10, -Exponent_Y) >= 1000) { Exponent_Y += 3; Digits_after_dp_Y = 3; }
                else if ((Y_end - Y_start) * Math.Pow(10, -Exponent_Y) >= 100) Digits_after_dp_Y = 1;
                else if ((Y_end - Y_start) * Math.Pow(10, -Exponent_Y) >= 10) Digits_after_dp_Y = 2;
                else Digits_after_dp_Y = 3;

                //Number of tick (space)
                err_min = 0;
                for (i = -2; i <= 2; i++)   //Look for near the target value (TICK_X_NUM, Constant this case)
                {
                    Double Tick_log = (Y_end - Y_start) / (i + TICK_Y_NUM) * Math.Pow(10, -Exponent_Y); //Pick tick number can divide a range with minimum error.
                    Double temp = Math.Abs(Math.Round(Tick_log, Digits_after_dp_Y) - Tick_log);
                    if (i == -2 || err_min > temp)
                    {
                        err_min = temp;
                        Tick_num_Y = i + TICK_Y_NUM;
                    }
                }
                Tick_inc_Y = Math.Abs(Y_end - Y_start) / Tick_num_Y;

            }

            //Draw the Frame of graph
            g.DrawRectangle(Plot_AxisPen, marginX1, marginY2, draw_width, draw_height);

            //Plot signal
            for (i = 0; i < wavelist.Count; i++)
            {
                if (plotflag[i])
                {
                    if (drawstyle == 0)
                    {
                        //scale to picture box (integer)
                        List<Point> pslist = new List<Point>();
                        for(j=0;j< wavelist[i].ValidLength;j++)
                        {
                            Point nextps = ValueToPoint(new Dpoint(wavelist[i][j].X, wavelist[i][j].Y));
                            //add only meaningful polygon
                            if (j >= 1)
                            {
                                if (pslist[pslist.Count - 1] != nextps)
                                {
                                    pslist.Add(nextps);
                                }
                            }else
                            {
                                pslist.Add(nextps);
                            }
                        }

                        //plot waveform
                        MGraph MG = new MGraph();
                        Pen graphpen = new Pen(MG.Makerainbow((double)i, 0.0, (double)MAX_PLOT), PLOT_LINE_WIDTH);
                        //Pen graphpen;
                        //switch (i)
                        //{
                        //    case 0:
                        //        graphpen = new Pen(Color.Yellow, PLOT_LINE_WIDTH);
                        //        break;
                        //    case 1:
                        //        graphpen = new Pen(Color.Blue, PLOT_LINE_WIDTH);
                        //        break;
                        //    case 2:
                        //        graphpen = new Pen(Color.Green, PLOT_LINE_WIDTH);
                        //        break;
                        //    case 3:
                        //        graphpen = new Pen(Color.Purple, PLOT_LINE_WIDTH);
                        //        break;
                        //    default:
                        //        graphpen = new Pen(Color.Black, PLOT_LINE_WIDTH);
                        //        break;
                        //}

                        //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                        if (pslist.Count >= 2)
                        {
                            //split points into 8000points. (8125 is limit?!)
                            int counter = 0;
                            int block_size = Math.Min(pslist.Count, 8000);
                            Point[] data_subset;
                            while (counter != pslist.Count)
                            {
                                data_subset = pslist.GetRange(counter, block_size).ToArray();
                                //Further simplification ?
                                //data_subset = RDP(data_subset, 1);
                                if (data_subset.Length >= 2)
                                {
                                    g.DrawLines(graphpen, data_subset);
                                }
                                counter += block_size;
                                block_size = Math.Min(pslist.Count - counter, 8000);
                            }
                        }
                    }
                    else if (drawstyle == 1) //dot style
                    {
                        j = 0;
                        while (j < wavelist[i].ValidLength)
                        {
                            Point nextps = ValueToPoint(new Dpoint(wavelist[i][j].X, wavelist[i][j].Y));
                            MGraph MG = new MGraph();
                            SolidBrush brush = new SolidBrush(MG.Makerainbow((double)i, 0.0, (double)MAX_PLOT));
                            g.FillEllipse(brush, nextps.X - PLOT_DOT_SIZE / 2, nextps.Y - PLOT_DOT_SIZE / 2, PLOT_DOT_SIZE, PLOT_DOT_SIZE);
                            j++;
                            if (j == wavelist[i].ValidLength) break;
                        }
                    }
                    else if (drawstyle == 2) //vertical lines
                    {
                        j = 0;
                        while (j < wavelist[i].ValidLength)
                        {
                            Point temp1 = ValueToPoint(new Dpoint(wavelist[i][j].X, 0));
                            Point temp2 = ValueToPoint(new Dpoint(wavelist[i][j].X, wavelist[i][j].Y));
                            Point[] ps = new Point[2];
                            ps[0] = temp1;
                            ps[1] = temp2;

                            MGraph MG = new MGraph();
                            Pen graphpen = new Pen(MG.Makerainbow((double)i, 0.0, (double)MAX_PLOT), PLOT_LINE_WIDTH);
                            g.DrawLines(graphpen, ps);
                            j++;
                            if (j == wavelist[i].ValidLength) break;
                        }
                    }
                }

                //Plot Markers
                Pen markpen;
                for (j = 0; j < X_marker.Count; j++)
                {
                    markpen = new Pen(XMarker_color[j], 1);
                    Dpoint temp = new Dpoint(X_marker[j], Y_start);
                    Point MV1A = ValueToPoint(temp);
                    temp = new Dpoint(X_marker[j], Y_end);
                    Point MV1B = ValueToPoint(temp);
                    g.DrawLine(markpen, MV1A, MV1B);
                }
                for (j = 0; j < Y_marker.Count; j++)
                {
                    markpen = new Pen(YMarker_color[j], 1);
                    Dpoint temp = new Dpoint(X_start, Y_marker[j]);
                    Point MV1A = ValueToPoint(temp);
                    temp = new Dpoint(X_end, Y_marker[j]);
                    Point MV1B = ValueToPoint(temp);
                    g.DrawLine(markpen, MV1A, MV1B);
                }

                //Draw the Frame of graph
                g.DrawRectangle(Plot_AxisPen, marginX1, marginY2, draw_width, draw_height);

            }

            // Create a StringFormat object with the each line of text, and the block
            // of text centered on the page.
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

            // Create a StringFormat object with the each line of text, and the block
            // of text right aligned
            StringFormat stringFormatR = new StringFormat();
            stringFormatR.Alignment = StringAlignment.Far;
            stringFormatR.LineAlignment = StringAlignment.Far;

            //Drawing Ticks
            Double X_start_floor = Math.Floor(X_start * Math.Pow(10, Digits_after_dp_X)) / Math.Pow(10, Digits_after_dp_X);  //floor in specfic digits.
            Double X_end_ceiling = Math.Ceiling(X_end * Math.Pow(10, Digits_after_dp_X)) / Math.Pow(10, Digits_after_dp_X);  //Ceiling in specific digits.
            int label_pos_X_init = (int)((X_start_floor - X_start) * (draw_width / (X_end - X_start))); //position on frame.

            for (i = 0; i <= Tick_num_X; i++) 
            {
                int label_pos_X = (int)(i * Tick_inc_X * (draw_width / (X_end - X_start))) + label_pos_X_init;

                if (label_pos_X >= 0 && label_pos_X <= draw_width)
                {
                    g.DrawLine(Plot_AxisPen, new Point(marginX1 + label_pos_X, canvas.Height - marginY1 - 6),
                                             new Point(marginX1 + label_pos_X, canvas.Height - marginY1));
                    RectangleF rect = new RectangleF(marginX1 + label_pos_X - draw_width / Tick_num_X / 2,
                                                            canvas.Height - marginY1 + fnt.Height / 2,
                                                            draw_width / Tick_num_X,
                                                            fnt.Height);
                    g.FillRectangle(Plot_Background, rect);
                    String tempstr = String.Empty;
                    for (j = 0; j < Digits_after_dp_X; j++) { tempstr += "0"; }
                    Double label_value = Math.Floor((X_start + Tick_inc_X * i)/pow10(Exponent_X) * pow10(Digits_after_dp_X))/pow10(Digits_after_dp_X);   
                    String fmtstr = string.Format("{0:0." + tempstr + "}",label_value);
                    g.DrawString(fmtstr, fnt, Plot_FontColor, rect, stringFormat);
                }
            }

            Double Y_start_floor = Math.Floor(Y_start * Math.Pow(10, Digits_after_dp_Y)) / Math.Pow(10, Digits_after_dp_Y);  //floor in specfic digits.
            Double Y_end_ceiling = Math.Ceiling(Y_end * Math.Pow(10, Digits_after_dp_Y)) / Math.Pow(10, Digits_after_dp_Y);  //Ceiling in specific digits
            int label_pos_Y_end = (int)((Y_end_ceiling - Y_start) * (draw_height / (Y_end - Y_start)));

            for (i = 0; i <= Tick_num_Y; i++)
            {
                //int label_pos_Y = (int)(i * draw_height / Tick_num_Y);
                int label_pos_Y = label_pos_Y_end - (int)(i * Tick_inc_Y * (draw_height / (Y_end - Y_start)));
                if (label_pos_Y >= 0 && label_pos_Y <= draw_height)
                {
                    g.DrawLine(Plot_AxisPen, new Point(marginX1 + 6, marginY2 + label_pos_Y),
                                                new Point(marginX1, marginY2 + label_pos_Y));
                    RectangleF rect = new RectangleF(fnt.Height, marginY2 + label_pos_Y - fnt.Height / 2,
                                                        marginX1 - fnt.Height * 2, fnt.Height);    //X,Y,Width,Height
                    g.FillRectangle(Plot_Background, rect);
                    String tempstr = String.Empty;
                    for (j = 0; j < Digits_after_dp_Y; j++) { tempstr += "0"; }
                    Double label_value = Math.Floor((Y_start + Tick_inc_Y * i)/pow10(Exponent_Y) * pow10(Digits_after_dp_Y)) / pow10(Digits_after_dp_Y);   
                    String fmtstr = string.Format("{0:0." + tempstr + "}",label_value);
                    g.DrawString(fmtstr, fnt, Plot_FontColor, rect, stringFormatR);
                }
            }

            //Draw X-axis title
            RectangleF rect2 = new RectangleF(marginX1, canvas.Height - marginY1 + title_fnt.Height * 3 / 2, draw_width, title_fnt.Height);
            g.FillRectangle(Plot_Background, rect2);
            if (Exponent_X != 0) g.DrawString(TitleX + String.Format(" x 10^{0:0}", Exponent_X), title_fnt, Plot_FontColor, rect2, stringFormat);
            else g.DrawString(TitleX, title_fnt, Plot_FontColor, rect2, stringFormat);

            //Draw Y-axis title margine Y1 bottom.
            g.TranslateTransform(canvas.Width / 2F, canvas.Height / 2F);    //Change origin for rotation.
            g.RotateTransform(-90.0F);                                      //Rotate image.
            Point draw_point = new Point(marginY2/2, -draw_width / 2 - (int)(title_fnt.Height*2.5));  // Point(X,Y) X: vertical Y:Horizontal (X-Y switched)
            if (Exponent_Y != 0) g.DrawString(TitleY + String.Format(" x 10^{0:0}", Exponent_Y), title_fnt, Plot_FontColor,draw_point, stringFormat);
            else g.DrawString(TitleY, title_fnt, Plot_FontColor, draw_point, stringFormat);
            g.ResetTransform();

            //Release
            g.Dispose();

            return canvas;
        }

        private static Point[] RDP(Point[] PointList, Double epsilon)
        {
            Int32 index = 0;
            Int32 i;
            double d;
            double dmax = 0;
            Point[] Result1, Result2;
            Point[] result;

            for (i = 1; i < PointList.Length - 1; i++)
            {
                d = PerpendicularDistance(PointList[i], PointList[0], PointList[PointList.Length - 1]);
                if (d > dmax)
                {
                    index = i;
                    dmax = d;
                }
            }

            if (dmax > epsilon)
            {
                Point[] TmpList1 = new Point[index + 1];
                Point[] TmpList2 = new Point[PointList.Length - index];
                Array.Copy(PointList, 0, TmpList1, 0, index + 1);
                Array.Copy(PointList, index, TmpList2, 0, PointList.Length - index);
                Result1 = RDP(TmpList1, epsilon);
                Result2 = RDP(TmpList2, epsilon);

                result = new Point[Result1.Length + Result2.Length - 1];
                Array.Copy(Result1, 0, result, 0, Result1.Length - 1);
                Array.Copy(Result2, 0, result, Result1.Length - 1, Result2.Length);
            }
            else
            {
                result = new Point[2];
                result[0] = PointList[0];
                result[1] = PointList[PointList.Length - 1];
            }

            return (result);
        }

        static double PerpendicularDistance(Point pt, Point lineStart, Point lineEnd)
        {
            double dx = lineEnd.X - lineStart.X;
            double dy = lineEnd.Y - lineStart.Y;

            // Normalize
            double mag = Math.Sqrt(dx * dx + dy * dy);
            if (mag > 0.0)
            {
                dx /= mag;
                dy /= mag;
            }
            double pvx = pt.X - lineStart.X;
            double pvy = pt.Y - lineStart.Y;

            // Get dot product (project pv onto normalized direction)
            double pvdot = dx * pvx + dy * pvy;

            // Scale line direction vector and subtract it from pv
            double ax = pvx - pvdot * dx;
            double ay = pvy - pvdot * dy;

            return Math.Sqrt(ax * ax + ay * ay);
        }

        //Convert the real number into point data in picture box.
        //Used for plotting, marker, etc.
        private Point ValueToPoint(Dpoint value)
        {
            Point result = new Point();
            int draw_width = canvas.Width - marginX1 - marginX2;
            int draw_height = canvas.Height - marginY2 - marginY1;

            double tmpval = (value.X - X_start) / (X_end - X_start);
            result.X = (int)(tmpval * draw_width + marginX1);
            tmpval = (value.Y - Y_start) / (Y_end - Y_start);
            result.Y = (int)(-1 * tmpval * draw_height + marginY2 + draw_height);

            //Force to be able to plot in view setting.
            if (result.X <= marginX1) result.X = marginX1;
            else if (result.X >= canvas.Width - marginX2) result.X = canvas.Width - marginX2;
            if (result.Y <= marginY2) result.Y = marginY2;
            else if (result.Y >= canvas.Height - marginY1) result.Y = canvas.Height - marginY1;

            return result;
        }

        //Convert point data in picture box into SIGNAL value..
        //Used for plotting, marker, etc.
        public Dpoint PointToValue(Point P)
        {
            Dpoint result = new Dpoint();
            result.X = (P.X - marginX1) / (canvas.Width - marginX1 - marginX2) * (X_end - X_start);
            result.Y = (canvas.Height - P.Y - marginY1) / (canvas.Height - marginY2 - marginY1) * (Y_end - Y_start);
            return result;
        }
    }



    //###############################################################################
    //2D version. (not updated since Ver 1.0.0)
    //###############################################################################


    public class SIGNAL2D
    {
        public Double X;
        public Double Y;
        public Double Z;

        public SIGNAL2D()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public SIGNAL2D(Double x, Double y, Double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public class DATAPLOT2D
    {

        public struct Dpoint
        {
            public Double X;
            public Double Y;

            public Dpoint(Double setX, Double setY)
            {
                X = setX;
                Y = setY;
            }
        }

        //Consts
        private static readonly Font fnt = new Font("Arial", 10);
        private static readonly Font title_fnt = new Font("Aliquam", 12);
        private const int MAX_PLOT = 6;
        private const int TICK_X_NUM = 5;
        private const int TICK_Y_NUM = 5;
        private const int marginX1 = 90;
        private const int marginX2 = 40;
        private const int marginY1 = 50;
        private const int marginY2 = 40;
        private const float PLOT_LINE_WIDTH = 1.0F;
        private const float AXIS_LINE_WIDTH = 1.5F;


        public Int32 Wavecount;
        private List<bool> plotflag;
        private List<SIGNAL2D[]> wavelist;
        private List<Int32> Nx = new List<Int32>();
        private List<Int32> Ny = new List<Int32>();

        private List<Dpoint> XYMarker = new List<Dpoint>();
        private List<Double> XMarker = new List<Double>();
        private List<Double> YMarker = new List<Double>();

        private List<Color> XYMarker_color = new List<Color>();
        private List<Color> XMarker_color = new List<Color>();
        private List<Color> YMarker_color = new List<Color>();

        String TitleX, TitleY, TitleZ;

        //viewing setting.
        public Double X_start, X_end;
        public Double Y_start, Y_end;
        public Double Z_start, Z_end;

        //Bitmap
        public Bitmap canvas2D;
        public Brush Plot_Background = Brushes.Black;
        public Pen Plot_AxisPen = Pens.Yellow;
        public Brush Plot_FontColor = Brushes.White;


        public DATAPLOT2D(Int32 plot_w, Int32 plot_h)
        {
            Wavecount = 0;
            wavelist = new List<SIGNAL2D[]>();
            plotflag = new List<bool>();
            TitleX = "X";
            TitleY = "Y";
            TitleY = "Z";

            canvas2D = new Bitmap(plot_w,plot_h);
        }

        public DATAPLOT2D(Int32 plot_w, Int32 plot_h, String Xtitle, String Ytitle, String Ztitle)
        {
            Wavecount = 0;

            wavelist = new List<SIGNAL2D[]>();
            plotflag = new List<bool>();

            TitleX = Xtitle;
            TitleY = Ytitle;
            TitleZ = Ztitle;
        }

        //clear waves.
        public void AllClear()
        {
            wavelist.Clear();
            plotflag.Clear();
            TitleX = String.Empty;
            TitleY = String.Empty;
            Wavecount = 0;
            Nx.Clear();
            Ny.Clear();
            XYMarker.Clear();
        }

        public enum MARKERTYPE : byte
        {
            X = 0,
            Y = 1,
            XY = 2
        }

        public void SetMarker(MARKERTYPE type, Double X, Double Y, Color color)
        {
            switch (type)
            {
                case MARKERTYPE.XY:
                    XYMarker.Add(new Dpoint(X, Y));
                    XYMarker_color.Add(color);
                    break;
                default:
                    break;
            }
        }

        public void RemoveMarker(MARKERTYPE type, Double X, Double Y)
        {
            switch (type)
            {
                case MARKERTYPE.XY:
                    for (Int32 i = 0; i < XYMarker.Count; i++)
                    {
                        if (XYMarker[i].X == X && XYMarker[i].Y == Y)
                        {
                            XYMarker.RemoveAt(i);
                            XYMarker_color.RemoveAt(i);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        public void RemoveMarkerAll(MARKERTYPE type)
        {
            switch (type)
            {
                case MARKERTYPE.XY:
                    XYMarker.Clear();
                    break;
                default:
                    break;
            }
        }



        public void Addwave(SIGNAL2D[] wave)
        {
            wavelist.Add(wave);
            plotflag.Add(true);
            FittingView();
            Wavecount++;
            Double[] testarray = new Double[wave.Length];
            for (Int32 i = 0; i < wave.Length; i++)
            {
                testarray[i] = wave[i].X;
            }
            Int32 count = 0;
            for (Int32 i = 0; i < wave.Length; i++)
            {
                if (wave[i].X == wave[0].X) count++;
            }

            Ny.Add(count);
            Nx.Add((Int32)(wave.Length / count));
        }

        //Replace waveform.
        public void Replacewave(SIGNAL2D[] wave, int index)
        {
            wavelist[index] = wave;
        }

        //Update the view setting so that all existing wave can be fit in plot area.
        public void FittingView()
        {
            Int32 i, j;
            List<double> x_array = new List<double>();
            List<double> y_array = new List<double>();
            List<double> z_array = new List<double>();

            for (i = 0; i < wavelist.Count; i++)
            {
                if (plotflag[i])
                {
                    for (j = 0; j < wavelist[i].Length; j++)
                    {
                        x_array.Add(wavelist[i][j].X);
                        y_array.Add(wavelist[i][j].Y);
                        z_array.Add(wavelist[i][j].Z);
                    }
                }
            }

            X_start = x_array.Min();
            X_end = x_array.Max();
            Y_start = y_array.Min();
            Y_end = y_array.Max();
            Z_start = z_array.Min();
            Z_end = z_array.Max();

        }

        /*     /(0,0) in picture box
        *   *_____________________________
        *   |  ____________Y2___________  |
        *   |  |                       |  |
        *   |  |                       |  |
        *   |  |                       |  |
        *   |X1|                       |X2| canvas.Height
        *   |  |                       |  |
        *   |  | /(data origin)        |  |
        *   |  *_______________________|  |
        *   |______________Y1_____________|
        *              canvas.Width
        */

        public Bitmap Plot2D()
        {
            int i, j;

            int draw_width = canvas2D.Width - marginX1 - marginX2;
            int draw_height = canvas2D.Height - marginY2 - marginY1;
            int draw_centerX = draw_width / 2 + marginX1;
            int draw_centerY = draw_height / 2 + marginY2;

            //Create a Graphics Object
            Graphics g = Graphics.FromImage(canvas2D);

            //Refresh Graph
            g.FillRectangle(Plot_Background, 0, 0, canvas2D.Width, canvas2D.Height);

            // Create a StringFormat object with the each line of text, and the block
            // of text centered on the page.
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Far;
            stringFormat.LineAlignment = StringAlignment.Far;

            //Plot waveform
            for (Int32 m = 0; m < wavelist.Count; m++)
            {
                if (plotflag[m])
                {
                    //Calculate pixel of each 2D data.
                    Int32 pixel_height = (Int32)(draw_height / Ny[m]);
                    Int32 pixel_width = (Int32)(draw_width / Nx[m]);

                    //Plot data first
                    for (j = 0; j < wavelist[m].Length; j++)
                    {
                        MGraph MG = new MGraph();

                        Point plotp = new Point();
                        double tmpval = (wavelist[m][j].X - X_start) / (X_end - X_start);
                        plotp.X = (Int32)(tmpval * (draw_width - pixel_width)) + marginX1;
                        tmpval = (wavelist[m][j].Y - Y_start) / (Y_end - Y_start);
                        plotp.Y = (Int32)(-1 * tmpval * (draw_height - pixel_height) + marginY2 + draw_height) - pixel_height;

                        Rectangle rect = new Rectangle(plotp.X, plotp.Y, pixel_width, pixel_height);
                        Color plot_color = MG.Makerainbow((double)(wavelist[m][j].Z), (double)Z_start, (double)Z_end);
                        //Color plot_color = MG.HeatMapColor((double)((wavelist[m][j].Z - Z_start) / (Z_end - Z_start)));
                        SolidBrush databrush = new SolidBrush(plot_color);
                        g.FillRectangle(databrush, rect);
                        //avoid space between pixels.
                        g.DrawRectangle(new Pen(plot_color, 1), rect);
                    }

                    //Draw the Frame of graph
                    g.DrawRectangle(Plot_AxisPen, marginX1, marginY2, draw_width, draw_height);

                    //Draw ticks.
                    Double Tick_X = Math.Abs(X_end - X_start) / (TICK_X_NUM-1);
                    Double Tick_Y = Math.Abs(Y_end - Y_start) / (TICK_Y_NUM -1);

                    Double Tick_dist_X = (draw_width - pixel_width) / (TICK_X_NUM-1);
                    Double Tick_dist_Y = (draw_height - pixel_height)/ (TICK_Y_NUM-1);

                    for (i = 0; i < TICK_X_NUM; i++)
                    {
                        g.DrawLine(Plot_AxisPen, new Point(marginX1 + (int)(i * Tick_dist_X), canvas2D.Height - marginY1 - 6),
                                                 new Point(marginX1 + (int)(i * Tick_dist_X), canvas2D.Height - marginY1));
                        Rectangle rect = new Rectangle(marginX1 + (int)(i * Tick_dist_X),
                                                       canvas2D.Height - marginY1 + fnt.Height / 2,
                                                       (int)Tick_dist_X,
                                                       fnt.Height);
                        g.FillRectangle(Plot_Background, rect);
                        g.DrawString(string.Format("{0:0.00e+00}", X_start + Tick_X * i), fnt, Plot_FontColor, rect, stringFormat);
                    }
                    for (i = 0; i < TICK_Y_NUM; i++)
                    {
                        g.DrawLine(Plot_AxisPen, new Point(marginX1 + 6, marginY2 + (int)(i * Tick_dist_Y)),
                                                 new Point(marginX1, marginY2 + (int)(i * Tick_dist_Y )));

                        Rectangle rect = new Rectangle(0, marginY2 + (int)((i + 0.5) * Tick_dist_Y)- fnt.Height / 2, marginX1, fnt.Height);
                        g.FillRectangle(Plot_Background, rect);
                        g.DrawString(string.Format("{0:f3}", Y_end - Tick_Y * i), fnt, Plot_FontColor, rect, stringFormat);
                    }

                    //Draw X Axis title
                    RectangleF rect2 = new RectangleF(marginX1, canvas2D.Height - marginY1 + fnt.Height * 3 / 2, draw_width, fnt.Height);
                    g.FillRectangle(Plot_Background, rect2);
                    g.DrawString(TitleX, fnt, Plot_FontColor, rect2, stringFormat);
                    //Draw Y Axis title
                    //Make rotated rectangle 
                    Rectangle rect3 = new Rectangle(0, 0, draw_height, fnt.Height);
                    g.RotateTransform(-90.0F);
                    g.TranslateTransform(rect3.Left, rect3.Bottom, System.Drawing.Drawing2D.MatrixOrder.Append);
                    g.DrawString(TitleY, fnt, Plot_FontColor, rect3, stringFormat);
                    g.ResetTransform();

                    //Draw Marker
                    for (j = 0; j < XYMarker.Count; j++)
                    {
                        MGraph MG = new MGraph();

                        Point plotp = new Point();
                        double tmpval = (XYMarker[j].X - X_start) / (X_end - X_start);
                        plotp.X = (Int32)(tmpval * (draw_width - pixel_width)) + marginX1;
                        tmpval = (XYMarker[j].Y - Y_start) / (Y_end - Y_start);
                        plotp.Y = (Int32)(-1 * tmpval * (draw_height - pixel_height) + marginY2 + draw_height) - pixel_height;
                        Rectangle rect = new Rectangle(plotp.X, plotp.Y, pixel_width, pixel_height);
                        g.DrawRectangle(new Pen(XYMarker_color[j], 1), rect);
                    }
                }
            }

            //Release
            g.Dispose();

            return canvas2D;
        }

        public Point PointToIndex(Point P)
        {
            Point result = new Point();

            int draw_width = canvas2D.Width - marginX1 - marginX2;
            int draw_height = canvas2D.Height - marginY2 - marginY1;

            //Calculate pixel of each 2D data.
            Int32 pixel_height = (Int32)(draw_height / Ny[Wavecount - 1]);
            Int32 pixel_width = (Int32)(draw_width / Nx[Wavecount - 1]);

            result.X = (P.X - marginX1) / (pixel_width);
            if(result.X >= Nx[Wavecount-1]) result.X = Nx[Wavecount-1]-1;
            result.Y = (canvas2D.Height - P.Y - marginY1) / (pixel_height);
            if (result.Y >= Ny[Wavecount - 1]) result.Y = Ny[Wavecount - 1] - 1;
            return result;
        }

    }
    //common functions.
    public class MGraph{

        //Not good.
        public Color HeatMapColor(double value)
        {
            double R = 0;
            double G = 0;
            double B = 0;

            Byte r, g, b;

            const Int32 Num_colors = 5;
            double[,] colors = { { 0, 0, 1 }, { 0, 1, 1 },{ 0, 1, 0 }, { 1, 1, 0 }, { 1, 0, 0 } }; //Blue, Cyan,Green,Yellow, Red.
            Int32 idx1, idx2;
            double fractBetween = 0;

            if (value <= 0) { idx1 = idx2 = 0; }
            else if (value >= 1) { idx1 = idx2 = Num_colors - 1; }
            else
            {
                value = value * (Num_colors - 1);
                idx1 = (Int32)Math.Floor(value);
                idx2 = idx1 + 1;
                fractBetween = value - (double)idx1;
            }

            R = (colors[idx2, 0] - colors[idx1, 0]) * fractBetween + colors[idx1, 0];
            G = (colors[idx2, 1] - colors[idx1, 1]) * fractBetween + colors[idx1, 1];
            B = (colors[idx2, 2] - colors[idx1, 2]) * fractBetween + colors[idx1, 2];

            r = (Byte)(R * 65535);
            g = (Byte)(G * 65535);
            b = (Byte)(B * 65535);

            Color res = new Color();
            res = Color.FromArgb(r, g, b);
            return res;

        }

        //Better one.
        public Color Makerainbow(double x, double x_min, double x_max)
            {
            double H;
            double S = 1.0;
            double V = 0.8;
            double f;
            double p, q, t;
            double R = 0;
            double G = 0;
            double B = 0;

            int i;
            Byte r, g, b;

            H = (x - x_min) / (x_max - x_min) * 360.0;

            if (S == 0)
            {
                R = V;
                G = V;
                B = V;
            }
            if (H == 360.0) H = 0;
            i = (int)(Math.Floor(H / 60.0)) % 6;
            f = H / 60.0 - Math.Floor(H / 60.0);

            p = V * (1.0 - S);
            q = V * (1.0 - (S * f));
            t = V * (1.0 - (S * (1.0 - f)));
            if (i == 0) { R = V; G = t; B = p; }
            if (i == 1) { R = q; G = V; B = p; }
            if (i == 2) { R = p; G = V; B = t; }
            if (i == 3) { R = p; G = q; B = V; }
            if (i == 4) { R = t; G = p; B = V; }
            if (i == 5) { R = V; G = p; B = q; }
            r = (Byte)(R * 65535);
            g = (Byte)(G * 65535);
            b = (Byte)(B * 65535);
            Color res = new Color();
            res = Color.FromArgb(r, g, b);
            return res;
        }

    }
}

