using UnityEngine;
using System.Linq;

[DisallowMultipleComponent]
public class FingerPen : MonoBehaviour
{
    [Header("References (assign these)")]
    public Transform fingertipTransform;    // <-- best: drag the index-tip transform from BuildingBlock hand here
    public Transform drawingBoard;          // <-- drag your DrawingBoard object here
    public LineRenderer linePrefab;         // <-- drag your LinePrefab prefab (not the instance)
    public Transform penTipVisual;          // <-- the yellow sphere (optional; used to show fingertip)
    [Header("Drawing settings")]
    public float writeDistance = 0.015f;    // how close to board to start drawing (meters)
    public int maxPointsPerLine = 2000;

    // runtime
    private LineRenderer currentLine;
    private int currentCount = 0;

    void Awake()
    {
        // If user didn't assign fingertip, try auto-find common names in scene
        if (fingertipTransform == null)
        {
            fingertipTransform = TryAutoFindFingertip();
            if (fingertipTransform != null)
                Debug.Log("[FingerPen] Autodetected fingertip: " + fingertipTransform.name);
            else
                Debug.LogWarning("[FingerPen] fingertipTransform not assigned and auto-search failed. Drag the index-tip transform from your Hand Tracking prefab into this field.");
        }

        // Pen tip visual: if none assigned, we'll create a small sphere
        if (penTipVisual == null)
        {
            var go = GameObject.Find("FingerPen_PenTipDebug");
            if (go == null)
            {
                GameObject s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                s.name = "FingerPen_PenTipDebug";
                s.transform.localScale = Vector3.one * 0.01f;
                Destroy(s.GetComponent<Collider>()); // no collider
                penTipVisual = s.transform;
            }
            else penTipVisual = go.transform;
        }

        // Hide penTipVisual if no fingertip available (it will be moved later)
        if (penTipVisual != null)
        {
            penTipVisual.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (fingertipTransform == null)
            return; // can't proceed without an index tip

        if (drawingBoard == null)
        {
            Debug.LogError("[FingerPen] drawingBoard not assigned!");
            return;
        }

        // show pen tip visual
        if (penTipVisual != null)
        {
            penTipVisual.gameObject.SetActive(true);
            penTipVisual.position = fingertipTransform.position;
            penTipVisual.rotation = fingertipTransform.rotation;
        }

        // compute distance to board along board normal
        Vector3 boardNormal = drawingBoard.forward; // IMPORTANT: board.forward must point TOWARD the player
        float signedDist = Vector3.Dot(boardNormal, fingertipTransform.position - drawingBoard.position);
        float dist = Mathf.Abs(signedDist);

        // when finger is close to board -> draw
        if (dist <= writeDistance)
        {
            DrawPoint(fingertipTransform.position);
        }
        else
        {
            EndLine();
        }
    }

    // Create or append a point to the current line
    void DrawPoint(Vector3 pos)
    {
        if (linePrefab == null)
        {
            Debug.LogError("[FingerPen] linePrefab not assigned.");
            return;
        }

        if (currentLine == null)
        {
            var go = Instantiate(linePrefab.gameObject);
            go.name = "DrawLine_" + Time.frameCount;
            currentLine = go.GetComponent<LineRenderer>();
            currentLine.positionCount = 0;
            currentCount = 0;
        }

        // limit points to avoid runaway memory
        if (currentCount >= maxPointsPerLine)
            return;

        currentCount++;
        currentLine.positionCount = currentCount;
        currentLine.SetPosition(currentCount - 1, pos);
    }

    void EndLine()
    {
        currentLine = null;
        currentCount = 0;
    }

    // Try to find a likely fingertip Transform in the scene (building-block / meta / oculus naming variations)
    Transform TryAutoFindFingertip()
    {
        // common name parts to search for
        string[] candidates = new string[] { "index", "indextip", "index_tip", "IndexTip", "Index_Tip", "indexTip" };

        // search all transforms in the scene (not ideal for huge scenes but fine here)
        var all = Resources.FindObjectsOfTypeAll<Transform>();

        // prefer objects that contain both 'right' and index (we're using right hand)
        foreach (var t in all)
        {
            string n = t.name.ToLowerInvariant();
            if (n.Contains("right") && candidates.Any(x => n.Contains(x)))
                return t;
        }

        // fallback: first transform that matches any index name
        foreach (var t in all)
        {
            string n = t.name.ToLowerInvariant();
            if (candidates.Any(x => n.Contains(x)))
                return t;
        }

        // nothing found
        return null;
    }
}
