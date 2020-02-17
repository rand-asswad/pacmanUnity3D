using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PacmanEngine;

public class GameController : MonoBehaviour {
    public bool paused = false;

    public CamChoice camera = CamChoice.Top;
    private Array<CamChoice,Camera> cameras;

    public int score;
    public int lives;
    public int level;

    public float fullSpeed = 11;
    public float fps = 60;

    public float[] ghostSpeed  = new float[2];
    public float[] pacmanSpeed = new float[2];

    public enum GameState : byte {Start, Play, Dead}
    public static GameState gameState;
    public static bool scaredGhosts;
    public int eatGhostPoints = 100;

    public float StartTime = 3;
    public float TimeScared = 8;
    
    public PacmanController pacman;
    public Array<Ghost,GhostController> ghosts;
    public GameObject collectables;

    public int pacdots = 0;
    public int powerpellets = 0;

    void Awake() {
        score = 0;
        lives = 3;
        level = 1;
        pacman = GameObject.Find("Pacman").GetComponent<PacmanController>();

        ghosts = new Array<Ghost, GhostController>();
        ghosts[Ghost.Blinky] = GameObject.Find("Blinky").GetComponent<GhostController>();
        ghosts[Ghost.Pinky] = GameObject.Find("Pinky").GetComponent<GhostController>();
        ghosts[Ghost.Inky] = GameObject.Find("Inky").GetComponent<GhostController>();
        ghosts[Ghost.Clyde] = GameObject.Find("Clyde").GetComponent<GhostController>();

        collectables = GameObject.Find("Collectables");

        SetSpeeds();
        InitCameras();
    }

    void InitCameras() {
        cameras = new Array<CamChoice,Camera>();
        cameras[CamChoice.POV] = GameObject.Find("POV Camera").GetComponent<Camera>();
        cameras[CamChoice.Top] = GameObject.Find("Top Camera").GetComponent<Camera>();
        cameras[CamChoice.BirdView] = GameObject.Find("BirdView Camera").GetComponent<Camera>();
        SetCamera(camera);
    }

    void ToggleCamera() {
        if (Input.GetKeyDown(KeyCode.C)) {
            int i = (int) camera;
            i = (i+1) % cameras.Length;
            camera = (CamChoice) i;
            SetCamera(camera);
        }
    }

    void SetCamera(CamChoice cam) {
        camera = cam;
        foreach (CamChoice c in cameras.keys) {
            cameras[c].enabled = (c == cam);
        }
        pacman.transform.localRotation = Quaternion.Euler(0, pacman.transform.localRotation.y, 0);
        if (cam == CamChoice.Top) {
            pacman.relativeTo = Space.World;
            pacman.transform.localRotation *= Quaternion.Euler(0, 0, 90);
        }
        else pacman.relativeTo = Space.Self;
    }
    
    void Update() {
        pauseControl();
        ToggleCamera();
        WinLevel();
    }

    void ResetCharacters() {
        pacman.Reset();
        foreach(GhostController ghost in ghosts) ghost.Reset();
    }

    void ResetCollectables() {
        foreach (Transform child in collectables.transform) {
            pacdots = powerpellets = 0;
            child.gameObject.SetActive(true);
            if (child.gameObject.CompareTag("Pac-Dot")) pacdots++;
            else if (child.gameObject.CompareTag("Power-Pellet")) powerpellets++;
        }
    }

    public void LoseLife() {
        lives--;
        Pause();
        if (lives > 0) {
            ResetCharacters();
            Resume();
        } else {
            print("Game over");
        }
    }

    void WinLevel() {
        if ((pacdots + powerpellets) == 0) {
            print("YOU WON!");
            Pause();
            level++;
            ResetCollectables();
            ResetCharacters();
            Resume();
        }
    }

    public void ScareGhosts() {
        foreach (GhostController ghost in ghosts) ghost.Scare();
    }


    void SetSpeeds() {
        ghostSpeed[0] = 0.75f;
        ghostSpeed[1]= 0.5f;
        pacmanSpeed[0] = 0.8f;
        pacmanSpeed[1] = 0.9f;
    }

    void pauseControl() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (paused) Resume();
            else Pause();
        }
    }

    void Resume() {
        paused = false;
        Time.timeScale = 1f;
    }
    
    void Pause() {
        paused = true;
        Time.timeScale = 0;
    }
}
