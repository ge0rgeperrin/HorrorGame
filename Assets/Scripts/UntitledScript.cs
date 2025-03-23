using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PlayFab;
using PlayFab.ClientModels;

using EpicTransport;

public class UntitledScript : MonoBehaviour
{
    internal static string untitledId { get { return id; } }
    private static string id;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => EOSSDKComponent.Initialized);

        var request = new LoginWithCustomIDRequest()
        {
            CustomId = EOSSDKComponent.LocalUserProductId.ToString(),
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams { GetPlayerProfile = true }
        };

        PlayFabClientAPI.LoginWithCustomID(request, (result) =>
        {
            Debug.Log("Logged into PlayFab");
            id = result.PlayFabId;
        },
        (error) =>
        {
            Debug.LogError(error.ErrorMessage);
        });
    }
}
