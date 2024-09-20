using System.IO;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
using static System.IO.Directory;
using static System.IO.Path;
using static UnityEngine.Application;
using static UnityEditor.AssetDatabase;    
#endif


namespace Yash
{
#if UNITY_EDITOR
    public static class ToolsMenu
    {
    
        [MenuItem("Tools/Setup/Create Default Folders")]
        public static void CreateDefaultFolders()
        {
            CreateDirectories("_Project", "Scripts", "Art", "Scenes","ScriptableObjects");
            Refresh();
        }

        public static void CreateDirectories(string root, params string[] dir)
        {
            var fullpath = Combine(dataPath, root);
            
            foreach (var newDir in dir)
                CreateDirectory(Combine(fullpath, newDir));
        }
    }
#endif
    
}
