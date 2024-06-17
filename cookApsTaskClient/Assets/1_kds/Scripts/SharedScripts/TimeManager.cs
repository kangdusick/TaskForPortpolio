using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

public static class TimeManager
{
    private static Dictionary<int, WaitForSeconds> _waitForSecondsDict = new Dictionary<int, WaitForSeconds>();
    private static int _initTick;
    private static DateTime _initDateTime;
    private static DateTime nextRefreshTime;
    public static bool isInit = false;
    public static async UniTask Init()
    {
        if(isInit)
        {
            return;
        }
        isInit = false;
        _initTick = Environment.TickCount;
        _initDateTime = DateTime.UtcNow.AddHours(9);
        await RefreshTimeManager();
        AutoRefreshRoutine();

    }
    private static async void AutoRefreshRoutine()
    {
        while (true)
        {
            if(nextRefreshTime<GetTickUTCNow())
            {
                await RefreshTimeManager();
            }
            await UniTask.Delay(1000);
        }
    }
    private static async UniTask RefreshTimeManager()
    {
        bool isRefresh = false;
        while (!isRefresh)
        {
            using (var request = UnityWebRequest.Get("www.naver.com"))//네이버 서버시간을 가져온다.
            {
                try
                {
                    await request.SendWebRequest();
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        var date = request.GetResponseHeader("date");
                        var parseDate = DateTime.Parse(date);
                        _initTick = Environment.TickCount;
                        _initDateTime = parseDate;
                        Debug.Log("Refresh TimeManager");
                        isRefresh = true;
                        isInit = true;
                        nextRefreshTime = GetTickUTCNow().AddMinutes(10);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.Message);
                    Debug.LogWarning(request.error);
                    await UniTask.Delay(1000);
                }

            }
        }
          
    }
    public static WaitForSeconds GetWaitForSeconds(float seconds)
    {
        int key = Mathf.FloorToInt(seconds * 1000f);
        if (!_waitForSecondsDict.ContainsKey(key))
        {
            _waitForSecondsDict.Add(key, new WaitForSeconds(seconds));
        }
        return _waitForSecondsDict[key];
    }
    private static int GetTimeElapseMilliSce()
    {
        return Environment.TickCount - _initTick;
    }

    public static DateTime GetTickUTCNow()
    {
        return _initDateTime.AddMilliseconds(GetTimeElapseMilliSce());
    }
}
