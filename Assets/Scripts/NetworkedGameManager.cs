using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using VRInSync.Network;
using UnityEngine.SceneManagement;

public class NetworkedGameManager : NetworkBehaviour
{
    public GameObject LobbyPanel;
    //public GameObject InGamePanel;
    public GameObject PausePanel;

    public NetworkedProgressBar progressSync;
    public BoxAutoMover boxAutoMover;
    public OpponentAutoMover[] opponentAutoMovers;
    public NetworkMusicManager musicManager;
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

    // Called by Start Button (inspector)
    public void StartGame()
    {
        StartGameServerRpc();
    }

    // Called by Resume Button (inspector)
    public void ResumeGame()
    {
        TogglePauseServerRpc();
    }

    // Called by Restart Buttons (inspector)
    public void RestartGame()
    {
        RestartGameServerRpc();
    }

    // Bind to button OnClick
    public void OnRestartButtonPressed()
    {
        Debug.Log("Restart button is pressed.");
        RestartGameServerRpc();
        progressSync.ClearEndStateClientRpc();
        progressSync.ResetProgress();             
    }


    [ServerRpc(RequireOwnership = false)]
    private void StartGameServerRpc()
    {
        netState.Value = GameState.Playing;

        // reset gameplay
        progressSync.ResetProgress();
        boxAutoMover.EnableMovement();
        foreach (var mover in opponentAutoMovers)
            mover.EnableMovement();

        // trigger global music
        musicManager.RequestStartMusicServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void TogglePauseServerRpc()
    {
        if (netState.Value == GameState.Playing)
        {
            netState.Value = GameState.Paused;
            Time.timeScale = 0f;
            boxAutoMover.DisableMovement();
            foreach (var mover in opponentAutoMovers)
                mover.DisableMovement();
            musicManager.PauseMusicClientRpc();
        }
        else if (netState.Value == GameState.Paused)
        {
            netState.Value = GameState.Playing;
            Time.timeScale = 1f;
            boxAutoMover.EnableMovement();
            foreach (var mover in opponentAutoMovers)
                mover.EnableMovement();
            musicManager.ResumeMusicClientRpc();
        }
    }

    //Reload scene: will go back to create lobby panel
    [ClientRpc]
    private void ReloadSceneClientRpc()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RestartGameServerRpc()
    {
        //ReloadSceneClientRpc();

        netState.Value = GameState.Playing;
        
        Time.timeScale = 1f;
        
        foreach (var mover in opponentAutoMovers)
        {
            mover.ResetPosition();
            mover.EnableMovement();
        }
        
        //progressSync.ClearEndStateClientRpc();
        //progressSync.ResetProgress();

        boxAutoMover.EnableMovement();
        

        // reschedule global music
        musicManager.StopMusicClientRpc();
        musicManager.RequestStartMusicServerRpc();
    }

    private void UpdateUI(GameState state)
    {
        LobbyPanel.SetActive(state == GameState.Lobby);
        //InGamePanel.SetActive(state == GameState.Playing);
        PausePanel.SetActive(state == GameState.Paused);
    }
}
