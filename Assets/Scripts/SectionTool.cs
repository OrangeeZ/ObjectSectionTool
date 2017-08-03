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
        while(length > 0)
        {
            var instance = Instantiate(_sectionPrefabs[Random.Range(0, _sectionPrefabs.Length)]);

            offset = GetPosition(instance, offset, ref length);
            instance.transform.position = rotation * offset;
            offset = GetPosition(instance, offset, ref length);

            instance.transform.rotation = rotation;
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
}
