using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenRecorder
{
    public partial class Form1 : Form
    {
        bool folderSelected = false;
        string outputPath = "";
        //string finalVideName = "FinalVideo.mp4";

        ScreenRecorder screenRec = new ScreenRecorder(new Rectangle(), ""); // Default

        public Form1()
        {
            InitializeComponent();
        }

        private void selectFolderBtn_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.Description = "Select an Output Folder";

            if(folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK) // If they select something
            {
                outputPath = folderBrowser.SelectedPath; // Path selected
                folderSelected = true;

                Rectangle bounds = Screen.FromControl(this).Bounds;
                screenRec = new ScreenRecorder(bounds, outputPath);
            }
            else
            {
                MessageBox.Show("Please select a folder!");
            }
        }

        private void tmrRecord_Tick(object sender, EventArgs e)
        {
            screenRec.RecordAudio();
            screenRec.RecordVideo();

            lblTime.Text = screenRec.GetElapsed();
        }

        private void recordBtn_Click(object sender, EventArgs e)
        {
            if(folderSelected)
            {
                tmrRecord.Start();
            }
            else
            {
                MessageBox.Show("You must select an output folder before recording!");
            }
        }

        private void stopBtn_Click(object sender, EventArgs e)
        {
            tmrRecord.Stop();
            screenRec.Stop();
            Application.Restart();
        }
    }
}
