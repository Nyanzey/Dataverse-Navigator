using UMAP;

// Clase de Dataset para JSON
public class Dataset
{
    public string FolderPath { get; set; } // Ruta de la carpeta
    public int Length { get; set; } // Num de images
	public int NumLabels { get; set; } // Num de cluster
	public int UmapNumNeighbors { get; set; } // Parametro de UMAP
	// public float UmapMinDistance { get; set; } // UMAP de C# no soporta ese parametro
	public string[] Filenames { get; set; } // Nombre de la imagen
	public float[][] Vectors { get; set; } // 1000 dimensiones, EfficientNet
	public float[][] Points { get; set; } // 3 dimensiones X Y Z, para UMAP
	public int[] Labels { get; set; } // Para Clustering
}

// Variables y funciones globales
public static class Global
{
    public static Dataset Data { get; set; }
	
	public static void ApplyUmap(int numNeighbors = 15)
	{
		Global.Data.UmapNumNeighbors = numNeighbors;
		var umap = new Umap(
			dimensions: 3,
			numberOfNeighbors: Global.Data.UmapNumNeighbors,
			random:  new DeterministicRandomGenerator(42),
			distance: Umap.DistanceFunctions.Euclidean
		);
		
		var numberOfEpochs = umap.InitializeFit(Global.Data.Vectors);
		
		for (var i = 0; i < numberOfEpochs; i++)
			umap.Step();
		
		Global.Data.Points = umap.GetEmbedding();
	}
}

// Clase para evitar aletoriedad en UMAP
public sealed class DeterministicRandomGenerator : IProvideRandomValues
{
	private readonly System.Random _rnd;
	
	public DeterministicRandomGenerator(int seed) => _rnd = new System.Random(seed);

	public bool IsThreadSafe => false;

	public int Next(int minValue, int maxValue) => _rnd.Next(minValue, maxValue);

	public float NextFloat() => (float)_rnd.NextDouble();

	public void NextFloats(System.Span<float> buffer)
	{
		for (var i = 0; i < buffer.Length; i++)
			buffer[i] = NextFloat();
	}
}