using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public Map[] maps;
    public int mapIndex;

    // 타일과 장애물에 사용할 프리팹
    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform mapFloor;
    public Transform navmeshFloor;
    public Transform navmeshMaskPrefab;

    // 맵의 크기를 설정하는 벡터 (가로 x 세로)
    public Vector2 maxMapSize;

    // 타일 외곽선을 조정하는 비율 (0~1)
    [Range(0, 1)]
    public float outlinePercent;
    public float tileSize;

    // 모든 타일의 좌표를 저장하는 리스트와 셔플된 좌표 큐
    List<Coord> allTileCoords;
    Queue<Coord> shuffledTileCoords;
    Queue<Coord> shuffledOpenTileCoords;
    Transform[,] tileMap;

    Map currentMap;

    // 시작 시 맵을 생성
    void Awake()
    {
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    void OnNewWave(int waveNumber)
    {
        mapIndex = waveNumber - 1;
        GenerateMap();
    }

    // 맵을 생성하는 함수
    public void GenerateMap()
    {
        currentMap = maps[mapIndex];
        tileMap = new Transform[currentMap.mapsize.x, currentMap.mapsize.y];
        System.Random prng = new System.Random(currentMap.seed);

        // 모든 타일의 좌표를 초기화하고 저장
        allTileCoords = new List<Coord>();
        for (int x = 0; x < currentMap.mapsize.x; x++)
        {
            for (int y = 0; y < currentMap.mapsize.y; y++)
            {
                allTileCoords.Add(new Coord(x, y));
            }
        }

        // 셔플된 타일 좌표를 큐에 저장
        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), currentMap.seed));

        // 기존 맵이 존재하면 삭제
        string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        // 새로운 맵의 부모 객체를 생성
        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        // 모든 타일을 생성하고 배치
        for (int x = 0; x < currentMap.mapsize.x; x++)
        {
            for (int y = 0; y < currentMap.mapsize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y);
                // 타일을 생성하고 회전 및 크기 조정
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90));
                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                newTile.parent = mapHolder; // 생성한 타일을 맵의 부모 객체에 연결
                tileMap[x, y] = newTile;
            }
        }

        // 장애물 배치를 위한 맵 배열 초기화
        bool[,] obstacleMap = new bool[(int)currentMap.mapsize.x, (int)currentMap.mapsize.y];

        // 장애물의 총 개수를 계산
        int obstacleCount = (int)(currentMap.mapsize.x * currentMap.mapsize.y * currentMap.obstaclePercent);
        int currentObstacleCount = 0;
        List<Coord> allOpenCoords = new List<Coord> (allTileCoords);

        // 장애물을 생성하고 배치
        for (int i = 0; i < obstacleCount; i++)
        {
            // 랜덤 좌표를 가져옴
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true; // 해당 위치에 장애물 표시
            currentObstacleCount++;

            // 장애물이 맵의 중심이 아니고 맵이 완전히 연결되어 있는지 확인
            if (randomCoord != currentMap.mapCenter && MapIsFullyAccessible(obstacleMap, currentObstacleCount))
            {
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)prng.NextDouble());
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);

                // 장애물을 생성하고 배치
                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * obstacleHeight / 2, Quaternion.identity);
                newObstacle.parent = mapHolder; // 장애물을 맵의 부모 객체에 연결
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
                // 장애물이 맵을 단절시킬 경우 되돌림
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

    // 맵이 완전히 연결되었는지 확인하는 함수 (BFS 알고리즘 사용)
    bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)]; // 방문 여부를 저장하는 배열
        Queue<Coord> queue = new Queue<Coord>();

        // 맵의 중심에서 탐색 시작
        queue.Enqueue(currentMap.mapCenter);
        mapFlags[currentMap.mapCenter.x, currentMap.mapCenter.y] = true;

        int accessibleTileCount = 1; // 접근 가능한 타일 수

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();

            // 상하좌우 이웃 타일을 검사
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;

                    // 대각선 제외 (x 또는 y가 0인 경우만 처리)
                    if (x == 0 || y == 0)
                    {
                        // 유효한 좌표인지 검사
                        if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1))
                        {
                            // 해당 타일을 방문하지 않았고 장애물이 아닌 경우
                            if (!mapFlags[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY])
                            {
                                mapFlags[neighbourX, neighbourY] = true;
                                queue.Enqueue(new Coord(neighbourX, neighbourY)); // 큐에 추가
                                accessibleTileCount++; // 접근 가능한 타일 수 증가
                            }
                        }
                    }
                }
            }
        }

        // 목표 접근 가능한 타일 수와 실제 타일 수가 같은지 비교
        int targetAccessibleTileCount = (int)(currentMap.mapsize.x * currentMap.mapsize.y - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }

    // 좌표를 월드 좌표로 변환하는 함수
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

    // 셔플된 랜덤 좌표를 가져오는 함수
    public Coord GetRandomCoord()
    {
        Coord randomCoord = shuffledTileCoords.Dequeue(); // 큐에서 좌표를 가져옴
        shuffledTileCoords.Enqueue(randomCoord); // 가져온 좌표를 다시 큐에 추가 (재사용 가능)
        return randomCoord;
    }

    public Transform GetRandomOpenTile()
    {
        Coord randomCoord = shuffledOpenTileCoords.Dequeue(); // 큐에서 좌표를 가져옴
        shuffledOpenTileCoords.Enqueue(randomCoord); // 가져온 좌표를 다시 큐에 추가 (재사용 가능)
        return tileMap[randomCoord.x, randomCoord.y];
    }

    // 좌표를 나타내는 구조체 정의
    [System.Serializable]
    public struct Coord
    {
        public int x;
        public int y;

        // 생성자: x, y 좌표를 설정
        public Coord(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        // 좌표 비교를 위한 == 연산자 오버로딩
        public static bool operator ==(Coord c1, Coord c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }

        // 좌표 비교를 위한 != 연산자 오버로딩
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