using System.Collections;
using UnityEngine;

public class ParabolicProjectile : ExplosiveProjectile
{
    public float height = 2f;
    public float speed = 5f;

    private Vector3 startPoint;
    private Vector3 targetPoint;
    private float travelTime;
    private float elapsedTime = 0f;

    public void Launch(Vector3 start, Vector3 target, float projectileSpeed, float arcHeight)
    {
        startPoint = start;
        targetPoint = target;
        speed = projectileSpeed;
        height = arcHeight;
        travelTime = Vector3.Distance(start, target) / speed;
        transform.position = startPoint;
        _hasExploded = false;
        StartCoroutine(MoveParabolic());
    }

    private IEnumerator MoveParabolic()
    {
        while (elapsedTime < travelTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / travelTime;
            Vector3 position = Vector3.Lerp(startPoint, targetPoint, t);
            position.y += height * Mathf.Sin(Mathf.PI * t); // Parabolik hareket
            transform.position = position;
            yield return null;
        }

        Explode();
    }
}
