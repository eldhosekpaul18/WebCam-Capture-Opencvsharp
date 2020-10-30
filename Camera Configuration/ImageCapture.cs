using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;

namespace Camera_Configuration
{
    public partial class ImageCapture : Form
    {
        private Thread camera;
        bool isCameraRunning = false;

        public ImageCapture()
        {
            InitializeComponent();
            _ListCameras();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (btnStart.Text.Equals("Start Camera"))
            {
                isCameraRunning = true;
                CaptureCamera();
                btnStart.Text = "Stop Camera";
            }
            else
            {
                isCameraRunning = false;
                stopCamera();
                btnStart.Text = "Start Camera";
            }
        }

        // Declare required methods
        private void CaptureCamera()
        {
            int cameraindex = Convert.ToInt32(cmbCamera.SelectedValue);
            string result = Camera.Init(cameraindex);
            txtOutput.AppendText("Camera Started" + Environment.NewLine + result + Environment.NewLine);
            camera = new Thread(new ThreadStart(CaptureCameraCallback));
            camera.Start();
        }

        private void stopCamera()
        {
            Camera.ResetCamera();
        }

        private void CaptureCameraCallback()
        {
            while (isCameraRunning)
            {
                pictureBox1.Image = Camera.ScanImage();
            }
        }

        private void btnSnapShot_Click(object sender, EventArgs e)
        {
            if (isCameraRunning)
            {
                // Take snapshot of the current image generate by OpenCV in the Picture Box
                Bitmap snapshot = new Bitmap(pictureBox1.Image);
                FolderBrowserDialog folderDlg = new FolderBrowserDialog();
                folderDlg.ShowNewFolderButton = true;
                // Show the FolderBrowserDialog.  
                DialogResult result = folderDlg.ShowDialog();
                if (result == DialogResult.OK)
                {
                    snapshot.Save(string.Format(folderDlg.SelectedPath + @"\{0}.jpg", Guid.NewGuid()), ImageFormat.Jpeg);
                }
            }
            else
            {
                Console.WriteLine("Cannot take picture if the camera isn't capturing image!");
            }
        }

        private void _ListCameras()
        {
            cmbCamera.DataSource = Camera.DetectCamerasConnected();
            cmbCamera.DisplayMember = "Text";
            cmbCamera.ValueMember = "Value";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            isCameraRunning = false;
            stopCamera();
        }
    }


}
