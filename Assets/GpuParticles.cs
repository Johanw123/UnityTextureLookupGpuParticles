using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Random = UnityEngine.Random;
using System.Threading.Tasks;

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

  private readonly List<Mesh> m_meshes = new List<Mesh>();

  private int m_currentParticles;

  public bool ThreeDMode = true;
  private float m_cameraScaleFactor = 1.0f;

  public Material ParticleMaterial;

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

    Reset(true);
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

    int count = -1;

    for (int y = 0; y < m_textureHeight; y++)
    {
      for (int x = 0; x < m_textureWidth; x++)
      {
        var r = Random.insideUnitSphere * 5.0f;
        var r2 = Random.insideUnitCircle * 2.0f;
        colsP[++count] = new Color(r.x, r.y, /*r.z * 0.01f*/0, 0) * (ThreeDMode ? 6.0f : 1.0f);
        colsV[count] = new Color(-r2.x, -r2.y, 0, 0) * 12.8f;
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
    var vertices = new Vector3[ParticleCount];

    foreach (var m in m_meshes)
    {
      m.Clear(false);
    }

    m_meshes.Clear();

    var meshCount = (int)Math.Ceiling(ParticleCount / 65000d);

    for (int i = 0; i < meshCount; i++)
    {
      m_meshes.Add(new Mesh());
    }

    var numParticlesSqrt = (int)Math.Floor(Math.Sqrt(ParticleCount));

    Parallel.For(0, ParticleCount, i =>
    {
      var x = i % numParticlesSqrt;
      var y = i / numParticlesSqrt;

      vertices[i] = new Vector3(x / (float)numParticlesSqrt, y / (float)numParticlesSqrt, i);
    });


    var numVertexPerMesh = (int)(vertices.Length / (float)meshCount);

    var indices = new int[numVertexPerMesh];

    for (int i = 0; i < numVertexPerMesh; i++)
      indices[i] = i;

    var vertexChunks = vertices.AsChunks(vertices.Length / meshCount).ToArray();

    for (int i = 0; i < meshCount; i++)
    {
      m_meshes[i].vertices = vertexChunks[i];
      m_meshes[i].SetIndices(indices, MeshTopology.Points, 0, false);
    }
  }

  void Update()
  {
    if (Input.GetAxis("Mouse ScrollWheel") < 0) // back/down
    {
      int newSize = (int)(Camera.main.orthographicSize + 10000f * Time.deltaTime);
      newSize = Math.Min(newSize, 20000);
      Camera.main.orthographicSize = newSize;
    }
    if (Input.GetAxis("Mouse ScrollWheel") > 0) // forward/up
    {
      int newSize = (int)(Camera.main.orthographicSize - 10000f * Time.deltaTime);
      newSize = Math.Max(newSize, 500);
      Camera.main.orthographicSize = newSize;
    }

    if (Input.GetKeyDown(KeyCode.Space))
    {
      Time.timeScale = Time.timeScale <= 0 ? 1.0f : 0.0f;
    }

    if (Input.GetKeyDown(KeyCode.R))
      Reset(true);
    if (Input.GetKeyDown(KeyCode.T))
      Reset(false);


    if (Input.GetKeyDown(KeyCode.Alpha1))
    {
      ParticleCount = 1000000;
      Reset(true);
    }
    if (Input.GetKeyDown(KeyCode.Alpha2))
    {
      ParticleCount = 2000000;
      Reset(true);
    }
    if (Input.GetKeyDown(KeyCode.Alpha3))
    {
      ParticleCount = 3000000;
      Reset(true);
    }
    if (Input.GetKeyDown(KeyCode.Alpha4))
    {
      ParticleCount = 4000000;
      Reset(true);
    }
    if (Input.GetKeyDown(KeyCode.Alpha5))
    {
      ParticleCount = 5000000;
      Reset(true);
    }
    if (Input.GetKeyDown(KeyCode.Alpha6))
    {
      ParticleCount = 6000000;
      Reset(true);
    }
  }

  public void Reset(bool positionsFromTexture)
  {
    //We recalculate render targets and meshes if particle count has changed
    if(ParticleCount != m_currentParticles)
    {
      SetupTextures();
      SetupMesh();
      SetupParticles();
    }

    RT_Position.DiscardContents();
    RT_Velocity.DiscardContents();

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
    Ray ray = Camera.main.ScreenPointToRay(screenPosition);
    Plane xy = new Plane(Vector3.forward, new Vector3(0, 0, z));
    float distance;
    xy.Raycast(ray, out distance);
    return ray.GetPoint(distance);
  }

  void OnPreRender()
  {
    if(ThreeDMode)
      m_cameraScaleFactor = Camera.main.fieldOfView * 10.0f / Screen.height;
    else
      m_cameraScaleFactor = Camera.main.orthographicSize * 00.5f / Screen.height;

    //m_velocityMaterial.SetVector("_GravityDirection", new Vector2(0, 1));
    m_velocityMaterial.SetTexture("_PosTex", RT_Position);
    m_velocityMaterial.SetTexture("_VelTex", RT_Velocity);
    m_velocityMaterial.SetFloat("_DeltaTime", Time.deltaTime);
    m_velocityMaterial.SetFloat("_GravityScale", 0);
    m_velocityMaterial.SetFloat("_MouseForce", ThreeDMode ? 150f : 15f);
    m_velocityMaterial.SetFloat("_CameraScaleFactor", m_cameraScaleFactor);

    var curMouseState = Input.GetMouseButton(0);

    var mousePos = Vector3.zero;

    if (ThreeDMode)
    {
      mousePos = GetWorldPositionOnPlane(Input.mousePosition, 0);
      mousePos = new Vector3(mousePos.x / Screen.width, mousePos.y / Screen.height, 0);
    }
    if (!ThreeDMode)
    {
      var wPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
      mousePos = new Vector3(wPos.x / Screen.width, wPos.y / Screen.height, 0);
    }

    m_velocityMaterial.SetVector("_MousePos", curMouseState ? mousePos : Vector3.zero);

    //Calculate and blit the new velocities to the Velocity Render Target
    RT_Velocity.DiscardContents();
    Graphics.Blit(RT_Empty, RT_Velocity, m_velocityMaterial);
    
    //Feed velocities and previous frame position to the Position-shader
    m_positionMaterial.SetTexture("_PosTex", RT_Position);
    m_positionMaterial.SetTexture("_VelTex", RT_Velocity);
    m_positionMaterial.SetFloat("_DeltaTime", Time.deltaTime);

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

    GL.Clear(true, true, Color.black);

    //Draws all the particles to screen
    foreach (var mesh in m_meshes)
    {
      Graphics.DrawMeshNow(mesh, new Vector3(0, 0, 0), Quaternion.identity);
    }
  }
}
