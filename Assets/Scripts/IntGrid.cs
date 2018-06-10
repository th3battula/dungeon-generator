using UnityEngine;

[System.Serializable]
public class IntGrid {
    [System.Serializable]
    public class IntRow {
        public int[] cols;
        public IntRow (int length, int defaultValue) {
            this.cols = new int[length];
            for (int i = 0; i < this.cols.Length; i++) {
                this.cols[i] = defaultValue;
            }
        }
    }
    public IntRow[] rows;

    public IntGrid (int width, int height) {
        InstantiateIntGrid(width, height);
    }

    public IntGrid (int width, int height, int defaultValue) {
        InstantiateIntGrid(width, height, defaultValue);
    }

    private void InstantiateIntGrid(int width, int height, int defaultValue = 0) {
        this.rows = new IntRow[height];
        for (int i = 0; i < height; i++) {
            this.rows[i] = new IntRow(width, defaultValue);
        }
    }

    public int this[int x, int y] {
        get {
            IntRow row = this.rows[y];
            int col = row.cols[x];
            return col;
        }
        set {
            IntRow row = this.rows[y];
            int[] cols = row.cols;
            cols[x] = value;
        }
    }

    public int this[Point p] {
        get {
            IntRow row = this.rows[p.y];
            int col = row.cols[p.x];
            return col;
        }
        set {
            IntRow row = this.rows[p.y];
            int[] cols = row.cols;
            cols[p.x] = value;
        }
    }

    public int GetLength(int index) {
        switch (index) {
            case 0:
                return rows.Length;
            case 1:
                return rows[0].cols.Length;
            default:
                return 0;
        }
    }

    public string ToJsonString(bool prettyPrint = false) {
        return JsonUtility.ToJson(this, prettyPrint);
    }

    public string ToString(bool prettyPrint = false) {
        int width = GetLength(0);
        int height = GetLength(1);
        string arr = "";

        for (int i = 0; i < height; i++) {
            arr += "[";
            for (int j = 0; j < width; j++) {
                arr += this[i, j];

                if (j < width - 1) arr += ",";
            }
            arr += "]";

            if (i < height - 1) arr += ",\n";
        }

        return arr;
    }
}