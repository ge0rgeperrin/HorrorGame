using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Steamworks;
using UnityEngine.XR;

public static class SubterraneaUtils
{
    public static Platform CurrentPlatform => GetPlatform();
    
    private static Platform GetPlatform()
    {
        var platform = Application.platform;

        if (platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor)
            return XRSettings.enabled ? Platform.WindowsVR : Platform.WindowsPC;

        if (platform == RuntimePlatform.OSXPlayer || platform == RuntimePlatform.OSXEditor)
            return Platform.Mac;

        if (platform == RuntimePlatform.Android)
            return XRSettings.enabled ? Platform.MetaQuest : Platform.Android;

        return Platform.Error;
    }
}

public enum Platform { WindowsPC, WindowsVR, Mac, MetaQuest, Android, iOS, Error }
