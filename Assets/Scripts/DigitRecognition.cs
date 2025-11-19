using UnityEngine;
using Unity.InferenceEngine;

public class DigitRecognition : MonoBehaviour
{
    [Header("AI Model Inputs")]
    public Texture2D testPicture;
    public ModelAsset modelAsset;

    [Header("Capture From Board")]
    public Camera captureCamera;
    public RenderTexture renderTexture;

    private Worker worker;
    private float[] results;

    void Start()
    {
        // Load the ONNX model
        Model model = ModelLoader.Load(modelAsset);

        FunctionalGraph graph = new FunctionalGraph();
        var inputs = graph.AddInputs(model);
        var outputs = Functional.Forward(model, inputs);

        Model runtimeModel = graph.Compile(outputs);
        worker = new Worker(runtimeModel, BackendType.GPUCompute);

        // Optional: Test with default image
        if (testPicture != null)
        {
            int predicted = RunAI(testPicture);
            Debug.Log("Sample Test Prediction: " + predicted);
        }
    }

    // Runs AI model on any 28x28 texture
    public int RunAI(Texture2D picture)
    {
        using Tensor<float> inputTensor = TextureConverter.ToTensor(picture, 28, 28, 1);
        worker.Schedule(inputTensor);

        Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;
        results = outputTensor.DownloadToArray();

        float[] probs = Softmax(results);

        int maxIndex = 0;
        float maxValue = probs[0];

        for (int i = 1; i < probs.Length; i++)
        {
            if (probs[i] > maxValue)
            {
                maxValue = probs[i];
                maxIndex = i;
            }
        }

        Debug.Log($"AI Prediction: {maxIndex} | Confidence: {(maxValue * 100f):F2}%");

        return maxIndex;
    }

    // Capture board → run AI
    public int RunAIFromBoard()
    {
        if (captureCamera == null || renderTexture == null)
        {
            Debug.LogError("DigitRecognition: Capture camera or render texture is missing!");
            return -1;
        }

        RenderTexture.active = renderTexture;

        Texture2D tex = new Texture2D(28, 28, TextureFormat.RGB24, false);

        captureCamera.targetTexture = renderTexture;
        captureCamera.Render();

        tex.ReadPixels(new Rect(0, 0, 28, 28), 0, 0);
        tex.Apply();

        captureCamera.targetTexture = null;
        RenderTexture.active = null;

        return RunAI(tex);
    }

    // Softmax function
    float[] Softmax(float[] logits)
    {
        float max = Mathf.Max(logits);
        float sumExp = 0;
        float[] exps = new float[logits.Length];

        for (int i = 0; i < logits.Length; i++)
        {
            exps[i] = Mathf.Exp(logits[i] - max);
            sumExp += exps[i];
        }

        float[] probs = new float[logits.Length];
        for (int i = 0; i < logits.Length; i++)
            probs[i] = exps[i] / sumExp;

        return probs;
    }

    private void OnDisable()
    {
        worker?.Dispose();
    }
}
