using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace ReadPoolTableFromImage
{
    public static class ImageHelpers
    {
        public static byte[] ToByteArray(this System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            return ms.ToArray();
        }

        public static Image ToImage(this byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }


        public static byte[] PixelsToByteArray(this System.Drawing.Image imageIn, out int stride, out Bitmap bitmap)
        {
            Bitmap bmp = new Bitmap(imageIn);

            // Lock the bitmap's bits.  
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            //FindTableClothColor(ref rgbValues, bmpData.Stride);

            //// Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            // Unlock the bits.
            bmp.UnlockBits(bmpData);

            stride = bmpData.Stride;
            bitmap = new Bitmap(bmp);
            return rgbValues;

            ////Pixel array
            //byte[] pixels = new byte[bmp.Width * bmp.Height * 4]; //account for stride if necessary and whether the image is 32 bit, 16 bit etc.

            //bmp.CopyPixels(..size, pixels, fullStride, 0);
        }

        /// <summary>
        /// This is only for testing. It will create a sample from the image and give it a red hue.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="stride"></param>
        private static void FindTableClothColor(ref byte[] bytes, int stride)
        {
            //LabColor labColor1 = new LabColor(51f, 2, 2);
            //LabColor labColor2 = new LabColor(50f, 0, 0);

            //float delta = LabColor.FindDeltaE94(labColor1, labColor2);

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
                    bytes[offset] = 255;
                    byte alpha = bytes[++offset];
                    Color pixelColor = Color.FromArgb(alpha, red, green, blue);

                    if (!colorPopularity.ContainsKey(pixelColor))
                        colorPopularity.Add(pixelColor, 0);
                    colorPopularity[pixelColor] += 1;
                }
            }

            Color popular = colorPopularity.ToList().OrderByDescending(k => k.Value).First().Key;
            LabColor labColor = new LabColor(popular);
        }

    }
}
