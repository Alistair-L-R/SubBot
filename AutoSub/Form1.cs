using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using AForge.Imaging;
using AForge.Vision;
using AForge.Imaging.Formats;
using System.Windows.Forms;

namespace AutoSub
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Activate the auto subscribe program?", "Activate?", MessageBoxButtons.YesNo);
            if (!(result == DialogResult.Yes))
            {
                Application.Exit();
                return;
            }
            System.Diagnostics.Process.Start("https://www.youtube.com/channel/UC-4R7Zp4G5MDaUl0qRQ8waw");
            TakeScreenshots();
        }

        Bitmap CaptureScreen()
        {
            var image = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            MessageBox.Show("AutoSub: Analysing screenshot, this may take about a minute.");
            var gfx = Graphics.FromImage(image);
            gfx.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
            return image;
        }

        public static Bitmap ConvertTo24bpp(System.Drawing.Image img)
        {
            var bmp = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var gr = Graphics.FromImage(bmp))
                gr.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
            return bmp;
        }

        private void MoveCursor(Point ad)
        {
            //Moves the cursor to the ad
            this.Cursor = new Cursor(Cursor.Current.Handle);
            Cursor.Position = new Point(ad.X + 10, ad.Y + 10);
            LeftMouseClick(ad.X + 10, ad.Y + 10);
            Application.Exit();
        }


        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        //This simulates a left mouse click
        public static void LeftMouseClick(int xpos, int ypos)
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
        }

        private void AiDetection(Bitmap screenshot, Bitmap subimage, Bitmap subimage2)
        {
            try
            {
                // create template matching algorithm's instance
                // (set similarity threshold to 80%)
                AForge.Imaging.ExhaustiveTemplateMatching tm = new AForge.Imaging.ExhaustiveTemplateMatching(0.85f);
                // find all matchings with specified above similarity

                AForge.Imaging.TemplateMatch[] matchings = tm.ProcessImage(screenshot, subimage);
                AForge.Imaging.TemplateMatch[] matchings2 = tm.ProcessImage(screenshot, subimage2);
                if (matchings.Length > 0)
                {
                    MoveCursor(matchings[0].Rectangle.Location);
                }
                else if (matchings2.Length > 0)
                {
                    MoveCursor(matchings2[0].Rectangle.Location);
                }
                else
                {
                    DialogResult result = MessageBox.Show("Auto Sub: No matchings, taking next screenshot. " + 
                        Environment.NewLine + "If it fails three times, I would quit the program.","No Matchings",MessageBoxButtons.RetryCancel);
                    if (result == DialogResult.Cancel)
                    {
                        Application.Exit();
                        return;
                    }
                    TakeScreenshots();
                }
            }
            catch
            {
                MessageBox.Show("Error lol");
            }
        }

        private void TakeScreenshots()
        {
            Bitmap sourceImage = ConvertTo24bpp(CaptureScreen());
            Assembly myAssembly = Assembly.GetExecutingAssembly();
            Stream myStream = myAssembly.GetManifestResourceStream("AutoSub.sub.jpg");
            Bitmap template = new Bitmap(myStream);
            myStream = myAssembly.GetManifestResourceStream("AutoSub.sub2.jpg");
            Bitmap template2 = new Bitmap(myStream);
            AiDetection(sourceImage, template, template2);
        }
    }
}
