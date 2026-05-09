using Fusion;
using UnityEngine;

// SRP: SOLO carga escenas
// OCP: implementa ISceneLoader, podemos hacer otras versiones
public class FusionSceneLoader : MonoBehaviour, ISceneLoader
{
    [SerializeField] private int _gameSceneIndex = 1;

    private NetworkRunner _runner;

    public void Initialize(NetworkRunner runner)
    {
        _runner = runner;
    }

    public void LoadGameScene()
    {
        if (_runner == null)
        {
            Debug.LogError("[FusionSceneLoader] Runner no inicializado.");
            return;
        }

        Debug.Log("[FusionSceneLoader] Cargando escena de juego...");
        _runner.LoadScene(SceneRef.FromIndex(_gameSceneIndex));
    }
}