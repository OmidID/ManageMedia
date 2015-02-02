using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Windows.Forms;
using OmidID.Drawing;
using OmidID.IO.SaveMedia.Config;
using OmidID.IO.SaveMedia.Exceptions;
using OmidID.IO.SaveMedia.Validation;

namespace OmidID.IO.SaveMedia {
    public class SaveUploadFile {

        #region Properties

        public Config.UploadSettings Settings { get; private set; }
        public Config.SaveSetting SaveSetting { get; private set; }

        #endregion

        #region Init

        public SaveUploadFile() : this(string.Empty) { }
        public SaveUploadFile(string SettingName) : this(string.Empty, SettingName) { }
        public SaveUploadFile(string ConfigSectionName, string SettingName) {
            Settings = Config.UploadSettings.GetSettings(ConfigSectionName);
            if (Settings == null)
                throw new InvalidConfigSourceException("'upload' section was not found.");

            if (string.IsNullOrEmpty(SettingName))
                SettingName = Settings.Default;

            SaveSetting = Settings.Items[SettingName];
            if (SaveSetting == null)
                throw new SettingNotFoundException(string.Format("save setting \"{0}\" was not found in the collection.", SettingName));
        }

        public SaveUploadFile(Config.UploadSettings setting) {
            if (setting == null)
                throw new ArgumentNullException("setting");

            this.Settings = setting;
        }

        #endregion

        #region Save

        public void Save(string inputName) {
            var file = HttpContext.Current.Request.Files[inputName];
            Save(file);
        }

        public void Save(HttpPostedFile file) {
            if (file == null)
                throw new ArgumentNullException("file");

            if (file.ContentLength < 1)
                throw new ArgumentException("Uploaded file is empty", "file");

            Save(file.InputStream, file.FileName);
        }

        public string Save(Stream stream, string fileName) {
            var validate = Validate(stream, fileName);
            if (validate.Count > 0)
                validate.ThrowException();

            var args = new SavingFileEventArgs(fileName, stream) {
                Path = SaveSetting.Path,
                Handled = false,
                AppPath = SaveSetting.Path
            };

            GenerateFileName(args);
            Saving(args);

            var keepFile = false;
            if (SaveSetting.CheckImageSetting)
                SaveImage(args, out keepFile);

            if (!keepFile)
                if (!SaveSetting.KeepOrginalImage && SaveSetting.CheckImageSetting)
                    File.Delete(args.SaveAs);

            return Path.GetFileName(args.SaveAs);
        }

        protected virtual void Saving(SavingFileEventArgs args) {
            if (File.Exists(args.SaveAs))
                File.Delete(args.SaveAs);

            if (!Directory.Exists(args.Path)) Directory.CreateDirectory(args.Path);
            using (var stream = File.Open(args.SaveAs, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)) {
                var buffer = new byte[1024];
                var readed = args.Stream.Read(buffer, 0, 1024);

                while (readed > 0) {
                    stream.Write(buffer, 0, readed);
                    readed = args.Stream.Read(buffer, 0, 1024);
                }
            }
        }

        #endregion

        #region Save Image

        private void SaveImage(SavingFileEventArgs args, out bool KeepFile) {
            var path = args.Path;
            var filename = Path.GetFileNameWithoutExtension(args.SaveAs);
            KeepFile = false;

            try {
                using (var mbit = new Bitmap(args.SaveAs)) {
                    foreach (var item in SaveSetting.ImageSetting) {
                        if (item.SaveImmidately)
                            SaveImage(mbit, path, filename, item);
                        else
                            KeepFile = true;
                    }
                }
            } catch { }
        }

        public void SaveImage(string bitmapFile, string ImageSettingKey) {
            var path = Path.GetDirectoryName(bitmapFile);
            var filename = Path.GetFileNameWithoutExtension(bitmapFile);

            using (var mbit = new Bitmap(bitmapFile))
                SaveImage(mbit, path, filename, SaveSetting.ImageSetting[ImageSettingKey]);
        }

        public void SaveImage(Bitmap mainBitmap, string path, string filename, Config.ImageSetting item) {
            var dir = Path.Combine(path, item.Name);
            var file = Path.Combine(dir, filename) + "." + item.SaveAs.ToString();

            using (var bit = DrawToBitmap(mainBitmap, item)) {
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                bit.Save(file, Tools.ConvertToImageFormat(item.SaveAs));
            }
        }

        public void SaveImage(Bitmap mainBitmap, Stream stream, ImageFormat format, Config.ImageSetting item) {
            using (var bit = DrawToBitmap(mainBitmap, item))
                bit.Save(stream, format);
        }

        public void SaveImage(Bitmap mainBitmap, Stream stream, Config.SaveType format, Config.ImageSetting item, EncoderParameters eparams) {
            var c = ImageCodecInfo.GetImageEncoders();
            var info = c.FirstOrDefault(w => w.MimeType == Tools.GetMimeType(format));
            using (var bit = DrawToBitmap(mainBitmap, item))
                bit.Save(stream, info, eparams);
        }

        public Bitmap DrawToBitmap(string bitmapFileName, string itemName) {
            return DrawToBitmap(bitmapFileName, SaveSetting.ImageSetting[itemName]);
        }

        public Bitmap DrawToBitmap(string bitmapFileName, Config.ImageSetting item) {
            using (var mbit = new Bitmap(bitmapFileName))
                return DrawToBitmap(mbit, item);
        }

        public Bitmap DrawToBitmap(Bitmap mainBitmap, string itemName) {
            return DrawToBitmap(mainBitmap, SaveSetting.ImageSetting[itemName]);
        }

        public Bitmap DrawToBitmap(Bitmap mainBitmap, Config.ImageSetting item) {
            var bit = GetImage(item, mainBitmap.Size);
            using (var g = Graphics.FromImage(bit)) {
                DrawImage(g, mainBitmap, item);

                if (item.Foreground != null)
                    if (!string.IsNullOrEmpty(item.Foreground.Path)) {
                        var foreground = GetForegroundImage(item.Foreground);
                        DrawImage(g, foreground, item.Foreground);
                    }
            }

            return bit;
        }

        protected virtual void DrawImage(Graphics graphic, Bitmap bitmap, Config.BaseImageSetting item) {
            if (item.Alpha < 1) {
                var CM = new ColorMatrix();
                var Ia = new ImageAttributes();

                CM.Matrix33 = item.Alpha;
                Ia.SetColorMatrix(CM);

                graphic.DrawImageEx(bitmap, new Rectangle(item.X, item.Y, Convert.ToInt32(graphic.VisibleClipBounds.Width), Convert.ToInt32(graphic.VisibleClipBounds.Height)), item.Zoom, Ia);
            } else {
                graphic.DrawImageEx(bitmap, new Rectangle(item.X, item.Y, Convert.ToInt32(graphic.VisibleClipBounds.Width), Convert.ToInt32(graphic.VisibleClipBounds.Height)), item.Zoom);
            }
        }

        private Bitmap GetForegroundImage(Config.Foreground foreground) {
            if (!string.IsNullOrEmpty(foreground.Path)) {
                if (foreground.Path.StartsWith("#")) {
                    return GetEmptyImage(foreground, new Size(foreground.Width, foreground.Height), foreground.Path);

                } else {
                    try {
                        var path = Tools.GetPath(foreground.Path);
                        if (System.IO.File.Exists(path)) {
                            var img = new Bitmap(path);
                            if (foreground.ChangeResolution)
                                img.SetResolution(foreground.ResolutionX, foreground.ResolutionY);

                            return img;
                        }
                    } catch { }
                }
            }
            return GetEmptyImage(foreground, new Size(foreground.Width, foreground.Height));
        }

        private Bitmap GetImage(Config.ImageSetting setting, Size ImageSize) {
            if (!string.IsNullOrEmpty(setting.Background)) {
                if (setting.Background.StartsWith("#")) {
                    return GetEmptyImage(setting, ImageSize, setting.Background);
                } else {
                    try {
                        var path = Tools.GetPath(setting.Background);
                        if (System.IO.File.Exists(path)) {
                            var img = new Bitmap(path);
                            if (setting.ChangeResolution)
                                img.SetResolution(setting.ResolutionX, setting.ResolutionY);

                            return img;
                        }
                    } catch { }
                }
            }

            return GetEmptyImage(setting, ImageSize);
        }

        private Bitmap GetEmptyImage(Config.BaseImageSetting setting, Size ImageSize) {
            var size = OmidID.Drawing.ImageResizer.CalculateImageSize(setting.Zoom, ImageSize, new Size(setting.Width, setting.Height));
            var img = new Bitmap(size.Width, size.Height);
            if (setting.ChangeResolution)
                img.SetResolution(setting.ResolutionX, setting.ResolutionY);

            return img;
        }

        private Bitmap GetEmptyImage(Config.BaseImageSetting setting, Size ImageSize, string BackgroundColor) {
            var img = GetEmptyImage(setting, ImageSize);

            using (var g = Graphics.FromImage(img))
                g.Clear(ColorTranslator.FromHtml(BackgroundColor));

            return img;
        }

        #endregion

        #region Path And File Manager

        public void GenerateFileName(SavingFileEventArgs args) {
            var extention = Path.GetExtension(args.FileName);
            args.Path = Tools.GetPath(args.Path);

            switch (SaveSetting.GenerateFileName) {
                case Config.FilenameType.GUID:
                    args.SaveAs = GenerateGUIDFileName(args);
                    break;
                case Config.FilenameType.Overwrite:
                    args.SaveAs = Path.Combine(args.Path, args.FileName);
                    args.AppPath += (args.AppPath.EndsWith("/") ? "" : "/") + args.FileName;
                    break;
                case Config.FilenameType.Auto:
                    args.SaveAs = GenerateAutoFileName(args);
                    break;
                case Config.FilenameType.Custom:
                    SaveSetting.GetFilenameGenerator().GenerateFileName(args);
                    args.Path = Path.GetDirectoryName(args.SaveAs);
                    break;
            }
        }

        private string GenerateGUIDFileName(SavingFileEventArgs args) {
            var Extention = Path.GetExtension(args.FileName);
            var filename = Guid.NewGuid().ToString() + Extention;
            var p = Path.Combine(args.Path, filename);

            if (File.Exists(p))
                p = GenerateGUIDFileName(args);
            else
                args.AppPath += (args.AppPath.EndsWith("/") ? "" : "/") + filename;

            return p;
        }

        private string GenerateAutoFileName(SavingFileEventArgs args) {
            var Extention = Path.GetExtension(args.FileName);
            var name = Path.GetFileNameWithoutExtension(args.FileName);
            var filename = name + Extention;

            var p = Path.Combine(args.Path, args.FileName);
            if (!Directory.Exists(args.Path))
                Directory.CreateDirectory(args.Path);
            if (Directory.GetFiles(args.Path, Path.GetFileNameWithoutExtension(args.FileName) + ".*").Any()) {
                var num = 0;

                do {
                    num++;
                    filename = name + "_" + num.ToString();
                    p = Path.Combine(args.Path, filename) + Extention;
                } while (Directory.GetFiles(args.Path, Path.GetFileNameWithoutExtension(filename) + ".*").Any());
            }

            args.AppPath += (args.AppPath.EndsWith("/") ? "" : "/") + filename;
            return p;
        }

        #endregion

        #region Validation

        public ValidateCollection Validate(Stream stream, string fileName) {
            var col = new ValidateCollection();

            if (stream.Length / 1024 > SaveSetting.MaximumSize)
                col.Add(new InvalidItem(InvalidType.MaximumSize, string.Format("Maximum size can be {0}kb", SaveSetting.MaximumSize), SaveSetting.MaximumSize));


            if (!SaveSetting.GetAcceptExtention().Contains(Path.GetExtension(fileName).Substring(1).ToLower()))
                col.Add(new InvalidItem(InvalidType.FileType, string.Format("File type can be \"{0}\"", SaveSetting.AcceptExtention), SaveSetting.AcceptExtention));

            return col;
        }

        #endregion

    }
}
