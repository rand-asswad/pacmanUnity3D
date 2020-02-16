using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PacmanEngine;

public class GhostController : MonoBehaviour {

    private GameController gc;
    private Maze maze;

    public GhostState state;
    private GhostState nextState;
    private Ghost ghost;
    private bool isDead;

    private GameObject ghostBody;
    private Material defaultColor;
    private Material scaredColor;
    private Material scaredFlash;
    private float flashInterval = 0.15f;
    private byte flashCounter = 10;

    private Vector3 scatterTarget;
    private Vector3 target;
    private Vector3 dest;
    private Direction dir;
    private Tile nextTile;

    public GameObject test;

    public float speed = 10; // to be checked later

    public float TimeHome = 0; // ghost-specific
    public float TimeScatter = 7;
    public float TimeChase = 20;
    public float TimeScared = 8;
    
    private float TimeRemaining;

    public float EndTimeHome;
    public float EndTimeScatter;
    public float EndTimeChase;
    public float EndTimeScared;


    private List<Vector3> LeaveHome;
    private int HomeIndex;

    private Array<Direction,Quaternion> Rotation;

    void Start() {
        AssignGhost();
        InitRotation();

        gc = GameObject.Find("Game Engine").GetComponent<GameController>();
        maze = GameObject.Find("Maze").GetComponent<Maze>();
        ghostBody = gameObject.transform.Find("GhostBody").gameObject;

        defaultColor = ghostBody.GetComponent<Renderer>().material;
        scaredColor = Resources.Load("Materials/ScaredBlue", typeof(Material)) as Material;
        scaredFlash = Resources.Load("Materials/ScaredWhite", typeof(Material)) as Material;
        ghostBody.GetComponent<Renderer>().material = defaultColor;

        nextTile = maze.GetNearestTile(maze.ghostHouse + new Vector3(3.5f, 0f, 5f));
        
        LeaveHome = new List<Vector3>();

        switch (ghost) {
            case (Ghost.Blinky) :
                TimeHome = 0;
                dir = Direction.left; nextTile = nextTile.left;
                scatterTarget = new Vector3(maze.width - 2, 0, maze.height + 4);
                break;
            case (Ghost.Pinky) :
                TimeHome = 0.5f;
                LeaveHome.Add(maze.ghostHouse + new Vector3(3.5f, 0f, 2f));
                scatterTarget = new Vector3(2, 0, maze.height + 4);
                break;
            case (Ghost.Inky) :
                TimeHome = 4.5f;
                LeaveHome.Add(maze.ghostHouse + new Vector3(1.5f, 0f, 2f));
                LeaveHome.Add(maze.ghostHouse + new Vector3(3.5f, 0f, 2f));
                scatterTarget = new Vector3(maze.width, 0, -1);
                break;
            case (Ghost.Clyde) :
                TimeHome = 8;
                LeaveHome.Add(maze.ghostHouse + new Vector3(5.5f, 0f, 2f));
                LeaveHome.Add(maze.ghostHouse + new Vector3(3.5f, 0f, 2f));
                scatterTarget = new Vector3(0, 0, -1);
                break;
        }
        LeaveHome.Add(maze.ghostHouse + new Vector3(3.5f, 0f, 5f));
        LeaveHome.Add(nextTile.pos);
        HomeIndex = 0;
        

        //transform.position = initialPosition;
        transform.position = LeaveHome[0];

        EndTimeHome = Time.time + gc.StartTime + TimeHome;
        state = GhostState.Home;
        nextState = GhostState.Scatter;
        target = scatterTarget;
        isDead = false;
    }

    void Update() {
        switch (state) {
            case GhostState.Inactive: break;
            case GhostState.Home: MoveAtHome(); break;
            case GhostState.Scatter:
                if (Time.time > EndTimeScatter) Chase();
                MoveToTarget();
                break;
            case GhostState.Chase:
                if (Time.time > EndTimeChase) Scatter();
                MoveToTarget();
                break;
            case GhostState.Scared:
                if (EndTimeScared - Time.time < 1.5f) Flash();
                MoveToTarget();
                break;
            case GhostState.Dead: MoveDead(); break;
        }
    }

    void MoveAtHome() {
        if (Time.time >= EndTimeHome) {
            Vector3 p = Vector3.MoveTowards(transform.position, dest, speed * Time.deltaTime);
            GetComponent<Rigidbody>().MovePosition(p);
        }

        if (Vector3.Distance(transform.position, LeaveHome[HomeIndex]) < Util.EPS) {
            HomeIndex++;
            if (HomeIndex < LeaveHome.Count) dest = LeaveHome[HomeIndex];
            else {
                HomeIndex--;
                dest = nextTile.pos;
                RestoreLastState();
            }
            
            if (dest.x > transform.position.x) dir = Direction.right;
            if (dest.x < transform.position.x) dir = Direction.left;
            if (dest.y > transform.position.y) dir = Direction.up;
            if (dest.y < transform.position.y) dir = Direction.down;
            transform.localRotation = Rotation[dir];
        }
    }
    void MoveDead() {
        Vector3 p = Vector3.MoveTowards(transform.position, dest, speed * Time.deltaTime);
        GetComponent<Rigidbody>().MovePosition(p);

        if (Vector3.Distance(transform.position, LeaveHome[HomeIndex]) < Util.EPS) {
            HomeIndex--;
            if (HomeIndex >= 0) dest = LeaveHome[HomeIndex];
            else {
                HomeIndex = 0;
                state = GhostState.Home;
                ghostBody.GetComponent<Renderer>().material = defaultColor;
                ghostBody.SetActive(true);
                isDead = false;
            }
            
            if (dest.x > transform.position.x) dir = Direction.right;
            if (dest.x < transform.position.x) dir = Direction.left;
            if (dest.y > transform.position.y) dir = Direction.up;
            if (dest.y < transform.position.y) dir = Direction.down;
            transform.localRotation = Rotation[dir];
        }
    }


    void MoveToTarget() {
        // handle maze edges
        Vector3 jumpWidth = new Vector3(maze.width, 0, 0);
        if (dir == Direction.right && transform.position.x > dest.x) transform.position -= jumpWidth;
        else if (dir == Direction.left && transform.position.x < dest.x) transform.position += jumpWidth;

        // move toward next tile
        Vector3 p = Vector3.MoveTowards(transform.position, dest, speed * Time.deltaTime);
        GetComponent<Rigidbody>().MovePosition(p);

        // if ghost reaches next tile
        if (Vector3.Distance(transform.position, dest) < Util.EPS) {
            // update target if chasing
            if (nextTile.isIntersection && state == GhostState.Chase) {
                Vector3 pacmanPos = Util.RoundVector(gc.pacman.transform.position);
                Vector3 pacmanDir4 = 4 * gc.pacman.currDirection;
                switch (ghost) {
                    case Ghost.Blinky: target = pacmanPos; break;
                    case Ghost.Pinky: target = pacmanPos + pacmanDir4; break;
                    case Ghost.Inky: target = 2 * pacmanPos + pacmanDir4 - gc.ghosts[Ghost.Blinky].transform.position; break;
                    case Ghost.Clyde:
                        if (Vector3.Distance(transform.position, pacmanPos) < 8) target = scatterTarget;
                        else target = pacmanPos;
                        break;
                }
            }

            // if dead and target reached go inside home
            if (isDead && Vector3.Distance(transform.position, LeaveHome[HomeIndex]) < Util.EPS) {
                state = GhostState.Dead;
                return;
            }

            // update direction
            dir = UpdateDirection(nextTile.isIntersection);

            // update tile and rotate ghost
            switch (dir) {
                case Direction.right: nextTile = nextTile.right; break;
                case Direction.down:  nextTile = nextTile.down;  break;
                case Direction.left:  nextTile = nextTile.left;  break;
                case Direction.up:    nextTile = nextTile.up;    break;
            }
            dest = nextTile.pos;
            transform.localRotation = Rotation[dir];
        }
    }

    Direction UpdateDirection(bool intersection) {
        Direction d = dir;
        if (intersection) {
            if (state == GhostState.Scared && !isDead) return RandomDirection();

            float min = Single.MaxValue;
            float dist;
            if (nextTile.right.isFree && dir != Direction.left) {
                dist = Vector3.Distance(nextTile.right.pos, target);
                if (dist <= min) {
                    d = Direction.right;
                    min = dist;
                }
            }
            if (nextTile.down.isFree && dir != Direction.up) {
                dist = Vector3.Distance(nextTile.down.pos, target);
                if (dist <= min) {
                    d = Direction.down;
                    min = dist;
                }
            }
            if (nextTile.left.isFree && dir != Direction.right) {
                dist = Vector3.Distance(nextTile.left.pos, target);
                if (dist <= min) {
                    d = Direction.left;
                    min = dist;
                }
            }
            if (nextTile.up.isFree && dir != Direction.down) {
                dist = Vector3.Distance(nextTile.up.pos, target);
                if (dist <= min) {
                    d = Direction.up;
                    min = dist;
                }
            }
            return d;
        }
        if (nextTile.right.isFree && dir != Direction.left) return Direction.right;
        if (nextTile.down.isFree && dir != Direction.up) return Direction.down;
        if (nextTile.left.isFree && dir != Direction.right) return Direction.left;
        if (nextTile.up.isFree && dir != Direction.down) return Direction.up;
        return d;
    }

    Direction RandomDirection() {
        List<Direction> possible = new List<Direction>();
        if (nextTile.right.isFree && dir != Direction.left) possible.Add(Direction.right);
        if (nextTile.down.isFree && dir != Direction.up)    possible.Add(Direction.down);
        if (nextTile.left.isFree && dir != Direction.right) possible.Add(Direction.left);
        if (nextTile.up.isFree && dir != Direction.down)    possible.Add(Direction.up);
        System.Random random = new System.Random();
        return possible[random.Next(0, possible.Count)];
    }

    void Flip() {
    }

    void Scatter() {
        Flip();
        state = GhostState.Scatter;
        nextState = GhostState.Chase;
        target = scatterTarget;
        EndTimeScatter = Time.time + TimeScatter;
    }
    
    void Chase() {
        Flip();
        state = GhostState.Chase;
        nextState = GhostState.Scatter;
        EndTimeChase = Time.time + TimeChase;
    }

    public void Scare() {
        switch (state) {
            case GhostState.Scatter: TimeRemaining = EndTimeScatter - Time.time; break;
            case GhostState.Chase: TimeRemaining = EndTimeChase - Time.time; break;
            case GhostState.Home: TimeRemaining = EndTimeHome - Time.time; break;
        }
        Flip();
        ghostBody.GetComponent<Renderer>().material = scaredColor;
        nextState = state;
        state = GhostState.Scared;
        EndTimeScared = Time.time + TimeScared;
        flashCounter = 10;
    }

    void Flash() {
        float dt = EndTimeScared - Time.time;
        if (dt < 0 || flashCounter == 0) {
            ghostBody.GetComponent<Renderer>().material = defaultColor;
            RestoreLastState();
        } else if (dt < flashInterval * flashCounter) {
            if ((flashCounter & 1) > 0) ghostBody.GetComponent<Renderer>().material = scaredColor;
            else ghostBody.GetComponent<Renderer>().material = scaredFlash;
            flashCounter--;
        }
    }

    private bool AssignGhost() {
        switch (gameObject.name) {
            case "Blinky" : ghost = Ghost.Blinky; break;
            case "Pinky" :  ghost = Ghost.Pinky;  break;
            case "Inky" :   ghost = Ghost.Inky;   break;
            case "Clyde" :  ghost = Ghost.Clyde;  break;
            default: return false;
        }
        return true;
    }


    private void InitRotation() {
        Rotation = new Array<Direction, Quaternion>();
        Rotation[Direction.up] = Quaternion.identity;
        Rotation[Direction.right] = Quaternion.Euler(0,  90, 0);
        Rotation[Direction.down]  = Quaternion.Euler(0, 180, 0);
        Rotation[Direction.left]  = Quaternion.Euler(0, -90, 0);
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Player")) {
            if (state == GhostState.Scared) {
                isDead = true;
                target = LeaveHome[HomeIndex];
                ghostBody.SetActive(false);
            } else {
                // kill pacman
            }
        }
    }


    void RestoreLastState() {
        switch (nextState) {
            case GhostState.Chase:
                state = GhostState.Chase;
                nextState = GhostState.Scatter;
                EndTimeChase = Time.time + TimeRemaining;
                break;
            case GhostState.Scatter:
                state = GhostState.Scatter;
                nextState = GhostState.Chase;
                target = scatterTarget;
                EndTimeScatter = Time.time + TimeRemaining;
                break;
            default: Scatter(); break;
        }
    }
}
