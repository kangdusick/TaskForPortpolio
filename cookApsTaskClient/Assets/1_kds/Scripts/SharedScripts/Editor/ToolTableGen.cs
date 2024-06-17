#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.AddressableAssets;
using File = System.IO.File;

public class ToolTableGen : OdinEditorWindow
{
    //{0} key 자료형
    //{1} 클래스명
    private const string DictFormat =
@"  
     public static Dictionary<{0}, {1}> {1}Dict {{ get; private set; }}";

    //{0}변수 리스트명(json파일의 row명과 같아야한다)
    //{1} 클래스명
    //{2} key 자료형
    private const string SwitchCaseFormat =
@"                    
                    case ""{0}"":
                        {1}Dict = LoadJson<{1}Loader, {2}, {1}>(item.Value.ToString()).MakeDict();
                        break;";

    //{0} DictFormat
    //{1} SwitchCaseFormat
    //{2} DictClearFormat
    private const string CsFormat =
@"//ToolTableGen에 의해 자동으로 생성된 스크립트입니다.
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using System.Numerics;
using Newtonsoft.Json;

public static class TableManager
{{
    public static bool isLoadDone;
    {0}


    public static void LoadTables()
    {{
        isLoadDone = false;
        var tableHandle = Addressables.LoadAssetAsync<TextAsset>(""Assets/1_kds/Json/Tables.json"").WaitForCompletion();
        var tableTostring = tableHandle.text;
        tableTostring = ES3.DecryptString(tableTostring);
        var jsonData = JObject.Parse(tableTostring);
        foreach (var item in jsonData)
        {{
            foreach (var jToken in item.Value)
            {{
                var table = jToken.ToObject<JProperty>();
                switch (table.Name)
                {{
                    {1}
                }}
            }}
        }}
        TMPLinkDetector.UpdateLanguageTablesWithLinks();
        isLoadDone = true;
    }}

    static Loader LoadJson<Loader, Key, Value>(string tableStr) where Loader : ILoader<Key, Value>
    {{
        var settings = new JsonSerializerSettings
        {{
            Converters = new List<JsonConverter> {{ new CustomIntConverter() }}
        }};
        return JsonConvert.DeserializeObject<Loader>(tableStr, settings);
    }}
}}
public class CustomIntConverter : JsonConverter
{{
    public override bool CanConvert(Type objectType)
    {{
        return objectType == typeof(int) || objectType == typeof(float) || objectType == typeof(BigInteger) || objectType == typeof(bool);
    }}

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {{
        if (reader.TokenType == JsonToken.Null)
        {{
            if (objectType == typeof(int))
            {{
                return 0;
            }}
            else if (objectType == typeof(float))
            {{
                return 0f;
            }}
            else if(objectType == typeof(BigInteger))
            {{
                return BigInteger.Zero;
            }}
            else if(objectType == typeof(bool))
            {{
                return false;
            }}
        }}

        if (reader.Value != null)
        {{
            if (objectType == typeof(int))
            {{
                if (int.TryParse(reader.Value.ToString(), out int intResult))
                {{
                    return intResult;
                }}
            }}
            else if (objectType == typeof(float))
            {{
                if (float.TryParse(reader.Value.ToString(), out float floatResult))
                {{
                    return floatResult;
                }}
            }}
            else if (objectType == typeof(BigInteger))
            {{
                string valueStr = reader.Value.ToString();

                // 지수 표기법을 처리하기 위해 double 또는 decimal로 변환
                if (double.TryParse(valueStr, out double doubleResult))
                {{
                    // double 값을 BigInteger로 변환
                    return new BigInteger(doubleResult);
                }}
                else if (BigInteger.TryParse(valueStr, out BigInteger bigIntegerResult))
                {{
                    return bigIntegerResult;
                }}
            }}
            else if (objectType == typeof(bool))
            {{
                if (bool.TryParse(reader.Value.ToString(), out bool boolResult))
                {{
                    return boolResult;
                }}
            }}
        }}

        if (objectType == typeof(BigInteger))
        {{
            return BigInteger.Zero;
        }}
        else if (objectType == typeof(float))
        {{
            return 0f;
        }}
        else if(objectType == typeof(bool))
        {{
            return false;
        }}
        return 0;
    }}

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {{
        serializer.Serialize(writer, value);
    }}
}}
public interface ILoader<Key, Value>
{{
    Dictionary<Key, Value> MakeDict();
}}";

    //{0} 클래스 이름
    private const string ClassNameFormat =
@"
[Serializable]
public class {0}
{{";

    //{0} 변수 타입
    //{1} 변수 이름
    private const string ClassVariableFormat = "\n\tpublic {0} {1};";

    private const string ClassListVariableFormat = "\n\tpublic {0} {1} = new();";
    //{0} 클래스 이름
    //{1} 딕셔너리 키 타입(uid의 타입)
    //{2} 변수 리스트명(json파일의 row명과 같아야한다)
    //{3} 첫번째 변수명(dictionary의 key로 쓰인다)
    private const string LoaderFormat =
@"
[Serializable]
public class {0}Loader : ILoader<{1}, {0}>
{{
    public List<{0}> {2} = new List<{0}>();

    public Dictionary<{1}, {0}> MakeDict()
    {{
        var dataDict = new Dictionary<{1}, {0}>();
        foreach (var data in {2})
        {{
            dataDict.Add(data.{3}.ParseEnum<{1}>(), data);
        }}
        return dataDict;
    }}
}}";
    [MenuItem("Tools/ToolTableGen")]
    public async static void Gen()
    {
        var tableHandle = Addressables.LoadAssetAsync<TextAsset>("Assets/1_kds/Json/Tables.json");
        await UniTask.WaitWhile(()=> !tableHandle.IsDone);
        var tableTostring = tableHandle.Result.text;
        var jsonData = JObject.Parse(tableTostring);
        StringBuilder csString = new StringBuilder();
        var dictFormat = "";
        var switchFormat = "";
        var classString = "";
        var loaderString = "";
        var enumString = "";
        bool isNoHash = false;
        List<string> nameList = new();

        foreach (var item in jsonData)
        {
            foreach (var jToken in item.Value)
            {
                var isMakeUidType = false;
                var table = jToken.ToObject<JProperty>();
                isNoHash = false;
                var className = item.Key;
                if(className.Contains("NoHash_"))
                {
                    isNoHash = true;
                    className = className.Split("NoHash_")[1];
                }
                Debug.Log(className);
                var tableRowName = table.Name;
                classString += string.Format(ClassNameFormat, className);
                foreach (var child in table.First[0].Children())
                {
                    var jProperty = child.ToObject<JProperty>();
                    var valType = MakeVariableType(jProperty);
                    if(valType.Contains("List"))
                    {
                        classString += string.Format(ClassListVariableFormat, valType, jProperty.Name);
                    }
                    else
                    {
                        classString += string.Format(ClassVariableFormat, valType, jProperty.Name);
                    }
                    if (!isMakeUidType)
                    {
                        //loader 생성 코드, 첫 번째 변수의 자료형과 이름이 여기서 결정된다
                        isMakeUidType = true;
                        loaderString += string.Format(LoaderFormat, className, "E" + className, tableRowName, jProperty.Name);
                        dictFormat += string.Format(DictFormat, "E" + className, className);
                        switchFormat += string.Format(SwitchCaseFormat, tableRowName, className, "E" + className);
                    }
                }
                classString += "\n}";

                //각 클래스 key의 이름으로 이루어진 Enum 생성
                nameList.Clear();
                foreach (var child in table.First)
                {
                    nameList.Add(child.First.First.ToString());
                }
                enumString += GameUtil.GenerateEnum("E"+className, nameList, isNoHash);
            }
        }
        csString.Append(string.Format(CsFormat, dictFormat, switchFormat));
        csString.Append("\n\n" + enumString);
        csString.Append("\n\n" + loaderString);
        csString.Append("\n\n" + classString);

        var filePath = $"{Application.dataPath}/1_kds/Scripts/codegen/TableManager.cs";

        tableTostring = ES3.EncryptString(tableTostring);
        File.WriteAllText($"{Application.dataPath}/1_kds/Json/Tables.json", tableTostring);

        File.WriteAllText(filePath, csString.ToString());
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        CompilationPipeline.RequestScriptCompilation();
    }
    private static string MakeVariableType(JProperty jProperty)
    {
        string variableType = "";
        switch (jProperty.Value.Type)
        {
            case JTokenType.None:
                break;
            case JTokenType.Object:
                break;
            case JTokenType.Array:
                variableType = string.Format("List<{0}>", MakeVariableType(jProperty.First[0]));
                break;
            case JTokenType.Constructor:
                break;
            case JTokenType.Property:
                break;
            case JTokenType.Comment:
                break;
            case JTokenType.Integer:
                variableType = (long)jProperty.Value >= 10000 ? "BigInteger" : "int";
                break;
            case JTokenType.Float:
                variableType = "float";
                break;
            case JTokenType.String:
                variableType = "string";
                break;
            case JTokenType.Boolean:
                variableType = "bool";
                break;
            case JTokenType.Null:
                break;
            case JTokenType.Undefined:
                break;
            case JTokenType.Date:
                break;
            case JTokenType.Raw:
                break;
            case JTokenType.Bytes:
                break;
            case JTokenType.Guid:
                break;
            case JTokenType.Uri:
                break;
            case JTokenType.TimeSpan:
                break;
            default:
                break;
        }
        return variableType;
    }
    private static string MakeVariableType(JToken jTocken)
    {
        string variableType = "";
        switch (jTocken.Type)
        {
            case JTokenType.None:
                break;
            case JTokenType.Object:
                break;
            case JTokenType.Array:
                variableType = string.Format("List<{0}>", MakeVariableType(jTocken.First[0]));
                break;
            case JTokenType.Constructor:
                break;
            case JTokenType.Property:
                break;
            case JTokenType.Comment:
                break;
            case JTokenType.Integer:
                variableType = int.Parse(jTocken.ToString())>= 10000 ? "BigInteger" : "int";
                break;
            case JTokenType.Float:
                variableType = "float";
                break;
            case JTokenType.String:
                variableType = "string";
                break;
            case JTokenType.Boolean:
                variableType = "bool";
                break;
            case JTokenType.Null:
                break;
            case JTokenType.Undefined:
                break;
            case JTokenType.Date:
                break;
            case JTokenType.Raw:
                break;
            case JTokenType.Bytes:
                break;
            case JTokenType.Guid:
                break;
            case JTokenType.Uri:
                break;
            case JTokenType.TimeSpan:
                break;
            default:
                break;
        }
        return variableType;
    }
}
#endif
