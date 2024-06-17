#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.AddressableAssets;
using File = System.IO.File;

public class ToolSpineManagerGen : OdinEditorWindow
{
    private const string Format =
@"//ToolSpineManagerGen 의해 자동으로 생성된 스크립트입니다..
using System.ComponentModel;
using Spine;
using Spine.Unity;
using System.Collections.Generic;
using System;
public static class SpineManager
{{
    public static void SetSkin(this SkeletonAnimation SpineCharacter, string eGangSkin)
    {{
        SetSkin(SpineCharacter, eGangSkin.ParseEnum<EGangSkin>());
    }}
    public static void SetSkin(this SkeletonAnimation SpineCharacter, EGangSkin eGangSkin)
    {{
        SpineCharacter.Skeleton.SetSkin(SpineCharacter.skeletonDataAsset.GetSkeletonData(true).FindSkin(eGangSkin.OriginName()));
        SpineCharacter.Skeleton.SetSlotsToSetupPose();
    }}
    public static void AddSkin(this SkeletonAnimation SpineCharacter, EGangSkin eGangSkin)
    {{
        Skin currentSkin = SpineCharacter.Skeleton.Skin;
        Skin addSkin = SpineCharacter.skeletonDataAsset.GetSkeletonData(true).FindSkin(eGangSkin.OriginName());
        Skin temporarySkin = new Skin(""temporarySkin"");
        // addSkin.Attachments의 항목을 임시 리스트에 복사
        var entries = new List<Skin.SkinEntry>(currentSkin.Attachments);

        foreach (var entry in entries)
        {{
            int slotIndex = entry.SlotIndex;
            string attachmentName = entry.Name;
            Attachment brownhairAttachment = entry.Attachment;

            // 현재 스킨의 해당 슬롯에 brownhairSkin의 어태치먼트 설정
            temporarySkin.SetAttachment(slotIndex, attachmentName, brownhairAttachment);
        }}

        entries = new List<Skin.SkinEntry>(addSkin.Attachments);
        // 임시 리스트를 순회
        foreach (var entry in entries)
        {{
            int slotIndex = entry.SlotIndex;
            string attachmentName = entry.Name;
            Attachment brownhairAttachment = entry.Attachment;

            // 현재 스킨의 해당 슬롯에 brownhairSkin의 어태치먼트 설정
            temporarySkin.SetAttachment(slotIndex, attachmentName, brownhairAttachment);
        }}

        // 변경된 스킨 적용하기
        SpineCharacter.Skeleton.SetSkin(temporarySkin);
        SpineCharacter.Skeleton.SetSlotsToSetupPose();

    }}
}}
{0}
{1}
{2}
{3}
{4}
";

    //{0} 변수 타입
    //{1} 변수 이름
    private const string ClassVariableFormat = "\n\tpublic {0} {1};";

    private static HashSet<string> gangTypeSet = new();

    [MenuItem("Tools/ToolSpineManagerGen")]
    public async static void Gen()
    {
        var tableHandle = Addressables.LoadAssetAsync<TextAsset>("Assets/1_kds/GangSpineExport/skeleton.json");
        await UniTask.WaitWhile(() => !tableHandle.IsDone);
        var tableTostring = tableHandle.Result.text;
        var jsonData = JObject.Parse(tableTostring);
        StringBuilder csString = new StringBuilder();


        List<string> nameList = new();
        foreach (var item in jsonData["skins"])
        {
            var originName = item["name"].ToString();
            if (originName.Equals("default"))
            {
                continue;
            }
            nameList.Add(originName);


            string pattern = @"\[(.*?)\]";
            Match match = Regex.Match(originName, pattern);
            if (match.Success)
            {
                string extractedWord = match.Groups[1].Value;
                gangTypeSet.Add(extractedWord);
            }

        }
        var eSpineSkin = GameUtil.GenerateEnum("EGangSkin", nameList);
        var eGangType = GameUtil.GenerateEnum("EGangType", gangTypeSet.ToList());


        nameList.Clear();
        JObject animationObject = (JObject)jsonData["animations"];

        foreach (var property in animationObject.Properties())
        {
            var originName = property.Name.ToString();
            nameList.Add(originName);
            var convertedName = GameUtil.ConvertNameToCodeName(originName);
        }
        var eSpineAnimation = GameUtil.GenerateEnum("EGangAnimation", nameList);

        nameList.Clear();
        foreach (var item in jsonData["slots"])
        {
            var originName = item["name"].ToString();
            if (originName.Equals("default"))
            {
                continue;
            }
            nameList.Add(originName);
        }
        var eSpinSlot = GameUtil.GenerateEnum("EGangSlot", nameList);

        nameList.Clear();
        JObject eventObject = (JObject)jsonData["events"];
        foreach (var property in eventObject.Properties())
        {
            var originName = property.Name.ToString();
            nameList.Add(originName);
        }
        var eSpineEvent = GameUtil.GenerateEnum("EGangEvent", nameList);

        var filePath = $"{Application.dataPath}/1_kds/Scripts/codegen/SpineManager.cs";

        File.WriteAllText(filePath, string.Format(Format, eSpineSkin, eSpineAnimation, eSpinSlot, eSpineEvent, eGangType));
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        CompilationPipeline.RequestScriptCompilation();
    }
}
#endif