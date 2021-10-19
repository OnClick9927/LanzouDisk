using LanZouCloudAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace LanZouWindow
{
    partial class DiskWindow
    {
        public class DiskTool
        {
            public class Data
            {
                public long pid;
                public long id;
                public string name;
                public bool has_pwd;
                public bool has_des;
            }
            public class FileData : Data
            {
                public string time;
                public string size;
                public string type;
                public int downs;
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
            public class FolderData : Data
            {
                public string path;
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
                        has_des = !string.IsNullOrEmpty(c.description),
                        desc = c.description
                    };
                }
            }
            public class RootFolderData : FolderData
            {
                public List<FolderData> allfolders = new List<FolderData>();
                public List<FileData> allfiles = new List<FileData>();
                public void Clear()
                {
                    allfolders.Clear();
                    allfiles.Clear();
                }
            }
            public class DiskData
            {
                private RootFolderData root;
                public bool IsRootFolder(FolderData data)
                {
                    return data == root;
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
                            root = new RootFolderData() { id = -1, path = "Root" };
                            root.Clear();
                            root.allfolders.Add(root);
                        }
                    }
                    FolderData _f = root.allfolders.Find(_data => { return _data.id == id; });
                    if (_f == null) return;
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
                    LoopRemoveUseLessData(_f.id);
                    root.allfiles.AddRange(_f.files);
                    root.allfolders.AddRange(_f.folders);
                }
                private void LoopRemoveUseLessData(long id)
                {
                    var files = root.allfiles.FindAll((data) => { return data.pid == id; });
                    var folders = root.allfolders.FindAll((data) => { return data.pid == id; });
                    foreach (var item in folders)
                    {
                        LoopRemoveUseLessData(item.id);
                        root.allfolders.Remove(item);
                    }
                    foreach (var item in files)
                    {
                        root.allfiles.Remove(item);
                    }
                }
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

            public async Task FreshCurrent()
            {
                await FreshFolder(current.id);
            }
            private void ShowNotification(GUIContent content)
            {
               EditorWindow.focusedWindow.ShowNotification(content);
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
