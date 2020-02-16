using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PacmanEngine;

public class PacmanController : MonoBehaviour {

    public Space relativeTo = Space.Self;

    public Vector3 initialPosition = new Vector3(13.5f, 0f, 7f);
    public float tileSize = 1f;
    public float speed = 10; // tiles per second

    Vector3 destination = Vector3.zero;
    public Vector3 currDirection = Vector3.zero;
    Vector3 nextDirection = Vector3.zero;
    Quaternion nextOrientation = Quaternion.identity;

    Animator animator = null;

    void Start() {
        transform.position = initialPosition;
        destination = transform.position;
        //transform.localRotation = Quaternion.Euler(0,  90, 0); // should be removed
        animator = GetComponent<Animator>();

        if (relativeTo == Space.Self) transform.localRotation = Quaternion.identity;
        else if (relativeTo == Space.World) transform.localRotation = Quaternion.Euler(0, 0, 90);
    }

    void Update() {
        bool isMoving = true;

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


        // consider changing when pacman reaches tile center (his destination)
        if (Vector3.Distance(destination, transform.position) < Util.EPS) {
        //if (destination == transform.position) {
            if (isValid(nextDirection)) {
                //print("next direction is valid -> changing direction to " + nextDirection.ToString() + " at " + transform.position.ToString());
                if (relativeTo == Space.Self) {
                    transform.localRotation *= nextOrientation;
                    nextOrientation = Quaternion.identity;
                } else if (relativeTo == Space.World) transform.localRotation = nextOrientation;

                destination = transform.position + nextDirection;
                currDirection = nextDirection;
            } else {
                //print("next direction is invalid -> direction unchanged: " + currDirection.ToString());
                if (isValid(currDirection)) destination = transform.position + currDirection;
                else {isMoving = false;
                //print("current direction invalid -> stopping");
                }
            }

            //destination = new Vector3((float)Math.Round(destination.x, 0), 0f,(float) Math.Round(destination.z, 0));
            destination = Util.RoundVector(destination);
        }

        Vector3 jumpWidth = new Vector3(28, 0, 0);
        if (transform.position.x >= 27.5 && currDirection.x > 0) {
            transform.position -= jumpWidth;
            destination -= jumpWidth;
        } else if (transform.position.x <= -0.5 && currDirection.x < 0) {
            transform.position += jumpWidth;
            destination += jumpWidth;
        }

        // update animation state
        animator.SetBool("isIdle", !isMoving);
        
        // move to destination
        if (isMoving) {
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
        //bool isHit = Physics.Linecast(pos, pos + direction, out hit, layerWallMask);
        bool isHit = Physics.Raycast(pos, direction, out hit, 1.5f, layerWallMask);
        //if (isHit) {
        //    print("Collision point ====================================" + hit.point.ToString());
        //    print("PACMAN position ====================================" + pos.ToString());
        //    print("WALL position ======================================" + hit.transform.position.ToString());
        //}
        //return (hit.collider == GetComponent<Collider>());
        return !isHit;
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Wall")) {
            print("COLLISION!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            //Debug.Break();
        }
    }
}