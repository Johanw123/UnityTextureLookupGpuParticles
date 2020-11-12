using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
public struct GenerateIndices : IJobParallelFor
{
  public NativeArray<int> Indices;

  public void Execute(int i)
  {
    Indices[i] = i;
  }
}
