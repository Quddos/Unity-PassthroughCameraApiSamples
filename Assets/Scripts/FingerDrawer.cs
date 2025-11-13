using UnityEngine;

public class FingerDrawer : MonoBehaviour
{
    public OVRHand rightHand;
    public OVRSkeleton rightHandSkeleton;
    public LineRenderer linePrefab;
    public DigitRecognition digitRecognition;
    public Camera captureCamera;
    public RenderTexture renderTexture;
    public float drawDistance = 0.02f; // Distance from board

    private LineRenderer currentLine;
    private Transform indexTip;

    void Start()
    {
        // Get index tip reference
        indexTip = rightHandSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform;
    }

    void Update()
    {
        if (indexTip == null) return;

        // Raycast from fingertip to plane
        Ray ray = new Ray(indexTip.position, indexTip.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 0.2f))
        {
            // Start drawing when pinching
            if (rightHand.GetFingerIsPinching(OVRHand.HandFinger.Index))
            {
                if (currentLine == null)
                {
                    currentLine = Instantiate(linePrefab);
                    currentLine.positionCount = 0;
                }

                currentLine.positionCount++;
                currentLine.SetPosition(currentLine.positionCount - 1, hit.point + hit.normal * drawDistance);
            }
            else if (currentLine != null)
            {
                // Finished drawing, analyze the stroke
                SaveAndRecognize();
                Destroy(currentLine.gameObject);
                currentLine = null;
            }
        }
    }

    void SaveAndRecognize()
    {
        // Render the board to a 28x28 texture for AI input
        RenderTexture.active = renderTexture;
        Texture2D texture = new Texture2D(28, 28, TextureFormat.RGB24, false);
        captureCamera.targetTexture = renderTexture;
        captureCamera.Render();
        texture.ReadPixels(new Rect(0, 0, 28, 28), 0, 0);
        texture.Apply();

        captureCamera.targetTexture = null;
        RenderTexture.active = null;

        int result = digitRecognition.RunAI(texture);
        Debug.Log("Recognized Digit: " + result);
    }
}
