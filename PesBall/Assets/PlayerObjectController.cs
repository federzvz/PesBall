using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;

public class PlayerObjectController : NetworkBehaviour
{
    //Player data
    [SyncVar] public int ConnectionID;
    [SyncVar] public int PlayerIdNumber;
    [SyncVar] public ulong PlayerSteamID;
    [SyncVar(hook = nameof(PlayerNameUpdate))] public string PlayerName;

    private CustomNetworkManager manager;

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private CustomNetworkManager Manager
    {
        get {
            if (manager != null) {
                return manager;
            }
            return manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }

    public override void OnStartAuthority()
    {
        CmdSetPlayerName(SteamFriends.GetPersonaName().ToString());
        gameObject.name = "LocalGamePlayer";
        LobbyController.Instance.FindLocalPlayer();
        LobbyController.Instance.UpdateLobbyName();
    }

    public override void OnStartClient()
    {
        Manager.GamePlayers.Add(this);
        LobbyController.Instance.UpdateLobbyName();
        LobbyController.Instance.UpdatePlayerList();
    }

    public override void OnStopClient()
    {
        Manager.GamePlayers.Remove(this);
        LobbyController.Instance.UpdatePlayerList();
    }

    [Command]
    private void CmdSetPlayerName(string PlayerName) {
        this.PlayerNameUpdate(this.PlayerName, PlayerName);
    }

    public void PlayerNameUpdate(string OldValue, string NewValue) {
        if (isServer) { // Host
            this.PlayerName = NewValue;
        }
        if (isClient) { // Client
            LobbyController.Instance.UpdatePlayerList();
        }
        
    }

    //Start Game
    public void CanStartGame(string SceneName) {
        if (hasAuthority) {
            cmdCanStartGame(SceneName);
        }
    }

    [Command]
    public void cmdCanStartGame(string SceneName)
    {
        manager.StartGame(SceneName);
    }

}
