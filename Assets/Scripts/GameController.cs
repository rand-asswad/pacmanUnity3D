using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PacmanEngine;

public class GameController : MonoBehaviour {

    public enum GameState : byte {Start, Play, Dead}
    public static GameState gameState;
    public static bool scaredGhosts;

    public float StartTime = 3;
    
    public PacmanController pacman;
    public Array<Ghost,GhostController> ghosts;

    public byte pacdots = 0;
    public byte powerpellets = 0;

    void Awake() {
        pacman = GameObject.Find("Pacman").GetComponent<PacmanController>();

        ghosts = new Array<Ghost, GhostController>();
        ghosts[Ghost.Blinky] = GameObject.Find("Blinky").GetComponent<GhostController>();
        ghosts[Ghost.Pinky] = GameObject.Find("Pinky").GetComponent<GhostController>();
        ghosts[Ghost.Inky] = GameObject.Find("Inky").GetComponent<GhostController>();
        ghosts[Ghost.Clyde] = GameObject.Find("Clyde").GetComponent<GhostController>();
    }

    public void ScareGhosts() {
        foreach (GhostController ghost in ghosts) ghost.Scare();
    }

    void Update() {
        if ((pacdots | powerpellets) == 0) {
            print("YOU WON!!!");
        }
    }
}
