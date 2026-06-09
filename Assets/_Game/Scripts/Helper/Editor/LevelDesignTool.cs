using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Handler.Extensions;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

public class LevelDesignTool : OdinEditorWindow
{
    private IDictionary<(int, int), GameObject> instantiatedCellPrefabs = new Dictionary<(int, int), GameObject>();

    [ListDrawerSettings(ShowIndexLabels = true)]
    public List<PrefabColorPair> PrefabColorMapping;
    [SerializeField]
    private int currentLevelIndex = 0;
    
    [TableMatrix(HorizontalTitle = "Prefab Matrix", SquareCells = false, DrawElementMethod = "DrawSquareCell", RowHeight = 50)]
    public string[,] PrefabMatrix;
    
    private const float MinT = 0.0f;
    private const float MaxT = 1.0f;
    [SerializeField] public float PathWidth = 5;  // Manually entered by the user in the editor
    private IDictionary<(int, int), GameObject> sharedInstantiatedPrefabs = new Dictionary<(int, int), GameObject>();

    [SerializeField] private float minRangeX = -3f;
    [SerializeField] private float maxRangeX = 3f;

    public List<PrefabProbabilityPair> PrefabProbabilities;

    // New variables for Instance-Specific Properties
    private IDictionary<(int, int), Dictionary<string, object>> instantiatedCellProperties = new Dictionary<(int, int), Dictionary<string, object>>();

    [System.Serializable]
    public class PrefabColorPair
    {
        [JsonIgnore]
        public GameObject Prefab;
        [JsonIgnore]
        public string PrefabPath;  // Will be serialized
        public Color Color;
        public Dictionary<string, object> Properties = new Dictionary<string, object>();
        public Vector2 Dimensions = Vector2Int.one;

        public void PrepareForSerialization()
        {
            // Populate PrefabPath from Prefab just before serializing
            PrefabPath = AssetDatabase.GetAssetPath(Prefab);
        }

        public void AfterDeserialization()
        {
            // Reassign Prefab from PrefabPath after deserializing
            if (!string.IsNullOrEmpty(PrefabPath))
            {
                Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            }
        }
    
        public string GetPrefabPath()
        {
            // Save the path relative to the "Assets" folder
            PrefabPath = AssetDatabase.GetAssetPath(Prefab);
            return PrefabPath;
        }
        
        public GameObject GetPrefab()
        {
            if (Prefab == null && !string.IsNullOrEmpty(PrefabPath))
            {
                // Load the prefab from the saved path
                Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            }

            return Prefab;
        }
        
    }
    

    [System.Serializable]
    public class PrefabProbabilityPair
    {
        public GameObject Prefab;
        public float Probability;
    }

    [OnInspectorGUI]
    void CustomInspectorGUI()
    {
        if (GUILayout.Button("Clear All"))
        {
            ClearAllCells();
        }
        if (GUILayout.Button("Resize Grid"))
        {
            ResizeGrid();
        }
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save Level"))
        {
            SaveLevelData();
        }
        if (GUILayout.Button("Load Level"))
        {
            LoadLevelData();
        }
        GUILayout.EndHorizontal();
    }

    [SerializeField] public int NumberOfRows = 3;
    [SerializeField] public int NumberOfColumns = 5;
    [SerializeField] public MeshRenderer TargetMeshRenderer;

    public LevelDesignTool()
    {
        ResizeGrid();
    }

    private void ResizeGrid()
    {
        PrefabMatrix = new string[NumberOfRows, NumberOfColumns];
        for (int i = 0; i < NumberOfRows; i++)
        {
            for (int j = 0; j < NumberOfColumns; j++)
            {
                PrefabMatrix[i, j] = string.Empty;
            }
        }
    }

    private string DrawSquareCell(Rect rect, int rowIndex, int columnIndex)
    {
        string prefabName = PrefabMatrix[rowIndex, columnIndex];
        Color originalColor = GUI.backgroundColor;
        Color newColor = GetColorByName(prefabName);
        newColor.a = 1;
        GUI.backgroundColor = newColor;

        if (GUI.Button(rect, prefabName))
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Reset"), false, () => ResetCell(rowIndex, columnIndex));

            foreach (PrefabColorPair prefabColorPair in PrefabColorMapping)
            {
                menu.AddItem(new GUIContent(prefabColorPair.GetPrefab().name), false, () => SetMultiCellPrefab(rowIndex, columnIndex, prefabColorPair, prefabColorPair.Dimensions));
            }

            menu.ShowAsContext();
        }

        GUI.backgroundColor = originalColor;
        return PrefabMatrix[rowIndex, columnIndex];
    }
    private void UpdatePrefabProperties(int rowIndex, int columnIndex, Dictionary<string, object> updatedProperties)
    {
        if (instantiatedCellProperties.TryGetValue((rowIndex, columnIndex), out var existingProperties))
        {
            foreach (var item in updatedProperties)
            {
                existingProperties[item.Key] = item.Value;
            }
        }
    }
    private void SetCellPrefab(int rowIndex, int columnIndex, PrefabColorPair prefabColorPair, GameObject existingObject)
    {
        UpdateOrCreateCellPrefab(rowIndex, columnIndex, existingObject, prefabColorPair);
        Repaint();
    }

    private void UpdateOrCreateCellPrefab(int rowIndex, int columnIndex, GameObject existingObject, PrefabColorPair prefabColorPair)
    {
        instantiatedCellPrefabs[(rowIndex, columnIndex)] = existingObject;
        PrefabMatrix[rowIndex, columnIndex] = prefabColorPair.GetPrefab().name;
        
        if (sharedInstantiatedPrefabs.TryGetValue((rowIndex, columnIndex), out GameObject previousObject))
        {
            if (previousObject != existingObject)
            {
                sharedInstantiatedPrefabs[(rowIndex, columnIndex)] = existingObject;
            }
        }
        else
        {
            sharedInstantiatedPrefabs.Add((rowIndex, columnIndex), existingObject);
        }
    }

   
    private void SetMultiCellPrefab(int rowIndex, int columnIndex, PrefabColorPair prefabColorPair, Vector2 dimensions)
    {
        Vector3 targetPosition = CalculatePosition(rowIndex, columnIndex);
        Quaternion targetRotation = CalculateRotation(rowIndex, columnIndex);
        
        GameObject instantiatedObject = InstantiatePrefabInScene(prefabColorPair.GetPrefab());

        if (instantiatedObject != null)
        {
            SetTransform(instantiatedObject, targetPosition, targetRotation);
            instantiatedObject.transform.parent=TargetMeshRenderer.transform;
        }

        FillCells(rowIndex, columnIndex, dimensions, prefabColorPair,instantiatedObject);
    }

    private GameObject InstantiatePrefabInScene(GameObject prefab)
    {
        return PrefabUtility.InstantiatePrefab(prefab, UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()) as GameObject;
    }

    private void SetTransform(GameObject obj, Vector3 position, Quaternion rotation)
    {
        position.y -= 0.9f;  // Adjust the position offset here
        obj.transform.position = position;
        obj.transform.rotation = rotation;
    }

    private void FillCells(int rowIndex, int columnIndex, Vector2 dimensions, PrefabColorPair prefabColorPair, GameObject instantiatedObject)
    {
        for (int i = 0; i < dimensions.x; i++)
        {
            for (int j = 0; j < dimensions.y; j++)
            {
                SetCellPrefab(rowIndex + i, columnIndex + j, prefabColorPair, instantiatedObject);
            }
        }
    }

    public Quaternion CalculateRotation(int rowIndex, int columnIndex)
    {
        return TargetMeshRenderer != null ? TargetMeshRenderer.transform.rotation : Quaternion.identity;
    }

   
    float CalculateRowMultiplier(int rowIndex)
    {
        return (float)(rowIndex - (NumberOfRows - 1) / 2.0f);
    }

    public Vector3 CalculatePosition(int rowIndex, int columnIndex)
    {
        if (TargetMeshRenderer == null)
        {
            return Vector3.zero;
        }

        float mappedT = NumberOfColumns <= 1 ? 0f : Mathf.Lerp(MinT, MaxT, (float)columnIndex / (NumberOfColumns - 1));
        Bounds bounds = TargetMeshRenderer.bounds;
        Vector3 forwardOffset = TargetMeshRenderer.transform.forward * Mathf.Lerp(-bounds.extents.z, bounds.extents.z, mappedT);
        Vector3 pointOnTrack = bounds.center + forwardOffset;
        float multiplier = CalculateRowMultiplier(rowIndex);
        float rowDivisor = Mathf.Max(1, NumberOfRows - 1);
        float targetHorizontalOffset = multiplier * (PathWidth / rowDivisor);

        return pointOnTrack + TargetMeshRenderer.transform.right * targetHorizontalOffset;
    }

    private Color GetColorByName(string prefabName)
    {
        var pair = PrefabColorMapping.FirstOrDefault(x => x != null && x.GetPrefab() != null && x.GetPrefab().name == prefabName);
        return pair != null ? pair.Color : Color.white;
    }

    private void ResetCell(int rowIndex, int columnIndex)
    {
        if (instantiatedCellPrefabs.TryGetValue((rowIndex, columnIndex), out GameObject previousPrefab))
        {
            // Destroy the prefab if it exists
            DestroyImmediate(previousPrefab);
            instantiatedCellPrefabs.Remove((rowIndex, columnIndex));

            // Check if this cell shares its prefab with other cells
            if (sharedInstantiatedPrefabs.TryGetValue((rowIndex, columnIndex), out GameObject sharedPrefab))
            {
                // Find and reset all cells that share this prefab
                var keysToRemove = new List<(int, int)>();
                foreach (var pair in sharedInstantiatedPrefabs)
                {
                    if (pair.Value == previousPrefab)
                    {
                        keysToRemove.Add(pair.Key);
                        instantiatedCellPrefabs.Remove(pair.Key);
                        PrefabMatrix[pair.Key.Item1, pair.Key.Item2] = string.Empty;
                    }
                }

                // Remove these keys from the sharedInstantiatedPrefabs dictionary
                foreach (var key in keysToRemove)
                {
                    sharedInstantiatedPrefabs.Remove(key);
                }
            }
        }

        PrefabMatrix[rowIndex, columnIndex] = string.Empty;
    }


    private void ClearAllCells()
    {
        foreach (var key in instantiatedCellPrefabs.Keys.ToList())
        {
            DestroyImmediate(instantiatedCellPrefabs[key].gameObject);
        }
        instantiatedCellPrefabs.Clear();

        for (int i = 0; i < NumberOfRows; i++)
        {
            for (int j = 0; j < NumberOfColumns; j++)
            {
                PrefabMatrix[i, j] = string.Empty;
            }
        }

        Repaint();
    }

    [MenuItem("Tools/Level Design Tool")]
    private static void OpenWindow()
    {
        GetWindow<LevelDesignTool>().Show();
    }
    
    private void SaveLevelData()
    {
        LevelData levelData = CreateLevelData();

        string filePath = Application.dataPath + $"/LevelData_{currentLevelIndex}.json";

        try
        {
            levelData.prefabColorPair.ForEach(x => x.PrepareForSerialization());
            ES3.Save($"/LevelData_{currentLevelIndex}.json", levelData, filePath);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save LevelData to {filePath}. Error: {e.Message}");
        }
    }

    private LevelData CreateLevelData()
    {
        List<string> flattenedMatrix = Flatten2DArray(PrefabMatrix);
        return new LevelData
        {
            NumberOfRows = this.NumberOfRows,
            NumberOfColumns = this.NumberOfColumns,
            PrefabMatrix = flattenedMatrix,
            prefabColorPair = this.PrefabColorMapping
            // ... (any other data you'd like to save)
        };
    }

    private List<string> Flatten2DArray(string[,] array2D)
    {
        List<string> list = new List<string>();
        for (int i = 0; i < array2D.GetLength(0); i++)
        {
            for (int j = 0; j < array2D.GetLength(1); j++)
            {
                list.Add(array2D[i, j]);
            }
        }
        return list;
    }
    
    
    
    private void LoadLevelData()
    {
        string filePath = Application.dataPath + $"/LevelData_{currentLevelIndex}.json";

        if (ES3.FileExists(filePath))
        {
            try
            {
                LevelData levelData = ES3.Load<LevelData>(filePath);
                ApplyLevelData(levelData);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load LevelData from {filePath}. Error: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"No saved LevelData found for {filePath}");
        }
    }

    private void ApplyLevelData(LevelData levelData)
    {
        
        levelData.prefabColorPair.ForEach(x => x.AfterDeserialization());

        NumberOfRows = levelData.NumberOfRows;
        NumberOfColumns = levelData.NumberOfColumns;
        PrefabMatrix = UnflattenListTo2DArray(levelData.PrefabMatrix, NumberOfRows, NumberOfColumns);
        PrefabColorMapping = levelData.prefabColorPair;

    }
    private string[,] UnflattenListTo2DArray(List<string> flatList, int rows, int cols)
    {
        string[,] array2D = new string[rows, cols];
        int index = 0;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                array2D[i, j] = flatList[index];
                index++;
            }
        }
        
        return array2D;
    }

    
    private void RedrawGrid()   // This method is called when the user clicks the "Redraw Grid" button
    {
        ClearAllCells();
        
        for (int i = 0; i < NumberOfRows; i++)
        {
            for (int j = 0; j < NumberOfColumns; j++)
            {
                string prefabName = PrefabMatrix[i, j];
                if (string.IsNullOrEmpty(prefabName)) continue;
        
                PrefabColorPair prefabColorPair = PrefabColorMapping.FirstOrDefault(x => x != null && x.GetPrefab() != null && x.GetPrefab().name == prefabName);
                
                SetMultiCellPrefab(i, j, prefabColorPair, prefabColorPair.Dimensions);
            }
        }
    }   
    
    
    [Serializable]
    public class LevelData
    {
        public int NumberOfRows;
        public int NumberOfColumns;
        public List<string> PrefabMatrix;
        public List<PrefabColorPair> prefabColorPair;
    }
    
}
