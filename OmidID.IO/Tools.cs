using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace OmidID.IO.SaveMedia {
    public static class Tools {

        static SortedDictionary<string, Models.UploadInfo> uploads = new SortedDictionary<string, Models.UploadInfo>();
        public static Models.UploadInfo GetUpdateInfo(string id) {
            var key = id;
            if (uploads.ContainsKey(key))
                return uploads[key];

            return null;
        }

        public static void RemoveUpdateInfo(string id) { if (uploads.ContainsKey(id)) uploads.Remove(id); }

        public static Models.UploadInfo NewUpdateInfo(string id) {
            var key = id;
            var item = new Models.UploadInfo(id);
            uploads[key] = item;
            return item;
        }

        public static string FileSize(long UploadedLength) {
            var outstr = "";
            if (UploadedLength < 1024)
                outstr = UploadedLength.ToString() + " byte";
            else if (UploadedLength < 1048576)
                outstr = (UploadedLength / 1024).ToString() + " Kb";
            else if (UploadedLength < 1073741824)
                outstr = (Convert.ToDecimal(UploadedLength) / 1048576M).ToString("###,##0.00") + " MB";

            return outstr;
        }

        public static FileType GetTypeOfFile(string Path) {
            switch (System.IO.Path.GetExtension(Path).Substring(1).ToLower()) {
                case "doc":
                case "docx":
                case "txt":
                case "rtf":
                case "pdf":
                case "htm":
                case "html":
                case "ptt":
                case "pss":
                case "pttx":
                case "pssx":
                case "xls":
                case "xlsx":
                case "accdb":
                case "mdb":
                case "mht":
                case "mhtl":
                case "mpp":
                case "wps":
                case "xml":
                    return FileType.Documents;
                case "jpg":
                case "jpeg":
                case "bmp":
                case "gif":
                case "png":
                case "psd":
                case "tiff":
                case "tif":
                case "tga":
                case "ico":
                case "icon":
                case "cur":
                case "al":
                case "dxf":
                case "crd":
                    return FileType.Pictures;
                case "mpg":
                case "mpeg":
                case "avi":
                case "3gp":
                case "mov":
                case "ogg":
                case "mp4":
                case "asf":
                case "wmv":
                case "rm":
                case "mpe":
                case "mpa":
                case "vob":
                case "ifo":
                case "m1v":
                case "m2v":
                case "mp2":
                case "swf":
                    return FileType.Videos;
                case "exe":
                case "dll":
                case "msi":
                case "com":
                case "hqx":
                case "msc":
                case "ocx":
                case "hta":
                    return FileType.Software;
                case "zip":
                case "rar":
                case "7z":
                case "tar":
                case "gz":
                case "wim":
                    return FileType.Compress;
                default:
                    return FileType.Other;
            }
        }


        public static ImageFormat ConvertToImageFormat(Config.SaveType saveType) {
            switch (saveType) {
                case Config.SaveType.PNG:
                    return ImageFormat.Png;
                case Config.SaveType.JPEG:
                    return ImageFormat.Jpeg;
                case Config.SaveType.GIF:
                    return ImageFormat.Gif;
                case Config.SaveType.BMP:
                    return ImageFormat.Bmp;
            }

            return ImageFormat.Png;
        }

        public static String GetMimeType(Config.SaveType type) {
            String s = null;
            switch (type) {
                case Config.SaveType.BMP:
                    s = "bmp";
                    break;
                case Config.SaveType.GIF:
                    s = "gif";
                    break;
                case Config.SaveType.JPEG:
                    s = "jpeg";
                    break;
                case Config.SaveType.PNG:
                    s = "png";
                    break;
            }
            if (!String.IsNullOrEmpty(s))
                s = String.Format("image/{0}", s);

            return s;
        }

        public static string GetMimeType(string fileName) {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }

        public static void CheckFile(string SettingName, string SaveSettingName, string ImageSettingName, string Filename, Stream Output) {
            var uploader = new SaveUploadFile(SettingName, SaveSettingName);
            var args = new SavingFileEventArgs(Filename, null) { AppPath = uploader.SaveSetting.Path };

            uploader.GenerateFileName(args);
            if (!File.Exists(args.SaveAs)) {
                try {
                    if (!Directory.Exists(args.Path))
                        Directory.CreateDirectory(args.Path);

                    var fpath = Tools.GetPath(args.AppPath);
                    var files = Directory.GetFiles(Path.GetDirectoryName(fpath), Path.GetFileNameWithoutExtension(args.FileName) + ".*");
                    var filename = files.Single();
                    if (!File.Exists(filename)) return;

                    using (var bit = new Bitmap(filename)) {
                        var item = uploader.SaveSetting.ImageSetting[ImageSettingName];
                        var dir = args.Path;
                        var file = Path.Combine(dir, Path.GetFileNameWithoutExtension(Filename)) + "." + item.SaveAs.ToString();

                        if (Output == null) {
                            if (!File.Exists(file))
                                using (var nbit = uploader.DrawToBitmap(bit, item)) {
                                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                                    nbit.Save(file, ConvertToImageFormat(item.SaveAs));
                                }
                        } else {
                            if (!File.Exists(file)) {
                                var open = File.OpenRead(file);
                                var buffer = new byte[2048];
                                var readed = open.Read(buffer, 0, 2048);

                                while (readed > 0) {
                                    Output.Write(buffer, 0, readed);
                                    readed = open.Read(buffer, 0, 2048);
                                }
                            } else {
                                using (var nbit = uploader.DrawToBitmap(bit, item)) {
                                    nbit.Save(Output, ConvertToImageFormat(item.SaveAs));
                                }
                            }
                        }
                    }

                } catch { }
            }
        }

        public static string GetPath(string path) {
            var ctx = System.Web.HttpContext.Current;
            var p = string.Empty;

            if (ctx == null) {
                if (path.StartsWith("~/"))
                    p = Application.StartupPath + path.Substring(2);

                if (path.StartsWith("/"))
                    p = Application.StartupPath + path.Substring(1);

                p = p.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            } else {
                p = ctx.Server.MapPath(path);
            }

            return p;
        }

        public static TempFileStream GetRandomFile() {
            var file = GetRandomFileName();
            var stream = new TempFileStream(file);

            CheckFileForDelete();

            return stream;
        }

        public static string GetRandomFileName() {
            if (!System.IO.Directory.Exists(TempUploadDirectory))
                Directory.CreateDirectory(TempUploadDirectory);

            return Path.Combine(TempUploadDirectory, Path.GetRandomFileName());
        }

        static string TempUploadDirectory = GetPath("~/Temps");
        private static void CheckFileForDelete() {
            if (Directory.Exists(TempUploadDirectory)) {

                foreach (var item in Directory.GetFiles(TempUploadDirectory)) {
                    try {
                        using (var stm = File.OpenWrite(item)) { }
                        File.Delete(item);
                    } catch { }
                }
            }
        }

    }
}
