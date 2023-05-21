using System.Drawing;
using System.Drawing.Drawing2D;

namespace SceneSculpt
{
    public static class BitmapExtensions
    {
        public static Bitmap ResizeAndCrop(this Bitmap b, Rectangle r)
        {
            int sourceWidth = b.Width;
            int sourceHeight = b.Height;
            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;
            nPercentW = ((float)r.Width / (float)sourceWidth);
            nPercentH = ((float)r.Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;
            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);
            Bitmap resized = new Bitmap(destWidth, destHeight);

            using (Graphics g = Graphics.FromImage(resized))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(b, 0, 0, destWidth, destHeight);
            }

            Bitmap target = new Bitmap(r.Width, r.Height);
            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(
                    resized,
                    new Rectangle(0, 0, target.Width, target.Height),
                    r,
                    GraphicsUnit.Pixel
                );

                return target;
            }
        }
    }
}
