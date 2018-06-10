using UnityEngine;

[System.Serializable]
public class Point {
    public int x, y;
    public Point (int x, int y) {
        this.x = x;
        this.y = y;
    }

    public static Point operator +(Point p1, Point p2) {
        return new Point(p1.x + p2.x, p1.y + p2.y);
    }

    public static Point operator +(Point p1, Vector2 p2) {
        return new Point(p1.x + (int)p2.x, p1.y + (int)p2.y);
    }

    public static Point operator +(Point p1, Vector3 p2) {
        return new Point(p1.x + (int)p2.x, p1.y + (int)p2.y);
    }

    public static Point operator *(Point p1, Point p2) {
        return new Point(p1.x * p2.x, p1.y * p2.y);
    }

    public static Point operator *(Point p1, Vector2 p2) {
        return new Point(p1.x * (int)p2.x, p1.y * (int)p2.y);
    }

    public static Point operator *(Point p1, Vector3 p2) {
        return new Point(p1.x * (int)p2.x, p1.y * (int)p2.y);
    }

    public override string ToString() {
        return string.Format("{0}, {1}", x, y);
    }
}