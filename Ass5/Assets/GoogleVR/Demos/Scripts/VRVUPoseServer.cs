using UnityEngine;
using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using UnityEngine.Assertions;

public class VRVUPoseServer : MonoBehaviour {

	public int portNo = 1235;
	public Vector3 position = new Vector3(0f, 0f, 0f);
	public Quaternion rotation = Quaternion.identity;

	private static bool serverActive = false;
	private static Thread senderThread;
	private static VRVUServer senderObj;
	private PacketHeader curr_pose = new PacketHeader();

	private BinaryReader reader;
	private float poseUpdateTime = 0.0f;
	public string filePath;
	// Use this for initialization
	void Start ()
	{
		startSender();
		Assert.IsTrue(filePath != "");

		reader = new BinaryReader (new FileStream (filePath, FileMode.Open));
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(senderObj != null)
		{
			serverActive = senderObj.getRunning();
		}
		else
		{
			serverActive = false;
		}
		if(serverActive)
		{
			poseUpdateTime += Time.deltaTime;
			if (poseUpdateTime > 0.1f) {
				poseUpdateTime -= 0.1f;

				curr_pose.t_x = reader.ReadSingle ();
				curr_pose.t_y = reader.ReadSingle ();
				curr_pose.t_z = reader.ReadSingle ();

				curr_pose.q_x = reader.ReadSingle ();
				curr_pose.q_y = reader.ReadSingle ();
				curr_pose.q_z = reader.ReadSingle ();
				curr_pose.q_w = reader.ReadSingle ();

				senderObj.sendPose(curr_pose);
			}
			/*
			curr_pose.t_x = position.x;
			curr_pose.t_y = position.y;
			curr_pose.t_z = position.z;

			curr_pose.q_x = rotation.x;
			curr_pose.q_y = rotation.y;
			curr_pose.q_z = rotation.z;
			curr_pose.q_w = rotation.w;

			senderObj.sendPose(curr_pose);*/

		}
	}

	public void startSender()
	{
		if(!serverActive)
		{
			if(senderObj == null)
			{
				senderObj = new VRVUServer();
				senderObj.init(portNo);
			}
			senderThread = new Thread(senderObj.DoWork);
			senderThread.Start();
			serverActive = true;
			Debug.Log ("PoseServer: sender started");
		}
	}
	
	public void stopSender()
	{
		if(serverActive)
		{
			serverActive = false;
			senderObj.RequestStop();
			senderThread.Join();
		}
	}
	
	public void OnApplicationQuit()
	{
		if(serverActive)
		{
			stopSender();
		}
	}
}

public class VRVUServer
{
	private PacketHeader pose_sent = new PacketHeader();
	private byte[] receivedData;
	private UdpClient server;
	private IPEndPoint ep;

	volatile private bool handshake = false;
	volatile private bool running = false; 

	public void init(int port_no)
	{
		Debug.Log("VRVU DUMMY Server - Sending 6DOF Pose Data (2017)");
		Debug.Log("==========================================================");

		IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port_no);
		server = new UdpClient(ipep);

		ep = new IPEndPoint(IPAddress.Any, 0);

		running = true;
	}

	public void sendPose(PacketHeader pose)
	{
		pose_sent = pose;
	}

	public bool getRunning()
	{
		return running;
	}

	public void RequestStop()
	{
		running = false;
	}

	private void receiveCallback(IAsyncResult res)
	{
		receivedData = server.EndReceive(res, ref ep);

		if (receivedData.Length == 28)
		{
			bool valid = true;

			for (int i = 0; i < 28; i++)
			{
				if (receivedData[i] != (i + 1))
				{
					valid = false;
					break;
				}
			}

			if (valid)
			{
				handshake = true;
				Debug.Log("Server handshake");
				return;
			}
		}
	}

	public void DoWork()
	{
		IAsyncResult res = server.BeginReceive(new AsyncCallback(receiveCallback), null);
		while (running && !handshake)
		{
			Thread.Sleep(1);
		}
		
		while (running)
		{
			byte[] packet_header = new byte[28];

			WritePacketHeader(ref packet_header);

			server.Send(packet_header, 28, ep);

			Thread.Sleep(10);
		}
		server.Close();
	}

	public void WritePacketHeader(ref byte[] byte_array)
	{
		MemoryStream stream = new MemoryStream(byte_array);
		BinaryWriter writer = new BinaryWriter(stream);

		writer.Write(pose_sent.t_x);
		writer.Write(pose_sent.t_y);
		writer.Write(pose_sent.t_z);

		writer.Write(pose_sent.q_x);
		writer.Write(pose_sent.q_y);
		writer.Write(pose_sent.q_z);
		writer.Write(pose_sent.q_w);
	}
}