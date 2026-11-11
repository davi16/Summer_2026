using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GrenadeProjectile : MonoBehaviour
{
    public SquadMemberAI.Team throwerTeam;
    public string throwerName = "Squad Grenade";
    public float fuseTime = 1.5f;
    public float explosionRadius = 6.5f;
    public int maxDamage = 60;
    public GameObject customExplosionPrefab;
    public AudioClip explosionSound;

    private Light blinkLight;
    private bool hasExploded = false;

    // פונקציה סטטית שיוצרת וזורקת רימון פיזי בקשת לעבר המטרה
    public static void LaunchPhysicalGrenade(
        Vector3 startPos,
        Vector3 targetPos,
        SquadMemberAI.Team team,
        string senderName,
        GameObject explosionEffectPrefab = null,
        AudioClip explosionClip = null)
    {
        // יצירת גוף הרימון הפיזי
        GameObject grenadeObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        grenadeObj.name = "PhysicalGrenade";
        grenadeObj.transform.position = startPos;
        grenadeObj.transform.localScale = new Vector3(0.22f, 0.26f, 0.22f);

        // צבע ירוק-זית צבאי כהה לרימון
        Renderer rend = grenadeObj.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader == null || mat.shader.name == "Hidden/InternalErrorShader")
            mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.15f, 0.25f, 0.12f, 1f);
        rend.sharedMaterial = mat;

        // חומר פיזיקלי קפיצי כדי שהרימון יתגלגל ויקפוץ על הרצפה
        SphereCollider col = grenadeObj.GetComponent<SphereCollider>();
        PhysicsMaterial bouncyMat = new PhysicsMaterial("GrenadeBounce");
        bouncyMat.bounciness = 0.45f;
        bouncyMat.dynamicFriction = 0.5f;
        col.material = bouncyMat;

        Rigidbody rb = grenadeObj.AddComponent<Rigidbody>();
        rb.mass = 0.8f;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // חישוב מסלול קשתי בליסטי אל המטרה
        Vector3 toTarget = targetPos - startPos;
        Vector3 toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);
        float flightTime = Mathf.Clamp(toTargetXZ.magnitude / 10f, 0.6f, 1.3f);

        Vector3 velocityXZ = toTargetXZ / flightTime;
        float velocityY = (toTarget.y / flightTime) + (0.5f * Mathf.Abs(Physics.gravity.y) * flightTime);
        rb.linearVelocity = velocityXZ + (Vector3.up * velocityY);
        rb.angularVelocity = Random.insideUnitSphere * 12f;

        // נורית אזהרה אדומה על הרימון
        Light warnLight = grenadeObj.AddComponent<Light>();
        warnLight.type = LightType.Point;
        warnLight.color = Color.red;
        warnLight.range = 2.5f;
        warnLight.intensity = 2f;

        GrenadeProjectile proj = grenadeObj.AddComponent<GrenadeProjectile>();
        proj.throwerTeam = team;
        proj.throwerName = senderName;
        proj.customExplosionPrefab = explosionEffectPrefab;
        proj.explosionSound = explosionClip;
        proj.blinkLight = warnLight;
    }

    void Start()
    {
        StartCoroutine(FuseAndExplodeRoutine());
    }

    private IEnumerator FuseAndExplodeRoutine()
    {
        float elapsed = 0f;
        while (elapsed < fuseTime)
        {
            if (blinkLight != null) blinkLight.enabled = !blinkLight.enabled;
            yield return new WaitForSeconds(0.15f);
            elapsed += 0.15f;
        }

        Explode();
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        Vector3 pos = transform.position;

        // 1. אפקט פיצוץ ויזואלי
        if (customExplosionPrefab != null)
        {
            GameObject fx = Instantiate(customExplosionPrefab, pos, Quaternion.identity);
            Destroy(fx, 3f);
        }
        else
        {
            CreateFallbackExplosionVisual(pos);
        }

        // 2. סאונד פיצוץ תלת-ממדי
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, pos, 1.0f);
        }

        // 3. פגיעה רדיאלית בכל דמות בטווח הפיצוץ
        // פגיעה רדיאלית באויבים בלבד - חסינות מלאה לשחקן ולחברי הצוות שלו (סעיף 6)!
        Collider[] hitColliders = Physics.OverlapSphere(pos, explosionRadius);
        HashSet<HealthSystem> damagedTargets = new HashSet<HealthSystem>();

        foreach (Collider hit in hitColliders)
        {
            HealthSystem hp = hit.GetComponentInParent<HealthSystem>();
            if (hp != null && hp.currentHP > 0 && !damagedTargets.Contains(hp))
            {
                // 1. הרימון לעולם לא פוגע בשחקן עצמו!
                if (hp.isPlayer) continue;

                // 2. הרימון לעולם לא פוגע בחברי הצוות של השחקן או בקבוצה שזרקה אותו!
                SquadMemberAI unitAI = hp.GetComponentInParent<SquadMemberAI>();
                if (unitAI != null && (unitAI.isPlayerTeammate || unitAI.myTeam == throwerTeam))
                {
                    continue;
                }

                damagedTargets.Add(hp);
                float dist = Vector3.Distance(pos, hp.transform.position);
                float factor = Mathf.Clamp01(1f - (dist / explosionRadius));
                int dmg = Mathf.RoundToInt(Mathf.Lerp(30f, maxDamage, factor));
                hp.TakeDamage(dmg, throwerName);
            }
        }

        Destroy(gameObject);
    }

    private void CreateFallbackExplosionVisual(Vector3 pos)
    {
        GameObject blast = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(blast.GetComponent<Collider>());
        blast.transform.position = pos;
        blast.transform.localScale = Vector3.one * 3.5f;

        Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        if (mat.shader == null || mat.shader.name == "Hidden/InternalErrorShader")
            mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(1f, 0.55f, 0.05f, 0.9f);
        blast.GetComponent<Renderer>().sharedMaterial = mat;

        Light flash = blast.AddComponent<Light>();
        flash.type = LightType.Point;
        flash.color = new Color(1f, 0.6f, 0.1f);
        flash.range = 10f;
        flash.intensity = 6f;

        Destroy(blast, 0.25f);
    }
}