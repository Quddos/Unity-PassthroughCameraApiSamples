// using UnityEngine;
// using Unity.InferenceEngine;


// public class DigitRecognition : MonoBehaviour
// {
//     public float threshold = 0.9f;
//     public Texture2D testPicture;
//     public ModelAsset modelAsset;
//     public float [] results;
//     private Worker worker;

//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
//         Model model = ModelLoader.Load(modelAsset);
//         FunctionalGraph graph = new FunctionalGraph();
//         FunctionalTensor[] inputs = graph.AddInputs(model);
//         FunctionalTensor[] outputs = Functional.Forward(model, inputs);
//         FunctionalTensor softmax = Functional.Softmax(outputs[0]);

//         Model runtimeModel = graph.Compile(outputs);
//         worker = new Worker(runtimeModel, BackendType.GPUCompute);

//         Debug.Log(RunAI(testPicture));
        
//     }

//     public int RunAI(Texture2D picture)
//     {
//         using Tensor<float> inputTensor = TextureConverter.ToTensor(picture, 28, 28, 1);
//         worker.Schedule(inputTensor);

//         // to get our results
//         Tensor<float> outputTensor= worker.PeekOutput() as Tensor<float>;

//         results = outputTensor.DownloadToArray();
//         return GetMaxIndex(results);
//     }

//     // private void /// <summary>
//     // /// This function is called when the behaviour becomes disabled or inactive.
//     // /// </summary>
//     private void OnDisable()
//     {
//         worker.Dispose();
//     }

//     public int GetMaxIndex(float[] array)
//     {
//         int maxIndex = 0;
//         for (int i=0; i<array.Length; i++)
//         {
//             if(array[i]>array[maxIndex])
//             {
//                 maxIndex = i;
//             }
//         }
//         if(array[maxIndex]>threshold)
//         {
//             return maxIndex;
//         }
//         else
//         {
//             return -1;
//         }
//     }

  
// }
