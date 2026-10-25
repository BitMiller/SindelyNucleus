using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

Controls:

Left click = toss
Right click = jerk
Mouse wheel = zoom
S = Spawn neutron
D = Delete neutron
E = - constant push
W = - constant pull
C = Center - force affects towards the gravity center
M = Mark - select
N = select None
B = invert selection

0-9 = color neutron under mouse cursor / color all neutrons if no neutron is under mouse cursor

*/

public class SphereSpawner : MonoBehaviour
{
    [Header("Relations")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform gravityTarget;
    [SerializeField] private float gravityTargetForce = 0.4f;

    [Header("Interaction")]
    [SerializeField] private float impulseStrength = 5f;
    [SerializeField] private float maxTossStrength = 1f;
    [SerializeField] private ForceDirectionMode forceDirectionMode;
    [SerializeField] private float damping;
    [SerializeField] private float quakeStrength = 1f;
    [SerializeField] private float gravityForceMultiplier = 1f;


    [Header("Animation")]
    [SerializeField] private float initialScaleMultiplier = 0.05f;
    [SerializeField] private float finalScaleMultiplier = 1f;
    [SerializeField] private float initialMass = 0.05f;
    [SerializeField] private float finalMass = 1;
    [SerializeField] private float spawnAnimationDuration = 1;

    [Header("Prefab")]
    [SerializeField] private GameObject neutronPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private int neutronCount = 20;
    [SerializeField] private float spawnRadius = 4f;

    private List<GameObject> neutrons = new List<GameObject>();
    private GameObject pointedNeutron;
    private Rigidbody pointedRigidbody;
    private RaycastHit lastRaycastHit;

    private bool gameObjectMissing = false;

    private float currentTossStrength = 0;
    private GameObject currentlyTossed = null;

    private bool key_quake = false;
    private bool key_applyForcePush = false;
    private bool key_applyForcePull = false;
    private bool key_forceInvert = false;

    private bool addToSelection = true;

    private float distanceCutOff = 0.02f;

    private int neutronIdCounter = 0;

    private int historyCursor = 0;

    private enum ForceDirection {
        GravityCenter,
        HitNormal
    }

    private enum ForceType {
        Force,
        Impulse
    }

    private enum SelectionMode
    {
        Select,
        Deselect,
        Invert
    }

    private class HistoryState
    {
        
    }

    private List<HistoryState> history = new List<HistoryState>();

/*
> https://www.google.com/search?q=rgb+for+9+rainbow+colors&sca_esv=28393f27b5912977&sxsrf=APpeQnvY9Fp04AiK3N3PA4UE3-nuTUOcJA%3A1792274021416&ei=Ze7Tav7vGMmOxc8PvLHt4QI&biw=1536&bih=730&uact=5&oq=rgb+for+9+rainbow+colors&gs_lp=Egxnd3Mtd2l6LXNlcnAiGHJnYiBmb3IgOSByYWluYm93IGNvbG9yczIFEAAY7wUyCBAAGIAEGKIEMgUQABjvBTIFEAAY7wUyCBAAGIAEGKIESMxwUABYnW5wAXgBkAEAmAGUAaAB5xSqAQQ4LjE3uAEDyAEA-AEBmAIZoALLFMICBBAjGCfCAggQABiABBixA8ICDhAuGIAEGLEDGMcBGNEDwgIOEC4YgAQYigUYsQMYgwHCAgsQLhiABBixAxiDAcICChAAGIAEGIoFGEPCAg0QABiABBiKBRhDGLEDwgIFEAAYgATCAgUQLhiABMICCxAuGIAEGMcBGK8BwgIIEAAYgAQYywHCAgYQABgWGB7CAggQABgWGB4YCsICCRAAGIAEGA0YE8ICChAAGAgYHhgNGBOYAwCSBwQ2LjE5oAfRmQGyBwQ1LjE5uAfJFMIHBjAuMjIuM8gHPoAIAQ&sclient=gws-wiz-serp

> https://colorswall.com/palette/176875

HEX codes (sorted): #00C0FF, #00D267, #81E650, #8B48FE, #CA41FC, #FF46FB, #FF5400, #FF8E00, #FFD200

Palette 9 Rainbow Colors combines 9 color codes:
HEX: #ff5400, RGB: (255, 84, 0); HEX: #ff8e00, RGB: (255, 142, 0); HEX: #ffd200, RGB: (255, 210, 0)
HEX: #81e650, RGB: (129, 230, 80); HEX: #00d267, RGB: (0, 210, 103); HEX: #00c0ff, RGB: (0, 192, 255)
HEX: #8b48fe, RGB: (139, 72, 254); HEX: #ca41fc, RGB: (202, 65, 252); HEX: #ff46fb, RGB: (255, 70, 251)
Simplified version of palette colors
Tint of Orange Red, Tint of Dark Orange, Shade of Gold, Shade of Lawn Green, Shade of Lime Green, Tint of Deep Sky Blue, Tint of Blue Violet, Shade of Medium Orchid, Tint of Magenta

• Color 1 (Orange-Red): (255, 84, 0)
• Color 2 (Orange): (255, 142, 0)
• Color 3 (Shining Gold): (255, 210, 0)
• Color 4 (Light Green): (129, 230, 80)
• Color 5 (Dark Green): (0, 210, 103)
• Color 6 (Cyan / Blue): (0, 192, 255)
• Color 7 (Violet / Purple): (139, 72, 254)
• Color 8 (Magenta / Purple): (202, 65, 252)
• Color 9 (Bright Pink): (255, 70, 251)
*/

    private static float dimColors = 0.65f;
    private float smoothnessBase = 0.5f;
    private float smoothnessSelection = 0.9f;
    private float selectionEmissionMultiplier = 1f;

    private Dictionary<int, Color> colors = new ()
    {
        { 0, new Color(128f/255f*dimColors, 128f/255f*dimColors, 128f/255f*dimColors) },
        { 1, new Color(255f/255f*dimColors, 084f/255f*dimColors, 000f/255f*dimColors) },
        { 2, new Color(255f/255f*dimColors, 142f/255f*dimColors, 000f/255f*dimColors) },
        { 3, new Color(255f/255f*dimColors, 210f/255f*dimColors, 000f/255f*dimColors) },
        { 4, new Color(129f/255f*dimColors, 230f/255f*dimColors, 080f/255f*dimColors) },
        { 5, new Color(000f/255f*dimColors, 210f/255f*dimColors, 103f/255f*dimColors) },
        { 6, new Color(000f/255f*dimColors, 192f/255f*dimColors, 255f/255f*dimColors) },
        { 7, new Color(139f/255f*dimColors, 072f/255f*dimColors, 254f/255f*dimColors) },
        { 8, new Color(202f/255f*dimColors, 065f/255f*dimColors, 252f/255f*dimColors) },
        { 9, new Color(255f/255f*dimColors, 070f/255f*dimColors, 251f/255f*dimColors) },
    };

    private Dictionary<int, Color> selectionColors = new ()
    {
        { 0, new Color(128f/255f, 128f/255f, 128f/255f) },
        { 1, new Color(255f/255f, 084f/255f, 000f/255f) },
        { 2, new Color(255f/255f, 142f/255f, 000f/255f) },
        { 3, new Color(255f/255f, 210f/255f, 000f/255f) },
        { 4, new Color(129f/255f, 230f/255f, 080f/255f) },
        { 5, new Color(000f/255f, 210f/255f, 103f/255f) },
        { 6, new Color(000f/255f, 192f/255f, 255f/255f) },
        { 7, new Color(139f/255f, 072f/255f, 254f/255f) },
        { 8, new Color(202f/255f, 065f/255f, 252f/255f) },
        { 9, new Color(255f/255f, 070f/255f, 251f/255f) },
    };

    public enum ForceDirectionMode
    {
        RelativeToGravityTarget,
        SurfaceNormal
    }

/*==============================*/
/*==============================*/
/*==============================*/

    private void Start()
    {
        SpawnNeutrons();
    }

/*==============================*/
/*==============================*/
/*==============================*/

    void Update()
    {
        if (gameObjectMissing)
            return;

        RaycastNeutron();

/*==============================*/
/*       Create / Delete        */
/*==============================*/

        if (Input.GetKeyDown(KeyCode.S))
        {
            SpawnNeutronAtCursor();
        }

        key_quake = Input.GetKey(KeyCode.Q);

        if (Input.GetKey(KeyCode.D))
        {
            DestroyNeutronAtCursor();
        }

/*==============================*/
/*             Move             */
/*==============================*/

        key_forceInvert = Input.GetKey(KeyCode.C);

        if (Input.GetMouseButtonDown(0))
        {
            ApplyForce(true, key_forceInvert ? ForceDirection.GravityCenter : ForceDirection.HitNormal, ForceType.Impulse);
        }

        if (Input.GetMouseButtonDown(1))
        {
            ApplyForce(false, key_forceInvert ? ForceDirection.GravityCenter : ForceDirection.HitNormal, ForceType.Impulse);
        }

        key_applyForcePush = Input.GetKey(KeyCode.E);
        key_applyForcePull = Input.GetKey(KeyCode.W);

/*==============================*/
/*            Color             */
/*==============================*/

        HandleColors();

/*==============================*/
/*            Select            */
/*==============================*/

        HandleSelect();

    }

/*==============================*/
/*==============================*/
/*==============================*/

    void FixedUpdate()
    {
        if (gameObjectMissing)
            return;

        if (key_applyForcePush)
        {
            ApplyForce(true, key_forceInvert ? ForceDirection.GravityCenter : ForceDirection.HitNormal, ForceType.Force);
        }

        if (key_applyForcePull)
        {
            ApplyForce(false, key_forceInvert ? ForceDirection.GravityCenter : ForceDirection.HitNormal, ForceType.Force);
        }

        if (key_quake)
        {
            Quake();
        }


        //> Gravitate neutrons:
        /*foreach (GameObject neutron in neutrons)
        {
            Rigidbody rb = neutron.GetComponent<Rigidbody>();

            Vector3 direction = gravityTarget.position - neutron.transform.position;
            rb.AddForce(direction * gravityTargetForce, ForceMode.Force);
        }*/

        for (int i = 0; i < neutrons.Count-1; i++)
            for (int j = i; j < neutrons.Count; j++)
            {
                Vector3 direction = (neutrons[i].transform.position - neutrons[j].transform.position).normalized;
                float distance = Vector3.Distance(neutrons[i].transform.position, neutrons[j].transform.position);

                if (distance > distanceCutOff)
                {
                    Rigidbody rb1 = neutrons[i].GetComponent<Rigidbody>();
                    Rigidbody rb2 = neutrons[j].GetComponent<Rigidbody>();

                    rb1.AddForce(direction * (1/(distance*distance*distance)) * gravityForceMultiplier * rb1.mass * rb2.mass * -1, ForceMode.Force);
                    rb2.AddForce(direction * (1/(distance*distance*distance)) * gravityForceMultiplier * rb1.mass * rb2.mass, ForceMode.Force);
                }
            }

    }

/*==============================*/
/*==============================*/
/*==============================*/

    private void SpawnNeutrons()
    {
        if (neutronPrefab == null)
        {
            Debug.LogError("neutronPrefab is not assigned!");
            gameObjectMissing = true;
            return;
        }

        if (gravityTarget == null)
        {
            Debug.LogError("gravityTarget is not assigned!");
            gameObjectMissing = true;
            return;
        }

        if (mainCamera == null)
        {
            Debug.LogError("mainCamera is not assigned!");
            gameObjectMissing = true;
            return;
        }

        for (int i = 0; i < neutronCount; i++)
        {
            Vector3 spawnPosition =
                transform.position + Random.insideUnitSphere * spawnRadius;

            GameObject neutron = Instantiate(
                neutronPrefab,
                spawnPosition,
                Quaternion.identity
            );

            neutrons.Add(neutron);

            Renderer renderer = neutron.GetComponent<Renderer>();
            renderer.material.color = colors[0];
            renderer.material.SetFloat("_Smoothness", smoothnessBase);
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", Color.black);

            Neutron neutronComponent = neutron.GetComponent<Neutron>();
            neutronComponent.Id = neutronIdCounter;
            neutronIdCounter++;
        }
    } //> &SpawnNeutrons()

/*==============================*/
/*==============================*/
/*==============================*/

    void ApplyForce(bool push, ForceDirection forceDirection, ForceType forceType)
    {
        if (pointedNeutron == null)
            return;

        Vector3 direction;

        if (
            forceDirectionMode == ForceDirectionMode.SurfaceNormal && forceDirection == ForceDirection.HitNormal ||
            forceDirectionMode == ForceDirectionMode.RelativeToGravityTarget && forceDirection == ForceDirection.GravityCenter
            )
        {
            direction = lastRaycastHit.normal * (-1);
        }
        else
        {
            direction =
                (gravityTarget.position - pointedNeutron.transform.position).normalized;
        }

        if (!push)
        {
            direction = -direction;
        }

        if (forceType == ForceType.Impulse)
            pointedRigidbody.AddForce(direction * impulseStrength, ForceMode.Impulse);
        else
            pointedRigidbody.AddForce(direction * maxTossStrength, ForceMode.Force);
    }

/*==============================*/
/*==============================*/
/*==============================*/

    void ChangeDumping()
    {
        foreach (GameObject neutron in neutrons)
        {
            Rigidbody rigidbody = neutron.GetComponent<Rigidbody>();
            rigidbody.linearDamping = damping;
        }
    }

/*==============================*/
/*==============================*/
/*==============================*/

    private void SpawnNeutronAtCursor()
    {
        if (neutrons.Count == 0 || pointedNeutron != null)
        {
            Vector3 spawnPosition;

            if (neutrons.Count == 0)
                spawnPosition = gravityTarget.position;
            else
                spawnPosition = lastRaycastHit.point + lastRaycastHit.normal * 0.1f;

            GameObject neutron = Instantiate(
                neutronPrefab,
                spawnPosition,
                Quaternion.identity
            );

            Neutron neutronComponent = neutron.GetComponent<Neutron>();
            neutronComponent.IsBeingAnimated = true;
            neutronComponent.Id = neutronIdCounter;
            neutronIdCounter++;

            neutrons.Add(neutron);

            Renderer renderer = neutron.GetComponent<Renderer>();
            renderer.material.color = colors[0];
            renderer.material.SetFloat("_Smoothness", smoothnessBase);
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", Color.black);
            //Debug.Log($"R: {renderer.material.color.r} / G: {renderer.material.color.g} / B: {renderer.material.color.b}");

            StartCoroutine(GrowNeutron(neutron));
        }
    }

/*==============================*/
/*==============================*/
/*==============================*/

    private void DestroyNeutronAtCursor()
    {
        if (pointedNeutron == null)
        {
            return;
        }

        Neutron neutronComponent = pointedNeutron.GetComponent<Neutron>();

        if (neutronComponent == null)
        {
            Debug.Log("DestroyNeutronAtCursor() : Attach the Neutron.cs component to the Neutron prefab!");
            return;
        }

        if (neutronComponent.IsBeingAnimated)
        {
            return;
        }

        neutronComponent.IsBeingAnimated = true;

        StartCoroutine(ShrinkNeutron(pointedNeutron));
    }

/*==============================*/
/*==============================*/
/*==============================*/

    private IEnumerator GrowNeutron(GameObject neutron)
    {
        Rigidbody rigidbody = neutron.GetComponent<Rigidbody>();

        Vector3 v3InitialScale = Vector3.one * initialScaleMultiplier;
        Vector3 v3FinalScale = Vector3.one * finalScaleMultiplier;

        neutron.transform.localScale = v3InitialScale;
        rigidbody.mass = initialMass;

        float elapsed = 0f;

        while (elapsed < spawnAnimationDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / spawnAnimationDuration);

            neutron.transform.localScale =
                Vector3.Lerp(v3InitialScale, v3FinalScale, t);

            rigidbody.mass = Mathf.Lerp(initialMass, finalMass, t);

            yield return null;
        }

        Neutron neutronComponent = neutron.GetComponent<Neutron>();
        neutronComponent.IsBeingAnimated = false;

        neutron.transform.localScale = v3FinalScale;
        rigidbody.mass = finalMass;
    }

/*==============================*/
/*==============================*/
/*==============================*/

    private IEnumerator ShrinkNeutron(GameObject neutron)
    {
        Rigidbody rigidbody = neutron.GetComponent<Rigidbody>();

        Vector3 initialScale = neutron.transform.localScale;
        float initialMass = rigidbody.mass;

        Vector3 finalScale = initialScale * initialScaleMultiplier;

        // Stop it from interacting physically while disappearing.
        /*rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        Collider[] colliders = neutron.GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }*/

        float elapsed = 0f;

        while (elapsed < spawnAnimationDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / spawnAnimationDuration
            );

            t = t * t * (3f - 2f * t);

            neutron.transform.localScale =
                Vector3.Lerp(initialScale, finalScale, t);

            rigidbody.mass = Mathf.Lerp(initialMass, finalMass, t);

            yield return null;
        }

        neutrons.Remove(neutron);
        Destroy(neutron);
    }

/*==============================*/
/*==============================*/
/*==============================*/

    private void Quake()
    {
        foreach (GameObject neutron in neutrons)
        {
            Rigidbody rb = neutron.GetComponent<Rigidbody>();

            Vector3 randomDirection = Random.onUnitSphere;

            rb.AddForce(randomDirection * quakeStrength, ForceMode.Impulse);
        }
    }

/*==============================*/
/*==============================*/
/*==============================*/

    private void ChangeColor(GameObject neutron, int colorId)
    {
        Renderer renderer = neutron.GetComponent<Renderer>();
        Neutron neutronComponent = neutron.GetComponent<Neutron>();

        neutronComponent.ColorId = colorId;
        renderer.material.color = neutronComponent.Selected ? selectionColors[colorId] : colors[colorId];
        renderer.material.SetFloat("_Smoothness", neutronComponent.Selected ? smoothnessSelection : smoothnessBase);
        renderer.material.SetColor("_EmissionColor", neutronComponent.Selected ?
                                                    selectionColors[neutronComponent.ColorId] * selectionEmissionMultiplier :
                                                    Color.black);
    }

/*==============================*/
/*==============================*/
/*==============================*/

    private void ResetColors(int colorId)
    {
        foreach (GameObject neutron in neutrons)
        {
            ChangeColor(neutron, colorId);
        }
    }

/*==============================*/
/*==============================*/
/*==============================*/

    private void HandleColors()
    {
        int highestNumKeyDown = -1;
        int highestNumKey = -1;

        for (int i = 0; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                highestNumKeyDown = i;
            if (Input.GetKey(KeyCode.Alpha0 + i))
                highestNumKey = i;
        }

        if (highestNumKeyDown >= 0 || highestNumKey >= 0)
        {
            //Debug.Log($"highestNumKeyDown: {highestNumKeyDown} | highestNumKey: {highestNumKey}");

            if (pointedNeutron == null && highestNumKeyDown >= 0)
            {
                ResetColors(highestNumKeyDown);
            }
            else if (pointedNeutron != null)
            {
                if (highestNumKeyDown >= 0)
                    ChangeColor(pointedNeutron, highestNumKeyDown);
                else if (highestNumKey >= 0)
                    ChangeColor(pointedNeutron, highestNumKey);
            }
        }
    }

/*==============================*/
/*==============================*/
/*==============================*/

    private void RaycastNeutron()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit) && neutrons.Contains(hit.collider.gameObject))
        {
            pointedNeutron = hit.collider.gameObject;
            pointedRigidbody = pointedNeutron.GetComponent<Rigidbody>();
            lastRaycastHit = hit;
        }
        else
        {
            pointedNeutron = null;
            pointedRigidbody = null;
        }
    }

/*==============================*/
/*==============================*/
/*==============================*/

    private void HandleSelect()
    {
        if (Input.GetKeyDown(KeyCode.M) && pointedNeutron != null)
        {
            SelectNeutron(pointedNeutron, SelectionMode.Invert);
            Neutron neutronComponent = pointedNeutron.GetComponent<Neutron>();
            addToSelection = neutronComponent.Selected;
        }

        else if (Input.GetKeyDown(KeyCode.N))
        {
            foreach (GameObject neutron in neutrons)
            {
                SelectNeutron(neutron, SelectionMode.Deselect);
            }
        }

        else if (Input.GetKeyDown(KeyCode.B))
        {
            foreach (GameObject neutron in neutrons)
            {
                SelectNeutron(neutron, SelectionMode.Invert);
            }
        }

        else if (Input.GetKey(KeyCode.M) && pointedNeutron != null)
        {
            SelectNeutron(pointedNeutron, addToSelection ? SelectionMode.Select : SelectionMode.Deselect);
        }
    }

/*==============================*/
/*==============================*/
/*==============================*/

    private void SelectNeutron(GameObject neutron, SelectionMode selectionMode)
    {
        Neutron neutronComponent = neutron.GetComponent<Neutron>();
        Renderer renderer = neutron.GetComponent<Renderer>();

        if (
            selectionMode == SelectionMode.Select ||
            selectionMode == SelectionMode.Invert && !neutronComponent.Selected
            )
        {
            neutronComponent.Selected = true;
            renderer.material.color = selectionColors[neutronComponent.ColorId];
            renderer.material.SetFloat("_Smoothness", smoothnessSelection);
            renderer.material.SetColor("_EmissionColor", selectionColors[neutronComponent.ColorId] * selectionEmissionMultiplier);
        }

        else if (
            selectionMode == SelectionMode.Deselect ||
            selectionMode == SelectionMode.Invert && neutronComponent.Selected
            )
        {
            neutronComponent.Selected = false;
            renderer.material.color = colors[neutronComponent.ColorId];
            renderer.material.SetFloat("_Smoothness", smoothnessBase);
            renderer.material.SetColor("_EmissionColor", Color.black);
        }
    }

}