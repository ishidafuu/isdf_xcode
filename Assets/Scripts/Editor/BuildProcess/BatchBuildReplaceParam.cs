using System;
using UnityEditor;

[Serializable]
public class BatchBuildReplaceParam
{
    public string appName = string.Empty; // アプリ名
    public string version = string.Empty; //バージョン
    public int buildVersion = 0; //ビルド番号

    public bool useAppCompressWithLz4 = false; // アプリのlz4圧縮
    public bool buildLoclaAssetBundle = false; // localAssetBundleのbuild

    public bool appProfilering = false; // Profileの有効


    public virtual BuildOptions CreateBuildOptions()
    {
        BuildOptions options = BuildOptions.None;

        if (useAppCompressWithLz4)
            options |= BuildOptions.CompressWithLz4;

        if (appProfilering)
        {
            options |= BuildOptions.Development;
            options |= BuildOptions.ConnectWithProfiler;
        }

        return options;
    }
}