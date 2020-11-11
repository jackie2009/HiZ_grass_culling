using System.Collections;
using System.Collections.Generic;
 using UnityEngine;
using UnityEngine.Rendering;

public class HzbDepthTexMaker : MonoBehaviour
{

    public RenderTexture hzbDepth;
     public Shader hzbShader;
    private Material hzbMat;
 
    public bool stopMpde;
    // Use this for initialization
    void Start()
    {
        hzbMat = new Material(hzbShader);
        Camera.main.depthTextureMode |= DepthTextureMode.Depth;

        hzbDepth = new RenderTexture(1024, 1024, 0, RenderTextureFormat.RHalf);
        hzbDepth.autoGenerateMips = false;

        hzbDepth.useMipMap = true;
        hzbDepth.filterMode = FilterMode.Point;
        hzbDepth.Create();
        HzbInstance.HZB_Depth = hzbDepth;

    }
    void OnDestroy()
    {
        hzbDepth.Release();
        Destroy(hzbDepth);

    }

    int ID_DepthTexture;
    int ID_InvSize;
#if UNITY_EDITOR
    void Update()
    {
#else

    void OnPreRender()
    {
#endif

        if (stopMpde)
        {

            return;
        }
        int w = hzbDepth.width;
        int h = hzbDepth.height;
        int level = 0;

        RenderTexture lastRt = null;
        if (ID_DepthTexture == 0)
        {
            ID_DepthTexture = Shader.PropertyToID("_DepthTexture");
            ID_InvSize = Shader.PropertyToID("_InvSize");
        }
        RenderTexture tempRT;
        while (h > 8)
        {


            hzbMat.SetVector(ID_InvSize, new Vector4(1.0f / w, 1.0f / h, 0, 0));

            tempRT = RenderTexture.GetTemporary(w, h, 0, hzbDepth.format);
            tempRT.filterMode = FilterMode.Point;
            if (lastRt == null)
            {
              //  hzbMat.SetTexture(ID_DepthTexture, Shader.GetGlobalTexture("_CameraDepthTexture"));
                Graphics.Blit(Shader.GetGlobalTexture("_CameraDepthTexture"), tempRT);
            }
            else
            {
                hzbMat.SetTexture(ID_DepthTexture, lastRt);
                Graphics.Blit(null, tempRT, hzbMat);
                RenderTexture.ReleaseTemporary(lastRt);
            }
            Graphics.CopyTexture(tempRT, 0, 0, hzbDepth, 0, level);
            lastRt = tempRT;

            w /= 2;
            h /= 2;
            level++;


        }
        RenderTexture.ReleaseTemporary(lastRt);
    }

}
