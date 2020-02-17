using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PacmanEngine;

public class GameUI : MonoBehaviour {

    private GameController game;
    private Canvas gameUI;

    public Text score;
    public Text lives;
    public Text level;

    public Text pause;
    public GameObject mainMenu;

    void Awake() {
        game = GameObject.Find("Game Engine").GetComponent<GameController>();
    }

    void Update() {
        score.text = "SCORE " + game.score.ToString();
        lives.text = "LIVES " + game.lives.ToString();
        level.text = "LEVEL " + game.level.ToString();

        switch (game.state) {
            case GameState.Init: pause.text = "GET READY!!!"; break;
            case GameState.Play: break;
            case GameState.Pause: pause.text = "PAUSED"; break;
            case GameState.Over: pause.text = "GAME OVER"; break;
        }

        pause.gameObject.SetActive(game.paused || game.state == GameState.Init);
        mainMenu.SetActive(game.state == GameState.Pause || game.state == GameState.Over);
    }
}
