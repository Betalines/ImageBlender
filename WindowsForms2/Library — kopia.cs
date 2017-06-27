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
        const string databaseFileName = "imgGallery.xml";
        XmlDocument database;
        //XmlNode rootNode;
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
            database = new XmlDocument();
            if (File.Exists(databaseFileName))
                database.Load(databaseFileName);
            else
            {
                XmlNode rootNode = database.CreateElement("data");
                database.AppendChild(rootNode);
            }
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
                RemoveFromDatabase(pictureBoxToDelete.ImagePath);
                SaveChangesInDatabase();
            }
        }

        private LibraryPictureBox CreatePictureBox()
        {
            LibraryPictureBox pictureBox = new LibraryPictureBox();
            pictureBox.SetLibraryPictureBoxStyle();
            pictureBox.stateChanged += PictureBox_stateChanged;

            return pictureBox;
        }

        private void RemoveFromDatabase(string filename)
        {
            XmlNode imageToDelete = null;
            XmlNodeList images = database.GetElementsByTagName("Image");
            foreach(XmlNode image in images)
            {
                if(image.Attributes["path"].Value == filename)
                {
                    imageToDelete = image;
                    break;
                }
            }

            database.DocumentElement.RemoveChild(imageToDelete);
            SaveChangesInDatabase();
        }

        private void AddToDatabase(string filename)
        {
            XmlNode image = database.CreateElement("Image");
            XmlAttribute attribute = database.CreateAttribute("path");
            attribute.Value = filename;
            image.Attributes.Append(attribute);
            database.DocumentElement.AppendChild(image);
            SaveChangesInDatabase();
        }

        public void AddToLibrary(Image image, string filename, bool fromDatabase = false)
        {
            if (imagePaths.Contains(filename)) return;

            LibraryPictureBox pictureBox = CreatePictureBox();
            pictureBox.AddImage(image, filename);

            imagePaths.Add(filename);
            this.Controls.Add(pictureBox);

            if(!fromDatabase)
               AddToDatabase(filename);
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

        private void SaveChangesInDatabase()
        {
            database.Save(databaseFileName);
        }

        public void LoadImagesFromDatabase()
        {
            List<string> imagesToRemove = new List<string>();
            foreach (XmlNode image in database.DocumentElement.ChildNodes)
            {
                string filename = image.Attributes["path"].Value;
                if (File.Exists(filename))
                    AddToLibrary(new Bitmap(filename), filename, true);
                else
                    imagesToRemove.Add(filename);
            }

            foreach (var filename in imagesToRemove)
                RemoveFromDatabase(filename);
        }

    }
}
