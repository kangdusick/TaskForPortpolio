using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Component = UnityEngine.Component;
using Unity.Mathematics;
using System.Threading;
using CodeStage.AntiCheat.ObscuredTypes;

public static class Extensions
{
    private static readonly Dictionary<(Type, string), Enum> stringToEnumDict = new();
    public static Dictionary<Type, Dictionary<GameObject, Component>> componentCache = new Dictionary<Type, Dictionary<GameObject, Component>>();
    public static void CancleWithDisposeCheck(this CancellationTokenSource cancellationTokenSource )
    {
        try
        {
            cancellationTokenSource.Cancel();
        }
        catch
        {
        }
    }
    public static T GetCashComponent<T>(this GameObject go) where T : Component
    {
        if (!componentCache.TryGetValue(typeof(T), out var gameObjectToComponentMap))
        {
            gameObjectToComponentMap = new Dictionary<GameObject, Component>();
            componentCache[typeof(T)] = gameObjectToComponentMap;
        }

        if (!gameObjectToComponentMap.TryGetValue(go, out var component))
        {
            component = go.GetComponent<T>();
            gameObjectToComponentMap[go] = component;
        }

        return component as T;
    }
    public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) where TKey : IComparable<TKey>
    {
        using (var enumerator = source.GetEnumerator())
        {
            if (!enumerator.MoveNext())
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }

            TSource minElement = enumerator.Current;
            TKey minKey = keySelector(minElement);
            while (enumerator.MoveNext())
            {
                TSource currentElement = enumerator.Current;
                TKey currentKey = keySelector(currentElement);
                if (currentKey.CompareTo(minKey) < 0)
                {
                    minElement = currentElement;
                    minKey = currentKey;
                }
            }

            return minElement;
        }
    }
   

    public static T RandomElement<T>(this HashSet<T> hashSet)
    {
        int index = Random.Range(0, hashSet.Count);
        var enumerator = hashSet.GetEnumerator();
        for (int i = 0; i <= index; i++)
        {
            enumerator.MoveNext();
        }
        return enumerator.Current;
    }
    public static T ParseEnum<T>(this string stringEnum) where T : Enum
    {
        var key = (typeof(T), stringEnum);
     
        if (stringToEnumDict.TryGetValue(key, out Enum enumValue))
        {
            return (T)enumValue;
        }
        else
        {
            T parsedEnum = (T)Enum.Parse(typeof(T), stringEnum);
            stringToEnumDict[key] = parsedEnum;
            return parsedEnum;
        }
    }
    public static void SetImageTargetSize(this Image targetImage, Sprite targetSprite, float targetSize)
    {
        targetImage.sprite = targetSprite;
        // 원본 이미지의 크기를 가져옵니다.
        float originalWidth = targetSprite.rect.width;
        float originalHeight = targetSprite.rect.height;

        // 비율을 계산합니다.
        float scaleFactor = Math.Min(targetSize / originalWidth, targetSize / originalHeight);

        // 이미지의 크기를 조정합니다.
        targetImage.rectTransform.sizeDelta = new Vector2(originalWidth * scaleFactor, originalHeight * scaleFactor);
    }
    public static void SetImageTargetSize(this Image targetImage, ESprite targetESprite, float targetSize)
    {
        SetImageTargetSize(targetImage, SpriteManager.Instance.LoadSprite(targetESprite), targetSize);
    }
    public static List<(EPowerTable ePowerTable, float powerAmount, int applyLv)> GetEquipPowerList(this CharacterTable characterTable)
    {
        var EquipPowerList = new List<(EPowerTable ePowerTable, float powerAmount, int applyLv)>();
        for (int i = 0; i < characterTable.EquipPowerKeyList.Count; i++)
        {
            var powerKey = characterTable.EquipPowerKeyList[i].ParseEnum<EPowerTable>();
            float powerAmount = 5f * Mathf.Pow(TableManager.ConfigTableDict[EConfigTable.powerPowPerStar].FloatValue, characterTable.star - 1) * TableManager.PowerTableDict[powerKey].amount;
            int applyLv = (i+1) *20;
            EquipPowerList.Add((powerKey, powerAmount, applyLv));
        }
        return EquipPowerList;
    }
    public static List<(EPowerTable ePowerTable, float powerAmount, int applyLv)> GetLastPowerList(this CharacterTable characterTable)
    {
        var lastPowerList = new List<(EPowerTable ePowerTable, float powerAmount, int applyLv)>();
        for (int i = 0; i < characterTable.LastePowerKeyList.Count; i++)
        {
            var powerKey = characterTable.LastePowerKeyList[i].ParseEnum<EPowerTable>();
            float powerAmount = 1f * Mathf.Pow(TableManager.ConfigTableDict[EConfigTable.powerPowPerStar].FloatValue,characterTable.star -1) * TableManager.PowerTableDict[powerKey].amount;
            int applyLv = (i) * 20+10;
            lastPowerList.Add((powerKey, powerAmount,applyLv));
        }
        return lastPowerList;
    }
    public static string LocalIzeText(this string keyLabel, params object[] args)
    {
        return LocalIzeText(keyLabel.ParseEnum<ELanguageTable>(), args);
    }
    public static string LocalIzeText(this ELanguageTable keyLabel, params object[] args)
    {
        string result;

        if (!TableManager.LanguageTableDict.ContainsKey(keyLabel))
        {
            switch (GameUtil.language)
            {
                case "kr":
                    switch (keyLabel)
                    {
                        case ELanguageTable.pleaseUpdate:
                            result = "최신 버전을 이용해주세요.";
                            break;
                        case ELanguageTable.needInternet:
                            result = "인터넷 연결이 필요합니다.";
                            break;
                        default:
                            result = string.Empty;
                            break;
                    }
                    break;
                default:
                    switch (keyLabel)
                    {
                        case ELanguageTable.pleaseUpdate:
                            result = "Please use the latest version.";
                            break;
                        case ELanguageTable.needInternet:
                            result = "Internet connection is required.";
                            break;
                        default:
                            result = string.Empty;
                            break;
                    }
                    break;
            }
        }
        else
        {
            switch (GameUtil.language)
            {
                case "kr":
                    result = TableManager.LanguageTableDict[keyLabel].kr;
                    break;
                default:
                    result = TableManager.LanguageTableDict[keyLabel].en;
                    break;
            }
        }
        result = args.Length > 0 ? string.Format(result, args) : result;
        return result;
    }
    private static readonly string[] suffixes =
{
    "", "K", "M", "B", "T",
    "Q", // Quadrillion (10^15)
    "Qu", // Quintillion (10^18)
    "S", // Sextillion (10^21)
    "Se", // Septillion (10^24)
    "O", // Octillion (10^27)
    "N", // Nonillion (10^30)
    "D", // Decillion (10^33)
    "UD", // Undecillion (10^36)
    "DD", // Duodecillion (10^39)
    "TD", // Tredecillion (10^42)
    "QaD", // Quattuordecillion (10^45)
    "QiD", // Quindecillion (10^48)
    "SD", // Sexdecillion (10^51)
    "SpD", // Septendecillion (10^54)
    "OD", // Octodecillion (10^57)
    "ND", // Novemdecillion (10^60)
    "V", // Vigintillion (10^63)
    "UV", // Unvigintillion (10^66)
    "DV", // Duovigintillion (10^69)
    "TV", // Tresvigintillion (10^72)
    "QaV", // Quattuorvigintillion (10^75)
    "QiV", // Quinvigintillion (10^78)
    "SV", // Sesvigintillion (10^81)
    "SpV", // Septemvigintillion (10^84)
    "OV", // Octovigintillion (10^87)
    "NV", // Novemvigintillion (10^90)
    // You can continue adding more suffixes following the pattern
};

    
    public static string KMBTUnit(this double num, bool isShowSign = false)
    {
        if (num <= 999)
        {
            return num.PrettyNumber(isShowSign);
        }
        return KMBTUnit(new BigInteger(num), isShowSign);
    }
    public static string KMBTUnit(this ObscuredInt num, bool isShowSign = false)
    {
        return KMBTUnit(new BigInteger(num), isShowSign);
    }
    public static string KMBTUnit(this int num, bool isShowSign = false)
    {
        return KMBTUnit(new BigInteger(num), isShowSign);
    }
    public static string KMBTUnit(this ObscuredLong num, bool isShowSign = false)
    {
        return KMBTUnit(new BigInteger(num), isShowSign);
    }
    public static string KMBTUnit(this ObscuredFloat num, bool isShowSign = false)
    {
        if (num <= 999)
        {
            return num.PrettyNumber(isShowSign);
        }
        return KMBTUnit(new BigInteger(num), isShowSign);
    }
  
    public static string KMBTUnit(this float num, bool isShowSign = false)
    {
        if (num <= 999)
        {
            return num.PrettyNumber(isShowSign);
        }
        return KMBTUnit(new BigInteger(num), isShowSign);
    }
    public static string KMBTUnit(this long num, bool isShowSign = false)
    {
        return KMBTUnit(new BigInteger(num), isShowSign);
    }
    public static string KMBTUnit(this decimal num, bool isShowSign = false)
    {
        return KMBTUnit(new BigInteger(num), isShowSign);
    }

    public static string KMBTUnit(this BigInteger num, bool isShowSign = false)
    {
        bool isNegative = num.Sign < 0;
        if (isNegative)
        {
            num = BigInteger.Negate(num);
        }

        //if (num <= 999999)
        //{
        //    return $"{(isNegative ? "-" : "")}{((double)num).PrettyNumber()}";
        //}

        int digits = num.ToString().Length - 1;
        int index = digits / 3;

        double scaledNum = (double)num / Math.Pow(1000, index);
        if (index >= suffixes.Length) index = suffixes.Length - 1;

        return $"{(isNegative ? "-" : "")}{scaledNum.PrettyNumber(isShowSign)}{suffixes[index]}";
    }

    public static string PrettyNumber(this float number,bool isShowSign = false ,int roundNum = 3)
    {
        return PrettyNumberInternal(number, isShowSign, roundNum);
    }

    public static string PrettyNumber(this double number, bool isShowSign = false,  int roundNum = 3)
    {
        return PrettyNumberInternal(number, isShowSign, roundNum);
    }
    public static string PrettyNumber(this ObscuredFloat number, bool isShowSign = false, int roundNum = 3)
    {
        return PrettyNumberInternal(number, isShowSign, roundNum);
    }
    private static string PrettyNumberInternal(double number, bool isShowSign, int roundNum)
    {
        // 숫자가 0인 경우
        if (number == 0)
        {
            return "0";
        }

        string result;
        int digitsBeforeDecimal;
        double scalingFactor;

        if(Math.Abs(number)>=100)
        {
            result = Mathf.FloorToInt((float)number).ToString();
        }
        else if (Math.Abs(number) >= 1)
        {
            digitsBeforeDecimal = (int)Math.Floor(Math.Log10(Math.Abs(number))) + 1;
            scalingFactor = Math.Pow(10, digitsBeforeDecimal - roundNum);
            result = (Math.Round(number / scalingFactor) * scalingFactor).ToString();
        }
        // 숫자의 절대값이 1 미만인 경우
        else
        {
            string numberStr = number.ToString().TrimStart('-', '0', '.');
            int firstNonZeroDigit = numberStr.Length > 0 ? numberStr.IndexOfAny("123456789".ToCharArray()) : -1;

            if (firstNonZeroDigit == -1)
            {
                return "0";
            }

            int totalDigits = firstNonZeroDigit + roundNum;
            scalingFactor = Math.Pow(10, totalDigits);
            result = (Math.Round(number * scalingFactor) / scalingFactor).ToString();
        }

        // isShowSign이 true이고 숫자가 0 이상인 경우, "+" 부호 추가
        if (isShowSign && number > 0)
        {
            result = "+" + result;
        }

        return result;
    }
    public static string OriginName<TEnum>(this TEnum val) where TEnum : Enum
    {
        DescriptionAttribute[] attributes = (DescriptionAttribute[])val
           .GetType()
           .GetField(val.ToString())
           .GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : val.ToString();
    }
    public static async UniTask ContinueWithNullCheck<T>(this UniTask<T> task, Action<T> continuation)
    {
        try
        {
            T result = await task;
            if (result != null)
            {
                continuation(result);
            }
        }
        catch (OperationCanceledException e)
        {
            Debug.LogError($"{e.StackTrace}\n{e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"{e.StackTrace}\n{e.Message}");
        }
    }
    public static Vector2 GetRandomPositionWithin(this RectTransform rectTransform)
    {
        // RectTransform의 코너들을 가져옵니다.
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        // 왼쪽 하단과 오른쪽 상단 코너 사이의 랜덤 위치를 계산합니다.
        float randomX = Random.Range(corners[0].x, corners[2].x);
        float randomY = Random.Range(corners[0].y, corners[2].y);

        return new Vector2(randomX, randomY);
    }
    private static int scaleFactor = 10000;
    private static BigInteger Multiply(this BigInteger bigInt, float floatValue)
    {
        BigInteger scaledMultiplier = new BigInteger(floatValue * scaleFactor);
        return (bigInt * scaledMultiplier) / scaleFactor;
    }
    private static BigInteger Multiply(BigInteger bigInt, double doubleValue)
    {
        BigInteger scaledMultiplier = new BigInteger(doubleValue * scaleFactor);
        return (bigInt * scaledMultiplier) / scaleFactor;
    }

    private static BigInteger ProcessValue<T>(BigInteger result, T value)
    {
        switch (value)
        {
            case int intValue:
                return result * intValue;
            case float floatValue:
                return Multiply(result, floatValue);
            case double doubleValue:
                return Multiply(result, doubleValue);
            case BigInteger bigInteger:
                return result * bigInteger;
            default:
                throw new ArgumentException("Unsupported type");
        }
    }

    public static BigInteger Multiply<T1, T2>(T1 value1, T2 value2)
    {
        BigInteger result = 1;
        result = ProcessValue(result, value1);
        result = ProcessValue(result, value2);
        return result;
    }

    public static BigInteger Multiply<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
    {
        BigInteger result = 1;
        result = ProcessValue(result, value1);
        result = ProcessValue(result, value2);
        result = ProcessValue(result, value3);
        return result;
    }

    public static BigInteger Multiply<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
    {
        BigInteger result = 1;
        result = ProcessValue(result, value1);
        result = ProcessValue(result, value2);
        result = ProcessValue(result, value3);
        result = ProcessValue(result, value4);
        return result;
    }
    public static BigInteger Multiply<T1, T2, T3, T4,T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
    {
        BigInteger result = 1;
        result = ProcessValue(result, value1);
        result = ProcessValue(result, value2);
        result = ProcessValue(result, value3);
        result = ProcessValue(result, value4);
        result = ProcessValue(result, value5);
        return result;
    }
    public static BigInteger Multiply<T1, T2, T3, T4, T5,T6>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5,T6 value6)
    {
        BigInteger result = 1;
        result = ProcessValue(result, value1);
        result = ProcessValue(result, value2);
        result = ProcessValue(result, value3);
        result = ProcessValue(result, value4);
        result = ProcessValue(result, value5);
        result = ProcessValue(result, value6);
        return result;
    }
}