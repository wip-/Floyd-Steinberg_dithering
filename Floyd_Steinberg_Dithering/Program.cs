using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Media;
using System.Threading;

namespace Floyd_Steinberg_Dithering
{
    public class Program
    {
        static Bitmap _display;
        static Bitmap _biTonal;
        static Timer _updateClockTimer;
        public static void Main()
        {
            // initialize our display buffer
            _display = new Bitmap(Bitmap.MaxWidth, Bitmap.MaxHeight);
            _biTonal = Dither();
            _display.DrawImage(0, 0, _biTonal, 0, 0, _biTonal.Width, _biTonal.Height);
            // flush the display buffer to the display
            _display.Flush();
            Thread.Sleep(1000);
            _display.DrawImage(0, 0, _biTonal, 0, 0, _biTonal.Width, _biTonal.Height);
            // flush the display buffer to the display
            _display.Flush();
            Thread.Sleep(1000);

            //// display the time immediately
            //UpdateTime(null);

            //// obtain the current time
            //DateTime currentTime = DateTime.Now;
            //// set up timer to refresh time every minute
            //TimeSpan dueTime = new TimeSpan(0, 0, 0, 59 - currentTime.Second, 1000 - currentTime.Millisecond); // start timer at beginning of next minute
            //TimeSpan period = new TimeSpan(0, 0, 1, 0, 0); // update time every minute
            //_updateClockTimer = new Timer(UpdateTime, null, dueTime, period); // start our update timer

            // go to sleep; time updates will happen automatically every minute
            Thread.Sleep(Timeout.Infinite);
        }

        static void UpdateTime(object state)
        {
            // obtain the current time
            DateTime currentTime = DateTime.Now;
            // clear our display buffer
            _display.Clear();

            // add your watchface drawing code here
            Font fontNinaB = Resources.GetFont(Resources.FontResources.NinaB);
            _display.DrawText(currentTime.Hour.ToString("D2") + ":" + currentTime.Minute.ToString("D2"), fontNinaB, Color.White, 46, 58);

            // flush the display buffer to the display
            _display.Flush();
        }

        /// Algorithm for performing Floyd-Steinberg Dithering on grayscale images to create bitonal images
        /// From http://en.wikipedia.org/wiki/Floyd%E2%80%93Steinberg_dithering
        ///
        /// for each y from top to bottom
        ///    for each x from left to right
        ///       oldpixel := pixel[x][y]
        ///       newpixel := find_closest_palette_color(oldpixel) -- Replace with WhiteOrBlack()
        ///       pixel[x][y] := newpixel
        ///       quant_error := oldpixel - newpixel
        ///       pixel[x+1][y] := pixel[x+1][y] + 7/16 * quant_error
        ///       pixel[x-1][y+1] := pixel[x-1][y+1] + 3/16 * quant_error
        ///       pixel[x][y+1] := pixel[x][y+1] + 5/16 * quant_error
        ///       pixel[x+1][y+1] := pixel[x+1][y+1] + 1/16 * quant_error
        private static Bitmap Dither()
        {
            byte[] gsBytes = Resources.GetBytes(Resources.BinaryResources.BlackAndWhiteAndGray); //GrayScale.GetBitmap();
            int bitmapHeaderSize = gsBytes.Length - 128 * 128;
            for (int y = 0; y < 128; y++)
                for (int x = 0; x < 128; x++)
                {
                    int pixelNdx = bitmapHeaderSize + y * 128 + x;
                    // Floyd-Steinberg Dithering
                    byte oldpixel = gsBytes[pixelNdx];
                    byte newpixel = RoundToBiTonal(oldpixel);
                    int quant_error = oldpixel - newpixel;

                    gsBytes[pixelNdx] = newpixel;
                    if (x + 1 < 128)
                        gsBytes[pixelNdx + 1] += ForceByte(quant_error * 7 / 16);
                    if (x - 1 > 0 & y + 1 < 128)
                        gsBytes[pixelNdx + 128 - 1] += ForceByte(quant_error * 3 / 16);
                    if (y + 1 < 128)
                        gsBytes[pixelNdx + 128 + 0] += ForceByte(quant_error * 5 / 16);
                    if (x + 1 < 128 & y + 1 < 128)
                        gsBytes[pixelNdx + 128 + 1] += ForceByte(quant_error * 1 / 16);
                }
            return new Bitmap(gsBytes, Bitmap.BitmapImageType.Bmp);
        }

        private static byte RoundToBiTonal(byte GrayscalePixel)
        {
            const byte white = 254; // Don't know what 255 is, but it looks black on screen.
            const byte black = 0;
            if (GrayscalePixel > white / 2) return white;
            else return black;
        }

        private static byte ForceByte(int value)
        {
            if (value < Byte.MinValue) return Byte.MinValue;
            if (value > Byte.MaxValue) return Byte.MaxValue;
            else return (byte)value;
        }
    }
}
