using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace LanZouWindow
{
    [CreateAssetMenu]
    public class LanzouCookie : ScriptableObject
    {
        public string ylogin = "";
        [TextArea(3,8)]
        public string phpdisk_info = "";
        [OnOpenAssetAttribute(1)]
        public static bool step1(int instanceID, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj is LanzouCookie)
            {
                DiskWindow.cookie = obj as LanzouCookie;
                var window=EditorWindow. GetWindow<DiskWindow>();
                return true;
            }
            return false; 
        }
    }
}
