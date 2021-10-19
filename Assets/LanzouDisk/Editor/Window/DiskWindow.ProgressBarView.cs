using LanZouCloudAPI;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LanZouWindow
{
    partial class DiskWindow
    {
        public class ProgressBarView : IProgress<ProgressInfo>
        {
            private Queue<ProgressInfo> downs = new Queue<ProgressInfo>();
            private object downsLock = new object();
            private string progressTxt = "";
            private float progress = 0;
            private string txtFormat = "";
            public static ProgressBarView current;
            public ProgressBarView(string txtFormat)
            {
                this.txtFormat = txtFormat;
            }
            private void SetProgressTxt(ProgressInfo value)
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
            void IProgress<ProgressInfo>.Report(ProgressInfo value)
            {
                lock (downsLock)
                {
                    downs.Enqueue(value);
                }
            }
        }

    }
}
