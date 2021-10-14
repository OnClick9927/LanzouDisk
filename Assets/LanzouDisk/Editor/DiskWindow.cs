using LanZouAPI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace LanZouWindow
{
    partial class DiskWindow
    {
        abstract class Page
        {
            public abstract void OnGUI();
        }

        class ContentPage : Page
        {
            class FileTree : TreeView
            {
                public string name;
                public string type;
                public string size;
                public string time;
                public int downs;
                public bool has_pwd;
                public bool has_des;
                public FileTree(TreeViewState state) : base(state)
                {
                    this.baseIndent = 20;
                    this.showAlternatingRowBackgrounds = true;
                    this.multiColumnHeader = new MultiColumnHeader(new MultiColumnHeaderState(new MultiColumnHeaderState.Column[] {

                        new MultiColumnHeaderState.Column()
                        {
                            headerContent=new GUIContent("Name"),
                            minWidth = 300,
                            width = 300
                        },
                        new MultiColumnHeaderState.Column()
                        {
                            headerContent=new GUIContent("Size"),
                            width=50,
                            autoResize=false,
                            maxWidth=50,
                        },

                           new MultiColumnHeaderState.Column()
                        {
                            headerContent=new GUIContent("Downs"),
                            width=40,
                            maxWidth=40,
                            minWidth=40,
                            autoResize=false
                        },
                        new MultiColumnHeaderState.Column()
                        {
                            headerContent=new GUIContent("*"),
                            width=20,
                            maxWidth=20,
                            minWidth=20,
                            autoResize=false
                        },
                         new MultiColumnHeaderState.Column()
                        {
                            headerContent=new GUIContent("..."),
                            width=20,
                            maxWidth=20,
                            minWidth=20,
                            autoResize=false
                        },

                    }));
                }
                private List<DiskData.FileData> files;

                public void ReadFiles(List<DiskData.FileData> files)
                {
                    this.files = files;
                    Reload();
                }
                protected override TreeViewItem BuildRoot()
                {
                    return new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
                }

                protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
                {
                    List<TreeViewItem> rows = new List<TreeViewItem>();
                    foreach (var item in files)
                    {
                        rows.Add(new TreeViewItem()
                        {
                            id = (int)item.id,
                            depth = 0,
                            displayName = item.name
                        });
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

                private void DownLoad(object userData)
                {
                    DiskData.FileData data = (DiskData.FileData)userData;
                    DiskTool.DownLoad(data.id, data.name, true);

                }

                private void Description(object userData)
                {
                    int id = (int)userData;

                    DiskTool.ShowDescription(id, false);

                }
                private void Delete(object userData)
                {
                    int id = (int)userData;
                    DiskTool.Delete((int)userData, true);

                }

                private void Share(object userData)
                {
                    DiskTool.Share((int)userData, true);
                }

                private void OpenRename(object userData)
                {
                    int id = (int)userData;
                    BeginRename(GetRows().ToList().Find(_data => _data.id == id));
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
                                    GUI.Label(rect, "*");
                                }
                                break;
                            case 4:
                                if (data.has_des)
                                {
                                    GUI.Label(rect, "...");
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
                        DiskTool.RenameFile(args.itemID, args.newName);
                    }
                }
                protected override bool CanRename(TreeViewItem item)
                {
                    return true;
                }
            }
            class FolderTree : TreeView
            {
                public FolderTree(TreeViewState state) : base(state)
                {
                    this.baseIndent = 20;
                    this.showAlternatingRowBackgrounds = true;
                    this.multiColumnHeader = new MultiColumnHeader(new MultiColumnHeaderState(new MultiColumnHeaderState.Column[] {

                        new MultiColumnHeaderState.Column()
                        {
                            headerContent=new GUIContent("Name"),
                            minWidth = 300,
                            width = 300
                        },
                        new MultiColumnHeaderState.Column()
                        {
                            headerContent=new GUIContent("*"),
                            width=20,
                            autoResize=false,
                            maxWidth=20,
                        },
                        new MultiColumnHeaderState.Column()
                        {
                            headerContent=new GUIContent("..."),
                            width=20,
                            maxWidth=20,
                            minWidth=20,
                            autoResize=false
                        },
                    }));
                }

                private List<DiskData.FolderData> folders;
                public void ReadFolders(List<DiskData.FolderData> folders)
                {
                    this.folders = folders;
                    Reload();
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


                private void Delete(object userData)
                {
                    int id = (int)userData;
                    DiskTool.Delete((int)userData, false);

                }
                private void Description(object userData)
                {
                    int id = (int)userData;

                    DiskTool.ShowDescription(id, false);

                }

                private void Share(object userData)
                {
                    DiskTool.Share((int)userData, false);
                }

                private void OpenRename(object userData)
                {
                    int id = (int)userData;
                    BeginRename(GetRows().ToList().Find(_data => _data.id == id));
                }

                protected override Rect GetRenameRect(Rect rowRect, int row, TreeViewItem item)
                {
                    return this.multiColumnHeader.GetCellRect(0, rowRect);
                }
                protected override void RenameEnded(RenameEndedArgs args)
                {
                    if (args.acceptedRename && args.originalName != args.newName && !string.IsNullOrEmpty(args.newName))
                    {
                        DiskTool.RenameFolder(args.itemID, args.newName);
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
                    foreach (var item in folders)
                    {
                        rows.Add(new TreeViewItem()
                        {
                            id = (int)item.id,
                            depth = 0,
                            displayName = item.name
                        });
                    }
                    return rows;
                }
                protected override void DoubleClickedItem(int id)
                {
                    DiskTool.OpenFolder(id);
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
                                var content = data.files.Count == 0 ? EditorGUIUtility.IconContent("Folder Icon") : EditorGUIUtility.IconContent("FolderEmpty Icon");
                                GUI.Label(rect, new GUIContent(data.name, content.image));
                                break;
                            case 1:
                                if (data.has_pwd)
                                {
                                    GUI.Label(rect, "*");
                                }
                                break;

                            case 2:
                                if (!string.IsNullOrEmpty(data.desc))
                                {
                                    GUI.Label(rect, "...");
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
            public ContentPage()
            {

            }
            public void FreshView()
            {
                folder.ReadFolders(_window.data.current.folders);
                file.ReadFiles(_window.data.current.files);
            }
            public void Init()
            {
                file = new FileTree(new TreeViewState());
                folder = new FolderTree(new TreeViewState());
                FreshView();
                sp = new SplitView() { split = 400 };
                sp.fistPan += First;
                sp.secondPan += Second;

            }

            private void Second(Rect rect)
            {
                var rs = rect.HorizontalSplit(20);
                GUILayout.BeginArea(rs[0], EditorStyles.toolbar);
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Files");
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(EditorGUIUtility.IconContent("d_CreateAddNew@2x"), EditorStyles.toolbarButton))
                        {
                            DiskTool.UpLoad();

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
                        GUILayout.Label("Folders");
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(EditorGUIUtility.IconContent("d_CreateAddNew@2x"), EditorStyles.toolbarButton))
                        {
                            DiskTool.NewFolder();
                        }
                    }
                    GUILayout.EndHorizontal();

                }
                GUILayout.EndArea();
                folder.OnGUI(rs[1]);
            }

            public override void OnGUI()
            {

                Rect local = new Rect(Vector2.zero, _window.position.size);

                var rs = local.HorizontalSplit(20);
                var rs1 = rs[1].HorizontalSplit(rs[1].height - 20);
                ToolBar(rs[0]);

                sp.OnGUI(rs1[0]);
                GUI.Box(rs1[1], "");
                EditorGUI.ProgressBar(rs1[1].Zoom(AnchorType.MiddleCenter, -6), progress, progressTxt);
            }
            private void ToolBar(Rect rect)
            {
                GUILayout.BeginArea(rect, EditorStyles.toolbar);
                {
                    GUILayout.BeginHorizontal();
                    {
                        using (new EditorGUI.DisabledGroupScope(!_window.data.CanGoBack()))
                        {

                            if (GUILayout.Button(EditorGUIUtility.IconContent("ArrowNavigationLeft"), EditorStyles.toolbarButton))
                            {
                                DiskTool.GoBack();
                            }
                        }
                        using (new EditorGUI.DisabledGroupScope(!_window.data.CanGoFront()))
                        {

                            if (GUILayout.Button(EditorGUIUtility.IconContent("ArrowNavigationRight"), EditorStyles.toolbarButton))
                            {
                                DiskTool.GoFront();
                            }
                        }
                        using (new EditorGUI.DisabledGroupScope(!_window.data.CanGoUp()))
                        {

                            if (GUILayout.Button(EditorGUIUtility.IconContent("d_scrollup"), EditorStyles.toolbarButton))
                            {
                                DiskTool.GoUp();
                            }
                        }
                        if (GUILayout.Button(EditorGUIUtility.IconContent("d_TreeEditor.Refresh"), EditorStyles.toolbarButton))
                        {
                            DiskTool.FreshCurrent();
                        }
                        GUILayout.Space(10);
                        using (new EditorGUI.DisabledGroupScope(true))
                        {
                            GUILayout.TextField("Path:" + _window.data.current.path);
                        }
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(EditorGUIUtility.IconContent("d_TerrainInspector.TerrainToolSettings"), GUILayout.Width(30)))
                        {
                            set = !set;
                        }
                        if (set)
                        {
                            _window.autoPath = GUILayout.Toggle(_window.autoPath, "Auto", EditorStyles.toolbarButton);
                            if (GUILayout.Button(EditorGUIUtility.IconContent("Folder Icon"), EditorStyles.toolbarButton, GUILayout.Width(30)))
                            {
                                DiskTool.ChooseSavePath();
                            }
                            GUILayout.Label("Save:" + _window.rootSavePath);
                        }

                    }
                    GUILayout.EndHorizontal();

                }
                GUILayout.EndArea();
            }
            private bool set;
            private string progressTxt = "";
            private float progress;
            public void ShowProgress(DownloadProgressInfo value)
            {
                switch (value.state)
                {
                    case DownloadProgressInfo.State.Start:
                    case DownloadProgressInfo.State.Ready:
                        progress = 0f;
                        progressTxt = $"DownLoad {value.filename}";
                        break;
                    case DownloadProgressInfo.State.Downloading:
                        progress = value.current / (float)value.total;
                        progressTxt = $"DownLoad {value.filename} ({value.current / value.total})";
                        break;
                    case DownloadProgressInfo.State.Finish:
                        progressTxt = "";
                        progress = 0;
                        break;
                    default:
                        break;
                }
            }

            public void FinishProgress()
            {

            }

            public void ShowProgress(UploadProgressInfo value)
            {
                switch (value.state)
                {
                    case UploadProgressInfo.State.Start:
                    case UploadProgressInfo.State.Ready:
                        progress = 0f;
                        progressTxt = $"UpLoad {value.filename}";
                        break;
                    case UploadProgressInfo.State.Uploading:
                        progress = value.current / (float)value.total;
                        progressTxt = $"UpLoad {value.filename} ({value.current / value.total})";
                        break;
                    case UploadProgressInfo.State.Finish:
                        progressTxt = "";
                        progress = 0;
                        break;
                    default:
                        break;
                }
            }
        }
        static class DiskTool
        {
            public static void Quit()
            {
                _window.Close();
            }
            public static void OnQuit()
            {
                if (lzy != null)
                {
                    lzy.logout();
                    lzy = null;
                }
            }
            private static LanZouCloud lzy;
            public static async void Login()
            {
                lzy = new LanZouCloud();
                var result = await lzy.login_by_cookie(_window.data.ylogin, _window.data.phpdisk_info);
                if (result != LanZouCode.SUCCESS) return;
                FreshData();
                _window.content.Init();
            }
            public static async void FreshData()
            {
                if (lzy == null)
                {
                    Login();
                    return;
                }
                var root = await lzy.get_folder_list(-1);
                var fs = await lzy.get_file_list(-1);
                _window.data.ReadRoot(root.folders, fs.files);
            }
            public static void FreshCurrent()
            {
                FreshFolder(_window.data.current.id);
                _window.content.FreshView();
            }
            public static async void FreshFolder(long id)
            {
                if (lzy == null)
                {
                    Login();
                    return;
                }
                var ds = await lzy.get_folder_list(id);
                var fs = await lzy.get_file_list(id);
                _window.data.FreshFolder(id, ds.folders, fs.files);
            }

            public static async void Share(long fid, bool is_file = true)
            {
                if (lzy == null)
                {
                    Login();
                    return;
                }
                var info = await lzy.get_share_info(fid, is_file);
                GUIUtility.systemCopyBuffer = $"名字：{info.name}\n链接：{info.url}\n提取码：{info.pwd}\n描述：{info.desc}";
                _window.ShowNotification(new GUIContent("提取方式已复制到粘贴板"));
            }
            public static async void ShowDescription(long fid, bool is_file = true)
            {
                if (lzy == null)
                {
                    Login();
                    return;
                }
                var info = await lzy.get_share_info(fid, is_file);
                _window.ShowNotification(new GUIContent(info.desc));
            }
            public static async void DownLoad(long fid, string name, bool is_file = true)
            {
                if (lzy == null)
                {
                    Login();
                    return;
                }
                string pname = name.Contains(".") ? name.Split('.')[0] : "";
                string ex = name.Contains(".") ? name.Split('.')[1] : "";
                string saveDir = "";
                if (_window.autoPath)
                {
                    saveDir = _window.rootSavePath;
                }
                else
                {
                    saveDir = EditorUtility.SaveFilePanel("Save", string.IsNullOrEmpty(_window.rootSavePath) ? "Assets" : _window.rootSavePath, pname, ex);
                    if (string.IsNullOrEmpty(saveDir)) return;
                }
                var info = await lzy.get_share_info(fid, is_file);
                var code = await lzy.down_file_by_url(info.url, saveDir, info.pwd, true, _window.content.ShowProgress);
                if (code.code != LanZouCode.SUCCESS)
                {
                    Log.Error("Down load Err " + code);
                }
                _window.content.FinishProgress();

            }
            public static async void UpLoad()
            {
                string saveDir = EditorUtility.OpenFilePanel("Save", string.IsNullOrEmpty(_window.rootSavePath) ? "Assets" : _window.rootSavePath, "");
                if (!string.IsNullOrEmpty(saveDir) && File.Exists(saveDir))
                {
                    var code = await lzy.upload_file(saveDir, _window.data.current.id, true, _window.content.ShowProgress);
                    if (code.code == LanZouCode.SUCCESS)
                    {
                        FreshFolder(_window.data.current.id);
                        _window.content.FreshView();
                    }
                    else
                    {
                        Debug.LogError(code);
                    }
                    _window.content.FinishProgress();
                }
            }

            public static void GoUp()
            {
                _window.data.GoUp();
                _window.content.FreshView();
            }
            public static void GoBack()
            {
                _window.data.GoBack();
                _window.content.FreshView();

            }
            public static void GoFront()
            {
                _window.data.GoFront();
                _window.content.FreshView();

            }
            public static void OpenFolder(int id)
            {
                _window.data.SetCurrentFolder(id);
                _window.content.FreshView();
            }
            public static async void RenameFile(long file_id, string filename)
            {
                if (lzy == null)
                {
                    Login();
                    return;
                }
                var code = await lzy.rename_file(file_id, filename);
                if (code == LanZouCode.SUCCESS)
                {
                    FreshFolder(_window.data.current.id);
                    _window.content.FreshView();
                }
                else
                {
                    Debug.LogError(code);
                }

            }
            public static async void RenameFolder(long folder_id, string folder_name)
            {
                if (lzy == null)
                {
                    Login();
                    return;
                }
                var code = await lzy.rename_dir(folder_id, folder_name);
                if (code == LanZouCode.SUCCESS)
                {
                    FreshFolder(_window.data.current.id);
                    _window.content.FreshView();
                }
                else
                {
                    Debug.LogError(code);
                }

            }
            public static async void NewFolder()
            {
                if (lzy == null)
                {
                    Login();
                    return;
                }

                var code = await lzy.mkdir("NewFolder", _window.data.current.id);
                if (code.code == LanZouCode.SUCCESS)
                {
                    FreshFolder(_window.data.current.id);
                    _window.content.FreshView();
                }
                else
                {
                    Debug.LogError(code);
                }
            }

            public static async void Delete(long fid, bool is_file)
            {
                if (lzy == null)
                {
                    Login();
                    return;
                }
                var code = await lzy.delete(fid, is_file);
                if (code == LanZouCode.SUCCESS)
                {
                    FreshFolder(_window.data.current.id);
                    _window.content.FreshView();
                }
                else
                {
                    Debug.LogError(code);
                }
            }

            public static void ChooseSavePath()
            {
                var str = EditorUtility.OpenFolderPanel("Save", "Assets", "");
                if (!string.IsNullOrEmpty(str) && Directory.Exists(str))
                {
                    _window.rootSavePath = str;
                }
            }
        }
        public class DiskData
        {
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
                        has_pwd = c.has_pwd,
                        time = c.time,
                        size = c.size,
                        type = c.type,
                        has_des = c.has_des,
                        downs = c.downs,
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
                        has_pwd = c.has_pwd,
                        desc = c.desc
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
            public Root root;
            public void ReadRoot(List<CloudFolder> folders, List<CloudFile> fs)
            {
                root = new Root() { id = -1, path = "Root" };
                root.Clear();
                root.folders = folders.ConvertAll(data =>
                {
                    FolderData _data = data;
                    _data.pid = -1;
                    _data.path = root.path + "/" + _data.name;
                    return _data;
                });
                root.files = fs.ConvertAll(data =>
                {
                    FileData _data = data;
                    return _data;
                });
                root.allfiles.AddRange(root.files);
                root.allfolders.AddRange(root.folders);
                root.allfolders.Add(root);
                for (int i = 0; i < root.folders.Count; i++)
                {
                    DiskTool.FreshFolder(root.folders[i].id);
                }
                SetCurrentFolder(-1);
            }
            public void FreshFolder(long id, List<CloudFolder> folders, List<CloudFile> fs)
            {

                FolderData _f = root.allfolders.Find(_data => { return _data.id == id; });
                if (_f == null)
                {
                    Debug.LogError("not find dolder " + id);
                    return;
                }
                _f.folders = folders.ConvertAll(data =>
                {
                    FolderData _data = data;
                    _data.pid = _f.id;
                    _data.path = _f.path + "/" + _data.name;
                    return _data;
                });
                _f.files = fs.ConvertAll(data =>
                {
                    FileData _data = data;
                    return _data;
                });

                root.allfiles.AddRange(_f.files);
                root.allfolders.AddRange(_f.folders);
                for (int i = 0; i < _f.folders.Count; i++)
                {
                    DiskTool.FreshFolder(_f.folders[i].id);
                }
            }
            public void SetCurrentFolder(long id)
            {
                FolderData data = null;
                if (id == -1)
                {
                    data = root;
                }
                else
                {
                    data = root.allfolders.Find(_data => { return _data.id == id; });
                }
                if (memory.Count != 0)
                {
                    memory.Clear();
                }
                stack.Push(data);
                current = stack.Peek();
            }
            public FolderData current;

            public bool CanGoUp()
            {
                return !isroot;
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
                if (isroot) return;
                SetCurrentFolder(current.pid);
            }
            public void GoBack()
            {
                if (stack.Count <= 1) return;
                var data = stack.Pop();
                memory.Push(data);
                current = stack.Peek();
            }
            public void GoFront()
            {
                if (memory.Count <= 0) return;
                var data = memory.Pop();
                stack.Push(data);
                current = stack.Peek();
            }
            private Stack<FolderData> memory = new Stack<FolderData>();
            private Stack<FolderData> stack = new Stack<FolderData>();

            public bool isroot { get { return current == root; } }

            public string ylogin { get { return cookie.ylogin; } }
            public string phpdisk_info
            {
                get
                {
                    return cookie.phpdisk_info;
                }

            }
        }
        private DiskData data = new DiskData();
        public static LanzouCookie cookie;
        public string cookiepath = "";
        private ContentPage content;
        private static DiskWindow _window;

        private string rootSavePath;
        private bool autoPath = true;
    }

    partial class DiskWindow : EditorWindow
    {
        private const string key0 = "1321321321346DiskWindow598794146555498";
        private const string key1 = "13213213213465165DiskWindow452123198794146555498";

        private void OnEnable()
        {
            autoPath = EditorPrefs.GetBool(key0, true);
            rootSavePath = EditorPrefs.GetString(key1, "Assets");

            _window = this;
            titleContent = new GUIContent("LanzouDisk");
            if (!string.IsNullOrEmpty(cookiepath))
            {
                cookie = AssetDatabase.LoadAssetAtPath<LanzouCookie>(cookiepath);
            }
            content = new ContentPage();
            DiskTool.Login();
        }
        private void OnDestroy()
        {
            DiskTool.OnQuit();
        }
        private void OnDisable()
        {
            EditorPrefs.SetBool(key0, autoPath);
            EditorPrefs.SetString(key1, rootSavePath);
            cookiepath = AssetDatabase.GetAssetPath(cookie);
        }
        private void OnGUI()
        {
            content.OnGUI();
        }
    }
}
