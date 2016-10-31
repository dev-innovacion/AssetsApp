using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Drawing.Drawing2D;

namespace Rivka.Images
{
    public class Images
    {
        private int cropX;
        private int cropY;
        private int cropHeight;
        private int cropWidth;

        private Image img;
        private Image imgcrop;
        private String path;
        private String name;

        private int resizeWidth;
        private int resizeHeight;

        public Images(string fullPath, string pathImage = "", string imageName = "") {
            this.img = Image.FromFile(fullPath);
            this.path = pathImage;
            this.name = imageName;
        }

        public void SetCoords(string x, string y, string width, string height){
            this.cropX = Convert.ToInt32(x);
            this.cropY = Convert.ToInt32(y);
            this.cropHeight = Convert.ToInt32(height);
            this.cropWidth = Convert.ToInt32(width);
        }

        public int getwidth() {
            return img.Width;
        }

        public int getheight()
        {
            return img.Height;
        }

        public void SetWidthHeight(string width, string height)
        {
            this.resizeHeight = Convert.ToInt32(height);
            this.resizeWidth = Convert.ToInt32(width);
        }

        public void cropOK()
        {
            imgcrop = this.cropImage(img, new Rectangle(this.cropX, this.cropY, this.cropWidth, this.cropHeight));
        }

        private Image cropImage(Image img, Rectangle cropArea)
        {
            Bitmap bmpImage = new Bitmap(img);
            Bitmap bmpCrop = bmpImage.Clone(cropArea,
                                            bmpImage.PixelFormat);
            return (Image)(bmpCrop);
        }

        /// <summary>
        ///     Makes a resize to the size given
        /// </summary>
        /// <param name="size">
        ///     The maximum new size of the image, use new Size(100,100) object
        /// </param>
        public void resizeImage(Size size)
        {
            int sourceWidth = this.img.Width;
            int sourceHeight = this.img.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)size.Width / (float)sourceWidth);
            nPercentH = ((float)size.Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);
            ImageFormat format = this.img.RawFormat;
            Image newImage = new Bitmap(destWidth, destHeight);
            Graphics.FromImage(newImage).DrawImage(this.img, 0, 0, destWidth, destHeight);

            // If image exists delete before save the new one.
            if (System.IO.File.Exists(this.path + "\\" + this.name))
            {
                this.img.Dispose();
                System.IO.File.Delete(this.path + "\\" + this.name);
                newImage.Save(this.path + "\\" + this.name, format);
                this.img = Image.FromFile(this.path + "\\" + this.name);
            }
            else
                newImage.Save(this.path + "\\" + this.name, this.img.RawFormat);

        }

        public void SaveImage(string path) {
            this.saveJpeg(path, new Bitmap(this.imgcrop), 85L);
        }

        private void saveJpeg(string path, Bitmap img, long quality)
        {
            // Encoder parameter for image quality
            EncoderParameter qualityParam =
                new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

            // Jpeg image codec
            ImageCodecInfo jpegCodec = getEncoderInfo("image/jpeg");

            if (jpegCodec == null)
                return;

            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;

            img.Save(path, jpegCodec, encoderParams);
        }

        private ImageCodecInfo getEncoderInfo(string mimeType)
        {
            // Get image codecs for all image formats
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];
            return null;
        }

        /// <summary>
        ///     Creates the thumbnail of a single image, by default max_height and max_width = 100px, 
        ///     and save it into the same folder with de "thumb_" prefix.
        /// </summary>
        /// <param name="maxWidth">
        ///     The max width of the image
        /// </param>
        /// <param name="maxHeight">
        ///     The max height of the image
        /// </param>
        public void createThumb(int maxWidth = 100, int maxHeight = 100)
        {
            Image srcImage = this.img;

            var ratioX = (double)maxWidth / srcImage.Width;
            var ratioY = (double)maxHeight / srcImage.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(srcImage.Width * ratio);
            var newHeight = (int)(srcImage.Height * ratio);

            Image newImage = new Bitmap(newWidth, newHeight);
            Graphics.FromImage(newImage).DrawImage(srcImage, 0, 0, newWidth, newHeight);

            newImage.Save(this.path + "\\thumb_" + this.name, srcImage.RawFormat);

        }
    }
}