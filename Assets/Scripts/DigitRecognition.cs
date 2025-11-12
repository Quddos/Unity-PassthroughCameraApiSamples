using UnityEngine;
using Unity.InferenceEngine;

public class DigitRecognition : MonoBehaviour
{
    public Texture2D testPicture;
    public ModelAsset modelAsset;
    public float[] results;
    private Worker worker;

    void Start()
    {
        // Load the AI model
        Model model = ModelLoader.Load(modelAsset);
        FunctionalGraph graph = new FunctionalGraph();
        FunctionalTensor[] inputs = graph.AddInputs(model);
        FunctionalTensor[] outputs = Functional.Forward(model, inputs);

        // Compile and prepare the model
        Model runtimeModel = graph.Compile(outputs);
        worker = new Worker(runtimeModel, BackendType.GPUCompute);

        // Run the AI model
        int predicted = RunAI(testPicture);
        Debug.Log("Predicted Digit: " + predicted);
    }

    public int RunAI(Texture2D picture)
    {
        using Tensor<float> inputTensor = TextureConverter.ToTensor(picture, 28, 28, 1);
        worker.Schedule(inputTensor);

        // Get the model output (logits)
        Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;
        results = outputTensor.DownloadToArray();

        // Convert to probabilities using Softmax
        float[] probs = Softmax(results);

        // Find the highest probability (predicted digit)
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

        Debug.Log("Softmax Probabilities: " + string.Join(", ", probs));
        Debug.Log("Highest Probability: " + (maxValue * 100f).ToString("F2") + "%");

        return maxIndex;
    }

    // Softmax logic: turns results into probabilities that add up to 1
    float[] Softmax(float[] logits)
    {
        float maxLogit = Mathf.Max(logits);
        float sumExp = 0f;
        float[] expVals = new float[logits.Length];

        for (int i = 0; i < logits.Length; i++)
        {
            expVals[i] = Mathf.Exp(logits[i] - maxLogit);
            sumExp += expVals[i];
        }

        float[] probs = new float[logits.Length];
        for (int i = 0; i < logits.Length; i++)
            probs[i] = expVals[i] / sumExp;

        return probs;
    }

    private void OnDisable()
    {
        worker.Dispose();
        // /jdkj
    }
}
