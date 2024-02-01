using UnityEngine;

public class Global 
{
    public const string SCENEGAME = "SampleScene";
    public const string LAYERMASKGROUND = "Ground";
    
    public static Vector3Int[] NavmeshLinkDirections = new Vector3Int[]
        { Vector3Int.right, Vector3Int.forward, Vector3Int.left, Vector3Int.back };
    
    public const int TOTALLIVES = 3;
    
    public const int DRAGDROPRAYCASTLENGTH = 25;
    public const int RAYCASTSETUPNAVMESH = 2;

    public static LayerMask LayerMaskGround;

    public static void Init()
    {
        LayerMaskGround =LayerMask.GetMask(LAYERMASKGROUND);
    }

    public static void Pause()
    {
        Time.timeScale = 0;
    }

    public static void Play()
    {
        Time.timeScale = 1;
    }

    public static void SlowDown()
    {
        Time.timeScale = 0.35f;
    }
}
