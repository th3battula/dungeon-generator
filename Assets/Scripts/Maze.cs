using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Maze {
    [HideInInspector] public float complexity, density;
    [HideInInspector] public int width, height;

    private int[] shape;
    [SerializeField] IntGrid _mazeGrid;

    public Maze (int width, int height, float complexity = 0.75f, float density = 0.75f) {
        this.width = width;
        this.height = height;
        this.shape = new int[] { height, width };
        this.complexity = (int)(complexity * (5.0f * (this.shape[0] + this.shape[1])));
        this.density = (int)(density * ((this.shape[0] / 2) * (this.shape[1] / 2)));

        this._mazeGrid = new IntGrid(this.shape[1], this.shape[0]);
        GenerateMaze();
    }

    public Maze (IntGrid maze, bool shouldGenerate = false, float complexity = 0.75f, float density = 0.75f) {
        int width = maze.GetLength(0);
        int height = maze.GetLength(1);
        this.width = width;
        this.height = height;

        this.shape = new int[] { height, width };
        this.complexity = (int)(complexity * (5.0f * (this.shape[0] + this.shape[1])));
        this.density = (int)(density * ((this.shape[0] / 2) * (this.shape[1] / 2)));

        this._mazeGrid = maze;
        if (shouldGenerate) {
            GenerateMaze();
        }
    }

    // TODO: Refactor this to use a different maze gen algorithm, this one kinda blows
    public void GenerateMaze(IntGrid _maze = null) {
        IntGrid mazeArray = this._mazeGrid;

        // Generate inner maze
        for (int i = 0; i < this.density; i++) {
            int x = Random.Range(0, this.shape[1] / 2) * 2;
            int y = Random.Range(0, this.shape[0] / 2) * 2;
            if (mazeArray[x, y] == 0) {
                mazeArray[x, y] = 1;

                for (int j = 0; j < this.complexity; j++) {
                    List<Vector3> neighbours = new List<Vector3>();
                    if (x > 1) { neighbours.Add(new Vector3(x - 2, y)); }
                    if (x < this.shape[1] - 2) { neighbours.Add(new Vector3(x + 2, y)); }
                    if (y > 1) { neighbours.Add(new Vector3(x, y - 2)); }
                    if (y < this.shape[0] - 2) { neighbours.Add(new Vector3(x, y + 2)); }
                    if (neighbours.Count > 0) {
                        int randIndex = Random.Range(0, neighbours.Count);
                        Vector3 neighbour = neighbours[randIndex];
                        int x_ = (int)neighbour.x;
                        int y_ = (int)neighbour.y;
                        if (mazeArray[x_, y_] == 0) {
                            mazeArray[x_, y_] = 1;
                            mazeArray[x_ + (x - x_) / 2, y_ + (y - y_) / 2] = 1;
                            x = x_;
                            y = y_;
                        }
                    }
                }
            }
        }

        // Border the maze
        int mazeWidth = mazeArray.GetLength(0);
        int mazeHeight = mazeArray.GetLength(1);
        for (int i = 0; i < mazeArray.GetLength(0); i++) {
            for (int j = 0; j < mazeArray.GetLength(1); j++) {
                if (i == 0 || j == 0 || i == mazeWidth - 1 || j == mazeHeight - 1) {
                    mazeArray[j, i] = 1;
                }
            }
        }

        this._mazeGrid = mazeArray;
    }

    public IntGrid mazeGrid {
        get {
            return _mazeGrid;
        }
    }

    public string ToString(bool prettyPrint) {
        return JsonUtility.ToJson(this, prettyPrint);
    }
}
