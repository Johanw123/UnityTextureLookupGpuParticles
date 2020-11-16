using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using Random = UnityEngine.Random;
using System.Threading.Tasks;
using Tayx.Graphy;
using Tayx.Graphy.CustomizationScene;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine.Rendering;

public class GpuParticles : MonoBehaviour
{
  private RenderTexture RT_Position;
  private RenderTexture RT_Velocity;

  private RenderTexture RT_Empty;

  private const int DEFAULT_PARTICLE_COUNT_SQRT = 1024;

  public int ParticleCount = DEFAULT_PARTICLE_COUNT_SQRT * DEFAULT_PARTICLE_COUNT_SQRT;

  private int m_textureWidth = DEFAULT_PARTICLE_COUNT_SQRT;
  private int m_textureHeight = DEFAULT_PARTICLE_COUNT_SQRT;

  private Material m_velocityMaterial;
  private Material m_positionMaterial;
  private Material m_particleMaterial;

  private Material m_particleInitPosMat;
  private Material m_particleInitVelMat;

  private Texture2D m_InitialPositions;
  private Texture2D m_InitialVelocities;

  private Mesh m_particleMesh;

  private int m_currentParticles;

  public bool ThreeDMode = true;
  private float m_cameraScaleFactor = 1.0f;

  public float CenterGravityForce = 3000f;
  public float MouseForcePull = 150.0f;
  public float MouseForcePush = 45.0f;

  public Material ParticleMaterial = null;
  private Camera m_camera;

  private InputHelper m_input = new InputHelper();

  void Start()
  {
    m_velocityMaterial = new Material(Shader.Find("Unlit/ParticlePhysicsVel"));
    m_positionMaterial = new Material(Shader.Find("Unlit/ParticlePhysicsPos"));

    m_particleMaterial = ParticleMaterial ?? new Material(Shader.Find("Unlit/Particle"));

    m_particleInitPosMat = new Material(Shader.Find("Unlit/ParticleInitialPosition"));
    m_particleInitVelMat = new Material(Shader.Find("Unlit/ParticleInitialVelocity"));

    QualitySettings.vSyncCount = 1;
    QualitySettings.maxQueuedFrames = 3;
    QualitySettings.shadows = ShadowQuality.Disable;
    Application.targetFrameRate = 60;

    m_camera = GetComponent<Camera>();

    Reset(true);
  }

  void Update()
  {
    m_input.UpdateInput(ThreeDMode, ref ParticleCount, Reset);
  }

  void SetupTextures()
  {
    if (ParticleCount <= 0)
      ParticleCount = DEFAULT_PARTICLE_COUNT_SQRT * DEFAULT_PARTICLE_COUNT_SQRT;

    var sqrt = Math.Sqrt(ParticleCount);

    //Find best fitting texture size with POT size
    for (int i = 2; i < 256; i *= 2)
    {
      int size = 64 * i;

      if (sqrt <= size)
      {
        m_textureWidth = m_textureHeight = size;
        break;
      }
    }

    if (RT_Position != null)
    {
      RT_Position.DiscardContents();
      RT_Velocity.DiscardContents();
      RT_Empty.DiscardContents();
    }

    SetupRT(ref RT_Position);
    SetupRT(ref RT_Velocity);
    SetupRT(ref RT_Empty);
  }

  private void SetupRT(ref RenderTexture RT)
  {
    RT?.DiscardContents(true, true);

    RT = new RenderTexture(m_textureWidth, m_textureHeight, 0, RenderTextureFormat.ARGBFloat,
      RenderTextureReadWrite.Default)
    {
      useMipMap = false,
      isPowerOfTwo = true,
      filterMode = FilterMode.Point,
    };
    RT.Create();
  }

  void SetupParticles()
  {
    m_InitialPositions = new Texture2D(m_textureWidth, m_textureHeight, TextureFormat.RGBAFloat, false, false);
    m_InitialVelocities = new Texture2D(m_textureWidth, m_textureHeight, TextureFormat.RGBAFloat, false, false);

    m_currentParticles = ParticleCount;

    var colsP = new Color[m_textureWidth * m_textureHeight];
    var colsV = new Color[m_textureWidth * m_textureHeight];

    int count = 0;

    for (int y = 0; y < m_textureHeight; y++)
    {
      for (int x = 0; x < m_textureWidth; x++)
      {
        var r = Random.insideUnitSphere * 5.0f;

        colsP[count] = new Color(r.x, r.y, 0, 0) * (ThreeDMode ? 6.0f : 1.0f);
        colsV[count] = new Color(r.x, r.y, 0, 0) * 112.8f;

        ++count;
      }
    }

    m_InitialPositions.SetPixels(colsP);
    m_InitialVelocities.SetPixels(colsV);
    m_InitialPositions.Apply(false, false);
    m_InitialVelocities.Apply(false, false);

    m_positionMaterial.SetFloat("_ThreeDFactor", ThreeDMode ? 1.0f : 0.0f);
  }

  void SetupMesh()
  {
    m_particleMesh?.Clear(false);
    m_particleMesh = new Mesh { indexFormat = IndexFormat.UInt32 };

    var numParticlesSqrt = (int)Math.Floor(Math.Sqrt(ParticleCount));

    var indices = GetIndicies(ParticleCount);
    var vertices = GetVertices(ParticleCount, numParticlesSqrt);

    m_particleMesh.vertices = vertices;
    m_particleMesh.SetIndices(indices, MeshTopology.Points, 0, false);
    m_particleMesh.Optimize();
  }
  private int[] GetIndicies(int size)
  {
    var indiciesResult = new NativeArray<int>(size, Allocator.TempJob);
    var indicesJob = new GenerateIndices { Indices = indiciesResult };
    var indiciesJobHandle = indicesJob.Schedule(size, 0);
    indiciesJobHandle.Complete();
    indiciesResult = indicesJob.Indices;

    var indices = new int[size];
    SetNativeVertexArray(indices, indiciesResult);

    indiciesResult.Dispose();

    return indices;
  }

  private Vector3[] GetVertices(int size, int numParticlesSqrt)
  {
    var result = new NativeArray<Vector3>(size, Allocator.TempJob);
    var job = new GenerateVertices
    {
      NumParticlesSqrt = numParticlesSqrt,
      Vertices = result
    };
    var indiciesJobHandle = job.Schedule(size, 0);
    indiciesJobHandle.Complete();
    result = job.Vertices;

    var vertices = new Vector3[size];
    SetNativeVertexArray(vertices, result);

    result.Dispose();

    return vertices;
  }

  unsafe void SetNativeVertexArray<T>(T[] vertexArray, NativeArray<T> vertexBuffer) where T : unmanaged
  {
    // pin the target vertex array and get a pointer to it
    fixed (void* vertexArrayPointer = vertexArray)
    {
      // memcopy the native array over the top
      UnsafeUtility.MemCpy(vertexArrayPointer, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(vertexBuffer),
        vertexArray.Length * (long)UnsafeUtility.SizeOf<T>());
    }
  }

  public void Reset(bool positionsFromTexture)
  {
    RT_Position?.DiscardContents(true, true);
    RT_Velocity?.DiscardContents(true, true);

    //We recalculate render targets and meshes if particle count has changed
    if (ParticleCount != m_currentParticles)
    {
      SetupTextures();
      SetupMesh();
      SetupParticles();
    }

    if (positionsFromTexture) //We can either Blit the original initial positions we created before
    {
      Graphics.Blit(m_InitialPositions, RT_Position);
      Graphics.Blit(m_InitialVelocities, RT_Velocity);
      //Blit Empty RT for no initial velocity
      //Graphics.Blit(RT_Empty, RT_Velocity);
    }
    else //Or we can let a shader reset/set the new positions/velocities
    {
      Graphics.Blit(RT_Empty, RT_Position, m_particleInitPosMat);
      Graphics.Blit(RT_Empty, RT_Velocity, m_particleInitVelMat);
    }
  }

  public Vector3 GetWorldPositionOnPlane(Vector3 screenPosition, float z)
  {
    Ray ray = m_camera.ScreenPointToRay(screenPosition);
    Plane xy = new Plane(Vector3.forward, new Vector3(0, 0, z));
    float distance;
    xy.Raycast(ray, out distance);
    return ray.GetPoint(distance);
  }

  void OnPreRender()
  {
    var curMouseStateLeft = Input.GetMouseButton(0);
    var curMouseStateRight = Input.GetMouseButton(1);

    if (ThreeDMode)
      m_cameraScaleFactor = m_camera.fieldOfView * 10.0f / Screen.height;
    else
      m_cameraScaleFactor = m_camera.orthographicSize * 0.5f / Screen.height;

    var mouseForce = MouseForcePull;/*(ThreeDMode ? 150f : 15f)*/;
    if (curMouseStateRight) mouseForce = -MouseForcePush;
    else if (!curMouseStateLeft) mouseForce = 0.0f;

    m_velocityMaterial.SetTexture("_PosTex", RT_Position);
    m_velocityMaterial.SetTexture("_VelTex", RT_Velocity);
    m_velocityMaterial.SetFloat("_DeltaTime", Time.deltaTime);
    m_velocityMaterial.SetFloat("_GravityScale", 0);
    m_velocityMaterial.SetFloat("_MouseForce", mouseForce);
    m_velocityMaterial.SetFloat("_CenterGravityForce", CenterGravityForce);
    m_velocityMaterial.SetFloat("_CameraScaleFactor", m_cameraScaleFactor);

    m_velocityMaterial.SetPass(0);

    var mousePos = Vector3.zero;

    if (ThreeDMode)
    {
      mousePos = GetWorldPositionOnPlane(Input.mousePosition, 0);
      mousePos = new Vector3(mousePos.x / Screen.width, mousePos.y / Screen.height, 0);
    }
    if (!ThreeDMode)
    {
      var wPos = m_camera.ScreenToWorldPoint(Input.mousePosition);
      mousePos = new Vector3(wPos.x / Screen.width, wPos.y / Screen.height, 0);
    }

    m_velocityMaterial.SetVector("_MousePos", (curMouseStateLeft || curMouseStateRight) ? mousePos : Vector3.zero);

    //Calculate and blit the new velocities to the Velocity Render Target
    RT_Velocity.DiscardContents();
    Graphics.Blit(RT_Empty, RT_Velocity, m_velocityMaterial);
    
    //Feed velocities and previous frame position to the Position-shader
    m_positionMaterial.SetTexture("_PosTex", RT_Position);
    m_positionMaterial.SetTexture("_VelTex", RT_Velocity);
    m_positionMaterial.SetFloat("_DeltaTime", Time.deltaTime);
    m_positionMaterial.SetPass(0);

    //Calculate and blit the new positions to the Position Render Target
    RT_Position.DiscardContents();
    Graphics.Blit(RT_Empty, RT_Position, m_positionMaterial);
  }

  void OnPostRender()
  {
    //Feed positions and velocities to the particle rendering shader
    m_particleMaterial.SetFloat("_ScreenWidth", Screen.width);
    m_particleMaterial.SetFloat("_ScreenHeight",  Screen.height);
    m_particleMaterial.SetTexture("_PosTex", RT_Position);
    m_particleMaterial.SetTexture("_VelTex", RT_Velocity);
    m_particleMaterial.SetPass(0);

    GL.Clear(true, true, Color.black, 1);

    //Draws all the particles to screen/render target
    Graphics.DrawMeshNow(m_particleMesh, new Vector3(0, 0, 0), Quaternion.identity);

    m_particleMaterial.SetPass(1);
    Graphics.Blit(null, null, m_particleMaterial);
  }
}
