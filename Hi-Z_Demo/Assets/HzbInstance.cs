using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class HzbInstance : MonoBehaviour {
 
	public ComputeShader shader;
	public Mesh mesh;
	public Material drawMat;
	public static RenderTexture HZB_Depth;
	public   Texture testDepth;
	ComputeBuffer bufferWithArgs;
	private uint[] args;
	private int CSCullingID;
	private Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 10000);
	ComputeBuffer posBuffer;
 
	static int staticRandomID = 0;
	float StaticRandom() {
		float v = 0;
		v = Mathf.Abs( Mathf.Sin(staticRandomID)) * 1000+  Mathf.Abs(Mathf.Cos(staticRandomID*0.1f)) * 100 ;
		v -= (int)v;
		 
		staticRandomID++;
		return v;
	}
	// 为地形创建 16万棵草 测试传统做法开销  terrain.terrainData.detailResolution 设置400 然后每个单位设置数量为1 就是16w了
	[ContextMenu("createGrassForTerrain")]
	void createGrassForTerrain() {
		Terrain terrain= FindObjectOfType<Terrain>();
		//terrain.terrainData.SetDetailLayer()
		int [,] grassCount = new int[terrain.terrainData.detailResolution, terrain.terrainData.detailResolution];
        for (int i = 0; i < terrain.terrainData.detailResolution; i++)
        {
            for (int j = 0; j < terrain.terrainData.detailResolution; j++)
            {
				grassCount[j, i] = 1;

			}
        }
		terrain.terrainData.SetDetailLayer(0, 0, 0, grassCount);
 
	}
	// Use this for initialization
	void Start () {
		//测试  16万棵草 computeshader 模式
		int count = 400*400;
	var terrain=	FindObjectOfType<Terrain>();
		Vector3[] posList = new Vector3[count];
		for (int i = 0; i < count; i++)
		{
			int x = i % 400;
			int z = i / 400;
			 posList[i] = new Vector3(x*0.5f+ StaticRandom(),0,z*0.5f+ StaticRandom());
			posList[i].y = terrain.SampleHeight(posList[i]);
 		}

 
		args = new uint[] { mesh.GetIndexCount(0), 0, 0, 0, 0 };
		bufferWithArgs = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
		bufferWithArgs.SetData(args);
		CSCullingID = shader.FindKernel("CSCulling");

		posBuffer = new ComputeBuffer(count, 4*3);
		posBuffer.SetData(posList);
		var posVisibleBuffer = new ComputeBuffer(count, 4*3);
		shader.SetBuffer(CSCullingID, "bufferWithArgs", bufferWithArgs);
		shader.SetBuffer(CSCullingID, "posAllBuffer", posBuffer);
		shader.SetBuffer(CSCullingID, "posVisibleBuffer", posVisibleBuffer);

		drawMat.SetBuffer("posVisibleBuffer", posVisibleBuffer);
 
	}
	void culling() {
 		shader.SetFloat("useHzb", useHzb ? 1 : 0);
		 args[1] = 0;
		 	bufferWithArgs.SetData(args);
		if (HZB_Depth != null)
		{
			shader.SetTexture(CSCullingID, "HZB_Depth", HZB_Depth);
			 
		}
		 
		shader.SetVector("cmrPos", Camera.main.transform.position);
		shader.SetVector("cmrDir", Camera.main.transform.forward);
		shader.SetFloat("cmrHalfFov", Camera.main.fieldOfView/2);
 		var m = GL.GetGPUProjectionMatrix( Camera.main.projectionMatrix,false) * Camera.main.worldToCameraMatrix;

		//高版本 可用  computeShader.SetMatrix("matrix_VP", m); 代替 下面数组传入
		float[] mlist = new float[] {
			m.m00,m.m10,m.m20,m.m30,
		   m.m01,m.m11,m.m21,m.m31,
			m.m02,m.m12,m.m22,m.m32,
			m.m03,m.m13,m.m23,m.m33
		};


		shader.SetFloats("matrix_VP", mlist);
		shader.Dispatch(CSCullingID, 400 / 16, 400 / 16, 1);
	}

	// Update is called once per frame
	void Update() {
		// Camera.main.transform.position += Camera.main.transform.right * Time.deltaTime *5* (Mathf.Sin(Time.timeSinceLevelLoad ) > 0 ? 1 : -1);
	//	Camera.main.transform.Rotate(Vector3.up, -Time.deltaTime * 60);
		if (computeshaderMode == false) return;
		if (updateCulling)
		{
			culling();
		}
		Graphics.DrawMeshInstancedIndirect(mesh, 0, drawMat, bounds, bufferWithArgs, 0, null, ShadowCastingMode.Off, false);
	}


	//以下为测试开关的临时代码 实际工程不会用到  所以用性能低下的OnGUI 写了下

 	public bool useHzb = false;
	public bool updateCulling = true;
	public bool computeshaderMode = true;

	void OnGUI() {

        if (GUILayout.Button("computeshaderMode:" + (computeshaderMode ? "on" : "off")))
        {
			computeshaderMode = !computeshaderMode;
			var terrain = FindObjectOfType<Terrain>();
			if (computeshaderMode)
			{
				terrain.detailObjectDistance = 0;
				GetComponent<HzbDepthTexMaker>().enabled = true;
			}
			else {
				terrain.detailObjectDistance = 150;
				GetComponent<HzbDepthTexMaker>().enabled = false;
			}
        }
        if (GUILayout.Button("useHzb:" + (useHzb ? "on" : "off")))
		{
			useHzb = !useHzb;
		}
  

        if (GUILayout.Button("updateCulling:" + (updateCulling ? "on" : "off")))
        {
            updateCulling = !updateCulling;
        }
        //if (GUILayout.Button("stopDepthBlit"))
        //{
        //	GetComponent<HzbTest>().stopMpde = true;
        //}
    }
}
