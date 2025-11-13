// using UnityEngine;
// using UnityEngine.XR.Hands;
// using UnityEngine.XR.Management;

// public class FingerDrawer : MonoBehaviour
// {
//     public Renderer drawSurface;          // Assign your Cube here
//     public Color drawColor = Color.black; // Ink color
//     public float brushSize = 5f;          // Brush thickness (pixels)
//     public XRHandSubsystem handSubsystem; // Auto-detected

//     private Texture2D drawTexture;
//     private Material surfaceMaterial;
//     private int texWidth = 512, texHeight = 512;

//     void Start()
//     {
//         // Create a blank white texture
//         drawTexture = new Texture2D(texWidth, texHeight, TextureFormat.RGB24, false);
//         Color[] fill = new Color[texWidth * texHeight];
//         for (int i = 0; i < fill.Length; i++) fill[i] = Color.white;
//         drawTexture.SetPixels(fill);
//         drawTexture.Apply();

//         surfaceMaterial = drawSurface.material;
//         surfaceMaterial.mainTexture = drawTexture;
//     }

//     void Update()
//     {
//         // Ensure we have a valid hand subsystem
//         if (handSubsystem == null)
//         {
//             handSubsystem = XRGeneralSettings.Instance?
//                 .Manager?
//                 .activeLoader?
//                 .GetLoadedSubsystem<XRHandSubsystem>();
//             if (handSubsystem == null) return;
//         }

//         // Use the right hand (change to leftHand if preferred)
//         XRHand hand = handSubsystem.rightHand;
//         if (!hand.isTracked) return;

//         // Get fingertip pose
//         if (hand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out Pose tipPose))
//         {
//             RaycastHit hit;
//             if (Physics.Raycast(tipPose.position, tipPose.forward, out hit, 0.1f))
//             {
//                 if (hit.collider != null && hit.collider.gameObject == drawSurface.gameObject)
//                 {
//                     Vector2 pixelUV = hit.textureCoord;
//                     pixelUV.x *= texWidth;
//                     pixelUV.y *= texHeight;

//                     DrawAt((int)pixelUV.x, (int)pixelUV.y);
//                 }
//             }
//         }
//     }

//     void DrawAt(int x, int y)
//     {
//         for (int i = -Mathf.RoundToInt(brushSize / 2); i < brushSize / 2; i++)
//         {
//             for (int j = -Mathf.RoundToInt(brushSize / 2); j < brushSize / 2; j++)
//             {
//                 int px = Mathf.Clamp(x + i, 0, texWidth - 1);
//                 int py = Mathf.Clamp(y + j, 0, texHeight - 1);
//                 drawTexture.SetPixel(px, py, drawColor);
//             }
//         }
//         drawTexture.Apply();
//     }
// }
