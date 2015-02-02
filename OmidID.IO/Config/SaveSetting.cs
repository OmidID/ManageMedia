using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace OmidID.IO.SaveMedia.Config {

    public enum FilenameType {
        GUID,
        Overwrite,
        Auto,
        Custom
    }

    public class SaveSetting : BaseKeyElement {

        string acceptExtention = "jpg,jpeg,png,bmp,gif,tif,tiff";
        string[] acceptExtentionSplited  = new string[] { "jpg", "jpeg", "png", "bmp", "gif", "tif", "tiff" };
        CustomFileNameProvider filenameGenerator;
        ImageSettingCollection imageSetting=new ImageSettingCollection();

        public string[] GetAcceptExtention() {
            if (acceptExtention != AcceptExtention) {
                acceptExtention = AcceptExtention;
                acceptExtentionSplited = acceptExtention.ToLower().Split(',');
            }

            return acceptExtentionSplited;
        }

        public CustomFileNameProvider GetFilenameGenerator() {
            if (filenameGenerator == null) {
                var t = Type.GetType(CustomFileNameType);
                if (t == null)
                    throw new ArgumentException("customFileNameType");

                filenameGenerator = Activator.CreateInstance(t) as CustomFileNameProvider;
                if (filenameGenerator == null)
                    throw new TypeInitializationException(t.FullName, null);
            }

            return filenameGenerator;
        }

        [ConfigurationProperty("path", DefaultValue = "~/Upload", IsRequired = false)]
        public string Path { get { return (string)this["path"]; } set { this["path"] = value; } }

        [ConfigurationProperty("extentions", DefaultValue = "jpg,jpeg,png,bmp,gif,tif,tiff", IsRequired = false)]
        public string AcceptExtention { get { return (string)this["extentions"]; } set { this["extentions"] = value; } }

        [ConfigurationProperty("maximumSize", DefaultValue = "1024", IsRequired = false)]
        [LongValidator]
        public long MaximumSize { get { return (long)this["maximumSize"]; } set { this["maximumSize"] = value; } }

        [ConfigurationProperty("checkImageSetting", DefaultValue = "true", IsRequired = false)]
        public bool CheckImageSetting { get { return (bool)this["checkImageSetting"]; } set { this["checkImageSetting"] = value; } }

        [ConfigurationProperty("imageSetting", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(ImageSettingCollection),
                                 AddItemName = "add",
                                 ClearItemsName = "clear",
                                 RemoveItemName = "remove")]
        public ImageSettingCollection ImageSetting {
            get {
                ImageSettingCollection imageSettingCollection =
                (ImageSettingCollection)base["imageSetting"];
                return imageSettingCollection;
            }

        }

        [ConfigurationProperty("keepOrginalImage", DefaultValue = "true", IsRequired = false)]
        public bool KeepOrginalImage { get { return (bool)this["keepOrginalImage"]; } set { this["keepOrginalImage"] = value; } }

        [ConfigurationProperty("generateFileName", DefaultValue = "Auto", IsRequired = false)]
        public FilenameType GenerateFileName { get { return (FilenameType)this["generateFileName"]; } set { this["generateFileName"] = value; } }

        [ConfigurationProperty("customFileNameType", DefaultValue = "", IsRequired = false)]
        public string CustomFileNameType { get { return (string)this["customFileNameType"]; } set { this["customFileNameType"] = value; } }

    }

}
