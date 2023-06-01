using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Emotion_beat
{
    public partial class Form2 : Form
    {
        System.IO.Stream str = Properties.Resources.Dascon;
        System.Media.SoundPlayer snd;

        /*
         Enable Dragging windows!!!!!!
        */
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        private void Form2_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }



        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            snd = new System.Media.SoundPlayer(str);
            snd.Play();

            textBox1.AppendText("Description\r\n");
            textBox1.AppendText("----------------------------------------------------------------------------\r\n");
        }

        private void MyClose_Click(object sender, EventArgs e)
        {
            this.Close();
            snd.Stop();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
