using System;
using System.Collections.Generic;
using System.Linq;

public static class ExtensionMethods
{
  public static IEnumerable<IEnumerable<T>> Split<T>(this T[] array, int size)
  {
    for (var i = 0; i < (float)array.Length / size; i++)
    {
      yield return array.Skip(i * size).Take(size);
    }
  }

  public static IEnumerable<T[]> SplitArray<T>(this T[] array, int size)
  {
    for (var i = 0; i < (float)array.Length / size; i++)
    {
      yield return array.Skip(i * size).Take(size).ToArray();
    }
  }

  public static IEnumerable<T[]> AsChunks<T>(this T[] source, int chunkMaxSize)
  {
    var pos = 0;
    var sourceLength = source.Length;
    do
    {
      var len = Math.Min(pos + chunkMaxSize, sourceLength) - pos;
      if (len == 0)
      {
        yield break;
      }
      var arr = new T[len];
      Array.Copy(source, pos, arr, 0, len);
      pos += len;
      yield return arr;
    } while (pos < sourceLength);
  }

  public static IEnumerable<IEnumerable<T>> SplitList<T>(this List<T> list, int size)
  {
    for (var i = 0; i < (float)list.Count / size; i++)
    {
      yield return list.Skip(i * size).Take(size).ToList();
    }
  }

  public static T[] Slice<T>(this T[] source, int start, int end)
  {
    // Handles negative ends.
    if (end < 0)
    {
      end = source.Length + end;
    }
    int len = end - start;

    // Return new array.
    T[] res = new T[len];
    for (int i = 0; i < len; i++)
    {
      res[i] = source[i + start];
    }
    return res;
  }
}