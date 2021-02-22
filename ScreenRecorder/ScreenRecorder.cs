using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;

using Accord.Video.FFMPEG;

namespace ScreenRecorder
{
    class ScreenRecorder
    {   //Video variables
        private Rectangle bounds;
        private string outputPath = ""; //Where the video will be stored
        private string tempPath = ""; //Temporary folder where the screenshots are stored before combining them to become a video
        private int fileCount = 1;
        private List<String> inputImageSequence = new List<string>(); //Store the names of all the screenshots

        //File variables
        private string audioName = "mic.wav"; //Store the audio
        private string videoName = "video.mp4"; //Store the video
        private string finalName = "FinalVideo.mp4"; //Allow the user to edit the name

        //TIme variable
        Stopwatch watch = new Stopwatch();

        //Audio variables
        public static class NativeMethods // Importing from a certain dll file and creating a method out of it
        {
            [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
            public static extern int mciSendString(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);

        }

        public ScreenRecorder(Rectangle b, string outPath)
        {
            CreateTempFolder("tempScreenshots");

            bounds = b;
            outputPath = outPath;
        }

        private void CreateTempFolder(string name)
        {
            if (Directory.Exists("D://"))//Checking if the user has a certain directory
            {
                string pathName = $"D://{name}"; //The dollar sign is to set a path directory name
                Directory.CreateDirectory(pathName);
                tempPath = pathName;
            }
            else
            {
                string pathName = $"C://{name}";//The dollar sign is to set a path directory name
                Directory.CreateDirectory(pathName);
                tempPath = pathName;
            }
        }

        private void DeletePath(string targetDir)
        {
            string[] files = Directory.GetFiles(targetDir); //Get all the files on the target directory
            string[] dirs = Directory.GetDirectories(targetDir); //Directory within the directory

            foreach(string file in files) //Every files that exist in the directory
            {
                File.SetAttributes(file, FileAttributes.Normal); //To make sure the file can be deleted (If it is read only)
                File.Delete(file); // Delete every single file in the directory
            }

            foreach (string dir in dirs) //Delete every directory
            {
                DeletePath(dir); //Delete every directory inside directory
            }

            Directory.Delete(targetDir, false); //Delete the target directory
        }

        private void DeleteFilesExcept(string targetFile, string excFile) //Delete all the files except the exception file
        {
            string[] files = Directory.GetFiles(targetFile);

            foreach(string file in files)
            {
                if(file!=excFile)
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
            }
        }

        public void cleanUp() //To remove the temp file in case of the program crashes
        {
            if(Directory.Exists(tempPath))
            {
                DeletePath(tempPath);
            }
        }

        public string GetElapsed() //To return the elapsed time
        {
            return string.Format("{0:D2}:{1:D2}:{2:D2}", watch.Elapsed.Hours, watch.Elapsed.Minutes, watch.Elapsed.Seconds); //Return the time in hours, minutes, seconds (Two decimal places)
        }

        public void RecordVideo()
        {
            watch.Start();

            using(Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height)) //Creating a bitmap equals to bounds and widths of the string
            {
                using (Graphics g = Graphics.FromImage(bitmap)) //Create a graphics variable to copy something from the bitmap
                {
                    g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size); //Creating a screenshot of the screen during this current iteration
                }
                string name = tempPath + "//screenshot-" + fileCount + ".png"; //Screenshot name
                bitmap.Save(name, ImageFormat.Png); //Save each file in png
                inputImageSequence.Add(name); //Add the directory to the image input sequence so we can keep track every screenshots that we saved
                fileCount++;

                bitmap.Dispose(); //Windows won't let you delete the file if it is still being used so we need to dispose it
            }
        }

        public void RecordAudio()
        {
            NativeMethods.mciSendString("open new Type waveaudio Alias recsound", "", 0, 0);
            NativeMethods.mciSendString("record recsound", "", 0, 0); // Default way for C# to record while using the native method
            Debug.WriteLine("Audio recording!");
        }

        private void SaveVideo(int width, int height, int frameRate) //Save the video to final location
        {
            using(VideoFileWriter vFwriter = new VideoFileWriter())
            {
                vFwriter.Open(outputPath + "//" + videoName, width, height, frameRate, VideoCodec.MPEG4); //Open the vFwriter
                foreach(string imageLoc in inputImageSequence) // Add each individual screenshot to the video file writer
                {
                    Bitmap imageFrame = System.Drawing.Image.FromFile(imageLoc) as Bitmap; 
                    vFwriter.WriteVideoFrame(imageFrame);
                    imageFrame.Dispose();
                }

                vFwriter.Close();
            }
        }

        private void SaveAudio()
        {
            string audioPath = "save recsound " + outputPath + "//" + audioName;
            NativeMethods.mciSendString(audioPath, "", 0, 0);
            NativeMethods.mciSendString("close recsound", "", 0, 0);
            Debug.WriteLine("Audio saving to " + audioPath);
        }

        private void CombineVideoAndAudio(string video, string audio)
        {
            string command = $"/c ffmpeg -i \"{video}\" -i \"{audio}\" -shortest {finalName}"; //Combine audio and video file, then saving it as the final name 
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                CreateNoWindow = false, // Create a window
                FileName = "cmd.exe", // Command prompt
                WorkingDirectory = outputPath, // Make sure that CMD is working in the outputpath (same directory)
                Arguments = command // Pass the command argument to CMD
            };

            using(Process exeProcess = Process.Start(startInfo)) // Execute the command
            {
                exeProcess.WaitForExit(); // Exits when terminated
            }
        }

        public void Stop()
        {
            watch.Stop();
            int width = bounds.Width;
            int height = bounds.Height;
            int frameRate = 10;

            SaveAudio();
            SaveVideo(width, height, frameRate);

            CombineVideoAndAudio(videoName, audioName);

            DeletePath(tempPath);
            //Remove this function if the audio is not recording
            //DeleteFilesExcept(outputPath, outputPath + "\\" + finalName); //Delete all files except the combined audio and video file

        }
    }
}
