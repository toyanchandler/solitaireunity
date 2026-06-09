using System;
using System.Collections.Generic;
using _Game.Scripts.Helper.Editor;
using _Game.Scripts.Project.SolitaireModule.Controllers;
using _Game.Scripts.Project.SolitaireModule.Runtime;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Rules;
using _Game.Scripts.Project.SolitaireModule.Views;
using _Game.Scripts.UI.Buttons;
using _Game.Scripts.UI.Screens;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace _Game.Scripts.Project.SolitaireModule.Editor
{
    public static class SolitaireSceneBuilder
    {
        private const string ScenePath = "Assets/_Game/Scenes/_TemplateScene.unity";
        private const string PrefabFolder = "Assets/_Game/Prefabs/_InGame/Solitaire";
        private const string SlotPrefabFolder = PrefabFolder + "/Slots";
        private const string ConfigFolder = "Assets/_Game/Config/Solitaire";
        private const string ArtFolder = "Assets/_Game/Art/Solitaire";
        private const string CardPrefabPath = PrefabFolder + "/Card.prefab";
        private const string RootPrefabPath = PrefabFolder + "/SolitaireRoot.prefab";
        private const string DeckRippleParticlePrefabPath = PrefabFolder + "/SolitaireDeckRippleParticle.prefab";
        private const string UndoButtonPrefabPath = "Assets/Art/UI/UI Prefabs/General/Button_SolitaireUndo.prefab";
        private const string ConfigPath = ConfigFolder + "/SolitaireDeckConfig.asset";
        private const string VisualCatalogPath = ConfigFolder + "/SolitaireCardVisualCatalog.asset";
        private const string BoardBackdropSpritePath = ArtFolder + "/UI/solitaire_board_felt_background.png";
        private const string DragShadowSpritePath = ArtFolder + "/UI/solitaire_card_drag_shadow_gen.png";
        private static readonly Color BoardFallbackBackgroundColor = new Color(0.04f, 0.09f, 0.07f, 1f);
        private const float CardWidth = 0.74f;
        private const float CardHeight = 1.07f;
        private const float CardAspectRatio = 1.45f;
        private const float CardHorizontalSpacingRatio = 0.10f;
        private const float MaxResponsiveCardWidth = 1.08f;
        private const float BoardHorizontalPadding = 0.28f;
        private const float BoardTopHudPadding = 1.20f;
        private const float BoardBottomPadding = 0.45f;
        private const float RowVerticalGap = 0.34f;
        private const float FaceUpTableauYOffset = 0.22f;
        private const float FaceDownTableauYOffset = 0.10f;
        private const float MinCompressedFaceUpYOffset = 0.12f;
        private const float TableauBottomPlayableY = -3.85f;
        private const float WasteStackXOffset = 0.09f;
        private const float DropSnapDistance = 0.30f;
        private static readonly Color SlotMarkerColor = new Color(0.74f, 0.98f, 0.82f, 0.62f);

        [MenuItem("Tools/Solitaire/Repair Main Scene")]
        public static void BuildMainScene()
        {
            RepairMainScene();
        }

        [MenuItem("Tools/Solitaire/Ensure Deck Ripple Particle")]
        public static void EnsureDeckRippleParticleMenu()
        {
            EnsureDeckRippleParticlePrefab();
            WireDeckRippleParticleIntoStockSlot();
            AssetDatabase.SaveAssets();
            Debug.Log("[SolitaireFx] DeckRippleParticle prefab ensured and wired into StockSlot.");
        }

        [MenuItem("Tools/Solitaire/Repair InGame Undo UI")]
        public static void RepairInGameUndoUi()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            RepairInGameUndoUiInOpenScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[SolitaireUndo] InGame undo UI repaired in _TemplateScene.");
        }

        public static void RepairMainScene()
        {
            EnsureFolders();
            Sprite slotSprite = EnsureSprite("SolitaireSlotGhost", Color.white);

            SolitaireCardVisualCatalogSO visualCatalog = LoadVisualCatalog();
            Sprite frontSprite = visualCatalog.GetFrontSprite(CardSuit.Hearts, CardRank.Ace);
            Sprite backSprite = visualCatalog.DefaultBackSprite;
            GameObject cardPrefab = EnsureCardPrefab(frontSprite);
            Dictionary<string, GameObject> slotPrefabs = EnsureSlotPrefabs(slotSprite);
            SolitaireDeckConfigSO config = EnsureConfig(cardPrefab.GetComponent<CardView>(), visualCatalog, frontSprite, backSprite);

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject existingRoot = GameObject.Find("SolitaireRoot");
            GameObject root = existingRoot != null
                ? RepairExistingSceneRoot(existingRoot, config)
                : BuildSceneRoot(cardPrefab, slotPrefabs, config);

            PrefabUtility.SaveAsPrefabAssetAndConnect(root, RootPrefabPath, InteractionMode.AutomatedAction);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            BakeDebugScenarioCanvasIfNeeded();
            Debug.Log("Solitaire main scene repair completed.");
        }

        private static void BakeDebugScenarioCanvasIfNeeded()
        {
            if (GameObject.Find("SolitaireDebugScenarioCanvas") != null)
                return;

            SolitaireDebugScenarioPanelBuilder.BakeDebugScenarioCanvas(saveScene: false);
        }

        [MenuItem("Tools/Solitaire/Rebuild Main Scene (Destructive)")]
        public static void RebuildMainSceneDestructive()
        {
            EnsureFolders();
            Sprite slotSprite = EnsureSprite("SolitaireSlotGhost", Color.white);

            SolitaireCardVisualCatalogSO visualCatalog = LoadVisualCatalog();
            Sprite frontSprite = visualCatalog.GetFrontSprite(CardSuit.Hearts, CardRank.Ace);
            Sprite backSprite = visualCatalog.DefaultBackSprite;
            GameObject cardPrefab = EnsureCardPrefab(frontSprite);
            Dictionary<string, GameObject> slotPrefabs = EnsureSlotPrefabs(slotSprite);
            SolitaireDeckConfigSO config = EnsureConfig(cardPrefab.GetComponent<CardView>(), visualCatalog, frontSprite, backSprite);

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject existingRoot = GameObject.Find("SolitaireRoot");

            if (existingRoot != null)
                UnityEngine.Object.DestroyImmediate(existingRoot);

            GameObject root = BuildSceneRoot(cardPrefab, slotPrefabs, config);
            PrefabUtility.SaveAsPrefabAssetAndConnect(root, RootPrefabPath, InteractionMode.AutomatedAction);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Solitaire main scene destructive rebuild completed.");
        }

        [MenuItem("Tools/Solitaire/Cleanup Legacy Runner Objects")]
        public static void CleanupLegacyRunnerObjectsMenu()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            CleanupLegacyRunnerObjects();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Solitaire legacy runner cleanup completed.");
        }

        [MenuItem("Tools/Solitaire/Validate Main Scene")]
        public static void ValidateMainSceneMenu()
        {
            ValidateMainScene();
        }

        public static void ValidateMainScene()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SolitaireModuleBootstrap bootstrap = UnityEngine.Object.FindFirstObjectByType<SolitaireModuleBootstrap>();

            if (bootstrap == null)
                throw new InvalidOperationException("SolitaireModuleBootstrap was not found in the main scene.");

            SolitaireDeckConfigSO config = GetRequiredSerializedObject<SolitaireDeckConfigSO>(bootstrap, "deckConfig");

            if (!config.Validate(out string error))
                throw new InvalidOperationException(error);

            CardView[] cards = UnityEngine.Object.FindObjectsByType<CardView>(FindObjectsSortMode.None);
            SolitaireSlotAnchor[] slots = UnityEngine.Object.FindObjectsByType<SolitaireSlotAnchor>(FindObjectsSortMode.None);
            BoxCollider2D[] boxCollider2Ds = UnityEngine.Object.FindObjectsByType<BoxCollider2D>(FindObjectsSortMode.None);
            Transform deckParent = GameObject.Find("SolitaireRoot/DeckParent")?.transform;
            Transform dragParent = GameObject.Find("SolitaireRoot/DragParent")?.transform;
            GameObject controllerHost = GameObject.Find("SolitaireRoot/Controllers/ControllerHost");
            SolitaireBoardCameraController boardCamera = UnityEngine.Object.FindFirstObjectByType<SolitaireBoardCameraController>(FindObjectsInactive.Include);

            if (cards.Length != SolitaireCardUtility.CardCount)
                throw new InvalidOperationException($"Expected 52 card instances, found {cards.Length}.");

            if (deckParent == null)
                throw new InvalidOperationException("DeckParent was not found under SolitaireRoot.");

            if (deckParent.childCount != SolitaireCardUtility.CardCount)
                throw new InvalidOperationException($"DeckParent must contain exactly 52 pre-baked card objects, found {deckParent.childCount}.");

            if (boardCamera == null || boardCamera.GetComponent<Camera>() == null)
                throw new InvalidOperationException("SolitaireBoardCameraController with Camera was not found in the main scene.");

            if (dragParent == null || dragParent.GetComponent<SolitaireDragLayer>() == null)
                throw new InvalidOperationException("DragParent with SolitaireDragLayer was not found under SolitaireRoot.");

            ValidateControllerHost(controllerHost);

            if (slots.Length != 13)
                throw new InvalidOperationException($"Expected 13 slot anchors, found {slots.Length}.");

            if (boxCollider2Ds.Length < 65)
                throw new InvalidOperationException($"Expected at least 65 BoxCollider2D components, found {boxCollider2Ds.Length}.");

            string legacyObject = FindLegacyRunnerObjectName();

            if (!string.IsNullOrEmpty(legacyObject))
                throw new InvalidOperationException($"Legacy runner object still exists in main scene: {legacyObject}.");

            var registry = new SolitaireViewRegistry();
            var seenCards = new bool[SolitaireCardUtility.CardCount];

            foreach (CardView card in cards)
            {
                if (card.transform.parent != deckParent)
                    throw new InvalidOperationException($"{card.name} is not pre-baked directly under DeckParent.");

                CardRuntimeIdentity identity = card.GetComponent<CardRuntimeIdentity>();

                if (identity == null)
                    throw new InvalidOperationException($"{card.name} is missing CardRuntimeIdentity.");

                if (identity.CardId < 0 || identity.CardId >= SolitaireCardUtility.CardCount)
                    throw new InvalidOperationException($"{card.name} has invalid CardId {identity.CardId}.");

                if (seenCards[identity.CardId])
                    throw new InvalidOperationException($"Duplicate Solitaire CardId {identity.CardId:00} in main scene.");

                seenCards[identity.CardId] = true;

                if (identity.Suit != SolitaireCardUtility.GetSuitFromId(identity.CardId) ||
                    identity.Rank != SolitaireCardUtility.GetRankFromId(identity.CardId))
                {
                    throw new InvalidOperationException($"{card.name} has mismatched identity data for CardId {identity.CardId}.");
                }

                if (card.GetComponent<BoxCollider>() != null)
                    throw new InvalidOperationException($"{card.name} still has a 3D BoxCollider.");

                registry.RegisterCard(card);
            }

            var seenSlots = new HashSet<string>();

            foreach (SolitaireSlotAnchor slot in slots)
            {
                if (slot.GetComponent<BoxCollider>() != null)
                    throw new InvalidOperationException($"{slot.name} still has a 3D BoxCollider.");

                string slotKey = $"{slot.PileType}:{slot.PileIndex}";

                if (!seenSlots.Add(slotKey))
                    throw new InvalidOperationException($"Duplicate Solitaire slot registration for {slot.PileType} {slot.PileIndex} in main scene.");

                registry.RegisterSlot(slot);
            }

            if (!registry.Validate(out error))
                throw new InvalidOperationException(error);

            Debug.Log("Solitaire main scene validation completed.");
        }

        public static void BuildAndValidateMainScene()
        {
            BuildMainScene();
            ValidateMainScene();
        }

        private static GameObject BuildSceneRoot(
            GameObject cardPrefab,
            IReadOnlyDictionary<string, GameObject> slotPrefabs,
            SolitaireDeckConfigSO config)
        {
            GameObject root = new GameObject("SolitaireRoot");
            GameObject deckParent = CreateChild(root.transform, "DeckParent");
            GameObject slotRoot = CreateChild(root.transform, "SlotRoot");
            GameObject foundationRoot = CreateChild(slotRoot.transform, "FoundationSlots");
            GameObject tableauRoot = CreateChild(slotRoot.transform, "TableauSlots");
            GameObject dragParent = CreateChild(root.transform, "DragParent");
            GameObject controllers = CreateChild(root.transform, "Controllers");
            GameObject debug = CreateChild(root.transform, "Debug");
            SolitaireDebugGizmos debugGizmos = debug.AddComponent<SolitaireDebugGizmos>();

            SolitaireBoardCameraController boardCameraController = CreateBoardCamera(root.transform);
            EnsureBoardBackdrop(boardCameraController);
            var cards = new CardView[SolitaireCardUtility.CardCount];
            var slots = new List<SolitaireSlotAnchor>(13);

            for (int i = 0; i < SolitaireCardUtility.CardCount; i++)
            {
                GameObject card = PrefabUtility.InstantiatePrefab(cardPrefab, deckParent.transform) as GameObject;

                if (card == null)
                    throw new InvalidOperationException("Failed to instantiate Card.prefab.");

                card.name = $"Card_{i:00}";
                card.transform.localPosition = new Vector3(0f, 0f, i * -0.005f);
                CardRuntimeIdentity identity = card.GetComponent<CardRuntimeIdentity>();
                identity.SetIdentity(i);
                cards[i] = card.GetComponent<CardView>();
            }

            AddSlot(slotPrefabs["StockSlot"], slotRoot.transform, "StockSlot", new Vector3(-1.84f, 3.2f, 0f), SolitairePileType.Stock, 0, CardSuit.Hearts, slots);
            AddSlot(slotPrefabs["WasteSlot"], slotRoot.transform, "WasteSlot", new Vector3(-1.24f, 3.2f, 0f), SolitairePileType.Waste, 0, CardSuit.Hearts, slots);
            AddSlot(slotPrefabs["FoundationSlot"], foundationRoot.transform, "FoundationSlot_Hearts", new Vector3(0.10f, 3.2f, 0f), SolitairePileType.Foundation, 0, CardSuit.Hearts, slots);
            AddSlot(slotPrefabs["FoundationSlot"], foundationRoot.transform, "FoundationSlot_Diamonds", new Vector3(0.68f, 3.2f, 0f), SolitairePileType.Foundation, 1, CardSuit.Diamonds, slots);
            AddSlot(slotPrefabs["FoundationSlot"], foundationRoot.transform, "FoundationSlot_Clubs", new Vector3(1.26f, 3.2f, 0f), SolitairePileType.Foundation, 2, CardSuit.Clubs, slots);
            AddSlot(slotPrefabs["FoundationSlot"], foundationRoot.transform, "FoundationSlot_Spades", new Vector3(1.84f, 3.2f, 0f), SolitairePileType.Foundation, 3, CardSuit.Spades, slots);

            for (int i = 0; i < SolitaireCardUtility.TableauCount; i++)
            {
                float x = -1.74f + (i * 0.58f);
                AddSlot(slotPrefabs["TableauSlot"], tableauRoot.transform, $"TableauSlot_{i:00}", new Vector3(x, 1.55f, 0f), SolitairePileType.Tableau, i, CardSuit.Hearts, slots);
            }

            dragParent.AddComponent<SolitaireDragLayer>();
            SolitaireModuleBootstrap bootstrap = root.AddComponent<SolitaireModuleBootstrap>();

            GameObject controllerHost = CreateChild(controllers.transform, "ControllerHost");
            SolitaireDeckController deckController = controllerHost.AddComponent<SolitaireDeckController>();
            SolitaireInputController inputController = controllerHost.AddComponent<SolitaireInputController>();
            SolitaireLayoutController layoutController = controllerHost.AddComponent<SolitaireLayoutController>();
            SolitairePointerInputSource pointerInputSource = controllerHost.AddComponent<SolitairePointerInputSource>();
            SolitaireHapticFeedbackProvider hapticFeedbackProvider = controllerHost.AddComponent<SolitaireHapticFeedbackProvider>();
            SolitaireLevelStartBridge levelStartBridge = controllerHost.AddComponent<SolitaireLevelStartBridge>();
            SolitaireWinBridge winBridge = controllerHost.AddComponent<SolitaireWinBridge>();
            controllerHost.AddComponent<SolitaireDebugScenarioRunner>();
            controllerHost.AddComponent<SolitaireModuleControllerHost>();

            WireBootstrap(bootstrap, config);
            SetObject(hapticFeedbackProvider, "deckConfig", config);
            return root;
        }

        private static GameObject RepairExistingSceneRoot(GameObject root, SolitaireDeckConfigSO config)
        {
            Transform deckParent = root.transform.Find("DeckParent");
            Transform dragParent = root.transform.Find("DragParent");
            Transform controllers = root.transform.Find("Controllers");
            SolitaireBoardCameraController boardCameraController = root.GetComponentInChildren<SolitaireBoardCameraController>(true);

            if (deckParent == null)
                throw new InvalidOperationException("Cannot repair SolitaireRoot because DeckParent is missing.");

            if (dragParent == null)
                throw new InvalidOperationException("Cannot repair SolitaireRoot because DragParent is missing.");

            if (controllers == null)
                controllers = CreateChild(root.transform, "Controllers").transform;

            if (boardCameraController == null)
                boardCameraController = CreateBoardCamera(root.transform);

            EnsureBoardBackdrop(boardCameraController);
            EnsureComponent<SolitaireDragLayer>(dragParent.gameObject);
            SolitaireModuleBootstrap bootstrap = EnsureComponent<SolitaireModuleBootstrap>(root);

            GameObject controllerHost = controllers.Find("ControllerHost")?.gameObject
                ?? controllers.Find("SolitaireModuleInstaller")?.gameObject;

            if (controllerHost == null)
                controllerHost = CreateChild(controllers, "ControllerHost");

            controllerHost.name = "ControllerHost";

            SolitaireDeckController deckController = EnsureComponent<SolitaireDeckController>(controllerHost);
            SolitaireInputController inputController = EnsureComponent<SolitaireInputController>(controllerHost);
            SolitaireLayoutController layoutController = EnsureComponent<SolitaireLayoutController>(controllerHost);
            SolitairePointerInputSource pointerInputSource = EnsureComponent<SolitairePointerInputSource>(controllerHost);
            SolitaireHapticFeedbackProvider hapticFeedbackProvider = EnsureComponent<SolitaireHapticFeedbackProvider>(controllerHost);
            SolitaireLevelStartBridge levelStartBridge = EnsureComponent<SolitaireLevelStartBridge>(controllerHost);
            SolitaireWinBridge winBridge = EnsureComponent<SolitaireWinBridge>(controllerHost);
            EnsureComponent<SolitaireDebugScenarioRunner>(controllerHost);
            EnsureComponent<SolitaireModuleControllerHost>(controllerHost);

            Transform slotRootTransform = root.transform.Find("SlotRoot");

            if (slotRootTransform == null)
                throw new InvalidOperationException("Cannot repair SolitaireRoot because SlotRoot is missing.");

            if (deckParent.childCount != SolitaireCardUtility.CardCount)
            {
                throw new InvalidOperationException(
                    $"Cannot repair SolitaireRoot because DeckParent has {deckParent.childCount} children instead of {SolitaireCardUtility.CardCount}.");
            }

            for (int i = 0; i < deckParent.childCount; i++)
            {
                Transform cardTransform = deckParent.GetChild(i);
                CardView card = cardTransform.GetComponent<CardView>();

                if (card == null)
                    throw new InvalidOperationException($"{cardTransform.name} under DeckParent is missing CardView.");

                CardRuntimeIdentity identity = EnsureComponent<CardRuntimeIdentity>(card.gameObject);
                identity.SetIdentity(i);

                if (card.GetComponent<CardDragBehaviour>() == null)
                    throw new InvalidOperationException($"{card.name} is missing CardDragBehaviour.");

                if (card.GetComponent<CardMotionPresenter>() == null)
                    throw new InvalidOperationException($"{card.name} is missing CardMotionPresenter.");

                if (!card.Validate(out string cardError))
                    throw new InvalidOperationException(cardError);
            }

            if (boardCameraController.GetComponent<Camera>() == null)
                throw new InvalidOperationException($"{boardCameraController.name} is missing Camera.");

            SolitaireDebugGizmos debugGizmos = root.GetComponentInChildren<SolitaireDebugGizmos>(true);

            WireBootstrap(bootstrap, config);
            SetObject(hapticFeedbackProvider, "deckConfig", config);
            return root;
        }

        private static void WireBootstrap(SolitaireModuleBootstrap bootstrap, SolitaireDeckConfigSO config)
        {
            SetObject(bootstrap, "deckConfig", config);

            if (!bootstrap.Validate(out string error))
                throw new InvalidOperationException($"Bootstrap wiring failed validation: {error}");
        }

        private static T EnsureComponent<T>(GameObject target)
            where T : Component
        {
            T component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

        private static T GetRequiredSerializedObject<T>(UnityEngine.Object target, string propertyName)
            where T : UnityEngine.Object
        {
            var serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
                throw new InvalidOperationException($"{target.name} is missing serialized property {propertyName}.");

            if (property.objectReferenceValue is T value)
                return value;

            throw new InvalidOperationException($"{target.name} is missing required {typeof(T).Name} reference in {propertyName}.");
        }

        private static void ValidateControllerHost(GameObject controllerHost)
        {
            if (controllerHost == null)
                throw new InvalidOperationException("ControllerHost was not found under SolitaireRoot/Controllers.");

            RequireControllerHostComponent<SolitaireModuleControllerHost>(controllerHost);
            RequireControllerHostComponent<SolitaireDeckController>(controllerHost);
            RequireControllerHostComponent<SolitaireInputController>(controllerHost);
            RequireControllerHostComponent<SolitaireLayoutController>(controllerHost);
            RequireControllerHostComponent<SolitairePointerInputSource>(controllerHost);
            RequireControllerHostComponent<SolitaireHapticFeedbackProvider>(controllerHost);
            RequireControllerHostComponent<SolitaireLevelStartBridge>(controllerHost);
            RequireControllerHostComponent<SolitaireWinBridge>(controllerHost);
        }

        private static void RequireControllerHostComponent<T>(GameObject controllerHost)
            where T : Component
        {
            if (controllerHost.GetComponent<T>() == null)
                throw new InvalidOperationException($"{typeof(T).Name} is missing on ControllerHost.");
        }

        private static void CleanupLegacyRunnerObjects()
        {
            string[] exactNames =
            {
                "InputManager",
                "LevelHolder",
                "LevelManager",
                "CharacterManager",
                "UpgradeManager",
                "LevelTimerManager",
                "CollectableAnimationProvider",
                "Character Camera",
                "InGame Camera",
                "EndMeta Camera"
            };

            Transform[] allTransforms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (Transform current in allTransforms)
            {
                if (current == null)
                    continue;

                string objectName = current.name;

                if (IsExactLegacyName(objectName, exactNames) ||
                    objectName.Contains("Road", StringComparison.OrdinalIgnoreCase) ||
                    objectName.Contains("Weapon", StringComparison.OrdinalIgnoreCase) ||
                    objectName.Contains("Projectile", StringComparison.OrdinalIgnoreCase) ||
                    objectName.Contains("CharacterMaster", StringComparison.OrdinalIgnoreCase) ||
                    objectName.Contains("StickMan", StringComparison.OrdinalIgnoreCase))
                {
                    UnityEngine.Object.DestroyImmediate(current.gameObject);
                }
            }
        }

        private static string FindLegacyRunnerObjectName()
        {
            string[] exactNames =
            {
                "InputManager",
                "LevelHolder",
                "LevelManager",
                "CharacterManager",
                "UpgradeManager",
                "LevelTimerManager",
                "CollectableAnimationProvider",
                "Character Camera",
                "InGame Camera",
                "EndMeta Camera"
            };

            Transform[] allTransforms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (Transform current in allTransforms)
            {
                string objectName = current.name;

                if (IsExactLegacyName(objectName, exactNames) ||
                    objectName.Contains("Road", StringComparison.OrdinalIgnoreCase) ||
                    objectName.Contains("Weapon", StringComparison.OrdinalIgnoreCase) ||
                    objectName.Contains("Projectile", StringComparison.OrdinalIgnoreCase) ||
                    objectName.Contains("CharacterMaster", StringComparison.OrdinalIgnoreCase) ||
                    objectName.Contains("StickMan", StringComparison.OrdinalIgnoreCase))
                {
                    return objectName;
                }
            }

            return string.Empty;
        }

        private static bool IsExactLegacyName(string objectName, IReadOnlyList<string> exactNames)
        {
            for (int i = 0; i < exactNames.Count; i++)
            {
                if (string.Equals(objectName, exactNames[i], StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static SolitaireBoardCameraController CreateBoardCamera(Transform root)
        {
            GameObject cameraObject = CreateChild(root, "SolitaireBoardCamera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0.35f, -10f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BoardFallbackBackgroundColor;
            camera.depth = 0f;
            cameraObject.AddComponent<AudioListener>();
            return cameraObject.AddComponent<SolitaireBoardCameraController>();
        }

        private static void EnsureBoardBackdrop(SolitaireBoardCameraController boardCameraController)
        {
            Transform cameraTransform = boardCameraController.transform;
            Transform backdropTransform = cameraTransform.Find("BoardBackdrop");
            GameObject backdropObject = backdropTransform != null
                ? backdropTransform.gameObject
                : CreateChild(cameraTransform, "BoardBackdrop");

            SpriteRenderer renderer = EnsureComponent<SpriteRenderer>(backdropObject);
            EnsureComponent<SolitaireBoardBackdropPresenter>(backdropObject);

            Sprite backdropSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BoardBackdropSpritePath);

            if (backdropSprite == null)
                throw new InvalidOperationException($"Missing board backdrop sprite at {BoardBackdropSpritePath}.");

            renderer.sprite = backdropSprite;
            renderer.color = Color.white;
            renderer.sortingOrder = -1000;

            Camera camera = boardCameraController.GetComponent<Camera>();

            if (camera != null)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = BoardFallbackBackgroundColor;
            }
        }

        private static void AddSlot(
            GameObject slotPrefab,
            Transform parent,
            string name,
            Vector3 position,
            SolitairePileType pileType,
            int pileIndex,
            CardSuit foundationSuit,
            ICollection<SolitaireSlotAnchor> slots)
        {
            GameObject slot = PrefabUtility.InstantiatePrefab(slotPrefab, parent) as GameObject;

            if (slot == null)
                throw new InvalidOperationException($"Failed to instantiate {slotPrefab.name}.");

            slot.name = name;
            slot.transform.position = position;
            SolitaireSlotAnchor anchor = slot.GetComponent<SolitaireSlotAnchor>();
            anchor.Configure(pileType, pileIndex, foundationSuit);
            slots.Add(anchor);
        }

        private static GameObject EnsureCardPrefab(Sprite frontSprite)
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(CardPrefabPath);

            if (existing != null)
            {
                UpdateExistingCardPrefab(frontSprite);
                return AssetDatabase.LoadAssetAtPath<GameObject>(CardPrefabPath);
            }

            GameObject card = new GameObject("Card");
            SpriteRenderer renderer = card.AddComponent<SpriteRenderer>();
            renderer.sprite = frontSprite;
            renderer.sortingOrder = 100;
            ApplyCardWorldSize(card, frontSprite);
            card.AddComponent<SortingGroup>();
            BoxCollider2D collider = card.AddComponent<BoxCollider2D>();
            collider.size = GetUnscaledColliderSize(card.transform.localScale.x);
            collider.isTrigger = true;
            CardRuntimeIdentity identity = card.AddComponent<CardRuntimeIdentity>();
            card.AddComponent<CardVisualStateMachine>();
            CardDragBehaviour dragBehaviour = card.AddComponent<CardDragBehaviour>();
            CardInputReceiver inputReceiver = card.AddComponent<CardInputReceiver>();
            CardMotionPresenter motionPresenter = card.AddComponent<CardMotionPresenter>();
            CardView view = card.AddComponent<CardView>();

            SetObject(inputReceiver, "identity", identity);
            SetObject(inputReceiver, "view", view);
            SetObject(inputReceiver, "dragBehaviour", dragBehaviour);
            SetObject(view, "identity", identity);
            SetObject(view, "visualStateMachine", card.GetComponent<CardVisualStateMachine>());
            SetObject(view, "cardRenderer", renderer);
            SetObject(view, "sortingGroup", card.GetComponent<SortingGroup>());
            SetObject(view, "motionPresenter", motionPresenter);
            EnsureCardDragShadow(card, renderer, view);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(card, CardPrefabPath);
            UnityEngine.Object.DestroyImmediate(card);
            return prefab;
        }

        private static void UpdateExistingCardPrefab(Sprite frontSprite)
        {
            GameObject card = PrefabUtility.LoadPrefabContents(CardPrefabPath);
            SpriteRenderer renderer = card.GetComponent<SpriteRenderer>();

            if (renderer != null)
                renderer.sprite = frontSprite;

            RemoveCardLabels(card.transform);
            ApplyCardWorldSize(card, frontSprite);
            BoxCollider2D collider = card.GetComponent<BoxCollider2D>();

            if (collider != null)
            {
                collider.size = GetUnscaledColliderSize(card.transform.localScale.x);
                collider.isTrigger = true;
            }

            CardMotionPresenter motionPresenter = card.GetComponent<CardMotionPresenter>();

            if (motionPresenter == null)
                motionPresenter = card.AddComponent<CardMotionPresenter>();

            CardView view = card.GetComponent<CardView>();

            if (view != null)
            {
                SetObject(view, "motionPresenter", motionPresenter);
                EnsureCardDragShadow(card, renderer, view);
            }

            PrefabUtility.SaveAsPrefabAsset(card, CardPrefabPath);
            PrefabUtility.UnloadPrefabContents(card);
        }

        private static void RemoveCardLabels(Transform card)
        {
            RemoveChildIfExists(card, "RankLabel");
            RemoveChildIfExists(card, "SuitLabel");
        }

        private static void EnsureCardDragShadow(GameObject card, SpriteRenderer cardRenderer, CardView view)
        {
            Transform existing = card.transform.Find("DragShadow");
            GameObject shadowObject;
            SpriteRenderer shadowRenderer;

            if (existing != null)
            {
                shadowObject = existing.gameObject;
                shadowRenderer = existing.GetComponent<SpriteRenderer>();

                if (shadowRenderer == null)
                    shadowRenderer = shadowObject.AddComponent<SpriteRenderer>();
            }
            else
            {
                shadowObject = new GameObject("DragShadow");
                shadowObject.transform.SetParent(card.transform, false);
                shadowObject.transform.localPosition = new Vector3(0.04f, -0.05f, 0.01f);
                shadowRenderer = shadowObject.AddComponent<SpriteRenderer>();
            }

            shadowRenderer.sortingOrder = -1;

            if (cardRenderer != null)
            {
                Sprite dragShadowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(DragShadowSpritePath);
                shadowRenderer.sprite = dragShadowSprite != null ? dragShadowSprite : cardRenderer.sprite;
                shadowRenderer.sortingLayerID = cardRenderer.sortingLayerID;
            }

            Color shadowColor = Color.black;
            shadowColor.a = 0f;
            shadowRenderer.color = shadowColor;
            SetObject(view, "dragShadowRenderer", shadowRenderer);
        }

        private static void RemoveChildIfExists(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);

            if (child != null)
                UnityEngine.Object.DestroyImmediate(child.gameObject);
        }

        private static void ApplyCardWorldSize(GameObject card, Sprite sprite)
        {
            if (sprite == null || sprite.bounds.size.x <= 0f || sprite.bounds.size.y <= 0f)
            {
                card.transform.localScale = Vector3.one;
                return;
            }

            float scale = Mathf.Min(CardWidth / sprite.bounds.size.x, CardHeight / sprite.bounds.size.y);
            card.transform.localScale = new Vector3(scale, scale, 1f);
        }

        private static Vector2 GetUnscaledColliderSize(float scale)
        {
            if (scale <= 0f)
                return new Vector2(CardWidth, CardHeight);

            return new Vector2(CardWidth / scale, CardHeight / scale);
        }

        private static Dictionary<string, GameObject> EnsureSlotPrefabs(Sprite slotSprite)
        {
            var prefabs = new Dictionary<string, GameObject>
            {
                ["StockSlot"] = EnsureSlotPrefab("StockSlot", SolitairePileType.Stock, 0, slotSprite),
                ["WasteSlot"] = EnsureSlotPrefab("WasteSlot", SolitairePileType.Waste, 0, slotSprite),
                ["FoundationSlot"] = EnsureSlotPrefab("FoundationSlot", SolitairePileType.Foundation, 0, slotSprite),
                ["TableauSlot"] = EnsureSlotPrefab("TableauSlot", SolitairePileType.Tableau, 0, slotSprite)
            };

            return prefabs;
        }

        private static GameObject EnsureSlotPrefab(string name, SolitairePileType pileType, int pileIndex, Sprite slotSprite)
        {
            string path = $"{SlotPrefabFolder}/{name}.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (existing != null)
            {
                UpdateExistingSlotPrefab(existing, pileType, pileIndex, slotSprite);
                return existing;
            }

            GameObject slot = new GameObject(name);
            SpriteRenderer renderer = slot.AddComponent<SpriteRenderer>();
            renderer.sprite = slotSprite;
            renderer.sortingOrder = 10;
            renderer.color = SlotMarkerColor;
            BoxCollider2D collider = slot.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(CardWidth, CardHeight);
            collider.isTrigger = true;
            SolitaireSlotAnchor anchor = slot.AddComponent<SolitaireSlotAnchor>();
            anchor.Configure(pileType, pileIndex, CardSuit.Hearts);
            SetObject(anchor, "boxCollider", collider);
            SetObject(anchor, "highlightRenderer", renderer);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(slot, path);
            UnityEngine.Object.DestroyImmediate(slot);
            return prefab;
        }

        private static void UpdateExistingSlotPrefab(GameObject prefab, SolitairePileType pileType, int pileIndex, Sprite slotSprite)
        {
            string path = AssetDatabase.GetAssetPath(prefab);
            GameObject root = PrefabUtility.LoadPrefabContents(path);

            SpriteRenderer renderer = root.GetComponent<SpriteRenderer>();
            if (renderer == null)
                renderer = root.AddComponent<SpriteRenderer>();

            renderer.sprite = slotSprite;
            renderer.sortingOrder = 10;
            renderer.color = SlotMarkerColor;

            BoxCollider2D collider = root.GetComponent<BoxCollider2D>();
            if (collider == null)
                collider = root.AddComponent<BoxCollider2D>();

            collider.size = new Vector2(CardWidth, CardHeight);
            collider.isTrigger = true;

            SolitaireSlotAnchor anchor = root.GetComponent<SolitaireSlotAnchor>();
            if (anchor == null)
                anchor = root.AddComponent<SolitaireSlotAnchor>();

            anchor.Configure(pileType, pileIndex, CardSuit.Hearts);
            SetObject(anchor, "boxCollider", collider);
            SetObject(anchor, "highlightRenderer", renderer);

            if (pileType == SolitairePileType.Stock)
                EnsureStockSlotDeckRippleFx(root);

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static GameObject EnsureDeckRippleParticlePrefab()
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(DeckRippleParticlePrefabPath);

            if (existing != null)
                return existing;

            var particleRoot = new GameObject("DeckRippleParticle");
            particleRoot.AddComponent<SolitaireDeckRippleParticleView>();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(particleRoot, DeckRippleParticlePrefabPath);
            UnityEngine.Object.DestroyImmediate(particleRoot);
            return prefab;
        }

        private static void WireDeckRippleParticleIntoStockSlot()
        {
            string stockSlotPath = $"{SlotPrefabFolder}/StockSlot.prefab";
            GameObject stockSlotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(stockSlotPath);

            if (stockSlotPrefab == null)
                return;

            GameObject root = PrefabUtility.LoadPrefabContents(stockSlotPath);
            EnsureStockSlotDeckRippleFx(root);
            PrefabUtility.SaveAsPrefabAsset(root, stockSlotPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void EnsureStockSlotDeckRippleFx(GameObject stockSlotRoot)
        {
            GameObject rippleParticlePrefab = EnsureDeckRippleParticlePrefab();

            SolitaireDeckRippleFx rippleFx = stockSlotRoot.GetComponent<SolitaireDeckRippleFx>();
            if (rippleFx == null)
                rippleFx = stockSlotRoot.AddComponent<SolitaireDeckRippleFx>();

            Transform rippleFxRoot = stockSlotRoot.transform.Find("DeckRippleFx");
            if (rippleFxRoot == null)
            {
                var rippleFxObject = new GameObject("DeckRippleFx");
                rippleFxRoot = rippleFxObject.transform;
                rippleFxRoot.SetParent(stockSlotRoot.transform, false);
                rippleFxRoot.localPosition = new Vector3(0f, 0f, -0.01f);
            }

            SolitairePulseRingView pulseRing = rippleFxRoot.GetComponent<SolitairePulseRingView>();
            if (pulseRing == null)
                pulseRing = rippleFxRoot.gameObject.AddComponent<SolitairePulseRingView>();

            Transform particleRoot = rippleFxRoot.Find("DeckRippleParticle");
            if (particleRoot == null)
            {
                GameObject particleInstance = (GameObject)PrefabUtility.InstantiatePrefab(rippleParticlePrefab, rippleFxRoot);
                particleInstance.name = "DeckRippleParticle";
                particleRoot = particleInstance.transform;
            }

            SolitaireDeckRippleParticleView particleView = particleRoot.GetComponent<SolitaireDeckRippleParticleView>();
            SetObject(rippleFx, "pulseRing", pulseRing);
            SetObject(rippleFx, "rippleParticles", particleView != null ? particleView.RippleParticles : particleRoot.GetComponent<ParticleSystem>());
        }

        private static SolitaireDeckConfigSO EnsureConfig(
            CardView cardPrefab,
            SolitaireCardVisualCatalogSO visualCatalog,
            Sprite frontSprite,
            Sprite backSprite)
        {
            SolitaireDeckConfigSO config = AssetDatabase.LoadAssetAtPath<SolitaireDeckConfigSO>(ConfigPath);

            if (config == null)
            {
                config = ScriptableObject.CreateInstance<SolitaireDeckConfigSO>();
                AssetDatabase.CreateAsset(config, ConfigPath);
            }

            SetObject(config, "cardPrefab", cardPrefab);
            SetObject(config, "cardVisualCatalog", visualCatalog);
            SetObject(config, "cardFrontSprite", frontSprite);
            SetObject(config, "cardBackSprite", backSprite);
            SetVector2(config, "cardSize", new Vector2(CardWidth, CardHeight));
            SetFloat(config, "cardAspectRatio", CardAspectRatio);
            SetFloat(config, "cardHorizontalSpacingRatio", CardHorizontalSpacingRatio);
            SetFloat(config, "maxResponsiveCardWidth", MaxResponsiveCardWidth);
            SetFloat(config, "boardHorizontalPadding", BoardHorizontalPadding);
            SetFloat(config, "boardTopHudPadding", BoardTopHudPadding);
            SetFloat(config, "boardBottomPadding", BoardBottomPadding);
            SetFloat(config, "rowVerticalGap", RowVerticalGap);
            SetFloat(config, "faceUpTableauYOffset", FaceUpTableauYOffset);
            SetFloat(config, "faceDownTableauYOffset", FaceDownTableauYOffset);
            SetFloat(config, "minCompressedFaceUpYOffset", MinCompressedFaceUpYOffset);
            SetFloat(config, "tableauBottomPlayableY", TableauBottomPlayableY);
            SetFloat(config, "wasteStackXOffset", WasteStackXOffset);
            SetFloat(config, "dropSnapDistance", DropSnapDistance);
            return config;
        }

        private static SolitaireCardVisualCatalogSO LoadVisualCatalog()
        {
            SolitaireCardVisualCatalogSO catalog = AssetDatabase.LoadAssetAtPath<SolitaireCardVisualCatalogSO>(VisualCatalogPath);

            if (catalog == null)
                throw new InvalidOperationException($"Solitaire visual catalog was not found at {VisualCatalogPath}.");

            if (!catalog.ValidateComplete(out string error))
                throw new InvalidOperationException(error);

            return catalog;
        }

        private static Sprite EnsureSprite(string name, Color color)
        {
            string texturePath = $"{ArtFolder}/{name}.png";
            var texture = new Texture2D(160, 228, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);
            Color shadow = new Color(0f, 0f, 0f, 0.18f);
            Color fill = new Color(0.70f, 1f, 0.80f, 0.11f);
            Color innerGlow = new Color(1f, 1f, 1f, 0.12f);
            Color rim = new Color(0.87f, 1f, 0.92f, 0.58f);
            Color highlight = new Color(1f, 1f, 1f, 0.38f);
            const int radius = 18;
            const int border = 4;
            const int inset = 8;

            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    bool inShadow = IsRoundedRectPixel(x - 3, y + 4, texture.width, texture.height, radius + 2);
                    bool inOuter = IsRoundedRectPixel(x, y, texture.width, texture.height, radius);
                    bool inInner = IsRoundedRectPixel(x - inset, y - inset, texture.width - (inset * 2), texture.height - (inset * 2), radius - 6);

                    Color pixel = clear;

                    if (inShadow)
                        pixel = shadow;

                    if (inOuter)
                        pixel = fill;

                    if (inOuter && !IsRoundedRectPixel(x - border, y - border, texture.width - (border * 2), texture.height - (border * 2), radius - 3))
                        pixel = rim;

                    if (inInner && (x < inset + border || x > texture.width - inset - border - 1 ||
                                    y < inset + border || y > texture.height - inset - border - 1))
                    {
                        pixel = Color.Lerp(pixel, innerGlow, innerGlow.a);
                    }

                    if (inOuter && y > texture.height - 34 && x > 18 && x < texture.width - 18)
                        pixel = Color.Lerp(pixel, highlight, 0.32f);

                    texture.SetPixel(x, y, pixel);
                }
            }

            texture.Apply();
            System.IO.File.WriteAllBytes(texturePath, texture.EncodeToPNG());
            AssetDatabase.ImportAsset(texturePath);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = texture.width / CardWidth;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();

            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);

            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Sprite sprite)
                    return sprite;
            }

            throw new InvalidOperationException($"Sprite asset could not be loaded from {texturePath}.");
        }

        private static bool IsRoundedRectPixel(int x, int y, int width, int height, int radius)
        {
            if (x < 0 || y < 0 || x >= width || y >= height)
                return false;

            int left = radius;
            int right = width - radius - 1;
            int bottom = radius;
            int top = height - radius - 1;
            int closestX = Mathf.Clamp(x, left, right);
            int closestY = Mathf.Clamp(y, bottom, top);
            int dx = x - closestX;
            int dy = y - closestY;
            return (dx * dx) + (dy * dy) <= radius * radius;
        }

        private static GameObject CreateChild(Transform parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child;
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/_Game", "Config");
            EnsureFolder("Assets/_Game/Config", "Solitaire");
            EnsureFolder("Assets/_Game", "Art");
            EnsureFolder("Assets/_Game/Art", "Solitaire");
            EnsureFolder("Assets/_Game/Prefabs/_InGame", "Solitaire");
            EnsureFolder(PrefabFolder, "Slots");
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{child}"))
                AssetDatabase.CreateFolder(parent, child);
        }

        private static void SetObject(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            var serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
                throw new InvalidOperationException($"{target.name} is missing serialized property {propertyName}.");

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetVector2(UnityEngine.Object target, string propertyName, Vector2 value)
        {
            var serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
                throw new InvalidOperationException($"{target.name} is missing serialized property {propertyName}.");

            property.vector2Value = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetFloat(UnityEngine.Object target, string propertyName, float value)
        {
            var serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
                throw new InvalidOperationException($"{target.name} is missing serialized property {propertyName}.");

            property.floatValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void RepairInGameUndoUiInOpenScene()
        {
            InGameView inGameView = UnityEngine.Object.FindFirstObjectByType<InGameView>(FindObjectsInactive.Include);
            if (inGameView == null)
                throw new InvalidOperationException("InGameView was not found in the open scene.");

            Transform inGameScreen = inGameView.transform.Find("InGameScreen");
            if (inGameScreen == null)
                throw new InvalidOperationException("InGameScreen child was not found under InGameView.");

            SolitaireUndoButton undoButton = inGameScreen.GetComponentInChildren<SolitaireUndoButton>(true);
            if (undoButton == null)
            {
                GameObject undoPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(UndoButtonPrefabPath);
                if (undoPrefab == null)
                    throw new InvalidOperationException($"Missing undo button prefab at {UndoButtonPrefabPath}.");

                GameObject undoInstance = PrefabUtility.InstantiatePrefab(undoPrefab, inGameScreen) as GameObject;
                if (undoInstance == null)
                    throw new InvalidOperationException("Failed to instantiate Button_SolitaireUndo.");

                undoInstance.name = "UndoButton";
                RectTransform undoRect = undoInstance.GetComponent<RectTransform>();
                undoRect.anchorMin = new Vector2(0.38f, 0.9215722f);
                undoRect.anchorMax = new Vector2(0.56f, 0.95000005f);
                undoRect.offsetMin = Vector2.zero;
                undoRect.offsetMax = Vector2.zero;
                undoButton = undoInstance.GetComponent<SolitaireUndoButton>();
            }

            undoButton.gameObject.SetActive(true);

            SolitaireDeckController deckController = UnityEngine.Object.FindFirstObjectByType<SolitaireDeckController>(FindObjectsInactive.Include);
            if (deckController == null)
                throw new InvalidOperationException("SolitaireDeckController was not found in the open scene.");

            SerializedObject undoSerializedObject = new SerializedObject(undoButton);
            undoSerializedObject.FindProperty("deckController").objectReferenceValue = deckController;
            undoSerializedObject.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject inGameViewSerializedObject = new SerializedObject(inGameView);
            inGameViewSerializedObject.FindProperty("undoButton").objectReferenceValue = undoButton;
            inGameViewSerializedObject.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(inGameView.gameObject);
            EditorUtility.SetDirty(undoButton.gameObject);
        }

        private static void SetArray<T>(UnityEngine.Object target, string propertyName, T[] values)
            where T : UnityEngine.Object
        {
            var serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null || !property.isArray)
                throw new InvalidOperationException($"{target.name} is missing serialized array {propertyName}.");

            property.arraySize = values.Length;

            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }
    }
}
