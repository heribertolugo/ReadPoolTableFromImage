using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReadPoolTableFromImage
{
    public partial class Form1 : Form
    {
        private FileDialog dialog;
        public Form1()
        {
            InitializeComponent();
            dialog = new OpenFileDialog();
            dialog.Title = "Select an image of a pool table";
            dialog.Filter = "jpg files (*.jpg)|*.jpg|png files (*.png)|*.png|bmp files (*.bmp)|*.bmp";
        }

        private void loadImageButton_Click(object sender, EventArgs e)
        {
            if (this.dialog.ShowDialog() != DialogResult.OK)
                return;

            Image image = new Bitmap(this.dialog.FileName);

            this.originalPictureBox.Image = image;
            this.DisectImage(image);
        }

        PoolImageAnalyzer analyzer = null;
        private void DisectImage(Image image)
        {
            analyzer = new PoolImageAnalyzer(image);
            this.calculatedPictureBox.Image = analyzer.ReadAndAnalyze();

        }

        LabColor? previousLabColor = null;
        Color previousColor;
        private void calculatedPictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (analyzer == null)
                return;
            // prevent out of bound coordinates if picture is smaller than our picture box
            if (e.X > calculatedPictureBox.Image.Width || e.Y > calculatedPictureBox.Image.Height)
                return;

            Color color = analyzer.GetColorAtCoordinate(e.Location);
            LabColor labColor = new LabColor(color);
            // testing and sampling data under mouse cursor while over image.
            if (previousLabColor.HasValue)
            {
                float delta = LabColor.FindDeltaE94(previousLabColor.Value, labColor);
                // delta of 10 seems to produce good results in finding objects on the table
                if (delta >= 10)
                {
                    Console.WriteLine();
                    Console.WriteLine("--------------------------------");
                    Console.WriteLine("Color = " + color);
                    Console.WriteLine("Previous Color = " + previousColor);
                    Console.WriteLine("Delta = " + delta);
                }
            }

                    Console.WriteLine();
                    Console.WriteLine();

            previousColor = color;
            previousLabColor = labColor;
        }
    }
}
