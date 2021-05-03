using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadPoolTableFromImage
{
    public class PoolImageAnalyzer
    {
        public const float BallDiameterInInches = 2.5f; 
        private Image _image;
        private Image _disectedImage;
        private IList<BallOnTable> _ballsOnTable;
        private Color _poolTableColor;
        private int _stride;

        public PoolImageAnalyzer(string path)
        {
            this._image = new Bitmap(path);
            this._ballsOnTable = new List<BallOnTable>();
        }

        public PoolImageAnalyzer(Image image)
        {
            this._image = image;
        }

        public byte[] Bytes
        {
            get; private set;
        }

        public Image ReadAndAnalyze()
        {
            Bitmap bitmap;
            this.Bytes = this._image.PixelsToByteArray(out this._stride, out bitmap);

            this._disectedImage = bitmap;

            //Color tableClothColor = FindTableClothColor(bytes, this._stride);

            return this._disectedImage;
        }

        public Image DisectedImage
        {
            get
            {
                return this._disectedImage;
            }
        }

        public void FindObjectsOnTable(Color tableClothColor, byte[] imageBytes)
        {

        }

        public Color GetColorAtCoordinate(Point point)
        {
            if (this.Bytes.Length < 1)
                throw new Exception("Cannot get color without an image");
            if (this._stride < 1)
                throw new Exception("Stride is not defined");
            int xOffset = point.X * 4;
            int yOffset = point.Y * this._stride;
            int offset = xOffset + yOffset;

            if (offset >= this.Bytes.Length || offset < 0)
                throw new IndexOutOfRangeException("The coordinates are outside of the image");

            byte blue = this.Bytes[offset];
            byte green = this.Bytes[++offset];
            byte red = this.Bytes[++offset];
            byte alpha = this.Bytes[++offset];
            Color pixelColor = Color.FromArgb(alpha, red, green, blue);

            return pixelColor;
        }

        /// <summary>
        /// Determines table cloth color by taking a sample from the image represented in the byte array.
        /// The most popular color in the sample should reflect the table cloth color.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="stride"></param>
        /// <returns></returns>
        private static Color FindTableClothColor(byte[] bytes, int stride)
        {
            int bytesPerPixel = 4; // each pixel is represented by 4 bytes
            int imageHeight = bytes.Length / stride; // get the height of the image represented by our bytes
            int imageWidth = stride / bytesPerPixel;
            Size quadrantSize = new Size(imageWidth / 2, imageHeight / 2); // split the image into 4 equally and get size of 1 piece
            // using our quadrant, create a rectangle at the center of the image 
            Rectangle sampleRect = new Rectangle(new Point(quadrantSize.Width / 2, quadrantSize.Height / 2), quadrantSize);
            // dictionary which will hold the number of occurrences for each color in our quadrant sample
            Dictionary<Color, int> colorPopularity = new Dictionary<Color, int>();

            // since the table cloth will be the most popular color we need not sample the entire image.
            // to retain some performance we'll sample the center of the image to determine the table cloth color
            // with such a small sample we shouldn't have to worry too much about the color variance caused by highlights and shadows
            // ....hopefully

            // we will read our sample (represented by sampleRect) from left to right along the X axis
            // and then continue on the next Y axis, which is obtained by using the images stride value
            for (int yPixelIndex = (sampleRect.Y * stride); yPixelIndex <= (sampleRect.Bottom * stride); yPixelIndex += stride)
            {
                for (int xPixelIndex = sampleRect.X * bytesPerPixel; xPixelIndex <= sampleRect.Right * bytesPerPixel; xPixelIndex += (bytesPerPixel))
                {
                    int offset = xPixelIndex + yPixelIndex;
                    if (offset >= bytes.Length)
                        break; // this should never happen
                    byte blue = bytes[offset];
                    byte green = bytes[++offset];
                    byte red = bytes[++offset];
                    byte alpha = bytes[++offset];
                    Color pixelColor = Color.FromArgb(alpha, red, green, blue);

                    if (!colorPopularity.ContainsKey(pixelColor))
                        colorPopularity.Add(pixelColor, 0);
                    colorPopularity[pixelColor] += 1; // increment a colors popularity
                }
            }
            // return the most popular color in our sample
            return colorPopularity.ToList().OrderByDescending(k => k.Value).First().Key;
        }


        public static IDictionary<TableSize, Size> TableSizesMap = new Dictionary<TableSize, Size>()
        {

        };
    }
}
