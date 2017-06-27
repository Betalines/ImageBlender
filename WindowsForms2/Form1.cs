using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsForms2
{
    enum HitTest
    {
        Caption = 2,
        Transparent = -1,
        Nowhere = 0,
        Client = 1,
        Left = 10,
        Right = 11,
        Top = 12,
        TopLeft = 13,
        TopRight = 14,
        Bottom = 15,
        BottomLeft = 16,
        BottomRight = 17,
        Border = 18
    }

    public partial class Form1 : Form
    {
        Library library;
        bool activeProgressBar1, activeProgressBar2;
        bool leftExists, rightExists;
        string filter = @"image files (*BMP; *.JPG; *.PNG) |  *.bmp; *.jpg; *.png | All files (*.*) | *.*";
        int imageNumber = 0;


        public Form1()
        {
            InitializeComponent();
            CreateLibrary();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            switch (m.Msg)
            {
                case 0x84: //WM_NCHITTEST
                    var result = (HitTest)m.Result.ToInt32();
                    if (result == HitTest.Top || result == HitTest.Bottom)
                        m.Result = new IntPtr((int)HitTest.Caption);
                    if (result == HitTest.TopLeft || result == HitTest.BottomLeft)
                        m.Result = new IntPtr((int)HitTest.Left);
                    if (result == HitTest.TopRight || result == HitTest.BottomRight)
                        m.Result = new IntPtr((int)HitTest.Right);

                    break;
            }
        }

        void CreateLibrary()
        {
            library = new Library();
            library.SetLibraryStyles();
            library.InitializeDatabase();
            library.LoadImagesFromDatabase();
            this.Controls.Add(library);
        }

        Bitmap GetScreenCapture()
        {
            Bitmap memoryImage;
            Graphics myGraphics = this.CreateGraphics();
            Size size = Screen.PrimaryScreen.Bounds.Size;
            memoryImage = new Bitmap(size.Width, size.Height, myGraphics);
            Graphics memoryGraphics = Graphics.FromImage(memoryImage);
            memoryGraphics.CopyFromScreen(0, 0, 0, 0, size);

            return memoryImage;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyData == Keys.F12)
            {
                if (!leftExists)
                {
                    leftPictureBox.Image = GetScreenCapture();
                    leftExists = true;
                }
                else
                {
                    rightPictureBox.Image = GetScreenCapture();
                    rightExists = true;
                }

                SwitchBlendingButtonForPicture();
            }
            if (e.KeyCode == Keys.Delete)
                library.DeletePicture();
        }

        void SwitchBlendingButtonForPicture()
        {
            if (leftExists && rightExists)
                blendingButton.Enabled = true;
            else
                blendingButton.Enabled = false;
        }

        void CommunicateInvalidFile(PictureBox pictureBox, ref bool exists)
        {
            MessageBox.Show("Invalid file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            pictureBox.Image = null;
            exists = false;
            SwitchBlendingButtonForPicture();
        }

        void LoadImage(PictureBox pictureBox, ref bool exists)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = filter;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string fileToOpen = dialog.FileName;

                try
                {
                    pictureBox.Image = new Bitmap(fileToOpen);
                }
                catch
                {
                    CommunicateInvalidFile(pictureBox, ref exists);
                    return;
                }

                exists = true;
                SwitchBlendingButtonForPicture();
            }
            else
            {
                pictureBox.Image = null;
                exists = false;
                SwitchBlendingButtonForPicture();
            }
        }


        public void LoadImageFromLibrary(PictureBox pictureBox, Image image, ref bool exists)
        {
            pictureBox.Image = image;
            exists = true;
            SwitchBlendingButtonForPicture();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            LibraryPictureBox libraryPictureBox;
            if (library.IsAnyClicked(out libraryPictureBox))
                LoadImageFromLibrary(leftPictureBox, libraryPictureBox.Image, ref leftExists);
            else
                LoadImage(leftPictureBox, ref leftExists);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            LibraryPictureBox libraryPictureBox;
            if (library.IsAnyClicked(out libraryPictureBox))
                LoadImageFromLibrary(rightPictureBox, libraryPictureBox.Image, ref rightExists);
            else
                LoadImage(rightPictureBox, ref rightExists);
        }

        void SwitchBlendingButtonForBlending()
        {
            if (activeProgressBar1 && activeProgressBar2)
                blendingButton.Enabled = false;
            else blendingButton.Enabled = true;
        }

        private void blendingButton_Click(object sender, EventArgs e)
        {
            Form2 newForm = new Form2(imageNumber++);
            newForm.progressChanged += NewForm_progressChanged;
            newForm.addedToLibrary += NewForm_addedToLibrary;
            double alpha = (double)trackBar1.Value / 10;

            if (!activeProgressBar1)
            {
                newForm.PrepareBlending(leftPictureBox.Image.Clone() as Bitmap, rightPictureBox.Image.Clone() as Bitmap, alpha, 1);
                activeProgressBar1 = true;
                progressBar1.Visible = true;
                operationProgressLabel.Visible = true;
                SwitchBlendingButtonForBlending();
            }
            else if (!activeProgressBar2)
            {
                newForm.PrepareBlending(leftPictureBox.Image.Clone() as Bitmap, rightPictureBox.Image.Clone() as Bitmap, alpha, 2);
                activeProgressBar2 = true;
                progressBar2.Visible = true;
                operationProgressLabel.Visible = true;
                SwitchBlendingButtonForBlending();
            }
        }

        private void NewForm_addedToLibrary(object sender, Image image, string filename)
        {
            library.AddToLibrary(image, filename);
        }

        void SwitchOperationProgressLabel()
        {
            if (!activeProgressBar1 && !activeProgressBar2)
                operationProgressLabel.Visible = false;
        }

        void UpdateProgressBar(Form2 form, ProgressBar progressBar, int progressProcentage, ref bool activeProgress)
        {
            if (progressProcentage == 100)
            {
                progressBar.Visible = false;
                activeProgress = false;
                SwitchOperationProgressLabel();
                SwitchBlendingButtonForBlending();
                form.Show();
            }
            else
                progressBar.Value = progressProcentage;
        }

        private void NewForm_progressChanged(object sender, int progressProcentage, int progressBarNumber)
        {
            Form2 form = sender as Form2;
            if (progressBarNumber == 1)
                UpdateProgressBar(form, progressBar1, progressProcentage, ref activeProgressBar1);
            else
                UpdateProgressBar(form, progressBar2, progressProcentage, ref activeProgressBar2);
        }
    }
}
