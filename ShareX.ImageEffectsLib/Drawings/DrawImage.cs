﻿#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2020 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using ShareX.HelpersLib;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.IO;

namespace ShareX.ImageEffectsLib
{
    [Description("Image")]
    public class DrawImage : ImageEffect
    {
        [DefaultValue(""), Editor(typeof(ImageFileNameEditor), typeof(UITypeEditor))]
        public string ImageLocation { get; set; }

        [DefaultValue(ContentAlignment.BottomRight), TypeConverter(typeof(EnumProperNameConverter))]
        public ContentAlignment Placement { get; set; }

        [DefaultValue(typeof(Point), "5, 5")]
        public Point Offset { get; set; }

        [DefaultValue(DrawImageSizeMode.DontResize), Description("How the image watermark should be rescaled, if at all."), TypeConverter(typeof(EnumDescriptionConverter))]
        public DrawImageSizeMode SizeMode { get; set; }

        [DefaultValue(typeof(Size), "0, 0")]
        public Size Size { get; set; }

        [DefaultValue(ImageInterpolationMode.HighQualityBicubic), TypeConverter(typeof(EnumProperNameConverter))]
        public ImageInterpolationMode InterpolationMode { get; set; }

        [DefaultValue(CompositingMode.SourceOver), TypeConverter(typeof(EnumProperNameConverter))]
        public CompositingMode CompositingMode { get; set; }

        [DefaultValue(true), Description("If image watermark size bigger than source image then don't draw it.")]
        public bool AutoHide { get; set; }

        public DrawImage()
        {
            this.ApplyDefaultPropertyValues();
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            if (SizeMode != DrawImageSizeMode.DontResize && Size.Width <= 0 && Size.Height <= 0)
            {
                return bmp;
            }

            string imageFilePath = Helpers.ExpandFolderVariables(ImageLocation, true);

            if (!string.IsNullOrEmpty(imageFilePath) && File.Exists(imageFilePath))
            {
                using (Bitmap bmpWatermark = ImageHelpers.LoadImage(imageFilePath))
                {
                    if (bmpWatermark != null)
                    {
                        Size imageSize;

                        if (SizeMode == DrawImageSizeMode.AbsoluteSize)
                        {
                            imageSize = ImageHelpers.ApplyAspectRatio(Size, bmpWatermark);
                        }
                        else if (SizeMode == DrawImageSizeMode.PercentageOfWatermark)
                        {
                            int width = (int)Math.Round(Size.Width / 100f * bmpWatermark.Width);
                            int height = (int)Math.Round(Size.Height / 100f * bmpWatermark.Height);
                            imageSize = ImageHelpers.ApplyAspectRatio(width, height, bmpWatermark);
                        }
                        else if (SizeMode == DrawImageSizeMode.PercentageOfCanvas)
                        {
                            int width = (int)Math.Round(Size.Width / 100f * bmp.Width);
                            int height = (int)Math.Round(Size.Height / 100f * bmp.Height);
                            imageSize = ImageHelpers.ApplyAspectRatio(width, height, bmpWatermark);
                        }
                        else
                        {
                            imageSize = bmpWatermark.Size;
                        }

                        Point imagePosition = Helpers.GetPosition(Placement, Offset, bmp.Size, imageSize);
                        Rectangle imageRectangle = new Rectangle(imagePosition, imageSize);

                        if (AutoHide && !new Rectangle(0, 0, bmp.Width, bmp.Height).Contains(imageRectangle))
                        {
                            return bmp;
                        }

                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.InterpolationMode = ImageHelpers.GetInterpolationMode(InterpolationMode);
                            g.PixelOffsetMode = PixelOffsetMode.Half;
                            g.CompositingMode = CompositingMode;
                            g.DrawImage(bmpWatermark, imageRectangle);
                        }
                    }
                }
            }

            return bmp;
        }
    }
}