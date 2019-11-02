using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public enum State
    {
        Booting, Introduction, Main_menu, Sequence, Level_playing, Level_lost, Level_won, Credits, Exiting
    }
    private State _gameState = State.Booting;
    public State GameState
    {
        private set
        {
            State previous = _gameState;
            _gameState = value;
            OnStateChange(previous, _gameState);
        }
        get
        {
            return _gameState;
        }
    }

    [Header("Levels")]
    [SerializeField] LevelController[] levels;
    [SerializeField] int currentLevelIdx = -1;
    public LevelController CurrentLevel
    {
        get
        {
            if (currentLevelIdx >= 0 && currentLevelIdx < levels.Length)
            {
                return levels[currentLevelIdx];
            }
            else
            {
                return null;
            }
        }
    }
    [Header("Sequences")]
    [SerializeField] Sequence sequence_introduction;
    [SerializeField] Sequence sequence_credits;
    [SerializeField] Sequence sequence_lose;
    [SerializeField] Sequence sequence_win;
    protected Sequence currentSequence;

    [Header("Menus")]
    [SerializeField] Menu mainMenu;
    [SerializeField] Menu optionMenu;

    [Header("Controls")]
    [SerializeField] InputManager[] inputOptions;
    [SerializeField] InputManager input;
    [SerializeField] UnityEngine.CursorLockMode cursorLock;

    [Header("Status")]
    public bool waiting = false;
    public bool paused = false;

    #region Events
    public delegate void GameStateChangeEvent(State previous, State Current);
    public GameStateChangeEvent OnStateChange;
    #endregion


    /** This is launched at start
     * * show any introductory sequences
     **/
    protected virtual void Start()
    {
        UnityEngine.Cursor.lockState = cursorLock;

        DoIntroduction();
    }

    /** This loop will be awake for the whole game lifespan
     * * handle levels
     * * handle pauses 
     * * handle menus
     **/
    protected virtual void Update()
    {
        /** Cancel sequences, if possible **/
        if (currentSequence != null && Input.GetKeyDown(input.key_cancel))
        {
            currentSequence.TryCancel();
        }

        if (!waiting)
        {
            if (GameState == State.Level_playing || GameState == State.Level_lost)
            {
                /** handle pause **/
                if (Input.GetKeyDown(input.key_pause))
                {
                    if (!paused)
                    {
                        DoPause();
                    }
                    else
                    {
                        DoUnpause();
                    }
                }
            }
        }
    }

    #region Pause management
    protected virtual void DoPause()
    {
        Debug.LogWarning("Game Paused");
        paused = true;
    }
    protected virtual void DoUnpause()
    {
        Debug.LogWarning("Game Un-Paused");
        paused = false;
    }
    #endregion

    #region Introduction management
    void DoIntroduction()
    {
        GameState = State.Introduction;
        if (sequence_introduction)
        {
            currentSequence = sequence_introduction;
            waiting = true;
            sequence_introduction.Run(OnIntroductionEnd);

        }
        else
        {
            OnIntroductionEnd();
        }
    }
    void OnIntroductionEnd()
    {
        if (mainMenu)
        {
            GameState = State.Main_menu;
            mainMenu.Run(null);

        }
        else
        {
            PlayFirstLevel();
        }
        waiting = false;
        currentSequence = null;
    }
    #endregion

    #region Level management
    protected void PlayFirstLevel()
    {
        PlayLevel(0);
    }
    protected void PlayNextLevel()
    {
        PlayLevel(currentLevelIdx + 1);
    }
    protected void PlayLevel(int level)
    {
        if (CurrentLevel)
        {
            if (currentLevelIdx != level)
            {
                CurrentLevel.EndLevel();
            }
            else
            {
                CurrentLevel.ResetLevel();
            }
        }
        currentLevelIdx = level;
        if (CurrentLevel)
        {
            GameState = State.Level_playing;
            CurrentLevel.StartLevel();
        }
        else
        {
            Debug.LogError("No level #" + currentLevelIdx);
        }
    }
    #endregion
    
    #region Win/Lose management // TODO: move to level?
    protected virtual void DoLoseLevel()
    {
        GameState = State.Level_lost;
        if (sequence_lose != null)
        {
            sequence_lose.Run(OnLoseLevelEnd);
            currentSequence = sequence_lose;
            waiting = true;
        }
        else
        {
            OnLoseLevelEnd();
        }
    }

    void OnLoseLevelEnd()
    {
        CurrentLevel.ResetLevel();
        waiting = false;
        currentSequence = null;
    }

    protected virtual void DoWinLevel()
    {
        GameState = State.Level_won;
        if (sequence_win != null)
        {
            sequence_win.Run(OnWinLevelEnd);
            currentSequence = sequence_lose;
            waiting = true;
        }
        else
        {
            OnWinLevelEnd();
        }
    }

    protected virtual void OnWinLevelEnd()
    {
        PlayNextLevel();
        waiting = false;
        currentSequence = null;
    }
    #endregion

    void DoExit()
    {
        GameState = State.Exiting;
    }
}

