using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

// Job adding two floating point values together
public struct MyParallelJob : IJobParallelFor
{
  [ReadOnly]
  public float numParticlesSqrt;

  public NativeArray<Vector3> vertices;

  public void Execute(int i)
  {
    var x = i % numParticlesSqrt;
    var y = i / numParticlesSqrt;

    vertices[i] = new Vector3(x / (float)numParticlesSqrt, y / (float)numParticlesSqrt, i);
  }

  //Parallel.For(0, ParticleCount, i =>
  //{
  //  var x = i % numParticlesSqrt;
  //  var y = i / numParticlesSqrt;

  //  vertices[i] = new Vector3(x / (float) numParticlesSqrt, y / (float) numParticlesSqrt, i);
  //});

}
