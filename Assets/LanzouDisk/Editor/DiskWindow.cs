using LanZouAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using static LanZouWindow.DiskWindow.DiskData;

namespace LanZouWindow
{
    partial class DiskWindow
    {
        class Contents
        {
            private static GUIContent folder = EditorGUIUtility.IconContent("Folder Icon");
            public static GUIContent name = new GUIContent("Name","文件名字");
            public static GUIContent size = new GUIContent("Size","文件大小");
            public static GUIContent password = new GUIContent("*","有密码？");
            public static GUIContent desc = new GUIContent("...","文件描述");
            public static GUIContent down = new GUIContent("Downs","下载次数");
            public static GUIContent newfolder =new GUIContent(EditorGUIUtility.IconContent("d_CreateAddNew@2x")) { tooltip="新建文件夹"};
            public static GUIContent uploadfile =new GUIContent(EditorGUIUtility.IconContent("d_CreateAddNew@2x")) { tooltip="上传文件"};
            public static GUIContent goback = new GUIContent(EditorGUIUtility.IconContent("ArrowNavigationLeft")) { tooltip = "返回" };
            public static GUIContent gofront = new GUIContent(EditorGUIUtility.IconContent("ArrowNavigationRight")) { tooltip = "前进" };
            public static GUIContent goup = new GUIContent(EditorGUIUtility.IconContent("d_scrollup")) { tooltip = "返回上一级" };
            public static GUIContent fresh = new GUIContent(EditorGUIUtility.IconContent("d_TreeEditor.Refresh")) { tooltip = "刷新" };
            public static GUIContent set = new GUIContent(EditorGUIUtility.IconContent("d_TerrainInspector.TerrainToolSettings")) { tooltip = "设置" };
            public static GUIContent choosefolder = new GUIContent(EditorGUIUtility.IconContent("Folder Icon")) { tooltip = "选择保存路径" };
            public static GUIContent choose = new GUIContent("Auto", "选择子文件夹");
            public static GUIContent path= new GUIContent("Path:","当前路径");
            public static GUIContent titleContent = new GUIContent("LanzouDisk");
            public static GUIContent files = new GUIContent("Files");
            public static GUIContent folders = new GUIContent("Folders");

            public static GUIContent GetFolder(string name)
            {
                return new GUIContent(name, folder.image);
            }
        }
        class ContentPage
        {
            class FileTree : TreeView
            {
                private List<DiskData.FileData> files;
                private DiskTool tool { get { return _window.tool; } }
                public FileTree(TreeViewState state) : base(state)
                {
                    this.baseIndent = 20;
                    this.showAlternatingRowBackgrounds = true;
                    this.multiColumnHeader = new MultiColumnHeader(new MultiColumnHeaderState(new MultiColumnHeaderState.Column[] {

                        new MultiColumnHeaderState.Column()
                        {
                            headerContent=Contents.name,
                            minWidth = 300,
                            width = 300
                        },
                        new MultiColumnHeaderState.Column()
                        {
                            headerContent=Contents.size,
                            width=50,
                            autoResize=false,
                            maxWidth=50,
                        },

                           new MultiColumnHeaderState.Column()
                        {
                            headerContent=Contents.down,
                            width=40,
                            maxWidth=40,
                            minWidth=40,
                            autoResize=false
                        },
                        new MultiColumnHeaderState.Column()
                        {
                            headerContent=Contents.password,
                            width=20,
                            maxWidth=20,
                            minWidth=20,
                            autoResize=false
                        },
                         new MultiColumnHeaderState.Column()
                        {
                            headerContent=Contents.desc,
                            width=20,
                            maxWidth=20,
                            minWidth=20,
                            autoResize=false
                        },

                    }));
                    Reload();
                }

                public void ReadFiles(List<DiskData.FileData> files)
                {
                    this.files = files;
                    Reload();
                }

                private void DownLoad(object userData)
                {
                    DiskData.FileData data = (DiskData.FileData)userData;
                    tool.DownLoadFile(data.id, data.name);
                }
                private void Description(object userData)
                {
                    int id = (int)userData;
                    tool.ShowDescription(id, false);
                }
                private void Delete(object userData)
                {
                    int id = (int)userData;
                    tool.Delete((int)userData, true);
                }
                private void Share(object userData)
                {
                    tool.Share((int)userData, true);
                }
                private void OpenRename(object userData)
                {
                    int id = (int)userData;
                    BeginRename(GetRows().ToList().Find(_data => _data.id == id));
                }



                protected override TreeViewItem BuildRoot()
                {
                    return new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
                }
                protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
                {
                    List<TreeViewItem> rows = new List<TreeViewItem>();
                    if (files != null)
                    {
                        foreach (var item in files)
                        {
                            rows.Add(new TreeViewItem()
                            {
                                id = (int)item.id,
                                depth = 0,
                                displayName = item.name
                            });
                        }
                    }

                    return rows;
                }
                protected override void ContextClickedItem(int id)
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Rename"), false, OpenRename, id);
                    menu.AddItem(new GUIContent("Share"), false, Share, id);
                    menu.AddItem(new GUIContent("Delete"), false, Delete, id);
                    var data = files.Find(_data => { return _data.id == id; });
                    menu.AddItem(new GUIContent("DownLoad"), false, DownLoad, data);

                    if (data.has_des)
                    {
                        menu.AddItem(new GUIContent("Description"), false, Description, id);
                    }
                    menu.ShowAsContext();
                }
                protected override void RowGUI(RowGUIArgs args)
                {
                    var data = files.Find(_data => { return _data.id == args.item.id; });
                    for (int i = 0; i < args.GetNumVisibleColumns(); i++)
                    {
                        Rect rect = args.GetCellRect(i);
                        switch (i)
                        {
                            case 0:
                                GUI.Label(rect, data.name);
                                break;
                            case 1:
                                GUI.Label(rect, data.size);
                                break;
                            case 2:
                                GUI.Label(rect, data.downs.ToString());
                                break;
                            case 3:
                                if (data.has_pwd)
                                {
                                    GUI.Label(rect, Contents.password);
                                }
                                break;
                            case 4:
                                if (data.has_des)
                                {
                                    GUI.Label(rect, Contents.desc);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                protected override Rect GetRenameRect(Rect rowRect, int row, TreeViewItem item)
                {
                    return this.multiColumnHeader.GetCellRect(0, rowRect);
                }
                protected override void RenameEnded(RenameEndedArgs args)
                {
                    if (args.acceptedRename && args.originalName != args.newName && !string.IsNullOrEmpty(args.newName))
                    {
                        tool.RenameFile(args.itemID, args.newName);
                    }
                }
                protected override bool CanRename(TreeViewItem item)
                {
                    return true;
                }
            }
            class FolderTree : TreeView
            {

                private List<DiskData.FolderData> folders;
                private DiskTool tool { get { return _window.tool; } }

                public FolderTree(TreeViewState state) : base(state)
                {
                    this.baseIndent = 20;
                    this.showAlternatingRowBackgrounds = true;
                    this.multiColumnHeader = new MultiColumnHeader(new MultiColumnHeaderState(new MultiColumnHeaderState.Column[] {

                        new MultiColumnHeaderState.Column()
                        {
                            headerContent=Contents.name,
                            minWidth = 300,
                            width = 300
                        },
                        new MultiColumnHeaderState.Column()
                        {
                            headerContent=Contents.password,
                            width=20,
                            autoResize=false,
                            maxWidth=20,
                        },
                        new MultiColumnHeaderState.Column()
                        {
                            headerContent=Contents.desc,
                            width=20,
                            maxWidth=20,
                            minWidth=20,
                            autoResize=false
                        },
                    }));
                    Reload();
                }
                public void ReadFolders(List<DiskData.FolderData> folders)
                {
                    this.folders = folders;
                    Reload();
                }

                private void Delete(object userData)
                {
                    int id = (int)userData;
                    tool.Delete((int)userData, false);
                }
                private void Description(object userData)
                {
                    int id = (int)userData;
                    tool.ShowDescription(id, false);
                }
                private void Share(object userData)
                {
                    tool.Share((int)userData, false);
                }
                private void OpenRename(object userData)
                {
                    int id = (int)userData;
                    BeginRename(GetRows().ToList().Find(_data => _data.id == id));
                }



                protected override void ContextClickedItem(int id)
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Rename"), false, OpenRename, id);
                    menu.AddItem(new GUIContent("Share"), false, Share, id);
                    menu.AddItem(new GUIContent("Delete"), false, Delete, id);
                    var data = folders.Find(_data => { return _data.id == id; });
                    if (!string.IsNullOrEmpty(data.desc))
                    {
                        menu.AddItem(new GUIContent("Description"), false, Description, id);
                    }
                    menu.ShowAsContext();
                }
                protected override Rect GetRenameRect(Rect rowRect, int row, TreeViewItem item)
                {
                    return this.multiColumnHeader.GetCellRect(0, rowRect);
                }
                protected override void RenameEnded(RenameEndedArgs args)
                {
                    if (args.acceptedRename && args.originalName != args.newName && !string.IsNullOrEmpty(args.newName))
                    {
                        tool.RenameFolder(args.itemID, args.newName);
                    }
                }
                protected override bool CanRename(TreeViewItem item)
                {
                    return true;
                }
                protected override TreeViewItem BuildRoot()
                {
                    return new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
                }
                protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
                {
                    List<TreeViewItem> rows = new List<TreeViewItem>();
                    if (folders != null)
                    {
                        foreach (var item in folders)
                        {
                            rows.Add(new TreeViewItem()
                            {
                                id = (int)item.id,
                                depth = 0,
                                displayName = item.name
                            });
                        }
                    }
                    return rows;
                }
                protected override void DoubleClickedItem(int id)
                {
                    tool.OpenFolder(id);
                }
                protected override void RowGUI(RowGUIArgs args)
                {
                    var data = folders.Find(_data => { return _data.id == args.item.id; });
                    for (int i = 0; i < args.GetNumVisibleColumns(); i++)
                    {
                        Rect rect = args.GetCellRect(i);
                        switch (i)
                        {
                            case 0:
                                GUI.Label(rect, Contents.GetFolder(data.name));
                                break;
                            case 1:
                                if (data.has_pwd)
                                {
                                    GUI.Label(rect, Contents.password);
                                }
                                break;
                            case 2:
                                if (!string.IsNullOrEmpty(data.desc))
                                {
                                    GUI.Label(rect, Contents.desc);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }


            private FolderTree folder;
            private FileTree file;
            private SplitView sp;
            private DiskTool tool { get { return _window.tool; } }
            public ContentPage()
            {
                file = new FileTree(new TreeViewState());
                folder = new FolderTree(new TreeViewState());
                FreshView();
                sp = new SplitView() { split = 400 };
                sp.fistPan += First;
                sp.secondPan += Second;
            }
            public void FreshView()
            {
                if (tool.current != null)
                {
                    folder.ReadFolders(tool.current.folders);
                    file.ReadFiles(tool.current.files);
                }
                else
                {
                    folder.ReadFolders(new List<FolderData>());
                    file.ReadFiles(new List<FileData>());
                }
            }
            private void Second(Rect rect)
            {
                var rs = rect.HorizontalSplit(20);
                GUILayout.BeginArea(rs[0], EditorStyles.toolbar);
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label(Contents.files);
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(Contents.uploadfile, EditorStyles.toolbarButton))
                        {
                            tool.UpLoadFile();
                        }
                    }
                    GUILayout.EndHorizontal();

                }
                GUILayout.EndArea();
                file.OnGUI(rs[1]);
            }
            private void First(Rect rect)
            {
                var rs = rect.HorizontalSplit(20);
                GUILayout.BeginArea(rs[0], EditorStyles.toolbar);
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label(Contents.folders);
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(Contents.newfolder, EditorStyles.toolbarButton))
                        {
                            tool.NewFolder();
                        }
                    }
                    GUILayout.EndHorizontal();

                }
                GUILayout.EndArea();

                folder.OnGUI(rs[1]);
            }

            public void OnGUI(Rect rect)
            {
                sp.OnGUI(rect);
            }
        }
        class DiskTool
        {
            public void OnQuit()
            {
                if (lzy != null)
                {
                    lzy.Logout();
                    lzy = null;
                }
            }
            public async void Login()
            {
                lzy = new LanZouCloud();
                var result = await lzy.Login(cookie.ylogin, cookie.phpdisk_info);
                if (result != LanZouCode.SUCCESS) return;
                async void FreshData()
                {
                    var root = await lzy.GetFolderList(-1);
                    var fs = await lzy.GetFileList(-1);
                    await _window.data.ReadRoot(root.folders, fs.files);
                    SetCurrentFolder(-1);
                }
                FreshData();
            }
            private LanZouCloud lzy;
            private void FreshContent()
            {
                _window.content.FreshView();
            }
            public async Task FreshFolder(long id)
            {
                var ds = await lzy.GetFolderList(id);
                var fs = await lzy.GetFileList(id);
                _window.data.FreshFolder(id, ds.folders, fs.files);
            }

            public async void FreshCurrent()
            {
                await FreshFolder(current.id);
                FreshContent();
            }

            public async void Share(long fid, bool is_file = true)
            {
                if (is_file)
                {
                    var info = await lzy.GetFileShareInfo(fid);
                    GUIUtility.systemCopyBuffer = $"名字：{info.name}\n链接：{info.url}\n提取码：{info.password}\n描述：{info.description}";
                }
                else
                {
                    var info = await lzy.GetFolderShareInfo(fid);
                    GUIUtility.systemCopyBuffer = $"名字：{info.name}\n链接：{info.url}\n提取码：{info.password}\n描述：{info.description}";
                }

                _window.ShowNotification(new GUIContent("提取方式已复制到粘贴板"));
            }
            public async void ShowDescription(long fid, bool is_file = true)
            {
                if (is_file)
                {
                    var info = await lzy.GetFileShareInfo(fid);
                    _window.ShowNotification(new GUIContent(info.description));
                }
                else
                {
                    var info = await lzy.GetFolderShareInfo(fid);
                    _window.ShowNotification(new GUIContent(info.description));
                }

            }
            public async void DownLoadFile(long fid, string name)
            {
                string pname = name.Contains(".") ? name.Split('.')[0] : "";
                string ex = name.Contains(".") ? name.Split('.')[1] : "";
                string saveDir = "";
                if (_window.set.choose)
                {
                    saveDir = _window.set.rootSavePath;
                }
                else
                {
                    saveDir = EditorUtility.SaveFilePanel("Save", string.IsNullOrEmpty(_window.set.rootSavePath) ?
                        "Assets" : _window.set.rootSavePath, pname, ex);
                    if (string.IsNullOrEmpty(saveDir)) return;
                }
                var info = await lzy.GetFileShareInfo(fid);
                var code = await lzy.DownloadFileByUrl(info.url, saveDir, info.password, true, _window.downLoad);
                if (code.code != LanZouCode.SUCCESS)
                {
                    Debug.LogError("Down load Err " + code);
                }
            }
            public async void UpLoadFile()
            {
                string saveDir = EditorUtility.OpenFilePanel("Save", string.IsNullOrEmpty(_window.set.rootSavePath) ? 
                    "Assets" : _window.set.rootSavePath, "");
                if (!string.IsNullOrEmpty(saveDir) && File.Exists(saveDir))
                {
                    var code = await lzy.UploadFile(saveDir, current.id, true, _window.upLoad);
                    if (code.code == LanZouCode.SUCCESS)
                    {
                        await FreshFolder(current.id);
                        FreshContent();
                    }
                    else
                    {
                        Debug.LogError(code);
                    }
                }
            }

            public void OpenFolder(int id)
            {
                SetCurrentFolder(id);
                FreshContent();
            }
            public async void RenameFile(long file_id, string filename)
            {
                var code = await lzy.RenameFile(file_id, filename);
                if (code == LanZouCode.SUCCESS)
                {
                    await FreshFolder(current.id);
                    FreshContent();
                }
                else
                {
                    Debug.LogError(code);
                }

            }
            public async void RenameFolder(long folder_id, string folder_name)
            {
                var code = await lzy.RenameFolder(folder_id, folder_name);
                if (code == LanZouCode.SUCCESS)
                {
                    await FreshFolder(current.id);
                    FreshContent();
                }
                else
                {
                    Debug.LogError(code);
                }

            }
            public async void NewFolder()
            {
                var code = await lzy.CreateFolder("NewFolder", current.id);
                if (code.code == LanZouCode.SUCCESS)
                {
                    await FreshFolder(current.id);
                    FreshContent();
                }
                else
                {
                    Debug.LogError(code);
                }
            }

            public async void Delete(long fid, bool is_file)
            {
                if (is_file)
                {
                    var code = await lzy.DeleteFile(fid);
                    if (code == LanZouCode.SUCCESS)
                    {
                        await FreshFolder(current.id);
                        FreshContent();
                    }
                    else
                    {
                        Debug.LogError(code);
                    }
                }
                else
                {
                    var code = await lzy.DeleteFolder(fid);
                    if (code == LanZouCode.SUCCESS)
                    {
                        await FreshFolder(current.id);
                        FreshContent();
                    }
                    else
                    {
                        Debug.LogError(code);
                    }
                }

            }

            public void ChooseSavePath()
            {
                var str = EditorUtility.OpenFolderPanel("Save", "Assets", "");
                if (!string.IsNullOrEmpty(str) && Directory.Exists(str))
                {
                    _window.set.rootSavePath = str;
                }
            }


            private Stack<FolderData> memory = new Stack<FolderData>();
            private Stack<FolderData> stack = new Stack<FolderData>();
            private FolderData _current;
            public FolderData current
            {
                get { return _current; }
                set
                {
                    _current = value;
                    FreshContent();
                }
            }
            public void SetCurrentFolder(long id)
            {
                FolderData data = _window.data.FindFolderById(id);
                if (memory.Count != 0)
                {
                    memory.Clear();
                }
                stack.Push(data);
                current = stack.Peek();
            }
            public bool CanGoUp()
            {
                return !_window.data.IsRootFolder(current);
            }
            public bool CanGoBack()
            {
                return stack.Count > 1;
            }
            public bool CanGoFront()
            {
                return memory.Count > 0;
            }
            public void GoUp()
            {
                if (_window.data.IsRootFolder(current)) return;
                SetCurrentFolder(current.pid);
                FreshContent();
            }
            public void GoBack()
            {
                if (stack.Count <= 1) return;
                var data = stack.Pop();
                memory.Push(data);
                current = stack.Peek();
                FreshContent();

            }
            public void GoFront()
            {
                if (memory.Count <= 0) return;
                var data = memory.Pop();
                stack.Push(data);
                current = stack.Peek();
                FreshContent();
            }

        }
        public class DiskData
        {
            private DiskTool tool { get { return _window.tool; } }

            public class FileData
            {
                public long id;
                public string name;
                public string time;
                public string size;
                public string type;
                public int downs;
                public bool has_pwd;
                public bool has_des;
                public static implicit operator FileData(CloudFile c)
                {
                    return new FileData()
                    {
                        id = c.id,
                        name = c.name,
                        has_pwd = c.hasPassword,
                        time = c.time,
                        size = c.size,
                        type = c.type,
                        has_des = c.hasPassword,
                        downs = c.downloads,
                    };
                }
            }
            public class FolderData
            {
                public long pid;
                public string path;
                public long id;
                public string name;
                public bool has_pwd;
                public string desc;
                public List<FolderData> folders = new List<FolderData>();
                public List<FileData> files = new List<FileData>();
                public static implicit operator FolderData(CloudFolder c)
                {
                    return new FolderData()
                    {
                        id = c.id,
                        name = c.name,
                        has_pwd = c.hasPassword,
                        desc = c.description
                    };
                }
            }
            public class Root : FolderData
            {
                public List<FolderData> allfolders = new List<FolderData>();
                public List<FileData> allfiles = new List<FileData>();

                public void Clear()
                {
                    allfolders.Clear();
                    allfiles.Clear();
                }
            }
            private Root root;
            public bool IsRootFolder(FolderData data)
            {
                return data == _window.data.root;
            }
            public FolderData FindFolderById(long id)
            {
                if (id == -1) return root;
                return root.allfolders.Find(_data => { return _data.id == id; });
            }
            public async Task ReadRoot(List<CloudFolder> folders, List<CloudFile> fs)
            {
                root = new Root() { id = -1, path = "Root" };
                root.Clear();
                if (folders != null)
                {
                    root.folders = folders.ConvertAll(data =>
                    {
                        FolderData _data = data;
                        _data.pid = -1;
                        _data.path = root.path + "/" + _data.name;
                        return _data;
                    });
                }
                if (fs != null)
                {
                    root.files = fs.ConvertAll(data =>
                    {
                        FileData _data = data;
                        return _data;
                    });
                }

                root.allfiles.AddRange(root.files);
                root.allfolders.AddRange(root.folders);
                root.allfolders.Add(root);
                for (int i = 0; i < root.folders.Count; i++)
                {
                    await tool.FreshFolder(root.folders[i].id);
                }
            }
            public async void FreshFolder(long id, List<CloudFolder> folders, List<CloudFile> fs)
            {

                FolderData _f = root.allfolders.Find(_data => { return _data.id == id; });
                if (_f == null)
                {
                    Debug.LogError("not find dolder " + id);
                    return;
                }
                if (folders != null)
                {
                    _f.folders = folders.ConvertAll(data =>
                    {
                        FolderData _data = data;
                        _data.pid = _f.id;
                        _data.path = _f.path + "/" + _data.name;
                        return _data;
                    });
                }
                if (fs != null)
                {
                    _f.files = fs.ConvertAll(data =>
                    {
                        FileData _data = data;
                        return _data;
                    });
                }


                root.allfiles.AddRange(_f.files);
                root.allfolders.AddRange(_f.folders);
                for (int i = 0; i < _f.folders.Count; i++)
                {
                    await tool.FreshFolder(_f.folders[i].id);
                }
            }

        }

        public class ProgressBarView : IProgress<ProgressInfo>
        {
            private Queue<ProgressInfo> downs = new Queue<ProgressInfo>();
            private string txtFormat = "";
            public static ProgressBarView current { get; private set; }
            public ProgressBarView(string txtFormat)
            {
                this.txtFormat = txtFormat;
            }

            private object downsLock = new object();
            private string progressTxt = "";
            private float progress = 0;
            public void OnGUI(Rect rect)
            {
                EditorGUI.ProgressBar(rect, progress, progressTxt);
                lock (downsLock)
                {
                    if (downs.Count <= 0) return;
                    ProgressInfo value = null;
                    while (downs.Count > 0)
                    {
                        value = downs.Dequeue();
                    }
                    switch (value.state)
                    {
                        case ProgressState.Start:
                        case ProgressState.Ready:
                            progress = 0f;
                            progressTxt = string.Format(txtFormat, progress.ToString("0.00 %"), value.fileName);
                            break;
                        case ProgressState.Progressing:
                            progress = value.current / (float)value.total;
                            progressTxt = string.Format(txtFormat, progress.ToString("0.00 %"), value.fileName);
                            break;
                        case ProgressState.Finish:
                            current = null;
                            break;
                        default:
                            break;
                    }

                }

            }

            public void Report(ProgressInfo value)
            {
                current = this;

                lock (downsLock)
                {
                    switch (value.state)
                    {
                        case ProgressState.Start:
                            _window.repaint = true;
                            break;
                        case ProgressState.Ready:
                            break;
                        case ProgressState.Progressing:
                            break;
                        case ProgressState.Finish:
                            _window.repaint = false;
                            break;
                        default:
                            break;
                    }
                    downs.Enqueue(value);
                }
            }
        }
        [System.Serializable]
        private class Setting
        {
            public bool open = false;
            public string rootSavePath = "Asset";
            public bool choose = true;
        }

        private Setting set=new  Setting();
        private DiskData data;
        private DiskTool tool;
        private string cookiepath = "";
        private ContentPage content;
        private bool repaint = false;

        public static LanzouCookie cookie;
        private static DiskWindow _window;
        private ProgressBarView downLoad = new ProgressBarView("({0}) DownLoad\t{1}");
        private ProgressBarView upLoad = new ProgressBarView("({0}) UpLoad\t{1}");

    }

    partial class DiskWindow : EditorWindow
    {
        private const string key0 = "1321321321346DiskWindow5987941465554987879";

        private void OnEnable()
        {
            set = new Setting();
            titleContent = Contents.titleContent;
            if (EditorPrefs.HasKey(key0)) set = JsonUtility.FromJson<Setting>(EditorPrefs.GetString(key0));
            data = new DiskData();
            if (!string.IsNullOrEmpty(cookiepath)) cookie = AssetDatabase.LoadAssetAtPath<LanzouCookie>(cookiepath);
            _window = this;
            tool = new DiskTool();
            content = new ContentPage();
            tool.Login();
        }
        private void OnDisable()
        {
            EditorPrefs.SetString(key0, JsonUtility.ToJson(set));
            cookiepath = AssetDatabase.GetAssetPath(cookie);
            tool.OnQuit();
        }
        private void OnGUI()
        {
            var local = new Rect(Vector2.zero, position.size);
            var rs = local.HorizontalSplit(20);
            var rs1 = rs[1].HorizontalSplit(rs[1].height - 20);
            ToolBar(rs[0]);
            content.OnGUI(rs1[0]);
            if (ProgressBarView.current!=null) ProgressBarView.current.OnGUI(rs1[1].Zoom(AnchorType.MiddleCenter, -6));
            if (repaint) Repaint();

        }
        private void ToolBar(Rect rect)
        {
            using (new EditorGUI.DisabledGroupScope(tool.current == null))
            {
                GUILayout.BeginArea(rect, EditorStyles.toolbar);
                {
                    GUILayout.BeginHorizontal();
                    {
                        using (new EditorGUI.DisabledGroupScope(!tool.CanGoBack()))
                        {

                            if (GUILayout.Button(Contents.goback, EditorStyles.toolbarButton))
                            {
                                tool.GoBack();
                            }
                        }
                        using (new EditorGUI.DisabledGroupScope(!tool.CanGoFront()))
                        {

                            if (GUILayout.Button(Contents.gofront, EditorStyles.toolbarButton))
                            {
                                tool.GoFront();
                            }
                        }
                        using (new EditorGUI.DisabledGroupScope(!tool.CanGoUp()))
                        {

                            if (GUILayout.Button(Contents.goup, EditorStyles.toolbarButton))
                            {
                                tool.GoUp();
                            }
                        }
                        if (GUILayout.Button(Contents.fresh, EditorStyles.toolbarButton))
                        {
                            tool.FreshCurrent();
                        }
                        GUILayout.Space(10);
                        using (new EditorGUI.DisabledGroupScope(true))
                        {
                            var op = GUILayout.MaxWidth(200);
                            if (tool.current != null)
                            {
                                GUILayout.Label(Contents.path);
                                GUILayout.TextField(tool.current.path, op);
                            }
                        }
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(Contents.set, GUILayout.Width(30)))
                        {
                            set.open = !set.open;
                        }
                        if (set.open)
                        {
                            set.choose = GUILayout.Toggle(set.choose, Contents.choose, EditorStyles.toolbarButton);
                            if (GUILayout.Button(Contents.choosefolder, EditorStyles.toolbarButton, GUILayout.Width(30)))
                            {
                                tool.ChooseSavePath();
                            }
                            GUILayout.Label(set.rootSavePath);
                        }

                    }
                    GUILayout.EndHorizontal();

                }
                GUILayout.EndArea();
            }
        }
    }
}
