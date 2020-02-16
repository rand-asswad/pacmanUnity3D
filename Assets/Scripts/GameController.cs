using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PacmanEngine;

public class GameController : MonoBehaviour {

    public enum GameState : byte {Start, Play, Dead}
    public static GameState gameState;
    public static bool scaredGhosts;
    
    public GameObject pacman;
    public Array<Ghost,GameObject> ghosts;

    void Awake() {
        pacman = GameObject.Find("Pacman");

        ghosts = new Array<Ghost, GameObject>();
        ghosts[Ghost.Blinky] = GameObject.Find("Blinky");
        ghosts[Ghost.Pinky] = GameObject.Find("Pinky");
        ghosts[Ghost.Inky] = GameObject.Find("Inky");
        ghosts[Ghost.Clyde] = GameObject.Find("Clyde");
    }

}
