using LanZouAPI;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Test : EditorWindow
{
    [MenuItem("Tools/LanZouCloud")]
    static void Init()
    {
        GetWindow<Test>(false, "LanZouCloud", true).Show();
    }

    private const string key0 = "1104264";
    private const string key1 = "VWAHNAJgAjoPPwdhWzVUB1MxDTxdDVA2UmkBZwI0BTdXYl9tVzANNQc9VDcMXwBvVWRSMQpkAGIDOAIzAzYKPVUwB2cCMgI3DzsHYls1VDlTMA09XTJQYlIzATcCNQU1V2FfZFdgDTEHPFRmDGMAU1U0UmgKZQBnAzACYwM1Cj1VZQc9AmM%3D";
    private LanZouCloud cloud = new LanZouCloud();

    private bool isLogin = false;
    private bool isFetchFiles = false;
    private Vector2 scroll = new Vector2();
    private CloudFileList fileList;
    private Dictionary<long, DownloadProgressInfo> downs = new Dictionary<long, DownloadProgressInfo>();


    private void OnEnable()
    {
        cloud.set_log_level(LanZouCloud.LogLevel.Info);
    }

    private void OnDisable()
    {

    }

    private void OnGUI()
    {
        EditorGUI.BeginDisabledGroup(isLogin);
        if (GUILayout.Button(isLogin ? "Wait for login..." : "Login"))
        {
            Login();
        }
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(isFetchFiles);
        if (GUILayout.Button(isFetchFiles ? "Fetching..." : "Fetch Files"))
        {
            FetchFiles();
        }
        EditorGUI.EndDisabledGroup();

        DrawFiles();
    }

    private void DrawFiles()
    {
        if (fileList == null || fileList.code != LanZouCode.SUCCESS)
            return;

        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (var file in fileList.files)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(file.name, GUILayout.Width(160));
            EditorGUILayout.LabelField(file.size, GUILayout.Width(80));

            if (downs.TryGetValue(file.id, out var val))
            {
                EditorGUILayout.LabelField($"[{val.current}/{val.total}]", GUILayout.Width(160));
            }
            else
            {
                if (GUILayout.Button("Download", GUILayout.Width(160)))
                {
                    DownloadFile(file.id);
                }
            }

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    private void Notify(string title, LanZouCode code)
    {
        ShowNotification(new GUIContent($"{title} {code}"));
    }

    private async void Login()
    {
        isLogin = true;
        var code = await cloud.login_by_cookie(key0, key1);
        isLogin = false;
        Notify("登录操作", code);
    }

    private async void FetchFiles()
    {
        isFetchFiles = true;
        fileList = await cloud.get_file_list();
        Notify("拉取文件", fileList.code);
        isFetchFiles = false;
    }

    private async void DownloadFile(long file_id)
    {
        downs[file_id] = new DownloadProgressInfo();

        await cloud.down_file_by_id(file_id, "download", false, (progress) =>
        {
            // Debug.Log($"download [{progress.current}/{progress.total}]");
            downs[file_id] = progress;
            Repaint();
        });

        if (downs.ContainsKey(file_id))
        {
            downs.Remove(file_id);
        }
    }
}
