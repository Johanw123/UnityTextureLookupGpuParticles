using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHelper : MonoBehaviour
{
  public void UpdateInput(bool ThreeDMode, ref int ParticleCount, Action<bool> ResetCallback)
  {
    if (Input.GetAxis("Mouse ScrollWheel") < 0) // back/down
    {
      if (ThreeDMode)
      {
        int newSize = (int) (Camera.main.transform.position.z - 2000f * Time.deltaTime);
        newSize = Math.Max(newSize, -2000);
        Camera.main.transform.localPosition = new Vector3(0, 0, newSize);
      }
      else
      {
        int newSize = (int) (Camera.main.orthographicSize + 100000f * Time.deltaTime);
        newSize = Math.Min(newSize, 100000);
        Camera.main.orthographicSize = newSize;
      }
    }

    if (Input.GetAxis("Mouse ScrollWheel") > 0) // forward/up
    {
      if (ThreeDMode)
      {
        int newSize = (int) (Camera.main.transform.position.z + 2000f * Time.deltaTime);
        newSize = Math.Min(newSize, -1);
        Camera.main.transform.localPosition = new Vector3(0, 0, newSize);
      }
      else
      {
        int newSize = (int) (Camera.main.orthographicSize - 100000f * Time.deltaTime);
        newSize = Math.Max(newSize, 500);
        Camera.main.orthographicSize = newSize;
      }
    }

    if (Input.GetKeyDown(KeyCode.Space))
    {
      Time.timeScale = Time.timeScale <= 0 ? 1.0f : 0.0f;
    }

    if (Input.GetKeyDown(KeyCode.R))
      ResetCallback?.Invoke(true);
    if (Input.GetKeyDown(KeyCode.T))
      ResetCallback?.Invoke(false);


    if (Input.GetKeyDown(KeyCode.Alpha1))
    {
      ParticleCount = 128*128;
      ResetCallback?.Invoke(false);
    }
    
    if (Input.GetKeyDown(KeyCode.Alpha2))
    {
      ParticleCount = 256*256;
      ResetCallback?.Invoke(false);
    }

    if (Input.GetKeyDown(KeyCode.Alpha3))
    {
      ParticleCount = 512*512;
      ResetCallback?.Invoke(false);
    }

    if (Input.GetKeyDown(KeyCode.Alpha4))
    {
      ParticleCount = 1024*1024;
      ResetCallback?.Invoke(false);
    }

    if (Input.GetKeyDown(KeyCode.Alpha5))
    {
      ParticleCount = 2048*2048;
      ResetCallback?.Invoke(false);
    }

    if (Input.GetKeyDown(KeyCode.Alpha6))
    {
      ParticleCount = 4096*4096;
      ResetCallback?.Invoke(false);
    }

    if (Input.GetKeyDown(KeyCode.Alpha7))
    {
      ParticleCount = 8192*8192;
      ResetCallback?.Invoke(false);
    }
  }
}