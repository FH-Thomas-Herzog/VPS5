using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Diffusions
{
    public partial class MainForm : Form
    {
        private Area currentArea;
        private ImageGenerator generator;
        private bool running;
        private const int tipSize = 50;
        private const double defaultHeat = 400.0;

        private bool parallel = false;
        public const int MAX_PARALLEL_OUTER = 10;
        public const int MAX_PARALLEL_INNER = 2;
        private readonly object mutex = new object();

        public MainForm()
        {
            InitializeComponent();

            pictureBox.Image = new Bitmap(pictureBox.Width, pictureBox.Height);
            Graphics graphics = Graphics.FromImage(pictureBox.Image);
            graphics.FillRectangle(Brushes.Azure, 0, 0, pictureBox.Width, pictureBox.Height);
            graphics.Dispose();

            generator = new SyncImageGenerator(); parallel = false;
            //generator = new ParallelImageGenerator();
            //parallel = true;
            generator.ImageGenerated += generator_ImageGenerated;
        }

        private void InitArea()
        {
            currentArea = new Area(pictureBox.Width, pictureBox.Height);

            for (int i = 0; i < pictureBox.Width - 1; i++)
            {
                for (int j = 0; j < pictureBox.Height - 1; j++)
                {
                    currentArea.Matrix[i, j] = 0;
                }
            }
            Reheat(currentArea.Matrix, 5, 5, pictureBox.Width, pictureBox.Height, 100, 150);
            Reheat(currentArea.Matrix, 100, 100, pictureBox.Width, pictureBox.Height, 80, defaultHeat);
        }

        private void Reheat(double[,] matrix, int x, int y, int width, int height, int size, double val)
        {
            // Here we need to lock the use of the matrix
            // for parallel
            if (parallel)
            {
                ReheatParallel(matrix, x, y, width, height, size, val);
            }
            // For synchronous
            else
            {
                ReheatSynch(matrix, x, y, width, height, size, val);
            }
        }

        private void ReheatSynch(double[,] matrix, int x, int y, int width, int height, int size, double val)
        {
            // Here we nned to lock the usage of the current area
            lock (mutex)
            {
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        matrix[(x + i) % width, (y + j) % height] = val;
                    }
                }
            }
        }

        private void ReheatParallel(double[,] matrix, int x, int y, int width, int height, int size, double val)
        {
            // Here we nned to lock the usage of the current area
            lock (mutex)
            {
                Parallel.For(0, size, (i) =>
                {
                    Parallel.For(0, size, (j) =>
                    {
                        matrix[(x + i) % width, (y + j) % height] = val;
                    });
                });
            }
        }

        private void UpdateImage(Area area)
        {
            toolStripStatusLabel.Text = "Calculating ...";
            generator.GenerateImage(area);
        }

        private void generator_ImageGenerated(object sender, EventArgs<Tuple<Area, Bitmap, TimeSpan>> e)
        {
            if (InvokeRequired)
                Invoke(new EventHandler<EventArgs<Tuple<Area, Bitmap, TimeSpan>>>(generator_ImageGenerated), sender, e);
            else
            {
                currentArea = e.Value.Item1;
                if (e.Value.Item2 != null)
                    pictureBox.Image = e.Value.Item2;
                if (generator.StopRequested)
                {
                    running = false;
                    startButton.Text = "Start";
                    toolStripStatusLabel.Text = "Done (Runtime: " + e.Value.Item3 + ")";
                }
                else
                {
                    startButton.Text = "Stop";
                    toolStripStatusLabel.Text = "Calculating. Runtime: " + e.Value.Item3 + ")";
                }
            }
        }

        #region Menu events
        private void saveToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string filename = saveFileDialog.FileName;

                ImageFormat format = null;
                if (filename.EndsWith("jpg")) format = ImageFormat.Jpeg;
                else if (filename.EndsWith("gif")) format = ImageFormat.Gif;
                else if (filename.EndsWith("png")) format = ImageFormat.Png;
                else format = ImageFormat.Bmp;

                pictureBox.Image.Save(filename, format);
            }
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SettingsDialog dialog = new SettingsDialog())
            {
                dialog.ShowDialog();
            }
        }
        #endregion

        #region Mouse events
        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            // prevent null pointer on non existing currentArea
            if (!running) return;

            int x = e.X;
            int y = e.Y;

            if (e.Button == MouseButtons.Left)
            {
                Reheat(currentArea.Matrix, x, y, currentArea.Width, currentArea.Height, tipSize, defaultHeat);
            }
        }
        #endregion

        private void startButton_Click(object sender, EventArgs e)
        {
            if (running)
            {
                running = false;
                generator.Stop();
            }
            else
            {
                running = true;
                InitArea();
                UpdateImage(currentArea);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            generator.ImageGenerated -= generator_ImageGenerated;
            if (running)
            {
                Invoke(new Action(generator.Stop));
            }
        }
    }
}
