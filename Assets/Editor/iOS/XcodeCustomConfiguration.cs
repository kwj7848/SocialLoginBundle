using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

using UnityEditor.iOS.Xcode;


public static class XcodeCustomConfiguration
{
    public static string bluetoothUsageDescription = "비콘인식을 위해 블루투스 사용 권한을 허용해주세요.";

    [PostProcessBuild(999)]
    public static void OnPostProcessBuild( BuildTarget buildTarget, string path)
    {
        if(buildTarget == BuildTarget.iOS)
        {
            {
                string projectPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";

                PBXProject pbxProject = new PBXProject();
                pbxProject.ReadFromFile(projectPath);

                string target = pbxProject.GetUnityMainTargetGuid();            
                pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");

                pbxProject.WriteToFile (projectPath);
            }

            {
                string infoPlistPath = path + "/Info.plist";

                PlistDocument plistDoc = new PlistDocument();
                plistDoc.ReadFromFile(infoPlistPath);
                if (plistDoc.root != null) {
                    plistDoc.root.SetString("NSBluetoothPeripheralUsageDescription", bluetoothUsageDescription);
                    plistDoc.root.SetString("NSBluetoothAlwaysUsageDescription", bluetoothUsageDescription);

                    plistDoc.WriteToFile(infoPlistPath);
                }
                else {
                    Debug.LogError("ERROR: Can't open " + infoPlistPath);
                }
            }

            //ITSAppUsesNonExemptEncryption
        }
    }

}