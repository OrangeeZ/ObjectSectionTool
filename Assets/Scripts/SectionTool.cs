using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class SectionTool : MonoBehaviour
{
    [SerializeField]
    private GameObject[] _sectionPrefabs;

    [SerializeField]
    private Vector3 _fromPoint;

    [SerializeField]
    private Vector3 _toPoint;

    private bool _isDirty;

    void OnValidate()
    {
        _isDirty = true;
    }

    void LateUpdate()
    {
        if (_isDirty)
        {
            GenerateSection();

            _isDirty = false;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(_fromPoint, 1f);
        Gizmos.DrawWireSphere(_toPoint, 1f);
    }

    private void GenerateSection()
    {
        foreach (var each in transform.OfType<Transform>().ToArray())
        {
            DestroyImmediate(each.gameObject);
        }

        var length = (_toPoint - _fromPoint).magnitude;
        var rotation = Quaternion.LookRotation(_toPoint - _fromPoint, Vector3.up);
        var offset = Vector3.zero;
        //for (var i = 0; i < length; i++)
        while (length > 0)
        {
            var instance = Instantiate(_sectionPrefabs[Random.Range(0, _sectionPrefabs.Length)]);

            var lengthDelta = 0f;
            var previousOffset = offset;

            instance.transform.position = rotation * offset;
            offset = GetPosition(instance, offset, ref lengthDelta);
            offset = GetPosition(instance, offset, ref lengthDelta);

            instance.transform.rotation = rotation;
            AlignTransformToTerrain(instance.transform);

            var projectedForward = instance.transform.forward;
            projectedForward.y = 0;
            projectedForward = projectedForward.normalized;

            Debug.Log(Vector3.Dot(projectedForward, instance.transform.forward));

            length += lengthDelta * Vector3.Dot(projectedForward, instance.transform.forward);
            offset = previousOffset + (offset - previousOffset) * Vector3.Dot(projectedForward, instance.transform.forward);
            //instance.transform.position = rotation * offset;
            //AlignTransformToTerrain(instance.transform);

            instance.transform.SetParent(transform);
        }
    }

    private Vector3 GetPosition(GameObject sectionInstance, Vector3 offset, ref float length)
    {
        var meshFilter = sectionInstance.GetComponent<MeshFilter>();
        var bounds = meshFilter.sharedMesh.bounds;
        var worldExtents = sectionInstance.transform.TransformVector(bounds.extents);

        //if (offset.magnitude == 0)
        {
            length -= worldExtents.z;
            offset += Vector3.forward * worldExtents.z;
        }

        //offset += Vector3.forward * worldSize.z * 0.5f;

        return offset;
    }

    private static void AlignTransformToTerrain(Transform target)
    {


        var bounds = GetObjectBounds(target);
        var localToWorldMatrix = target.localToWorldMatrix;

        var samplingPoints = new List<Vector3>();
        var extents = bounds.extents;
        var reflectedExtents = bounds.extents;
        reflectedExtents.x *= -1;

        var center = bounds.center;

        samplingPoints.Add(center + extents);
        samplingPoints.Add(center + reflectedExtents);
        samplingPoints.Add(center - extents);
        samplingPoints.Add(center - reflectedExtents);

        for (var i = 0; i < samplingPoints.Count; ++i)
        {
            var point = samplingPoints[i];
            point.y = center.y - extents.y;
            point = localToWorldMatrix.MultiplyPoint3x4(point);
            point.y = Terrain.activeTerrain.SampleHeight(point);

            samplingPoints[i] = point;

            //Debug.DrawRay(samplingPoints[i], Vector3.up, Color.red);

        }


        var forwardA = samplingPoints[0] - samplingPoints[3];
        var forwardB = samplingPoints[1] - samplingPoints[2];
        var averageForward = Vector3.Lerp(forwardA, forwardB, 0.5f);

        var normalA = Vector3.Cross(forwardA, samplingPoints[0] - samplingPoints[1]);
        var normalB = Vector3.Cross(forwardB, samplingPoints[3] - samplingPoints[2]);
        var averageNormal = Vector3.Lerp(normalA, normalB, 0.5f);

        //var averageHeight = samplingPoints.Select(_ => _.y).Average();

        //var targetPosition = target.position;
        //targetPosition.y = averageHeight;

        //target.position = targetPosition;
        target.rotation = Quaternion.LookRotation(averageForward, averageNormal);

        var height = Terrain.activeTerrain.SampleHeight(target.position);
        var position = target.position;
        position.y = height;
        target.position = position;
    }

    private static Bounds GetObjectBounds(Transform target)
    {
        var meshFilter = target.GetComponentInChildren<MeshFilter>();
        if (meshFilter != null)
        {
            return meshFilter.sharedMesh.bounds;
        }

        return target.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh.bounds;
    }
}
