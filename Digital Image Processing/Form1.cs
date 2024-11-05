using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebCamLib;

namespace Digital_Image_Processing
{
    public partial class Form1 : Form
    {
        Bitmap loaded, processed, subtracted;

        System.Windows.Forms.Timer currentTimer;
        Device[] devices;

        public Form1()
        {
            InitializeComponent();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            loaded = new Bitmap(openFileDialog1.FileName);
            pictureBox1.Image = loaded;
        }

        private void pixelCopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded == null) return;

            processed = new Bitmap(loaded.Width, loaded.Height);

            Color pixel;
            for (int x = 0; x < loaded.Width; x++)
            {
                for (int y = 0; y < loaded.Height; y++)
                {
                    pixel = loaded.GetPixel(x, y);
                    processed.SetPixel(x, y, pixel);
                }
            }
            pictureBox2.Image = processed;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            processed.Save(saveFileDialog1.FileName);
        }

        private void greyscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded == null) return;
            processed = new Bitmap(loaded.Width, loaded.Height);

            Color pixel, gray;
            int average;
            for (int x = 0; x < loaded.Width; x++)
            {
                for (int y = 0; y < loaded.Height; y++)
                {
                    pixel = loaded.GetPixel(x, y);
                    average = (int)(pixel.R + pixel.G + pixel.B) / 3;
                    gray = Color.FromArgb(average, average, average);
                    processed.SetPixel(x, y, gray);
                }
            }
            pictureBox2.Image = processed;
        }

        private void colorInversionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded == null) return;
            processed = new Bitmap(loaded.Width, loaded.Height);

            Color pixel, inverted;
            for (int x = 0; x < loaded.Width; x++)
            {
                for (int y = 0; y < loaded.Height; y++)
                {
                    pixel = loaded.GetPixel(x, y);
                    inverted = Color.FromArgb(255 - pixel.R, 255 - pixel.G, 255 - pixel.B);
                    processed.SetPixel(x, y, inverted);
                }
            }
            pictureBox2.Image = processed;
        }

        private void histogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded == null) return;

            int[] histData = new int[256];
            Color pixel;
            int maxFreq = 420;

            for (int x = 0; x < loaded.Width; x++)
                for (int y = 0; y < loaded.Height; y++)
                {
                    pixel = loaded.GetPixel(x, y);
                    int ave = (pixel.R + pixel.G + pixel.B) / 3;
                    histData[ave]++;

                    if (histData[ave] > maxFreq)
                        maxFreq = histData[ave];
                }

            processed = new Bitmap(256, 420);
            int mFactor = maxFreq / 420;
            int count;

            for (int i = 0; i < 256; i++)
            {
                count = Math.Min(420, histData[i] / mFactor);

                for (int j = 0; j < count; j++)
                    processed.SetPixel(i, 419 - j, Color.Black);
            }

            pictureBox2.Image = processed;
        }

        private void sepiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded == null)
                return;

            processed = new Bitmap(loaded.Width, loaded.Height);

            Color pixel;
            for (int x = 0; x < loaded.Width; x++)
                for (int y = 0; y < loaded.Height; y++)
                {
                    pixel = loaded.GetPixel(x, y);
                    processed.SetPixel(x, y,
                        Color.FromArgb(
                            (int)Math.Min(pixel.R * 0.393 + pixel.G * 0.769 + pixel.B * 0.189, 255),
                            (int)Math.Min(pixel.R * 0.349 + pixel.G * 0.686 + pixel.B * 0.168, 255),
                            (int)Math.Min(pixel.R * 0.272 + pixel.G * 0.534 + pixel.B * 0.131, 255)
                            )
                        );
                }

            pictureBox2.Image = processed;
        }

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            processed = new Bitmap(openFileDialog2.FileName);
            pictureBox2.Image = processed;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (loaded == null || processed == null) return;

            int greyGreen = 255 / 3;
            int threshold = 5;

            Color pixel;
            subtracted = new Bitmap(loaded.Width, loaded.Height);

            for (int x = 0; x < loaded.Width; x++)
            {
                if (x >= processed.Width) break;

                for (int y = 0; y < loaded.Height; y++)
                {
                    if (y >= processed.Height) break;

                    pixel = loaded.GetPixel(x, y);

                    subtracted.SetPixel(x, y,
                        Math.Abs((pixel.R + pixel.G + pixel.B) / 3 - greyGreen) < threshold ? processed.GetPixel(x, y) : pixel);
                }
            }
            pictureBox3.Image = subtracted;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (processed == null) return;

            Image map = getData();

            if (map != null)
            {
                loaded = new Bitmap(map);
                subtracted = new Bitmap(loaded.Width, loaded.Height);

                BitmapData bmLoaded = loaded.LockBits(
                    new Rectangle(0, 0, loaded.Width, loaded.Height),
                    ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb
                    );

                BitmapData bmProcessed = processed.LockBits(
                    new Rectangle(0, 0, processed.Width, processed.Height),
                    ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb
                    );

                BitmapData bmSubtracted = subtracted.LockBits(
                    new Rectangle(0, 0, subtracted.Width, subtracted.Height),
                    ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb
                    );

                int limitAve = 255 / 3;
                int threshold = 5;

                unsafe
                {
                    int paddingLoaded = bmLoaded.Stride - loaded.Width * 3;
                    int paddingProcessed = bmProcessed.Stride - processed.Width * 3;
                    int paddingSubtracted = bmSubtracted.Stride - subtracted.Width * 3;

                    byte* pLoaded = (byte*)bmLoaded.Scan0;
                    byte* pProcessed = (byte*)bmProcessed.Scan0;
                    byte* pSubtracted = (byte*)bmSubtracted.Scan0;

                    byte* start_p_processed = (byte*)bmProcessed.Scan0;

                    for (int i = 0;
                        i < loaded.Height;
                        i++, pLoaded += paddingLoaded, pSubtracted += paddingSubtracted)
                    {
                        for (int j = 0;
                            j < loaded.Width;
                            j++, pLoaded += 3, pSubtracted += 3)
                        {
                            if (Math.Abs(pLoaded[0] + pLoaded[1] + pLoaded[2] - limitAve) < threshold)
                            {
                                pSubtracted[0] = pProcessed[0];
                                pSubtracted[1] = pProcessed[1];
                                pSubtracted[2] = pProcessed[2];
                            }
                            else
                            {
                                pSubtracted[0] = pLoaded[0];
                                pSubtracted[1] = pLoaded[1];
                                pSubtracted[2] = pLoaded[2];
                            }

                            if (j < processed.Width)
                                pProcessed += 3;
                        }

                        if (i < processed.Height)
                            pProcessed = start_p_processed + i * 3;
                    }
                }

                loaded.UnlockBits(bmLoaded);
                processed.UnlockBits(bmProcessed);
                subtracted.UnlockBits(bmSubtracted);

                pictureBox3.Image = subtracted;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            devices = DeviceManager.GetAllDevices();
        }

        private void turnOnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            devices[0].ShowWindow(pictureBox1);
        }

        private void turnOffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            devices[0].Stop();
        }

        private void subtractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentTimer != null) currentTimer.Enabled = false;

            currentTimer = timer1;
            currentTimer.Enabled = true;
        }

        private Image getData()
        {
            IDataObject data;
            devices[0].Sendmessage();
            data = Clipboard.GetDataObject();
            return (Image)data.GetData("System.Drawing.Bitmap", true);
        }
    }
}
