namespace Emotion_beat
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.BTN_single = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.CMBOX_inputrange_A = new System.Windows.Forms.ComboBox();
            this.BTN_stop = new System.Windows.Forms.Button();
            this.BTN_start = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.TBOX_fsmpl = new System.Windows.Forms.TextBox();
            this.TBOX_sample_time = new System.Windows.Forms.TextBox();
            this.CHKBOX_en_A = new System.Windows.Forms.CheckBox();
            this.CHKBOX_en_B = new System.Windows.Forms.CheckBox();
            this.CMBOX_inputrange_B = new System.Windows.Forms.ComboBox();
            this.CMBOX_trg_ch = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.TBOX_trg_level = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.CMBOX_trig_direction = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.Group_Trigger = new System.Windows.Forms.GroupBox();
            this.CMBOX_A_ACDC = new System.Windows.Forms.ComboBox();
            this.CMBOX_B_ACDC = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.TBOX_timerange = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.BTN_Save = new System.Windows.Forms.Button();
            this.BTN_Load = new System.Windows.Forms.Button();
            this.BTN_dsp = new System.Windows.Forms.Button();
            this.CMBX_fmpl_unit = new System.Windows.Forms.ComboBox();
            this.CMBX_recordtime_unit = new System.Windows.Forms.ComboBox();
            this.CMBX_interval_unit = new System.Windows.Forms.ComboBox();
            this.Label_time = new System.Windows.Forms.Label();
            this.LBL_title = new System.Windows.Forms.Label();
            this.pictureBox5 = new System.Windows.Forms.PictureBox();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.BeatTimer = new System.Windows.Forms.Timer(this.components);
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.LBL_FFT = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.Group_Trigger.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // BTN_single
            // 
            this.BTN_single.ForeColor = System.Drawing.Color.Black;
            this.BTN_single.Location = new System.Drawing.Point(6, 19);
            this.BTN_single.Name = "BTN_single";
            this.BTN_single.Size = new System.Drawing.Size(78, 26);
            this.BTN_single.TabIndex = 0;
            this.BTN_single.Text = "SingleShot";
            this.BTN_single.UseVisualStyleBackColor = true;
            this.BTN_single.Click += new System.EventHandler(this.BTN_Single_clicked);
            // 
            // textBox1
            // 
            this.textBox1.AcceptsReturn = true;
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBox1.Location = new System.Drawing.Point(0, 516);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(1232, 56);
            this.textBox1.TabIndex = 1;
            // 
            // CMBOX_inputrange_A
            // 
            this.CMBOX_inputrange_A.FormattingEnabled = true;
            this.CMBOX_inputrange_A.Location = new System.Drawing.Point(66, 17);
            this.CMBOX_inputrange_A.Name = "CMBOX_inputrange_A";
            this.CMBOX_inputrange_A.Size = new System.Drawing.Size(121, 21);
            this.CMBOX_inputrange_A.TabIndex = 2;
            // 
            // BTN_stop
            // 
            this.BTN_stop.Location = new System.Drawing.Point(988, 383);
            this.BTN_stop.Name = "BTN_stop";
            this.BTN_stop.Size = new System.Drawing.Size(79, 32);
            this.BTN_stop.TabIndex = 6;
            this.BTN_stop.Text = "Stop";
            this.BTN_stop.UseVisualStyleBackColor = true;
            this.BTN_stop.Click += new System.EventHandler(this.BTN_stop_Click);
            // 
            // BTN_start
            // 
            this.BTN_start.Location = new System.Drawing.Point(914, 383);
            this.BTN_start.Name = "BTN_start";
            this.BTN_start.Size = new System.Drawing.Size(73, 32);
            this.BTN_start.TabIndex = 7;
            this.BTN_start.Text = "Start";
            this.BTN_start.UseVisualStyleBackColor = true;
            this.BTN_start.Click += new System.EventHandler(this.BTN_start_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 50;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(552, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Sample Frequency";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(90, 26);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(64, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "Record time";
            // 
            // TBOX_fsmpl
            // 
            this.TBOX_fsmpl.Location = new System.Drawing.Point(653, 24);
            this.TBOX_fsmpl.Name = "TBOX_fsmpl";
            this.TBOX_fsmpl.Size = new System.Drawing.Size(72, 20);
            this.TBOX_fsmpl.TabIndex = 16;
            this.TBOX_fsmpl.Text = "0.5";
            this.TBOX_fsmpl.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TBOX_sample_time
            // 
            this.TBOX_sample_time.Location = new System.Drawing.Point(160, 23);
            this.TBOX_sample_time.Name = "TBOX_sample_time";
            this.TBOX_sample_time.Size = new System.Drawing.Size(36, 20);
            this.TBOX_sample_time.TabIndex = 17;
            this.TBOX_sample_time.Text = "15";
            this.TBOX_sample_time.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // CHKBOX_en_A
            // 
            this.CHKBOX_en_A.AutoSize = true;
            this.CHKBOX_en_A.Checked = true;
            this.CHKBOX_en_A.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CHKBOX_en_A.ForeColor = System.Drawing.Color.White;
            this.CHKBOX_en_A.Location = new System.Drawing.Point(6, 19);
            this.CHKBOX_en_A.Name = "CHKBOX_en_A";
            this.CHKBOX_en_A.Size = new System.Drawing.Size(59, 17);
            this.CHKBOX_en_A.TabIndex = 14;
            this.CHKBOX_en_A.Text = "Enable";
            this.CHKBOX_en_A.UseVisualStyleBackColor = true;
            // 
            // CHKBOX_en_B
            // 
            this.CHKBOX_en_B.AutoSize = true;
            this.CHKBOX_en_B.ForeColor = System.Drawing.Color.White;
            this.CHKBOX_en_B.Location = new System.Drawing.Point(8, 19);
            this.CHKBOX_en_B.Name = "CHKBOX_en_B";
            this.CHKBOX_en_B.Size = new System.Drawing.Size(59, 17);
            this.CHKBOX_en_B.TabIndex = 22;
            this.CHKBOX_en_B.Text = "Enable";
            this.CHKBOX_en_B.UseVisualStyleBackColor = true;
            // 
            // CMBOX_inputrange_B
            // 
            this.CMBOX_inputrange_B.FormattingEnabled = true;
            this.CMBOX_inputrange_B.Location = new System.Drawing.Point(66, 17);
            this.CMBOX_inputrange_B.Name = "CMBOX_inputrange_B";
            this.CMBOX_inputrange_B.Size = new System.Drawing.Size(121, 21);
            this.CMBOX_inputrange_B.TabIndex = 19;
            // 
            // CMBOX_trg_ch
            // 
            this.CMBOX_trg_ch.FormattingEnabled = true;
            this.CMBOX_trg_ch.Location = new System.Drawing.Point(133, 14);
            this.CMBOX_trg_ch.Name = "CMBOX_trg_ch";
            this.CMBOX_trg_ch.Size = new System.Drawing.Size(118, 21);
            this.CMBOX_trg_ch.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(11, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Source";
            // 
            // TBOX_trg_level
            // 
            this.TBOX_trg_level.Location = new System.Drawing.Point(193, 41);
            this.TBOX_trg_level.Name = "TBOX_trg_level";
            this.TBOX_trg_level.Size = new System.Drawing.Size(57, 20);
            this.TBOX_trg_level.TabIndex = 12;
            this.TBOX_trg_level.Text = "10";
            this.TBOX_trg_level.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(11, 47);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(57, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Level [mV]";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(11, 97);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "Delay [%]";
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(24, 115);
            this.trackBar1.Maximum = 100;
            this.trackBar1.Minimum = -100;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(200, 45);
            this.trackBar1.TabIndex = 16;
            // 
            // CMBOX_trig_direction
            // 
            this.CMBOX_trig_direction.FormattingEnabled = true;
            this.CMBOX_trig_direction.Location = new System.Drawing.Point(133, 67);
            this.CMBOX_trig_direction.Name = "CMBOX_trig_direction";
            this.CMBOX_trig_direction.Size = new System.Drawing.Size(117, 21);
            this.CMBOX_trig_direction.TabIndex = 17;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.ForeColor = System.Drawing.Color.White;
            this.label7.Location = new System.Drawing.Point(11, 72);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(49, 13);
            this.label7.TabIndex = 18;
            this.label7.Text = "Direction";
            // 
            // Group_Trigger
            // 
            this.Group_Trigger.Controls.Add(this.label7);
            this.Group_Trigger.Controls.Add(this.CMBOX_trig_direction);
            this.Group_Trigger.Controls.Add(this.trackBar1);
            this.Group_Trigger.Controls.Add(this.label5);
            this.Group_Trigger.Controls.Add(this.label4);
            this.Group_Trigger.Controls.Add(this.TBOX_trg_level);
            this.Group_Trigger.Controls.Add(this.label3);
            this.Group_Trigger.Controls.Add(this.CMBOX_trg_ch);
            this.Group_Trigger.ForeColor = System.Drawing.Color.White;
            this.Group_Trigger.Location = new System.Drawing.Point(936, 24);
            this.Group_Trigger.Name = "Group_Trigger";
            this.Group_Trigger.Size = new System.Drawing.Size(264, 166);
            this.Group_Trigger.TabIndex = 10;
            this.Group_Trigger.TabStop = false;
            this.Group_Trigger.Text = "Trigger Setting";
            // 
            // CMBOX_A_ACDC
            // 
            this.CMBOX_A_ACDC.FormattingEnabled = true;
            this.CMBOX_A_ACDC.Items.AddRange(new object[] {
            "AC",
            "DC"});
            this.CMBOX_A_ACDC.Location = new System.Drawing.Point(193, 17);
            this.CMBOX_A_ACDC.Name = "CMBOX_A_ACDC";
            this.CMBOX_A_ACDC.Size = new System.Drawing.Size(57, 21);
            this.CMBOX_A_ACDC.TabIndex = 19;
            // 
            // CMBOX_B_ACDC
            // 
            this.CMBOX_B_ACDC.FormattingEnabled = true;
            this.CMBOX_B_ACDC.Items.AddRange(new object[] {
            "AC",
            "DC"});
            this.CMBOX_B_ACDC.Location = new System.Drawing.Point(193, 17);
            this.CMBOX_B_ACDC.Name = "CMBOX_B_ACDC";
            this.CMBOX_B_ACDC.Size = new System.Drawing.Size(57, 21);
            this.CMBOX_B_ACDC.TabIndex = 20;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.CHKBOX_en_A);
            this.groupBox1.Controls.Add(this.CMBOX_inputrange_A);
            this.groupBox1.Controls.Add(this.CMBOX_A_ACDC);
            this.groupBox1.ForeColor = System.Drawing.Color.White;
            this.groupBox1.Location = new System.Drawing.Point(12, 24);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(264, 48);
            this.groupBox1.TabIndex = 23;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "A";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.CHKBOX_en_B);
            this.groupBox2.Controls.Add(this.CMBOX_inputrange_B);
            this.groupBox2.Controls.Add(this.CMBOX_B_ACDC);
            this.groupBox2.ForeColor = System.Drawing.Color.White;
            this.groupBox2.Location = new System.Drawing.Point(282, 24);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(264, 48);
            this.groupBox2.TabIndex = 24;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "B";
            // 
            // TBOX_timerange
            // 
            this.TBOX_timerange.Location = new System.Drawing.Point(1084, 298);
            this.TBOX_timerange.Name = "TBOX_timerange";
            this.TBOX_timerange.Size = new System.Drawing.Size(62, 20);
            this.TBOX_timerange.TabIndex = 26;
            this.TBOX_timerange.Text = "2000";
            this.TBOX_timerange.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(950, 301);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(129, 13);
            this.label1.TabIndex = 25;
            this.label1.Text = "Signal Processing Interval";
            // 
            // BTN_Save
            // 
            this.BTN_Save.ForeColor = System.Drawing.Color.Black;
            this.BTN_Save.Location = new System.Drawing.Point(6, 51);
            this.BTN_Save.Name = "BTN_Save";
            this.BTN_Save.Size = new System.Drawing.Size(65, 35);
            this.BTN_Save.TabIndex = 27;
            this.BTN_Save.Text = "Save wave";
            this.BTN_Save.UseVisualStyleBackColor = true;
            this.BTN_Save.Click += new System.EventHandler(this.button1_Click);
            // 
            // BTN_Load
            // 
            this.BTN_Load.ForeColor = System.Drawing.Color.Black;
            this.BTN_Load.Location = new System.Drawing.Point(72, 51);
            this.BTN_Load.Name = "BTN_Load";
            this.BTN_Load.Size = new System.Drawing.Size(67, 35);
            this.BTN_Load.TabIndex = 28;
            this.BTN_Load.Text = "Load wave";
            this.BTN_Load.UseVisualStyleBackColor = true;
            this.BTN_Load.Click += new System.EventHandler(this.BTN_Load_Click);
            // 
            // BTN_dsp
            // 
            this.BTN_dsp.ForeColor = System.Drawing.Color.Black;
            this.BTN_dsp.Location = new System.Drawing.Point(145, 53);
            this.BTN_dsp.Name = "BTN_dsp";
            this.BTN_dsp.Size = new System.Drawing.Size(101, 35);
            this.BTN_dsp.TabIndex = 29;
            this.BTN_dsp.Text = "Signal Processing";
            this.BTN_dsp.UseVisualStyleBackColor = true;
            this.BTN_dsp.Click += new System.EventHandler(this.BTN_dsp_Click);
            // 
            // CMBX_fmpl_unit
            // 
            this.CMBX_fmpl_unit.FormattingEnabled = true;
            this.CMBX_fmpl_unit.Location = new System.Drawing.Point(731, 24);
            this.CMBX_fmpl_unit.Name = "CMBX_fmpl_unit";
            this.CMBX_fmpl_unit.Size = new System.Drawing.Size(66, 21);
            this.CMBX_fmpl_unit.TabIndex = 32;
            // 
            // CMBX_recordtime_unit
            // 
            this.CMBX_recordtime_unit.FormattingEnabled = true;
            this.CMBX_recordtime_unit.Location = new System.Drawing.Point(202, 23);
            this.CMBX_recordtime_unit.Name = "CMBX_recordtime_unit";
            this.CMBX_recordtime_unit.Size = new System.Drawing.Size(59, 21);
            this.CMBX_recordtime_unit.TabIndex = 33;
            // 
            // CMBX_interval_unit
            // 
            this.CMBX_interval_unit.FormattingEnabled = true;
            this.CMBX_interval_unit.Location = new System.Drawing.Point(1152, 298);
            this.CMBX_interval_unit.Name = "CMBX_interval_unit";
            this.CMBX_interval_unit.Size = new System.Drawing.Size(35, 21);
            this.CMBX_interval_unit.TabIndex = 34;
            // 
            // Label_time
            // 
            this.Label_time.AutoSize = true;
            this.Label_time.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label_time.ForeColor = System.Drawing.Color.Yellow;
            this.Label_time.Location = new System.Drawing.Point(1103, 5);
            this.Label_time.Name = "Label_time";
            this.Label_time.Size = new System.Drawing.Size(97, 18);
            this.Label_time.TabIndex = 35;
            this.Label_time.Text = "00:00:00 AM";
            this.Label_time.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LBL_title
            // 
            this.LBL_title.AutoSize = true;
            this.LBL_title.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LBL_title.ForeColor = System.Drawing.Color.White;
            this.LBL_title.Location = new System.Drawing.Point(28, 4);
            this.LBL_title.Name = "LBL_title";
            this.LBL_title.Size = new System.Drawing.Size(171, 20);
            this.LBL_title.TabIndex = 37;
            this.LBL_title.Text = "Emotion Beat (Ver 0.1)";
            // 
            // pictureBox5
            // 
            this.pictureBox5.Image = global::Emotion_beat.Properties.Resources.logo;
            this.pictureBox5.Location = new System.Drawing.Point(2, 1);
            this.pictureBox5.Name = "pictureBox5";
            this.pictureBox5.Size = new System.Drawing.Size(23, 23);
            this.pictureBox5.TabIndex = 38;
            this.pictureBox5.TabStop = false;
            this.pictureBox5.Click += new System.EventHandler(this.pictureBox5_Click);
            this.pictureBox5.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ShowItClickable);
            // 
            // pictureBox4
            // 
            this.pictureBox4.Image = global::Emotion_beat.Properties.Resources.cross;
            this.pictureBox4.Location = new System.Drawing.Point(1206, 0);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(26, 26);
            this.pictureBox4.TabIndex = 36;
            this.pictureBox4.TabStop = false;
            this.pictureBox4.Click += new System.EventHandler(this.pictureBox4_Click);
            this.pictureBox4.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ShowItClickable);
            // 
            // pictureBox3
            // 
            this.pictureBox3.Location = new System.Drawing.Point(287, 298);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(571, 212);
            this.pictureBox3.TabIndex = 31;
            this.pictureBox3.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Location = new System.Drawing.Point(10, 298);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(269, 212);
            this.pictureBox2.TabIndex = 30;
            this.pictureBox2.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(10, 78);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(848, 212);
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            // 
            // BeatTimer
            // 
            this.BeatTimer.Enabled = true;
            this.BeatTimer.Interval = 10;
            this.BeatTimer.Tick += new System.EventHandler(this.BeatTimer_Tick);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.BTN_single);
            this.groupBox3.Controls.Add(this.CMBX_recordtime_unit);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.TBOX_sample_time);
            this.groupBox3.Controls.Add(this.BTN_dsp);
            this.groupBox3.Controls.Add(this.BTN_Save);
            this.groupBox3.Controls.Add(this.BTN_Load);
            this.groupBox3.ForeColor = System.Drawing.Color.White;
            this.groupBox3.Location = new System.Drawing.Point(933, 196);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(267, 94);
            this.groupBox3.TabIndex = 39;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Debug";
            // 
            // LBL_FFT
            // 
            this.LBL_FFT.AutoSize = true;
            this.LBL_FFT.ForeColor = System.Drawing.Color.White;
            this.LBL_FFT.Location = new System.Drawing.Point(292, 300);
            this.LBL_FFT.Name = "LBL_FFT";
            this.LBL_FFT.Size = new System.Drawing.Size(26, 13);
            this.LBL_FFT.TabIndex = 40;
            this.LBL_FFT.Text = "FFT";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(914, 435);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(90, 26);
            this.button1.TabIndex = 41;
            this.button1.Text = "Music Play Test";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(1232, 572);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.LBL_FFT);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.pictureBox5);
            this.Controls.Add(this.LBL_title);
            this.Controls.Add(this.pictureBox4);
            this.Controls.Add(this.Label_time);
            this.Controls.Add(this.CMBX_interval_unit);
            this.Controls.Add(this.CMBX_fmpl_unit);
            this.Controls.Add(this.pictureBox3);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.TBOX_timerange);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.TBOX_fsmpl);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Group_Trigger);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.BTN_start);
            this.Controls.Add(this.BTN_stop);
            this.Controls.Add(this.textBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Form1";
            this.Text = "`";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.Group_Trigger.ResumeLayout(false);
            this.Group_Trigger.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BTN_single;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ComboBox CMBOX_inputrange_A;
        private System.Windows.Forms.Button BTN_stop;
        private System.Windows.Forms.Button BTN_start;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox TBOX_fsmpl;
        private System.Windows.Forms.TextBox TBOX_sample_time;
        private System.Windows.Forms.CheckBox CHKBOX_en_A;
        private System.Windows.Forms.CheckBox CHKBOX_en_B;
        private System.Windows.Forms.ComboBox CMBOX_inputrange_B;
        private System.Windows.Forms.ComboBox CMBOX_trg_ch;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TBOX_trg_level;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.ComboBox CMBOX_trig_direction;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox Group_Trigger;
        private System.Windows.Forms.ComboBox CMBOX_A_ACDC;
        private System.Windows.Forms.ComboBox CMBOX_B_ACDC;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox TBOX_timerange;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button BTN_Save;
        private System.Windows.Forms.Button BTN_Load;
        private System.Windows.Forms.Button BTN_dsp;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.ComboBox CMBX_fmpl_unit;
        private System.Windows.Forms.ComboBox CMBX_recordtime_unit;
        private System.Windows.Forms.ComboBox CMBX_interval_unit;
        private System.Windows.Forms.Label Label_time;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.Label LBL_title;
        private System.Windows.Forms.PictureBox pictureBox5;
        private System.Windows.Forms.Timer BeatTimer;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label LBL_FFT;
        private System.Windows.Forms.Button button1;
    }
}

