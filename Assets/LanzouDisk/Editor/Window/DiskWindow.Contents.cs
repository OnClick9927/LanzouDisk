using System;
using UnityEditor;
using UnityEngine;

namespace LanZouWindow
{
    partial class DiskWindow
    {
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
            public static GUIContent choosefolder = new GUIContent(EditorGUIUtility.IconContent("Folder Icon")) { tooltip = "选择路径" };
            public static GUIContent path = new GUIContent("Path:", "当前路径");
            public static GUIContent titleContent = new GUIContent("LanzouDisk");
            public static GUIContent files = new GUIContent("Files");
            public static GUIContent folders = new GUIContent("Folders");
            public static GUIContent upcloud = new GUIContent(EditorGUIUtility.IconContent("d_CloudConnect").image, "上传") { text = "↑" };
            public static GUIContent downcloud = new GUIContent(EditorGUIUtility.IconContent("d_CloudConnect").image) { text = "↓" };
            public static GUIContent dragFiles = new GUIContent(EditorGUIUtility.IconContent("console.infoicon").image, "拖拽文件/文件夹到此处") { text = "Drag Files Or Folders Here" };
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
            public static GUIContent uploadlistlabel = new GUIContent();
            public static GUIContent downloadlistlabel = new GUIContent();

            public static GUIContent GetDownLoadListLabel(int count)
            {
                downloadlistlabel.text = $"Download List  {count}";
                downloadlistlabel.tooltip = $"剩余下载任务 {count} 个";

                return downloadlistlabel;
            }
            public static GUIContent GetUpLoadListLabel(int count)
            {
                uploadlistlabel.text = $"UpLoad List  {count}";
                uploadlistlabel.tooltip = $"剩余上传任务 {count} 个";

                return uploadlistlabel;
            }
        }

    }
}
