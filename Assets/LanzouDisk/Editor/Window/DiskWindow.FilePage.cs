using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using static LanZouWindow.DiskWindow.DiskTool;

namespace LanZouWindow
{
    partial class DiskWindow
    {
        class FilePage : TreeView
        {
            private int fileCount;
            private int folderCount;
            private List<Data> datas = new List<Data>();
            private DiskTool tool;
            private FolderData current;

            public FilePage(DiskTool tool) : base(new TreeViewState())
            {
                this.tool = tool;
                this.baseIndent = 20;
                this.showAlternatingRowBackgrounds = true;
                this.multiColumnHeader = new MultiColumnHeader(new MultiColumnHeaderState(new MultiColumnHeaderState.Column[] {

                        new MultiColumnHeaderState.Column()
                        {
                            headerContent=Contents.name,
                            minWidth = 400,
                            width = 400
                        },
                        new MultiColumnHeaderState.Column()
                        {
                            headerContent=Contents.size,
                            width=120,
                            autoResize=false,
                            maxWidth=120,
                        },

                           new MultiColumnHeaderState.Column()
                        {

                            headerContent=Contents.down,
                            width=100,
                            maxWidth=100,
                            autoResize=false
                        },
                        new MultiColumnHeaderState.Column()
                        {
                            headerContent=Contents.password,
                            width=40,
                            maxWidth=40,
                            autoResize=false
                        },
                         new MultiColumnHeaderState.Column()
                        {
                            headerContent=Contents.desc,
                            width=40,
                            maxWidth=40,
                            autoResize=false
                        },

                    }));
                tool.FreshView += FreshView;

                Reload();
                FreshView(current);
                this.multiColumnHeader.ResizeToFit();
            }
            private void FreshView(FolderData current)
            {
                if (current != null)
                {
                    ReadFiles(current.files, current.folders);
                }
                else
                {
                    ReadFiles(new List<FileData>(), new List<FolderData>());
                }
            }
            public override void OnGUI(Rect rect)
            {
                var rs = rect.HorizontalSplit(20);
                GUILayout.BeginArea(rs[0], EditorStyles.toolbar);
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label(Contents.folders);
                        GUILayout.Label(folderCount.ToString());
                        GUILayout.Label(Contents.files);
                        GUILayout.Label(fileCount.ToString());

                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(Contents.newfolder, EditorStyles.toolbarButton, GUILayout.Width(40)))
                        {
                            tool.NewFolder();
                        }
                    }
                    GUILayout.EndHorizontal();

                }
                GUILayout.EndArea();
                base.OnGUI(rs[1]);
            }


            private void ReadFiles(List<FileData> files, List<FolderData> folders)
            {
                fileCount = files == null ? 0 : files.Count;
                folderCount = folders == null ? 0 : folders.Count;

                datas.Clear();
                datas.AddRange(folders);
                datas.AddRange(files);
                Reload();
            }
            private bool IsFile(Data data)
            {
                return data is FileData;
            }
            private bool IsFolder(Data data)
            {
                return data is FolderData;
            }
            private Data FindData(long id)
            {
                return this.datas.Find(_data => { return _data.id == id; });
            }
            private void DownLoad(object userData)
            {
                IList<int> list = (IList<int>)userData;
                if (list == null || list.Count <= 0) return;
                DownLoadData[] datas = new DownLoadData[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    var id = list[i];
                    var data = FindData(id);
                    datas[i] = new DownLoadData()
                    {
                        fid = data.id,
                        name = data.name,
                        floder = IsFolder(data)
                    };
                }
                tool.DownLoad(datas);
            }
            private void Description(object userData)
            {
                int id = (int)userData;
                var data = FindData(id);
                tool.ShowDescription(id, IsFile(data));
            }
            private async void Delete(object userData)
            {
                IList<int> list = (IList<int>)userData;
                if (list == null || list.Count <= 0) return;
                for (int i = 0; i < list.Count; i++)
                {
                    int id = (int)list[i];
                    var data = FindData(id);
                    await tool.Delete(id, IsFile(data));
                }

            }
            private void Share(object userData)
            {
                int id = (int)userData;
                var data = FindData(id);
                tool.Share((int)userData, IsFile(data));
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
                foreach (var item in datas)
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
                var data = FindData(id);
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Rename"), false, OpenRename, id);
                menu.AddItem(new GUIContent("Share"), false, Share, id);
                menu.AddItem(new GUIContent("Delete"), false, Delete, this.GetSelection());
                menu.AddItem(new GUIContent("DownLoad"), false, DownLoad, this.GetSelection());
                if (data.has_des)
                {
                    menu.AddItem(new GUIContent("Description"), false, Description, id);
                }
                menu.ShowAsContext();
            }
            protected override void RowGUI(RowGUIArgs args)
            {
                var data = FindData(args.item.id);
                for (int i = 0; i < args.GetNumVisibleColumns(); i++)
                {
                    Rect rect = args.GetCellRect(i);
                    switch (i)
                    {
                        case 0:
                            if (IsFolder(data))
                            {
                                GUI.Label(rect, Contents.GetFolder(data.name));
                            }
                            else
                            {
                                GUI.Label(rect, data.name);
                            }
                            break;
                        case 1:
                            if (IsFile(data))
                            {
                                GUI.Label(rect, (data as FileData).size);
                            }
                            break;
                        case 2:
                            if (IsFile(data))
                            {
                                GUI.Label(rect, (data as FileData).downs.ToString());
                            }
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
                    int id = (int)args.itemID;
                    var data = FindData(id);
                    tool.Rename(args.itemID, args.newName, IsFile(data));
                }
            }
            protected override bool CanRename(TreeViewItem item)
            {
                return true;
            }
            protected override void DoubleClickedItem(int id)
            {
                var data = FindData(id);
                if (IsFolder(data))
                {
                    tool.OpenFolder(id);
                }
            }
        }

    }
}
