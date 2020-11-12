using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
public struct GenerateVertices : IJobParallelFor
{
  [ReadOnly]
  public int NumParticlesSqrt;

  public NativeArray<Vector3> Vertices;

  public void Execute(int i)
  {
    var x = i % NumParticlesSqrt;
    var y = i / NumParticlesSqrt;

    Vertices[i] = new Vector3((x / (float)NumParticlesSqrt), (y / (float)NumParticlesSqrt), i);
  }
}