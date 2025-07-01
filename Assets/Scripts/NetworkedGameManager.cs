using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using VRInSync.Network;


public class NetworkedGameManager : NetworkBehaviour
{
    public GameObject LobbyPanel;
    public GameObject InGamePanel;
    public GameObject PausePanel;

    public BoxProgressSync progressSync;
    public BoxAutoMover boxAutoMover;
    public OpponentAutoMover opponentAutoMover;
    public NetworkMusicManager musicManager;    // 拖入 NetworkMusicManager
    public InputActionReference pauseAction;

    public enum GameState : byte { Lobby, Playing, Paused }
    private NetworkVariable<GameState> netState =
        new NetworkVariable<GameState>(
            GameState.Lobby,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    private void OnEnable()
    {
        pauseAction.action.performed += OnPausePressed;
        pauseAction.action.Enable();
    }

    private void OnDisable()
    {
        pauseAction.action.performed -= OnPausePressed;
        pauseAction.action.Disable();
    }

    public override void OnNetworkSpawn()
    {
        netState.OnValueChanged += (_, newState) => UpdateUI(newState);
        UpdateUI(netState.Value);
    }

    private void OnPausePressed(InputAction.CallbackContext ctx)
    {
        if (netState.Value == GameState.Playing || netState.Value == GameState.Paused)
            TogglePauseServerRpc();
    }

    // 客户端 Start 按钮调用
    public void StartGame()
    {
        StartGameServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartGameServerRpc()
    {
        netState.Value = GameState.Playing;

        // reset gameplay
        //progressSync.ResetProgress();
        boxAutoMover.EnableMovement();
        opponentAutoMover.EnableMovement();

        // 让音乐在全局同步后开始
        musicManager.RequestStartMusicServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void TogglePauseServerRpc()
    {
        if (netState.Value == GameState.Playing)
        {
            netState.Value = GameState.Paused;

            // pause gameplay
            Time.timeScale = 0f;
            boxAutoMover.DisableMovement();
            opponentAutoMover.DisableMovement();

            // pause music on all clients
            musicManager.PauseMusicClientRpc();
        }
        else if (netState.Value == GameState.Paused)
        {
            netState.Value = GameState.Playing;

            // resume gameplay
            Time.timeScale = 1f;
            boxAutoMover.EnableMovement();
            opponentAutoMover.EnableMovement();

            // resume music on all clients
            musicManager.ResumeMusicClientRpc();
        }
    }

    // 客户端 Restart 按钮调用
    public void RestartGame()
    {
        RestartGameServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RestartGameServerRpc()
    {
        netState.Value = GameState.Playing;

        // reset gameplay
        //progressSync.ResetProgress();
        boxAutoMover.EnableMovement();
        opponentAutoMover.EnableMovement();

        // 重新调度全局音乐播放
        musicManager.RequestStartMusicServerRpc();
    }

    // 根据网络状态切面板
    private void UpdateUI(GameState state)
    {
        LobbyPanel.SetActive(state == GameState.Lobby);
        InGamePanel.SetActive(state == GameState.Playing);
        PausePanel.SetActive(state == GameState.Paused);
    }
}
