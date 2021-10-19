using UnityEditor;
using UnityEngine;

namespace LanZouWindow
{
    partial class DiskWindow : EditorWindow
    {
        public static LanzouCookie cookie;
        private const string key0 = "1321321321346DiskWindow5987941465554987879";
        private string cookiepath = "";
        private DiskSetting set;
        private DiskTool tool;
        private FilePage filepage;
        private ProgressBarView downLoad;
        private ProgressBarView upLoad;

        private void OnEnable()
        {
            set = new DiskSetting();
            titleContent = Contents.titleContent;
            if (EditorPrefs.HasKey(key0)) set = JsonUtility.FromJson<DiskSetting>(EditorPrefs.GetString(key0));
            if (!string.IsNullOrEmpty(cookiepath)) cookie = AssetDatabase.LoadAssetAtPath<LanzouCookie>(cookiepath);
            downLoad = new ProgressBarView("DownLoad ({0}) \t{1}");
            upLoad = new ProgressBarView("UpLoad ({0}) \t{1}");
            tool = new DiskTool(set,downLoad,upLoad);
            filepage = new FilePage(tool);
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
            if (set.select != ToolType.Home)
            {
                local.height -= 200;
            }
            var rs = local.HorizontalSplit(22);
            var rs1 = rs[1].HorizontalSplit(rs[1].height - 20,2,false);
            ToolBar(rs[0]);
            filepage.OnGUI(rs1[0]);
            if (ProgressBarView.current != null)
            {
                ProgressBarView.current.OnGUI(rs1[1].Zoom(AnchorType.MiddleCenter, -6));
            }
            if (set.select != ToolType.Home)
            {
                var rect = new Rect(0, position.height - 200, position.width, 200);
                GUI.Box(rect, "",EditorStyles.helpBox);
                if (set.select == ToolType.List)
                {
                    ShowList(rect);
                }
                else if (set.select == ToolType.Cound)
                {
                    UpLoad(rect);
                }
                else if (set.select == ToolType.Set)
                {
                    SettingGUI(rect);
                }
            }
            tool.Update();
        }
        private void OnInspectorUpdate()
        {
            Repaint();
        }
        private void OnFocus()
        {
            Repaint();
        }
        private void SettingGUI(Rect rect)
        {
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

        private Vector2 listscroll;
        private void ShowList(Rect rect)
        {
            GUILayout.BeginArea(rect.Zoom(AnchorType.MiddleCenter, -10));
            {
                listscroll = GUILayout.BeginScrollView(listscroll);
                {
                    tool.OnListGUI();
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();
        }

        private void UpLoad(Rect rect)
        {
            GUIStyle dragFileStyle = new GUIStyle(EditorStyles.helpBox)
            {
                fontSize = 30,
                alignment = TextAnchor.MiddleCenter
            };
            GUI.Label(rect.Zoom(AnchorType.MiddleCenter, -10), Contents.dragFiles, dragFileStyle);
            Event eve = Event.current;
            switch (eve.type)
            {
                case EventType.DragUpdated:
                    if (rect.Contains(eve.mousePosition))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                        Event.current.Use();
                    }
                    break;
                case EventType.DragPerform:
                    DragAndDrop.AcceptDrag();
                    Event.current.Use();
                    break;
                case EventType.DragExited:
                    if (rect.Contains(eve.mousePosition))
                    {
                        if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                        {
                            tool.UpLoad(DragAndDrop.paths);
                        }
                    }
                    break;
            }
        }


        private void ToolBar(Rect rect)
        {
            using (new EditorGUI.DisabledGroupScope(tool.freshing))
            {
                GUILayout.BeginArea(rect, EditorStyles.toolbar);
                {
                    GUILayout.BeginHorizontal();
                    {
                        //using (new EditorGUI.DisabledGroupScope(!tool.CanGoBack()))
                        //{

                        //    if (GUILayout.Button(Contents.goback, EditorStyles.toolbarButton,GUILayout.Width(30)))
                        //    {
                        //        tool.GoBack();
                        //    }
                        //}
                        //using (new EditorGUI.DisabledGroupScope(!tool.CanGoFront()))
                        //{

                        //    if (GUILayout.Button(Contents.gofront, EditorStyles.toolbarButton, GUILayout.Width(30)))
                        //    {
                        //        tool.GoFront();
                        //    }
                        //}
                        //using (new EditorGUI.DisabledGroupScope(!tool.CanGoUp()))
                        //{

                        //    if (GUILayout.Button(Contents.goup, EditorStyles.toolbarButton, GUILayout.Width(30)))
                        //    {
                        //        tool.GoUp();
                        //    }
                        //}
                        //if (GUILayout.Button(Contents.fresh, EditorStyles.toolbarButton, GUILayout.Width(30)))
                        //{
                        //    tool.FreshCurrent();
                        //}
                        set.select = (ToolType)GUILayout.Toolbar((int)set.select, Contents.toolSelect, EditorStyles.toolbarButton, GUILayout.Width(200));
                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button(Contents.help, EditorStyles.toolbarButton, GUILayout.Width(30)))
                        {
                            Application.OpenURL(LanzouCookie.helpUrl);
                        }
                        if (GUILayout.Button(Contents.web, EditorStyles.toolbarButton, GUILayout.Width(30)))
                        {
                            Application.OpenURL(LanzouCookie.webUrl);
                        }
                        GUILayout.Space(10);
                        //using (new EditorGUI.DisabledGroupScope(true))
                        //{
                        //    GUILayout.Label(Contents.path, GUILayout.Width(30));

                        //    GUILayout.TextField(tool.GetCurrentPath(), GUILayout.ExpandWidth(true));
                        //    if (tool.freshing)
                        //    {
                        //        GUILayout.Label(Contents.GetFreshing(),GUILayout.Width(30));
                        //    }
                        //    else
                        //    {
                        //        GUILayout.Space(30);
                        //    }
                        //}
                    }
                    GUILayout.EndHorizontal();

                }
                GUILayout.EndArea();
            }
        }
    }
}
