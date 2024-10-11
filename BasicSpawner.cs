using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner _runner;
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    DynamicJoystick dj;
    ActionJoystick aj;

    Vector3 spawnPosition;
    bool isVisible;

    private void Awake()
    {
        dj = FindObjectOfType<DynamicJoystick>();
        aj = FindObjectOfType<ActionJoystick>();
    }

    public void Setup(bool visible)
    {
        isVisible = visible;
        if (isVisible)
        {
            spawnPosition = new Vector3(GameManager.gm.spawnPos.x - 4, 1, GameManager.gm.spawnPos.z - 4);
        }
        else
        {
            spawnPosition = GameManager.gm.character[0].transform.parent.position;
        }
        enterWorld();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer)
        {
           NetworkObject netObj =  runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
            _spawnedCharacters.Add(player, netObj);
        }
    }

    /*public void regPlayer(NetworkObject netObj)
    {
        _spawnedCharacters.Add(netObj.InputAuthority, netObj);
    }*/

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if(_spawnedCharacters.TryGetValue(player, out NetworkObject netObj))
        {
            _spawnedCharacters[player].RequestStateAuthority();
            runner.Despawn(netObj);
            _spawnedCharacters.Remove(player);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        data.direction = new Vector3(dj.Horizontal, 0, dj.Vertical);

        if (Input.GetKey(KeyCode.W))
            data.direction += Vector3.forward;

        if (Input.GetKey(KeyCode.S))
            data.direction += Vector3.back;

        if (Input.GetKey(KeyCode.A))
            data.direction += Vector3.left;

        if (Input.GetKey(KeyCode.D))
            data.direction += Vector3.right;

        input.Set(data);
    }

    public void takeOver(NetworkObject netObj)
    {
        netObj.RequestStateAuthority();
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        throw new NotImplementedException();
    }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
        throw new NotImplementedException();
    }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void enterWorld()
    {
        if (DataServer.call.isBHMode || DataServer.call.isTHMode)
        {
            RectTransform rect = dj.gameObject.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(rect.sizeDelta.x * -1, rect.anchoredPosition.y);

            rect = aj.gameObject.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(rect.sizeDelta.x, rect.anchoredPosition.y);

            StartGame(GameMode.Single);
        }
        else
        {
            StartGame(GameMode.Shared);
        }
    }
    public void exitWorld()
    {
        _runner.Shutdown();
    }

    async void StartGame(GameMode mode)
    {
        if (_runner != null)
            return;

        // Create the Fusion runner and let it know that we will be providing user input
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        // Start or join (depends on gamemode) a session with a specific name
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = DataServer.call.isBHMode || DataServer.call.isTHMode ? DataServer.call.uID.Substring(0, 8) + DataServer.call.uID.Substring(DataServer.call.uID.Length - 8, 4) : GameManager.gm.roomID,
            Scene = SceneManager.GetActiveScene().buildIndex,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }
    
    public void visibleChar()
    {
        _runner.IsVisible = true;
    }

    public void disableChar()
    {
        _runner.IsVisible = false;
    }
}
