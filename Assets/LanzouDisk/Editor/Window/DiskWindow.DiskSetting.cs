namespace LanZouWindow
{
    partial class DiskWindow
    {
        [System.Serializable]
        public class DiskSetting
        {
            public ToolType select;
            public string rootSavePath = "Asset";
            public bool uploadOverWrite = false;
            public bool downloadOverWrite = true;
            public string NewFolderName = "NewFolder";
            public string NewFolderDesc;
            public bool showDownLoadList = true;
            public bool showUpLoadList = true;

        }

    }
}
