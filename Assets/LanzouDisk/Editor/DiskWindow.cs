using LanZouCloudAPI;
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
        public enum SelectType
        {
            Home,
            Set,
            Cound,
            List,
        }
        static class Contents
        {
            private static GUIContent folder = new GUIContent(EditorGUIUtility.IconContent("Folder Icon"));
            public static GUIContent name = new GUIContent("Name", "文件名字");
            public static GUIContent size = new GUIContent("Size", "文件大小");
            public static GUIContent password = new GUIContent("*", "有密码？");
            public static GUIContent desc = new GUIContent("...", "文件描述");
            public static GUIContent down = new GUIContent("Downs", "下载次数");
            public static GUIContent newfolder = new GUIContent(EditorGUIUtility.IconContent("Folder Icon")) { tooltip = "新建文件夹" };
            public static GUIContent uploadfile = new GUIContent(EditorGUIUtility.IconContent("d_CreateAddNew@2x")) { tooltip = "上传文件" };
            public static GUIContent goback = new GUIContent(EditorGUIUtility.IconContent("ArrowNavigationLeft")) { tooltip = "返回" };
            public static GUIContent gofront = new GUIContent(EditorGUIUtility.IconContent("ArrowNavigationRight")) { tooltip = "前进" };
            public static GUIContent goup = new GUIContent(EditorGUIUtility.IconContent("d_scrollup")) { tooltip = "返回上一级" };
            public static GUIContent fresh = new GUIContent(EditorGUIUtility.IconContent("d_TreeEditor.Refresh")) { tooltip = "刷新" };
            public static GUIContent set = new GUIContent(EditorGUIUtility.IconContent("d_TerrainInspector.TerrainToolSettings")) { tooltip = "设置" };
            public static GUIContent choosefolder = new GUIContent(EditorGUIUtility.IconContent("Folder Icon")) { tooltip = "选择保存路径" };
            public static GUIContent choose = new GUIContent("Auto", "选择子文件夹");
            public static GUIContent path = new GUIContent("Path:", "当前路径");
            public static GUIContent titleContent = new GUIContent("LanzouDisk");
            public static GUIContent files = new GUIContent("Files");
            public static GUIContent folders = new GUIContent("Folders");
            public static GUIContent upcloud = new GUIContent(EditorGUIUtility.IconContent("d_CloudConnect").image, "上传") { text = "↑" };
            public static GUIContent downcloud = new GUIContent(EditorGUIUtility.IconContent("d_CloudConnect").image) { text = "↓" };
            public static GUIContent dragFiles = new GUIContent(EditorGUIUtility.IconContent("console.infoicon").image, "拖拽文件到此处") { text = "Drag Files Here" };
            public static GUIContent list = new GUIContent(EditorGUIUtility.IconContent("d_align_vertically_center").image, "传输列表") { };
            public static GUIContent home = new GUIContent(EditorGUIUtility.IconContent("d_CanvasGroup Icon").image, "主页") { };
            public static GUIContent[] toolSelect = new GUIContent[] { Contents.home, set, Contents.upcloud, Contents.list, };
            public static GUIContent rootSavePathLabel = new GUIContent("Root Save Path", "本地根路径");
            public static GUIContent downloadOverWriteLabel = new GUIContent("Download File OverWrite", "下载文件重写");
            public static GUIContent uploadOverWriteLabel = new GUIContent("Upload File OverWrite", "上传文件重写");
            public static GUIContent NewFolderDescLabel = new GUIContent("New Folder Desciption", "新建文件夹描述");
            public static GUIContent NewFolderNameLabel = new GUIContent("New Folder Name", "新建文件夹名称");
            public static GUIContent help = EditorGUIUtility.IconContent("_Help");
            public static GUIContent web = EditorGUIUtility.IconContent("d_BuildSettings.Web.Small");

            public static GUIContent GetFolder(string name)
            {
                folder.text = name;
                return folder;
            }
            public static GUIContent GetFreshing()
            {
                return EditorGUIUtility.IconContent($"d_WaitSpin{Math.Round(EditorApplication.timeSinceStartup % 11).ToString("00")}");
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

                    IList<int> list = (IList<int>)userData;
                    if (list == null || list.Count <= 0) return;
                    DownLoadProgressBar.DownLoadData[] datas = new DownLoadProgressBar.DownLoadData[list.Count];
                    for (int i = 0; i < list.Count; i++)
                    {
                        var id = list[i];
                        var data = files.Find(_data => { return _data.id == id; });
                        datas[i] = new DownLoadProgressBar.DownLoadData()
                        {
                            fid = data.id,
                            name = data.name,
                            floder=false
                        };
                    }
                    _window.downLoad.DownLoad(datas);
                }
                private void Description(object userData)
                {
                    int id = (int)userData;
                    tool.ShowDescription(id, false);
                }
                private async void Delete(object userData)
                {

                    IList<int> list = (IList<int>)userData;
                    if (list == null || list.Count <= 0) return;
                    for (int i = 0; i < list.Count; i++)
                    {
                        int id = (int)list[i];
                        await tool.Delete(id, true);
                    }

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
                    menu.AddItem(new GUIContent("Delete"), false, Delete, this.GetSelection());

                    menu.AddItem(new GUIContent("DownLoad"), false, DownLoad, this.GetSelection());

                    var data = files.Find(_data => { return _data.id == id; });
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

                private async void Delete(object userData)
                {
                    IList<int> list = (IList<int>)userData;
                    if (list == null || list.Count <= 0) return;
                    for (int i = 0; i < list.Count; i++)
                    {
                        int id = (int)list[i];
                        await tool.Delete(id, false);
                    }
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
                private void DownLoad(object userData)
                {

                    IList<int> list = (IList<int>)userData;
                    if (list == null || list.Count <= 0) return;
                    DownLoadProgressBar.DownLoadData[] datas = new DownLoadProgressBar.DownLoadData[list.Count];
                    for (int i = 0; i < list.Count; i++)
                    {
                        var id = list[i];
                        var data = folders.Find(_data => { return _data.id == id; });
                        datas[i] = new DownLoadProgressBar.DownLoadData()
                        {
                            fid = data.id,
                            name = data.name,
                            floder=true,
                        };
                    }
                    _window.downLoad.DownLoad(datas);
                }



                protected override void ContextClickedItem(int id)
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Rename"), false, OpenRename, id);
                    menu.AddItem(new GUIContent("Share"), false, Share, id);
                    menu.AddItem(new GUIContent("Delete"), false, Delete, this.GetSelection());
                    menu.AddItem(new GUIContent("DownLoad"), false, DownLoad, this.GetSelection());

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
                        //GUILayout.FlexibleSpace();
                        //if (GUILayout.Button(Contents.uploadfile, EditorStyles.toolbarButton))
                        //{
                        //    tool.UpLoadFile();
                        //}
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
                        if (GUILayout.Button(Contents.newfolder, EditorStyles.toolbarButton, GUILayout.Width(40)))
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
            public async void OnQuit()
            {
                if (lzy != null)
                {
                    await lzy.Logout();
                    lzy = null;
                }
            }
            public async void Login()
            {
                freshing = true;
                lzy = new LanZouCloud();
                lzy.SetLogLevel(LanZouCloud.LogLevel.Info);
                var result = await lzy.Login(cookie.ylogin, cookie.phpdisk_info);
                if (result.code != LanZouCode.SUCCESS) return;
                await FreshFolder(-1);
                SetCurrentFolder(-1);
            }
            private LanZouCloud lzy;
            private void FreshContent()
            {
                _window.content.FreshView();
            }
            public bool freshing { get; private set; }
            public async Task FreshFolder(long id)
            {
                freshing = true;
                var ds = await lzy.GetFolderList(id);
                var fs = await lzy.GetFileList(id);
                _window.data.FreshFolder(id, ds.folders, fs.files);
                if (current!=null &&current.id==id)
                {
                    current = _window.data.FindFolderById(id);
                }
                freshing = false;
            }

            public async void FreshCurrent()
            {
                await FreshFolder(current.id);
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
            public async Task DownLoadFile(long fid, string name, string savePath,IProgress<ProgressInfo> progress = null)
            {
                if (string.IsNullOrEmpty(_window.set.rootSavePath))
                {
                    ChooseSavePath();
                    return;
                }
                string pname = name.Contains(".") ? name.Split('.')[0] : "";
                string ex = name.Contains(".") ? name.Split('.')[1] : "";
                var info = await lzy.GetFileShareInfo(fid);
                var code = await lzy.DownloadFileByUrl(info.url, savePath, info.password, _window.set.downloadOverWrite, progress);
                if (code.code != LanZouCode.SUCCESS)
                {
                    Debug.LogError("Down load Err " + code);
                }
            }

            public async Task UpLoadFile(string file_path, long folder_id = -1, IProgress<ProgressInfo> progress = null)
            {
                var code = await lzy.UploadFile(file_path, folder_id, _window.set.uploadOverWrite, progress);
                if (code.code == LanZouCode.SUCCESS)
                {
                    await FreshFolder(folder_id);
                }
                else
                {
                    Debug.LogError(code);
                }
            }

            public async Task UpLoadFolder(string file_path, long folder_id = -1, IProgress<ProgressInfo> progress = null, bool isRoot = true)
            {
                // 创建文件夹
                var folderName = new DirectoryInfo(file_path).Name;
                var result = await lzy.CreateFolder(folderName, folder_id);
                if (result.code == LanZouCode.SUCCESS)
                {
                    // 只刷新根目录
                    if (isRoot)
                    {
                        await FreshFolder(folder_id);
                    }
                }
                else
                {
                    // 创建失败，不执行之后操作
                    Debug.LogError(result);
                    return;
                }

                // 上传子文件
                foreach (var fi in Directory.GetFiles(file_path))
                {
                    var upload = await lzy.UploadFile(fi, result.id, _window.set.uploadOverWrite, progress);
                    if (upload.code == LanZouCode.SUCCESS)
                    {
                        // 应该不需要刷新吧
                        // await FreshFolder(result.id);
                        // FreshContent();
                    }
                    else
                    {
                        Debug.LogError(upload);
                    }
                }

                // 递归子文件夹（深度遍历）
                foreach (var dir in Directory.GetDirectories(file_path))
                {
                    await UpLoadFolder(dir, result.id, progress, false);
                }
            }
            public async Task DownLoadFolder(long fid, string name, string savePath,IProgress<ProgressInfo> progress = null)
            {
                string path = Path.Combine(savePath, name);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var ds = await lzy.GetFolderList(fid);
                var fs = await lzy.GetFileList(fid);
                foreach (var item in ds.folders)
                {
                    await DownLoadFolder(item.id, item.name, path, progress);
                }
                foreach (var item in fs.files)
                {
                    await DownLoadFile(item.id, item.name, path, progress);
                }
            }
            public void OpenFolder(int id)
            {
                SetCurrentFolder(id);
                //FreshContent();
                FreshCurrent();
            }
            public async void RenameFile(long file_id, string filename)
            {
                var result = await lzy.RenameFile(file_id, filename);
                if (result.code == LanZouCode.SUCCESS)
                {
                    await FreshFolder(current.id);
                }
                else
                {
                    Debug.LogError(result);
                }

            }
            public async void RenameFolder(long folder_id, string folder_name)
            {
                var result = await lzy.RenameFolder(folder_id, folder_name);
                if (result.code == LanZouCode.SUCCESS)
                {
                    await FreshFolder(current.id);
                }
                else
                {
                    Debug.LogError(result);
                }

            }
            public async void NewFolder()
            {
                var code = await lzy.CreateFolder(_window.set.NewFolderName, current.id, _window.set.NewFolderDesc);
                if (code.code == LanZouCode.SUCCESS)
                {
                    await FreshFolder(current.id);
                }
                else
                {
                    Debug.LogError(code);
                }
            }

            public async Task Delete(long fid, bool is_file)
            {
                if (is_file)
                {
                    var result = await lzy.DeleteFile(fid);
                    if (result.code == LanZouCode.SUCCESS)
                    {
                        await FreshFolder(current.id);
                    }
                    else
                    {
                        Debug.LogError(result);
                    }
                }
                else
                {
                    var result = await lzy.DeleteFolder(fid);
                    if (result.code == LanZouCode.SUCCESS)
                    {
                        await FreshFolder(current.id);
                    }
                    else
                    {
                        Debug.LogError(result);
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


            private Stack<long> memory = new Stack<long>();
            private Stack<long> stack = new Stack<long>();
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
                stack.Push(id);
                current = _window.data.FindFolderById(stack.Peek());
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
                var find = _window.data.FindFolderById(stack.Peek());
                if (find == null)
                {
                    stack.Pop();
                    GoBack();
                }
                else
                {
                    current = find;
                    FreshContent();
                }
            }
            public void GoFront()
            {
                if (memory.Count <= 0) return;
                var data = memory.Pop();
                stack.Push(data);
                var find = _window.data.FindFolderById(stack.Peek());
                if (find == null)
                {
                    stack.Pop();
                    GoFront();
                }
                else
                {
                    current = find;
                    FreshContent();
                }
            }

           
        }
        public class DiskData
        {
            private DiskTool tool { get { return _window.tool; } }

            public class FileData
            {
                public long pid;
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
            public void FreshFolder(long id, List<CloudFolder> folders, List<CloudFile> fs)
            {
                if (id == -1)
                {
                    if (root == null)
                    {
                        root = new Root() { id = -1, path = "Root" };
                        root.Clear();
                        root.allfolders.Add(root);
                    }
                }
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
                        _data.pid = data.id;
                        return _data;
                    });
                }
                root.allfiles.RemoveAll((data) => { return data.pid == _f.id; });
                root.allfolders.RemoveAll((data) => { return data.pid == _f.id; });
                root.allfiles.AddRange(_f.files);
                root.allfolders.AddRange(_f.folders);
            }

        }

        public class ProgressBarView : IProgress<ProgressInfo>
        {
            private Queue<ProgressInfo> downs = new Queue<ProgressInfo>();
            protected string txtFormat = "";
            public static ProgressBarView current { get; protected set; }
            public ProgressBarView(string txtFormat)
            {
                this.txtFormat = txtFormat;
            }
            private object downsLock = new object();
            protected string progressTxt = "";
            protected float progress = 0;

            protected virtual void SetProgressTxt(ProgressInfo value)
            {
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


                        break;
                    default:
                        break;
                }

            }
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
                    SetProgressTxt(value);
                }

            }
            public virtual void Update() { }
            public virtual void Report(ProgressInfo value)
            {
                lock (downsLock)
                {
                    downs.Enqueue(value);
                }
            }
        }
        public class DownLoadProgressBar : ProgressBarView
        {
            private DiskTool tool { get { return _window.tool; } }

            public DownLoadProgressBar() : base("DownLoad ( {2} ) - ({0}) \t{1}") { }
            private Queue<DownLoadData> queue = new Queue<DownLoadData>();
            private object _lock = new object();

            public class DownLoadData
            {
                public long fid;
                public string name;
                public bool floder;
            }
            public int count = 0;
            public void DownLoad(DownLoadData[] paths)
            {
                if (paths == null || paths.Length <= 0) return;
                var fid = tool.current.id;
                lock (_lock)
                {
                    foreach (var item in paths)
                    {
                        queue.Enqueue(item);
                    }
                }
            }
            public override void Update()
            {
                LoopUpLoad();
            }
            private async void LoopUpLoad()
            {
                lock (_lock)
                {
                    count = queue.Count;
                }
                if (current != null) return;
                DownLoadData data = null;
                lock (_lock)
                {
                    if (queue.Count > 0)
                    {
                        data = queue.Peek();
                    }
                }
                if (data != null)
                {
                    current = this;
                    if (data.floder)
                        await tool.DownLoadFolder(data.fid, data.name, _window.set.rootSavePath, this);

                    else
                        await tool.DownLoadFile(data.fid, data.name, _window.set.rootSavePath, this);
                    queue.Dequeue();
                    current = null;
                }
            }
            protected override void SetProgressTxt(ProgressInfo value)
            {
                switch (value.state)
                {
                    case ProgressState.Start:
                    case ProgressState.Ready:
                        progress = 0f;
                        progressTxt = string.Format(txtFormat, progress.ToString("0.00 %"), value.fileName, count);
                        break;
                    case ProgressState.Progressing:
                        progress = value.current / (float)value.total;
                        progressTxt = string.Format(txtFormat, progress.ToString("0.00 %"), value.fileName, count);
                        break;
                    case ProgressState.Finish:
                        break;
                    default:
                        break;
                }

            }

            public void OnListGUI()
            {
                GUILayout.Label($"Download List Count ({count})");
                foreach (var item in queue)
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(20);
                        GUILayout.Label(item.name, item == queue.Peek() ? (GUIStyle)"SelectionRect" : "label");
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }
        public class UpLoadProgressBar : ProgressBarView
        {
            private DiskTool tool { get { return _window.tool; } }
            private Queue<UpLoadData> queue = new Queue<UpLoadData>();
            private object _lock = new object();
            public UpLoadProgressBar() : base("UpLoad ( {2} ) - ({0}) \t{1}") { }
            public int count = 0;

            public override void Update()
            {
                LoopUpLoad();
            }
            private class UpLoadData
            {
                public long fid;
                public string path;
            }
            public void UpLoad(string[] paths)
            {
                if (paths == null || paths.Length <= 0) return;
                var fid = tool.current.id;
                paths = paths.Distinct().ToArray();
                lock (_lock)
                {
                    foreach (var item in paths)
                    {
                        queue.Enqueue(new UpLoadData()
                        {
                            path = item,
                            fid = fid,
                        });
                    }
                }

            }
            private async void LoopUpLoad()
            {
                lock (_lock)
                {
                    count = queue.Count;
                }
                if (current != null) return;

                UpLoadData data = null;
                lock (_lock)
                {
                    if (queue.Count > 0)
                    {
                        data = queue.Peek();
                    }
                }
                if (data != null)
                {
                    current = this;
                    if (File.Exists(data.path))
                        await tool.UpLoadFile(data.path, data.fid, this);
                    else if (Directory.Exists(data.path))
                        await tool.UpLoadFolder(data.path, data.fid, this);
                    else
                        Debug.LogError("Path not found: " + data.path);
                    queue.Dequeue();
                    current = null;
                }
            }
            protected override void SetProgressTxt(ProgressInfo value)
            {
                switch (value.state)
                {
                    case ProgressState.Start:
                    case ProgressState.Ready:
                        progress = 0f;
                        progressTxt = string.Format(txtFormat, progress.ToString("0.00 %"), value.fileName, count);
                        break;
                    case ProgressState.Progressing:
                        progress = value.current / (float)value.total;
                        progressTxt = string.Format(txtFormat, progress.ToString("0.00 %"), value.fileName, count);
                        break;
                    case ProgressState.Finish:
                        break;
                    default:
                        break;
                }

            }

            public void OnListGUI()
            {
                GUILayout.Label($"Upload List Count ({count})");
                foreach (var item in queue)
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(20);

                        GUILayout.Label(item.path, item == queue.Peek() ? (GUIStyle)"SelectionRect" : "label");
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }


        [System.Serializable]
        private class Setting
        {
            public SelectType select;
            public string rootSavePath = "Asset";
            public bool uploadOverWrite = false;
            public bool downloadOverWrite = true;
            public string NewFolderName = "NewFolder";
            public string NewFolderDesc;
        }

        private Setting set = new Setting();
        private DiskData data;
        private DiskTool tool;
        private string cookiepath = "";
        private ContentPage content;

        public static LanzouCookie cookie;
        private static DiskWindow _window;
        private DownLoadProgressBar downLoad = new DownLoadProgressBar();
        private UpLoadProgressBar upLoad = new UpLoadProgressBar();

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
            if (set.select != SelectType.Home)
            {
                local.height -= 200;
            }
            var rs = local.HorizontalSplit(20);
            var rs1 = rs[1].HorizontalSplit(rs[1].height - 20);
            ToolBar(rs[0]);
            content.OnGUI(rs1[0]);
            if (ProgressBarView.current != null)
            {
                ProgressBarView.current.OnGUI(rs1[1].Zoom(AnchorType.MiddleCenter, -6));
            }
            if (set.select != SelectType.Home)
            {
                var rect = new Rect(0, position.height - 200, position.width, 200);
                if (set.select == SelectType.List)
                {
                    ShowList(rect);

                }
                else if (set.select == SelectType.Cound)
                {
                    UpLoad(rect);
                }
                else if (set.select == SelectType.Set)
                {
                    SettingGUI(rect);
                }
            }
            upLoad.Update();
            downLoad.Update();
            if (tool.freshing || ProgressBarView.current != null)
            {
                Repaint();
            }
        }

        private void SettingGUI(Rect rect)
        {
            GUI.Box(rect, "");
            GUILayout.BeginArea(rect.Zoom(AnchorType.MiddleCenter, -20));
            {
                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(Contents.rootSavePathLabel, new GUIContent(set.rootSavePath));
                    if (GUILayout.Button(Contents.choosefolder, EditorStyles.toolbarButton, GUILayout.Width(30)))
                    {
                        tool.ChooseSavePath();
                    }
                }
                GUILayout.EndHorizontal();
                set.downloadOverWrite = EditorGUILayout.Toggle(Contents.downloadOverWriteLabel, set.downloadOverWrite);
                set.uploadOverWrite = EditorGUILayout.Toggle(Contents.uploadOverWriteLabel, set.uploadOverWrite);
                set.NewFolderName = EditorGUILayout.TextField(Contents.NewFolderNameLabel, set.NewFolderName);
                GUILayout.Label(Contents.NewFolderDescLabel);
                set.NewFolderDesc = EditorGUILayout.TextArea(set.NewFolderDesc, GUILayout.MinHeight(50));
            }
            GUILayout.EndArea();
        }

        private void ShowList(Rect rect)
        {
            GUI.Box(rect, "");
            GUILayout.BeginArea(rect.Zoom(AnchorType.MiddleCenter, -20));
            {
                upLoad.OnListGUI();
                downLoad.OnListGUI();
            }
            GUILayout.EndArea();
        }

        private void UpLoad(Rect rect)
        {
            GUIStyle dragFileStyle = new GUIStyle("helpbox")
            {
                fontSize = 30,
                alignment = TextAnchor.MiddleCenter
            };
            GUI.Box(rect, "");
            GUI.Label(rect.Zoom(AnchorType.MiddleCenter, -10), Contents.dragFiles, dragFileStyle);
            var info = DragAndDropTool.Drag(Event.current, rect);
            if (info.enterArera && info.compelete && Event.current.type == EventType.Used)
            {
                if (info.paths != null && info.paths.Length > 0)
                {
                    upLoad.UpLoad(info.paths);
                }
            }
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

                            if (GUILayout.Button(Contents.goback, EditorStyles.toolbarButton,GUILayout.Width(30)))
                            {
                                tool.GoBack();
                            }
                        }
                        using (new EditorGUI.DisabledGroupScope(!tool.CanGoFront()))
                        {

                            if (GUILayout.Button(Contents.gofront, EditorStyles.toolbarButton, GUILayout.Width(30)))
                            {
                                tool.GoFront();
                            }
                        }
                        using (new EditorGUI.DisabledGroupScope(!tool.CanGoUp()))
                        {

                            if (GUILayout.Button(Contents.goup, EditorStyles.toolbarButton, GUILayout.Width(30)))
                            {
                                tool.GoUp();
                            }
                        }
                        if (GUILayout.Button(Contents.fresh, EditorStyles.toolbarButton, GUILayout.Width(30)))
                        {
                            tool.FreshCurrent();
                        }
                        if (GUILayout.Button(Contents.web, EditorStyles.toolbarButton, GUILayout.Width(30)))
                        {
                            Application.OpenURL("https://up.woozooo.com/");
                        }
                        if (GUILayout.Button(Contents.help, EditorStyles.toolbarButton, GUILayout.Width(30)))
                        {
                            Application.OpenURL("https://bbs.zsxwz.com/thread-2505.html");
                        }
                        GUILayout.Space(10);
                        using (new EditorGUI.DisabledGroupScope(true))
                        {
                            GUILayout.Label(Contents.path, GUILayout.Width(30));
                            GUILayout.TextField(tool.current == null ? "" : tool.current.path, GUILayout.ExpandWidth(true));
                            if (tool.freshing)
                            {
                                GUILayout.Label(Contents.GetFreshing(),GUILayout.Width(30));
                            }
                            else
                            {
                                GUILayout.Space(30);
                            }
                        }
                        //GUILayout.FlexibleSpace();
                        set.select = (SelectType)GUILayout.Toolbar((int)set.select, Contents.toolSelect, EditorStyles.toolbarButton, GUILayout.Width(200));
                    }
                    GUILayout.EndHorizontal();

                }
                GUILayout.EndArea();
            }
        }
    }
}
