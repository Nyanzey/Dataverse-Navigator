using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.IO;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System.Text.Json;
using Unity.Barracuda;

public class DataProcessor : MonoBehaviour
{
    static string FolderPath;
	public NNModel ModelAsset;

    public void SelectPath()
    {
        FolderPath = EditorUtility.OpenFolderPanel("Select folder", "", "");
		
        if (string.IsNullOrEmpty(FolderPath))
        {
            Debug.Log("Error");
			return;
        }
		
		Debug.Log("Selected: " + FolderPath);
		Debug.Log("Start loading dataset: " + System.DateTime.Now);
		
		if (!File.Exists(FolderPath + "\\dataset.json"))
		{
			GenerateDataset();
		}
		else
		{
			Global.Data = JsonSerializer.Deserialize<Dataset>(File.ReadAllText(FolderPath + "\\dataset.json"));
			Global.Data.FolderPath = FolderPath;
		}
		
		Debug.Log("End loading dataset: " + System.DateTime.Now);
		SceneManager.LoadScene("MainScene");
    }
	
	public static string[] GetFilesFrom(string searchFolder, string[] filters, bool isRecursive)
	{
		List<string> filesFound = new List<string>();
		var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
		
		foreach (var filter in filters)
		{
		   filesFound.AddRange(Directory.GetFiles(searchFolder, string.Format("*.{0}", filter), searchOption));
		}
		
		return filesFound.ToArray();
	}
	
	public void GenerateDataset()
	{
		Model model = ModelLoader.Load(ModelAsset);
		IWorker worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);
		string[] files = GetFilesFrom(FolderPath, new string[] {"bmp", "png", "jpeg", "jpg"}, false);
		
		Global.Data = new Dataset() {
			FolderPath = FolderPath,
			Length = files.Length,
			NumLabels = 0,
			UmapNumNeighbors = 15,
			// UmapMinDistance = 0.1F,
			Filenames = new string[files.Length],
			Vectors = new float[files.Length][],
			Points = new float[files.Length][],
			Labels = new int[files.Length]
		};

		for (int i = 0; i < files.Length; ++i)
		{
			Global.Data.Filenames[i] = files[i].Substring(FolderPath.Length + 1);
			var image = Image.Load<Rgba32>(files[i]);
			var smallestSide = Mathf.Min(image.Width, image.Height);
			image.Mutate(x => x.Crop(
				new Rectangle(
					(image.Width - smallestSide) / 2,
					(image.Height - smallestSide) / 2,
					smallestSide,
					smallestSide
			)));
			image.Mutate(x => x.Resize(224, 224, KnownResamplers.NearestNeighbor));
			var input = new Tensor(1, 224, 224, 3);

			for (var x = 0; x < 224; x++)
			{
				for (var y = 0; y < 224; y++)
				{
					// normalizacion [-1, 1]
					input[0, x, y, 0] = ((float)(image[x, y].R - 127)) / 128.0F;
					input[0, x, y, 1] = ((float)(image[x, y].G - 127)) / 128.0F;
					input[0, x, y, 2] = ((float)(image[x, y].B - 127)) / 128.0F;
				}
			}

			worker.Execute(input);
			input.Dispose();
			image.Dispose();
			Global.Data.Vectors[i] = worker.PeekOutput().ToReadOnlyArray();
		}
		
		worker.Dispose();
		Global.ApplyUmap();
		string json = JsonSerializer.Serialize(Global.Data);
		Debug.Log("JSON Length: " + json.Length);
		File.WriteAllText(FolderPath + "\\dataset.json", json);
	}
}