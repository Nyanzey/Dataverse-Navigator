using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System;
using UnityEngine;
using TMPro;
using System.Threading;
using MPack;

public class ConnectionScript : MonoBehaviour
{
	public GameObject pointGenerator;
	public GameObject connectionCanvasText;
	private SynchronizationContext syncContext;
	private Thread thread;
	private Socket conn;
	private NetworkStream stream;
	private Utils utilscript;

    // Start is called before the first frame update
    void Start()
    {
		syncContext = SynchronizationContext.Current;
		utilscript = GameObject.FindGameObjectWithTag("menu").GetComponent<Utils>();
		try
		{
			conn = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			conn.Connect("127.0.0.1", 5000);
			stream = new NetworkStream(conn);
			Debug.Log("Conexion con controlador establecida");

			thread = new Thread(ReceiveMessages);
			thread.Start();

			//Changing text of connection state in navigator menu
			TextMeshProUGUI connectionText = connectionCanvasText.GetComponent<TextMeshProUGUI>();
			connectionText.text = "Controller connected";
			connectionText.color = Color.green;
		}
		catch (SocketException ex)
		{
			Debug.LogError($"SocketException: {ex.Message}");
			
		}
		catch (Exception ex)
		{
			Debug.LogError($"Exception: {ex.Message}");
		}
	}
	
	void OnApplicationQuit()
    {
        if (thread != null)
		{
			Debug.Log("Cerrando thread");
            thread.Abort();
		}
    }

	void ReceiveMessages()
	{
		while (true)
		{
			var msg = MToken.ParseFromStream(stream);
			var type = msg["type"].To<string>();
			
			switch (type)
			{
			case "create_workspace":
				syncContext.Post(_ =>
				{
					Debug.Log("Create workspace with id: " + msg["workspace_id"].To<int>());
				}, null);
				break;
			case "delete_workspace":
				syncContext.Post(_ =>
				{
					utilscript.deleteClusteringSpace(msg["workspace_id"].To<int>());
				}, null);
				break;
			case "update_points":
				// Para evitar error de threading unity
				syncContext.Post(_ =>
				{
					pointGenerator.GetComponent<DataPlotter>().Plot(msg["points"].To<float[][]>(), msg["workspace_id"].To<int>());
					pointGenerator.GetComponent<DataPlotter>().EnclosePoints();
				}, null);
				break;
			case "update_labels":
				syncContext.Post(_ =>
				{
					utilscript.updateLabels(msg["workspace_id"].To<int>(), msg["labels"].To<int[]>(), msg["colors"].To<float[][]>());
				}, null);
				break;
			case "response_image":
				syncContext.Post(_ =>
				{
					GameObject parentObj = GameObject.FindWithTag("LabelUI");
					if (parentObj != null)
					{
						Transform childTransform = FindChildRecursive(parentObj.transform, "Panel");
						if (childTransform != null)
						{
							GameObject childObj = childTransform.gameObject;
							SetPanelValues(childObj, msg["title"].To<string>(), msg["image"].To<byte[]>(), msg["coords"].To<float[]>());
							childObj.SetActive(true);
						}
					}
				}, null);
				break;
			case "remove_point_selection":
				syncContext.Post(_ =>
				{
					utilscript.deselectPoint(msg["workspace_id"].To<int>(), msg["index"].To<int>());
				}, null);
				break;
			default:
				Debug.Log("Mensaje no soportado");
				break;
			}
		}
	}

	public void SendSelection(int spaceId, List<int> indexes)
	{
		var msg = new MDict {
			{"type", "set_selection"},
			{"workspace_id", spaceId},
			{"indexes", MToken.From(indexes.ToArray())},
		};
		msg.EncodeToStream(stream);
	}
	
	public void SendClearSelection(int spaceId)
	{
		var msg = new MDict {
			{"type", "clear_selection"},
			{"workspace_id", spaceId},
		};
		msg.EncodeToStream(stream);
	}

	public void SendRequestImage(int spaceId, int index)
	{
		var msg = new MDict {
			{"type", "request_image"},
			{"workspace_id", spaceId},
			{"index", index},
		};
		msg.EncodeToStream(stream);
	}

	void SetPanelValues(GameObject panel, string txt, byte[] img, float[] coords)
    {
		Transform title = FindChildRecursive(panel.transform, "Title");
        Transform image = FindChildRecursive(panel.transform, "Image");
        Transform coordinates = FindChildRecursive(panel.transform, "Coordinates");
        
        if (image == null) Debug.Log("image panel object not found.");
        if (title == null) Debug.Log("title panel object not found.");
        if (coordinates == null) Debug.Log("coordinates panel object not found.");

        // Get the TextMeshPro component attached to the GameObject
        TextMeshProUGUI titleText = title.GetComponent<TextMeshProUGUI>();
        titleText.text = txt;

        TextMeshProUGUI coordinatesText = coordinates.GetComponent<TextMeshProUGUI>();
        coordinatesText.text = string.Format("{0:.##} {1:.##} {2:.##}", coords[0], coords[1], coords[2]);
		
		// Para cargar imagenes del disco
		// Puede que haya una fuga de memoria aca
		Texture2D tex = new Texture2D(400, 400);
		tex.LoadImage(img);
		image.GetComponent<UnityEngine.UI.Image>().overrideSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0));
    }

	Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }

            // Recursively search through children
            Transform result = FindChildRecursive(child, name);
            if (result != null)
            {
                return result;
            }
        }

        // Child not found
        return null;
    }
}