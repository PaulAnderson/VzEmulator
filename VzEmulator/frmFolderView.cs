using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VzEmulator
{
    public partial class frmFolderView : Form
    {
        public frmMain FrmMain { get; }
        public string SelectedPath { get; }

        public Dictionary<string, Bitmap> imageCache = new Dictionary<string, Bitmap>();
        MachineRunner previewRunner;
        MachineRunner previewRunner2;

        byte[] cleanStartSnapShot;

        public frmFolderView()
        {
            InitializeComponent();
        }

        public frmFolderView(frmMain frmMain, string selectedPath)
        {
            //Todo store state of each image in dict, run for a further second each time the image is clicked, or grab a few images and show an animation/series of images

            FrmMain = frmMain;
            SelectedPath = selectedPath;

            InitializeComponent();

            //Get list of files in selectedPath
            var files = System.IO.Directory.GetFiles(selectedPath, "*.vz");
            //Add item to listbox1 for each file
            foreach (var file in files)
            {
                //convert file to a listviewitem
                var fileName = Path.GetFileName(file);
                var item = new ListViewItem(fileName);
                item.Tag = file;
                listView1.Items.Add(item);
            }


            string dosRomFileName = "Roms/VZDOS.ROM";
            string romFilename = "Roms/VZ300.ROM";

            //todo load program, run for a set number of cycles, get image

            if (previewRunner == null)
            {
                previewRunner = new MachineRunner(new Machine());
                previewRunner2 = new MachineRunner(new Machine());


                //create runner for this and future previews
                previewRunner.Start(null);
                previewRunner2.Start(null);

                while (previewRunner.LatestImage == null || previewRunner2.LatestImage == null)
                {
                    Thread.Sleep(0);
                }
                previewRunner.Run(VzConstants.DefaultBasicStart, true);
                previewRunner.GetSnapshot();
                while (previewRunner.LatestSnapshot == null)
                {
                    Thread.Sleep(0);
                }
                cleanStartSnapShot = previewRunner.LatestSnapshot;

                previewRunner2.ApplySnapshot(cleanStartSnapShot);
                previewRunner2.Pause();

            }


            //Run new thread which iterates over files and loads them into the emulator, populating imageCache in the background
            //todo this is a bit of a hack, should really be using a background worker
            new Thread(() =>
            {
                foreach (var file in files)
                {
                    if (!imageCache.ContainsKey(file))

                        previewRunner2.LatestImage = null;
                    previewRunner2.ApplySnapshot(cleanStartSnapShot);
                    try
                    {
                        previewRunner2.LoadAndRunFile(file);
                    }
                    catch (Exception ex)
                    {
                        //todo display error
                        continue;
                    }

                    while (previewRunner2.LatestImage == null)
                    {
                        Thread.Sleep(0);
                    }

                    //todo caching only works for text mode, need to fix it for graphics mode
                    var image = (Bitmap)previewRunner2.LatestImage.Clone();

                    try
                    {
                        imageCache.Add(file, image);
                    }
                    catch (ArgumentException ex)
                    {
                        //todo threadsafe dictionary
                    }

                }
            }).Start();

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count < 1) return;

            var fileName = (string)listView1.SelectedItems[0].Tag;



            panel1.BackgroundImageLayout = ImageLayout.Stretch;
            panel1.Size = new Size(256, 192);
            panel1.BackgroundImage = GetFilePreviewImage(fileName);
            panel1.Show();
        }

        private Bitmap GetFilePreviewImage(string fileName)
        {
            Bitmap image = null;

            if (!imageCache.ContainsKey(fileName))
            {
                //todo load program, run for a set number of cycles, get image
                previewRunner.LatestImage = null;
                previewRunner.ApplySnapshot(cleanStartSnapShot);
                try
                {
                    previewRunner.LoadAndRunFile(fileName);
                }
                catch (Exception ex)
                {
                    //todo display error
                    return image;
                }

                while (previewRunner.LatestImage == null)
                {
                    System.Threading.Thread.Sleep(0);
                }

                //todo caching only works for text mode, need to fix it for graphics mode
                image = (Bitmap)previewRunner.LatestImage.Clone();

                try
                {
                    imageCache.Add(fileName, image);
                } catch (ArgumentException ex)
                {
                    //todo threadsafe dictionary
                }
            }
            else
            {
                image = imageCache[fileName];
            }

            return image;
        }
    }
}
