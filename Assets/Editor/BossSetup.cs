using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BossSetup
{
    const string Dir = "Assets/Sprites/Boss/";
    const string EnemyPath = "Assets/Prefabs/Enemy.prefab";
    const string BossPath = "Assets/Prefabs/Boss.prefab";
    const string ArrowPath = "Assets/Prefabs/Arrow.prefab";
    const string CoinPath = "Assets/Prefabs/Coin.prefab";
    const string WallPath = "Assets/Prefabs/WallSegment.prefab";

    [MenuItem("Tools/Boss ve Denge: Kur")]
    static void Setup()
    {
        string report = "";

        string[] files = { "Cyclop_Walk", "Cyclop_Idle", "Cyclop_Hit", "Cyclop_Face" };
        foreach (var f in files)
        {
            string path = Dir + f + ".png";
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null)
            {
                EditorUtility.DisplayDialog("Boss", path + " bulunamadı. Assets/Sprites/Boss klasörünü kontrol et.", "Tamam");
                return;
            }
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = 16f;
            imp.filterMode = FilterMode.Point;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.mipmapEnabled = false;
            imp.alphaIsTransparency = true;
            imp.SaveAndReimport();
        }

        var walk = AssetDatabase.LoadAssetAtPath<Texture2D>(Dir + "Cyclop_Walk.png");
        var idle = AssetDatabase.LoadAssetAtPath<Texture2D>(Dir + "Cyclop_Idle.png");
        var hit = AssetDatabase.LoadAssetAtPath<Texture2D>(Dir + "Cyclop_Hit.png");
        var face = AssetDatabase.LoadAssetAtPath<Sprite>(Dir + "Cyclop_Face.png");

        var enemyAsset = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPath);
        if (enemyAsset == null)
        {
            EditorUtility.DisplayDialog("Boss", EnemyPath + " bulunamadı.", "Tamam");
            return;
        }
        var coinPrefab = enemyAsset.GetComponent<CoinDrop>() != null ? enemyAsset.GetComponent<CoinDrop>().coinPrefab : null;

        var root = PrefabUtility.LoadPrefabContents(EnemyPath);
        var eAnim = root.GetComponent<DirectionalSheetAnimator>();
        var eEnemy = root.GetComponent<Enemy>();
        eEnemy.moveSpeed = 1.8f;
        eEnemy.attackDamage = 8f;
        eEnemy.attackCooldown = 1.2f;
        root.GetComponent<Health>().maxHealth = 50f;
        var eDrop = root.GetComponent<CoinDrop>();
        if (eDrop != null)
        {
            eDrop.minCoins = 1;
            eDrop.maxCoins = 3;
        }
        PrefabUtility.SaveAsPrefabAsset(root, EnemyPath);
        PrefabUtility.UnloadPrefabContents(root);
        report += "Ayı: can 50, hasar 8, bekleme 1.2, hız 1.8.\n";

        AssetDatabase.DeleteAsset(BossPath);
        AssetDatabase.CopyAsset(EnemyPath, BossPath);
        var boss = PrefabUtility.LoadPrefabContents(BossPath);
        boss.name = "Boss";
        boss.transform.localScale = Vector3.one;
        var bAnim = boss.GetComponent<DirectionalSheetAnimator>();
        if (bAnim != null) Object.DestroyImmediate(bAnim);

        foreach (var c in boss.GetComponents<Collider2D>()) Object.DestroyImmediate(c);
        var col = boss.AddComponent<CircleCollider2D>();
        col.radius = 0.9f;

        var sr = boss.GetComponent<SpriteRenderer>();
        sr.sprite = SheetUtil.Slice(idle, 50, 50, 16f)[0];

        var bh = boss.GetComponent<Health>();
        bh.maxHealth = 600f;

        var be = boss.GetComponent<Enemy>();
        be.moveSpeed = 1.4f;
        be.detectRange = 5f;
        be.attackRange = 1.6f;
        be.attackDamage = 25f;
        be.attackCooldown = 1.5f;
        be.wallReach = 1.4f;
        be.separationRadius = 1.6f;
        be.slotSpread = 0.5f;

        var bd = boss.GetComponent<CoinDrop>();
        if (bd == null) bd = boss.AddComponent<CoinDrop>();
        bd.coinPrefab = coinPrefab;
        bd.minCoins = 25;
        bd.maxCoins = 30;

        var bar = boss.GetComponent<HealthBar>();
        if (bar == null) bar = boss.AddComponent<HealthBar>();
        bar.width = 2.6f;
        bar.height = 0.2f;
        bar.offset = new Vector2(0f, 2f);

        var ba = boss.GetComponent<BossAnimator>();
        if (ba == null) ba = boss.AddComponent<BossAnimator>();
        ba.walkSheet = walk;
        ba.idleSheet = idle;
        ba.hitSheet = hit;
        ba.cell = 50;

        PrefabUtility.SaveAsPrefabAsset(boss, BossPath);
        PrefabUtility.UnloadPrefabContents(boss);
        report += "Boss prefab'ı hazır: can 600, hasar 25, bekleme 1.5, hız 1.4.\n";

        var arrow = PrefabUtility.LoadPrefabContents(ArrowPath);
        var ac = arrow.GetComponent<Arrow>();
        ac.damage = 25f;
        ac.speed = 12f;
        PrefabUtility.SaveAsPrefabAsset(arrow, ArrowPath);
        PrefabUtility.UnloadPrefabContents(arrow);
        report += "Ok hasarı 25.\n";

        var coin = PrefabUtility.LoadPrefabContents(CoinPath);
        if (coin.transform.localScale.x < 1.5f)
        {
            coin.transform.localScale = coin.transform.localScale * 2f;
            report += "Altın 2 kat büyütüldü.\n";
        }
        PrefabUtility.SaveAsPrefabAsset(coin, CoinPath);
        PrefabUtility.UnloadPrefabContents(coin);

        var wall = PrefabUtility.LoadPrefabContents(WallPath);
        var wh = wall.GetComponentInChildren<Health>();
        if (wh != null) wh.maxHealth = 100f;
        PrefabUtility.SaveAsPrefabAsset(wall, WallPath);
        PrefabUtility.UnloadPrefabContents(wall);
        report += "Duvar canı 100.\n";

        foreach (var w in Object.FindObjectsByType<WallSegment>(FindObjectsSortMode.None))
        {
            var h = w.GetComponent<Health>();
            if (h == null) continue;
            Undo.RecordObject(h, "Duvar can");
            h.maxHealth = 100f;
            EditorUtility.SetDirty(h);
        }

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var ph = player.GetComponent<Health>();
            if (ph != null)
            {
                Undo.RecordObject(ph, "Oyuncu can");
                ph.maxHealth = 100f;
                EditorUtility.SetDirty(ph);
                report += "Oyuncu canı 100.\n";
            }
        }

        var spawner = Object.FindFirstObjectByType<WaveSpawner>();
        if (spawner != null)
        {
            Undo.RecordObject(spawner, "Spawner");
            spawner.enemyCount = 8;
            spawner.countIncrease = 3;
            spawner.spawnInterval = 1.5f;
            spawner.breakTime = 25f;
            spawner.bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BossPath).GetComponent<Enemy>();
            spawner.bossAfterWave = 2;
            spawner.bossEscorts = 6;
            spawner.bossFace = face;
            spawner.bossName = "Dead Eye";

            var intro = Object.FindFirstObjectByType<IntroSequence>();
            if (intro != null)
            {
                spawner.dialogue = intro.dialogue;
                spawner.playerFace = intro.playerFace;
                spawner.playerName = intro.playerName;
            }

            if (spawner.target != null)
            {
                var house = spawner.target.GetComponent<Health>();
                if (house != null)
                {
                    Undo.RecordObject(house, "Ev can");
                    house.maxHealth = 500f;
                    EditorUtility.SetDirty(house);
                    report += "Ev canı 500.\n";
                }
            }

            EditorUtility.SetDirty(spawner);
            EditorSceneManager.MarkSceneDirty(spawner.gameObject.scene);
            report += "Dalgalar: 8 ayı, her dalga +3; 3. dalga boss.\n";
        }
        else report += "WaveSpawner sahnede bulunamadı, sahne açık mı?\n";

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Boss ve Denge", report + "\nSahneyi kaydet (Ctrl+S).", "Tamam");
    }
}
