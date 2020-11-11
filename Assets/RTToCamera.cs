using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTToCamera : MonoBehaviour
{
  public RenderTexture RT;

  void OnPreRender()
  {
    //GL.Clear(true, true, Color.clear);
    Graphics.Blit(RT, null as RenderTexture);
  }
}
