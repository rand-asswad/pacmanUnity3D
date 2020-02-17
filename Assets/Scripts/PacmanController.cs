using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PacmanEngine;

public class PacmanController : MonoBehaviour {

    private GameController gc;

    public Space relativeTo = Space.Self;
    public bool active;
    private float TimeToActivate = Single.MaxValue;

    private bool energized;
    private float EndEnergyTime;

    public Vector3 initialPosition = new Vector3(13.5f, 0f, 7f);
    public float tileSize = 1f;

    private float speed;

    private Vector3 destination = Vector3.zero;
    public Vector3 currDirection = Vector3.zero;
    private Vector3 nextDirection = Vector3.zero;
    private Quaternion nextOrientation = Quaternion.identity;

    Animator animator = null;

    void Start() {
        gc = GameObject.Find("Game Engine").GetComponent<GameController>();
        animator = GetComponent<Animator>();

        if (relativeTo == Space.Self) transform.localRotation = Quaternion.identity;
        else if (relativeTo == Space.World) transform.localRotation = Quaternion.Euler(0, 0, 90);

        Reset();
    }

    public void Reset() {
        active = false;
        TimeToActivate = Time.time + gc.StartTime;
        transform.position = initialPosition;
        destination = transform.position;
        currDirection = Vector3.zero;
        nextDirection = Vector3.zero;
        nextOrientation = Quaternion.identity;
        speed = gc.pacmanSpeed[0] * gc.fullSpeed;
        energized = false;
    }

    void GetInput() {
        if (relativeTo == Space.Self) {
            // get next move
            if (Input.GetKeyDown(KeyCode.UpArrow))         nextOrientation = Quaternion.identity;
            else if (Input.GetKeyDown(KeyCode.RightArrow)) nextOrientation = Quaternion.Euler(0,  90, 0);
            else if (Input.GetKeyDown(KeyCode.DownArrow))  nextOrientation = Quaternion.Euler(0, 180, 0);
            else if (Input.GetKeyDown(KeyCode.LeftArrow))  nextOrientation = Quaternion.Euler(0, -90, 0);

            // calculate global directions
            currDirection = transform.TransformDirection(Vector3.forward);
            nextDirection = nextOrientation * currDirection;
        } else if (relativeTo == Space.World) {
            // get next move
            if (Input.GetKeyDown(KeyCode.UpArrow))         {nextDirection = Vector3.forward;nextOrientation = Quaternion.Euler(0, 0, 90);}
            else if (Input.GetKeyDown(KeyCode.RightArrow)) {nextDirection = Vector3.right;  nextOrientation = Quaternion.Euler(0, 90, 90);}
            else if (Input.GetKeyDown(KeyCode.DownArrow))  {nextDirection = Vector3.back;   nextOrientation = Quaternion.Euler(0, 180, 90);}
            else if (Input.GetKeyDown(KeyCode.LeftArrow))  {nextDirection = Vector3.left;   nextOrientation = Quaternion.Euler(0, -90, 90);}
        }
    }

    void Update() {
        if (energized && Time.time > EndEnergyTime) {
            energized = false;
            speed = gc.pacmanSpeed[0] * gc.fullSpeed;
            gc.eatGhostPoints = 100;
        }

        GetInput();

        if (!active) {
            if (Time.time < TimeToActivate) return;
            active = true;
        }

        // consider changing when pacman reaches tile center (his destination)
        if (Vector3.Distance(destination, transform.position) < Util.EPS) {
        //if (destination == transform.position) {
            if (isValid(nextDirection)) {
                if (relativeTo == Space.Self) {
                    transform.localRotation *= nextOrientation;
                    nextOrientation = Quaternion.identity;
                } else if (relativeTo == Space.World) transform.localRotation = nextOrientation;

                destination = transform.position + nextDirection;
                currDirection = nextDirection;
            } else {
                if (isValid(currDirection)) destination = transform.position + currDirection;
                else {
                    active = false;
                    currDirection = Vector3.zero;
                }
            }

            destination = Util.RoundVector(destination);
        }
        
        // update animation state
        animator.SetBool("isIdle", !active);
        

        Vector3 jumpWidth = new Vector3(28, 0, 0);
        if (transform.position.x >= 27.5 && currDirection.x > 0) {
            transform.position -= jumpWidth;
            destination -= jumpWidth;
        } else if (transform.position.x <= -0.5 && currDirection.x < 0) {
            transform.position += jumpWidth;
            destination += jumpWidth;
        }

        // move to destination
        if (active) {
            Vector3 p = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
            GetComponent<Rigidbody>().MovePosition(p);
        }
    }

    bool isValid(Vector3 direction) {
        // check if pacman collides with himself along line
        Vector3 pos = transform.position;
        direction *= 1.5f;

        RaycastHit hit;
        int layerWallMask = 1 << 8;
        bool isHit = Physics.Raycast(pos, direction, out hit, 1.5f, layerWallMask);
        return !isHit;
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Wall")) {
            print("COLLISION!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            //Debug.Break();
        }
        if (other.gameObject.CompareTag("Power-Pellet")) {
            gc.ScareGhosts();
            gc.powerpellets--;
            gc.score += 50;
            other.gameObject.SetActive(false);
            energized = true;
            speed = gc.pacmanSpeed[1] * gc.fullSpeed;
            EndEnergyTime = Time.time + gc.TimeScared;
            //TimeToActivate = Time.time + 3/gc.fps; // pause for 3 frames
        } else if (other.gameObject.CompareTag("Pac-Dot")) {
            gc.pacdots--;
            gc.score += 10;
            other.gameObject.SetActive(false);
            //TimeToActivate = Time.time + 1/gc.fps; // pause for 1 frame
        }
    }
}