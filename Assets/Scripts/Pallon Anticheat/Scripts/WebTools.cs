using System;
using System.Collections;
using Subterranea;
using UnityEngine.Networking;

namespace PallonAnticheat
{
    using UnityEngine;

    public static class WebTools
    {
        public static IEnumerator DownloadImage(string url, Action<Texture2D> callback)
        {
            if (LoginManager.LoggedIn)
            {
                using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Texture2D image = ((DownloadHandlerTexture)request.downloadHandler).texture;
                    image.filterMode = FilterMode.Point;
                    callback?.Invoke(image);
                }
                else
                {
                    MonkeLogger.Log(level: MonkeLogger.LogLevel.Error,
                        log:
                        $"Failed to download the players profile pic! Fallbacking to error image on servers. Error: {request.error}");
                    LoginManager.Instance.StartCoroutine(DownloadImage(Constants.FailedToDownloadURL,
                        c => { callback?.Invoke(c); }));
                }
            }
        }
    }
}