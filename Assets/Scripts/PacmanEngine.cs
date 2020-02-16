using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PacmanEngine {

    public enum Ghost {Blinky, Pinky, Inky, Clyde}
    public enum GhostState {Inactive, Home, Scatter, Chase, Scared, Dead}

    public enum Direction {right, down, left, up} // increasing priority order

    public class Tile {
        public Vector3 pos;
        public bool isFree;
        public bool isIntersection;
        public byte nbAdj;

        public Tile up, down, left, right;

        public Tile(int x, int y) {
            pos = new Vector3(x, 0, y);
            isFree = isIntersection = false;
            up = down = left = right = null;
            nbAdj = 0;
        }
    }
    
    public class Util {
        public static float EPS = 0.001f;

        public static Vector3 RoundVector(Vector3 vec) {
            return new Vector3((float) Math.Round(vec.x, 0), 0,(float) Math.Round(vec.z, 0));
        }
    }
    

    public class Array<E,T> : IEnumerable where E : Enum {
        private readonly T[] data;
        public int Length { get; }

        public Array() {
            Length = Enum.GetValues(typeof(E)).Length;
            data = new T[Length];
        }

        public T this[E key] {
            get { return data[Convert.ToInt32(key)]; }
            set { data[Convert.ToInt32(key)] = value; }
        }

        public IEnumerator GetEnumerator() {
            return data.GetEnumerator();
        }
    }
}