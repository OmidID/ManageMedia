using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace OmidID.Drawing {

    public enum ZoomType {
        Tile,
        Center,
        Stretch,
        Zoom,
        CenterIfNoZoom,
        Crop
    }

    public static class ImageResizer {

        public static Bitmap ResizeImage(this Bitmap b, Size size) {
            return ResizeImage(b, size, ZoomType.Zoom);
        }

        public static Bitmap ResizeImage(this Bitmap b, Size size, ZoomType ZoomType) {
            var bit = new Bitmap(size.Width, size.Height);
            using (var g = Graphics.FromImage(bit)) {
                g.DrawImageEx(b, new Rectangle(0, 0, size.Width, size.Height), ZoomType);
            }

            return bit;
        }

        public static Bitmap ResizeImage(this Bitmap b, Size size,
                                  CompositingQuality CompositingQuality,
                                  InterpolationMode InterpolationMode,
                                  PixelOffsetMode PixelOffsetMode,
                                  SmoothingMode SmoothingMode,
                                  ZoomType ZoomType
                                 ) {
            var bit = new Bitmap(size.Width, size.Height);
            using (var g = Graphics.FromImage(bit)) {
                g.DrawImageEx(b, new Rectangle(0, 0, size.Width, size.Height), CompositingQuality, InterpolationMode,
                              PixelOffsetMode, SmoothingMode, ZoomType, null);
            }

            return bit;
        }

        public static void DrawImageEx(this Graphics g, Bitmap bit, ZoomType ZoomType) {
            DrawImageEx(g, bit, new Rectangle(Convert.ToInt32(g.ClipBounds.X), Convert.ToInt32(g.ClipBounds.Y), Convert.ToInt32(g.ClipBounds.Width), Convert.ToInt32(g.ClipBounds.Height)), ZoomType);
        }

        public static void DrawImageEx(this Graphics g, Bitmap bit, ZoomType ZoomType, ImageAttributes ImageAttributes) {
            DrawImageEx(g, bit, new Rectangle(Convert.ToInt32(g.ClipBounds.X), Convert.ToInt32(g.ClipBounds.Y), Convert.ToInt32(g.ClipBounds.Width), Convert.ToInt32(g.ClipBounds.Height)), ZoomType, ImageAttributes);
        }

        public static void DrawImageEx(this Graphics g, Bitmap bit, Rectangle area, ZoomType ZoomType) {
            DrawImageEx(g, bit, area, CompositingQuality.HighQuality, InterpolationMode.High, PixelOffsetMode.HighQuality, SmoothingMode.HighQuality, ZoomType, null);
        }

        public static void DrawImageEx(this Graphics g, Bitmap bit, Rectangle area, ZoomType ZoomType, ImageAttributes ImageAttributes) {
            DrawImageEx(g, bit, area, CompositingQuality.HighQuality, InterpolationMode.High, PixelOffsetMode.HighQuality, SmoothingMode.HighQuality, ZoomType, ImageAttributes);
        }

        public static void DrawImageEx(this Graphics g, Bitmap bit, Rectangle area,
                                       CompositingQuality CompositingQuality,
                                       InterpolationMode InterpolationMode,
                                       PixelOffsetMode PixelOffsetMode,
                                       SmoothingMode SmoothingMode,
                                       ZoomType ZoomType, ImageAttributes ImageAttributes
                               ) {
            g.CompositingQuality = CompositingQuality;
            g.InterpolationMode = InterpolationMode;
            g.PixelOffsetMode = PixelOffsetMode;
            g.SmoothingMode = SmoothingMode;

            if (ZoomType == ZoomType.CenterIfNoZoom) {
                if (bit.Width < area.Width && bit.Height < area.Height)
                    ZoomType = Drawing.ZoomType.Center;
                else
                    ZoomType = Drawing.ZoomType.Zoom;
            }

            switch (ZoomType) {
                case ZoomType.Tile:
                    if (ImageAttributes != null) {
                        using (var brush = new TextureBrush(bit, area, ImageAttributes)) {
                            g.FillRectangle(brush, area);
                        }
                    } else {
                        using (var brush = new TextureBrush(bit)) {
                            g.FillRectangle(brush, area);
                        }
                    }
                    break;
                case ZoomType.Center:
                    area.X += (area.Width - bit.Width) / 2;
                    area.Y += (area.Height - bit.Height) / 2;

                    area.Width = bit.Width;
                    area.Height = bit.Height;

                    FinalDraw(g, bit, area, ImageAttributes);
                    break;
                case ZoomType.Stretch:
                    FinalDraw(g, bit, area, ImageAttributes);
                    break;
                case ZoomType.Zoom:
                    int nw, nh;
                    SetZoomSize(bit.Width, bit.Height, area.Width, area.Height, out nw, out nh);

                    area.X = ((area.Width - nw) / 2) + area.X;
                    area.Y = ((area.Height - nh) / 2) + area.Y;
                    area.Width = nw;
                    area.Height = nh;

                    FinalDraw(g, bit, area, ImageAttributes);
                    break;
                case Drawing.ZoomType.Crop:
                    SetCorpSize(bit.Width, bit.Height, area.Width, area.Height, out nw, out nh);

                    g.SetClip(area);
                    area.X = ((area.Width - nw) / 2) + area.X;
                    area.Y = ((area.Height - nh) / 2) + area.Y;
                    area.Width = nw;
                    area.Height = nh;

                    FinalDraw(g, bit, area, ImageAttributes);
                    break;
            }
        }

        private static void FinalDraw(Graphics g, Bitmap bit, Rectangle area, ImageAttributes ImageAttributes) {
            //var x = new FrameDimension(bit.FrameDimensionsList[0]);
            //bit.SelectActiveFrame(x, 10)

            if (ImageAttributes != null)
                g.DrawImage(bit, area, area.X, area.Y, area.Width, area.Height, GraphicsUnit.Pixel, ImageAttributes);
            else
                g.DrawImage(bit, area);
        }

        public static void SetZoomSize(int width, int height, int dw, int dh, out int nw, out int nh) {
            nh = height * dw / width;
            nw = dw;

            if (nh > dh) {
                nw = width * dh / height;
                nh = dh;
            }
        }

        public static void SetCorpSize(int width, int height, int dw, int dh, out int nw, out int nh) {
            nh = height * dw / width;
            nw = dw;

            if (nh < dh) {
                nw = width * dh / height;
                nh = dh;
            }
        }

        public static Size CalculateImageSize(ZoomType ZoomType, Size FromSize, Size ToSize) {
            if (ZoomType == ZoomType.CenterIfNoZoom) {
                if (FromSize.Width < ToSize.Width && FromSize.Height < ToSize.Height)
                    ZoomType = Drawing.ZoomType.Center;
                else
                    ZoomType = Drawing.ZoomType.Zoom;
            }

            switch (ZoomType) {
                case Drawing.ZoomType.Tile: return ToSize;
                case Drawing.ZoomType.Center:
                    var left = (FromSize.Width - ToSize.Width) / 2;
                    var top = (FromSize.Height - ToSize.Height) / 2;

                    return new Size(left < 0 ? ToSize.Width : ToSize.Width - left, top < 0 ? ToSize.Height : ToSize.Height - top);
                case Drawing.ZoomType.Stretch: return ToSize;
                case Drawing.ZoomType.Zoom:
                     int nw, nh;
                     SetZoomSize(FromSize.Width, FromSize.Height, ToSize.Width, ToSize.Height, out nw, out nh);
                     return new Size(nw, nh);
            }

            return ToSize;
        }

    }
}
