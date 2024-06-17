//ToolTableGen에 의해 자동으로 생성된 스크립트입니다.
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
{
    public static bool isLoadDone;
      
     public static Dictionary<EPowerTable, PowerTable> PowerTableDict { get; private set; }  
     public static Dictionary<EAbilityTable, AbilityTable> AbilityTableDict { get; private set; }  
     public static Dictionary<ECharacterTable, CharacterTable> CharacterTableDict { get; private set; }  
     public static Dictionary<EConfigTable, ConfigTable> ConfigTableDict { get; private set; }  
     public static Dictionary<ELanguageTable, LanguageTable> LanguageTableDict { get; private set; }  
     public static Dictionary<ELinkTable, LinkTable> LinkTableDict { get; private set; }  
     public static Dictionary<EStageInfoTable, StageInfoTable> StageInfoTableDict { get; private set; }  
     public static Dictionary<EWeatherTable, WeatherTable> WeatherTableDict { get; private set; }  
     public static Dictionary<ESkillTrainingTable, SkillTrainingTable> SkillTrainingTableDict { get; private set; }  
     public static Dictionary<EPatchNoteTable, PatchNoteTable> PatchNoteTableDict { get; private set; }  
     public static Dictionary<EShopTable, ShopTable> ShopTableDict { get; private set; }  
     public static Dictionary<EInGameShopTable, InGameShopTable> InGameShopTableDict { get; private set; }  
     public static Dictionary<EInGameEventTable, InGameEventTable> InGameEventTableDict { get; private set; }


    public static void LoadTables()
    {
        isLoadDone = false;
        var tableHandle = Addressables.LoadAssetAsync<TextAsset>("Assets/1_kds/Json/Tables.json").WaitForCompletion();
        var tableTostring = tableHandle.text;
        tableTostring = ES3.DecryptString(tableTostring);
        var jsonData = JObject.Parse(tableTostring);
        foreach (var item in jsonData)
        {
            foreach (var jToken in item.Value)
            {
                var table = jToken.ToObject<JProperty>();
                switch (table.Name)
                {
                                        
                    case "powerTables":
                        PowerTableDict = LoadJson<PowerTableLoader, EPowerTable, PowerTable>(item.Value.ToString()).MakeDict();
                        break;                    
                    case "AbilityTables":
                        AbilityTableDict = LoadJson<AbilityTableLoader, EAbilityTable, AbilityTable>(item.Value.ToString()).MakeDict();
                        break;                    
                    case "characterTables":
                        CharacterTableDict = LoadJson<CharacterTableLoader, ECharacterTable, CharacterTable>(item.Value.ToString()).MakeDict();
                        break;                    
                    case "configTables":
                        ConfigTableDict = LoadJson<ConfigTableLoader, EConfigTable, ConfigTable>(item.Value.ToString()).MakeDict();
                        break;                    
                    case "languageTables":
                        LanguageTableDict = LoadJson<LanguageTableLoader, ELanguageTable, LanguageTable>(item.Value.ToString()).MakeDict();
                        break;                    
                    case "linkTables":
                        LinkTableDict = LoadJson<LinkTableLoader, ELinkTable, LinkTable>(item.Value.ToString()).MakeDict();
                        break;                    
                    case "stageInfoTables":
                        StageInfoTableDict = LoadJson<StageInfoTableLoader, EStageInfoTable, StageInfoTable>(item.Value.ToString()).MakeDict();
                        break;                    
                    case "weatherTables":
                        WeatherTableDict = LoadJson<WeatherTableLoader, EWeatherTable, WeatherTable>(item.Value.ToString()).MakeDict();
                        break;                    
                    case "SkilltrainingTables":
                        SkillTrainingTableDict = LoadJson<SkillTrainingTableLoader, ESkillTrainingTable, SkillTrainingTable>(item.Value.ToString()).MakeDict();
                        break;                    
                    case "patchNoteTables":
                        PatchNoteTableDict = LoadJson<PatchNoteTableLoader, EPatchNoteTable, PatchNoteTable>(item.Value.ToString()).MakeDict();
                        break;                    
                    case "shopTables":
                        ShopTableDict = LoadJson<ShopTableLoader, EShopTable, ShopTable>(item.Value.ToString()).MakeDict();
                        break;                    
                    case "ingameShopTables":
                        InGameShopTableDict = LoadJson<InGameShopTableLoader, EInGameShopTable, InGameShopTable>(item.Value.ToString()).MakeDict();
                        break;                    
                    case "inGameEventTables":
                        InGameEventTableDict = LoadJson<InGameEventTableLoader, EInGameEventTable, InGameEventTable>(item.Value.ToString()).MakeDict();
                        break;
                }
            }
        }
        TMPLinkDetector.UpdateLanguageTablesWithLinks();
        isLoadDone = true;
    }

    static Loader LoadJson<Loader, Key, Value>(string tableStr) where Loader : ILoader<Key, Value>
    {
        var settings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new CustomIntConverter() }
        };
        return JsonConvert.DeserializeObject<Loader>(tableStr, settings);
    }
}
public class CustomIntConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(int) || objectType == typeof(float) || objectType == typeof(BigInteger) || objectType == typeof(bool);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            if (objectType == typeof(int))
            {
                return 0;
            }
            else if (objectType == typeof(float))
            {
                return 0f;
            }
            else if(objectType == typeof(BigInteger))
            {
                return BigInteger.Zero;
            }
            else if(objectType == typeof(bool))
            {
                return false;
            }
        }

        if (reader.Value != null)
        {
            if (objectType == typeof(int))
            {
                if (int.TryParse(reader.Value.ToString(), out int intResult))
                {
                    return intResult;
                }
            }
            else if (objectType == typeof(float))
            {
                if (float.TryParse(reader.Value.ToString(), out float floatResult))
                {
                    return floatResult;
                }
            }
            else if (objectType == typeof(BigInteger))
            {
                string valueStr = reader.Value.ToString();

                // 지수 표기법을 처리하기 위해 double 또는 decimal로 변환
                if (double.TryParse(valueStr, out double doubleResult))
                {
                    // double 값을 BigInteger로 변환
                    return new BigInteger(doubleResult);
                }
                else if (BigInteger.TryParse(valueStr, out BigInteger bigIntegerResult))
                {
                    return bigIntegerResult;
                }
            }
            else if (objectType == typeof(bool))
            {
                if (bool.TryParse(reader.Value.ToString(), out bool boolResult))
                {
                    return boolResult;
                }
            }
        }

        if (objectType == typeof(BigInteger))
        {
            return BigInteger.Zero;
        }
        else if (objectType == typeof(float))
        {
            return 0f;
        }
        else if(objectType == typeof(bool))
        {
            return false;
        }
        return 0;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}
public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}

public enum EPowerTable
{
    [Description("valueTypeDefine")]
	valueTypeDefine = -1227803469,
	[Description("AllMonsterHealth")]
	AllMonsterHealth = -1995023588,
	[Description("AllMonsterHealthIncrease")]
	AllMonsterHealthIncrease = 680836676,
	[Description("AllMonsterHealthAmplification")]
	AllMonsterHealthAmplification = 711088742,
	[Description("AllMonsterHealthSuppression")]
	AllMonsterHealthSuppression = -2028411655,
	[Description("FarmHp")]
	FarmHp = 224289723,
	[Description("FarmHpIncrease")]
	FarmHpIncrease = 956960155,
	[Description("FarmHpAmplification")]
	FarmHpAmplification = 802277231,
	[Description("FarmHpSuppression")]
	FarmHpSuppression = -1104125136,
	[Description("FarmDefense")]
	FarmDefense = -2128107211,
	[Description("FarmDefenseIncrease")]
	FarmDefenseIncrease = 1085915669,
	[Description("FarmDefenseAmplification")]
	FarmDefenseAmplification = 957543005,
	[Description("FarmDefenseSuppression")]
	FarmDefenseSuppression = -1730100402,
	[Description("PetMovementSpeed")]
	PetMovementSpeed = 1332422782,
	[Description("PetMovementSpeedIncrease")]
	PetMovementSpeedIncrease = -527394922,
	[Description("PetMovementSpeedAmplification")]
	PetMovementSpeedAmplification = -1963353396,
	[Description("PetMovementSpeedSuppression")]
	PetMovementSpeedSuppression = -910968361,
	[Description("AllPetMovementSpeed")]
	AllPetMovementSpeed = 1576664997,
	[Description("AllPetMovementSpeedIncrease")]
	AllPetMovementSpeedIncrease = -1572782779,
	[Description("AllPetMovementSpeedAmplification")]
	AllPetMovementSpeedAmplification = 909477037,
	[Description("AllPetMovementSpeedSuppression")]
	AllPetMovementSpeedSuppression = -1431887106,
	[Description("AllMonsterMovementSpeed")]
	AllMonsterMovementSpeed = 141324946,
	[Description("AllMonsterMovementSpeedIncrease")]
	AllMonsterMovementSpeedIncrease = -1787712782,
	[Description("AllMonsterMovementSpeedAmplification")]
	AllMonsterMovementSpeedAmplification = -1914677832,
	[Description("AllMonsterMovementSpeedSuppression")]
	AllMonsterMovementSpeedSuppression = 1952489603,
	[Description("AllMonsterDefense")]
	AllMonsterDefense = -733776818,
	[Description("AllMonsterDefenseIncrease")]
	AllMonsterDefenseIncrease = 1034956454,
	[Description("AllMonsterDefenseAmplification")]
	AllMonsterDefenseAmplification = -2093208580,
	[Description("AllMonsterDefenseSuppression")]
	AllMonsterDefenseSuppression = -272289305,
	[Description("FenceDurabilityRecoverySpeed")]
	FenceDurabilityRecoverySpeed = 1966643303,
	[Description("FenceDurabilityRecoverySpeedIncrease")]
	FenceDurabilityRecoverySpeedIncrease = 1377380191,
	[Description("FenceDurabilityRecoverySpeedAmplification")]
	FenceDurabilityRecoverySpeedAmplification = 168495059,
	[Description("FenceDurabilityRecoverySpeedSuppression")]
	FenceDurabilityRecoverySpeedSuppression = 1565970788,
	[Description("AllMonsterHealthRecoverySpeed")]
	AllMonsterHealthRecoverySpeed = -1592500474,
	[Description("AllMonsterHealthRecoverySpeedIncrease")]
	AllMonsterHealthRecoverySpeedIncrease = 923049086,
	[Description("AllMonsterHealthRecoverySpeedAmplification")]
	AllMonsterHealthRecoverySpeedAmplification = 382318548,
	[Description("AllMonsterHealthRecoverySpeedSuppression")]
	AllMonsterHealthRecoverySpeedSuppression = 150450079,
	[Description("AllMonsterAttackPower")]
	AllMonsterAttackPower = -1858505595,
	[Description("AllMonsterAttackPowerIncrease")]
	AllMonsterAttackPowerIncrease = -1957017371,
	[Description("AllMonsterAttackPowerAmplification")]
	AllMonsterAttackPowerAmplification = -205189299,
	[Description("AllMonsterAttackPowerSuppression")]
	AllMonsterAttackPowerSuppression = 135572382,
	[Description("FarmAttackPower")]
	FarmAttackPower = 1893344498,
	[Description("FarmAttackPowerIncrease")]
	FarmAttackPowerIncrease = 452826386,
	[Description("FarmAttackPowerAmplification")]
	FarmAttackPowerAmplification = -1729712168,
	[Description("FarmAttackPowerSuppression")]
	FarmAttackPowerSuppression = 589633699,
	[Description("FarmAttackSpeed")]
	FarmAttackSpeed = 1891893308,
	[Description("FarmAttackSpeedIncrease")]
	FarmAttackSpeedIncrease = -207488220,
	[Description("FarmAttackSpeedAmplification")]
	FarmAttackSpeedAmplification = -1125255098,
	[Description("FarmAttackSpeedSuppression")]
	FarmAttackSpeedSuppression = 1707303385,
	[Description("PetAttackSpeed")]
	PetAttackSpeed = 192543811,
	[Description("PetAttackSpeedIncrease")]
	PetAttackSpeedIncrease = -345789981,
	[Description("PetAttackSpeedAmplification")]
	PetAttackSpeedAmplification = -1976146697,
	[Description("PetAttackSpeedSuppression")]
	PetAttackSpeedSuppression = -187778408,
	[Description("AllPetAttackSpeed")]
	AllPetAttackSpeed = 1727544104,
	[Description("AllPetAttackSpeedIncrease")]
	AllPetAttackSpeedIncrease = 899505752,
	[Description("AllPetAttackSpeedAmplification")]
	AllPetAttackSpeedAmplification = -1122148614,
	[Description("AllPetAttackSpeedSuppression")]
	AllPetAttackSpeedSuppression = 1653387549,
	[Description("AllMonsterAttackSpeed")]
	AllMonsterAttackSpeed = -1858411017,
	[Description("AllMonsterAttackSpeedIncrease")]
	AllMonsterAttackSpeedIncrease = -1621684305,
	[Description("AllMonsterAttackSpeedAmplification")]
	AllMonsterAttackSpeedAmplification = 1616128067,
	[Description("AllMonsterAttackSpeedSuppression")]
	AllMonsterAttackSpeedSuppression = 338513396,
	[Description("BulletPower")]
	BulletPower = 302561950,
	[Description("BulletPowerIncrease")]
	BulletPowerIncrease = -1698467338,
	[Description("BulletPowerAmplification")]
	BulletPowerAmplification = 11299628,
	[Description("BulletPowerSuppression")]
	BulletPowerSuppression = 875867191,
	[Description("PetPower")]
	PetPower = 1198641501,
	[Description("PetPowerIncrease")]
	PetPowerIncrease = 661620989,
	[Description("PetPowerAmplification")]
	PetPowerAmplification = -403477979,
	[Description("PetPowerSuppression")]
	PetPowerSuppression = -1844552106,
	[Description("AllPetPower")]
	AllPetPower = -1262370506,
	[Description("AllPetPowerIncrease")]
	AllPetPowerIncrease = -2030679506,
	[Description("AllPetPowerAmplification")]
	AllPetPowerAmplification = -285928028,
	[Description("AllPetPowerSuppression")]
	AllPetPowerSuppression = -1466927729,
	[Description("GrenadeAttackPower")]
	GrenadeAttackPower = -754785862,
	[Description("GrenadeAttackPowerIncrease")]
	GrenadeAttackPowerIncrease = 1227058138,
	[Description("GrenadeAttackPowerAmplification")]
	GrenadeAttackPowerAmplification = 396133536,
	[Description("GrenadeAttackPowerSuppression")]
	GrenadeAttackPowerSuppression = -1514627189,
	[Description("FarmProduction")]
	FarmProduction = -1423533010,
	[Description("FarmProductionIncrease")]
	FarmProductionIncrease = -1870385338,
	[Description("FarmProductionAmplification")]
	FarmProductionAmplification = -401932644,
	[Description("FarmProductionSuppression")]
	FarmProductionSuppression = 19522951,
	[Description("Production")]
	Production = -131651694,
	[Description("ProductionIncrease")]
	ProductionIncrease = -1918410766,
	[Description("ProductionAmplification")]
	ProductionAmplification = 1274265784,
	[Description("ProductionSuppression")]
	ProductionSuppression = -430781565,
	[Description("FarmProductionSpeed")]
	FarmProductionSpeed = -509946317,
	[Description("FarmProductionSpeedIncrease")]
	FarmProductionSpeedIncrease = 1045562899,
	[Description("FarmProductionSpeedAmplification")]
	FarmProductionSpeedAmplification = 686433287,
	[Description("FarmProductionSpeedSuppression")]
	FarmProductionSpeedSuppression = -503292280,
	[Description("ProductionSpeed")]
	ProductionSpeed = -1832044473,
	[Description("ProductionSpeedIncrease")]
	ProductionSpeedIncrease = -1096622337,
	[Description("ProductionSpeedAmplification")]
	ProductionSpeedAmplification = 747021427,
	[Description("ProductionSpeedSuppression")]
	ProductionSpeedSuppression = -632221948,
	[Description("Debt")]
	Debt = 4771260,
	[Description("DebtIncrease")]
	DebtIncrease = -1371453276,
	[Description("DebtAmplification")]
	DebtAmplification = 222501574,
	[Description("DebtSuppression")]
	DebtSuppression = -704699303,
	[Description("ChickenProduction")]
	ChickenProduction = -613586283,
	[Description("ChickenProductionIncrease")]
	ChickenProductionIncrease = -1220306123,
	[Description("ChickenProductionAmplification")]
	ChickenProductionAmplification = -596598403,
	[Description("ChickenProductionSuppression")]
	ChickenProductionSuppression = -445435794,
	[Description("LeopardProduction")]
	LeopardProduction = 859749943,
	[Description("LeopardProductionIncrease")]
	LeopardProductionIncrease = -307599121,
	[Description("LeopardProductionAmplification")]
	LeopardProductionAmplification = 1180776323,
	[Description("LeopardProductionSuppression")]
	LeopardProductionSuppression = -975495116,
	[Description("FarmBounty")]
	FarmBounty = -1012993702,
	[Description("FarmBountyIncrease")]
	FarmBountyIncrease = -1528880454,
	[Description("FarmBountyAmplification")]
	FarmBountyAmplification = 18728832,
	[Description("FarmBountySuppression")]
	FarmBountySuppression = 2060387435,
	[Description("FarmBountyProduction")]
	FarmBountyProduction = 779636435,
	[Description("FarmBountyProductionIncrease")]
	FarmBountyProductionIncrease = -2136246157,
	[Description("FarmBountyProductionAmplification")]
	FarmBountyProductionAmplification = 1744019559,
	[Description("FarmBountyProductionSuppression")]
	FarmBountyProductionSuppression = 660806376,
	[Description("FeedingRate")]
	FeedingRate = 956542939,
	[Description("FeedingRateIncrease")]
	FeedingRateIncrease = 752866171,
	[Description("FeedingRateAmplification")]
	FeedingRateAmplification = -515121585,
	[Description("FeedingRateSuppression")]
	FeedingRateSuppression = 922778128,
	[Description("FarmFeedingRate")]
	FarmFeedingRate = -615344977,
	[Description("FarmFeedingRateIncrease")]
	FarmFeedingRateIncrease = 1402783623,
	[Description("FarmFeedingRateAmplification")]
	FarmFeedingRateAmplification = 412970747,
	[Description("FarmFeedingRateSuppression")]
	FarmFeedingRateSuppression = 972908716,
	[Description("AllLivestockMaximumSatiety")]
	AllLivestockMaximumSatiety = -1869345419,
	[Description("AllLivestockMaximumSatietyIncrease")]
	AllLivestockMaximumSatietyIncrease = -1695398315,
	[Description("AllLivestockMaximumSatietyAmplification")]
	AllLivestockMaximumSatietyAmplification = 902052253,
	[Description("AllLivestockMaximumSatietySuppression")]
	AllLivestockMaximumSatietySuppression = -1999624050,
	[Description("FeedingSatiety")]
	FeedingSatiety = 392132600,
	[Description("FeedingSatietyIncrease")]
	FeedingSatietyIncrease = -1424181784,
	[Description("FeedingSatietyAmplification")]
	FeedingSatietyAmplification = -371156054,
	[Description("FeedingSatietySuppression")]
	FeedingSatietySuppression = 776713709,
	[Description("GrenadeCost")]
	GrenadeCost = -338956484,
	[Description("GrenadeCostIncrease")]
	GrenadeCostIncrease = 1531636260,
	[Description("GrenadeCostAmplification")]
	GrenadeCostAmplification = -1266521786,
	[Description("GrenadeCostSuppression")]
	GrenadeCostSuppression = 1774195417,
	[Description("GrenadeArea")]
	GrenadeArea = -339145880,
	[Description("GrenadeAreaIncrease")]
	GrenadeAreaIncrease = 838257816,
	[Description("GrenadeAreaAmplification")]
	GrenadeAreaAmplification = -683591110,
	[Description("GrenadeAreaSuppression")]
	GrenadeAreaSuppression = 148792925,
	[Description("BulletSize")]
	BulletSize = -1514284528,
	[Description("BulletSizeIncrease")]
	BulletSizeIncrease = 177062496,
	[Description("BulletSizeAmplification")]
	BulletSizeAmplification = 248463586,
	[Description("BulletSizeSuppression")]
	BulletSizeSuppression = -1687693723,
	[Description("OreDamage")]
	OreDamage = -946828246,
	[Description("OreDamageIncrease")]
	OreDamageIncrease = -1306879414,
	[Description("OreDamageAmplification")]
	OreDamageAmplification = 745958448,
	[Description("OreDamageSuppression")]
	OreDamageSuppression = -492168197,
	[Description("AbilityDiceCost")]
	AbilityDiceCost = 867648169,
	[Description("AbilityDiceCostIncrease")]
	AbilityDiceCostIncrease = 2101773425,
	[Description("AbilityDiceCostAmplification")]
	AbilityDiceCostAmplification = 441222745,
	[Description("AbilityDiceCostSuppression")]
	AbilityDiceCostSuppression = -718008214,
	[Description("NaturaldisasterSpeed")]
	NaturaldisasterSpeed = -1598630672,
	[Description("NaturaldisasterSpeedIncrease")]
	NaturaldisasterSpeedIncrease = -170060800,
	[Description("NaturaldisasterSpeedAmplification")]
	NaturaldisasterSpeedAmplification = 736453506,
	[Description("NaturaldisasterSpeedSuppression")]
	NaturaldisasterSpeedSuppression = 74479621,
	[Description("MonsterAppearSpeed")]
	MonsterAppearSpeed = 1558302805,
	[Description("MonsterAppearSpeedIncrease")]
	MonsterAppearSpeedIncrease = 463045109,
	[Description("MonsterAppearSpeedAmplification")]
	MonsterAppearSpeedAmplification = -543829955,
	[Description("MonsterAppearSpeedSuppression")]
	MonsterAppearSpeedSuppression = 1883447854,
	[Description("WantedCriminalBounty")]
	WantedCriminalBounty = -2116785852,
	[Description("GoldenOrgeBounty")]
	GoldenOrgeBounty = -238584498,
	[Description("EliteOrcPerWave")]
	EliteOrcPerWave = -1051526848,
	[Description("CurrentMoneyAmplification")]
	CurrentMoneyAmplification = -470061052,
	[Description("Money")]
	Money = 133513537,
	[Description("Loan")]
	Loan = 4527437,
	[Description("StoneThrowChanceOnFeeding")]
	StoneThrowChanceOnFeeding = -1469435221,
	[Description("GrenadeChanceOnShoot")]
	GrenadeChanceOnShoot = -204270975,
	[Description("ExtraBulletChanceOnShoot")]
	ExtraBulletChanceOnShoot = -481463099,
	[Description("ContinuousGrenadePurchase")]
	ContinuousGrenadePurchase = -1752295931,
	[Description("BulletPenetrationChance")]
	BulletPenetrationChance = 736082586,
	[Description("GrainAcquisition")]
	GrainAcquisition = -374967591,
	[Description("MilkAcquisition")]
	MilkAcquisition = 280321275,
	[Description("GemAcquisition")]
	GemAcquisition = -1994686759,
	[Description("AdditionalWaveOpportunities")]
	AdditionalWaveOpportunities = -597402750,
	[Description("FenceCounterattack")]
	FenceCounterattack = 1678013562,
	[Description("BountyChestIncrease")]
	BountyChestIncrease = -1545625939,
	[Description("BulletCriticalChance")]
	BulletCriticalChance = -778479968,
	[Description("SummonBattleGolem")]
	SummonBattleGolem = 696661568,
	[Description("SummonGoldenSheep")]
	SummonGoldenSheep = 1429704722,
	[Description("SummonGoldenChicken")]
	SummonGoldenChicken = -1930186598,
	[Description("SummonWhiteLeopard")]
	SummonWhiteLeopard = -287637404,
	[Description("SummonMotherChicken")]
	SummonMotherChicken = 256608108,
	[Description("SummonTurkey")]
	SummonTurkey = 883960348,
	[Description("SummonGolem")]
	SummonGolem = -1379219982,
	[Description("PoisonRain")]
	PoisonRain = -29584207,
	[Description("PetAttackPowerProportionalToFarmAttack")]
	PetAttackPowerProportionalToFarmAttack = 1362237159,
	[Description("MeatDropBounty")]
	MeatDropBounty = -207332508,
	[Description("AutomaticProduceCollection")]
	AutomaticProduceCollection = 1672016064,
	[Description("ButcheryProduction")]
	ButcheryProduction = 13017148,
	[Description("ProducePerCycle")]
	ProducePerCycle = 1028394236,
	[Description("SummonBundle")]
	SummonBundle = 335032042,
	[Description("MeatDropChance")]
	MeatDropChance = -176405051,
	[Description("ExtraBulletChance100CooldownSpeed")]
	ExtraBulletChance100CooldownSpeed = -1572896720,
	[Description("GrenadeSplitDamage")]
	GrenadeSplitDamage = -724767614,
	[Description("BulletCriticalAdditionalDamage")]
	BulletCriticalAdditionalDamage = -1286079092,
	[Description("CastleHolding")]
	CastleHolding = -1852262508,
	[Description("FarmHealthProportionalToPetHealth")]
	FarmHealthProportionalToPetHealth = -1813508984,
	[Description("PetNaturalHealingBasic")]
	PetNaturalHealingBasic = 1476558981,
	[Description("PetRevivalSpeedBasic")]
	PetRevivalSpeedBasic = -9952926,
	[Description("LivestockResuscitationSpeed")]
	LivestockResuscitationSpeed = 91311715,
	[Description("FenceRepairWhenAttack")]
	FenceRepairWhenAttack = -2143040947,
	[Description("AdditionalAbilityChance")]
	AdditionalAbilityChance = -1728425858,
	[Description("AddStartAbility")]
	AddStartAbility = 658700934,
	[Description("KingStoneChanceAmplify")]
	KingStoneChanceAmplify = -236457161,
	[Description("CompoundInterest")]
	CompoundInterest = -965993834,
	[Description("TimeLimit")]
	TimeLimit = 326374945,
	
}

public enum EAbilityTable
{
    [Description("valueTypeDefine")]
	valueTypeDefine = -1227803469,
	[Description("OneMoreShot")]
	OneMoreShot = 810103726,
	[Description("IncreasedAttackSpeed")]
	IncreasedAttackSpeed = 1183854902,
	[Description("Penetrate")]
	Penetrate = 762309557,
	[Description("WantedCriminal")]
	WantedCriminal = 1702279001,
	[Description("IncreasesAllAttackPower")]
	IncreasesAllAttackPower = -1462568354,
	[Description("GrenadeWhenAttacking")]
	GrenadeWhenAttacking = 1705760405,
	[Description("IncreasedGrenadeAttack")]
	IncreasedGrenadeAttack = -1580631023,
	[Description("IncreasedBulletAttack")]
	IncreasedBulletAttack = 408209321,
	[Description("IncreasedPetAttack")]
	IncreasedPetAttack = 1886452488,
	[Description("ChickenProduction")]
	ChickenProduction = -613586283,
	[Description("IncreasedProduction")]
	IncreasedProduction = 1872664004,
	[Description("ReduceDebt")]
	ReduceDebt = 1040113058,
	[Description("LeopardProduction")]
	LeopardProduction = 859749943,
	[Description("IncreasedNumberOfFeeders")]
	IncreasedNumberOfFeeders = 1344368735,
	[Description("EliteMonster")]
	EliteMonster = 910319486,
	[Description("KingStone")]
	KingStone = -2015187395,
	[Description("IncreaseInMoneyYouHave")]
	IncreaseInMoneyYouHave = 2029869573,
	[Description("IncreasedSatiety")]
	IncreasedSatiety = -995527276,
	[Description("IncreasedProductionSpeed")]
	IncreasedProductionSpeed = 910785369,
	[Description("Loan")]
	Loan = 4527437,
	[Description("AbilityGrain")]
	AbilityGrain = 1400211460,
	[Description("IncreasedMovementSpeed")]
	IncreasedMovementSpeed = -459214665,
	[Description("AbilityMilk")]
	AbilityMilk = -924831662,
	[Description("ReducedStamina")]
	ReducedStamina = 113890590,
	[Description("GrenadePriceReduced")]
	GrenadePriceReduced = -1020890620,
	[Description("IncreasedFenceStamina")]
	IncreasedFenceStamina = -627762297,
	[Description("DefenseDecrease")]
	DefenseDecrease = 1985122533,
	[Description("WallCounterAttack")]
	WallCounterAttack = -173445639,
	[Description("FenceAutomaticRecovery")]
	FenceAutomaticRecovery = -1499175450,
	[Description("AdvancedGrenade")]
	AdvancedGrenade = -1155571003,
	[Description("IncreasedBounty")]
	IncreasedBounty = -199927472,
	[Description("AdditionalWaveButton")]
	AdditionalWaveButton = 951129343,
	[Description("IncreaseAllProfits")]
	IncreaseAllProfits = 1779610209,
	[Description("PetAttackSpeed")]
	PetAttackSpeed = 192543811,
	
}

public enum ECharacterTable
{
    [Description("valueTypeDefine")]
	valueTypeDefine = -1227803469,
	[Description("Dusick")]
	Dusick = 305423268,
	[Description("BangMaendeok")]
	BangMaendeok = 487369773,
	[Description("Jophiro")]
	Jophiro = 456232882,
	[Description("JoPalmae")]
	JoPalmae = 272179188,
	[Description("GeumYungjae")]
	GeumYungjae = 722958082,
	[Description("DoBalho")]
	DoBalho = 668310590,
	[Description("SaYoosa")]
	SaYoosa = -1066708322,
	[Description("YoonHeon")]
	YoonHeon = 1429854814,
	[Description("PyoBeoma")]
	PyoBeoma = 1780082919,
	[Description("KangMujin")]
	KangMujin = 367650923,
	[Description("BanAri")]
	BanAri = 110888596,
	[Description("Chick")]
	Chick = 143329393,
	[Description("Chicken")]
	Chicken = 300593944,
	[Description("BlackChicken")]
	BlackChicken = -362185087,
	[Description("Darkness")]
	Darkness = 2404242,
	[Description("GoldChicken")]
	GoldChicken = 1877588074,
	[Description("Panther")]
	Panther = -282960231,
	[Description("WhitePanther")]
	WhitePanther = -1601603334,
	[Description("MotherChicken")]
	MotherChicken = -810211761,
	[Description("Turkey")]
	Turkey = -183129023,
	[Description("Wall")]
	Wall = 4264659,
	[Description("LittleBunny")]
	LittleBunny = -355285743,
	[Description("Bunny")]
	Bunny = 142506221,
	[Description("Sheep")]
	Sheep = 128549172,
	[Description("Ram")]
	Ram = 132513,
	[Description("Bull")]
	Bull = 4596776,
	[Description("Ox")]
	Ox = 4690,
	[Description("Dog")]
	Dog = 154227,
	[Description("Hound")]
	Hound = 136373279,
	[Description("Puggy")]
	Puggy = 128718073,
	[Description("Pug")]
	Pug = 133945,
	[Description("Scarecrow")]
	Scarecrow = 968922356,
	[Description("Pony")]
	Pony = 4177443,
	[Description("Horse")]
	Horse = 136369254,
	[Description("Monkey")]
	Monkey = -156052680,
	[Description("BossMonkey")]
	BossMonkey = 1827540795,
	[Description("Goat")]
	Goat = 4747386,
	[Description("WildGoat")]
	WildGoat = -1005585874,
	[Description("Bat")]
	Bat = 147880,
	[Description("MohawkBat")]
	MohawkBat = 859570425,
	[Description("Feeder")]
	Feeder = 229145658,
	[Description("Carpenter")]
	Carpenter = -851833495,
	[Description("Auriel")]
	Auriel = -9304147,
	[Description("Butcher")]
	Butcher = -518556548,
	[Description("BomberMan")]
	BomberMan = 226441830,
	[Description("Amazon")]
	Amazon = -31945125,
	[Description("Log")]
	Log = 146043,
	[Description("GoldLog")]
	GoldLog = -294749955,
	[Description("SilverBullet")]
	SilverBullet = -593834092,
	[Description("GoldBullet")]
	GoldBullet = -1798538559,
	[Description("Donkey")]
	Donkey = 299790931,
	[Description("Burro")]
	Burro = 142478459,
	[Description("Golem")]
	Golem = 147179217,
	[Description("BattleGolem")]
	BattleGolem = -1248138629,
	[Description("GoldenSheep")]
	GoldenSheep = -1382942019,
	[Description("BundleOrigin")]
	BundleOrigin = 566228889,
	[Description("BountyChestOrigin")]
	BountyChestOrigin = 2002844063,
	[Description("BabyOrc")]
	BabyOrc = -844800001,
	[Description("AdultOrc")]
	AdultOrc = -349010283,
	[Description("Ogre")]
	Ogre = 4500634,
	[Description("RedBabyOrc")]
	RedBabyOrc = 1193107942,
	[Description("RedAdultOrc")]
	RedAdultOrc = -487511574,
	[Description("RedOgre")]
	RedOgre = 2016585321,
	[Description("GoldenOgre")]
	GoldenOgre = -736749391,
	[Description("EliteOrc")]
	EliteOrc = -1602480226,
	[Description("Rat")]
	Rat = 132536,
	[Description("Crawler")]
	Crawler = 582587161,
	[Description("Boar")]
	Boar = 4594587,
	[Description("ClubOgre")]
	ClubOgre = 672386274,
	[Description("BackOgre")]
	BackOgre = -464481877,
	[Description("ShotPutOgre")]
	ShotPutOgre = -86094805,
	[Description("ArcherOrc")]
	ArcherOrc = -2027336856,
	[Description("BungeeOrc")]
	BungeeOrc = 846792011,
	[Description("ChemicalOrc")]
	ChemicalOrc = 1209885551,
	[Description("HydeOrc")]
	HydeOrc = -1530061445,
	[Description("IronOrc")]
	IronOrc = 113371133,
	[Description("MageOrc")]
	MageOrc = -125825371,
	[Description("MindEyeOrc")]
	MindEyeOrc = -50252404,
	[Description("NinjaOrc")]
	NinjaOrc = 1126833861,
	[Description("priestOrc")]
	priestOrc = 1579903020,
	[Description("PumpkinHeadOrc")]
	PumpkinHeadOrc = -1545174125,
	[Description("ShopLifterOrc")]
	ShopLifterOrc = 1633966965,
	[Description("SuperManOrc")]
	SuperManOrc = -892030810,
	[Description("ThiefOrc")]
	ThiefOrc = 392515109,
	[Description("NecromancerOrc")]
	NecromancerOrc = -1472681130,
	[Description("WolfOrc")]
	WolfOrc = -1609726053,
	[Description("Skeleton")]
	Skeleton = -503504990,
	[Description("Tornado")]
	Tornado = -600822346,
	[Description("Wasp")]
	Wasp = 4265522,
	[Description("FarmRat")]
	FarmRat = -1636955420,
	[Description("MeatBug")]
	MeatBug = -245323892,
	[Description("OrcTotem")]
	OrcTotem = -1654746870,
	[Description("Yeti")]
	Yeti = 3911032,
	[Description("Meteor")]
	Meteor = -147117525,
	[Description("Asteroid")]
	Asteroid = -1664524808,
	[Description("VolcanoWorm")]
	VolcanoWorm = 1909199950,
	[Description("WallLava")]
	WallLava = 18654957,
	[Description("Ghost")]
	Ghost = 147022072,
	[Description("IronRock")]
	IronRock = -780596814,
	[Description("BronzeRock")]
	BronzeRock = -320832872,
	[Description("SilverRock")]
	SilverRock = -832399617,
	[Description("GoldRock")]
	GoldRock = -547604322,
	[Description("PlatinumRock")]
	PlatinumRock = -1134253492,
	[Description("DiaRock")]
	DiaRock = 526686880,
	[Description("BlackRock")]
	BlackRock = -1863904733,
	
}

public enum EConfigTable
{
    [Description("valueTypeDefine")]
	valueTypeDefine = -1227803469,
	[Description("masterID")]
	masterID = -457037462,
	[Description("playTime")]
	playTime = 1433429054,
	[Description("initdebt")]
	initdebt = 1603355690,
	[Description("debtIncreaseTime")]
	debtIncreaseTime = -3579301,
	[Description("CompoundInterestDefault")]
	CompoundInterestDefault = 99337341,
	[Description("StartChickenCount")]
	StartChickenCount = -674602857,
	[Description("des")]
	des = 178761,
	[Description("researchInitCost")]
	researchInitCost = 540750145,
	[Description("timeReward_GemPerHour")]
	timeReward_GemPerHour = 827883036,
	[Description("timeReward_MilkPerHour")]
	timeReward_MilkPerHour = 1569305584,
	[Description("timeReward_GrainPerHour")]
	timeReward_GrainPerHour = 860985480,
	[Description("firstClearMilkReward_Iron")]
	firstClearMilkReward_Iron = -1625280873,
	[Description("firstClearMilkReward_Dia")]
	firstClearMilkReward_Dia = -1022257195,
	[Description("firstClearGemReward_Iron")]
	firstClearGemReward_Iron = -1696702465,
	[Description("firstClearGemReward_Dia")]
	firstClearGemReward_Dia = 83802701,
	[Description("firstClearGrainReward_Iron")]
	firstClearGrainReward_Iron = 2043936779,
	[Description("firstClearGrainReward_Dia")]
	firstClearGrainReward_Dia = -349697335,
	[Description("abilityRerollCost")]
	abilityRerollCost = 353629902,
	[Description("abilityTimer")]
	abilityTimer = -87932974,
	[Description("chickCost")]
	chickCost = 1596469694,
	[Description("pantherCost")]
	pantherCost = -599543274,
	[Description("wallRepairCost")]
	wallRepairCost = -287640957,
	[Description("buyAbilityCost")]
	buyAbilityCost = 1305022974,
	[Description("abilityCostAmpPerBuy")]
	abilityCostAmpPerBuy = 857006961,
	[Description("debtRepayCost")]
	debtRepayCost = 45618396,
	[Description("grenadeCost")]
	grenadeCost = 1319736988,
	[Description("grenadeDmgMultiplier")]
	grenadeDmgMultiplier = -152544452,
	[Description("wallRepairDuration")]
	wallRepairDuration = -1832102808,
	[Description("wallRepairRatePerSec")]
	wallRepairRatePerSec = -1399894726,
	[Description("missionOpenTime")]
	missionOpenTime = 380183288,
	[Description("enemyReinforcePerWave")]
	enemyReinforcePerWave = 2071602134,
	[Description("StartMoney")]
	StartMoney = 2037274711,
	[Description("grainReward")]
	grainReward = -1617362683,
	[Description("medalUpBtn")]
	medalUpBtn = -424570571,
	[Description("inGameBuyInflation")]
	inGameBuyInflation = -2090210342,
	[Description("inGameBuyInflationPerTime")]
	inGameBuyInflationPerTime = 1768127828,
	[Description("MonsterOneWhenSpawn")]
	MonsterOneWhenSpawn = -113386670,
	[Description("MonsterTWOWhenSpawn")]
	MonsterTWOWhenSpawn = -971881604,
	[Description("MonsterAllWhenSpawn")]
	MonsterAllWhenSpawn = -683001595,
	[Description("GhostSpawnMinTime")]
	GhostSpawnMinTime = 823506448,
	[Description("GhostSpawnMaxTime")]
	GhostSpawnMaxTime = 1073433098,
	[Description("GhostGemSpawnNum")]
	GhostGemSpawnNum = 1926157078,
	[Description("GhostCoinReward")]
	GhostCoinReward = 1560960986,
	[Description("GhostGrainReward")]
	GhostGrainReward = 2125433652,
	[Description("GhostMilkReward")]
	GhostMilkReward = -364318484,
	[Description("GhostGemReward")]
	GhostGemReward = 2147059504,
	[Description("GhostDissapearTime")]
	GhostDissapearTime = 1500415719,
	[Description("BossBountyAmp")]
	BossBountyAmp = 1537479909,
	[Description("masterAbilityLv")]
	masterAbilityLv = 344269247,
	[Description("defaultSeal")]
	defaultSeal = -1365987443,
	[Description("maxBuySeal")]
	maxBuySeal = -1250672678,
	[Description("dayAdsLimit")]
	dayAdsLimit = -1210153080,
	[Description("powerPowPerStar")]
	powerPowPerStar = -383604115,
	
}

public enum ELanguageTable
{
    [Description("valueTypeDefine")]
	valueTypeDefine = -1227803469,
	[Description("Seal")]
	Seal = 4142072,
	[Description("attack")]
	attack = 961085471,
	[Description("attack_Increase")]
	attack_Increase = 1312340726,
	[Description("attack_amp")]
	attack_amp = 1852732896,
	[Description("Boss")]
	Boss = 4594120,
	[Description("hp")]
	hp = 5407,
	[Description("hp_recovery")]
	hp_recovery = 1746736427,
	[Description("BuyCount")]
	BuyCount = 1210298654,
	[Description("StageTimeInflation")]
	StageTimeInflation = 2088916350,
	[Description("ExtraBulletChance")]
	ExtraBulletChance = 1352053103,
	[Description("GrenadeChanceOnShoot")]
	GrenadeChanceOnShoot = -204270975,
	[Description("KingStoneChance")]
	KingStoneChance = 908970697,
	[Description("StageLvRewardAmp")]
	StageLvRewardAmp = -1806787878,
	[Description("Repair")]
	Repair = -351204168,
	[Description("Bounty")]
	Bounty = 119807390,
	[Description("Production")]
	Production = -131651694,
	[Description("ProductionSpeed")]
	ProductionSpeed = -1832044473,
	[Description("ProductionCycle")]
	ProductionCycle = -1847746660,
	[Description("RapidFire")]
	RapidFire = -1571609757,
	[Description("RapidFireCycle")]
	RapidFireCycle = 953702671,
	[Description("ResuscitationCycle")]
	ResuscitationCycle = -1742567090,
	[Description("ResurrectCycle")]
	ResurrectCycle = -303182482,
	[Description("FeedingCycle")]
	FeedingCycle = -426249255,
	[Description("FeedingSpeed")]
	FeedingSpeed = -410249726,
	[Description("FarmFeedingSpeed")]
	FarmFeedingSpeed = -1896381610,
	[Description("AllPetMoveSpeed")]
	AllPetMoveSpeed = 19160493,
	[Description("KingStoneChanceMultiplierDict")]
	KingStoneChanceMultiplierDict = 738291096,
	[Description("ChickenProductionMultiplierDict")]
	ChickenProductionMultiplierDict = -1316784300,
	[Description("PantherProductionMultiplierDict")]
	PantherProductionMultiplierDict = -1853603123,
	[Description("AllPetAttackMultiplierDict")]
	AllPetAttackMultiplierDict = 647644102,
	[Description("AllPetAttackSpeedMultiplierDict")]
	AllPetAttackSpeedMultiplierDict = 2098984957,
	[Description("AllPetMoveSpeedMultiplierDict")]
	AllPetMoveSpeedMultiplierDict = 1077058316,
	[Description("FarmProductionSpeedMultiplierDict")]
	FarmProductionSpeedMultiplierDict = 1672261650,
	[Description("AllMonsterHpMultiplierDict")]
	AllMonsterHpMultiplierDict = -1434488661,
	[Description("AllMonsterMoveSpeedMultiplierDict")]
	AllMonsterMoveSpeedMultiplierDict = -1696823113,
	[Description("AllMonsterDeffenceDict")]
	AllMonsterDeffenceDict = 308426046,
	[Description("AllMonsterAttackSpeedMultiplierDict")]
	AllMonsterAttackSpeedMultiplierDict = -149362522,
	[Description("MaxSatietyMultiplierDict")]
	MaxSatietyMultiplierDict = -998883481,
	[Description("GranadeDamageMultiplierDict")]
	GranadeDamageMultiplierDict = 125794981,
	[Description("FarmAttackSpeedMultiplierDict")]
	FarmAttackSpeedMultiplierDict = -1854477599,
	[Description("GranadeRangeDict")]
	GranadeRangeDict = 70333122,
	[Description("oreStoneDamageMultiplierDict")]
	oreStoneDamageMultiplierDict = -1230756854,
	[Description("BulletSizeMultiplierDict")]
	BulletSizeMultiplierDict = 450784965,
	[Description("MeatDropBountyDictDesc")]
	MeatDropBountyDictDesc = -1809391361,
	[Description("Bulletpenetrationprobability")]
	Bulletpenetrationprobability = -564270929,
	[Description("FeedingSpeedMultiplierDict")]
	FeedingSpeedMultiplierDict = -162662825,
	[Description("attackDict")]
	attackDict = 1852036231,
	[Description("BulletDamageMultiplierDict")]
	BulletDamageMultiplierDict = -1475707347,
	[Description("FeedSatietyDict")]
	FeedSatietyDict = -1599173502,
	[Description("AllMonsterAttackMultiplierDict")]
	AllMonsterAttackMultiplierDict = 612650679,
	[Description("FarmDefenceDict")]
	FarmDefenceDict = -949808111,
	[Description("naturalDisasterOccurrence")]
	naturalDisasterOccurrence = -1521061840,
	[Description("MonsterAppearanceInterval")]
	MonsterAppearanceInterval = 299672950,
	[Description("GrenadeDamage")]
	GrenadeDamage = 600029676,
	[Description("KingStoneDamage")]
	KingStoneDamage = 1222843866,
	[Description("abilityRerollCostDict")]
	abilityRerollCostDict = -379200426,
	[Description("WallHp")]
	WallHp = -196628309,
	[Description("CompoundInterestDict")]
	CompoundInterestDict = 657421838,
	[Description("BulletDamage")]
	BulletDamage = 440338808,
	[Description("Bulletcriticalhitprobability")]
	Bulletcriticalhitprobability = -1241398564,
	[Description("proportionalToInflation")]
	proportionalToInflation = -516954547,
	[Description("stamina")]
	stamina = -1594690926,
	[Description("stamina_recovery")]
	stamina_recovery = 744508508,
	[Description("attackSpeed")]
	attackSpeed = 1589338912,
	[Description("moveSpeed")]
	moveSpeed = 534880181,
	[Description("FarmAttackPower")]
	FarmAttackPower = 1893344498,
	[Description("FarmHealth")]
	FarmHealth = -1422694371,
	[Description("FarmProduction")]
	FarmProduction = -1423533010,
	[Description("FarmBounty")]
	FarmBounty = -1012993702,
	[Description("LastingEffect")]
	LastingEffect = -1115741618,
	[Description("EquipEffect")]
	EquipEffect = -2039661578,
	[Description("EquipFarmer")]
	EquipFarmer = -2014272308,
	[Description("EquipAssistant")]
	EquipAssistant = -1738804421,
	[Description("EquipPet")]
	EquipPet = -1121298780,
	[Description("Naturaldisaster")]
	Naturaldisaster = -443473457,
	[Description("StageLvInflation")]
	StageLvInflation = -1136554731,
	[Description("CharacterInfo")]
	CharacterInfo = -2091976204,
	[Description("Hunger")]
	Hunger = -45840306,
	[Description("AssistantDesc")]
	AssistantDesc = -2010777426,
	[Description("Assistant")]
	Assistant = 1110752961,
	[Description("Farmer")]
	Farmer = 224290938,
	[Description("Grain")]
	Grain = 147317236,
	[Description("Milk")]
	Milk = 4312834,
	[Description("limitAbility")]
	limitAbility = -778165350,
	[Description("FarmerAndPet")]
	FarmerAndPet = 1599562556,
	[Description("Satiety")]
	Satiety = -1025587170,
	[Description("failToLoadAd")]
	failToLoadAd = -1269512173,
	[Description("lackOfKey")]
	lackOfKey = -1812933188,
	[Description("Selectfarmer")]
	Selectfarmer = 851247090,
	[Description("Dice")]
	Dice = 4775054,
	[Description("cancle")]
	cancle = 1116690083,
	[Description("Population")]
	Population = -646986798,
	[Description("tutorial")]
	tutorial = -892763607,
	[Description("WanderingMerchant")]
	WanderingMerchant = 1762260280,
	[Description("merchantAppeared")]
	merchantAppeared = 1677554695,
	[Description("abilityCard")]
	abilityCard = 1243698465,
	[Description("rerollAbilityCard")]
	rerollAbilityCard = 768752155,
	[Description("abilityCardDesc")]
	abilityCardDesc = 18962254,
	[Description("rerollAbilityCardDesc")]
	rerollAbilityCardDesc = 662969992,
	[Description("Prey")]
	Prey = 4157339,
	[Description("LoanSharksProposal")]
	LoanSharksProposal = -1143415491,
	[Description("LoanSharksProposalDesc")]
	LoanSharksProposalDesc = 1029913194,
	[Description("petSlotUnlockStage")]
	petSlotUnlockStage = 1552743244,
	[Description("petSlotUnlockByShop")]
	petSlotUnlockByShop = 1397782561,
	[Description("shop")]
	shop = 5163651,
	[Description("noty_touchLockedPet")]
	noty_touchLockedPet = -137330236,
	[Description("production")]
	production = -256496974,
	[Description("growSpeed")]
	growSpeed = 567572171,
	[Description("bounty")]
	bounty = 1089389246,
	[Description("startCoin")]
	startCoin = 1070270866,
	[Description("lobbyCoin")]
	lobbyCoin = -823173550,
	[Description("startDept")]
	startDept = 1070061212,
	[Description("server")]
	server = 655543622,
	[Description("now")]
	now = 176653,
	[Description("gpgsLogin")]
	gpgsLogin = 261352995,
	[Description("gameCenter")]
	gameCenter = -573684810,
	[Description("guestLogin")]
	guestLogin = -2129311440,
	[Description("cantSaveGuest")]
	cantSaveGuest = 1995636750,
	[Description("cantLoadGuest")]
	cantLoadGuest = 72047499,
	[Description("SaveDone")]
	SaveDone = -1792163348,
	[Description("LoadDone")]
	LoadDone = -2118538781,
	[Description("waitAndRetry")]
	waitAndRetry = -506045809,
	[Description("Inflation")]
	Inflation = 1255891947,
	[Description("needClearStage")]
	needClearStage = -569035484,
	[Description("needClearBeforeStage")]
	needClearBeforeStage = -1120709637,
	[Description("skillTraining")]
	skillTraining = -395815600,
	[Description("negotiationSkill")]
	negotiationSkill = -1657366527,
	[Description("slaughterSkill")]
	slaughterSkill = 636870389,
	[Description("huntingSkill")]
	huntingSkill = 1539283999,
	[Description("architecturalSkill")]
	architecturalSkill = 885947273,
	[Description("negotiationSkill_Desc2")]
	negotiationSkill_Desc2 = 2040652227,
	[Description("slaughterSkill_Desc2")]
	slaughterSkill_Desc2 = -505552481,
	[Description("huntingSkill_Desc2")]
	huntingSkill_Desc2 = -864759903,
	[Description("architecturalSkill_Desc2")]
	architecturalSkill_Desc2 = 662839611,
	[Description("firstClearReward_Iron")]
	firstClearReward_Iron = 1639030552,
	[Description("firstClearReward_dia")]
	firstClearReward_dia = 1854024464,
	[Description("ClearReward_repeat")]
	ClearReward_repeat = 128551887,
	[Description("addCharacterDesc")]
	addCharacterDesc = -1936591230,
	[Description("PetInventory")]
	PetInventory = 1654486762,
	[Description("watchADAndPlay")]
	watchADAndPlay = -1490031800,
	[Description("spring")]
	spring = 644339704,
	[Description("summer")]
	summer = 639775528,
	[Description("fall")]
	fall = 5719224,
	[Description("winter")]
	winter = 780740436,
	[Description("volcano")]
	volcano = 1905952747,
	[Description("chaos")]
	chaos = 174843917,
	[Description("reward")]
	reward = 626332578,
	[Description("weather")]
	weather = -1689491731,
	[Description("flood")]
	flood = 177680465,
	[Description("thunder")]
	thunder = 675081299,
	[Description("storm")]
	storm = 159217006,
	[Description("meatbug")]
	meatbug = -253270580,
	[Description("wasp")]
	wasp = 5274450,
	[Description("farmRat")]
	farmRat = -1408456124,
	[Description("orcTotem")]
	orcTotem = 958354794,
	[Description("blizzard")]
	blizzard = 769755001,
	[Description("meteor")]
	meteor = 822464331,
	[Description("lava")]
	lava = 5299233,
	[Description("volcanoWorm")]
	volcanoWorm = -1882579794,
	[Description("flood_Desc")]
	flood_Desc = 1795930527,
	[Description("thunder_Desc")]
	thunder_Desc = -431517759,
	[Description("storm_Desc")]
	storm_Desc = -1146302438,
	[Description("meatbug_Desc")]
	meatbug_Desc = 1072890808,
	[Description("wasp_Desc")]
	wasp_Desc = 506965246,
	[Description("farmRat_Desc")]
	farmRat_Desc = -1990438352,
	[Description("orcTotem_Desc")]
	orcTotem_Desc = 1592725654,
	[Description("blizzard_Desc")]
	blizzard_Desc = -2012610649,
	[Description("meteor_Desc")]
	meteor_Desc = 1745636313,
	[Description("lava_Desc")]
	lava_Desc = 1322386319,
	[Description("volcanoWorm_Desc")]
	volcanoWorm_Desc = -1305062822,
	[Description("yeti")]
	yeti = 4681624,
	[Description("yeti_Desc")]
	yeti_Desc = 2127786724,
	[Description("Dusick")]
	Dusick = 305423268,
	[Description("BangMaendeok")]
	BangMaendeok = 487369773,
	[Description("Jophiro")]
	Jophiro = 456232882,
	[Description("JoPalmae")]
	JoPalmae = 272179188,
	[Description("GeumYungjae")]
	GeumYungjae = 722958082,
	[Description("DoBalho")]
	DoBalho = 668310590,
	[Description("SaYoosa")]
	SaYoosa = -1066708322,
	[Description("YoonHeon")]
	YoonHeon = 1429854814,
	[Description("PyoBeoma")]
	PyoBeoma = 1780082919,
	[Description("KangMujin")]
	KangMujin = 367650923,
	[Description("BanAri")]
	BanAri = 110888596,
	[Description("Chick")]
	Chick = 143329393,
	[Description("Chicken")]
	Chicken = 300593944,
	[Description("BlackChicken")]
	BlackChicken = -362185087,
	[Description("Darkness")]
	Darkness = 2404242,
	[Description("GoldChicken")]
	GoldChicken = 1877588074,
	[Description("Panther")]
	Panther = -282960231,
	[Description("WhitePanther")]
	WhitePanther = -1601603334,
	[Description("MotherChicken")]
	MotherChicken = -810211761,
	[Description("Turkey")]
	Turkey = -183129023,
	[Description("Wall")]
	Wall = 4264659,
	[Description("LittleBunny")]
	LittleBunny = -355285743,
	[Description("Bunny")]
	Bunny = 142506221,
	[Description("Sheep")]
	Sheep = 128549172,
	[Description("Ram")]
	Ram = 132513,
	[Description("Bull")]
	Bull = 4596776,
	[Description("Ox")]
	Ox = 4690,
	[Description("Dog")]
	Dog = 154227,
	[Description("Hound")]
	Hound = 136373279,
	[Description("Puggy")]
	Puggy = 128718073,
	[Description("Pug")]
	Pug = 133945,
	[Description("Scarecrow")]
	Scarecrow = 968922356,
	[Description("Pony")]
	Pony = 4177443,
	[Description("Horse")]
	Horse = 136369254,
	[Description("Monkey")]
	Monkey = -156052680,
	[Description("BossMonkey")]
	BossMonkey = 1827540795,
	[Description("Goat")]
	Goat = 4747386,
	[Description("WildGoat")]
	WildGoat = -1005585874,
	[Description("Bat")]
	Bat = 147880,
	[Description("MohawkBat")]
	MohawkBat = 859570425,
	[Description("Feeder")]
	Feeder = 229145658,
	[Description("Carpenter")]
	Carpenter = -851833495,
	[Description("Auriel")]
	Auriel = -9304147,
	[Description("Butcher")]
	Butcher = -518556548,
	[Description("BomberMan")]
	BomberMan = 226441830,
	[Description("Amazon")]
	Amazon = -31945125,
	[Description("Log")]
	Log = 146043,
	[Description("GoldLog")]
	GoldLog = -294749955,
	[Description("SilverBullet")]
	SilverBullet = -593834092,
	[Description("GoldBullet")]
	GoldBullet = -1798538559,
	[Description("Donkey")]
	Donkey = 299790931,
	[Description("Burro")]
	Burro = 142478459,
	[Description("Golem")]
	Golem = 147179217,
	[Description("BattleGolem")]
	BattleGolem = -1248138629,
	[Description("GoldenSheep")]
	GoldenSheep = -1382942019,
	[Description("BundleOrigin")]
	BundleOrigin = 566228889,
	[Description("BountyChestOrigin")]
	BountyChestOrigin = 2002844063,
	[Description("BabyOrc")]
	BabyOrc = -844800001,
	[Description("AdultOrc")]
	AdultOrc = -349010283,
	[Description("Ogre")]
	Ogre = 4500634,
	[Description("RedBabyOrc")]
	RedBabyOrc = 1193107942,
	[Description("RedAdultOrc")]
	RedAdultOrc = -487511574,
	[Description("RedOgre")]
	RedOgre = 2016585321,
	[Description("GoldenOgre")]
	GoldenOgre = -736749391,
	[Description("EliteOrc")]
	EliteOrc = -1602480226,
	[Description("Rat")]
	Rat = 132536,
	[Description("Crawler")]
	Crawler = 582587161,
	[Description("Boar")]
	Boar = 4594587,
	[Description("ClubOgre")]
	ClubOgre = 672386274,
	[Description("BackOgre")]
	BackOgre = -464481877,
	[Description("ShotPutOgre")]
	ShotPutOgre = -86094805,
	[Description("ArcherOrc")]
	ArcherOrc = -2027336856,
	[Description("BungeeOrc")]
	BungeeOrc = 846792011,
	[Description("ChemicalOrc")]
	ChemicalOrc = 1209885551,
	[Description("HydeOrc")]
	HydeOrc = -1530061445,
	[Description("IronOrc")]
	IronOrc = 113371133,
	[Description("MageOrc")]
	MageOrc = -125825371,
	[Description("MindEyeOrc")]
	MindEyeOrc = -50252404,
	[Description("NinjaOrc")]
	NinjaOrc = 1126833861,
	[Description("priestOrc")]
	priestOrc = 1579903020,
	[Description("PumpkinHeadOrc")]
	PumpkinHeadOrc = -1545174125,
	[Description("ShopLifterOrc")]
	ShopLifterOrc = 1633966965,
	[Description("SuperManOrc")]
	SuperManOrc = -892030810,
	[Description("ThiefOrc")]
	ThiefOrc = 392515109,
	[Description("NecromancerOrc")]
	NecromancerOrc = -1472681130,
	[Description("WolfOrc")]
	WolfOrc = -1609726053,
	[Description("Skeleton")]
	Skeleton = -503504990,
	[Description("Tornado")]
	Tornado = -600822346,
	[Description("Wasp")]
	Wasp = 4265522,
	[Description("FarmRat")]
	FarmRat = -1636955420,
	[Description("MeatBug")]
	MeatBug = -245323892,
	[Description("OrcTotem")]
	OrcTotem = -1654746870,
	[Description("Yeti")]
	Yeti = 3911032,
	[Description("Meteor")]
	Meteor = -147117525,
	[Description("Asteroid")]
	Asteroid = -1664524808,
	[Description("VolcanoWorm")]
	VolcanoWorm = 1909199950,
	[Description("WallLava")]
	WallLava = 18654957,
	[Description("Ghost")]
	Ghost = 147022072,
	[Description("IronRock")]
	IronRock = -780596814,
	[Description("BronzeRock")]
	BronzeRock = -320832872,
	[Description("SilverRock")]
	SilverRock = -832399617,
	[Description("GoldRock")]
	GoldRock = -547604322,
	[Description("PlatinumRock")]
	PlatinumRock = -1134253492,
	[Description("DiaRock")]
	DiaRock = 526686880,
	[Description("BlackRock")]
	BlackRock = -1863904733,
	[Description("Dusick_Desc")]
	Dusick_Desc = 820542736,
	[Description("BangMaendeok_Desc")]
	BangMaendeok_Desc = -188730901,
	[Description("Jophiro_Desc")]
	Jophiro_Desc = 1941904606,
	[Description("JoPalmae_Desc")]
	JoPalmae_Desc = -1241250240,
	[Description("GeumYungjae_Desc")]
	GeumYungjae_Desc = 1097574798,
	[Description("DoBalho_Desc")]
	DoBalho_Desc = 402587850,
	[Description("SaYoosa_Desc")]
	SaYoosa_Desc = -896172630,
	[Description("YoonHeon_Desc")]
	YoonHeon_Desc = -1524511126,
	[Description("PyoBeoma_Desc")]
	PyoBeoma_Desc = 1737654005,
	[Description("KangMujin_Desc")]
	KangMujin_Desc = -896103175,
	[Description("BanAri_Desc")]
	BanAri_Desc = -243442592,
	[Description("Chick_Desc")]
	Chick_Desc = 27319743,
	[Description("Chicken_Desc")]
	Chicken_Desc = 707682148,
	[Description("BlackChicken_Desc")]
	BlackChicken_Desc = 262849903,
	[Description("Darkness_Desc")]
	Darkness_Desc = 324935742,
	[Description("GoldChicken_Desc")]
	GoldChicken_Desc = -970289770,
	[Description("Panther_Desc")]
	Panther_Desc = -1487694649,
	[Description("WhitePanther_Desc")]
	WhitePanther_Desc = -1159750906,
	[Description("MotherChicken_Desc")]
	MotherChicken_Desc = 463680605,
	[Description("Turkey_Desc")]
	Turkey_Desc = -614109393,
	[Description("Wall_Desc")]
	Wall_Desc = 592935233,
	[Description("LittleBunny_Desc")]
	LittleBunny_Desc = 1406598495,
	[Description("Bunny_Desc")]
	Bunny_Desc = -210231893,
	[Description("Sheep_Desc")]
	Sheep_Desc = -795193984,
	[Description("Ram_Desc")]
	Ram_Desc = 1189005839,
	[Description("Bull_Desc")]
	Bull_Desc = -375198700,
	[Description("Ox_Desc")]
	Ox_Desc = 1068398078,
	[Description("Dog_Desc")]
	Dog_Desc = 108895201,
	[Description("Hound_Desc")]
	Hound_Desc = 1340418573,
	[Description("Puggy_Desc")]
	Puggy_Desc = -1304698329,
	[Description("Pug_Desc")]
	Pug_Desc = -719449241,
	[Description("Scarecrow_Desc")]
	Scarecrow_Desc = 266187072,
	[Description("Pony_Desc")]
	Pony_Desc = -980708687,
	[Description("Horse_Desc")]
	Horse_Desc = 2026148882,
	[Description("Monkey_Desc")]
	Monkey_Desc = -740549884,
	[Description("BossMonkey_Desc")]
	BossMonkey_Desc = 2042666153,
	[Description("Goat_Desc")]
	Goat_Desc = -649112698,
	[Description("WildGoat_Desc")]
	WildGoat_Desc = 1518265562,
	[Description("Bat_Desc")]
	Bat_Desc = -1235403116,
	[Description("MohawkBat_Desc")]
	MohawkBat_Desc = 532641319,
	[Description("Feeder_Desc")]
	Feeder_Desc = 1001406790,
	[Description("Carpenter_Desc")]
	Carpenter_Desc = 1275378615,
	[Description("Auriel_Desc")]
	Auriel_Desc = -320338325,
	[Description("Butcher_Desc")]
	Butcher_Desc = -1130808,
	[Description("BomberMan_Desc")]
	BomberMan_Desc = 745926162,
	[Description("Amazon_Desc")]
	Amazon_Desc = 15297225,
	[Description("Log_Desc")]
	Log_Desc = 2045901033,
	[Description("GoldLog_Desc")]
	GoldLog_Desc = 1620548507,
	[Description("SilverBullet_Desc")]
	SilverBullet_Desc = 676606816,
	[Description("GoldBullet_Desc")]
	GoldBullet_Desc = -30691153,
	[Description("Donkey_Desc")]
	Donkey_Desc = -2102801983,
	[Description("Burro_Desc")]
	Burro_Desc = -536254743,
	[Description("Golem_Desc")]
	Golem_Desc = -352898273,
	[Description("BattleGolem_Desc")]
	BattleGolem_Desc = -666416919,
	[Description("GoldenSheep_Desc")]
	GoldenSheep_Desc = 14365275,
	[Description("BundleOrigin_Desc")]
	BundleOrigin_Desc = -1538417721,
	[Description("BountyChestOrigin_Desc")]
	BountyChestOrigin_Desc = 416711821,
	[Description("BabyOrc_Desc")]
	BabyOrc_Desc = -1087233299,
	[Description("AdultOrc_Desc")]
	AdultOrc_Desc = -271267501,
	[Description("Ogre_Desc")]
	Ogre_Desc = 389841958,
	[Description("RedBabyOrc_Desc")]
	RedBabyOrc_Desc = -1392216430,
	[Description("RedAdultOrc_Desc")]
	RedAdultOrc_Desc = -846328810,
	[Description("RedOgre_Desc")]
	RedOgre_Desc = -779894089,
	[Description("GoldenOgre_Desc")]
	GoldenOgre_Desc = 1752052991,
	[Description("EliteOrc_Desc")]
	EliteOrc_Desc = -1623730006,
	[Description("Rat_Desc")]
	Rat_Desc = 1890089092,
	[Description("Crawler_Desc")]
	Crawler_Desc = -1333186489,
	[Description("Boar_Desc")]
	Boar_Desc = 1418642185,
	[Description("ClubOgre_Desc")]
	ClubOgre_Desc = -2062481170,
	[Description("BackOgre_Desc")]
	BackOgre_Desc = -486855623,
	[Description("ShotPutOgre_Desc")]
	ShotPutOgre_Desc = 475922873,
	[Description("ArcherOrc_Desc")]
	ArcherOrc_Desc = 974810196,
	[Description("BungeeOrc_Desc")]
	BungeeOrc_Desc = 2083182041,
	[Description("ChemicalOrc_Desc")]
	ChemicalOrc_Desc = -341847683,
	[Description("HydeOrc_Desc")]
	HydeOrc_Desc = -1619200535,
	[Description("IronOrc_Desc")]
	IronOrc_Desc = -300839781,
	[Description("MageOrc_Desc")]
	MageOrc_Desc = 1342053955,
	[Description("MindEyeOrc_Desc")]
	MindEyeOrc_Desc = 1467305080,
	[Description("NinjaOrc_Desc")]
	NinjaOrc_Desc = -1548092573,
	[Description("priestOrc_Desc")]
	priestOrc_Desc = -2086107752,
	[Description("PumpkinHeadOrc_Desc")]
	PumpkinHeadOrc_Desc = 1583816193,
	[Description("ShopLifterOrc_Desc")]
	ShopLifterOrc_Desc = -236713933,
	[Description("SuperManOrc_Desc")]
	SuperManOrc_Desc = 1277165906,
	[Description("ThiefOrc_Desc")]
	ThiefOrc_Desc = 334794435,
	[Description("NecromancerOrc_Desc")]
	NecromancerOrc_Desc = 389374050,
	[Description("WolfOrc_Desc")]
	WolfOrc_Desc = -997533943,
	[Description("Skeleton_Desc")]
	Skeleton_Desc = -795013458,
	[Description("Tornado_Desc")]
	Tornado_Desc = 1182474818,
	[Description("Wasp_Desc")]
	Wasp_Desc = -471882146,
	[Description("FarmRat_Desc")]
	FarmRat_Desc = -837190320,
	[Description("MeatBug_Desc")]
	MeatBug_Desc = -823321992,
	[Description("OrcTotem_Desc")]
	OrcTotem_Desc = -1878199882,
	[Description("Yeti_Desc")]
	Yeti_Desc = -196753340,
	[Description("Meteor_Desc")]
	Meteor_Desc = 1297867705,
	[Description("Asteroid_Desc")]
	Asteroid_Desc = 1462921156,
	[Description("VolcanoWorm_Desc")]
	VolcanoWorm_Desc = 1876121210,
	[Description("WallLava_Desc")]
	WallLava_Desc = 747024299,
	[Description("Ghost_Desc")]
	Ghost_Desc = 1979860164,
	[Description("IronRock_Desc")]
	IronRock_Desc = -1055661346,
	[Description("BronzeRock_Desc")]
	BronzeRock_Desc = 1328105444,
	[Description("SilverRock_Desc")]
	SilverRock_Desc = -2027992083,
	[Description("GoldRock_Desc")]
	GoldRock_Desc = -62629974,
	[Description("PlatinumRock_Desc")]
	PlatinumRock_Desc = -24342728,
	[Description("DiaRock_Desc")]
	DiaRock_Desc = -1195285268,
	[Description("BlackRock_Desc")]
	BlackRock_Desc = -598163791,
	[Description("TotalProfit")]
	TotalProfit = -1908960535,
	[Description("Mission1_Desc")]
	Mission1_Desc = -1621067552,
	[Description("Mission2_Desc")]
	Mission2_Desc = -1529448699,
	[Description("Mission3_Desc")]
	Mission3_Desc = -1441781470,
	[Description("Mission4_Desc")]
	Mission4_Desc = -1593980833,
	[Description("Mission5_Desc")]
	Mission5_Desc = -1742727300,
	[Description("Mission6_Desc")]
	Mission6_Desc = -1651599487,
	[Description("Mission7_Desc")]
	Mission7_Desc = -1563933250,
	[Description("Mission8_Desc")]
	Mission8_Desc = -1228560709,
	[Description("Mission9_Desc")]
	Mission9_Desc = -1377258008,
	[Description("Mission10_Desc")]
	Mission10_Desc = -1812574424,
	[Description("Mission11_Desc")]
	Mission11_Desc = -1900298245,
	[Description("Reward1_Desc")]
	Reward1_Desc = -919371875,
	[Description("Reward2_Desc")]
	Reward2_Desc = -1010990728,
	[Description("Reward3_Desc")]
	Reward3_Desc = -862227893,
	[Description("Reward4_Desc")]
	Reward4_Desc = -953855434,
	[Description("Reward5_Desc")]
	Reward5_Desc = -1041522663,
	[Description("Reward6_Desc")]
	Reward6_Desc = -1132649484,
	[Description("Reward7_Desc")]
	Reward7_Desc = -983903017,
	[Description("Reward8_Desc")]
	Reward8_Desc = -1075514190,
	[Description("Reward9_Desc")]
	Reward9_Desc = -1163181419,
	[Description("Reward10_Desc")]
	Reward10_Desc = -383920433,
	[Description("Reward11_Desc")]
	Reward11_Desc = -359283284,
	[Description("Reward12_Desc")]
	Reward12_Desc = -441604367,
	[Description("Complete")]
	Complete = -1920078994,
	[Description("MissionProgress")]
	MissionProgress = 1345282182,
	[Description("NoMoney")]
	NoMoney = -73882994,
	[Description("EggHint")]
	EggHint = -1795669567,
	[Description("PreyHint")]
	PreyHint = -312462292,
	[Description("PantherInitHint")]
	PantherInitHint = -454158760,
	[Description("WhitePantherInitHint")]
	WhitePantherInitHint = 547238071,
	[Description("WallHalfHpHint")]
	WallHalfHpHint = -654286381,
	[Description("BlackPortionHint")]
	BlackPortionHint = -1370268042,
	[Description("FirstWaveHint")]
	FirstWaveHint = 1560340771,
	[Description("FirstWave2Hint")]
	FirstWave2Hint = 1030689521,
	[Description("ArcherOrcInitHint")]
	ArcherOrcInitHint = -2049461339,
	[Description("FloodInitHint")]
	FloodInitHint = -1150023696,
	[Description("PumpkinOrcInitHint")]
	PumpkinOrcInitHint = -1996012580,
	[Description("NineMinuteHint")]
	NineMinuteHint = 137060768,
	[Description("Dept0Hint")]
	Dept0Hint = -1319482603,
	[Description("WallReviveHint")]
	WallReviveHint = 229999511,
	[Description("startMoney")]
	startMoney = -1179239497,
	[Description("TwoMinuteHint")]
	TwoMinuteHint = -1560706076,
	[Description("MissionHint")]
	MissionHint = 336247052,
	[Description("OrcRocksHint")]
	OrcRocksHint = -367966168,
	[Description("FarmRatHint")]
	FarmRatHint = 1358939807,
	[Description("MeatBugHint")]
	MeatBugHint = 2052074615,
	[Description("OrcTotemHint")]
	OrcTotemHint = -1168415375,
	[Description("BlizzardHint")]
	BlizzardHint = -723134354,
	[Description("GhostHint")]
	GhostHint = 1171470451,
	[Description("GameGoalHint")]
	GameGoalHint = -1591034629,
	[Description("AbilityHint")]
	AbilityHint = -924980504,
	[Description("cheaterHint")]
	cheaterHint = -1351666558,
	[Description("passiveEffectHint")]
	passiveEffectHint = -1685784518,
	[Description("GameLoseTip")]
	GameLoseTip = 808505521,
	[Description("Exit_Desc")]
	Exit_Desc = -1291590027,
	[Description("Victory_Desc")]
	Victory_Desc = -315547403,
	[Description("Lose_Desc")]
	Lose_Desc = -301457440,
	[Description("LoadingSound")]
	LoadingSound = 2041720466,
	[Description("LoadingTime")]
	LoadingTime = 343233598,
	[Description("LoadingObject")]
	LoadingObject = -783270716,
	[Description("LoadingImage")]
	LoadingImage = 2054324664,
	[Description("LoadingUserData")]
	LoadingUserData = 1253647276,
	[Description("LoadingDone")]
	LoadingDone = 343712385,
	[Description("LogOut")]
	LogOut = 55745289,
	[Description("Start")]
	Start = 127711705,
	[Description("Recieve")]
	Recieve = 2018578954,
	[Description("FalseLogin")]
	FalseLogin = -873823733,
	[Description("PleaseLoginGoogle")]
	PleaseLoginGoogle = 2101153769,
	[Description("acquirePet")]
	acquirePet = 1786731124,
	[Description("lackOfMilk")]
	lackOfMilk = -366176046,
	[Description("petLvUp")]
	petLvUp = 1106611035,
	[Description("CantRecieveYet")]
	CantRecieveYet = -511728622,
	[Description("timeRemainYet")]
	timeRemainYet = 1852740366,
	[Description("needGem")]
	needGem = 232203864,
	[Description("freefirstreroll_Desc")]
	freefirstreroll_Desc = 1941543937,
	[Description("noaddpackage")]
	noaddpackage = 1455784941,
	[Description("gameSpeed")]
	gameSpeed = 1243583342,
	[Description("firstgem")]
	firstgem = -1464386820,
	[Description("freefirstreroll")]
	freefirstreroll = 142427539,
	[Description("dailygem")]
	dailygem = -709032653,
	[Description("dailymilk")]
	dailymilk = -505481413,
	[Description("dailygrain")]
	dailygrain = 1520054959,
	[Description("smallgem")]
	smallgem = 784174629,
	[Description("middlegem")]
	middlegem = 884761685,
	[Description("largegem")]
	largegem = -2135231511,
	[Description("megagem")]
	megagem = -263011762,
	[Description("grandgem")]
	grandgem = 2055242280,
	[Description("milk_small")]
	milk_small = -1022668154,
	[Description("milk_middle")]
	milk_middle = -1358820114,
	[Description("grain_small")]
	grain_small = -1979658968,
	[Description("grain_middle")]
	grain_middle = -2045362528,
	[Description("gameSpeed_Desc")]
	gameSpeed_Desc = -1045597670,
	[Description("adGem_Desc")]
	adGem_Desc = 418994547,
	[Description("adGrain_Desc")]
	adGrain_Desc = 1409995207,
	[Description("adMilk_Desc")]
	adMilk_Desc = 2093214263,
	[Description("smallgem_Desc")]
	smallgem_Desc = 163860163,
	[Description("middlegem_Desc")]
	middlegem_Desc = -1824021741,
	[Description("largegem_Desc")]
	largegem_Desc = 495969591,
	[Description("megagem_Desc")]
	megagem_Desc = 4810874,
	[Description("grandgem_Desc")]
	grandgem_Desc = 724782612,
	[Description("milk_small_Desc")]
	milk_small_Desc = 1079820978,
	[Description("milk_middle_Desc")]
	milk_middle_Desc = 1859840154,
	[Description("grain_small_Desc")]
	grain_small_Desc = -953556204,
	[Description("grain_middle_Desc")]
	grain_middle_Desc = -1284095764,
	[Description("dailygem_Desc")]
	dailygem_Desc = 458434977,
	[Description("dailymilk_Desc")]
	dailymilk_Desc = 2132849833,
	[Description("dailygrain_Desc")]
	dailygrain_Desc = -1244263747,
	[Description("desc_IncreaseByMaxClearLv")]
	desc_IncreaseByMaxClearLv = -611481729,
	[Description("Unlock_PetSlot1")]
	Unlock_PetSlot1 = -2106958854,
	[Description("Unlock_PetSlot2")]
	Unlock_PetSlot2 = -2106958855,
	[Description("Unlock_PetSlot1_Desc")]
	Unlock_PetSlot1_Desc = 1628739590,
	[Description("Unlock_PetSlot2_Desc")]
	Unlock_PetSlot2_Desc = 1716648743,
	[Description("AbilitySeal")]
	AbilitySeal = -924295384,
	[Description("AbilitySeal_Desc")]
	AbilitySeal_Desc = -466775788,
	[Description("Tutorial_Intro0")]
	Tutorial_Intro0 = -300959110,
	[Description("Tutorial_Intro1")]
	Tutorial_Intro1 = -300959109,
	[Description("tutorial_BuyReroll")]
	tutorial_BuyReroll = 1707094362,
	[Description("tutorial_BuyChick")]
	tutorial_BuyChick = -922729628,
	[Description("tutorial_CilckGameSpeed")]
	tutorial_CilckGameSpeed = -2095859531,
	[Description("tutorial_FeedMode")]
	tutorial_FeedMode = -1143744987,
	[Description("tutorial_ShotMode")]
	tutorial_ShotMode = 577236613,
	[Description("tutorial_GrenadeMode")]
	tutorial_GrenadeMode = -1441017717,
	[Description("tutorial_WallRepair")]
	tutorial_WallRepair = 479854011,
	[Description("tutorial_BuyPanther")]
	tutorial_BuyPanther = -1135559332,
	[Description("tutorial_BuyAbilityCard")]
	tutorial_BuyAbilityCard = 906699492,
	[Description("tutorial_DebtRepay")]
	tutorial_DebtRepay = 437959478,
	[Description("tutorial_BuyMedal")]
	tutorial_BuyMedal = -939806515,
	[Description("tutorial_ClickTimeReward")]
	tutorial_ClickTimeReward = 165295998,
	[Description("tutorial_ClickFarmerAndPet")]
	tutorial_ClickFarmerAndPet = 1496860191,
	[Description("tutorial_EquipDusick")]
	tutorial_EquipDusick = 1785280695,
	[Description("tutorial_EquipDog")]
	tutorial_EquipDog = -769808482,
	[Description("tutorial_ClickEquipPet")]
	tutorial_ClickEquipPet = -320349497,
	[Description("tutorial_touch10Sec_Feed")]
	tutorial_touch10Sec_Feed = -340416812,
	[Description("tutorial_touch5Sec_Attack")]
	tutorial_touch5Sec_Attack = 820182888,
	[Description("tutorial_getMasterAbility")]
	tutorial_getMasterAbility = 4769192,
	[Description("pleaseUpdate")]
	pleaseUpdate = 867724706,
	[Description("acquirePet_StageDesc")]
	acquirePet_StageDesc = -1208827144,
	[Description("acquirePet_EvolveDesc")]
	acquirePet_EvolveDesc = -1010510245,
	[Description("acquirePet_TutorialDesc")]
	acquirePet_TutorialDesc = 774956618,
	[Description("acquirePet_ShopDesc")]
	acquirePet_ShopDesc = -2139468188,
	[Description("lackOfGrain")]
	lackOfGrain = 1538666372,
	[Description("skillTraininglevelUpInfo")]
	skillTraininglevelUpInfo = 1081709429,
	[Description("lvup")]
	lvup = 5320762,
	[Description("DebtRepay")]
	DebtRepay = -58183653,
	[Description("icon_medal_0")]
	icon_medal_0 = 1927756737,
	[Description("icon_medal_1")]
	icon_medal_1 = 1927756736,
	[Description("icon_medal_2")]
	icon_medal_2 = 1927756739,
	[Description("icon_medal_3")]
	icon_medal_3 = 1927756738,
	[Description("icon_medal_4")]
	icon_medal_4 = 1927756741,
	[Description("icon_medal_5")]
	icon_medal_5 = 1927756740,
	[Description("needInternet")]
	needInternet = 1029800656,
	[Description("special")]
	special = -1476916812,
	[Description("gem")]
	gem = 185620,
	[Description("grainAndMilk")]
	grainAndMilk = -1671997406,
	[Description("pet")]
	pet = 159322,
	[Description("addPet")]
	addPet = 945791695,
	[Description("addFarmer")]
	addFarmer = 1663712869,
	[Description("addAssistant")]
	addAssistant = -1194347208,
	[Description("selectPetToPlay")]
	selectPetToPlay = -2013025777,
	[Description("monsterAppearing")]
	monsterAppearing = 171639664,
	[Description("storeToClick")]
	storeToClick = -1633282517,
	[Description("CheckReallyGoOut")]
	CheckReallyGoOut = 1365276022,
	[Description("yes")]
	yes = 151016,
	[Description("no")]
	no = 5702,
	[Description("patchNote")]
	patchNote = -1563184185,
	[Description("buy")]
	buy = 181065,
	[Description("buyCheckAgain")]
	buyCheckAgain = -2028629607,
	[Description("timeReward_Desc")]
	timeReward_Desc = 1689547751,
	[Description("requireGrainForNextLv")]
	requireGrainForNextLv = -1267288041,
	[Description("OneMoreShot")]
	OneMoreShot = 810103726,
	[Description("IncreasedAttackSpeed")]
	IncreasedAttackSpeed = 1183854902,
	[Description("Penetrate")]
	Penetrate = 762309557,
	[Description("WantedCriminal")]
	WantedCriminal = 1702279001,
	[Description("IncreasesAllAttackPower")]
	IncreasesAllAttackPower = -1462568354,
	[Description("GrenadeWhenAttacking")]
	GrenadeWhenAttacking = 1705760405,
	[Description("IncreasedGrenadeAttack")]
	IncreasedGrenadeAttack = -1580631023,
	[Description("IncreasedBulletAttack")]
	IncreasedBulletAttack = 408209321,
	[Description("IncreasedPetAttack")]
	IncreasedPetAttack = 1886452488,
	[Description("ChickenProduction")]
	ChickenProduction = -613586283,
	[Description("IncreasedProduction")]
	IncreasedProduction = 1872664004,
	[Description("ReduceDebt")]
	ReduceDebt = 1040113058,
	[Description("LeopardProduction")]
	LeopardProduction = 859749943,
	[Description("IncreasedNumberOfFeeders")]
	IncreasedNumberOfFeeders = 1344368735,
	[Description("EliteMonster")]
	EliteMonster = 910319486,
	[Description("KingStone")]
	KingStone = -2015187395,
	[Description("IncreaseInMoneyYouHave")]
	IncreaseInMoneyYouHave = 2029869573,
	[Description("IncreasedSatiety")]
	IncreasedSatiety = -995527276,
	[Description("IncreasedProductionSpeed")]
	IncreasedProductionSpeed = 910785369,
	[Description("Loan")]
	Loan = 4527437,
	[Description("AbilityGrain")]
	AbilityGrain = 1400211460,
	[Description("IncreasedMovementSpeed")]
	IncreasedMovementSpeed = -459214665,
	[Description("AbilityMilk")]
	AbilityMilk = -924831662,
	[Description("ReducedStamina")]
	ReducedStamina = 113890590,
	[Description("GrenadePriceReduced")]
	GrenadePriceReduced = -1020890620,
	[Description("IncreasedFenceStamina")]
	IncreasedFenceStamina = -627762297,
	[Description("DefenseDecrease")]
	DefenseDecrease = 1985122533,
	[Description("WallCounterAttack")]
	WallCounterAttack = -173445639,
	[Description("FenceAutomaticRecovery")]
	FenceAutomaticRecovery = -1499175450,
	[Description("AdvancedGrenade")]
	AdvancedGrenade = -1155571003,
	[Description("IncreasedBounty")]
	IncreasedBounty = -199927472,
	[Description("AdditionalWaveButton")]
	AdditionalWaveButton = 951129343,
	[Description("IncreaseAllProfits")]
	IncreaseAllProfits = 1779610209,
	[Description("PetAttackSpeed")]
	PetAttackSpeed = 192543811,
	[Description("rewardList")]
	rewardList = -2020307486,
	[Description("ads_attainDouble")]
	ads_attainDouble = -202195790,
	[Description("tipView")]
	tipView = 709220701,
	[Description("continueDesc")]
	continueDesc = 1495813135,
	[Description("selectedAbilityCardList")]
	selectedAbilityCardList = -644654516,
	[Description("AbilityList")]
	AbilityList = -924860713,
	[Description("specialAbility_Desc")]
	specialAbility_Desc = -1893584134,
	[Description("language")]
	language = -1555445031,
	[Description("goToCafe")]
	goToCafe = -2000681199,
	[Description("credit")]
	credit = 1134442344,
	[Description("setting")]
	setting = -1155148343,
	[Description("timeReward")]
	timeReward = -1424926023,
	[Description("selectLoginType")]
	selectLoginType = -72101974,
	[Description("guestCantUseStore")]
	guestCantUseStore = 2116590683,
	[Description("accountLink")]
	accountLink = 2029136584,
	[Description("accountLinkFalse")]
	accountLinkFalse = 1391325345,
	[Description("accountLinked")]
	accountLinked = 85105959,
	[Description("specialStage")]
	specialStage = 525133738,
	[Description("eventStage")]
	eventStage = -134051555,
	[Description("infiniteStage")]
	infiniteStage = -527044261,
	[Description("cantEnterEvent")]
	cantEnterEvent = 1050592783,
	[Description("requireLvUp")]
	requireLvUp = -1164296803,
	[Description("prepareingNextStage")]
	prepareingNextStage = -1887713029,
	[Description("requireMaxStage")]
	requireMaxStage = 2082982256,
	[Description("sealInfo")]
	sealInfo = -2005732392,
	[Description("changeMission")]
	changeMission = 1252831751,
	[Description("open")]
	open = 5528531,
	[Description("stage")]
	stage = 159233951,
	[Description("StageInfo")]
	StageInfo = 23331963,
	[Description("StageEnd")]
	StageEnd = -691988118,
	[Description("StageClear")]
	StageClear = 721135070,
	[Description("Difficulty")]
	Difficulty = 1082287592,
	[Description("Coin")]
	Coin = 4620780,
	[Description("Reinforce")]
	Reinforce = 562151638,
	[Description("CoinReinforce")]
	CoinReinforce = -845955725,
	[Description("Modification")]
	Modification = 864532691,
	[Description("Adventurer")]
	Adventurer = 647954837,
	[Description("Lobby")]
	Lobby = 140349081,
	[Description("Enter")]
	Enter = 141396535,
	[Description("CheckLobbyAgain")]
	CheckLobbyAgain = 1871434537,
	[Description("NeedToClearCurrentLv")]
	NeedToClearCurrentLv = -1492751136,
	[Description("TrapConfiguration")]
	TrapConfiguration = -1336928136,
	[Description("ProjectileConfiguration")]
	ProjectileConfiguration = -245920652,
	[Description("Level")]
	Level = 140508555,
	[Description("Speed_Normal")]
	Speed_Normal = -1541590262,
	[Description("Speed_Double")]
	Speed_Double = -1240614204,
	[Description("Speed_Pause")]
	Speed_Pause = 1217543963,
	[Description("DefaultValue")]
	DefaultValue = 389068001,
	[Description("BasicValue")]
	BasicValue = -695084458,
	[Description("FinalValue")]
	FinalValue = -448106676,
	[Description("AttackPower")]
	AttackPower = -1705156346,
	[Description("Health")]
	Health = -60597927,
	[Description("Naturalhealing")]
	Naturalhealing = -475128316,
	[Description("Mana")]
	Mana = 4321090,
	[Description("MovementSpeed")]
	MovementSpeed = 209557933,
	[Description("AttackInterval")]
	AttackInterval = -642660984,
	[Description("AttackSpeed")]
	AttackSpeed = -1697725632,
	[Description("RangedAttackSpeed")]
	RangedAttackSpeed = 425320099,
	[Description("Defense")]
	Defense = 409592545,
	[Description("MentalStrength")]
	MentalStrength = -1089701513,
	[Description("Vulnerability")]
	Vulnerability = -109267981,
	[Description("Poison")]
	Poison = -280526869,
	[Description("Burn")]
	Burn = 4596080,
	[Description("Slow")]
	Slow = 4142944,
	[Description("Fear")]
	Fear = 4707857,
	[Description("Knockback")]
	Knockback = -1941572756,
	[Description("Essence")]
	Essence = -1204008841,
	[Description("Essencebox")]
	Essencebox = -1355584386,
	[Description("Increase")]
	Increase = -1360648769,
	[Description("Amplify")]
	Amplify = -1006473675,
	[Description("Suppress")]
	Suppress = 949763122,
	[Description("Desperation")]
	Desperation = -466199019,
	[Description("Rage")]
	Rage = 4108240,
	[Description("Crisis")]
	Crisis = 157098396,
	[Description("subValueDesc_Resistant")]
	subValueDesc_Resistant = 363250785,
	[Description("subValueDesc_RafidFirePeriod")]
	subValueDesc_RafidFirePeriod = 1060585551,
	[Description("subValueDesc_FeedingPeriod")]
	subValueDesc_FeedingPeriod = -664834273,
	[Description("subValueDesc_AttackPeriod")]
	subValueDesc_AttackPeriod = 1504269091,
	[Description("subValueDesc_ProductionPeriod")]
	subValueDesc_ProductionPeriod = -472002970,
	[Description("subValueDesc_ResuscitationPeriod")]
	subValueDesc_ResuscitationPeriod = 480602930,
	[Description("subValueDesc_NaturaldisasterSpeed")]
	subValueDesc_NaturaldisasterSpeed = 911978547,
	[Description("subValueDesc_Revive")]
	subValueDesc_Revive = -1154980557,
	[Description("subValueDesc_MonsterAppearPeriod")]
	subValueDesc_MonsterAppearPeriod = -769562354,
	[Description("EnhancementStone")]
	EnhancementStone = 759716478,
	[Description("HeroSetting")]
	HeroSetting = 587378565,
	[Description("StatusEffect")]
	StatusEffect = -1991268614,
	[Description("Status")]
	Status = -335906223,
	[Description("Equipping")]
	Equipping = -399495023,
	[Description("LackOfMoney")]
	LackOfMoney = -978783759,
	[Description("RecieveX2")]
	RecieveX2 = -1470841248,
	[Description("AlreadyGain")]
	AlreadyGain = -1675988968,
	[Description("stoneslot")]
	stoneslot = 1226223486,
	[Description("inventoryslot")]
	inventoryslot = -1636933007,
	[Description("noaddpackage_Desc")]
	noaddpackage_Desc = 1054864043,
	[Description("noaddpackage_Desc2")]
	noaddpackage_Desc2 = -1658953081,
	[Description("firstgem_Desc")]
	firstgem_Desc = -576255928,
	[Description("gemPurchase_Desc")]
	gemPurchase_Desc = -445542409,
	[Description("accountLink_NoLink")]
	accountLink_NoLink = 1396026790,
	[Description("accountLinked_NoLink")]
	accountLinked_NoLink = 1033689511,
	[Description("CharacterlevelUpInfo")]
	CharacterlevelUpInfo = 1863874097,
	[Description("WhenKill")]
	WhenKill = 934927923,
	[Description("AutoSkill")]
	AutoSkill = -340487859,
	[Description("AutoTab")]
	AutoTab = -286384817,
	[Description("AutoCollect")]
	AutoCollect = 398436892,
	[Description("ReinforceWeapon")]
	ReinforceWeapon = -808496558,
	[Description("AutoSkillDesc")]
	AutoSkillDesc = -249206182,
	[Description("AutoTabDesc")]
	AutoTabDesc = 1695507420,
	[Description("AutoCollectDesc")]
	AutoCollectDesc = 2101675771,
	[Description("DisableAuto")]
	DisableAuto = 862639576,
	[Description("OK")]
	OK = 4705,
	[Description("LackOfGoods")]
	LackOfGoods = -965123261,
	[Description("Okx2")]
	Okx2 = 4488971,
	[Description("MaxLevelInfo")]
	MaxLevelInfo = -1101210453,
	[Description("SwordGem_Bronze")]
	SwordGem_Bronze = -509610782,
	[Description("SwordGem_Silver")]
	SwordGem_Silver = -789161263,
	[Description("SwordGem_Gold")]
	SwordGem_Gold = -1752420144,
	[Description("SwordGem_Platinum")]
	SwordGem_Platinum = 1162332218,
	[Description("SwordGem_Dia")]
	SwordGem_Dia = -1164909064,
	[Description("FindSpoil")]
	FindSpoil = -1857942791,
	[Description("IAPRestore")]
	IAPRestore = -1316219045,
	[Description("BuyInventory")]
	BuyInventory = 524473095,
	[Description("EssenceFull")]
	EssenceFull = 927670620,
	[Description("StoneFull")]
	StoneFull = -759975147,
	[Description("TrapFull")]
	TrapFull = -2050602515,
	[Description("CantBuyAnymore")]
	CantBuyAnymore = 1272769728,
	[Description("Lock")]
	Lock = 4527370,
	[Description("Unlock")]
	Unlock = -369016969,
	[Description("CantSellEquippedStone")]
	CantSellEquippedStone = -281052631,
	[Description("CantSellLockeedStone")]
	CantSellLockeedStone = -1530366375,
	[Description("SellCheckAgain")]
	SellCheckAgain = -1621292745,
	[Description("SellAllStoneCheckAgain")]
	SellAllStoneCheckAgain = 1169861279,
	[Description("Sell")]
	Sell = 4141651,
	[Description("SellAll")]
	SellAll = -1170064956,
	[Description("FindSpoilCompleted")]
	FindSpoilCompleted = 333505652,
	[Description("IAPDoneMessage")]
	IAPDoneMessage = -1293178184,
	[Description("GemPurchaseDoneMessage")]
	GemPurchaseDoneMessage = -415116414,
	[Description("TotalEquippedStonePower")]
	TotalEquippedStonePower = 637048854,
	[Description("DamageVisible")]
	DamageVisible = 679804566,
	[Description("ProbabilityOfRangedAttackWhenSteppingOnATrap")]
	ProbabilityOfRangedAttackWhenSteppingOnATrap = 1041840525,
	[Description("maxTimeReward")]
	maxTimeReward = 369593933,
	[Description("ReviewTitle")]
	ReviewTitle = -1114361421,
	[Description("ReviewDesc")]
	ReviewDesc = -589649380,
	[Description("Find")]
	Find = 4711790,
	[Description("Indomitable")]
	Indomitable = -326123783,
	[Description("StoneSortDesc")]
	StoneSortDesc = -2031971755,
	[Description("Select")]
	Select = -314836881,
	[Description("SelectAll")]
	SelectAll = 902989480,
	[Description("DeselectAll")]
	DeselectAll = -483021805,
	[Description("Sort")]
	Sort = 4144159,
	[Description("Performance")]
	Performance = -1074931865,
	[Description("Damage")]
	Damage = 286293736,
	[Description("GrantDesc")]
	GrantDesc = -1453258472,
	[Description("FarmAttackPowerLink")]
	FarmAttackPowerLink = 2086156434,
	[Description("FarmHealthLink")]
	FarmHealthLink = -1795413071,
	[Description("FarmProductionLink")]
	FarmProductionLink = 1092753358,
	[Description("FarmBountyLink")]
	FarmBountyLink = 228126266,
	[Description("DefenseLink")]
	DefenseLink = 958502597,
	[Description("MentalStrengthLink")]
	MentalStrengthLink = 143946923,
	[Description("AmplificationLink")]
	AmplificationLink = -459730793,
	[Description("SuppresLink")]
	SuppresLink = -852382797,
	[Description("movementspeedLink")]
	movementspeedLink = -515961663,
	[Description("NaturalhealingLink")]
	NaturalhealingLink = 59018876,
	[Description("DesperationLink")]
	DesperationLink = 119268777,
	[Description("RageLink")]
	RageLink = 1592156968,
	[Description("CrisisLink")]
	CrisisLink = -329314828,
	[Description("RapidFireLink")]
	RapidFireLink = 861447879,
	[Description("PoisonRainLink")]
	PoisonRainLink = -1351704427,
	[Description("CompoundInterestLink")]
	CompoundInterestLink = 657167990,
	[Description("blackfeedLink")]
	blackfeedLink = -1520523908,
	[Description("ManaRecoverySpeed")]
	ManaRecoverySpeed = 311878192,
	[Description("HealthRecoverySpeed")]
	HealthRecoverySpeed = -74446407,
	[Description("ManaConsumptionResetTime")]
	ManaConsumptionResetTime = -170947777,
	[Description("HeroAttack")]
	HeroAttack = -1092965573,
	[Description("TrapPerformance")]
	TrapPerformance = 480451988,
	[Description("MaximumMana")]
	MaximumMana = -149814130,
	[Description("heroAttackPowerIncrease")]
	heroAttackPowerIncrease = 952915938,
	[Description("heroAttackPowerAmplification")]
	heroAttackPowerAmplification = 99026888,
	[Description("healthIncrease")]
	healthIncrease = -174220735,
	[Description("healthAmplification")]
	healthAmplification = 85657097,
	[Description("TrapPerformanceIncrease")]
	TrapPerformanceIncrease = 29229820,
	[Description("TrapPerformanceAmplification")]
	TrapPerformanceAmplification = -305755874,
	[Description("deleteAccount")]
	deleteAccount = 1401038335,
	[Description("enterDelete")]
	enterDelete = 546799790,
	[Description("AllMonsterHealthPowerDesc")]
	AllMonsterHealthPowerDesc = -1189168276,
	[Description("FarmHpPowerDesc")]
	FarmHpPowerDesc = 676478641,
	[Description("FarmDefensePowerDesc")]
	FarmDefensePowerDesc = -1924947469,
	[Description("PetMovementSpeedPowerDesc")]
	PetMovementSpeedPowerDesc = -407333858,
	[Description("AllPetMovementSpeedPowerDesc")]
	AllPetMovementSpeedPowerDesc = -1689641661,
	[Description("AllMonsterMovementSpeedPowerDesc")]
	AllMonsterMovementSpeedPowerDesc = -190285534,
	[Description("AllMonsterDefensePowerDesc")]
	AllMonsterDefensePowerDesc = 176295758,
	[Description("FenceDurabilityRecoverySpeedPowerDesc")]
	FenceDurabilityRecoverySpeedPowerDesc = -1560380931,
	[Description("AllMonsterHealthRecoverySpeedPowerDesc")]
	AllMonsterHealthRecoverySpeedPowerDesc = -921527770,
	[Description("AllMonsterAttackPowerPowerDesc")]
	AllMonsterAttackPowerPowerDesc = -1230893533,
	[Description("FarmAttackPowerPowerDesc")]
	FarmAttackPowerPowerDesc = -715691774,
	[Description("FarmAttackSpeedPowerDesc")]
	FarmAttackSpeedPowerDesc = 355610892,
	[Description("PetAttackSpeedPowerDesc")]
	PetAttackSpeedPowerDesc = 712826457,
	[Description("AllPetAttackSpeedPowerDesc")]
	AllPetAttackSpeedPowerDesc = -1222092648,
	[Description("BulletPowerPowerDesc")]
	BulletPowerPowerDesc = -1088196674,
	[Description("PetPowerPowerDesc")]
	PetPowerPowerDesc = -1527609861,
	[Description("AllPetPowerPowerDesc")]
	AllPetPowerPowerDesc = -458900746,
	[Description("GrenadeAttackPowerPowerDesc")]
	GrenadeAttackPowerPowerDesc = 814375370,
	[Description("FarmProductionPowerDesc")]
	FarmProductionPowerDesc = 387808430,
	[Description("FarmProductionSpeedPowerDesc")]
	FarmProductionSpeedPowerDesc = 238605609,
	[Description("ProductionSpeedPowerDesc")]
	ProductionSpeedPowerDesc = -1375346723,
	[Description("DebtPowerDesc")]
	DebtPowerDesc = -1367567476,
	[Description("ChickenProductionPowerDesc")]
	ChickenProductionPowerDesc = -89958829,
	[Description("LeopardProductionPowerDesc")]
	LeopardProductionPowerDesc = -1183667539,
	[Description("FarmBountyPowerDesc")]
	FarmBountyPowerDesc = 1196829930,
	[Description("FarmBountyProductionPowerDesc")]
	FarmBountyProductionPowerDesc = 1649662409,
	[Description("FeedingRatePowerDesc")]
	FeedingRatePowerDesc = -92577839,
	[Description("FarmFeedingRatePowerDesc")]
	FarmFeedingRatePowerDesc = 1461447973,
	[Description("AllLivestockMaximumSatietyPowerDesc")]
	AllLivestockMaximumSatietyPowerDesc = -1417795533,
	[Description("FeedingSatietyPowerDesc")]
	FeedingSatietyPowerDesc = -585280952,
	[Description("GrenadeCostPowerDesc")]
	GrenadeCostPowerDesc = -1566105076,
	[Description("GrenadeAreaPowerDesc")]
	GrenadeAreaPowerDesc = 224473304,
	[Description("BulletSizePowerDesc")]
	BulletSizePowerDesc = -546219904,
	[Description("OreDamagePowerDesc")]
	OreDamagePowerDesc = -1034679974,
	[Description("AbilityDiceCostPowerDesc")]
	AbilityDiceCostPowerDesc = 1774238711,
	[Description("MonsterAppearSpeedPowerDesc")]
	MonsterAppearSpeedPowerDesc = 1071220755,
	[Description("WantedCriminalBountyPowerDesc")]
	WantedCriminalBountyPowerDesc = -146810188,
	[Description("GoldenOrgeBountyPowerDesc")]
	GoldenOrgeBountyPowerDesc = 1987585614,
	[Description("EliteOrcPerWavePowerDesc")]
	EliteOrcPerWavePowerDesc = -779705456,
	[Description("CurrentMoneyAmplificationPowerDesc")]
	CurrentMoneyAmplificationPowerDesc = 2143817076,
	[Description("MoneyPowerDesc")]
	MoneyPowerDesc = -1242584273,
	[Description("LoanPowerDesc")]
	LoanPowerDesc = 2037130251,
	[Description("StoneThrowChanceOnFeedingPowerDesc")]
	StoneThrowChanceOnFeedingPowerDesc = -100683071,
	[Description("GrenadeChanceOnShootPowerDesc")]
	GrenadeChanceOnShootPowerDesc = -1501756817,
	[Description("ExtraBulletChanceOnShootPowerDesc")]
	ExtraBulletChanceOnShootPowerDesc = -1059162525,
	[Description("ContinuousGrenadePurchasePowerDesc")]
	ContinuousGrenadePurchasePowerDesc = -475325277,
	[Description("BulletPenetrationChancePowerDesc")]
	BulletPenetrationChancePowerDesc = -828445654,
	[Description("GrainAcquisitionPowerDesc")]
	GrainAcquisitionPowerDesc = -1260636409,
	[Description("MilkAcquisitionPowerDesc")]
	MilkAcquisitionPowerDesc = -1611379471,
	[Description("GemAcquisitionPowerDesc")]
	GemAcquisitionPowerDesc = 441756423,
	[Description("AdditionalWaveOpportunitiesPowerDesc")]
	AdditionalWaveOpportunitiesPowerDesc = 1281765938,
	[Description("FenceCounterattackPowerDesc")]
	FenceCounterattackPowerDesc = -1001962358,
	[Description("BountyChestIncreasePowerDesc")]
	BountyChestIncreasePowerDesc = -394257045,
	[Description("BulletCriticalChancePowerDesc")]
	BulletCriticalChancePowerDesc = -409339024,
	[Description("SummonBattleGolemPowerDesc")]
	SummonBattleGolemPowerDesc = 1923614352,
	[Description("SummonGoldenSheepPowerDesc")]
	SummonGoldenSheepPowerDesc = 1388743586,
	[Description("SummonGoldenChickenPowerDesc")]
	SummonGoldenChickenPowerDesc = -2053130710,
	[Description("SummonWhiteLeopardPowerDesc")]
	SummonWhiteLeopardPowerDesc = -272523180,
	[Description("SummonMotherChickenPowerDesc")]
	SummonMotherChickenPowerDesc = 741454940,
	[Description("SummonTurkeyPowerDesc")]
	SummonTurkeyPowerDesc = 1813892204,
	[Description("SummonGolemPowerDesc")]
	SummonGolemPowerDesc = 1452009474,
	[Description("PoisonRainPowerDesc")]
	PoisonRainPowerDesc = 956262655,
	[Description("PetAttackPowerProportionalToFarmAttackPowerDesc")]
	PetAttackPowerProportionalToFarmAttackPowerDesc = 2039171965,
	[Description("MeatDropBountyPowerDesc")]
	MeatDropBountyPowerDesc = 1481549140,
	[Description("AutomaticProduceCollectionPowerDesc")]
	AutomaticProduceCollectionPowerDesc = -5301232,
	[Description("ButcheryProductionPowerDesc")]
	ButcheryProductionPowerDesc = 1310953740,
	[Description("ProducePerCyclePowerDesc")]
	ProducePerCyclePowerDesc = 1020255436,
	[Description("SummonBundlePowerDesc")]
	SummonBundlePowerDesc = -1535321062,
	[Description("ExtraBulletChance100CooldownSpeedPowerDesc")]
	ExtraBulletChance100CooldownSpeedPowerDesc = -649847392,
	[Description("GrenadeSplitDamagePowerDesc")]
	GrenadeSplitDamagePowerDesc = -502218446,
	[Description("BulletCriticalAdditionalDamagePowerDesc")]
	BulletCriticalAdditionalDamagePowerDesc = -198481156,
	[Description("CastleHoldingPowerDesc")]
	CastleHoldingPowerDesc = -2013023260,
	[Description("FarmHealthProportionalToPetHealthPowerDesc")]
	FarmHealthProportionalToPetHealthPowerDesc = 636796024,
	[Description("PetNaturalHealingBasicPowerDesc")]
	PetNaturalHealingBasicPowerDesc = -213482973,
	[Description("PetRevivalSpeedBasicPowerDesc")]
	PetRevivalSpeedBasicPowerDesc = -1822622062,
	[Description("LivestockResuscitationSpeedPowerDesc")]
	LivestockResuscitationSpeedPowerDesc = -1053178247,
	[Description("RangeAttackPowerDesc")]
	RangeAttackPowerDesc = 25774870,
	[Description("EnemyDefenseReductionOnAttackPowerDesc")]
	EnemyDefenseReductionOnAttackPowerDesc = 1629855319,
	[Description("FenceRepairWhenAttackPowerDesc")]
	FenceRepairWhenAttackPowerDesc = 69799691,
	[Description("FenceCounterattack")]
	FenceCounterattack = 1678013562,
	[Description("AdditionalAbilityChancePowerDesc")]
	AdditionalAbilityChancePowerDesc = 1756785694,
	[Description("AddStartAbilityPowerDesc")]
	AddStartAbilityPowerDesc = -220533082,
	[Description("KingStoneChanceAmplifyPowerDesc")]
	KingStoneChanceAmplifyPowerDesc = -1655368275,
	[Description("CompoundInterestPowerDesc")]
	CompoundInterestPowerDesc = -227294122,
	[Description("AllMonsterAttackSpeedPowerDesc")]
	AllMonsterAttackSpeedPowerDesc = 1461443181,
	[Description("NaturaldisasterSpeedPowerDesc")]
	NaturaldisasterSpeedPowerDesc = 1147323232,
	[Description("TimeLimitPowerDesc")]
	TimeLimitPowerDesc = 104987023,
	[Description("ProductionPowerDesc")]
	ProductionPowerDesc = 53044258,
	
}

public enum ELinkTable
{
    [Description("FarmAttackPower")]
	FarmAttackPower = 1893344498,
	[Description("FarmHealth")]
	FarmHealth = -1422694371,
	[Description("FarmProduction")]
	FarmProduction = -1423533010,
	[Description("FarmBounty")]
	FarmBounty = -1012993702,
	[Description("Defense")]
	Defense = 409592545,
	[Description("Amplification")]
	Amplification = 523564339,
	[Description("Suppres")]
	Suppres = -1631930465,
	[Description("movementspeed")]
	movementspeed = 551426285,
	[Description("Performance")]
	Performance = -1074931865,
	[Description("Naturalhealing")]
	Naturalhealing = -475128316,
	[Description("RapidFire")]
	RapidFire = -1571609757,
	[Description("PoisonRain")]
	PoisonRain = -29584207,
	[Description("CompoundInterest")]
	CompoundInterest = -965993834,
	[Description("blackfeed")]
	blackfeed = -1137986300,
	
}

public enum EStageInfoTable
{
    [Description("valueTypeDefine")]
	valueTypeDefine = 0,
	[Description("Stage1")]
	Stage1 = 1,
	[Description("Stage2")]
	Stage2 = 2,
	[Description("Stage3")]
	Stage3 = 3,
	[Description("Stage4")]
	Stage4 = 4,
	[Description("Stage5")]
	Stage5 = 5,
	[Description("Stage6")]
	Stage6 = 6,
	[Description("Stage7")]
	Stage7 = 7,
	[Description("Stage8")]
	Stage8 = 8,
	[Description("Stage9")]
	Stage9 = 9,
	[Description("Stage10")]
	Stage10 = 10,
	[Description("Stage11")]
	Stage11 = 11,
	[Description("Stage12")]
	Stage12 = 12,
	[Description("Stage13")]
	Stage13 = 13,
	[Description("Stage14")]
	Stage14 = 14,
	[Description("Stage15")]
	Stage15 = 15,
	[Description("Stage16")]
	Stage16 = 16,
	[Description("Stage17")]
	Stage17 = 17,
	[Description("Stage18")]
	Stage18 = 18,
	[Description("Stage19")]
	Stage19 = 19,
	[Description("Stage20")]
	Stage20 = 20,
	[Description("Stage21")]
	Stage21 = 21,
	[Description("Stage22")]
	Stage22 = 22,
	[Description("Stage23")]
	Stage23 = 23,
	[Description("Stage24")]
	Stage24 = 24,
	[Description("Stage25")]
	Stage25 = 25,
	[Description("Stage26")]
	Stage26 = 26,
	[Description("Stage27")]
	Stage27 = 27,
	[Description("Stage28")]
	Stage28 = 28,
	[Description("Stage29")]
	Stage29 = 29,
	[Description("Stage30")]
	Stage30 = 30,
	[Description("Stage31")]
	Stage31 = 31,
	[Description("Stage32")]
	Stage32 = 32,
	[Description("Stage33")]
	Stage33 = 33,
	[Description("Stage34")]
	Stage34 = 34,
	[Description("Stage35")]
	Stage35 = 35,
	[Description("Stage36")]
	Stage36 = 36,
	[Description("Stage37")]
	Stage37 = 37,
	[Description("Stage38")]
	Stage38 = 38,
	[Description("Stage39")]
	Stage39 = 39,
	[Description("Stage40")]
	Stage40 = 40,
	[Description("Stage41")]
	Stage41 = 41,
	[Description("Stage42")]
	Stage42 = 42,
	[Description("Stage43")]
	Stage43 = 43,
	[Description("Stage44")]
	Stage44 = 44,
	[Description("Stage45")]
	Stage45 = 45,
	[Description("Stage46")]
	Stage46 = 46,
	[Description("Stage47")]
	Stage47 = 47,
	[Description("Stage48")]
	Stage48 = 48,
	[Description("Stage49")]
	Stage49 = 49,
	[Description("Stage50")]
	Stage50 = 50,
	[Description("Stage51")]
	Stage51 = 51,
	[Description("Stage52")]
	Stage52 = 52,
	[Description("Stage53")]
	Stage53 = 53,
	[Description("Stage54")]
	Stage54 = 54,
	[Description("Stage55")]
	Stage55 = 55,
	[Description("Stage56")]
	Stage56 = 56,
	[Description("Stage57")]
	Stage57 = 57,
	[Description("Stage58")]
	Stage58 = 58,
	[Description("Stage59")]
	Stage59 = 59,
	[Description("Stage60")]
	Stage60 = 60,
	[Description("Stage61")]
	Stage61 = 61,
	[Description("Stage62")]
	Stage62 = 62,
	[Description("Stage63")]
	Stage63 = 63,
	[Description("Stage64")]
	Stage64 = 64,
	[Description("Stage65")]
	Stage65 = 65,
	[Description("Stage66")]
	Stage66 = 66,
	[Description("Stage67")]
	Stage67 = 67,
	[Description("Stage68")]
	Stage68 = 68,
	[Description("Stage69")]
	Stage69 = 69,
	[Description("Stage70")]
	Stage70 = 70,
	[Description("Stage71")]
	Stage71 = 71,
	[Description("Stage72")]
	Stage72 = 72,
	[Description("Stage73")]
	Stage73 = 73,
	[Description("Stage74")]
	Stage74 = 74,
	[Description("Stage75")]
	Stage75 = 75,
	[Description("Stage76")]
	Stage76 = 76,
	[Description("Stage77")]
	Stage77 = 77,
	[Description("Stage78")]
	Stage78 = 78,
	[Description("Stage79")]
	Stage79 = 79,
	[Description("Stage80")]
	Stage80 = 80,
	[Description("Stage81")]
	Stage81 = 81,
	[Description("Stage82")]
	Stage82 = 82,
	[Description("Stage83")]
	Stage83 = 83,
	[Description("Stage84")]
	Stage84 = 84,
	[Description("Stage85")]
	Stage85 = 85,
	[Description("Stage86")]
	Stage86 = 86,
	[Description("Stage87")]
	Stage87 = 87,
	[Description("Stage88")]
	Stage88 = 88,
	[Description("Stage89")]
	Stage89 = 89,
	[Description("Stage90")]
	Stage90 = 90,
	[Description("Stage91")]
	Stage91 = 91,
	[Description("Stage92")]
	Stage92 = 92,
	[Description("Stage93")]
	Stage93 = 93,
	[Description("Stage94")]
	Stage94 = 94,
	[Description("Stage95")]
	Stage95 = 95,
	[Description("Stage96")]
	Stage96 = 96,
	[Description("Stage97")]
	Stage97 = 97,
	[Description("Stage98")]
	Stage98 = 98,
	[Description("Stage99")]
	Stage99 = 99,
	[Description("Stage100")]
	Stage100 = 100,
	[Description("Stage101")]
	Stage101 = 101,
	[Description("Stage102")]
	Stage102 = 102,
	[Description("Stage103")]
	Stage103 = 103,
	[Description("Stage104")]
	Stage104 = 104,
	[Description("Stage105")]
	Stage105 = 105,
	[Description("Stage106")]
	Stage106 = 106,
	[Description("Stage107")]
	Stage107 = 107,
	[Description("Stage108")]
	Stage108 = 108,
	[Description("Stage109")]
	Stage109 = 109,
	[Description("Stage110")]
	Stage110 = 110,
	[Description("Stage111")]
	Stage111 = 111,
	[Description("Stage112")]
	Stage112 = 112,
	[Description("Stage113")]
	Stage113 = 113,
	[Description("Stage114")]
	Stage114 = 114,
	[Description("Stage115")]
	Stage115 = 115,
	[Description("Stage116")]
	Stage116 = 116,
	[Description("Stage117")]
	Stage117 = 117,
	[Description("Stage118")]
	Stage118 = 118,
	[Description("Stage119")]
	Stage119 = 119,
	[Description("Stage120")]
	Stage120 = 120,
	[Description("Stage121")]
	Stage121 = 121,
	[Description("Stage122")]
	Stage122 = 122,
	[Description("Stage123")]
	Stage123 = 123,
	[Description("Stage124")]
	Stage124 = 124,
	[Description("Stage125")]
	Stage125 = 125,
	[Description("Stage126")]
	Stage126 = 126,
	[Description("Stage127")]
	Stage127 = 127,
	[Description("Stage128")]
	Stage128 = 128,
	[Description("Stage129")]
	Stage129 = 129,
	[Description("Stage130")]
	Stage130 = 130,
	[Description("Stage131")]
	Stage131 = 131,
	[Description("Stage132")]
	Stage132 = 132,
	[Description("Stage133")]
	Stage133 = 133,
	[Description("Stage134")]
	Stage134 = 134,
	[Description("Stage135")]
	Stage135 = 135,
	
}

public enum EWeatherTable
{
    [Description("valueTypeDefine")]
	valueTypeDefine = -1227803469,
	[Description("flood")]
	flood = 177680465,
	[Description("thunder")]
	thunder = 675081299,
	[Description("storm")]
	storm = 159217006,
	[Description("meatbug")]
	meatbug = -253270580,
	[Description("wasp")]
	wasp = 5274450,
	[Description("farmRat")]
	farmRat = -1408456124,
	[Description("orcTotem")]
	orcTotem = 958354794,
	[Description("blizzard")]
	blizzard = 769755001,
	[Description("yeti")]
	yeti = 4681624,
	[Description("meteor")]
	meteor = 822464331,
	[Description("lava")]
	lava = 5299233,
	[Description("volcanoWorm")]
	volcanoWorm = -1882579794,
	
}

public enum ESkillTrainingTable
{
    [Description("valueTypeDefine")]
	valueTypeDefine = -1227803469,
	[Description("ampProduction")]
	ampProduction = -77938026,
	[Description("ampBounty")]
	ampBounty = -854033774,
	[Description("ampAttack")]
	ampAttack = -989979165,
	[Description("ampHp")]
	ampHp = 168775923,
	
}

public enum EPatchNoteTable
{
    [Description("_0_0_58")]
	_0_0_58 = 1632765545,
	[Description("_0_0_57")]
	_0_0_57 = 1632765542,
	[Description("_0_0_56")]
	_0_0_56 = 1632765543,
	[Description("_0_0_50")]
	_0_0_50 = 1632765537,
	
}

public enum EShopTable
{
    [Description("valueTypeDefine")]
	valueTypeDefine = -1227803469,
	[Description("noaddpackagefarmer2")]
	noaddpackagefarmer2 = -1447182898,
	[Description("gamespeedfarmer2")]
	gamespeedfarmer2 = 2054172619,
	[Description("firstgemfarmer2")]
	firstgemfarmer2 = -339341627,
	[Description("dailygemfarmer2")]
	dailygemfarmer2 = -1632305712,
	[Description("dailymilkfarmer2")]
	dailymilkfarmer2 = -153031464,
	[Description("dailygrainfarmer2")]
	dailygrainfarmer2 = -493351364,
	[Description("freefirstrerollfarmer2")]
	freefirstrerollfarmer2 = 1765058992,
	[Description("smallgemfarmer2")]
	smallgemfarmer2 = 1339455622,
	[Description("middlegemfarmer2")]
	middlegemfarmer2 = -584676938,
	[Description("largegemfarmer2")]
	largegemfarmer2 = -1895769414,
	[Description("megagemfarmer2")]
	megagemfarmer2 = -1346971637,
	[Description("grandgemfarmer2")]
	grandgemfarmer2 = -1987831263,
	[Description("adGem")]
	adGem = 169030069,
	[Description("adGrain")]
	adGrain = -770113127,
	[Description("adMilk")]
	adMilk = 945278697,
	[Description("milk_small")]
	milk_small = -1022668154,
	[Description("milk_middle")]
	milk_middle = -1358820114,
	[Description("grain_small")]
	grain_small = -1979658968,
	[Description("grain_middle")]
	grain_middle = -2045362528,
	[Description("Unlock_PetSlot1")]
	Unlock_PetSlot1 = -2106958854,
	[Description("Unlock_PetSlot2")]
	Unlock_PetSlot2 = -2106958855,
	[Description("Feeder")]
	Feeder = 229145658,
	[Description("Carpenter")]
	Carpenter = -851833495,
	[Description("Auriel")]
	Auriel = -9304147,
	[Description("Butcher")]
	Butcher = -518556548,
	[Description("BomberMan")]
	BomberMan = 226441830,
	[Description("Amazon")]
	Amazon = -31945125,
	[Description("Log")]
	Log = 146043,
	[Description("SilverBullet")]
	SilverBullet = -593834092,
	[Description("seal")]
	seal = 5150744,
	[Description("Jophiro")]
	Jophiro = 456232882,
	[Description("JoPalmae")]
	JoPalmae = 272179188,
	[Description("GeumYungjae")]
	GeumYungjae = 722958082,
	[Description("DoBalho")]
	DoBalho = 668310590,
	[Description("SaYoosa")]
	SaYoosa = -1066708322,
	[Description("YoonHeon")]
	YoonHeon = 1429854814,
	[Description("PyoBeoma")]
	PyoBeoma = 1780082919,
	[Description("KangMujin")]
	KangMujin = 367650923,
	[Description("BanAri")]
	BanAri = 110888596,
	
}

public enum EInGameShopTable
{
    [Description("valueTypeDefine")]
	valueTypeDefine = -1227803469,
	[Description("abilityCard")]
	abilityCard = 1243698465,
	[Description("rerollAbilityCard")]
	rerollAbilityCard = 768752155,
	[Description("GoldChicken")]
	GoldChicken = 1877588074,
	[Description("WhitePanther")]
	WhitePanther = -1601603334,
	[Description("MotherChicken")]
	MotherChicken = -810211761,
	[Description("Turkey")]
	Turkey = -183129023,
	[Description("Golem")]
	Golem = 147179217,
	[Description("BattleGolem")]
	BattleGolem = -1248138629,
	[Description("GoldenSheep")]
	GoldenSheep = -1382942019,
	[Description("farmattack")]
	farmattack = 1860031435,
	[Description("farmhp")]
	farmhp = 1201491195,
	[Description("farmbounty")]
	farmbounty = 2011691162,
	[Description("farmProduction")]
	farmProduction = 1357891470,
	[Description("milk")]
	milk = 5321762,
	[Description("grain")]
	grain = 178839828,
	
}

public enum EInGameEventTable
{
    [Description("valueTypeDefine")]
	valueTypeDefine = -1227803469,
	[Description("FarmProduction")]
	FarmProduction = -1423533010,
	[Description("FarmHp")]
	FarmHp = 224289723,
	[Description("FarmBounty")]
	FarmBounty = -1012993702,
	[Description("FarmAttackPower")]
	FarmAttackPower = 1893344498,
	[Description("FarmAttackSpeed")]
	FarmAttackSpeed = 1891893308,
	[Description("MonsterHp")]
	MonsterHp = -1108376293,
	[Description("MonsterAttack")]
	MonsterAttack = -1857262101,
	[Description("MonsterMove")]
	MonsterMove = 2185196,
	[Description("MonterAttackSpeed")]
	MonterAttackSpeed = -403513755,
	[Description("Debt")]
	Debt = 4771260,
	[Description("compound")]
	compound = 637629408,
	[Description("monsterDeffence")]
	monsterDeffence = -1959487645,
	[Description("disasterPeriod")]
	disasterPeriod = -1208518635,
	[Description("Milk")]
	Milk = 4312834,
	[Description("Grain")]
	Grain = 147317236,
	[Description("Loan")]
	Loan = 4527437,
	[Description("Money")]
	Money = 133513537,
	[Description("AbilityCard")]
	AbilityCard = -924767551,
	[Description("TimeLimit")]
	TimeLimit = 326374945,
	[Description("GoldenOgre")]
	GoldenOgre = -736749391,
	[Description("AbilityRerollCost")]
	AbilityRerollCost = -1466138066,
	
}




[Serializable]
public class PowerTableLoader : ILoader<EPowerTable, PowerTable>
{
    public List<PowerTable> powerTables = new List<PowerTable>();

    public Dictionary<EPowerTable, PowerTable> MakeDict()
    {
        var dataDict = new Dictionary<EPowerTable, PowerTable>();
        foreach (var data in powerTables)
        {
            dataDict.Add(data.key.ParseEnum<EPowerTable>(), data);
        }
        return dataDict;
    }
}
[Serializable]
public class AbilityTableLoader : ILoader<EAbilityTable, AbilityTable>
{
    public List<AbilityTable> AbilityTables = new List<AbilityTable>();

    public Dictionary<EAbilityTable, AbilityTable> MakeDict()
    {
        var dataDict = new Dictionary<EAbilityTable, AbilityTable>();
        foreach (var data in AbilityTables)
        {
            dataDict.Add(data.key.ParseEnum<EAbilityTable>(), data);
        }
        return dataDict;
    }
}
[Serializable]
public class CharacterTableLoader : ILoader<ECharacterTable, CharacterTable>
{
    public List<CharacterTable> characterTables = new List<CharacterTable>();

    public Dictionary<ECharacterTable, CharacterTable> MakeDict()
    {
        var dataDict = new Dictionary<ECharacterTable, CharacterTable>();
        foreach (var data in characterTables)
        {
            dataDict.Add(data.key.ParseEnum<ECharacterTable>(), data);
        }
        return dataDict;
    }
}
[Serializable]
public class ConfigTableLoader : ILoader<EConfigTable, ConfigTable>
{
    public List<ConfigTable> configTables = new List<ConfigTable>();

    public Dictionary<EConfigTable, ConfigTable> MakeDict()
    {
        var dataDict = new Dictionary<EConfigTable, ConfigTable>();
        foreach (var data in configTables)
        {
            dataDict.Add(data.key.ParseEnum<EConfigTable>(), data);
        }
        return dataDict;
    }
}
[Serializable]
public class LanguageTableLoader : ILoader<ELanguageTable, LanguageTable>
{
    public List<LanguageTable> languageTables = new List<LanguageTable>();

    public Dictionary<ELanguageTable, LanguageTable> MakeDict()
    {
        var dataDict = new Dictionary<ELanguageTable, LanguageTable>();
        foreach (var data in languageTables)
        {
            dataDict.Add(data.key.ParseEnum<ELanguageTable>(), data);
        }
        return dataDict;
    }
}
[Serializable]
public class LinkTableLoader : ILoader<ELinkTable, LinkTable>
{
    public List<LinkTable> linkTables = new List<LinkTable>();

    public Dictionary<ELinkTable, LinkTable> MakeDict()
    {
        var dataDict = new Dictionary<ELinkTable, LinkTable>();
        foreach (var data in linkTables)
        {
            dataDict.Add(data.key.ParseEnum<ELinkTable>(), data);
        }
        return dataDict;
    }
}
[Serializable]
public class StageInfoTableLoader : ILoader<EStageInfoTable, StageInfoTable>
{
    public List<StageInfoTable> stageInfoTables = new List<StageInfoTable>();

    public Dictionary<EStageInfoTable, StageInfoTable> MakeDict()
    {
        var dataDict = new Dictionary<EStageInfoTable, StageInfoTable>();
        foreach (var data in stageInfoTables)
        {
            dataDict.Add(data.key.ParseEnum<EStageInfoTable>(), data);
        }
        return dataDict;
    }
}
[Serializable]
public class WeatherTableLoader : ILoader<EWeatherTable, WeatherTable>
{
    public List<WeatherTable> weatherTables = new List<WeatherTable>();

    public Dictionary<EWeatherTable, WeatherTable> MakeDict()
    {
        var dataDict = new Dictionary<EWeatherTable, WeatherTable>();
        foreach (var data in weatherTables)
        {
            dataDict.Add(data.key.ParseEnum<EWeatherTable>(), data);
        }
        return dataDict;
    }
}
[Serializable]
public class SkillTrainingTableLoader : ILoader<ESkillTrainingTable, SkillTrainingTable>
{
    public List<SkillTrainingTable> SkilltrainingTables = new List<SkillTrainingTable>();

    public Dictionary<ESkillTrainingTable, SkillTrainingTable> MakeDict()
    {
        var dataDict = new Dictionary<ESkillTrainingTable, SkillTrainingTable>();
        foreach (var data in SkilltrainingTables)
        {
            dataDict.Add(data.key.ParseEnum<ESkillTrainingTable>(), data);
        }
        return dataDict;
    }
}
[Serializable]
public class PatchNoteTableLoader : ILoader<EPatchNoteTable, PatchNoteTable>
{
    public List<PatchNoteTable> patchNoteTables = new List<PatchNoteTable>();

    public Dictionary<EPatchNoteTable, PatchNoteTable> MakeDict()
    {
        var dataDict = new Dictionary<EPatchNoteTable, PatchNoteTable>();
        foreach (var data in patchNoteTables)
        {
            dataDict.Add(data.key.ParseEnum<EPatchNoteTable>(), data);
        }
        return dataDict;
    }
}
[Serializable]
public class ShopTableLoader : ILoader<EShopTable, ShopTable>
{
    public List<ShopTable> shopTables = new List<ShopTable>();

    public Dictionary<EShopTable, ShopTable> MakeDict()
    {
        var dataDict = new Dictionary<EShopTable, ShopTable>();
        foreach (var data in shopTables)
        {
            dataDict.Add(data.key.ParseEnum<EShopTable>(), data);
        }
        return dataDict;
    }
}
[Serializable]
public class InGameShopTableLoader : ILoader<EInGameShopTable, InGameShopTable>
{
    public List<InGameShopTable> ingameShopTables = new List<InGameShopTable>();

    public Dictionary<EInGameShopTable, InGameShopTable> MakeDict()
    {
        var dataDict = new Dictionary<EInGameShopTable, InGameShopTable>();
        foreach (var data in ingameShopTables)
        {
            dataDict.Add(data.key.ParseEnum<EInGameShopTable>(), data);
        }
        return dataDict;
    }
}
[Serializable]
public class InGameEventTableLoader : ILoader<EInGameEventTable, InGameEventTable>
{
    public List<InGameEventTable> inGameEventTables = new List<InGameEventTable>();

    public Dictionary<EInGameEventTable, InGameEventTable> MakeDict()
    {
        var dataDict = new Dictionary<EInGameEventTable, InGameEventTable>();
        foreach (var data in inGameEventTables)
        {
            dataDict.Add(data.key.ParseEnum<EInGameEventTable>(), data);
        }
        return dataDict;
    }
}


[Serializable]
public class PowerTable
{
	public string key;
	public float amount;
	public int multiplyType;
	public string desckLanguageKey;
}
[Serializable]
public class AbilityTable
{
	public string key;
	public string descLanguageKey;
	public string nameLanguageKey;
	public string masterLanguageKey;
	public List<string> PowerKeyList = new();
	public List<float> PowerAmountList = new();
	public List<string> MasterPowerKeyList = new();
	public List<float> MasterPowerAmountList = new();
	public bool isLimitedOne;
}
[Serializable]
public class CharacterTable
{
	public string key;
	public string skinKey;
	public int skinType;
	public int characterType;
	public bool isAttackPet;
	public bool isStagePoolable;
	public bool isCanBackpack;
	public List<int> collisionType = new();
	public List<int> detectorType = new();
	public List<int> detectPriority = new();
	public int star;
	public string hintKey;
	public string makeProductSoundKey;
	public string productPrefabKey;
	public string evolveKey;
	public string Eprefab;
	public string attackAnimKey;
	public string productionAnimKey;
	public string nameLanguageKey;
	public string descLanguageKey;
	public string acquireDescLanguageKey;
	public float baseMoveSpeed;
	public float maxSatiety;
	public float SatietyRate;
	public int attackType;
	public float baseAttack;
	public float baseHp;
	public float hp_recovery;
	public float baseAttackSpeed;
	public float deffense;
	public float population;
	public float bounty;
	public List<string> BasePowerKeyList = new();
	public List<float> BasePowerAmountList = new();
	public List<string> LastePowerKeyList = new();
	public List<string> EquipPowerKeyList = new();
}
[Serializable]
public class ConfigTable
{
	public string key;
	public int IntValue;
	public float FloatValue;
	public List<int> IntArray = new();
	public List<float> FloatArray = new();
	public List<string> StringArray = new();
}
[Serializable]
public class LanguageTable
{
	public string key;
	public string kr;
	public string en;
}
[Serializable]
public class LinkTable
{
	public string key;
	public string kr;
	public string en;
	public string LanguageKey;
}
[Serializable]
public class StageInfoTable
{
	public string key;
	public int stage;
	public string stageNameLanguageKey;
	public int stageLv_Main;
	public int stageLv_Sub;
	public bool petUnlock;
	public string newPet;
	public List<string> DefaultMonster = new();
	public List<string> weatherList = new();
	public List<string> monsterList = new();
}
[Serializable]
public class WeatherTable
{
	public string key;
	public string LanguageKey_name;
	public string LanguageKey_Desc;
}
[Serializable]
public class SkillTrainingTable
{
	public string key;
	public string nameLanguageKey;
	public string desc1LanguageKey;
	public string desc2LanguageKey;
}
[Serializable]
public class PatchNoteTable
{
	public string key;
	public string kr;
	public string en;
}
[Serializable]
public class ShopTable
{
	public string key;
	public string nameLanguageKey;
	public string descLanguageKey;
	public List<int> price_cash = new();
	public int price_gem;
	public int amount;
}
[Serializable]
public class InGameShopTable
{
	public string key;
	public string iconKey;
	public string nameLanguageKey;
	public string descLanguageOrPowerKey;
	public int price;
	public int power;
}
[Serializable]
public class InGameEventTable
{
	public string key;
	public List<int> abilableTypeList = new();
	public List<string> descLanguageOrPowerKeyList = new();
}