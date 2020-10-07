using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BatchBuild
{
    public const string FOLER_PARAM_PATH = "BuildParam";

    static string companyName = "com.isdf";
    static string androidBundle = "com.isdf.xcode";
    static string iosBundle = "com.isdf.xcode";
    static string appName = "isdf_xcode";
    static string version = "0.0.1";
    static int buildVersion = 1;

    static BatchBuildReplaceParam batchBuildReplaceParam = null;


    /// <summary>
    /// ビルド設定の上書き( 外部 )
    /// </summary>
    private static void CreateBuildParam()
    {
        batchBuildReplaceParam = null;

        const string PARAM_FILE = "build_param.json";
        const string PARAM_PATH = FOLER_PARAM_PATH + "/" + PARAM_FILE;

        try
        {
            var isFolder = Directory.Exists(FOLER_PARAM_PATH);
            Debug.Log("-----------------BuildParam Forlder ? " + isFolder);

            if (!isFolder)
                Directory.CreateDirectory(FOLER_PARAM_PATH);

            if (isFolder)
            {
                var isFile = File.Exists(PARAM_PATH);

                Debug.Log("-----------------BuildParam FILE ? " + isFile);

                if (isFile)
                {
                    var str = File.ReadAllText(PARAM_PATH, Encoding.UTF8);

                    if (!string.IsNullOrEmpty(str))
                    {
                        batchBuildReplaceParam = JsonUtility.FromJson<BatchBuildReplaceParam>(str);
                    }

                    Debug.Log("-----------------BuildParam str" + str);

                    File.Delete(PARAM_PATH);
                }
            }
        }
        catch (Exception)
        {
            throw new Exception("-----------------BuildParam file read faild!!!---------------------");
        }

        // 外部ファイルから生成出来なかったら、Defaultで生成する
        batchBuildReplaceParam ??= new BatchBuildReplaceParam();

        BuildCacheParam buildCacheParam = BuildCacheParam.Create(batchBuildReplaceParam);
        // cacheを更新
        buildCacheParam.SaveLocal();

        // cacheからの反映 ---------------------------------------------------------
        // 次のbuildversionを指定
        batchBuildReplaceParam.buildVersion = buildCacheParam.BuildVersion();

        // commandLineからの反映 -----------------------------------------------------
        var args = Environment.GetCommandLineArgs();

        var builder = new StringBuilder("-----------------ReplaceBuildSetting--------------\n");

        foreach (var e in args)
        {
            // プロファイルON
            if (e.Contains("APP_PUROFAIRINNGU"))
            {
                builder.AppendLine("APP_PUROFAIRINNGU");

                batchBuildReplaceParam.appProfilering = true;
            }
        }

        builder.AppendLine("----------------------------------------");

        Debug.Log(builder.ToString());

        ReplacePlayerSetting();

        // Unity ロゴを非表示にする( Plus/Proライセンスが必要 )
        PlayerSettings.SplashScreen.showUnityLogo = false;
    }


    /// <summary>
    /// 既存部分あんまり変えるの怖いので、static変数の値をすげ替えるだけにしておく
    /// </summary>
    private static void ReplacePlayerSetting()
    {
#if !SER
        //アプリ名変更
        if (!string.IsNullOrEmpty(batchBuildReplaceParam.appName))
            appName = batchBuildReplaceParam.appName;
#endif
        //アプリバージョン変更
        if (!string.IsNullOrEmpty(batchBuildReplaceParam.version))
            version = batchBuildReplaceParam.version;

        //ビルド番号変更
        if (batchBuildReplaceParam.buildVersion > 0)
            buildVersion = batchBuildReplaceParam.buildVersion;

        // localAssetBundleの作成
        if (batchBuildReplaceParam.buildLoclaAssetBundle)
        {
// #if UNITY_ANDROID
//             LocalAssetBundleBuilder.BuildANDROID();
// #elif UNITY_IOS
//             LocalAssetBundleBuilder.BuildIOS();
// #endif
        }
    }


    private static void Common(BuildTargetGroup targetGroup, string identifer)
    {
        // ビルドセッティングの設定
        PlayerSettings.SetScriptingBackend(targetGroup, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetIncrementalIl2CppBuild(targetGroup, true); // インクリメンタルBuild有効
        PlayerSettings.stripEngineCode = true;

        PlayerSettings.companyName = companyName; //"inmotion(" + System.DateTime.Now.ToString() + ")";
        PlayerSettings.productName = appName;
        PlayerSettings.SetApplicationIdentifier(targetGroup, appName);

        PlayerSettings.bundleVersion = version;

        PlayerSettings.SetApplicationIdentifier(targetGroup, identifer);
    }


    private static void SetStackTraceType(StackTraceLogType traceType)
    {
        PlayerSettings.SetStackTraceLogType(LogType.Error, traceType);
        PlayerSettings.SetStackTraceLogType(LogType.Assert, traceType);
        PlayerSettings.SetStackTraceLogType(LogType.Warning, traceType);
        PlayerSettings.SetStackTraceLogType(LogType.Log, traceType);
        PlayerSettings.SetStackTraceLogType(LogType.Exception, traceType);
    }


    // Android用ビルド
    [MenuItem("Tools/Build Android")]
    public static void BuildAndroid()
    {
        Debug.Log("android build start --------------------");

        // 外部のビルド設定を適用
        CreateBuildParam();

        // Android側は元々『BuildOptionsの指定がNone』だったので、想定どおりに動けば問題ない
        BuildOptions options = batchBuildReplaceParam.CreateBuildOptions();

        Debug.Log("---------------- BuildOptions :" + options.ToString());

        Common(BuildTargetGroup.Android, androidBundle);
        PlayerSettings.Android.bundleVersionCode = buildVersion;
        //PlayerSettings.statusBarHidden = true;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;

#if SER || PUR || STR
        // ビルドセッティングの設定
        PlayerSettings.statusBarHidden = true;

        // keystoreの指定。アプリに含めないようにEditorフォルダに入れてます。
        //PlayerSettings.asset内に直指定
        PlayerSettings.Android.keystoreName = Directory.GetCurrentDirectory() + "/KeyStore/.keystore";
        // keystore作成時に設定したkestoreのパスワード
        PlayerSettings.Android.keystorePass = "";
        //// keystore作成時に設定したalias名
        PlayerSettings.Android.keyaliasName = "";
        //// keystore作成時に設定したaliasのパスワード
        PlayerSettings.Android.keyaliasPass = "";

        //Log
        SetStackTraceType(StackTraceLogType.None);
#elif STG
        //Log
        SetStackTraceType(StackTraceLogType.None);
#else
        //Log
        SetStackTraceType(StackTraceLogType.ScriptOnly);
#endif

        if (!Directory.Exists("Build"))
            Directory.CreateDirectory("Build");

        //本番と同時にイストアール出来るけと、pushか来ないになる
        //File.Delete("Assets/Plugins/Android/AndroidManifest.xml");
        //本番と同時にイストアールできない、機能か正常になる
        //File.Delete("Assets/Plugins/Android/dev/AndroidManifest.xml");

        var buildReport = BuildPipeline.BuildPlayer(GetScenes(), "Build/android.apk", BuildTarget.Android, options);
        CheckBuildSuccess(buildReport);

        Debug.Log("android build end   --------------------");
    }


    // iOS用ビルド
    [MenuItem("Tools/Build iOS")]
    public static void BuildiOS()
    {
        Debug.Log("iOS build start ------------------------");

        // 外部のビルド設定を適用
        CreateBuildParam();

        BuildOptions opt = batchBuildReplaceParam.CreateBuildOptions();
        Debug.Log("---------------- BuildOptions :" + opt.ToString());

        // ビルドセッティングの設定
        Common(BuildTargetGroup.iOS, iosBundle);

        PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 1); // 1 ARM64 2 Universal (ARMv7 or ARM64)
        EditorUserBuildSettings.iOSBuildConfigType = iOSBuildType.Release; // xcode上でのビルドオプション設定
        PlayerSettings.iOS.buildNumber = buildVersion.ToString();
        PlayerSettings.iOS.appleEnableAutomaticSigning = true; // こいつをonにしないと、jenkinsでbuildしたときに失敗する

#if SER || PUR || STR
        // 本番は手動で行うのでoffにする
        PlayerSettings.iOS.appleEnableAutomaticSigning = false;

        //Log
        SetStackTraceType( StackTraceLogType.None);
#else
        //Log
        SetStackTraceType(StackTraceLogType.ScriptOnly);
#endif

        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;

        PlayerSettings.iOS.targetOSVersionString = "11.0";
        PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
        PlayerSettings.statusBarHidden = true;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

        if (!Directory.Exists("Build/Device"))
            Directory.CreateDirectory("Build/Device");

        var buildReport = BuildPipeline.BuildPlayer(GetScenes(), "Build/Device", BuildTarget.iOS, opt);
        CheckBuildSuccess(buildReport);

        Debug.Log("iOS build end   ------------------------");
    }


    // シーンを Editor の設定から取り出す
    static string[] GetScenes()
    {
        return EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);
    }


    static void CheckBuildSuccess(BuildReport buildReport)
    {
        // // buildReport.resultを見て、戻り値を決定する
        if (buildReport.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("build success");
        }
        else
        {
            throw new Exception("build faild");
        }
    }
}