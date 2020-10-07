#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
namespace NK
{
    public static class iOSPostProcess
    {
        [PostProcessBuild]
        private static void OnPostProcessBuild( BuildTarget buildTarget, string path )
        {
            if ( buildTarget != BuildTarget.iOS ) return;

            var projectPath = PBXProject.GetPBXProjectPath( path );
            var project = new PBXProject();
            project.ReadFromFile( projectPath );
            
            string[] targetGuids = { project.GetUnityMainTargetGuid(), 
                project.GetUnityFrameworkTargetGuid() };

            project.SetBuildProperty(targetGuids, "ENABLE_BITCODE", "NO");

            project.WriteToFile( projectPath );
        }
    }
}
#endif
#endif