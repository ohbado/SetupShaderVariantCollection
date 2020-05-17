using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System;
using System.Text;
using System.Collections.Generic;

public class SetupSVCollection : EditorWindow
{
    private ShaderVariantCollection svc;
    private string filePath;
    /// <summary>
    /// shaderKeywordの列番号
    /// </summary>
    private readonly int shaderKeywordColumn = 6;
    /// <summary>
    /// Shader名の列番号
    /// </summary>
    private readonly int shaderNameColumn = 1;
    /// <summary>
    /// passの列番号
    /// </summary>
    private readonly int passTypeColumn = 4;

    [MenuItem("Tools/SetupShaderVariantCollection")]
    public static void ShowWindow()
    {
        GetWindow(typeof(SetupSVCollection));
    }

    void OnGUI()
    {
        svc = (ShaderVariantCollection)EditorGUILayout.ObjectField("ShaderVariantCollection", svc, typeof(ShaderVariantCollection), false);

        if (string.IsNullOrEmpty(filePath))
        {
            EditorGUILayout.LabelField("Select CSV File");
        }
        else
        {
            EditorGUILayout.LabelField(this.filePath);
        }
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Select CSV File", GUILayout.Width(100.0f)))
        {
            this.filePath = EditorUtility.OpenFilePanelWithFilters("", "Select shader compile profiler csv", new string[] { "shader compile log", "csv" });
        }

        if (GUILayout.Button("Set Variant", GUILayout.Width(80.0f)))
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Debug.LogError("No such File ");
            }
            if (!svc)
            {
                Debug.LogError("No ShaderVariantCollection");
            }
            else
            {
                //処理
                CSVToSVC(svc, filePath);
            }
        }
        EditorGUILayout.EndHorizontal();

    }

    private void CSVToSVC(ShaderVariantCollection _svc, string csvPath)
    {
        //CSVから要素を取り出す
        var svList = MakeShaderVariantListFromPath(csvPath, shaderKeywordColumn, shaderNameColumn);

        //CSVからの要素をShaderVariantCollectionに追加
        AddShaderVariantFromShaderList(_svc, svList);
    }

    private void AddShaderVariantFromShaderList(ShaderVariantCollection _svc, List<ShaderVariantCollection.ShaderVariant> svList)
    {
        foreach (var sv in svList)
        {
            if (!_svc.Contains(sv))
            {
                //存在しないので追加
                _svc.Add(sv);
            }
        }
    }

    private List<ShaderVariantCollection.ShaderVariant> MakeShaderVariantListFromPath(string path, int keywordColumn, int shderNameColumn)
    {
        var csvStringList = LoadCSVToStringList(path);
        Debug.Log("Deleted from CSV " + csvStringList[0].ToString());
        csvStringList.RemoveAt(0);
        return MakeShaderVariantListFromStringList(csvStringList, keywordColumn, shderNameColumn);
    }

    private List<string[]> LoadCSVToStringList(string path)
    {
        var list = new List<string[]>();
        StreamReader reader = new StreamReader(path, Encoding.GetEncoding("UTF-8"));
        while (reader.Peek() >= 0)
        {
            list.Add(reader.ReadLine().Split(','));
        }
        reader.Close();
        return list;
    }

    /// <summary>
    /// CSVを読み込んだ文字列リストから要素を作成
    /// </summary>
    /// <param name="csvStrings"></param>
    /// <returns></returns>
    private List<ShaderVariantCollection.ShaderVariant> MakeShaderVariantListFromStringList(List<string[]> csvStrings, int keywordColumn,int shderNameColumn)
    {
        var returnList = new List<ShaderVariantCollection.ShaderVariant>();
        foreach (var csvstr in csvStrings)
        {
            var shaderName = csvstr[shderNameColumn];
            //shader取得
            var sd = Shader.Find(shaderName);
            if (sd == null)
            {
                continue;
            }
            //keyword取得
            var leywd = GetKeywordFromCSVList(csvstr, keywordColumn);

            var sdkw = new ShaderVariantCollection.ShaderVariant();
            sdkw.shader = sd;
            sdkw.keywords = leywd;
            UnityEngine.Rendering.PassType pt;
            if (GetPassType(csvstr, passTypeColumn, out pt))
            {
                sdkw.passType = pt;
            }
            returnList.Add(sdkw);
        }
        return returnList;
    }

    /// <summary>
    /// 渡された1行からkeywordを配列として取り出す
    /// </summary>
    /// <param name="rowstr"></param>
    /// <param name="keywordColumn"></param>
    /// <returns></returns>
    private string[] GetKeywordFromCSVList(string[] rowstr, int keywordColumn)
    {
        //1行取り出し
        var keywords = rowstr[keywordColumn];
        //空白区切りで要素の取り出し
        return keywords.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
    }

    private bool GetPassType(string[] rowstr, int passTypeColumn, out UnityEngine.Rendering.PassType passType)
    {
        var typeStr = rowstr[passTypeColumn];
        Enum.TryParse(typeStr, out passType);
        return Enum.IsDefined(typeof(UnityEngine.Rendering.PassType), passType);
    }
}
