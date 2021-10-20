using LanZouCloudAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace LanZouWindow
{
    partial class DiskWindow
    {
        public class DiskTool
        {
            public class MoveFilePop : PopupWindowContent
            {
                public static void Show(Rect rect,DiskData data,IList<int> list,DiskTool tool)
                {
                    MoveFilePop pop= new MoveFilePop();
                    pop.tree = new Tree(data, list,tool);
                    PopupWindow.Show(rect,pop);
                }
                private Tree tree;
                private class Tree : TreeView
                {
                    private DiskData data;
                    private readonly IList<int> list;
                    private readonly DiskTool tool;

                    public Tree(DiskData data, IList<int> list, DiskTool tool) : base(new TreeViewState())
                    {
                        this.data = data;
                        this.list = list;
                        this.tool = tool;
                        Reload();
                        Fresh();
                    }
                    private async void Fresh()
                    {
                        await tool.FreshFolderList();
                        Reload();
                    }
                    private async void Move(long fid)
                    {
                        EditorWindow.focusedWindow.Close();
                        foreach (var item in list)
                        {
                            await tool.DoMoveFile(item, fid, data.IsFile(item));
                        }
                    }

                    protected override TreeViewItem BuildRoot()
                    {
                        return new TreeViewItem { id = -2, depth = -1, displayName = "Root" };
                    }
                    protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
                    {
                        List<TreeViewItem> rows = new List<TreeViewItem>();
                        TreeViewItem view = new TreeViewItem()
                        {
                            id = -1,
                            depth = 1,
                            displayName = data.path
                        };
                        root.AddChild(view);
                        rows.Add(view);
                        LoopBuild(data.id, view, rows);
                        return rows;
                    }
                    private void LoopBuild(long id,TreeViewItem parrent, List<TreeViewItem> rows)
                    {
                        var datas = data.GetSubFolders(id);
                        foreach (var item in datas)
                        {
                            TreeViewItem view = new TreeViewItem()
                            {
                                id = (int)item.id,
                                depth = parrent.depth + 1,
                                displayName = item.name
                            };
                            parrent.AddChild(view);
                            rows.Add(view);
                            LoopBuild(view.id, view, rows);
                        }
                    }

                    protected override void RowGUI(RowGUIArgs args)
                    {
                        Rect rect = args.rowRect;
                        rect.x += args.item.depth * 10;
                        rect.width -= args.item.depth * 10;
                        GUI.Label(rect,Contents.GetFolder(args.item.displayName));
                    }
                    protected override void DoubleClickedItem(int id)
                    {
                        Move(id);
                    }
                    public override void OnGUI(Rect rect)
                    {
                        base.OnGUI(rect.Zoom(AnchorType.MiddleCenter,-2));
                    }
                }
                public override void OnGUI(Rect rect)
                {
                    GUI.Box(rect, "", EditorStyles.helpBox);
                    var rs = rect.HorizontalSplit(20);
                    GUI.Label(rs[0], Contents.clickMove, EditorStyles.toolbarButton);
                    tree.OnGUI(rs[1]);
                }
            }
            public class Data
            {
                public long pid;
                public long id;
                public string name;
                public bool has_pwd;
                public bool has_des;
                public string path;
            }
            public class FileData : Data
            {
                public string time;
                public string size;
                public string type;
                public int downs;
            }
            public class FolderData : Data
            {
                public string desc;               
            }
            public class DiskData: FolderData
            {
                private List<FolderData> allfolders = new List<FolderData>();
                private List<FileData> allfiles = new List<FileData>();
                public DiskData()
                {
                    id = -1;
                    path = "Root";
                    allfolders.Add(this);
                }

                public bool IsRootFolder(FolderData data)
                {
                    return data == this;
                }
                public bool IsFile(long id)
                {
                    return FindFileById(id)!=null;
                }
                public FolderData FindFolderById(long id)
                {
                    if (id == -1) return this;
                    return allfolders.Find(_data => { return _data.id == id; });
                }
                public FileData FindFileById(long id)
                {
                    return allfiles.Find(_data => { return _data.id == id; });
                }

               
                public List<FolderData> GetSubFolders(long folderid)
                {
                    return allfolders.FindAll(_data => { return _data.pid == folderid; });
                }
                public List<FileData> GetSubFliles(long folderid)
                {
                    return allfiles.FindAll(_data => { return _data.pid == folderid; });
                }
                public void FreshFolder(long id, List<CloudFolder> folders, List<CloudFile> fs)
                {
                    FolderData parent = FindFolderById(id);
                    if (parent == null) return;
                    if (folders != null)
                    {
                        foreach (CloudFolder cloud in folders)
                        {
                            long cid = cloud.id;
                            FolderData fdata = FindFolderById(cid);
                            if (fdata==null)
                            {
                                fdata = new FolderData();
                                allfolders.Add(fdata);
                            }

                            fdata.pid = parent.id;
                            fdata.id = cloud.id;
                            fdata.name = cloud.name;
                            fdata.has_pwd = cloud.hasPassword;
                            fdata.has_des = !string.IsNullOrEmpty(cloud.description);
                            fdata.desc = cloud.description;
                            fdata.path = parent.path + "/" + parent.name;
                        }
                    }
                    if (fs != null)
                    {
                        foreach (CloudFile cloud in fs)
                        {
                            long cid = cloud.id;
                            FileData fdata = FindFileById(cid);
                            if (fdata==null)
                            {
                                fdata = new FileData();
                                allfiles.Add(fdata);
                            }
                            fdata.pid = parent.id;
                            fdata.id = cloud.id;
                            fdata.name = cloud.name;
                            fdata.has_pwd = cloud.hasPassword;
                            fdata.has_des = cloud.hasPassword;
                            fdata.time = cloud.time;
                            fdata.size = cloud.size;
                            fdata.type = cloud.type;
                            fdata.downs = cloud.downloads;
                            fdata.path = parent.path + "/" + parent.name;
                        }
                    }
                }

                public void DeleteFile(long fid)
                {
                    allfiles.RemoveAll(data => { return data.id == fid; });
                }
                public void DeleteFolder(long fid)
                {
                    allfolders.RemoveAll(data => { return data.id == fid; });
                }
            }


            public List<FolderData> GetSubFolders(long folderid)
            {
               return data.GetSubFolders(folderid);

            }
            public List<FileData> GetSubFliles(long folderid)
            {
                return data.GetSubFliles(folderid);
            }

            public DiskTool(DiskSetting set, ProgressBarView downLoad, ProgressBarView upLoad)
            {
                this.downLoad = downLoad;
                this.upLoad = upLoad;
                this.set = set;
                data = new DiskData();
            }
            private ProgressBarView downLoad;
            private ProgressBarView upLoad;
            private DiskData data;
            private DiskSetting set;
            private LanZouCloud lzy;
            public bool freshing { get; private set; }
            public event Action<FolderData> FreshView;

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
            private void FreshContent()
            {
                FreshView?.Invoke(current);
            }
            public async Task FreshFolder(long id)
            {
                freshing = true;
                var ds = await lzy.GetFolderList(id);
                var fs = await lzy.GetFileList(id);
                data.FreshFolder(id, ds.folders, fs.files);
                if (current!=null &&current.id==id)
                {
                    current = data.FindFolderById(id);
                }
                freshing = false;
            }
            public async Task FreshFolderList(long id=-1)
            {
                freshing = true;
                var ds = await lzy.GetFolderList(id);
                if (ds.folders!=null)
                {
                    data.FreshFolder(id, ds.folders, null);
                    foreach (var item in ds.folders)
                    {
                        await FreshFolderList(item.id);
                    }
                }
                freshing = false;
            }

            public async Task FreshCurrent()
            {
                await FreshFolder(current.id);
            }
            private void ShowNotification(GUIContent content)
            {
               EditorWindow.focusedWindow.ShowNotification(content);
            }
 
            public async Task DoMoveFile(long file, long folder,bool isfile)
            {
                if (isfile)
                {
                    var result = await lzy.MoveFile(file, folder);
                    if (result.code == LanZouCode.SUCCESS)
                    {
                        data.DeleteFile(file);
                       await FreshFolder(folder);
                        await FreshCurrent();
                    }
                }
                else
                {
                    var result = await lzy.MoveFolder(file, folder);
                    if (result.code == LanZouCode.SUCCESS)
                    {
                        data.DeleteFolder(file);
                        await FreshFolder(folder);
                        await FreshCurrent();

                    }
                }
            }
            public void MoveFile(Rect rect, IList<int> list)
            {
                MoveFilePop.Show(rect, data,list,this);
               
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
                ShowNotification(new GUIContent("提取方式已复制到粘贴板"));
            }
            public async void ShowDescription(long fid, bool is_file = true)
            {
                if (is_file)
                {
                    var info = await lzy.GetFileShareInfo(fid);
                    ShowNotification(new GUIContent(info.description));
                }
                else
                {
                    var info = await lzy.GetFolderShareInfo(fid);
                    ShowNotification(new GUIContent(info.description));
                }

            }
            public async Task Delete(long fid, bool is_file,bool rootFolder=true)
            {
                if (is_file)
                {
                    var result = await lzy.DeleteFile(fid);
                    if (result.code == LanZouCode.SUCCESS)
                    {
                        this.data.DeleteFile(fid);
                        await FreshFolder(current.id);
                    }
                    else
                    {
                        Debug.LogError(result);
                    }
                }
                else
                {
                    var child = await lzy.GetFolderList(fid);
                    if (child.folders!=null && child.folders.Count>0)
                    {
                        foreach (var item in child.folders)
                        {
                            await Delete(item.id, false,false);
                        }
                    }
                    var result = await lzy.DeleteFolder(fid);
                    if (result.code == LanZouCode.SUCCESS)
                    {
                        this.data.DeleteFolder(fid);
                        if (rootFolder)
                        {
                            await FreshCurrent();
                        }
                    }
                    else
                    {
                        Debug.LogError(result);
                    }
                }

            }
            public async void Rename(long file_id, string filename,bool isfile)
            {
                if (isfile)
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
                else
                {
                    var result = await lzy.RenameFolder(file_id, filename);
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
            public void OpenFolder(int id)
            {
                SetCurrentFolder(id);
                //FreshContent();
                FreshCurrent();
            }
            public async void NewFolder()
            {
                var code = await lzy.CreateFolder(set.NewFolderName, current.id, set.NewFolderDesc);
                if (code.code == LanZouCode.SUCCESS)
                {
                    await FreshFolder(current.id);
                }
                else
                {
                    Debug.LogError(code);
                }
            }
            public void ChooseSavePath()
            {
                var str = EditorUtility.OpenFolderPanel("Save", "Assets", "");
                if (!string.IsNullOrEmpty(str) && Directory.Exists(str))
                {
                    set.rootSavePath = str;
                }
            }


            private Stack<long> memory = new Stack<long>();
            private Stack<long> stack = new Stack<long>();
            private FolderData _current;
            private FolderData current
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
                FolderData data = this.data.FindFolderById(id);
                if (memory.Count != 0)
                {
                    memory.Clear();
                }
                stack.Push(id);
                current = this.data.FindFolderById(stack.Peek());
            }
            public bool CanGoUp()
            {
                return !data.IsRootFolder(current);
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
                if (data.IsRootFolder(current)) return;
                SetCurrentFolder(current.pid);
                FreshContent();
            }
            public void GoBack()
            {
                if (stack.Count <= 1) return;
                var data = stack.Pop();
                memory.Push(data);
                var find = this.data.FindFolderById(stack.Peek());
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
                var find = this.data.FindFolderById(stack.Peek());
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

            public string GetCurrentPath()
            {
                return current == null ? "" : current.path;
            }


            private async Task DownLoadFile(long fid, string name, string savePath, IProgress<ProgressInfo> progress = null)
            {
                if (string.IsNullOrEmpty(set.rootSavePath))
                {
                    ChooseSavePath();
                    return;
                }
                string pname = name.Contains(".") ? name.Split('.')[0] : "";
                string ex = name.Contains(".") ? name.Split('.')[1] : "";
                var info = await lzy.GetFileShareInfo(fid);
                var code = await lzy.DownloadFileByUrl(info.url, savePath, info.password, set.downloadOverWrite, progress);
                if (code.code != LanZouCode.SUCCESS)
                {
                    Debug.LogError("Down load Err " + code);
                }
            }
            private async Task UpLoadFile(string file_path, long folder_id = -1, IProgress<ProgressInfo> progress = null)
            {
                var code = await lzy.UploadFile(file_path, folder_id, set.uploadOverWrite, progress);
                if (code.code == LanZouCode.SUCCESS)
                {
                    await FreshFolder(folder_id);
                }
                else
                {
                    Debug.LogError(code);
                }
            }
            private async Task UpLoadFolder(string file_path, long folder_id = -1, IProgress<ProgressInfo> progress = null, bool isRoot = true)
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
                    var upload = await lzy.UploadFile(fi, result.id, set.uploadOverWrite, progress);
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
            private async Task DownLoadFolder(long fid, string name, string savePath, IProgress<ProgressInfo> progress = null)
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


            private Queue<DownLoadData> downLoadqueue = new Queue<DownLoadData>();
            private object _lock_download = new object();
            private Queue<UpLoadData> queue_upload = new Queue<UpLoadData>();
            private object _lockupload = new object();
            private int downloadCount = 0;
            private int uploadCount = 0;

            public class UpLoadData
            {
                public long fid;
                public string path;
            }
            public class DownLoadData
            {
                public long fid;
                public string name;
                public bool floder;
            }
            public void DownLoad(DownLoadData[] paths)
            {
                if (paths == null || paths.Length <= 0) return;
                lock (_lock_download)
                {
                    foreach (var item in paths)
                    {
                        downLoadqueue.Enqueue(item);
                    }
                }
            }
            public void UpLoad(string[] paths)
            {
                if (paths == null || paths.Length <= 0) return;
                paths = paths.Distinct().ToArray();
                lock (_lockupload)
                {
                    foreach (var item in paths)
                    {
                        queue_upload.Enqueue(new UpLoadData()
                        {
                            path = item,
                            fid = current.id,
                        });
                    }
                }

            }
            private async void LoopDownLoad()
            {
                if (ProgressBarView.current != null) return;
                DownLoadData data = null;
                lock (_lock_download)
                {
                    if (downLoadqueue.Count > 0)
                    {
                        data = downLoadqueue.Peek();
                    }
                }
                if (data != null)
                {
                    ProgressBarView.current = downLoad;
                    if (data.floder)
                        await DownLoadFolder(data.fid, data.name, set.rootSavePath, ProgressBarView.current);
                    else
                        await DownLoadFile(data.fid, data.name, set.rootSavePath, ProgressBarView.current);
                    downLoadqueue.Dequeue();
                    ProgressBarView.current = null;
                }
            }
            private async void LoopUpLoad()
            {
                if (ProgressBarView.current != null) return;

                UpLoadData data = null;
                lock (_lockupload)
                {
                    if (queue_upload.Count > 0)
                    {
                        data = queue_upload.Peek();
                    }
                }
                if (data != null)
                {
                    ProgressBarView.current = upLoad;
                    if (File.Exists(data.path))
                        await UpLoadFile(data.path, data.fid, ProgressBarView.current);
                    else if (Directory.Exists(data.path))
                        await UpLoadFolder(data.path, data.fid, ProgressBarView.current);
                    else
                        Debug.LogError("Path not found: " + data.path);
                    queue_upload.Dequeue();
                    ProgressBarView.current = null;
                }
            }
            public void Update()
            {
                lock (_lock_download)
                {
                    downloadCount = downLoadqueue.Count;
                }
                lock (_lockupload)
                {
                    uploadCount = queue_upload.Count;
                }
                LoopDownLoad();
                LoopUpLoad();
            }

            public void OnListGUI()
            {
                if (EditorGUILayout.DropdownButton(Contents.GetDownLoadListLabel(downloadCount), FocusType.Passive))
                {
                    set.showDownLoadList = !set.showDownLoadList;
                };

                if (set.showDownLoadList)
                {
                    foreach (var item in downLoadqueue)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(20);
                            GUILayout.Label(item.name, item == downLoadqueue.Peek() ? (GUIStyle)"SelectionRect" : EditorStyles.label, GUILayout.ExpandWidth(true));
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.Space(5);
                if (EditorGUILayout.DropdownButton(Contents.GetUpLoadListLabel(uploadCount), FocusType.Passive))
                {
                    set.showUpLoadList = !set.showUpLoadList;
                };
                if (set.showUpLoadList)
                {
                    foreach (var item in queue_upload)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(20);
                            GUILayout.Label(item.path, item == queue_upload.Peek() ? (GUIStyle)"SelectionRect" : EditorStyles.label, GUILayout.ExpandWidth(true));
                        }
                        GUILayout.EndHorizontal();
                    }
                }


            }
        }

    }
}
