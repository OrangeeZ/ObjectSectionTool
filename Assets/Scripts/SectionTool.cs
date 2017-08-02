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
        var rotation = Quaternion.LookRotation(_fromPoint - _toPoint, Vector3.up);
        for (var i = 0; i < length; i++)
        {
            var instance = Instantiate(_sectionPrefabs[Random.Range(0, _sectionPrefabs.Length - 1)]);
            instance.transform.position = rotation * GetPosition(instance, i);
            instance.transform.rotation = rotation;
            instance.transform.SetParent(transform);
        }
    }

    private Vector3 GetPosition(GameObject sectionInstance, int index)
    {
        var meshFilter = sectionInstance.GetComponent<MeshFilter>();
        var bounds = meshFilter.sharedMesh.bounds;
        var offset = Vector3.forward * bounds.extents.z;

        return offset + Vector3.forward * index * bounds.size.z;
    }
}
