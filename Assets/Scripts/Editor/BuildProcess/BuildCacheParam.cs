using System;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Cache用ファイル
/// </summary>
[Serializable]
public class BuildCacheParam
{
    const string BUILD_CACHE_PARAM_PATH = BatchBuild.FOLER_PARAM_PATH + "/" + "build_cache_param.json";

    [SerializeField] int buildVersion; // buildversionを毎度指定するのはしんどい

    public BuildCacheParam() { }
    public BuildCacheParam(int _buildVersion) => buildVersion = _buildVersion;

    public int BuildVersion() => buildVersion;
    public void IncrementBuildVersion() => ++buildVersion;


    /// <summary>
    /// cacheのセーブ
    /// </summary>
    public void SaveLocal()
    {
        // save
        try
        {
            var str = JsonUtility.ToJson(this);

            Debug.Log("-----------------BuildCacheParam " + str);
            // 被らないようにlocalに保存し直す
            File.WriteAllText(BUILD_CACHE_PARAM_PATH, str, Encoding.UTF8);
        }
        catch (Exception)
        {
            throw new Exception("-----------------BuildCacheParam file write faild!!!---------------------");
        }
    }


    /// <summary>
    /// cacheのロード
    /// </summary>
    /// <returns></returns>
    static BuildCacheParam LoadLocal()
    {
        BuildCacheParam buildCacheParam = default;

        try
        {
            bool isFile = File.Exists(BUILD_CACHE_PARAM_PATH);

            Debug.Log("-----------------BuildCacheParam" +
                      " File ? " + isFile);

            // ファイルあり
            if (isFile)
            {
                var str = File.ReadAllText(BUILD_CACHE_PARAM_PATH, Encoding.UTF8);
                buildCacheParam = JsonUtility.FromJson<BuildCacheParam>(str);
            }
        }
        catch (Exception)
        {
            throw new Exception("-----------------BuildCacheParam file read faild!!!---------------------");
        }

        // cacheがなければ作る
        buildCacheParam = buildCacheParam ?? new BuildCacheParam();

        return buildCacheParam;
    }


    /// <summary>
    /// cacheのロード及び、作成
    /// </summary>
    /// <param name="batchBuildReplaceParam"></param>
    /// <returns></returns>
    public static BuildCacheParam Create(BatchBuildReplaceParam batchBuildReplaceParam)
    {
        if (batchBuildReplaceParam == null)
            throw new Exception("-----------------BatchBuildReplaceParam null !!---------------------");

        BuildCacheParam buildCacheParam = default;

        // 指定が無ければ、localに記録してあるversionに従う(ファイルが消えた時とか、指定したい時がある)
        if (batchBuildReplaceParam.buildVersion == 0)
        {
            buildCacheParam = LoadLocal();
            // 自動Increment!
            buildCacheParam.IncrementBuildVersion();
        }
        // jenkinsからの指定がある(使い勝手を考慮して、常にCacheを上書きするようにする)
        else
        {
            buildCacheParam = new BuildCacheParam(batchBuildReplaceParam.buildVersion);
        }
        return buildCacheParam;
    }
}