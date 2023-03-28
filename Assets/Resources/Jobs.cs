using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
public struct GenerateMeshData : IJobParallelFor
{
  [ReadOnly]
  public int NumParticlesSqrt;

  public NativeArray<Vector3> Vertices;
  public NativeArray<int> Indices;

  public void Execute(int i)
  {
    var x = i % NumParticlesSqrt;
    var y = i / NumParticlesSqrt;

    Vertices[i] = new Vector3((x / (float)NumParticlesSqrt), (y / (float)NumParticlesSqrt), i);
    Indices[i] = i;
  } 
}

public struct GenerateTextureData : IJobParallelFor
{
  [ReadOnly]
  public int NumParticles;
  [ReadOnly] 
  public float Factor;

  public NativeArray<Color> Positions;
  public NativeArray<Color> Velocities;

  public void Execute(int i)
  {
    var r = Unity.Mathematics.Random.CreateFromIndex((uint)i);
    var xy = r.NextFloat2Direction();
    Positions[i] = new Color(xy.x, xy.y, 0, 0);
    Velocities[i] = new Color(xy.x, xy.y, 0, 0) * 112.8f;
  }
}