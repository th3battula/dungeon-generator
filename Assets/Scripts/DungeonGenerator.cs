using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour {
    [SerializeField] GameObject roomPrefab;
    [SerializeField] int retryAttempts = 200;
    [SerializeField] int dungeonWidth = 101;
    [SerializeField] int dungeonHeight = 101;
    [SerializeField] float roomMinDimension = 3f;
    [SerializeField] float roomMaxDimension = 15f;
    [SerializeField] int tileSize = 3;
    [SerializeField] float minDistanceBetweenRooms = 1f;
    [SerializeField] float complexity = 0.95f;
    [SerializeField] float density = 0.95f;

    int retryCount = 0;
    int fillAttempts = 10;
    Rect dungeonRect;
    ObjectPool tiles, filledTiles;
    IntGrid dungeon;

    int floodFilledPointCount;

    bool generate = false;
    bool isGenerating = false;

    void Update() {
        if (generate) {
            generate = false;

            Generate();
        }

        if (Input.GetButtonDown("Fire1")) {
            Generate();
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(dungeonWidth, dungeonHeight));
    }

    IEnumerator GenerateDungeon () {
        if (dungeonHeight % 2 == 0) throw new System.FormatException ("dungeonHeight should be an odd number");
        if (dungeonWidth % 2 == 0) throw new System.FormatException ("roomMaxDimension should be an odd number");
        if (roomMinDimension % 2 == 0) throw new System.FormatException ("roomMinDimension should be an odd number");
        if (roomMaxDimension % 2 == 0) throw new System.FormatException ("roomMaxDimension should be an odd number");
        if (tileSize % 2 == 0) throw new System.FormatException ("tileSize should be an odd number");

        isGenerating = true;
        retryCount = 0;
        dungeonRect = Rect.zero;
        dungeonRect.height = dungeonHeight - 2; // minus 2 to give a 1 unit edge padding around the whole dungeon
        dungeonRect.width = dungeonWidth - 2; // minus 2 to give a 1 unit edge padding around the whole dungeon
        dungeonRect.center = transform.position;
        if (tiles != null) tiles.ClearPool();

        Camera.main.orthographicSize = Mathf.Max(dungeonWidth/2, dungeonHeight/2) + 2;
        dungeon = new IntGrid(dungeonWidth, dungeonHeight);
        tiles = new ObjectPool(roomPrefab, dungeonWidth * dungeonHeight, false, false, transform);

        // make retryAttempts attempts to place rooms randomly within the dungeon
        while (retryCount < retryAttempts) {
            Vector2 randomPosition = new Vector2(ClampValueToOddNumber(Random.Range(0, dungeonWidth) - dungeonWidth/2f), ClampValueToOddNumber(Random.Range(0, dungeonHeight) - dungeonHeight/2f));
            Vector2 randomScale = new Vector2(ClampValueToOddNumber(Random.Range(roomMinDimension, roomMaxDimension)), ClampValueToOddNumber(Random.Range(roomMinDimension, roomMaxDimension)));

            Collider2D overlapBox = Physics2D.OverlapBox(randomPosition, randomScale + (Vector2.one * minDistanceBetweenRooms * 2), 0f);
            if (overlapBox == null && IsRoomInBounds(randomPosition, randomScale)) {
                ObjectPool.PooledObject room = tiles.GetObjectFromPool;
                room.SetActive(true);
                room.position = randomPosition;
                room.scale = randomScale;
                yield return new WaitForSeconds(0.1f);
            } else {
                retryCount++;
            }
        }

        // carve out the rooms from the array of cells and disable the rooms in the pool
        foreach (KeyValuePair<string, ObjectPool.PooledObject> entry in tiles.GetObjectPool) {
            if (entry.Value.activeInHierarchy) {
                Vector3 roomPositionOffset = new Vector3(dungeonWidth/2f, dungeonHeight/2f);
                GameObject room = entry.Value.gameObject;
                Vector3 scale = room.transform.localScale;
                for (float x = -scale.x / 2; x <= scale.x / 2; x++) {
                    for (float y = -scale.y / 2; y <= scale.y / 2; y++) {
                        Point point = new Point((int)x, (int)y) + room.transform.position + roomPositionOffset;
                        dungeon[point.x, point.y] = 2;
                    }
                }

                tiles.Disable(entry.Key);
                yield return null;
            }
        }

        // generate the maze in the empty space of the dungeon
        dungeon = new Maze(dungeon, true).mazeGrid;

        yield return StartCoroutine(RenderDungeon());
        yield return StartCoroutine(StartFloodFill());
        yield return StartCoroutine(RenderDungeon());
        yield return StartCoroutine(RemoveDeadEnds());
        yield return StartCoroutine(RenderDungeon());

        isGenerating = false;
    }

    IEnumerator RenderDungeon() {
        tiles.DisableAll();

        for (int x = 0; x < dungeonWidth; x++) {
            for (int y = 0; y < dungeonHeight; y++) {
                if (dungeon[x, y] == 2) dungeon[x, y] = 0;
                if (dungeon[x, y] == 1) {
                    ObjectPool.PooledObject tile = tiles.GetObjectFromPool;
                    tile.SetActive(true);
                    tile.scale = Vector3.one;
                    tile.position = new Vector3(
                        x - (dungeonWidth / 2) + (tile.scale.x / 2),
                        y - (dungeonHeight / 2) + (tile.scale.y / 2)
                    );
                }
            }
            yield return null;
        }
    }

    IEnumerator StartFloodFill() {
        floodFilledPointCount = 0;
        IntGrid floodFilledCells = new IntGrid(dungeonWidth, dungeonHeight);
        ObjectPool filledTiles = new ObjectPool(roomPrefab, 0, true, true, new GameObject("Filled Tiles").transform);
        for (int i = 0; i < dungeonWidth; i++) {
            for (int j = 0; j < dungeonHeight; j++) {
                floodFilledCells[i,j] = dungeon[i,j];
            }
        }

        int x = Random.Range(0, dungeonWidth - 1);
        int y = Random.Range(0, dungeonHeight - 1);
        while (floodFilledCells[x, y] == 1) {
            x = Random.Range(0, dungeonWidth);
            y = Random.Range(0, dungeonHeight);
        }

        floodFilledPointCount++;;
        StartCoroutine(FloodFill(floodFilledCells, filledTiles, new Point(x, y)));

        while (floodFilledPointCount > 0) yield return null;

        int filledCount = 0;
        for (int i = 0; i < dungeonWidth; i++) {
            for (int j = 0; j < dungeonHeight; j++) {
                if (floodFilledCells[i,j] == 2) filledCount++;
            }
        }

        if (filledCount < (dungeonHeight * dungeonWidth * 0.045f)) { // filled count < 10% of 45% of tile count
            if (fillAttempts >= 0) {
                filledTiles.ClearPool();
                Debug.Log("Fill quality below threshold, restarting fill");
                fillAttempts--;
                StartCoroutine(StartFloodFill());
            } else {
                Debug.Log("Refill attemps reached, restarting dungeon generation");
                StartCoroutine(GenerateDungeon());
            }
        } else {
            for (int i = 0; i < dungeonWidth; i++) {
                for (int j = 0; j < dungeonHeight; j++) {
                    if (floodFilledCells[i,j] == 0) floodFilledCells[i,j] = 1;
                }
            }
        }

        filledTiles.ClearPool();
        dungeon = floodFilledCells;
    }

    IEnumerator FloodFill(IntGrid floodFilledCells, ObjectPool filledTiles, Point point) {
        Point up = point + Vector2.up;
        Point down = point + Vector2.down;
        Point left = point + Vector2.left;
        Point right = point + Vector2.right;

        if (point.x < 0 || point.y < 0 || point.x > dungeonWidth - 1 || point.y > dungeonHeight -1) {
            floodFilledPointCount--;
            yield break;
        }
        if (floodFilledCells[point.x, point.y] != 0) {
            floodFilledPointCount--;
            yield break;
        }

        floodFilledCells[point.x, point.y] = 2;

        ObjectPool.PooledObject pooledObj = filledTiles.GetObjectFromPool;
        pooledObj.position = new Vector2(
            point.x - (dungeonWidth / 2) + (pooledObj.scale.x / 2),
            point.y - (dungeonHeight / 2) + (pooledObj.scale.y / 2)
        );

        SpriteRenderer r = pooledObj.gameObject.GetComponent<SpriteRenderer>();
        if (r != null) r.color = Color.red;

        yield return null;
        if (dungeon[up] == 0) {
            floodFilledPointCount++;
            StartCoroutine(FloodFill(floodFilledCells, filledTiles, up));
        }
        if (dungeon[down] == 0) {
            floodFilledPointCount++;
            StartCoroutine(FloodFill(floodFilledCells, filledTiles, down));
        }
        if (dungeon[left] == 0) {
            floodFilledPointCount++;
            StartCoroutine(FloodFill(floodFilledCells, filledTiles, left));
        }
        if (dungeon[right] == 0) {
            floodFilledPointCount++;
            StartCoroutine(FloodFill(floodFilledCells, filledTiles, right));
        }

        floodFilledPointCount--;
    }

    IEnumerator RemoveDeadEnds() {
        int iterations = 100;
        while (iterations > 0) {
            for (int i = 0; i < dungeonWidth; i++) {
                for (int j = 0; j < dungeonHeight; j++) {
                    Point point = new Point(i, j);
                    Point up = point + Vector2.up;
                    Point down = point + Vector2.down;
                    Point left = point + Vector2.left;
                    Point right = point + Vector2.right;

                    if (dungeon[point] == 0) {
                        int count = 0;
                        if (up.y < dungeonHeight && dungeon[up] == 1) count++;
                        if (down.y >= 0 && dungeon[down] == 1) count++;
                        if (left.x >= 0 && dungeon[left] == 1) count++;
                        if (right.x < dungeonWidth && dungeon[right] == 1) count++;

                        if (count >= 3) dungeon[point] = 1;
                    }
                }
            }
            if (iterations % 10 == 0) yield return null;
            iterations--;
        }
    }

    bool IsRoomInBounds(Vector3 roomPosition, Vector3 roomScale) {
        for (float x = -roomScale.x / 2; x <= roomScale.x / 2; x += roomScale.x / 2) {
            for (float y = -roomScale.y / 2; y <= roomScale.y / 2; y += roomScale.y / 2) {
                Vector3 point = roomPosition + new Vector3(x, y);
                if (!dungeonRect.Contains(point)) {
                    return false;
                }
            }
        }
        return true;
    }

    float ClampValueToOddNumber(float value) {
        return (value % 2) < 0.5f ? Mathf.Floor(value) : Mathf.Ceil(value + 0.001f);
    }

    public void Generate() {
        if (!isGenerating) {
            StartCoroutine(GenerateDungeon());
        }
    }
}
