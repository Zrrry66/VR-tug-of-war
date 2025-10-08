using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using VRInSync.Network;
using UnityEngine.SceneManagement;
using System.Collections;

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

    public void ReloadScene()
    {
        ReloadSceneRpc();
    }

    // Bind to button OnClick
    public void OnRestartButtonPressed()
    {
        //DisablePhysicsRpc();
        Debug.Log("Restart button is pressed.");
        progressSync.ResetProgress();
        RestartGameServerRpc();          
    }


    [ServerRpc(RequireOwnership = false)]
    private void StartGameServerRpc()
    {
        netState.Value = GameState.Playing;

        // reset gameplay
        progressSync.ResetProgress();

        // start movers after a 3-second delay
        StartCoroutine(DelayedEnableMovers());
        // trigger global music
        musicManager.RequestStartMusicServerRpc();
    }

    // Coroutine: wait 3 seconds, then enable both movers on the server
    private IEnumerator DelayedEnableMovers()
    {
        // wait for 3 seconds before enabling movement
        yield return new WaitForSeconds(3f);

        // enable box mover
        boxAutoMover.EnableMovement();

        // enable all opponent movers
        foreach (var mover in opponentAutoMovers)
            mover.EnableMovement();
    }

    [Rpc(SendTo.Everyone, RequireOwnership = true)]
    private void DisablePhysicsRpc()
    {
        // TODO: remeber the original state of the rigidBody before disabling
        var rigBodys = FindObjectsByType<Rigidbody>(sortMode:FindObjectsSortMode.None);
        foreach(var rigBody in rigBodys)
        {
            rigBody.isKinematic = true;
        }
    }

    [Rpc(SendTo.Everyone, RequireOwnership = true)]
    private void EnablePhysicsRpc()
    {
        // TODO: apply the original state of the rigidBody again
        var rigBodys = FindObjectsByType<Rigidbody>(sortMode: FindObjectsSortMode.None);
        foreach (var rigBody in rigBodys)
        {
            rigBody.isKinematic = false;
        }
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
    [Rpc(SendTo.Everyone)]
    private void ReloadSceneRpc()
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
        
        progressSync.ClearEndStateClientRpc();
        //progressSync.ResetProgress();

        boxAutoMover.EnableMovement();

        //EnablePhysicsRpc();

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
