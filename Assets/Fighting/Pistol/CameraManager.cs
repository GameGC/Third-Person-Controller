using System;
using System.Linq;
using Cinemachine;
using ThirdPersonController.Core.DI;
using UnityEditor;
using UnityEngine;

public enum CameraType : byte
{
    Follow = 0,
    Aiming = 1,
    AimingSniper = 2,
}

public class CameraManager : MonoBehaviour
{
    /// <summary>
    /// constant size depended on CameraType enums count
    /// </summary>
    [SerializeField, EnumedArray(typeof(CameraType))]
    private CinemachineVirtualCameraBase[] cameras;
    private CameraType _activeLook = CameraType.Follow;
    
    public ReferenceResolver resolver;
    
    public void SetActiveCamera(CameraType type)
    {
        CinemachineVirtualCameraBase prevLook = cameras[(int) _activeLook];
        CinemachineVirtualCameraBase newLook = cameras[(int) type];
        
        
        if(newLook == prevLook) return;

        CopyXAndY(newLook, prevLook);

        

        prevLook.gameObject.SetActive(false);
        newLook.gameObject.SetActive(true);
        _activeLook = type;
    }

    private void CopyXAndY(CinemachineVirtualCameraBase prevLook,CinemachineVirtualCameraBase newLook)
    {
        
        float xAxisValue = 0;
        float yAxisValue = 0;
        Pose state = new Pose();
        {
            if (prevLook is CinemachineFreeLook freeLook)
            {
                xAxisValue = freeLook.m_XAxis.Value ;
                yAxisValue = freeLook.m_YAxis.Value ;
                freeLook.transform.GetLocalPositionAndRotation(out state.position,out state.rotation);
            }
            else if (prevLook is CinemachineVirtualCamera virtualC)
            {
                var pov = virtualC.GetComponentPipeline().First(c => c is CinemachinePOV) as CinemachinePOV;
                xAxisValue = pov.m_HorizontalAxis.Value;
                yAxisValue = pov.m_VerticalAxis.Value;
                virtualC.transform.GetLocalPositionAndRotation(out state.position,out state.rotation);
            }
        }
        {
            if (newLook is CinemachineFreeLook freeLook)
            {
                freeLook.m_XAxis.Value = xAxisValue;
                freeLook.m_YAxis.Value = yAxisValue;
                freeLook.transform.SetLocalPositionAndRotation(state.position,state.rotation);
            }
            else if (newLook is CinemachineVirtualCamera virtualC)
            {
                var pov = virtualC.GetComponentPipeline().First(c => c is CinemachinePOV) as CinemachinePOV;
                pov.m_HorizontalAxis.Value = xAxisValue;
                pov.m_VerticalAxis.Value = yAxisValue;
                virtualC.transform.SetLocalPositionAndRotation(state.position,state.rotation);
                virtualC.transform.SetLocalPositionAndRotation(state.position,state.rotation);
            }
        }
    }

    private void PasteTargets(ICinemachineCamera newLook)
    {
        newLook.Follow = resolver.GetComponent<Transform>();
        newLook.LookAt = resolver.GetNamedComponent<Transform>("HeadLookAt");
    }
    
    private void PasteCustomTargets(ICinemachineCamera newLook,Transform follow,Transform lookAt)
    {
        newLook.Follow = follow ? follow : resolver.GetComponent<Transform>();
        newLook.LookAt = lookAt ? lookAt : resolver.GetNamedComponent<Transform>("HeadLookAt");
    }
    
    
    public void ReplaceCamera(CameraType type,CinemachineVirtualCameraBase prefab,Transform follow=null,Transform lookAt=null)
    {
        prefab.gameObject.SetActive(true);
        
        int previousIndex = (byte) type;
        if (cameras[previousIndex] != null)
        {
            var prevLook = cameras[previousIndex];

            bool wasActive = prevLook.gameObject.activeSelf;
            prevLook.gameObject.SetActive(false);

            var newLook = Instantiate(prefab, transform);
            newLook.name = prefab.name;

            PasteCustomTargets(newLook, follow, lookAt);

            Destroy(prevLook.gameObject);
            cameras[previousIndex] = newLook;

            newLook.gameObject.SetActive(wasActive);
        }
        else
        {
            var newLook = Instantiate(prefab, transform);
            newLook.name = prefab.name;
            
            PasteCustomTargets(newLook, follow, lookAt);
            newLook.gameObject.SetActive(false);
            cameras[(int) type] = newLook;
        }

    }


#if UNITY_EDITOR
    private static readonly int enumNamesSize = Enum.GetValues(typeof(CameraType)).Length;
    
    private void OnValidate()
    {
        if(cameras.Length!=enumNamesSize)
            Array.Resize(ref cameras,enumNamesSize);
    }

    private void Reset() => cameras = new CinemachineVirtualCameraBase[enumNamesSize];
#endif
}

public class EnumedArrayAttribute : PropertyAttribute
{
    public Type enumType;

    public EnumedArrayAttribute(Type enumType)
    {
        this.enumType = enumType;
    }
}

[CustomPropertyDrawer(typeof(EnumedArrayAttribute))]
public class EnumedArrayDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var enumNames = (attribute as EnumedArrayAttribute).enumType.GetEnumNames();
        
        Rect labelRect = position;
        labelRect.width /= 3;
        position.x += labelRect.width+1;
        position.width -= labelRect.width-1;

        var path = property.propertyPath;
        int index = int.Parse(path.Substring(path.LastIndexOf('[')+1, path.LastIndexOf(']') - path.LastIndexOf('[')-1));

        EditorGUI.BeginDisabledGroup(true);
        EditorGUI.LabelField(labelRect, enumNames[index], EditorStyles.popup);
        EditorGUI.EndDisabledGroup();

        EditorGUI.PropertyField(position, property, GUIContent.none);
    }
}