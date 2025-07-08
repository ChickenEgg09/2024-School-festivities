using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public Map[] maps;
    public int mapIndex;

    // Ÿ�ϰ� ��ֹ��� ����� ������
    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform mapFloor;
    public Transform navmeshFloor;
    public Transform navmeshMaskPrefab;

    // ���� ũ�⸦ �����ϴ� ���� (���� x ����)
    public Vector2 maxMapSize;

    // Ÿ�� �ܰ����� �����ϴ� ���� (0~1)
    [Range(0, 1)]
    public float outlinePercent;
    public float tileSize;

    // ��� Ÿ���� ��ǥ�� �����ϴ� ����Ʈ�� ���õ� ��ǥ ť
    List<Coord> allTileCoords;
    Queue<Coord> shuffledTileCoords;
    Queue<Coord> shuffledOpenTileCoords;
    Transform[,] tileMap;

    Map currentMap;

    // ���� �� ���� ����
    void Awake()
    {
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    void OnNewWave(int waveNumber)
    {
        mapIndex = waveNumber - 1;
        GenerateMap();
    }

    // ���� �����ϴ� �Լ�
    public void GenerateMap()
    {
        currentMap = maps[mapIndex];
        tileMap = new Transform[currentMap.mapsize.x, currentMap.mapsize.y];
        System.Random prng = new System.Random(currentMap.seed);

        // ��� Ÿ���� ��ǥ�� �ʱ�ȭ�ϰ� ����
        allTileCoords = new List<Coord>();
        for (int x = 0; x < currentMap.mapsize.x; x++)
        {
            for (int y = 0; y < currentMap.mapsize.y; y++)
            {
                allTileCoords.Add(new Coord(x, y));
            }
        }

        // ���õ� Ÿ�� ��ǥ�� ť�� ����
        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), currentMap.seed));

        // ���� ���� �����ϸ� ����
        string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        // ���ο� ���� �θ� ��ü�� ����
        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        // ��� Ÿ���� �����ϰ� ��ġ
        for (int x = 0; x < currentMap.mapsize.x; x++)
        {
            for (int y = 0; y < currentMap.mapsize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y);
                // Ÿ���� �����ϰ� ȸ�� �� ũ�� ����
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90));
                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                newTile.parent = mapHolder; // ������ Ÿ���� ���� �θ� ��ü�� ����
                tileMap[x, y] = newTile;
            }
        }

        // ��ֹ� ��ġ�� ���� �� �迭 �ʱ�ȭ
        bool[,] obstacleMap = new bool[(int)currentMap.mapsize.x, (int)currentMap.mapsize.y];

        // ��ֹ��� �� ������ ���
        int obstacleCount = (int)(currentMap.mapsize.x * currentMap.mapsize.y * currentMap.obstaclePercent);
        int currentObstacleCount = 0;
        List<Coord> allOpenCoords = new List<Coord> (allTileCoords);

        // ��ֹ��� �����ϰ� ��ġ
        for (int i = 0; i < obstacleCount; i++)
        {
            // ���� ��ǥ�� ������
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true; // �ش� ��ġ�� ��ֹ� ǥ��
            currentObstacleCount++;

            // ��ֹ��� ���� �߽��� �ƴϰ� ���� ������ ����Ǿ� �ִ��� Ȯ��
            if (randomCoord != currentMap.mapCenter && MapIsFullyAccessible(obstacleMap, currentObstacleCount))
            {
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)prng.NextDouble());
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);

                // ��ֹ��� �����ϰ� ��ġ
                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * obstacleHeight / 2, Quaternion.identity);
                newObstacle.parent = mapHolder; // ��ֹ��� ���� �θ� ��ü�� ����
                newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize, obstacleHeight, (1 - outlinePercent) * tileSize);

                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                float colourPercent = randomCoord.y / (float)currentMap.mapsize.y;
                obstacleMaterial.color = Color.Lerp(currentMap.foregroundColour, currentMap.backgroundColour, colourPercent);
                obstacleRenderer.sharedMaterial = obstacleMaterial;

                allOpenCoords.Remove(randomCoord);
            }
            else
            {
                // ��ֹ��� ���� ������ų ��� �ǵ���
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }

        shuffledOpenTileCoords = new Queue<Coord>(Utility.ShuffleArray(allOpenCoords.ToArray(), currentMap.seed));

        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * (currentMap.mapsize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity);
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapsize.x) / 2f, 1, currentMap.mapsize.y) * tileSize;

        Transform maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * (currentMap.mapsize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity);
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxMapSize.x - currentMap.mapsize.x) / 2f, 1, currentMap.mapsize.y) * tileSize;

        Transform maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * (currentMap.mapsize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity);
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapsize.y) / 2f) * tileSize;

        Transform maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * (currentMap.mapsize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity);
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapsize.y) / 2f) * tileSize;

        navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;
        mapFloor.localScale = new Vector3(currentMap.mapsize.x * tileSize, currentMap.mapsize.y * tileSize, 1f);
    }

    // ���� ������ ����Ǿ����� Ȯ���ϴ� �Լ� (BFS �˰��� ���)
    bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)]; // �湮 ���θ� �����ϴ� �迭
        Queue<Coord> queue = new Queue<Coord>();

        // ���� �߽ɿ��� Ž�� ����
        queue.Enqueue(currentMap.mapCenter);
        mapFlags[currentMap.mapCenter.x, currentMap.mapCenter.y] = true;

        int accessibleTileCount = 1; // ���� ������ Ÿ�� ��

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();

            // �����¿� �̿� Ÿ���� �˻�
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;

                    // �밢�� ���� (x �Ǵ� y�� 0�� ��츸 ó��)
                    if (x == 0 || y == 0)
                    {
                        // ��ȿ�� ��ǥ���� �˻�
                        if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1))
                        {
                            // �ش� Ÿ���� �湮���� �ʾҰ� ��ֹ��� �ƴ� ���
                            if (!mapFlags[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY])
                            {
                                mapFlags[neighbourX, neighbourY] = true;
                                queue.Enqueue(new Coord(neighbourX, neighbourY)); // ť�� �߰�
                                accessibleTileCount++; // ���� ������ Ÿ�� �� ����
                            }
                        }
                    }
                }
            }
        }

        // ��ǥ ���� ������ Ÿ�� ���� ���� Ÿ�� ���� ������ ��
        int targetAccessibleTileCount = (int)(currentMap.mapsize.x * currentMap.mapsize.y - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }

    // ��ǥ�� ���� ��ǥ�� ��ȯ�ϴ� �Լ�
    Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-currentMap.mapsize.x / 2f + 0.5f + x, 0, -currentMap.mapsize.y / 2f + 0.5f + y) * tileSize;
    }

    public Transform GetTileFormPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapsize.x - 1) / 2f);
        int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapsize.y - 1) / 2f);
        x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);
        return tileMap[x, y];
    }

    // ���õ� ���� ��ǥ�� �������� �Լ�
    public Coord GetRandomCoord()
    {
        Coord randomCoord = shuffledTileCoords.Dequeue(); // ť���� ��ǥ�� ������
        shuffledTileCoords.Enqueue(randomCoord); // ������ ��ǥ�� �ٽ� ť�� �߰� (���� ����)
        return randomCoord;
    }

    public Transform GetRandomOpenTile()
    {
        Coord randomCoord = shuffledOpenTileCoords.Dequeue(); // ť���� ��ǥ�� ������
        shuffledOpenTileCoords.Enqueue(randomCoord); // ������ ��ǥ�� �ٽ� ť�� �߰� (���� ����)
        return tileMap[randomCoord.x, randomCoord.y];
    }

    // ��ǥ�� ��Ÿ���� ����ü ����
    [System.Serializable]
    public struct Coord
    {
        public int x;
        public int y;

        // ������: x, y ��ǥ�� ����
        public Coord(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        // ��ǥ �񱳸� ���� == ������ �����ε�
        public static bool operator ==(Coord c1, Coord c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }

        // ��ǥ �񱳸� ���� != ������ �����ε�
        public static bool operator !=(Coord c1, Coord c2)
        {
            return !(c1 == c2);
        }
    }

    [System.Serializable]
    public class Map
    {
        public Coord mapsize;
        [Range(0, 1)]
        public float obstaclePercent;
        public int seed;
        public float minObstacleHeight;
        public float maxObstacleHeight;
        public Color foregroundColour;
        public Color backgroundColour;

        public Coord mapCenter
        {
            get
            {
                return new Coord(mapsize.x / 2, mapsize.y / 2);
            }
        }
    }
}