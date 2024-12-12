using System.Collections;
using System.Collections.Generic;

namespace UnityEngine
{
    public static class UnityExtensions
    {
        public static Vector3 ToVector3XZ(this Vector2 v2)
        {
            return new Vector3(v2.x, 0, v2.y);
        }


        const int layerCount = 32;
        public static void SetToCollisionMask(this LayerMask layerMask, Layer layer)
        {
            layerMask.SetToCollisionMask((int)layer);
        }
        public static void SetToCollisionMask(this LayerMask layerMask, string layer)
        {
            layerMask.SetToCollisionMask(LayerMask.NameToLayer(layer));
        }
        public static void SetToCollisionMask(this LayerMask layerMask, int layer)
        {
            int collisionMask = 0;
            for (int i = 0; i < layerCount; i++)
            {
                if (!Physics.GetIgnoreLayerCollision(layer, i))
                    collisionMask |= 1 << i;
            }

            layerMask.value = collisionMask;
        }

        public static Vector3 RoundToInt(this Vector3 v3) => new(
            System.MathF.Round(v3.x, System.MidpointRounding.AwayFromZero),
            System.MathF.Round(v3.y, System.MidpointRounding.AwayFromZero),
            System.MathF.Round(v3.z, System.MidpointRounding.AwayFromZero));

        public static Vector3 ToVector3(this Vector3Int v3I) => new(v3I.x, v3I.y, v3I.z);
        public static Vector3 DivideComponentWise(this Vector3 lhs, Vector3 rhs) => new(lhs.x / rhs.x, lhs.y / rhs.y, lhs.z / rhs.z);
        public static Vector4 ToVector4(this Vector3 v3, float w) => new(v3.x, v3.y, v3.z, w);
        public static Vector3Int ToVector3Int(this Vector3 v3) => new((int)v3.x, (int)v3.y, (int)v3.z);
        public static Vector3Int FloorToVector3Int(this Vector3 v3) => new(Mathf.FloorToInt(v3.x), Mathf.FloorToInt(v3.y), Mathf.FloorToInt(v3.z));
        public static Vector3Int CeilingToVector3Int(this Vector3 v3) => new(Mathf.CeilToInt(v3.x), Mathf.CeilToInt(v3.y), Mathf.CeilToInt(v3.z));
    }

}
