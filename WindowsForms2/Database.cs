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
    class Database
    {
        const string databaseFileName = "imgGallery.xml";
        XmlDocument database;

        public Database()
        {
            database = new XmlDocument();
        }

        public void CreateDatabase()
        {
            if (!File.Exists(databaseFileName))
            {
                XmlNode rootNode = database.CreateElement("data");
                database.AppendChild(rootNode);
            }
            else
                database.Load(databaseFileName);
        }

        public void RemoveFromDatabase(string filename)
        {
            XmlNode imageToDelete = null;
            XmlNodeList images = database.GetElementsByTagName("Image");
            foreach (XmlNode image in images)
            {
                if (image.Attributes["path"].Value == filename)
                {
                    imageToDelete = image;
                    break;
                }
            }

            database.DocumentElement.RemoveChild(imageToDelete);
            SaveChangesInDatabase();
        }

        public void AddToDatabase(string filename)
        {
            XmlNode image = database.CreateElement("Image");
            XmlAttribute attribute = database.CreateAttribute("path");
            attribute.Value = filename;
            image.Attributes.Append(attribute);
            database.DocumentElement.AppendChild(image);
            SaveChangesInDatabase();
        }

        public void SaveChangesInDatabase()
        {
            database.Save(databaseFileName);
        }

        public List<string> LoadImagesFromDatabase()
        {
            List<string> imagesToRemove = new List<string>();
            List<string> imagesToLibrary = new List<string>();

            foreach (XmlNode image in database.DocumentElement.ChildNodes)
            {
                string filename = image.Attributes["path"].Value;
                if (File.Exists(filename))
                    imagesToLibrary.Add(filename);
                else
                    imagesToRemove.Add(filename);
            }

            RemoveInvalidFiles(imagesToRemove);
            return imagesToLibrary;
        }

        public void RemoveInvalidFiles(List<string> imagesToRemove)
        {
            if(imagesToRemove.Count > 0)
                foreach (var filename in imagesToRemove)
                    RemoveFromDatabase(filename);
        }

    }
}
