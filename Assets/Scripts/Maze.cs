using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using PacmanEngine;

public class Maze : MonoBehaviour {

    public Vector3 ghostHouse;
    public byte width, height;
    public byte pacdots = 0;

    public Tile[,] tileMap;
    private char[,] charMap;
    
    private GameController gc;

    public GameObject SimpleWall, Corner, Inner,
        DoubleWall, DoubleCorner, InnerCorner, InnerCornerPointy,
        Piece1, Piece2, Door,
        PacDot, PowerPellet;
    
    public TextAsset mazeFile;

    private Transform walls, collectables;

    void Awake() {
        gc = GameObject.Find("Game Engine").GetComponent<GameController>();
        
        walls = transform.Find("Walls");
        collectables = transform.Find("Collectables");

        ReadMazeFile();
        parseMaze();

    }

    public Tile GetNearestTile(Vector3 position) {
        int i = height - 1 - (int) Math.Round(position.z, MidpointRounding.AwayFromZero);
        int j = (int) Math.Round(position.x, MidpointRounding.AwayFromZero);

        if (i < 0) i = 0;
        if (j < 0) j = 0;
        if (i >= height) i = height - 1;
        if (j >= width) j = width - 1;

        return tileMap[i, j];
    }

    private void parseMaze() {
        tileMap = new Tile[height, width];
        Tile tile = null;

        int z = height;
        int i0 = 0, j0 = 0;
        for (int i = 0; i < height; i++) {
            z--;
            for (int j = 0; j < width; j++) {
                tileMap[i, j] = new Tile(j, z);
                tile = tileMap[i, j];
                tile.isFree = parseCode(charMap[i, j], new Vector3(j, 0, z));

                if (j > 0) {
                    tile.left = tileMap[i, j0];
                    tileMap[i, j0].right = tile;

                    if (tile.isFree && tile.left.isFree) {
                        tile.nbAdj++;
                        tile.left.nbAdj++;
                        if (tile.left.nbAdj > 2) tile.left.isIntersection = true;
                    }
                }

                if (i > 0) {
                    tile.up = tileMap[i0, j];
                    tileMap[i0, j].down = tile;
                    
                    if (tile.isFree && tile.up.isFree) {
                        tile.nbAdj++;
                        tile.up.nbAdj++;
                        if (tile.up.nbAdj > 2) tile.up.isIntersection = true;
                    }
                }

                if (j == width - 1) {
                    tileMap[i, 0].left = tile;
                    tile.right = tileMap[i, 0];
                    
                    if (tile.isFree && tile.right.isFree) {
                        tile.nbAdj++;
                        tile.right.nbAdj++;
                        if (tile.right.nbAdj > 2) tile.right.isIntersection = true;
                        if (tile.nbAdj > 2) tile.isIntersection = true;
                    }
                }
                
                if (i == height - 1) {
                    tileMap[i, 0].up = tile;
                    tile.down = tileMap[0, j];
                    
                    if (tile.isFree && tile.down.isFree) {
                        tile.nbAdj++;
                        tile.down.nbAdj++;
                        if (tile.down.nbAdj > 2) tile.down.isIntersection = true;
                        if (tile.nbAdj > 2) tile.isIntersection = true;
                    }
                }

                j0 = j;
            }
            i0 = i;
        }
        
    }

    private bool parseCode(char code, Vector3 position) {
        Quaternion rot0 = Quaternion.identity;
        Quaternion rot1 = Quaternion.Euler(0,  90, 0);
        Quaternion rot2 = Quaternion.Euler(0, 180, 0);
        Quaternion rot3 = Quaternion.Euler(0, -90, 0);

        switch (code) {
            case 'r': Instantiate(SimpleWall, position, rot0, walls); break;
            case 'b': Instantiate(SimpleWall, position, rot1, walls); break;
            case 'l': Instantiate(SimpleWall, position, rot2, walls); break;
            case 't': Instantiate(SimpleWall, position, rot3, walls); break;
            
            case '1': Instantiate(Corner, position, rot0, walls); break;
            case '2': Instantiate(Corner, position, rot1, walls); break;
            case '3': Instantiate(Corner, position, rot2, walls); break;
            case '4': Instantiate(Corner, position, rot3, walls); break;
            
            case '5': Instantiate(Inner, position, rot0, walls); break;
            case '6': Instantiate(Inner, position, rot1, walls); break;
            case '7': Instantiate(Inner, position, rot2, walls); break;
            case '8': Instantiate(Inner, position, rot3, walls); break;
            
            case 'w': Instantiate(DoubleWall, position, rot0, walls); break;
            case 'n': Instantiate(DoubleWall, position, rot1, walls); break;
            case 'e': Instantiate(DoubleWall, position, rot2, walls); break;
            case 's': Instantiate(DoubleWall, position, rot3, walls); break;
            
            case 'a': Instantiate(DoubleCorner, position, rot0, walls); break;
            case 'c': Instantiate(DoubleCorner, position, rot1, walls); break;
            case 'd': Instantiate(DoubleCorner, position, rot2, walls); break;
            case 'f': Instantiate(DoubleCorner, position, rot3, walls); break;
            
            case 'u': Instantiate(InnerCorner, position, rot0, walls); break;
            case 'v': Instantiate(InnerCorner, position, rot1, walls); break;
            case 'y': Instantiate(InnerCorner, position, rot2, walls); break;
            case 'z': Instantiate(InnerCorner, position, rot3, walls); break;
            
            case 'i': Instantiate(InnerCornerPointy, position, rot0, walls); break;
            case 'j': Instantiate(InnerCornerPointy, position, rot1, walls); break;
            case 'k': Instantiate(InnerCornerPointy, position, rot2, walls); break;
            case 'm': Instantiate(InnerCornerPointy, position, rot3, walls); ghostHouse = position; break;
            
            case 'h': Instantiate(Piece1, position, rot0, walls); break;
            case 'o': Instantiate(Piece1, position, rot1, walls); break;
            case 'q': Instantiate(Piece1, position, rot2, walls); break;
            
            case 'g': Instantiate(Piece2, position, rot0, walls); break;
            case 'p': Instantiate(Piece2, position, rot1, walls); break;
            case 'x': Instantiate(Piece2, position, rot2, walls); break;
            
            case '-': Instantiate(Door, position, rot0, walls); break;

            case '.': Instantiate(PacDot, position, rot0, collectables); pacdots++; return true;
            case '0': Instantiate(PowerPellet, position, rot0, collectables); return true;

            case ' ': return true;

            // unrecognized
            case '?': break;
            default: return false;
        }

        return false;
    }

    void ReadMazeFile() {
        string path = "Assets/Scripts/maze.txt";
        string[] lines = System.IO.File.ReadAllLines(path);

        height = (byte) lines.Length;
        width = (byte) lines[0].Length;
        charMap = new char[height, width];
        int i = 0;
        foreach (string line in lines) {
            for (int j = 0; j < line.Length; j++) {
                charMap[i, j] = line[j];
            }
            i++;
        }
    }

}

