using UnityEditor;
using UnityEngine;

internal static class ColliderExtensions
{
    [MenuItem("CONTEXT/Collider/Place Collider By Bounds", false)]
    public static void PlacePivotOnChildsCenter(MenuCommand command)
    {
        var collider = command.context as Collider;
        if (PrefabUtility.IsPartOfAnyPrefab(collider.gameObject))
            PrefabUtility.UnpackPrefabInstance(collider.gameObject, PrefabUnpackMode.OutermostRoot,
                InteractionMode.UserAction);


        var renderers = collider.GetComponentsInChildren<Renderer>();
        var bounds = new Bounds
        {
            center = renderers[0].bounds.center
        };
        foreach (var componentsInChild in renderers)
        {
            bounds.Encapsulate(componentsInChild.bounds);
        }

        if (collider is BoxCollider box)
        {
            box.center = box.transform.InverseTransformPoint(bounds.center);
            box.size = box.transform.InverseTransformVector(bounds.size);
        }
        else if(collider is CapsuleCollider capsule)
        {
            capsule.center = capsule.transform.InverseTransformPoint(bounds.center);
            var  size = capsule.transform.InverseTransformVector(bounds.size);
            
            var tempSize = new Vector3(Mathf.Abs(size.x),Mathf.Abs(size.y),Mathf.Abs(size.z));
            var max = Mathf.Max(tempSize.x, tempSize.y, tempSize.z);
            capsule.direction = max == tempSize.x ? 0 : max == tempSize.y ? 1 : 2;

            capsule.radius = Mathf.Abs(size.x + size.y + size.z) / 6;
            capsule.height = Mathf.Abs(size[capsule.direction]);

            //capsule.radius = Mathf.Abs(size[capsule.direction]);
            //capsule.height = (((Vector2) size).magnitude);
        }
        else if(collider is SphereCollider sphere)
        {
            sphere.center = sphere.transform.InverseTransformPoint(bounds.center);
            var size = sphere.transform.InverseTransformVector(bounds.size);
            sphere.radius = size.magnitude / 2;
        }
    }
}