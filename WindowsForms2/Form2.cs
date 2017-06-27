using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsForms2
{
    public delegate void ProgressChangedHandler(Object sender, int progressProcentage, int progressBarNumber);
    public delegate void AddingToLibraryHandler(Object sender, Image imege, string filename);
    public partial class Form2 : Form
    {
        Bitmap bmpLeft;
        Bitmap bmpRight;
        int progressBarNumber;
        int imageNumber;
        public event ProgressChangedHandler progressChanged;
        public event AddingToLibraryHandler addedToLibrary;

        public Form2(int imageNumber)
        {
            this.imageNumber = imageNumber;
            InitializeComponent();
        }

        public void SetImage(Bitmap bmp)
        {
            pictureBox.Image = bmp;
        }

        public void PrepareBlending(Bitmap bmpLeft, Bitmap bmpRight, double alpha, int progressBarNumber)
        {
            this.bmpLeft = bmpLeft;
            this.bmpRight = bmpRight;
            this.progressBarNumber = progressBarNumber;
            backgroundWorker1.RunWorkerAsync(alpha);
        }

        private void pictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(pictureBox, new Point(e.X, e.Y));
            }
        }

        private string SaveWithSaveDialog()
        {
            saveDialog.Filter = @"image files (*BMP; *.JPG; *.PNG) |  *.bmp; *.jpg; *.png | All files (*.*) | *.*";
            saveDialog.FileName = "newImage" + imageNumber.ToString();
            ImageFormat format = ImageFormat.Png;

            if (saveDialog.ShowDialog() == DialogResult.OK && saveDialog.FileName != "")
            {
                string ext = System.IO.Path.GetExtension(saveDialog.FileName);
                switch (ext)
                {
                    case ".jpg":
                        format = ImageFormat.Jpeg;
                        break;
                    case ".bmp":
                        format = ImageFormat.Bmp;
                        break;
                }
                pictureBox.Image.Save(saveDialog.FileName, format);
                return saveDialog.FileName;
            }

            return null;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveWithSaveDialog();
        }
        
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            double? alfa = e.Argument as double?;
           
            int width = Math.Min(bmpLeft.Width, bmpRight.Width);
            int height = Math.Min(bmpLeft.Height, bmpRight.Height);
            Bitmap bmpBlended = new Bitmap(width, height);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color oldLeft = bmpLeft.GetPixel(i, j);
                    Color oldRight = bmpRight.GetPixel(i, j);

                    int colorA = (int)(oldLeft.A * alfa) + (int)(oldRight.A * (1 - alfa));
                    int colorR = (int)(oldLeft.R * alfa) + (int)(oldRight.R * (1 - alfa));
                    int colorG = (int)(oldLeft.G * alfa) + (int)(oldRight.G * (1 - alfa));
                    int colorB = (int)(oldLeft.B * alfa) + (int)(oldRight.B * (1 - alfa));
                    bmpBlended.SetPixel(i, j, Color.FromArgb(colorA, colorR, colorG, colorB));
                }
                int progress = (int)((double)(i + 1) * 100.0 / (double)width);
                backgroundWorker1.ReportProgress(progress);
                //Thread.Sleep(20);
            }

            this.pictureBox.Image = bmpBlended;
            this.Size = new Size(width, height);
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressChanged(this, e.ProgressPercentage, this.progressBarNumber);
        }

        private string SaveOnDisk(Image image)
        {
            string filename = "newImage" + imageNumber.ToString() + ".bmp";
            try
            {
                image.Save(filename, ImageFormat.Bmp);
            }
            catch
            {
                MessageBox.Show("Please change filename and save file on disk");
                filename = SaveWithSaveDialog();
            }
            return filename;
        }

        private void addToLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filename = SaveOnDisk(pictureBox.Image);
            addedToLibrary(this, pictureBox.Image, filename);
        }
    }
}
