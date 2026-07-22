using System;
using System.Collections.Generic;
using System.IO;
using PokedexAR;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PokedexAR.Editor
{
    /// <summary>Builds the complete portrait AR scene from the supplied assets.</summary>
    public static class PokedexProjectBuilder
    {
        private const string ScenePath = "Assets/Scenes/PokedexAR.unity";
        private const string PokemonRoot = "Assets/Pokemon";
        private static readonly Color Ink = new Color32(13, 20, 32, 255);
        private static readonly Color Paper = new Color32(238, 244, 248, 255);
        private static readonly Color Red = new Color32(224, 59, 62, 255);
        private static readonly Color Cyan = new Color32(47, 198, 211, 255);

        /// <summary>Creates materials, models, interaction objects, responsive UI and build settings.</summary>
        [MenuItem("Pokedex AR/Build Complete Project")]
        public static void BuildProject()
        {
            ConfigureTargetTexture("Assets/Pokemon/Targets/AbraTarget.png");
            ConfigureTargetTexture("Assets/Pokemon/Targets/FroakieTarget.png");
            ConfigureTargetTexture("Assets/Editor/Vuforia/ImageTargetTextures/taller_3/abra_scaled.jpg");
            ConfigureTargetTexture("Assets/Editor/Vuforia/ImageTargetTextures/taller_3/greninja_scaled.jpg");
            ConfigureTargetTexture("Assets/Editor/Vuforia/ImageTargetTextures/taller_3/rati_scaled.jpg");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera camera = CreateCameraAndLighting();
            CreateEnvironment();
            PokedexPanelController panel = CreateInterface(camera, out Text statusLabel, out Transform selectorRow);

            PokemonTargetController psychicTarget = CreatePokemonTarget(
                "AbraLine",
                new Vector3(-0.72f, 0.12f, -0.35f),
                "Assets/Editor/Vuforia/ImageTargetTextures/taller_3/abra_scaled.jpg",
                new[]
                {
                    Stage("Abra", "Psíquico", "Percibe el peligro y se teletransporta incluso mientras duerme.", "Assets/Pokemon/Models/Abra/Abra.FBX", new Color32(246, 191, 62, 255), 1.04f),
                    Stage("Kadabra", "Psíquico", "Su poderosa energía psíquica altera aparatos electrónicos cercanos.", "Assets/Pokemon/Models/Kadabra/KadabraM.FBX", new Color32(239, 151, 51, 255), 1.16f),
                    Stage("Alakazam", "Psíquico", "Su cerebro nunca deja de crecer y recuerda cada experiencia desde que nace.", "Assets/Pokemon/Models/Alakazam/AlakazamM.FBX", new Color32(205, 91, 179, 255), 1.28f)
                },
                panel);

            PokemonTargetController waterTarget = CreatePokemonTarget(
                "FroakieLine",
                new Vector3(0.72f, 0.12f, -0.35f),
                "Assets/Editor/Vuforia/ImageTargetTextures/taller_3/greninja_scaled.jpg",
                new[]
                {
                    Stage("Froakie", "Agua", "Sus burbujas elásticas amortiguan ataques y le permiten moverse con agilidad.", "Assets/Pokemon/Models/Froakie/Froakie.FBX", new Color32(66, 188, 222, 255), 0.92f),
                    Stage("Frogadier", "Agua", "Lanza piedras envueltas en burbujas con una precisión extraordinaria.", "Assets/Pokemon/Models/Frogadier/Frogadier.FBX", new Color32(47, 138, 205, 255), 1.03f),
                    Stage("Greninja", "Agua / Siniestro", "Crea estrellas arrojadizas de agua capaces de cortar materiales resistentes.", "Assets/Pokemon/Models/Greninja/Greninja.FBX", new Color32(37, 78, 145, 255), 1.15f)
                },
                panel,
                2);

            PokemonTargetController normalTarget = CreatePokemonTarget(
                "RattataLine",
                new Vector3(0f, 0.12f, 0.45f),
                "Assets/Editor/Vuforia/ImageTargetTextures/taller_3/rati_scaled.jpg",
                new[]
                {
                    Stage("Rattata", "Normal", "Roe todo lo que encuentra y puede sobrevivir en casi cualquier lugar.", "Assets/Pokemon/Models/Rattata/Rattata_M.FBX", new Color32(157, 104, 184, 255), 1.08f),
                    Stage("Raticate", "Normal", "Usa sus fuertes incisivos para derribar obstaculos y defender su territorio.", "Assets/Pokemon/Models/Raticate/Raticate_M.FBX", new Color32(191, 142, 76, 255), 0.94f)
                },
                panel);

            CreateSelectorButton(selectorRow, "NORMAL", new Color32(113, 101, 128, 255), normalTarget);
            CreateSelectorButton(selectorRow, "PSÍQUICO", new Color32(119, 62, 143, 255), psychicTarget);
            CreateSelectorButton(selectorRow, "AGUA", new Color32(27, 129, 170, 255), waterTarget);

            GameObject systems = new GameObject("AR Systems");
            PokemonInputController input = systems.AddComponent<PokemonInputController>();
            input.Configure(camera);

            VuforiaRuntimeTargetFactory factory = systems.AddComponent<VuforiaRuntimeTargetFactory>();
            factory.Configure(camera, new[]
            {
                new VuforiaRuntimeTargetFactory.InstantTarget
                {
                    databaseName = "taller_3",
                    databaseTargetName = "rati",
                    image = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Vuforia/ImageTargetTextures/taller_3/rati_scaled.jpg"),
                    physicalWidth = 0.12f,
                    contentScale = 0.06f,
                    content = normalTarget
                },
                new VuforiaRuntimeTargetFactory.InstantTarget
                {
                    databaseName = "taller_3",
                    databaseTargetName = "greninja",
                    image = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Vuforia/ImageTargetTextures/taller_3/greninja_scaled.jpg"),
                    physicalWidth = 0.12f,
                    contentScale = 0.06f,
                    content = waterTarget
                },
                new VuforiaRuntimeTargetFactory.InstantTarget
                {
                    databaseName = "taller_3",
                    databaseTargetName = "abra",
                    image = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Vuforia/ImageTargetTextures/taller_3/abra_scaled.jpg"),
                    physicalWidth = 0.12f,
                    contentScale = 0.06f,
                    content = psychicTarget
                }
            }, statusLabel);

            ConfigurePlayer();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("Pokedex AR project built successfully.");
        }

        /// <summary>Builds a portrait Android APK using the requested delivery name.</summary>
        [MenuItem("Pokedex AR/Build Android APK")]
        public static void BuildAndroid()
        {
            BuildProject();
            VuforiaSceneSetup.PrepareProject();
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Builds"));
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/T3_21652040-8.apk",
                targetGroup = BuildTargetGroup.Android,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };
            BuildPipeline.BuildPlayer(options);
        }

        /// <summary>Renders a 1080x2400 preview for visual quality assurance.</summary>
        [MenuItem("Pokedex AR/Capture Portrait Preview")]
        public static void CapturePreview()
        {
            BuildProject();
            EditorSceneManager.OpenScene(ScenePath);
            Camera camera = Camera.main;
            PokedexPanelController panel = UnityEngine.Object.FindObjectOfType<PokedexPanelController>();
            CanvasGroup panelGroup = panel.GetComponent<CanvasGroup>();
            panelGroup.alpha = 0f;

            string outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Documentation");
            Directory.CreateDirectory(outputDirectory);
            RenderPreview(camera, Path.Combine(outputDirectory, "PokedexAR_Preview.png"));

            panel.transform.Find("Pokemon Name").GetComponent<Text>().text = "ABRA";
            panel.transform.Find("Pokemon Type").GetComponent<Text>().text = "TIPO  PSÍQUICO";
            panel.transform.Find("Pokemon Description").GetComponent<Text>().text =
                "Percibe el peligro y se teletransporta incluso mientras duerme.";
            panel.transform.Find("Accent Stripe").GetComponent<Image>().color = new Color32(246, 191, 62, 255);
            panelGroup.alpha = 1f;
            RenderPreview(camera, Path.Combine(outputDirectory, "PokedexAR_Detail.png"));
            Debug.Log("Portrait previews saved to Documentation.");
        }

        private static void RenderPreview(Camera camera, string outputPath)
        {
            RenderTexture renderTexture = new RenderTexture(1080, 2400, 24, RenderTextureFormat.ARGB32);
            Texture2D screenshot = new Texture2D(1080, 2400, TextureFormat.RGB24, false);
            RenderTexture previous = RenderTexture.active;
            camera.targetTexture = renderTexture;
            camera.Render();
            RenderTexture.active = renderTexture;
            screenshot.ReadPixels(new Rect(0, 0, 1080, 2400), 0, 0);
            screenshot.Apply();
            camera.targetTexture = null;
            RenderTexture.active = previous;

            File.WriteAllBytes(outputPath, screenshot.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(screenshot);
            renderTexture.Release();
            UnityEngine.Object.DestroyImmediate(renderTexture);
        }

        private static Camera CreateCameraAndLighting()
        {
            GameObject cameraObject = new GameObject("AR Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 4.4f, -7.8f);
            cameraObject.transform.rotation = Quaternion.Euler(22f, 0f, 0f);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(7, 13, 23, 255);
            camera.fieldOfView = 39f;
            camera.nearClipPlane = 0.03f;

            GameObject keyLight = new GameObject("Key Light", typeof(Light));
            keyLight.transform.rotation = Quaternion.Euler(42f, -28f, 0f);
            Light key = keyLight.GetComponent<Light>();
            key.type = LightType.Directional;
            key.color = new Color32(255, 236, 214, 255);
            key.intensity = 1.1f;
            key.shadows = LightShadows.Soft;

            GameObject fillLight = new GameObject("Cyan Fill", typeof(Light));
            fillLight.transform.position = new Vector3(0f, 4f, -1f);
            Light fill = fillLight.GetComponent<Light>();
            fill.type = LightType.Point;
            fill.color = Cyan;
            fill.range = 10f;
            fill.intensity = 1.7f;
            return camera;
        }

        private static void CreateEnvironment()
        {
            Material floorMaterial = CreateMaterial("Floor", new Color32(18, 28, 42, 255), null);
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Demo Surface";
            floor.transform.localScale = new Vector3(0.75f, 1f, 0.55f);
            floor.GetComponent<Renderer>().sharedMaterial = floorMaterial;

            for (int index = 0; index < 7; index++)
            {
                GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
                line.name = $"Grid Line {index + 1}";
                line.transform.position = new Vector3(-3.3f + index * 1.1f, 0.012f, 0.9f);
                line.transform.localScale = new Vector3(0.012f, 0.012f, 5.3f);
                line.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Grid", new Color32(30, 93, 107, 255), null);
                UnityEngine.Object.DestroyImmediate(line.GetComponent<Collider>());
            }
        }

        private static PokedexPanelController CreateInterface(Camera camera, out Text statusLabel, out Transform selectorRow)
        {
            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            GameObject canvasObject = new GameObject("Portrait UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 0.5f;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 2400f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.7f;

            GameObject safeArea = UiObject("Safe Area", canvasObject.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            safeArea.AddComponent<SafeAreaFitter>();

            Image topBar = UiImage("Top Bar", safeArea.transform, new Color32(9, 18, 31, 238),
                new Vector2(0f, 0.87f), Vector2.one, Vector2.zero, Vector2.zero);
            Text title = UiText("Title", topBar.transform, "POKÉDEX  AR", 54, FontStyle.Bold, Paper,
                new Vector2(0.07f, 0.45f), new Vector2(0.93f, 0.94f), Vector2.zero, Vector2.zero, TextAnchor.MiddleLeft);
            title.resizeTextForBestFit = true;
            title.resizeTextMinSize = 34;
            title.resizeTextMaxSize = 54;
            statusLabel = UiText("Status", topBar.transform, "MODO DEMO", 23, FontStyle.Normal, Cyan,
                new Vector2(0.07f, 0.13f), new Vector2(0.93f, 0.45f), Vector2.zero, Vector2.zero, TextAnchor.MiddleLeft);

            GameObject selector = UiObject("Target Selector", safeArea.transform,
                new Vector2(0.07f, 0.77f), new Vector2(0.93f, 0.845f), Vector2.zero, Vector2.zero);
            HorizontalLayoutGroup layout = selector.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 24f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            selectorRow = selector.transform;

            Text hint = UiText("Gesture Hint", safeArea.transform, "DESLIZA  ‹  EVOLUCIONES  ›   ·   TOCA PARA IDENTIFICAR",
                24, FontStyle.Bold, new Color32(202, 216, 226, 255),
                new Vector2(0.05f, 0.045f), new Vector2(0.95f, 0.095f), Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
            hint.resizeTextForBestFit = true;
            hint.resizeTextMinSize = 17;
            hint.resizeTextMaxSize = 24;

            GameObject panelRoot = UiObject("Pokedex Panel", safeArea.transform,
                new Vector2(0.035f, 0.11f), new Vector2(0.965f, 0.42f), Vector2.zero, Vector2.zero);
            Image panelBackground = panelRoot.AddComponent<Image>();
            panelBackground.color = new Color32(9, 19, 31, 248);
            CanvasGroup group = panelRoot.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            Image stripe = UiImage("Accent Stripe", panelRoot.transform, Red,
                new Vector2(0f, 0f), new Vector2(0.025f, 1f), Vector2.zero, Vector2.zero);
            Text name = UiText("Pokemon Name", panelRoot.transform, "POKÉMON", 57, FontStyle.Bold, Paper,
                new Vector2(0.08f, 0.67f), new Vector2(0.82f, 0.93f), Vector2.zero, Vector2.zero, TextAnchor.MiddleLeft);
            name.resizeTextForBestFit = true;
            name.resizeTextMinSize = 35;
            name.resizeTextMaxSize = 57;
            Text type = UiText("Pokemon Type", panelRoot.transform, "TIPO", 24, FontStyle.Bold, Cyan,
                new Vector2(0.08f, 0.52f), new Vector2(0.84f, 0.69f), Vector2.zero, Vector2.zero, TextAnchor.MiddleLeft);
            Text description = UiText("Pokemon Description", panelRoot.transform, string.Empty, 30, FontStyle.Normal, new Color32(211, 223, 231, 255),
                new Vector2(0.08f, 0.10f), new Vector2(0.92f, 0.52f), Vector2.zero, Vector2.zero, TextAnchor.UpperLeft);
            description.horizontalOverflow = HorizontalWrapMode.Wrap;
            description.verticalOverflow = VerticalWrapMode.Truncate;

            Button close = UiButton("Close", panelRoot.transform, "×", new Color32(42, 57, 72, 255), Paper,
                new Vector2(0.84f, 0.72f), new Vector2(0.95f, 0.92f));
            PokedexPanelController controller = panelRoot.AddComponent<PokedexPanelController>();
            controller.Configure(group, name, type, description, stripe, close);
            return controller;
        }

        private static PokemonTargetController CreatePokemonTarget(
            string name,
            Vector3 position,
            string targetTexturePath,
            PokemonStageDefinition[] definitions,
            PokedexPanelController panel,
            int initialStageIndex = 0)
        {
            GameObject targetRoot = new GameObject(name);
            targetRoot.transform.position = position;
            AudioSource audioSource = targetRoot.AddComponent<AudioSource>();
            audioSource.volume = 0.8f;

            GameObject card = GameObject.CreatePrimitive(PrimitiveType.Cube);
            card.name = "Printable Image Target Preview";
            card.transform.SetParent(targetRoot.transform, false);
            card.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            card.transform.localScale = new Vector3(1.22f, 0.035f, 1.22f);
            card.GetComponent<Renderer>().sharedMaterial = CreateMaterial(
                name + " Target",
                Color.white,
                AssetDatabase.LoadAssetAtPath<Texture2D>(targetTexturePath));
            UnityEngine.Object.DestroyImmediate(card.GetComponent<Collider>());

            List<PokemonStage> stages = new List<PokemonStage>();
            for (int index = 0; index < definitions.Length; index++)
            {
                PokemonStageDefinition definition = definitions[index];
                GameObject stageObject = new GameObject(definition.displayName);
                stageObject.transform.SetParent(targetRoot.transform, false);
                stageObject.transform.localPosition = new Vector3(0f, 0.08f, 0f);
                stageObject.AddComponent<PokemonShowcaseMotion>();

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(definition.modelPath);
                if (prefab == null)
                {
                    throw new InvalidOperationException($"Missing model at {definition.modelPath}");
                }

                GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(prefab, stageObject.transform);
                model.name = definition.displayName + " Model";
                NormalizeModel(model, stageObject.transform.position, definition.displayName == "Greninja" ? 1.35f : 1.12f);

                BoxCollider collider = stageObject.AddComponent<BoxCollider>();
                collider.center = new Vector3(0f, 0.72f, 0f);
                collider.size = new Vector3(1.25f, 1.55f, 1.25f);
                stageObject.SetActive(index == initialStageIndex);
                stages.Add(new PokemonStage
                {
                    displayName = definition.displayName,
                    type = definition.type,
                    description = definition.description,
                    modelObject = stageObject,
                    accentColor = definition.accent,
                    cryPitch = definition.cryPitch
                });
            }

            PokemonTargetController controller = targetRoot.AddComponent<PokemonTargetController>();
            controller.Configure(stages.ToArray(), panel, card, true, initialStageIndex);
            return controller;
        }

        private static void NormalizeModel(GameObject model, Vector3 anchor, float desiredHeight)
        {
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            float scale = desiredHeight / Mathf.Max(0.001f, bounds.size.y);
            model.transform.localScale *= scale;
            bounds = renderers[0].bounds;
            for (int index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            Vector3 desiredBase = anchor + Vector3.up * 0.03f;
            Vector3 currentBase = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            model.transform.position += desiredBase - currentBase;
            model.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }

        private static void CreateSelectorButton(Transform parent, string label, Color color, PokemonTargetController target)
        {
            Button button = UiButton(label, parent, label, color, Color.white, Vector2.zero, Vector2.one);
            UnityEventTools.AddPersistentListener(button.onClick, target.SelectAsCurrent);
        }

        private static PokemonStageDefinition Stage(
            string displayName,
            string type,
            string description,
            string modelPath,
            Color accent,
            float cryPitch)
        {
            return new PokemonStageDefinition(displayName, type, description, modelPath, accent, cryPitch);
        }

        private static Material CreateMaterial(string name, Color color, Texture texture)
        {
            string folder = PokemonRoot + "/Generated";
            if (!AssetDatabase.IsValidFolder(folder))
            {
                AssetDatabase.CreateFolder(PokemonRoot, "Generated");
            }

            string path = folder + "/" + name.Replace(" ", string.Empty) + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard"));
                material.name = name;
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            material.mainTexture = texture;
            material.SetFloat("_Glossiness", texture == null ? 0.18f : 0.34f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ConfigureTargetTexture(string path)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.isReadable = true;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }

        private static void ConfigurePlayer()
        {
            PlayerSettings.companyName = "Juan Zúñiga Maluenda";
            PlayerSettings.productName = "Pokédex AR";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "cl.ucn.juanzuniga.pokedexar");
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.forceInternetPermission = false;
            PlayerSettings.statusBarHidden = true;
        }

        private static GameObject UiObject(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return gameObject;
        }

        private static Image UiImage(string name, Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject gameObject = UiObject(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            Image image = gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text UiText(
            string name,
            Transform parent,
            string value,
            int fontSize,
            FontStyle style,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax,
            TextAnchor alignment)
        {
            GameObject gameObject = UiObject(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            Text text = gameObject.AddComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static Button UiButton(string name, Transform parent, string label, Color background, Color foreground, Vector2 anchorMin, Vector2 anchorMax)
        {
            Image image = UiImage(name, parent, background, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            Button button = image.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = Color.Lerp(background, Color.white, 0.16f);
            colors.pressedColor = Color.Lerp(background, Color.black, 0.18f);
            button.colors = colors;
            Text text = UiText("Label", button.transform, label, 29, FontStyle.Bold, foreground,
                Vector2.zero, Vector2.one, new Vector2(12f, 8f), new Vector2(-12f, -8f), TextAnchor.MiddleCenter);
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 20;
            text.resizeTextMaxSize = 34;
            return button;
        }

        private readonly struct PokemonStageDefinition
        {
            public readonly string displayName;
            public readonly string type;
            public readonly string description;
            public readonly string modelPath;
            public readonly Color accent;
            public readonly float cryPitch;

            public PokemonStageDefinition(string displayName, string type, string description, string modelPath, Color accent, float cryPitch)
            {
                this.displayName = displayName;
                this.type = type;
                this.description = description;
                this.modelPath = modelPath;
                this.accent = accent;
                this.cryPitch = cryPitch;
            }
        }
    }
}
