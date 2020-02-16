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

    private Vector3 scatterTarget;
    private Vector3 target;
    private Vector3 dest;
    private Direction dir;
    private Tile nextTile;

    public GameObject test;

    public float speed = 10; // to be checked later

    public float TimeToLeaveHome;
    public float TimeToEndScatter;
    public float TimeToEndChase;

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
        scaredColor = Resources.Load<Material>("Materials/ScaredBlue");
        nextTile = maze.GetNearestTile(maze.ghostHouse + new Vector3(3.5f, 0f, 5f));
        
        LeaveHome = new List<Vector3>();
        HomeIndex = 0;
        nextState = GhostState.Scared;

        switch (ghost) {
            case (Ghost.Blinky) :
                TimeToLeaveHome = Time.time + 2;
                dir = Direction.left; nextTile = nextTile.left;
                scatterTarget = new Vector3(maze.width - 2, 0, maze.height + 4);
                Instantiate(test, scatterTarget, Quaternion.identity);
                break;
            case (Ghost.Pinky) :
                LeaveHome.Add(maze.ghostHouse + new Vector3(3.5f, 0f, 2f));
                TimeToLeaveHome = Time.time + 2.5f;
                scatterTarget = new Vector3(2, 0, maze.height + 4);
                break;
            case (Ghost.Inky) :
                LeaveHome.Add(maze.ghostHouse + new Vector3(1.5f, 0f, 2f));
                LeaveHome.Add(maze.ghostHouse + new Vector3(3.5f, 0f, 2f));
                TimeToLeaveHome = Time.time + 6.5f;
                scatterTarget = new Vector3(maze.width, 0, -1);
                break;
            case (Ghost.Clyde) :
                LeaveHome.Add(maze.ghostHouse + new Vector3(5.5f, 0f, 2f));
                LeaveHome.Add(maze.ghostHouse + new Vector3(3.5f, 0f, 2f));
                TimeToLeaveHome = Time.time + 11;
                scatterTarget = new Vector3(0, 0, -1);
                break;
        }
        LeaveHome.Add(maze.ghostHouse + new Vector3(3.5f, 0f, 5f));
        LeaveHome.Add(nextTile.pos);
        

        //transform.position = initialPosition;
        transform.position = LeaveHome[0];

        state = GhostState.Home;
        target = scatterTarget;
        isDead = false;

    }

    void Update() {
        switch (state) {
            case GhostState.Home: MoveAtHome(); break;
            case GhostState.Scatter:
            case GhostState.Chase:
            case GhostState.Scared:
                MoveToTarget();
                break;
            case GhostState.Dead: MoveDead(); break;
        }
    }

    void MoveAtHome() {
        if (Time.time >= TimeToLeaveHome) {
            Vector3 p = Vector3.MoveTowards(transform.position, dest, speed * Time.deltaTime);
            GetComponent<Rigidbody>().MovePosition(p);
        }

        if (Vector3.Distance(transform.position, LeaveHome[HomeIndex]) < Util.EPS) {
            HomeIndex++;
            if (HomeIndex < LeaveHome.Count) dest = LeaveHome[HomeIndex];
            else {
                HomeIndex--;
                dest = nextTile.pos;
                state = nextState;
            }
            
            if (dest.x > transform.position.x) dir = Direction.right;
            if (dest.x < transform.position.x) dir = Direction.left;
            if (dest.y > transform.position.y) dir = Direction.up;
            if (dest.y < transform.position.y) dir = Direction.down;
            print(dir);
            transform.localRotation = Rotation[dir];
        }
    }
    void MoveDead() {
        Vector3 p = Vector3.MoveTowards(transform.position, dest, speed * Time.deltaTime);
        GetComponent<Rigidbody>().MovePosition(p);

        if (Vector3.Distance(transform.position, LeaveHome[HomeIndex]) < Util.EPS) {
            print("Reached node: " + HomeIndex.ToString());
            HomeIndex--;
            if (HomeIndex >= 0) dest = LeaveHome[HomeIndex];
            else {
                HomeIndex = 0;
                state = GhostState.Home;
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
                Vector3 pacmanDir4 = 4 * gc.pacman.GetComponent<PacmanController>().currDirection;
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
                print("Im still here");
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

    void Scatter() {
        state = GhostState.Scatter;
        target = scatterTarget;
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
            isDead = true;
            //state = GhostState.Scatter;
            target = LeaveHome[HomeIndex];
            ghostBody.SetActive(false);
        }
    }
}
