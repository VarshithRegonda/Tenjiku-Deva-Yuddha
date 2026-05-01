using System.Collections.Generic;
using UnityEngine;

namespace TenjikuDevaYuddha.Core
{
    /// <summary>
    /// Manages the game grid — tile data, placement validation, and coordinate conversion.
    /// Uses a flat 2D grid with isometric projection for rendering.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        private int _width;
        private int _height;
        private GridTileData[,] _tiles;

        // Visual
        private GameObject _gridParent;
        private GameObject[,] _tileVisuals;
        private GameObject _hoverIndicator;

        private void Awake()
        {
            Instance = this;
        }

        // ─────────────────────────────────────────────
        //  Initialization
        // ─────────────────────────────────────────────
        public void Initialize(int width, int height, int seed)
        {
            _width = width;
            _height = height;
            _tiles = new GridTileData[width, height];

            // Generate terrain procedurally
            GenerateTerrain(seed);

            // Create visual grid
            CreateGridVisuals();

            // Create hover indicator
            CreateHoverIndicator();
        }

        private void GenerateTerrain(int seed)
        {
            Random.InitState(seed);

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    float noise = Mathf.PerlinNoise(
                        (x + seed) * 0.08f,
                        (y + seed) * 0.08f
                    );

                    float moisture = Mathf.PerlinNoise(
                        (x + seed + 500) * 0.06f,
                        (y + seed + 500) * 0.06f
                    );

                    TerrainType terrain;
                    if (noise < 0.25f)
                        terrain = TerrainType.Water;
                    else if (noise < 0.35f)
                        terrain = TerrainType.Riverbank;
                    else if (noise > 0.75f)
                        terrain = TerrainType.Mountain;
                    else if (moisture > 0.6f && noise > 0.4f)
                        terrain = TerrainType.Forest;
                    else if (moisture < 0.25f && noise > 0.5f)
                        terrain = TerrainType.Desert;
                    else if (Random.value < 0.02f)
                        terrain = TerrainType.SacredGround;
                    else
                        terrain = TerrainType.Plains;

                    _tiles[x, y] = new GridTileData
                    {
                        Position = new Vector2Int(x, y),
                        Terrain = terrain,
                        IsOccupied = false,
                        OccupantBuildingId = null
                    };
                }
            }

            // Ensure the starting area (center) is plains
            int cx = _width / 2;
            int cy = _height / 2;
            for (int x = cx - 4; x <= cx + 4; x++)
            {
                for (int y = cy - 4; y <= cy + 4; y++)
                {
                    if (x >= 0 && x < _width && y >= 0 && y < _height)
                    {
                        _tiles[x, y].Terrain = TerrainType.Plains;
                        _tiles[x, y].IsOccupied = false;
                    }
                }
            }
        }

        // ─────────────────────────────────────────────
        //  Placement Validation
        // ─────────────────────────────────────────────
        public bool CanPlaceBuilding(Vector2Int pos, Vector2Int size)
        {
            for (int x = pos.x; x < pos.x + size.x; x++)
            {
                for (int y = pos.y; y < pos.y + size.y; y++)
                {
                    if (!IsValidPosition(x, y)) return false;
                    if (_tiles[x, y].IsOccupied) return false;
                    if (_tiles[x, y].Terrain == TerrainType.Water) return false;
                    if (_tiles[x, y].Terrain == TerrainType.Mountain) return false;
                }
            }
            return true;
        }

        public void OccupyTiles(Vector2Int pos, Vector2Int size, string buildingId)
        {
            for (int x = pos.x; x < pos.x + size.x; x++)
            {
                for (int y = pos.y; y < pos.y + size.y; y++)
                {
                    if (IsValidPosition(x, y))
                    {
                        _tiles[x, y].IsOccupied = true;
                        _tiles[x, y].OccupantBuildingId = buildingId;
                    }
                }
            }
        }

        public void FreeTiles(Vector2Int pos, Vector2Int size)
        {
            for (int x = pos.x; x < pos.x + size.x; x++)
            {
                for (int y = pos.y; y < pos.y + size.y; y++)
                {
                    if (IsValidPosition(x, y))
                    {
                        _tiles[x, y].IsOccupied = false;
                        _tiles[x, y].OccupantBuildingId = null;
                    }
                }
            }
        }

        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < _width && y >= 0 && y < _height;
        }

        public GridTileData GetTile(int x, int y)
        {
            if (!IsValidPosition(x, y)) return null;
            return _tiles[x, y];
        }

        public GridTileData GetTile(Vector2Int pos) => GetTile(pos.x, pos.y);

        // ─────────────────────────────────────────────
        //  Coordinate Conversion
        // ─────────────────────────────────────────────
        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            return new Vector3(
                gridPos.x * GameConstants.TILE_SIZE,
                0f,
                gridPos.y * GameConstants.TILE_SIZE
            );
        }

        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPos.x / GameConstants.TILE_SIZE),
                Mathf.FloorToInt(worldPos.z / GameConstants.TILE_SIZE)
            );
        }

        /// <summary>
        /// Raycast from camera to grid and return grid position.
        /// </summary>
        public bool RaycastToGrid(out Vector2Int gridPos)
        {
            gridPos = Vector2Int.zero;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Raycast to a ground plane at y=0
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                gridPos = WorldToGrid(hitPoint);
                return IsValidPosition(gridPos.x, gridPos.y);
            }
            return false;
        }

        // ─────────────────────────────────────────────
        //  Visual Grid
        // ─────────────────────────────────────────────
        private void CreateGridVisuals()
        {
            if (_gridParent != null) Destroy(_gridParent);
            _gridParent = new GameObject("Grid");
            _tileVisuals = new GameObject[_width, _height];

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var tile = _tiles[x, y];
                    GameObject tileObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    tileObj.name = $"Tile_{x}_{y}";
                    tileObj.transform.SetParent(_gridParent.transform);
                    tileObj.transform.position = GridToWorld(new Vector2Int(x, y)) + new Vector3(0.5f * GameConstants.TILE_SIZE, 0.01f, 0.5f * GameConstants.TILE_SIZE);
                    tileObj.transform.rotation = Quaternion.Euler(90, 0, 0);
                    tileObj.transform.localScale = Vector3.one * GameConstants.TILE_SIZE * 0.95f;

                    var renderer = tileObj.GetComponent<Renderer>();
                    renderer.material.color = GetTerrainColor(tile.Terrain);

                    // Remove collider from visual tiles — we'll raycast to ground plane instead
                    var collider = tileObj.GetComponent<Collider>();
                    if (collider != null) Destroy(collider);

                    _tileVisuals[x, y] = tileObj;
                }
            }
        }

        private Color GetTerrainColor(TerrainType terrain)
        {
            return terrain switch
            {
                TerrainType.Plains => new Color(0.45f, 0.65f, 0.30f),       // Green
                TerrainType.Water => new Color(0.20f, 0.40f, 0.70f),        // Blue
                TerrainType.Forest => new Color(0.15f, 0.40f, 0.15f),       // Dark green
                TerrainType.Mountain => new Color(0.50f, 0.45f, 0.40f),     // Gray-brown
                TerrainType.SacredGround => new Color(0.90f, 0.80f, 0.40f), // Golden
                TerrainType.Desert => new Color(0.85f, 0.75f, 0.50f),       // Sandy
                TerrainType.Riverbank => new Color(0.35f, 0.55f, 0.45f),    // Teal
                _ => Color.gray
            };
        }

        private void CreateHoverIndicator()
        {
            _hoverIndicator = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _hoverIndicator.name = "HoverIndicator";
            _hoverIndicator.transform.rotation = Quaternion.Euler(90, 0, 0);
            _hoverIndicator.transform.localScale = Vector3.one * GameConstants.TILE_SIZE;

            var renderer = _hoverIndicator.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = new Color(1f, 1f, 1f, 0.3f);
            renderer.material = mat;

            var collider = _hoverIndicator.GetComponent<Collider>();
            if (collider != null) Destroy(collider);

            _hoverIndicator.SetActive(false);
        }

        private void Update()
        {
            // Update hover indicator
            if (RaycastToGrid(out Vector2Int hoverPos))
            {
                _hoverIndicator.SetActive(true);
                Vector3 worldPos = GridToWorld(hoverPos) + new Vector3(0.5f * GameConstants.TILE_SIZE, 0.02f, 0.5f * GameConstants.TILE_SIZE);
                _hoverIndicator.transform.position = worldPos;

                // Color based on whether we can build here
                var renderer = _hoverIndicator.GetComponent<Renderer>();
                if (BuildingSystem.Instance != null && BuildingSystem.Instance.IsInBuildMode)
                {
                    var def = BuildingDatabase.GetBuilding(BuildingSystem.Instance.SelectedBuildingType);
                    bool canPlace = def != null && CanPlaceBuilding(hoverPos, def.Size);
                    renderer.material.color = canPlace ?
                        new Color(0f, 1f, 0f, 0.35f) :
                        new Color(1f, 0f, 0f, 0.35f);

                    // Resize indicator to building size
                    if (def != null)
                    {
                        _hoverIndicator.transform.localScale = new Vector3(
                            def.Size.x * GameConstants.TILE_SIZE,
                            def.Size.y * GameConstants.TILE_SIZE,
                            1f
                        );
                    }

                    // Click to place
                    if (canPlace && Input.GetMouseButtonDown(0))
                    {
                        BuildingSystem.Instance.PlaceBuilding(hoverPos);
                    }
                }
                else
                {
                    renderer.material.color = new Color(1f, 1f, 1f, 0.2f);
                    _hoverIndicator.transform.localScale = Vector3.one * GameConstants.TILE_SIZE;
                }

                GameEvents.TileHovered(hoverPos);

                // Right-click to select tile / building
                if (Input.GetMouseButtonDown(1))
                {
                    GameEvents.TileSelected(hoverPos);
                    var tile = GetTile(hoverPos);
                    if (tile != null && tile.IsOccupied && !string.IsNullOrEmpty(tile.OccupantBuildingId))
                    {
                        GameEvents.BuildingSelected(tile.OccupantBuildingId);
                    }
                }
            }
            else
            {
                _hoverIndicator.SetActive(false);
            }
        }

        public Vector2Int GetMapCenter()
        {
            return new Vector2Int(_width / 2, _height / 2);
        }
    }

    // ─────────────────────────────────────────────
    //  Grid Tile Data
    // ─────────────────────────────────────────────
    [System.Serializable]
    public class GridTileData
    {
        public Vector2Int Position;
        public TerrainType Terrain;
        public bool IsOccupied;
        public string OccupantBuildingId;
    }
}
