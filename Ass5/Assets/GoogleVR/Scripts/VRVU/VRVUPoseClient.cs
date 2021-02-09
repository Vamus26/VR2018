using UnityEngine;
using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;

public class PacketHeader
{
	public float t_x;
	public float t_y;
	public float t_z;

	public float q_w;
	public float q_x;
	public float q_y;
	public float q_z;
};

public class VRVUPoseClient : MonoBehaviour
{

	public int portNo = 1235;
	public string serverIpAddressString = "127.0.0.1";

	private static bool clientActive = false;
	private static Thread listenerThread;
	private static VRVUClient listenerObj;
	private PacketHeader curr_pose = new PacketHeader();
   
    // Use this for initialization
    void Start()
	{
		startListener();
       
    }

	// Update is called once per frame
	void Update()
	{
		if (listenerObj != null)
		{
			clientActive = listenerObj.getRunning();
		}
		else
		{
			clientActive = false;
		}
		if (clientActive)
		{
			curr_pose = listenerObj.getPose();
		}
	}

	public void startListener()
	{
		if (!clientActive)
		{
			if (listenerObj == null)
			{
				listenerObj = new VRVUClient();
				listenerObj.init(serverIpAddressString, portNo);
			}
			listenerThread = new Thread(listenerObj.DoWork);
			listenerThread.Start();
			clientActive = true;
			Debug.Log("PoseClient: listener started");
		}
	}

	public void stopListener()
	{
		if (clientActive)
		{
			clientActive = false;
			listenerObj.RequestStop();
			listenerThread.Join();
		}
	}

	public void OnApplicationQuit()
	{
		if (clientActive)
		{
			stopListener();
		}
	}

	public Vector3 getPosition()
	{
		Vector3 pos;
		pos.x = (float)curr_pose.t_y;
		pos.y = -(float)curr_pose.t_z;
		pos.z = (float)curr_pose.t_x;
		return pos;
	}

	public Quaternion getRotation()
	{
		Quaternion rotation;
		rotation.x = -curr_pose.q_y;
		rotation.y = curr_pose.q_z;
		rotation.z = -curr_pose.q_x;
		rotation.w = curr_pose.q_w;

		Vector3 eul_rotation;
		eul_rotation.x = rotation.eulerAngles.x;
		eul_rotation.y = rotation.eulerAngles.y;
		eul_rotation.z = rotation.eulerAngles.z;
		Quaternion adjusted_rotation = Quaternion.Euler(eul_rotation);

		return adjusted_rotation;
	}
}

public class VRVUClient
{
	private PacketHeader pose_received = new PacketHeader();
	private UdpClient client;
	private byte[] receivedData;
	private IPEndPoint ep;
    public BinaryWriter writer;
    volatile private bool running = false;
    private int counter;

    public void init(string ip_address, int port_no)
	{
        counter = 0;
        string filePath = @"C:\VRVU4\Data";
        writer = new BinaryWriter(File.Open(filePath + "capture.txt", FileMode.Create));
        Debug.Log("VRVU DUMMY CLIENT - Receiving 6DOF Pose Data (2017)");
		Debug.Log("==========================================================");

		ep = new IPEndPoint(IPAddress.Parse(ip_address), port_no); // endpoint where server is listening
		try
		{
			client = new UdpClient(port_no + 1);
			client.Connect(ep);
			running = true;
		}
		catch (Exception ex)
		{
			Debug.Log("Client Connect exception: " + ex.Message);
			running = false;
		}
	}

	public bool getRunning()
	{
		return running;
	}

	public PacketHeader getPose()
	{
		return pose_received;
	}

	public void RequestStop()
	{
		running = false;
	}

	private bool sanityCheck(ref byte[] bytes)
	{
		if (receivedData.Length != 28) //no full packet header
		{
			return false;
		}

		for (int b = 0; b < 28; b++) // we don't expect everything to be 0
		{
			if (receivedData[b] != 0x0)
			{ return true; }
		}
		return false;
	}

	private void receiveCallback(IAsyncResult res)
	{
		receivedData = client.EndReceive(res, ref ep);

		if (sanityCheck(ref receivedData))
		{
			ReadPacketHeader(receivedData);
			Debug.Log("Received pose!");
			Debug.Log(new Vector3(pose_received.t_x, pose_received.t_y, pose_received.t_z));
			Debug.Log(new Vector4(pose_received.q_x, pose_received.q_y, pose_received.q_z, pose_received.q_w));
		}
	}

	private void handShake()
	{
		client.Send(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28 }, 28);
	}

	public void DoWork()
	{
		handShake();

		while (running)
		{
			IAsyncResult res = client.BeginReceive(new AsyncCallback(receiveCallback), null);

			int slept = 0;

			while (running && !res.IsCompleted)
			{
				Thread.Sleep(1);
				slept++;
				if (slept > 100)
				{
					handShake();
					slept = 0;
				}
			}
		}
		client.Close();
	}

	public void ReadPacketHeader(byte[] byte_array)
	{
        if (counter < 500)
        {
            MemoryStream stream = new MemoryStream(byte_array);
            BinaryReader reader = new BinaryReader(stream);
            counter++;
            pose_received.t_x = reader.ReadSingle();
            pose_received.t_y = reader.ReadSingle();
            pose_received.t_z = reader.ReadSingle();

            pose_received.q_w = reader.ReadSingle();
            pose_received.q_x = reader.ReadSingle();
            pose_received.q_y = reader.ReadSingle();
            pose_received.q_z = reader.ReadSingle();

            writer.Write(pose_received.t_x);

            writer.Write(pose_received.t_y);

            writer.Write(pose_received.t_z);
            writer.Write(pose_received.q_w);

            writer.Write(pose_received.q_x);

            writer.Write(pose_received.q_y);

            writer.Write(pose_received.q_z);

        }
        else
        {
            writer.Close();
        }
    }
}