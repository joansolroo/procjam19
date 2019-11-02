using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    [SerializeField] public GameController game;

    [Header("Links")]
    [SerializeField] public Player player;
    [SerializeField] public GameObject resetables;
    [SerializeField] public Transform respawn;

    public bool initialized = false;
    [SerializeField] GameObject instanciatedScene;


    #region Events
    public delegate void LevelEvent();
    public LevelEvent OnLevelReset;
    #endregion

    private void Start()
    {
        PrepareScene();
    }
    public virtual void PrepareScene()
    {
        resetables.SetActive(false);
    }
    public void StartLevel()
    {
        if (!initialized)
        {
            ResetLevel();
            initialized = true;
        }
    }
    public virtual void ResetLevel()
    {
        if (instanciatedScene)
        {
            DestroyImmediate(instanciatedScene);
        }
        instanciatedScene = Instantiate(resetables);
        instanciatedScene.SetActive(true);

        player.transform.position = respawn.transform.position;
        player.ResetChanges();
        player.gameObject.SetActive(true);

        OnLevelReset?.Invoke();
    }
    public virtual void EndLevel()
    {
        Debug.LogWarning("End level.");
    }
}
