using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsForms2
{
    public class Library : FlowLayoutPanel
    {

        List<string> imagePaths = new List<string>();
        Database database;
        Array data;

        public Library() : base() { }

        public void SetLibraryStyles()
        {
            BackColor = SystemColors.ControlLight;
            AllowDrop = true;
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            AutoScroll = true;
            BorderStyle = BorderStyle.Fixed3D;
            Location = new Point(12, 12);
            Size = new Size(295, 353);
            TabIndex = 5;
            Name = "library";

            DragEnter += Library_DragEnter;
            DragDrop += Library_DragDrop;
        }

        public void InitializeDatabase()
        {
            database = new Database();
            database.CreateDatabase();
        }

        private void Library_DragDrop(object sender, DragEventArgs e)
        {
            if (data == null) return;

            for (int i = 0; i < data.Length; i++)
            {
                string filename;
                if (GetFilename(out filename, i))
                    AddToLibrary(new Bitmap(filename), filename);
            }

        }

        private bool GetFilename(out string filename, int index)
        {
            filename = null;
            if (data.GetValue(index) is String)
            {
                filename = ((string[])data)[index];
                string ext = Path.GetExtension(filename).ToLower();
                if ((ext == ".jpg") || (ext == ".png") || (ext == ".bmp"))
                    return true;
            }
            return false;
        }

        private void Library_DragEnter(object sender, DragEventArgs e)
        {
            if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                data = ((IDataObject)e.Data).GetData("FileDrop") as Array;
                e.Effect = DragDropEffects.Copy;
            }
            else
                e.Effect = DragDropEffects.None;
        }

        public bool IsAnyClicked(out LibraryPictureBox outPictureBox)
        {
            foreach (LibraryPictureBox pictureBox in this.Controls)
            {
                if (pictureBox.IsClicked)
                {
                    outPictureBox = pictureBox;
                    return true;
                }
            }
            outPictureBox = null;
            return false;
        }

        public void DeletePicture()
        {
            LibraryPictureBox pictureBoxToDelete;
            if (IsAnyClicked(out pictureBoxToDelete))
            {
                this.Controls.Remove(pictureBoxToDelete);
                imagePaths.Remove(pictureBoxToDelete.ImagePath);
                database.RemoveFromDatabase(pictureBoxToDelete.ImagePath);
                database.SaveChangesInDatabase();
            }
        }

        private LibraryPictureBox CreatePictureBox()
        {
            LibraryPictureBox pictureBox = new LibraryPictureBox();
            pictureBox.SetLibraryPictureBoxStyle();
            pictureBox.stateChanged += PictureBox_stateChanged;

            return pictureBox;
        }

        
        public void AddToLibrary(Image image, string filename, bool fromDatabase = false)
        {
            if (imagePaths.Contains(filename)) return;

            LibraryPictureBox pictureBox = CreatePictureBox();
            pictureBox.AddImage(image, filename);

            imagePaths.Add(filename);
            this.Controls.Add(pictureBox);

            if(!fromDatabase)
               database.AddToDatabase(filename);
        }

        private void PictureBox_stateChanged(object sender)
        {
            foreach (LibraryPictureBox pictureBox in this.Controls)
            {
                if (pictureBox.IsClicked && pictureBox != sender as LibraryPictureBox)
                {
                    pictureBox.UnclickPictureBox();
                }
            }
        }

      
        public void LoadImagesFromDatabase()
        {
            List<string> imagesToLibrary = database.LoadImagesFromDatabase();

            foreach (var filename in imagesToLibrary)
                AddToLibrary(new Bitmap(filename), filename, true);
        }

    }
}
