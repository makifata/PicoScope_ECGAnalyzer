/* 
 * MDataplot Ver. 1.2
 * Mitsutoshi Makihata
 * 
 * 190116 Improve the Autotick function
 * 200318 Bugfix in fittingview(). C and Y can not automaticaly fit simultaneously..
 * 200330 Bugfix in AutoTicking of Y-axis.
 * 200401 Improve Auto Ticking and Engineering notation
 */ 

/*
 TO DO list
  + Readability update, using Properties?
  + not class, but 
 */

using System;
using System.Collections.Generic;//List
using System.Linq;
using System.Text;
using System.Drawing;

namespace MDataplot
{

    //Constant value to be used both 1D and 2D.
    static class Consts
    {
        //Consts
        public static readonly Font fnt = new Font("Arial",10);
        public static readonly Font title_fnt = new Font("Aliquam", 12);
        public const int MAX_PLOT = 6;
        public const int TICK_X_NUM = 5;
        public const int TICK_Y_NUM = 5;
        public const int marginX1 = 90;
        public const int marginX2 = 40;
        public const int marginY1 = 50;
        public const int marginY2 = 40;
        public const float PLOT_LINE_WIDTH = 1.0F;
        public const float AXIS_LINE_WIDTH = 1.5F;
    }

    // this should be struct?
    public class SIGNAL
    {
        public Double X;
        public Double Y;
        public Boolean valid;

        //if no values are passed, just make dummy data (Empty).
        public SIGNAL()
        {
            X = 0;
            Y = 0;
            valid = false;
        }

        public SIGNAL(Double x, Double y)
        {
              X = x;
              Y = y;
              valid = true;
        }

    }

    //Alternate SIGNAL[] to WAVE (to separate graphics and data handling)
    public class WAVE
    {
        public SIGNAL[] wave;
        
        //create signal array.
        public WAVE(int size)
        {
            wave = new SIGNAL[size];
            for (int i = 0; i < size; i++) wave[i] = new SIGNAL();
        }

        // Clear wave
        public void Clear()
        {
            for (int i = 0; i < wave.Length; i++) { wave[i].valid = false; }
        }
        //"Indexer or Smart array
        public SIGNAL this[int index]
        {
            get { return wave[index]; }
            set { wave[index] = value; }
        }

        //adding point
        public void AddPoint(double newx, double newy)
        {
            Int32 i, j;

            //search for last data
            for (i = 0; i < wave.Length; i++) if (!wave[i].valid) break;
            if (i == wave.Length)
            {
                for (j = 0; j < wave.Length - 1; j++)
                {
                    wave[j] = wave[j + 1];
                }
                wave[wave.Length - 1].X = newx;
                wave[wave.Length - 1].Y = newy;
                wave[wave.Length - 1].valid = true;
            }
            else
            {
                wave[i].X = newx;
                wave[i].Y = newy;
                wave[i].valid = true;
            }
        }
    }

    class DATAPLOT
    {
        //List of traces
        private List<SIGNAL[]> wavelist = new List<SIGNAL[]>();     //List of waves
        private List<bool> plotflag  = new List<bool>();            //show or hide of each wave.

        //Markers
        private List<Double> X_marker =  new List<Double>();        //Vertical Coursol ?(Global)
        private List<Double> Y_marker = new List<Double>();         //Horizontal Coursol ?(Global)
        private List<Color> XMarker_color = new List<Color>();      //Color of Vertical Coursol
        private List<Color> YMarker_color = new List<Color>();      //Color of Horizontal Coursol

        //Plot setting
        private String TitleX;
        private String TitleY;
        public Boolean Auto_tick = true;  //True: Auto (5 ticks) False: Manual (use tick_inc)
        public int Tick_num_X;
        public int Tick_num_Y;
        public Double Tick_inc_X;
        public Double Tick_inc_Y;
        public int Exponent_X;  //10^{??}
        public int Exponent_Y;  //10^{??}
        public int Digits_after_dp_X;
        public int Digits_after_dp_Y;

        //Plot area
        public Double X_start;
        public Double X_end;
        public Double Y_start;
        public Double Y_end;

        //Bitmap, pen, brush
        public Bitmap canvas;
        public Brush Plot_Background = Brushes.Black;
        public Pen   Plot_AxisPen = Pens.Yellow;
        public Brush Plot_FontColor = Brushes.White;

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
            Plot_AxisPen = new Pen(axis, Consts.AXIS_LINE_WIDTH);
        }
        
        //Add single SIGNAL point to existing waveindex. Return false if wave is not exist.
        //Useful for realtime plotting. No fitting.
        public Boolean AddPoint(int index, SIGNAL newpoint)
        {
            Int32 i,j;
            //Make sure newpoint's flag is on.
            newpoint.valid = true;

            if (index >= 0 && index < wavelist.Count)
            {
                //search for last data
                for (i = 0; i < wavelist[index].Length; i++) if (!wavelist[index][i].valid) break;
                if (i == wavelist[index].Length)
                {
                    for (j = 0; j < wavelist[index].Length - 1; j++)
                    {
                        wavelist[index][j] = wavelist[index][j + 1];
                    }
                    wavelist[index][wavelist[index].Length - 1] = newpoint;
                }
                else
                {
                    wavelist[index][i] = newpoint;
                }
                return true;
            }
            else
            {
                return false;   //ERROR
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

        //Clear specific wave using waveindex
        public bool ClearWave(Int32 waveindex)
        {
            if (waveindex < 0 || waveindex >= wavelist.Count)
            {
                return false;
            }
            else
            {
                for(Int16 i = 0; i < wavelist[waveindex].Length; i++)
                {
                    wavelist[waveindex][i].valid = false;
                }
                return true;
            }
        }

        //Add wave, fitting, and return waveindex.
        public int Addwave(SIGNAL[] wave)
        {
            //Add new wave.
            wavelist.Add(wave);
            plotflag.Add(true);
            return wavelist.Count - 1;
        }

        public SIGNAL[] Getwave(Int32 waveindex)
        {
            return wavelist[waveindex];
        }

        //Replace waveform.
        public void Replacewave(SIGNAL[] wave, int index)
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

            Double scaleX = (X_end - X_start) / (canvas.Width - Consts.marginX1 - Consts.marginX2);
            Double scaleY = (Y_end - Y_start) / (canvas.Height - Consts.marginY2 - Consts.marginY1);

            X_end = X_start + (P2.X - Consts.marginX1) * scaleX; //Do this first
            X_start += (P1.X - Consts.marginX1) * scaleX;

            Y_start = Y_end - (P2.Y - Consts.marginY2) * scaleY;//Do this first
            Y_end -= (P1.Y - Consts.marginY2) * scaleY;
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
                    for (Int32 j = 0; j < wavelist[i].Length; j++)
                    {
                        if (wavelist[i][j].X > X1 && wavelist[i][j].X < X2 && wavelist[i][j].valid)
                        {
                            testarray.Add(wavelist[i][j].Y);
                        }
                    }
                }
            }
            Y_start  = testarray.Min() - (testarray.Max() - testarray.Min()) * 0.01;
            Y_end    = testarray.Max() + (testarray.Max() - testarray.Min()) * 0.01;
            
        }

        private Boolean isinrange(SIGNAL point)
        {
            //return (point.X >= X_start && point.X <= X_end && point.Y >= Y_start && point.Y <= Y_end);
            return (point.X >= X_start && point.X <= X_end);
        }

        //Update the view setting so that all existing wave can be fit in plot area.
        public void FittingView(Boolean X, Boolean Y)
        {
            Int32 i, j;
            List<Double> x_array = new List<Double>();
            List<Double> y_array = new List<Double>();

            for (i = 0; i < wavelist.Count; i++)
            {
                if (plotflag[i])
                {
                    for (j = 0; j < wavelist[i].Length; j++)
                    {
                        if (wavelist[i][j].valid)
                        {
                            if(X) x_array.Add(wavelist[i][j].X);
                            if(Y) y_array.Add(wavelist[i][j].Y);
                        }
                    }
                }
            }

            if (x_array.Count >= 2 && X)
            {
                X_start = x_array.Min();
                X_end = x_array.Max();
            }

            if (y_array.Count >= 2 && Y)
            {
                Y_start = y_array.Min();
                Y_end = y_array.Max();
                //Y_start = y_array.Min() - (y_array.Max() - y_array.Min()) * 0.01;
                //Y_end   = y_array.Max() + (y_array.Max() - y_array.Min()) * 0.01;
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
                XMarker_color.Add(Color.Black);
            }
            else
            {
                Y_marker.Add(value);
                YMarker_color.Add(Color.Black);
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
            int draw_width = canvas.Width - Consts.marginX1 - Consts.marginX2;
            int draw_height = canvas.Height - Consts.marginY2 - Consts.marginY1;
            int draw_centerX = draw_width / 2 + Consts.marginX1;
            int draw_centerY = draw_height / 2 + Consts.marginY2;

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
                    Double Tick_log = (X_end - X_start) / (i + Consts.TICK_X_NUM) * Math.Pow(10, -Exponent_X); //Pick tick number can divide a range with minimum error.
                    Double temp = Math.Abs(Math.Round(Tick_log, Digits_after_dp_X) - Tick_log);
                    if (i == -2 || err_min > temp)
                    {
                        err_min = temp;
                        Tick_num_X = i + Consts.TICK_X_NUM;
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
                    Double Tick_log = (Y_end - Y_start) / (i + Consts.TICK_Y_NUM) * Math.Pow(10, -Exponent_Y); //Pick tick number can divide a range with minimum error.
                    Double temp = Math.Abs(Math.Round(Tick_log, Digits_after_dp_Y) - Tick_log);
                    if (i == -2 || err_min > temp)
                    {
                        err_min = temp;
                        Tick_num_Y = i + Consts.TICK_Y_NUM;
                    }
                }
                Tick_inc_Y = Math.Abs(Y_end - Y_start) / Tick_num_Y;

            }

            //Draw the Frame of graph
            g.DrawRectangle(Plot_AxisPen, Consts.marginX1, Consts.marginY2, draw_width, draw_height);

            //Plot signal
            for (i = 0; i < wavelist.Count; i++)
            {
                if (plotflag[i])
                {
                    //scale to picture box (integer)
                    Point[] ps = new Point[wavelist[i].Length];
                    Point last_valid = new Point(0, 0);
                    for (j = 0; j < wavelist[i].Length; j++)
                    {
                        if (!wavelist[i][j].valid)
                        {
                            ps[j] = last_valid;
                        }
                        else
                        {
                            last_valid = ValueToPoint(wavelist[i][j]);
                            ps[j] = ValueToPoint(wavelist[i][j]);
                        }
                    }

                    //plot waveform
                    //Pen graphpen = new Pen (Makerainbow((double)i, 0.0, (double)MAX_PLOT),PLOT_LINE_WIDTH);
                    Pen graphpen;
                    switch (i)
                    {
                        case 0:
                            graphpen = new Pen(Color.Yellow, Consts.PLOT_LINE_WIDTH);
                            break;
                        case 1:
                            graphpen = new Pen(Color.Blue, Consts.PLOT_LINE_WIDTH);
                            break;
                        case 2:
                            graphpen = new Pen(Color.Green, Consts.PLOT_LINE_WIDTH);
                            break;
                        case 3:
                            graphpen = new Pen(Color.Purple, Consts.PLOT_LINE_WIDTH);
                            break;
                        default:
                            graphpen = new Pen(Color.Black, Consts.PLOT_LINE_WIDTH);
                            break;
                    }
                    if (ps.Length >= 2)
                    {
                        //smoothing!
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        g.DrawLines(graphpen, ps);
                    }
                }

                //Plot Markers
                Pen markpen;
                for (j = 0; j < X_marker.Count; j++)
                {
                    markpen = new Pen(XMarker_color[j], 1);
                    SIGNAL temp = new SIGNAL(X_marker[j], Y_start);
                    Point MV1A = ValueToPoint(temp);
                    temp = new SIGNAL(X_marker[j], Y_end);
                    Point MV1B = ValueToPoint(temp);
                    g.DrawLine(markpen, MV1A, MV1B);
                }
                for (j = 0; j < Y_marker.Count; j++)
                {
                    markpen = new Pen(YMarker_color[j], 1);
                    SIGNAL temp = new SIGNAL(X_start, Y_marker[j]);
                    Point MV1A = ValueToPoint(temp);
                    temp = new SIGNAL(X_end, Y_marker[j]);
                    Point MV1B = ValueToPoint(temp);
                    g.DrawLine(markpen, MV1A, MV1B);
                }
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
                    g.DrawLine(Plot_AxisPen, new Point(Consts.marginX1 + label_pos_X, canvas.Height - Consts.marginY1 - 6),
                                             new Point(Consts.marginX1 + label_pos_X, canvas.Height - Consts.marginY1));
                    RectangleF rect = new RectangleF(Consts.marginX1 + label_pos_X - draw_width / Tick_num_X / 2,
                                                            canvas.Height - Consts.marginY1 + Consts.fnt.Height / 2,
                                                            draw_width / Tick_num_X,
                                                            Consts.fnt.Height);
                    g.FillRectangle(Plot_Background, rect);
                    String tempstr = String.Empty;
                    for (j = 0; j < Digits_after_dp_X; j++) { tempstr += "0"; }
                    Double label_value = Math.Floor((X_start + Tick_inc_X * i)/pow10(Exponent_X) * pow10(Digits_after_dp_X))/pow10(Digits_after_dp_X);   
                    String fmtstr = string.Format("{0:0." + tempstr + "}",label_value);
                    g.DrawString(fmtstr, Consts.fnt, Plot_FontColor, rect, stringFormat);
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
                    g.DrawLine(Plot_AxisPen, new Point(Consts.marginX1 + 6, Consts.marginY2 + label_pos_Y),
                                                new Point(Consts.marginX1, Consts.marginY2 + label_pos_Y));
                    RectangleF rect = new RectangleF(Consts.fnt.Height, Consts.marginY2 + label_pos_Y - Consts.fnt.Height / 2,
                                                        Consts.marginX1 - Consts.fnt.Height * 2, Consts.fnt.Height);    //X,Y,Width,Height
                    g.FillRectangle(Plot_Background, rect);
                    String tempstr = String.Empty;
                    for (j = 0; j < Digits_after_dp_Y; j++) { tempstr += "0"; }
                    Double label_value = Math.Floor((Y_start + Tick_inc_Y * i)/pow10(Exponent_Y) * pow10(Digits_after_dp_Y)) / pow10(Digits_after_dp_Y);   
                    String fmtstr = string.Format("{0:0." + tempstr + "}",label_value);
                    g.DrawString(fmtstr, Consts.fnt, Plot_FontColor, rect, stringFormatR);
                }
            }

            //Draw X-axis title
            RectangleF rect2 = new RectangleF(Consts.marginX1, canvas.Height - Consts.marginY1 + Consts.title_fnt.Height * 3 / 2, draw_width, Consts.title_fnt.Height);
            g.FillRectangle(Plot_Background, rect2);
            if (Exponent_X != 0) g.DrawString(TitleX + String.Format(" x 10^{0:0}", Exponent_X), Consts.title_fnt, Plot_FontColor, rect2, stringFormat);
            else g.DrawString(TitleX, Consts.title_fnt, Plot_FontColor, rect2, stringFormat);

            //Draw Y-axis title margine Y1 bottom.
            g.TranslateTransform(canvas.Width / 2F, canvas.Height / 2F);    //Change origin for rotation.
            g.RotateTransform(-90.0F);                                      //Rotate image.
            Point draw_point = new Point(Consts.marginY2/2, -draw_width / 2 - (int)(Consts.title_fnt.Height*2.5));  // Point(X,Y) X: vertical Y:Horizontal (X-Y switched)
            if (Exponent_Y != 0) g.DrawString(TitleY + String.Format(" x 10^{0:0}", Exponent_Y), Consts.title_fnt, Plot_FontColor,draw_point, stringFormat);
            else g.DrawString(TitleY, Consts.title_fnt, Plot_FontColor, draw_point, stringFormat);
            g.ResetTransform();

            //Release
            g.Dispose();

            return canvas;
        }


        //Convert the real number into point data in picture box.
        //Used for plotting, marker, etc.
        private Point ValueToPoint(SIGNAL value)
        {
            Point result = new Point();
            int draw_width = canvas.Width - Consts.marginX1 - Consts.marginX2;
            int draw_height = canvas.Height - Consts.marginY2 - Consts.marginY1;

            double tmpval = (value.X - X_start) / (X_end - X_start);
            result.X = (int)(tmpval * draw_width + Consts.marginX1);
            tmpval = (value.Y - Y_start) / (Y_end - Y_start);
            result.Y = (int)(-1 * tmpval * draw_height + Consts.marginY2 + draw_height);

            //Force to be able to plot in view setting.
            if (result.X <= Consts.marginX1) result.X = Consts.marginX1;
            else if (result.X >= canvas.Width - Consts.marginX2) result.X = canvas.Width - Consts.marginX2;
            if (result.Y <= Consts.marginY2) result.Y = Consts.marginY2;
            else if (result.Y >= canvas.Height - Consts.marginY1) result.Y = canvas.Height - Consts.marginY1;

            return result;
        }

        //Convert point data in picture box into SIGNAL value..
        //Used for plotting, marker, etc.
        public SIGNAL PointToValue(Point P)
        {
            SIGNAL result = new SIGNAL();
            result.X = (P.X - Consts.marginX1) / (canvas.Width - Consts.marginX1 - Consts.marginX2) * (X_end - X_start);
            result.Y = (canvas.Height - P.Y - Consts.marginY1) / (canvas.Height - Consts.marginY2 - Consts.marginY1) * (Y_end - Y_start);
            return result;
        }
    }






    //###############################################################################
    //2D version. (still remain old version like Ver1.0...



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
        public Int32 Wavecount;
        private List<bool> plotflag;
        private List<SIGNAL2D[]> wavelist;
        private List<Int32> Nx = new List<Int32>();
        private List<Int32> Ny = new List<Int32>();

        private List<SIGNAL> XYMarker = new List<SIGNAL>();
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
                    XYMarker.Add(new SIGNAL(X, Y));
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

            int draw_width = canvas2D.Width - Consts.marginX1 - Consts.marginX2;
            int draw_height = canvas2D.Height - Consts.marginY2 - Consts.marginY1;
            int draw_centerX = draw_width / 2 + Consts.marginX1;
            int draw_centerY = draw_height / 2 + Consts.marginY2;

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
                        plotp.X = (Int32)(tmpval * (draw_width - pixel_width)) + Consts.marginX1;
                        tmpval = (wavelist[m][j].Y - Y_start) / (Y_end - Y_start);
                        plotp.Y = (Int32)(-1 * tmpval * (draw_height - pixel_height) + Consts.marginY2 + draw_height) - pixel_height;

                        Rectangle rect = new Rectangle(plotp.X, plotp.Y, pixel_width, pixel_height);
                        Color plot_color = MG.Makerainbow((double)(wavelist[m][j].Z), (double)Z_start, (double)Z_end);
                        //Color plot_color = MG.HeatMapColor((double)((wavelist[m][j].Z - Z_start) / (Z_end - Z_start)));
                        SolidBrush databrush = new SolidBrush(plot_color);
                        g.FillRectangle(databrush, rect);
                        //avoid space between pixels.
                        g.DrawRectangle(new Pen(plot_color, 1), rect);
                    }

                    //Draw the Frame of graph
                    g.DrawRectangle(Plot_AxisPen, Consts.marginX1, Consts.marginY2, draw_width, draw_height);

                    //Draw ticks.
                    Double Tick_X = Math.Abs(X_end - X_start) / (Consts.TICK_X_NUM-1);
                    Double Tick_Y = Math.Abs(Y_end - Y_start) / (Consts.TICK_Y_NUM -1);

                    Double Tick_dist_X = (draw_width - pixel_width) / (Consts.TICK_X_NUM-1);
                    Double Tick_dist_Y = (draw_height - pixel_height)/ (Consts.TICK_Y_NUM-1);

                    for (i = 0; i < Consts.TICK_X_NUM; i++)
                    {
                        g.DrawLine(Plot_AxisPen, new Point(Consts.marginX1 + (int)(i * Tick_dist_X), canvas2D.Height - Consts.marginY1 - 6),
                                                 new Point(Consts.marginX1 + (int)(i * Tick_dist_X), canvas2D.Height - Consts.marginY1));
                        Rectangle rect = new Rectangle(Consts.marginX1 + (int)(i * Tick_dist_X),
                                                       canvas2D.Height - Consts.marginY1 + Consts.fnt.Height / 2,
                                                       (int)Tick_dist_X,
                                                       Consts.fnt.Height);
                        g.FillRectangle(Plot_Background, rect);
                        g.DrawString(string.Format("{0:0.00e+00}", X_start + Tick_X * i), Consts.fnt, Plot_FontColor, rect, stringFormat);
                    }
                    for (i = 0; i < Consts.TICK_Y_NUM; i++)
                    {
                        g.DrawLine(Plot_AxisPen, new Point(Consts.marginX1 + 6, Consts.marginY2 + (int)(i * Tick_dist_Y)),
                                                 new Point(Consts.marginX1, Consts.marginY2 + (int)(i * Tick_dist_Y )));

                        Rectangle rect = new Rectangle(0, Consts.marginY2 + (int)((i + 0.5) * Tick_dist_Y)- Consts.fnt.Height / 2, Consts.marginX1, Consts.fnt.Height);
                        g.FillRectangle(Plot_Background, rect);
                        g.DrawString(string.Format("{0:f3}", Y_end - Tick_Y * i), Consts.fnt, Plot_FontColor, rect, stringFormat);
                    }

                    //Draw X Axis title
                    RectangleF rect2 = new RectangleF(Consts.marginX1, canvas2D.Height - Consts.marginY1 + Consts.fnt.Height * 3 / 2, draw_width, Consts.fnt.Height);
                    g.FillRectangle(Plot_Background, rect2);
                    g.DrawString(TitleX, Consts.fnt, Plot_FontColor, rect2, stringFormat);
                    //Draw Y Axis title
                    //Make rotated rectangle 
                    Rectangle rect3 = new Rectangle(0, 0, draw_height, Consts.fnt.Height);
                    g.RotateTransform(-90.0F);
                    g.TranslateTransform(rect3.Left, rect3.Bottom, System.Drawing.Drawing2D.MatrixOrder.Append);
                    g.DrawString(TitleY, Consts.fnt, Plot_FontColor, rect3, stringFormat);
                    g.ResetTransform();

                    //Draw Marker
                    for (j = 0; j < XYMarker.Count; j++)
                    {
                        MGraph MG = new MGraph();

                        Point plotp = new Point();
                        double tmpval = (XYMarker[j].X - X_start) / (X_end - X_start);
                        plotp.X = (Int32)(tmpval * (draw_width - pixel_width)) + Consts.marginX1;
                        tmpval = (XYMarker[j].Y - Y_start) / (Y_end - Y_start);
                        plotp.Y = (Int32)(-1 * tmpval * (draw_height - pixel_height) + Consts.marginY2 + draw_height) - pixel_height;
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

            int draw_width = canvas2D.Width - Consts.marginX1 - Consts.marginX2;
            int draw_height = canvas2D.Height - Consts.marginY2 - Consts.marginY1;

            //Calculate pixel of each 2D data.
            Int32 pixel_height = (Int32)(draw_height / Ny[Wavecount - 1]);
            Int32 pixel_width = (Int32)(draw_width / Nx[Wavecount - 1]);

            result.X = (P.X - Consts.marginX1) / (pixel_width);
            if(result.X >= Nx[Wavecount-1]) result.X = Nx[Wavecount-1]-1;
            result.Y = (canvas2D.Height - P.Y - Consts.marginY1) / (pixel_height);
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

