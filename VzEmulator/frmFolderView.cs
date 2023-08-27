using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

namespace VzEmulator
{
    public partial class frmFolderView : Form
    {
        readonly string VZ_FILE_IMAGE = "VZ";
        readonly string DSK_FILE_IMAGE = "DSK";
        readonly string FOLDER_IMAGE = "FOLDER";

        public frmMain FrmMain { get; }
        string selectedPath;
        public string SelectedPath { get => selectedPath; set
            {
                selectedPath = value;
                this.Text = "Folder View - " + selectedPath;
            } }

        public Dictionary<string, (Bitmap,Byte[] )> imageCache = new Dictionary<string, (Bitmap,Byte[])>();
        MachineRunner previewRunner;
        MachineRunner previewRunner2;

        byte[] cleanStartSnapShot;

        MachinePresenter MainMachineInstance;

        public frmFolderView()
        {
            InitializeComponent();
        }

        public frmFolderView(frmMain frmMain, MachinePresenter mainMachineInstance, string selectedPath)
        {
            //Todo store state of each image in dict, run for a further second each time the image is clicked, or grab a few images and show an animation/series of images

            FrmMain = frmMain;
            SelectedPath = selectedPath;
            MainMachineInstance = mainMachineInstance;

            InitializeComponent();

            var files = PopulateListView(selectedPath);
            InitPreviewRunners();
            CacheFiles(files);

        }
       
        private String[] PopulateListView(string path)
        {
            listView1.Clear();
            listView1.Columns.Add("Filename", 200);
            listView1.Columns.Add("Size", 200);
            listView1.Columns.Add("Type", 200);
            listView1.Columns.Add("StartAddress", 200);

            //Get list of files in selectedPath
            var files = System.IO.Directory.GetFiles(path, "*.vz");
            var folders = System.IO.Directory.GetDirectories(path);

            //Add item to listbox1 for each file
            foreach (var file in files)
            {
                var fileDetails = GetFileDetails(file);

                //convert file to a listviewitem
                var fileName = Path.GetFileName(file);
                var item = new ListViewItem(fileDetails );
                item.Tag = file;
                item.ImageKey = VZ_FILE_IMAGE;

                listView1.Items.Add(item);

            }
            foreach (var folder in folders)
            {
                //convert file to a listviewitem
                var folderName = Path.GetFileName(folder) + Path.DirectorySeparatorChar;
                var item = new ListViewItem(new string[] { folderName ,"", "folder" });
                item.Tag = folder;
                item.ImageKey = FOLDER_IMAGE;

                listView1.Items.Add(item);
            }

            return files;
        }

        private string[] GetFileDetails(string file)
        {
            var fileName = Path.GetFileName(file);

            try
            {
                //Get file size for file
                var fileInfo = new FileInfo(file);
                var fileSize = fileInfo.Length;
                //Read file and confirm header info and file type
                var fileStream = new FileStream(file, FileMode.Open);
                var fileReader = new BinaryReader(fileStream);
                byte[] header = new byte[24];
                //Read 24 bytes into array
                fileReader.Read(header, 0, 24);
                var vzFile = new VzFile() { content = header };
                fileReader.Close();
                fileStream.Close();

                return new string[]
                {
                fileName,
                fileSize.ToString() + " bytes",
                $"{vzFile.fileType:X2}",
                $"{vzFile.startaddr_h:X2}{vzFile.startaddr_l:X2}"
                };
            } catch   
            { 
                return new string[]
                {
                    fileName
                };
            }

        }

        private bool abortCacheOperation = false;
        private void CacheFiles(string[] files)
        {
            var swAbort = new Stopwatch();
            swAbort.Start();
            while (abortCacheOperation && swAbort.ElapsedMilliseconds<1000)
            {
                Thread.Sleep(0);
            }

            //Run new thread which iterates over files and loads them into the emulator, populating imageCache in the background
            new Thread(() =>
            {
                foreach (var file in files)
                {
                    if (abortCacheOperation)
                    {
                        abortCacheOperation = false; 
                        return;
                    }

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
                    var sw = new Stopwatch();
                    sw.Start();
                    while (previewRunner2.LatestImage == null && sw.ElapsedMilliseconds<1000 && !abortCacheOperation)
                    {
                        Thread.Sleep(0);
                    }
                    if (previewRunner2.LatestImage == null) continue;
                    //todo caching only works for text mode, need to fix it for graphics mode
                    var image = (Bitmap)previewRunner2.LatestImage.Clone();

                    //todo get machine snapshot and cache it
                    previewRunner2.GetSnapshot();
                    sw.Restart();
                    while (previewRunner2.LatestSnapshot == null && sw.ElapsedMilliseconds < 1000 && !abortCacheOperation)
                    {
                        Thread.Sleep(0);
                    }
                    if (previewRunner2.LatestSnapshot == null) continue;
                    try
                    {
                        imageCache.Add(file, (image,previewRunner2.LatestSnapshot));
                    }
                    catch (ArgumentException ex)
                    {
                        //todo threadsafe dictionary
                    }

                }
            }).Start();
        }
        private void InitPreviewRunners()
        {
            string dosRomFileName = "Roms/VZDOS.ROM";
            string romFilename = "Roms/VZ300.ROM";

            if (previewRunner == null)
            {
                previewRunner = new MachineRunner(new Machine() { Tag= "PreviewRunner" });
                previewRunner2 = new MachineRunner(new Machine() { Tag = "PreviewRunnerCache" });

                //create runner for this and future previews
                previewRunner.Start(null);
                previewRunner2.Start(null);

                while (previewRunner.LatestImage == null || previewRunner2.LatestImage == null)
                {
                    Thread.Sleep(0);
                }
                
                previewRunner.GetSnapshot();
                while (previewRunner.LatestSnapshot == null)
                {
                    Thread.Sleep(0); //todo expire loop if too much time taken
                }
                cleanStartSnapShot = previewRunner.LatestSnapshot;

            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count < 1) return;

            var fileName = (string)listView1.SelectedItems[0].Tag;
            var shortName = listView1.SelectedItems[0].Text;
            if (shortName.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                abortCacheOperation=true;

                //Hide preview image
                panel1.BackgroundImage = null;
                panel1.Hide();

                //Load selected folder
                SelectedPath = fileName;
                var files = PopulateListView(fileName);
                CacheFiles(files);
                return;
            }
            if (fileName.ToUpper().EndsWith(".VZ"))
            {
                panel1.BackgroundImageLayout = ImageLayout.Stretch;
                panel1.Show();
                panel1.BackgroundImage = GetFilePreviewImage(fileName,false);
            }

        }

        private Bitmap GetFilePreviewImage(string fileName, bool refresh )
        {
            Bitmap image = null;

            if (refresh && imageCache.ContainsKey (fileName))
            {
                imageCache.Remove(fileName);
            }
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

                previewRunner.GetSnapshot();
                while (previewRunner.LatestSnapshot == null)
                {
                    Thread.Sleep(0);
                }

                try
                {
                    imageCache.Add(fileName, (image, previewRunner.LatestSnapshot)); //todo initiate and wait for snapshot
                } catch (ArgumentException ex)
                {
                    //todo threadsafe dictionary
                }
            }
            else
            {
                image = imageCache[fileName].Item1;
            }

            return image;
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            //Go up one directory level
            var upPath = System.IO.Path.Combine(SelectedPath,"..");
            SelectedPath = System.IO.Path.GetFullPath(upPath);
            PopulateListView(SelectedPath);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count < 1) return;

            var fileName = (string)listView1.SelectedItems[0].Tag;

            if (imageCache.ContainsKey(fileName))
            {

                MainMachineInstance.ApplySnapshot(imageCache[fileName].Item2);

            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count < 1) return;

            var fileName = (string)listView1.SelectedItems[0].Tag;
            MainMachineInstance.LoadFile(fileName);
        }

        Object lockObjpreviewRunner3 = new object();
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            //todo async processing, add job to queue, display updated screen if still selected.

            Monitor.Enter(lockObjpreviewRunner3);
            try
            {
                var previewRunner3 = new MachineRunner(new Machine() { Tag = "PreviewRunne3r" });
                previewRunner3.Start("");
                if (listView1.SelectedItems.Count < 1) return;
                var fileName = (string)listView1.SelectedItems[0].Tag;

                if (imageCache.ContainsKey(fileName))
                {
                    previewRunner3.ApplySnapshot(imageCache[fileName].Item2);
                    previewRunner3.run1s();
                    var sw = new Stopwatch();
                    sw.Start();
                    while (previewRunner3.LatestImage == null && sw.ElapsedMilliseconds<1000)
                    {
                        Thread.Sleep(0); //todo exit after  1s
                    }
                    if (previewRunner3.LatestImage == null)
                    {
                        //todo display error
                        return;
                    }
                    var image = (Bitmap)previewRunner3.LatestImage.Clone();
                    previewRunner3.GetSnapshot();
                    sw.Restart();
                    while (previewRunner3.LatestSnapshot == null && sw.ElapsedMilliseconds < 1000)
                    {
                        Thread.Sleep(0);
                    }
                    if (previewRunner3.LatestSnapshot == null)
                    {
                        //todo display error
                        return;
                    }
                    try
                    {
                        imageCache[fileName] = (image, previewRunner3.LatestSnapshot);
                    }
                    catch (ArgumentException ex)
                    {
                        //todo threadsafe dictionary
                    }

                    panel1.BackgroundImageLayout = ImageLayout.Stretch;
                    panel1.Show();
                    panel1.BackgroundImage = image;
                }
            } finally
            {
                Monitor.Exit(lockObjpreviewRunner3);
            }
            
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            if ((int)listView1.View==4)
            {
                  listView1.View = View.LargeIcon;
            } else
            {
                listView1.View = (View)((int)listView1.View + 1);
            }
            Text = listView1.View.ToString();
        }
    }
}
