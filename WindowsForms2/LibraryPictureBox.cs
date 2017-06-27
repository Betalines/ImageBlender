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
    public delegate void StateChangedHandler(Object sender);

    public class LibraryPictureBox:PictureBox
    {
        bool isClicked;
        public event StateChangedHandler stateChanged;

        public LibraryPictureBox() : base() { }

        public bool IsClicked
        {
            get { return isClicked; }
        }

        public string ImagePath { get; set; }

        public void SetLibraryPictureBoxStyle()
        {
            Size = new Size(160, 160);
            SizeMode = PictureBoxSizeMode.StretchImage;
            Padding = new Padding(5, 5, 5, 5);
            BackColor = Color.White;
            MouseClick += LibraryPictureBox_MouseClick;
            
        }

        public void ClickPictureBox()
        {
            isClicked = true;
            BackColor = Color.Orange;
        }

        public void UnclickPictureBox()
        {
            isClicked = false;
            BackColor = Color.White;
        }

        public void LibraryPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            stateChanged(this);

            if (isClicked)
                UnclickPictureBox();
            else
               ClickPictureBox();
        }

        public void AddImage(Image image, string filename)
        {
            Image = image;
            ImagePath = filename;
        }

        
    }
}
