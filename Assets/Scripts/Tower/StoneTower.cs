using System.Collections;
using UnityEngine;

public class StoneTower : Tower
{
    [Header("Stone Tower Settings")]
    [Tooltip("Mechanism child that will animate (raise) before shooting")]
    [SerializeField] private Transform mechanismTransform;
    [SerializeField] private Animator mechanismAnimator;
    [Tooltip("Animator trigger to start the raise animation")]
    [SerializeField] private string raiseTrigger = "Raise";

    [Header("Projectile / Explosion")]
    [Tooltip("Explosion radius for projectiles")]
    public float explosionRadius = 2f;

    [Header("Audio")]
    [SerializeField] private bool playShootSound = true;

    private bool _isPlayingAnimation = false;

    protected override void Start()
    {
        base.Start();
        _shootTimer = Data.shootInterval;

        // try to find mechanism animator/transform if not assigned
        if (mechanismTransform == null)
        {
            Transform found = transform.Find("Mechanism");
            if (found != null) mechanismTransform = found;
        }
        if (mechanismAnimator == null && mechanismTransform != null)
        {
            mechanismAnimator = mechanismTransform.GetComponent<Animator>();
        }
    }

    protected override void Update()
    {
        _enemiesInRange.RemoveAll(e => e == null);

        if (_enemiesInRange.Count == 0 || _isPlayingAnimation) return;

        _shootTimer -= Time.deltaTime;

        if (_shootTimer <= 0f)
        {
            _shootTimer = Data.shootInterval;
            StartShootingSequence();
        }
    }

    private void StartShootingSequence()
    {
        if (_isPlayingAnimation) return;

        _isPlayingAnimation = true;

        if (mechanismAnimator != null)
        {
            mechanismAnimator.SetTrigger(raiseTrigger);
        }
        else
        {
            StartCoroutine(FallbackMechanismMotion());
        }
    }

    private IEnumerator FallbackMechanismMotion()
    {
        if (mechanismTransform == null)
        {
            SpawnFourProjectiles();
            _isPlayingAnimation = false;
            yield break;
        }

        Vector3 start = mechanismTransform.localPosition;
        Vector3 up = start + Vector3.up * 0.5f;
        float t = 0f;
        float dur = 0.18f;

        while (t < dur)
        {
            mechanismTransform.localPosition = Vector3.Lerp(start, up, t / dur);
            t += Time.deltaTime;
            yield return null;
        }
        mechanismTransform.localPosition = up;

        yield return new WaitForSeconds(0.05f);

        SpawnFourProjectiles();

        t = 0f;
        while (t < dur)
        {
            mechanismTransform.localPosition = Vector3.Lerp(up, start, t / dur);
            t += Time.deltaTime;
            yield return null;
        }
        mechanismTransform.localPosition = start;

        _isPlayingAnimation = false;
    }

    public void OnMechanismRaised()
    {
        // Play stone shoot sound when mechanism is raised
        if (playShootSound && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayStoneShoot();
        }

        SpawnFourProjectiles();
    }

    public void OnMechanismAnimationComplete()
    {
        _isPlayingAnimation = false;
    }

    private void SpawnFourProjectiles()
    {
        Vector3 spawnOrigin = mechanismTransform != null ? mechanismTransform.position : transform.position;

        Vector2[] dirs = new Vector2[] { Vector2.right, Vector2.up, Vector2.left, Vector2.down };

        for (int i = 0; i < dirs.Length; i++)
        {
            GameObject proj = projectilePooler.GetProjectile();
            if (proj == null)
            {
                Debug.LogError("StoneTower: Could not get projectile from pool!");
                continue;
            }

            // Transform'u ayarla
            proj.transform.position = spawnOrigin;
            proj.transform.rotation = Quaternion.identity;

            // ExplosiveProjectile component'ini al
            ExplosiveProjectile expl = proj.GetComponent<ExplosiveProjectile>();
            if (expl != null)
            {
                // Özellikleri ayarla
                expl.explosionRadius = explosionRadius;
                expl.explosionDamage = Data.damage;

                // AKTÝF ET
                proj.SetActive(true);

                // Initialize et - bu artýk animator'ý doðru þekilde resetleyecek
                expl.Initialize(Data, dirs[i], Data.projectileDuration);

                Debug.Log($"StoneTower spawned projectile {i}, active: {proj.activeInHierarchy}");
            }
            else
            {
                Debug.LogError("StoneTower: ExplosiveProjectile component not found!");
                projectilePooler.ReturnProjectile(proj);
            }
        }
    }
}