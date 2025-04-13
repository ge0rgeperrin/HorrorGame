#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using System.Collections;
using Steamworks;
#endif

using UnityEngine;
using Beebyte.Obfuscator;
using System.Text;
using System;
using PallonAnticheat;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Subterranea;
using Logger = PallonAnticheat.Logger;

[Skip]
public static class PlayerManager
{
    public static string Username { get { return GetUsername(); } set { SetUsername(value); } }

    public static Color Color { get { return GetColor(); } set { SetColor(value); } }

    public static string InfoDump => GetInfoDump();
    
    

    /// <summary>
    /// Returns battery amount with percentage sign (%)
    /// </summary>
    /// <returns>000%</returns>
    public static string GetBatteryPercentage()
    {
        return $"{GetBatteryAmount()}%";
    }
    
    /// <summary>
    /// Battery amount of VR headset (DOES NOT WORK ON PC)
    /// </summary>
    /// <returns>A value (float) of 1-100</returns>
    public static float GetBatteryAmount()
    {
        if (LoginManager.IsOnQuest)
        {
            float battery = SystemInfo.batteryLevel;
            if (battery < 0) 
                return -1f; 
            return Mathf.Round(battery * 100f);
        }
        MonkeLogger.Log(log: "Cannot get battery level on PC!", level: MonkeLogger.LogLevel.Error);
        return 0f;
    }

    /// <summary>
    /// Battery level of VR headset (DOES NOT WORK ON PC)
    /// </summary>
    /// <returns>A level enum that depends on battery amount range</returns>
    public static BatteryLevel GetBatteryLevel()
    {
        if (LoginManager.IsOnQuest)
        {
            float batteryAmount = GetBatteryAmount();
            if (batteryAmount < 0)
                return BatteryLevel.InvalidBatteryAmountGiven;
            if (batteryAmount <= 35f) 
                return BatteryLevel.Level1;
            if (batteryAmount <= 60f) 
                return BatteryLevel.Level2;
            if (batteryAmount <= 80f) 
                return BatteryLevel.Level3;
            if (batteryAmount <= 100f) 
                return BatteryLevel.Level4;
        }
        return BatteryLevel.PCOrNotReady;
    }

    public enum BatteryLevel
    {
        InvalidBatteryAmountGiven,
        PCOrNotReady,
        Level1,
        Level2,
        Level3,
        Level4
    }

    private static string GetInfoDump()
    {
        return $"Device Identifier: {SystemInfo.deviceUniqueIdentifier}-Device Name: {SystemInfo.deviceName}";
    }

    /// <summary> Returns a Regexed, clean version of the player's platform username! </summary>
    public static string PlatformUsername
    {
        get
        {
            string platformUsername;
            
#if DISABLESTEAMWORKS
            platformUsername = OculusManager.UserData.Username;
#else
            platformUsername = SteamManager.UserData.SteamUsername;
#endif
            
            string cleanPlatformUsername = Regex.Replace(platformUsername, @"[^A-Za-z0-9_]", "").ToUpper();

            if (!string.IsNullOrEmpty(cleanPlatformUsername))
            {
                return cleanPlatformUsername;
            }

            return DesperateFallback();
        }
    }
    
    public static string Fallback()
    {
#if DISABLESTEAMWORKS
        return OculusManager.UserData.Username;
#else
        return SteamManager.UserData.SteamUsername;
#endif
    }
    
    public static string DesperateFallback() => $"MONKEY{UnityEngine.Random.Range(50000, 60000)}";

    public static byte[] NetworkReady(string s) => Encoding.UTF8.GetBytes(s);

    public static string NormalUseReady(byte[] b) => Encoding.UTF8.GetString(b);

    private static Color GetColor()
    {
        Color loadColor = JsonUtility.FromJson<Color>(PlayerPrefs.GetString("Color"));
        return loadColor;
    }
    
    private static void SetColor(Color color)
    {
        PlayerPrefs.SetString("Color", JsonUtility.ToJson(color));
        
        /*Player.Instance.UpdateOfflineColor();-
        if (MonkeNetworkManager.RoomData.InRoom)
            MonkeNetworkPlayer.Instance.SetColor(color);*/
    }

    private static void SetUsername(string newUsername = "MONKEY")
    {
        if (string.IsNullOrEmpty(newUsername) || newUsername.Length < 3 || newUsername.Length > 23)
        {
            if (newUsername != Fallback())
            {
                Debug.LogError("Invalid username. Fallbacking to Platform username..");
                Username = Fallback();
                return;
            }
        }

        newUsername = Regex.Replace(newUsername, @"[^A-Za-z0-9_]", "").ToUpper();

        if (string.IsNullOrEmpty(newUsername))
        {
            newUsername = DesperateFallback();
        }

        MonkeLogger.Log($"(PlayerManager) Attempting to set username to {newUsername}!");

        /*if (MonkeNetworkManager.Instance && MonkeNetworkManager.RoomData.InRoom)
            MonkeNetworkPlayer.Instance.SetUsername(NetworkReady(newUsername));*/

        PlayerPrefs.SetString(PallonAnticheat.Constants.usernamePlayerPrefsKey, newUsername);
        
        MonkeLogger.Log($"Set username as {newUsername}");
    }

    #region Time Converting Shit

    /// <summary> Returns the local time in 12-hour format. </summary>
    /// <returns> MM/DD/YY HH:MM:SS AM/PM</returns>
    public static string GetLocalTimeIn12Hour() => TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local).ToString();

    /// <summary> Returns the date from the DateTimeOffset provided </summary>
    /// <param name="dt"></param>
    /// <returns> MM/DD/YYYY </returns>
    public static string GetDate(DateTimeOffset dt)
    {
        dt = TimeZoneInfo.ConvertTime(dt, TimeZoneInfo.Local);
        return $"{dt.Month}/{dt.Day}/{dt.Year}";
    }

    /// <summary> Returns the date from the DateTime provided </summary>
    /// <param name="dt"></param>
    /// <returns> MM/DD/YYYY </returns>
    public static string GetDate(DateTime dt)
    {
        dt = TimeZoneInfo.ConvertTime(dt, TimeZoneInfo.Local);
        return $"{dt.Month}/{dt.Day}/{dt.Year}";
    }

    /// <summary> Returns the time from the DateTimeOffset provided </summary>
    /// <param name="dt"></param>
    /// <returns> HH/MM/SS </returns>
    public static string GetTime(DateTimeOffset dt)
    {
        dt = TimeZoneInfo.ConvertTime(dt, TimeZoneInfo.Local);
        return $"{dt.Hour}:{dt.TimeOfDay.Minutes:D2} {dt:tt}";
    }

    /// <summary> Returns the time from the DateTime provided </summary>
    /// <param name="dt"></param>
    /// <returns> HH/MM/SS </returns>
    public static string GetTime(DateTime dt)
    {
        dt = TimeZoneInfo.ConvertTime(dt, TimeZoneInfo.Local);
        return $"{dt.Hour}:{dt.TimeOfDay.Minutes:D2} {dt:tt}";
    }

    /// <summary>
    /// Example: "Wednesday, May 16, 2001"
    /// </summary>
    public static string Today => LocalDateTimeNow.ToLongDateString();
    /// <summary>
    /// Example: "Wednesday"
    /// </summary>
    public static string TodayWeekday => LocalDateTimeNow.DayOfWeek.ToString();
    

    public static DateTimeOffset LocalDateTimeOffsetNow => TimeZoneInfo.ConvertTime(DateTimeOffset.Now, TimeZoneInfo.Local);
    public static DateTime LocalDateTimeNow => System.DateTime.Now.ToLocalTime();

    #endregion

    public static PlayerLeaderboardEntry GetLocalLeaderboardEntry(List<PlayerLeaderboardEntry> target)
    {
        if (target != null)
        {
            return target.FirstOrDefault(e => LoginManager.Helpers.IsLocalPlayer(e.PlayFabId));
        }
        MonkeLogger.Log(level: MonkeLogger.LogLevel.Error, log: "Failed to find local player in leaderboard entries list provided!");
        return null;
    }

    private static string GetUsername() => PlayerPrefs.GetString(PallonAnticheat.Constants.usernamePlayerPrefsKey);
}