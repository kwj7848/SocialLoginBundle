#if UNITY_IOS
using UnityEngine;
using System.Collections;
using UnityEditor.Callbacks;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using System.IO;
using System;
using System.Linq;

public static class KakaoPostprocess
{

    const string KAKAO_APP_KEY = "bc17e414c7708d541bf6f23e0ebe03b9";
    const string KAKAO_URL_SCHEME = "kakao" + KAKAO_APP_KEY;

    static string ProjectPath = string.Empty;
    static string PbxProjectPath = string.Empty;

    [PostProcessBuild(999)]    // @param - order index in the callback
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        // 빌드 후 실행되는 callback func	
        if (target == BuildTarget.iOS)    // iOS 플랫폼일 경우만
        {
            ProjectPath = path;
            PbxProjectPath = PBXProject.GetPBXProjectPath(path);

            PostProcessIosProject();
        }
    }

    static void PostProcessIosProject()
    {
        // do something
        ModifyProject(AddLinkerFlag);
        ModifyPlist(AddKakaoAppKey);
        ModifyPlist(AddApplicationQuerySceheme);
        ModifyPlist(AddKakaoTalkUrlScheme);
    }

#region helpers

    // 빌드 설정 변경 helper
    static void ModifyProject(Action<PBXProject> modifier)
    {
        try
        {
            PBXProject project = new PBXProject();
            project.ReadFromString(File.ReadAllText(PbxProjectPath));

            modifier(project);

            File.WriteAllText(PbxProjectPath, project.WriteToString());
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    // Info.plist 설정 변경 helper
    static void ModifyPlist(Action<PlistDocument> modifier)
    {
        try
        {
            var plistInfoFile = new PlistDocument();

            string infoPlistPath = Path.Combine(ProjectPath, "Info.plist");
            plistInfoFile.ReadFromString(File.ReadAllText(infoPlistPath));

            modifier(plistInfoFile);

            File.WriteAllText(infoPlistPath, plistInfoFile.WriteToString());
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }


#endregion
    // 빌드 Linker 설정 추가
    static void AddLinkerFlag(PBXProject project)
    {
        project.ReadFromString(File.ReadAllText(PbxProjectPath));
        string buildTarget = project.GetUnityMainTargetGuid();
        project.AddBuildProperty(buildTarget, "OTHER_LDFLAGS", "-all_load");
    }


    // KAKAO_APP_KEY property 추가
    static void AddKakaoAppKey(PlistDocument plist)
    {
        plist.root.SetString("KAKAO_APP_KEY", KAKAO_APP_KEY);
    }

    // LSApplicationQueriesSchemes property 추가
    static void AddApplicationQuerySceheme(PlistDocument plist)
    {
        const string LSApplicationQueriesSchemes = "LSApplicationQueriesSchemes";

        string[] kakaoSchemes =
        {
        "kakaokompassauth",
        KAKAO_URL_SCHEME,
        "kakaolink",
        "kakaotalk-5.9.7"
        };

        PlistElementArray appsArray;
        appsArray = plist.root.values.ContainsKey(LSApplicationQueriesSchemes) ?
                (PlistElementArray)plist.root.values[LSApplicationQueriesSchemes] :
                plist.root.CreateArray(LSApplicationQueriesSchemes);
        kakaoSchemes.ToList().ForEach(appsArray.AddString);
    }

    // URL Scheme 설정 추가
    static void AddKakaoTalkUrlScheme(PlistDocument plist)
    {
        const string CFBundleURLTypes = "CFBundleURLTypes";
        const string CFBundleURLSchemes = "CFBundleURLSchemes";

        if (!plist.root.values.ContainsKey(CFBundleURLTypes))
        {
            plist.root.CreateArray(CFBundleURLTypes);
        }

        var cFBundleURLTypesElem = plist.root.values[CFBundleURLTypes] as PlistElementArray;

        var getSocialUrlSchemesArray = new PlistElementArray();
        getSocialUrlSchemesArray.AddString(KAKAO_URL_SCHEME);

        PlistElementDict getSocialSchemeElem = cFBundleURLTypesElem.AddDict();
        getSocialSchemeElem.values[CFBundleURLSchemes] = getSocialUrlSchemesArray;
    }





}
#endif
