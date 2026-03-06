using UnityEngine;

public interface IEnemyVision
{
    float VisionRange { get; }
    float VisionAngle { get; }
    Transform EyePosition { get; }
    LayerMask ObstacleLayer { get; }
    bool IsFacingRight { get; }
    bool IsAlert { get; } 
}
