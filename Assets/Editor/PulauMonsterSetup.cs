using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Menu bantu P4 (Pulau Monster). Urutan pakai:
//   1. Pulau Monster > 1. Buat 12 MonsterData + Database
//   2. Buka Scn_World, lalu Pulau Monster > 2. Setup Scn_World
//   3. Pulau Monster > 3. Buat Scn_Bootstrap + Scn_Battle + Build Settings   (lalu Play)
public static class PulauMonsterSetup
{
    private const string DataDir = "Assets/Data/Monsters";
    private const string DbPath = "Assets/Data/MonsterDatabase.asset";
    private const string PrefabDir = "Assets/Prefabs";
    private const string WildPrefabPath = "Assets/Prefabs/Pfb_WildMonster.prefab";

    // ------------------------------------------------------------------
    //  Data: sama persis dengan MonsterDatabase.csv
    // ------------------------------------------------------------------
    private struct Row
    {
        public string id, name, evolvesTo;
        public Element element;
        public int stage, hp, atk, def, spd, evolveLevel;
    }

    private static Row R(string id, string name, Element e, int stage,
                         int hp, int atk, int def, int spd, string evo, int evoLevel)
    {
        return new Row
        {
            id = id, name = name, element = e, stage = stage,
            hp = hp, atk = atk, def = def, spd = spd,
            evolvesTo = evo, evolveLevel = evoLevel
        };
    }

    private static readonly Row[] Rows =
    {
        R("MON_001", "Pyrit",    Element.Pyro,   1,  40, 14,  8, 10, "Pyrago",   5),
        R("MON_002", "Pyrago",   Element.Pyro,   2,  72, 25, 14, 18, "Infernox", 9),
        R("MON_003", "Infernox", Element.Pyro,   3, 120, 42, 24, 30, null,       0),
        R("MON_004", "Blubi",    Element.Aqua,   1,  48, 10, 12,  8, "Finnox",   5),
        R("MON_005", "Finnox",   Element.Aqua,   2,  86, 18, 21, 14, "Tidalord", 9),
        R("MON_006", "Tidalord", Element.Aqua,   3, 144, 30, 36, 24, null,       0),
        R("MON_007", "Sprig",    Element.Sprout, 1,  52,  9, 11,  7, "Bloomlet", 5),
        R("MON_008", "Bloomlet", Element.Sprout, 2,  93, 16, 19, 12, "Florazor", 9),
        R("MON_009", "Florazor", Element.Sprout, 3, 156, 27, 33, 21, null,       0),
        R("MON_010", "Zapi",     Element.Volt,   1,  38, 12,  7, 14, "Voltcat",  5),
        R("MON_011", "Voltcat",  Element.Volt,   2,  68, 21, 12, 25, "Raijugar", 9),
        R("MON_012", "Raijugar", Element.Volt,   3, 114, 36, 21, 42, null,       0),
    };

    private static string SkillFor(Element e)
    {
        switch (e)
        {
            case Element.Pyro:   return "Semburan Api";
            case Element.Aqua:   return "Tembakan Air";
            case Element.Sprout: return "Daun Tajam";
            default:             return "Setrum Kilat";
        }
    }

    // Penempatan versi demo (slide Praktikum 4, langkah 3-4)
    private struct Spawn { public string name; public float x, y; public int level; public bool boss; }

    private static Spawn S(string name, float x, float y, int level, bool boss = false)
    {
        return new Spawn { name = name, x = x, y = y, level = level, boss = boss };
    }

    private static readonly Spawn[] Spawns =
    {
        // rumput barat  X 5,5-9,5 ; Y 14,5-17,5 ; Lv 2-4
        S("Sprig",    6.0f, 16.0f, 2),
        S("Zapi",     8.5f, 15.0f, 3),
        S("Blubi",    7.0f, 17.0f, 3),
        S("Pyrit",    9.0f, 16.0f, 4),
        // rumput timur  X 26,5-31,5 ; Y 14,5-17,5 ; Lv 5-7
        S("Pyrago",  27.5f, 16.0f, 5),
        S("Finnox",  29.5f, 15.0f, 6),
        S("Voltcat", 28.0f, 17.0f, 6),
        S("Bloomlet",31.0f, 16.0f, 7),
        // Gua Petir ; Lv 9 ; diam
        S("Raijugar",30.5f, 31.5f, 9, true),
    };

    // ==================================================================
    //  MENU 1 : 12 MonsterData + MonsterDatabase
    // ==================================================================
    [MenuItem("Pulau Monster/1. Buat 12 MonsterData + Database")]
    public static void CreateMonsterAssets()
    {
        EnsureFolder(DataDir);
        Dictionary<string, Sprite> sprites = BuildSpriteIndex();

        // Tahap 1: buat semua aset dulu supaya Evolves To bisa menunjuk ke aset mana pun
        var assets = new Dictionary<string, MonsterData>();
        foreach (Row r in Rows)
        {
            string path = $"{DataDir}/Mon_{r.name}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<MonsterData>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<MonsterData>();
                AssetDatabase.CreateAsset(asset, path);
            }
            assets[r.name] = asset;
        }

        // Tahap 2: isi semua field
        var noFrames = new List<string>();
        foreach (Row r in Rows)
        {
            MonsterData asset = assets[r.name];
            var so = new SerializedObject(asset);
            so.FindProperty("id").stringValue = r.id;
            so.FindProperty("displayName").stringValue = r.name;
            so.FindProperty("element").enumValueIndex = (int)r.element;
            so.FindProperty("baseHP").intValue = r.hp;
            so.FindProperty("baseATK").intValue = r.atk;
            so.FindProperty("baseDEF").intValue = r.def;
            so.FindProperty("baseSPD").intValue = r.spd;
            so.FindProperty("skillName").stringValue = SkillFor(r.element);
            so.FindProperty("evolvesTo").objectReferenceValue =
                r.evolvesTo != null ? assets[r.evolvesTo] : null;
            so.FindProperty("evolveLevel").intValue = r.evolveLevel;

            List<Sprite> frames = FindFrames(sprites, $"{r.element}_{r.stage}_{r.name}_");
            if (frames.Count > 0)
            {
                SerializedProperty fp = so.FindProperty("frames");
                fp.arraySize = frames.Count;
                for (int i = 0; i < frames.Count; i++)
                    fp.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
            }
            else
            {
                noFrames.Add($"{r.name} (cari: {r.element}_{r.stage}_{r.name}_...)");
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
        }

        // Tahap 3: MonsterDatabase
        var db = AssetDatabase.LoadAssetAtPath<MonsterDatabase>(DbPath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<MonsterDatabase>();
            AssetDatabase.CreateAsset(db, DbPath);
        }
        var dso = new SerializedObject(db);
        SerializedProperty list = dso.FindProperty("monsters");
        list.arraySize = Rows.Length;
        for (int i = 0; i < Rows.Length; i++)
            list.GetArrayElementAtIndex(i).objectReferenceValue = assets[Rows[i].name];
        dso.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(db);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = db;
        EditorGUIUtility.PingObject(db);

        Debug.Log("[PulauMonster] 12 MonsterData + MonsterDatabase siap di Assets/Data " +
                  "(evolusi Lv 5 dan 9).");
        if (noFrames.Count > 0)
            Debug.LogWarning("[PulauMonster] Sprite frame tidak ketemu untuk: " +
                string.Join(", ", noFrames) +
                ". Seret sprite-nya manual ke field Frames di aset itu " +
                "(menu ini aman dijalankan ulang, Frames yang sudah kamu isi tidak dihapus).");
        else
            Debug.Log("[PulauMonster] Frame sprite terisi otomatis untuk semua monster.");
    }

    // ==================================================================
    //  MENU 2 : isi Scn_World (buka Scn_World dulu)
    // ==================================================================
    [MenuItem("Pulau Monster/2. Setup Scn_World (monster liar, installer, partner)")]
    public static void SetupWorld()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Pulau Monster", "Stop Play dulu.", "OK");
            return;
        }

        var db = AssetDatabase.LoadAssetAtPath<MonsterDatabase>(DbPath);
        if (db == null)
        {
            EditorUtility.DisplayDialog("Pulau Monster",
                "MonsterDatabase belum ada. Jalankan menu 1 dulu.", "OK");
            return;
        }

        var player = Object.FindFirstObjectByType<PlayerMovement>();
        if (player == null)
        {
            EditorUtility.DisplayDialog("Pulau Monster",
                "PlayerMovement tidak ditemukan di scene yang terbuka.\nBuka Scn_World dulu, lalu ulangi.", "OK");
            return;
        }
        var interactor = Object.FindFirstObjectByType<PlayerInteractor>();

        // Tag Player (dipakai pemeriksaan sentuhan monster)
        if (!player.CompareTag("Player")) player.gameObject.tag = "Player";

        int wildLayer = LayerMask.NameToLayer("Wild");
        if (wildLayer < 0)
            Debug.LogWarning("[PulauMonster] Layer 'Wild' belum ada, monster dibuat di layer Default.");
        else if (Physics2D.GetIgnoreLayerCollision(wildLayer, player.gameObject.layer))
            Debug.LogWarning("[PulauMonster] Layer Wild TIDAK bertabrakan dengan layer Player. " +
                "Edit > Project Settings > Physics 2D > Layer Collision Matrix: centang keduanya.");

        // Material sprite: samakan dengan sprite Player supaya pencahayaan sama
        Material spriteMaterial = null;
        var playerSr = player.GetComponentInChildren<SpriteRenderer>();
        if (playerSr != null) spriteMaterial = playerSr.sharedMaterial;

        GameObject prefab = EnsureWildPrefab(wildLayer, spriteMaterial);

        // Monster liar (dibuat ulang bila menu dijalankan lagi)
        GameObject oldParent = GameObject.Find("WildMonsters");
        if (oldParent != null) Object.DestroyImmediate(oldParent);
        var parent = new GameObject("WildMonsters");

        foreach (Spawn sp in Spawns)
        {
            var data = AssetDatabase.LoadAssetAtPath<MonsterData>($"{DataDir}/Mon_{sp.name}.asset");
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.transform);
            inst.name = "Wild_" + sp.name;
            inst.transform.position = new Vector3(sp.x, sp.y, 0f);

            var so = new SerializedObject(inst.GetComponent<WildMonster>());
            so.FindProperty("species").objectReferenceValue = data;
            so.FindProperty("level").intValue = sp.level;
            so.FindProperty("isBoss").boolValue = sp.boss;
            so.FindProperty("stationary").boolValue = sp.boss;
            so.ApplyModifiedPropertiesWithoutUndo();

            var sr = inst.GetComponent<SpriteRenderer>();
            if (data != null && data.Frames != null && data.Frames.Length > 0)
            {
                sr.sprite = data.Frames[0];
                PrefabUtility.RecordPrefabInstancePropertyModifications(sr);
            }

            if (sp.boss)
            {
                Transform alertT = inst.transform.Find("Alert");
                if (alertT != null)
                {
                    alertT.localPosition = new Vector3(0f, 2.2f, 0f);   // tanda ! boss y +2,2
                    PrefabUtility.RecordPrefabInstancePropertyModifications(alertT);
                }
            }
        }

        // Partner (SpriteRenderer + PartnerFollower)
        var follower = Object.FindFirstObjectByType<PartnerFollower>();
        if (follower == null)
        {
            var partnerGO = new GameObject("PartnerFollower");
            var psr = partnerGO.AddComponent<SpriteRenderer>();
            psr.sortingLayerName = "Characters";
            if (spriteMaterial != null) psr.sharedMaterial = spriteMaterial;
            follower = partnerGO.AddComponent<PartnerFollower>();
        }

        // WorldInstaller di ROOT scene
        var installer = Object.FindFirstObjectByType<WorldInstaller>();
        if (installer == null)
            installer = new GameObject("WorldInstaller").AddComponent<WorldInstaller>();
        installer.transform.SetParent(null);

        var iso = new SerializedObject(installer);
        iso.FindProperty("player").objectReferenceValue = player;
        iso.FindProperty("interactor").objectReferenceValue = interactor;
        iso.FindProperty("follower").objectReferenceValue = follower;
        iso.FindProperty("savePoint").objectReferenceValue = FindSavePoint();
        iso.FindProperty("debugStarter").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<MonsterData>($"{DataDir}/Mon_Pyrit.asset");
        iso.ApplyModifiedPropertiesWithoutUndo();

        if (interactor == null)
            Debug.LogWarning("[PulauMonster] PlayerInteractor tidak ditemukan. Seret manual ke field " +
                             "Interactor di WorldInstaller.");

        Scene scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[PulauMonster] Scn_World siap: {Spawns.Length} monster (8 liar + Raijugar), " +
                  "WorldInstaller, dan PartnerFollower.");
    }

    private static GameObject EnsureWildPrefab(int wildLayer, Material spriteMaterial)
    {
        EnsureFolder(PrefabDir);

        var go = new GameObject("Pfb_WildMonster");
        if (wildLayer >= 0) go.layer = wildLayer;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Characters";
        if (spriteMaterial != null) sr.sharedMaterial = spriteMaterial;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;   // Freeze Rotation Z
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;

        GameObject alert = CreateAlert(go.transform, 1.3f, spriteMaterial);

        var wm = go.AddComponent<WildMonster>();
        var so = new SerializedObject(wm);
        so.FindProperty("body").objectReferenceValue = sr;
        so.FindProperty("alert").objectReferenceValue = alert;
        so.ApplyModifiedPropertiesWithoutUndo();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, WildPrefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    // Tanda "!" : pakai sprite UI_Alert bila ada, kalau tidak pakai teks "!" merah.
    private static GameObject CreateAlert(Transform parent, float y, Material spriteMaterial)
    {
        var go = new GameObject("Alert");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(0f, y, 0f);

        Sprite alertSprite = FindSpriteStartingWith("UI_Alert");
        if (alertSprite != null)
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = alertSprite;
            sr.sortingLayerName = "Characters";
            sr.sortingOrder = 20;
            if (spriteMaterial != null) sr.sharedMaterial = spriteMaterial;
        }
        else
        {
            var tm = go.AddComponent<TextMesh>();
            tm.text = "!";
            tm.fontSize = 64;
            tm.characterSize = 0.1f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.fontStyle = FontStyle.Bold;
            tm.color = Color.red;
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null)
            {
                tm.font = font;
                var mr = go.GetComponent<MeshRenderer>();
                mr.sharedMaterial = font.material;
                mr.sortingLayerName = "Characters";
                mr.sortingOrder = 20;
            }
            Debug.LogWarning("[PulauMonster] Sprite UI_Alert tidak ketemu, tanda ! dibuat dari teks. " +
                             "Ganti dengan sprite UI_Alert di objek Alert pada Pfb_WildMonster kalau mau.");
        }

        go.SetActive(false);
        return go;
    }

    private static Transform FindSavePoint()
    {
        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            string n = t.name.ToLowerInvariant();
            if (n.Contains("simpan") || n.Contains("save")) return t;
        }
        return null;
    }

    // ==================================================================
    //  MENU 3 : Scn_Bootstrap + Scn_Battle + Build Settings
    // ==================================================================
    [MenuItem("Pulau Monster/3. Buat Scn_Bootstrap + Scn_Battle + Build Settings")]
    public static void CreateBootstrapAndBattle()
    {
        var db = AssetDatabase.LoadAssetAtPath<MonsterDatabase>(DbPath);
        if (db == null)
        {
            EditorUtility.DisplayDialog("Pulau Monster",
                "MonsterDatabase belum ada. Jalankan menu 1 dulu.", "OK");
            return;
        }

        string worldPath = FindScenePath("Scn_World");
        if (worldPath == null)
        {
            EditorUtility.DisplayDialog("Pulau Monster",
                "Scene bernama Scn_World tidak ditemukan di Assets.", "OK");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string dir = Path.GetDirectoryName(worldPath).Replace('\\', '/');
        string battlePath = dir + "/Scn_Battle.unity";
        string bootPath = dir + "/Scn_Bootstrap.unity";

        BuildBattleScene(battlePath);
        BuildBootstrapScene(bootPath, db);
        SetBuildScenes(bootPath, worldPath, battlePath);

        EditorSceneManager.OpenScene(bootPath);
        Debug.Log("[PulauMonster] Scn_Bootstrap & Scn_Battle dibuat dan masuk Build Settings. " +
                  "Scn_Bootstrap sudah terbuka: tekan Play.");
    }

    private static void BuildBattleScene(string path)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Material unlit = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");

        // Kamera battle: depth lebih tinggi dari kamera dunia, hapus layar dengan warna solid
        var camGO = new GameObject("Battle Camera");
        camGO.transform.position = new Vector3(0f, 0f, -10f);
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 4.5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.2f);
        cam.depth = 10f;

        // Latar BG_Battle_Grassland (Sprite Mode Single)
        Sprite bg = LoadBattleBackground();
        if (bg != null)
        {
            var bgGO = new GameObject("Background");
            var bgSr = bgGO.AddComponent<SpriteRenderer>();
            bgSr.sprite = bg;
            bgSr.sortingOrder = -10;
            bgSr.sharedMaterial = unlit;
            Vector2 size = bg.bounds.size;
            float scale = Mathf.Max(16f / size.x, 9f / size.y);
            bgGO.transform.localScale = new Vector3(scale, scale, 1f);
        }
        else
        {
            Debug.LogWarning("[PulauMonster] BG_Battle_Grassland tidak ketemu, latar battle polos.");
        }

        SpriteRenderer enemy = MakeBattleSprite("Enemy", new Vector3(3.5f, 1.5f, 0f), unlit);
        SpriteRenderer partner = MakeBattleSprite("Partner", new Vector3(-3.5f, -1.5f, 0f), unlit);

        var installer = new GameObject("BattleInstaller").AddComponent<BattleInstaller>();
        var so = new SerializedObject(installer);
        so.FindProperty("enemyRenderer").objectReferenceValue = enemy;
        so.FindProperty("partnerRenderer").objectReferenceValue = partner;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, path);
    }

    private static SpriteRenderer MakeBattleSprite(string name, Vector3 pos, Material material)
    {
        var go = new GameObject(name);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(4f, 4f, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sharedMaterial = material;
        sr.sortingOrder = 0;
        return sr;
    }

    private static Sprite LoadBattleBackground()
    {
        foreach (string guid in AssetDatabase.FindAssets("BG_Battle_Grassland t:Texture2D"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;   // langkah di slide: Single -> Apply
                importer.SaveAndReimport();
            }
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null) return s;
        }
        return null;
    }

    private static void BuildBootstrapScene(string path, MonsterDatabase db)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Fader: Canvas overlay + CanvasGroup + Image hitam
        var canvasGO = new GameObject("FadeCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        var group = canvasGO.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;

        var imageGO = new GameObject("Black");
        imageGO.transform.SetParent(canvasGO.transform, false);
        var image = imageGO.AddComponent<UnityEngine.UI.Image>();
        image.color = Color.black;
        image.raycastTarget = false;
        RectTransform rt = image.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var fader = canvasGO.AddComponent<ScreenFader>();
        var fso = new SerializedObject(fader);
        fso.FindProperty("group").objectReferenceValue = group;
        fso.ApplyModifiedPropertiesWithoutUndo();

        // GameBootstrap
        var boot = new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
        var so = new SerializedObject(boot);
        so.FindProperty("firstScene").stringValue = "Scn_World";
        so.FindProperty("monsterDatabase").objectReferenceValue = db;
        so.FindProperty("fader").objectReferenceValue = fader;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, path);
    }

    private static void SetBuildScenes(params string[] first)
    {
        var list = new List<EditorBuildSettingsScene>();
        foreach (string p in first) list.Add(new EditorBuildSettingsScene(p, true));
        foreach (EditorBuildSettingsScene s in EditorBuildSettings.scenes)
            if (!first.Contains(s.path)) list.Add(s);
        EditorBuildSettings.scenes = list.ToArray();
    }

    // ==================================================================
    //  Util
    // ==================================================================
    private static string FindScenePath(string sceneName)
    {
        foreach (string guid in AssetDatabase.FindAssets(sceneName + " t:Scene"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) == sceneName) return path;
        }
        return null;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }

    // Semua sprite di Assets (termasuk potongan sprite sheet), dicari berdasarkan nama.
    private static Dictionary<string, Sprite> BuildSpriteIndex()
    {
        var index = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            foreach (Sprite s in AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>())
                index[s.name] = s;
        }
        return index;
    }

    // Contoh prefix "Pyro_1_Pyrit_" -> Pyro_1_Pyrit_16x16_0 ... _3 (maks. 4, urut nomor frame)
    private static List<Sprite> FindFrames(Dictionary<string, Sprite> index, string prefix)
    {
        return index
            .Where(kv => kv.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .OrderBy(kv => TrailingNumber(kv.Key))
            .Select(kv => kv.Value)
            .Take(4)
            .ToList();
    }

    private static int TrailingNumber(string name)
    {
        int i = name.LastIndexOf('_');
        return i >= 0 && int.TryParse(name.Substring(i + 1), out int n) ? n : int.MaxValue;
    }

    private static Sprite FindSpriteStartingWith(string prefix)
    {
        Dictionary<string, Sprite> index = BuildSpriteIndex();
        if (index.TryGetValue(prefix, out Sprite exact)) return exact;
        return index
            .Where(kv => kv.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .OrderBy(kv => kv.Key)
            .Select(kv => kv.Value)
            .FirstOrDefault();
    }
}
