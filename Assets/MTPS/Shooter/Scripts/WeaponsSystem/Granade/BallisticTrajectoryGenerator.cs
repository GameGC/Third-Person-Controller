using Fighting.Pushing;
using MTPS.Core;
using UnityEngine;

public class BallisticTrajectoryGenerator : MonoBehaviour
{
    [Range(10, 100)] public int linePoints = 25;
    [SerializeField] private float heightMul = 1;

    

    private Transform _crossHair;
    private Transform _upperPointYPos;
    private Transform _granadeInstance;


    private void Awake()
    {
        var resolver = GetComponentInParent<ReferenceResolver>();
        
        _crossHair = resolver.GetNamedComponent<Transform>("TargetLook");
        _upperPointYPos = resolver.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head).transform.GetChild(0);
    }

    // Start is called before the first frame update
    private void Start()
    {
        _granadeInstance = GetComponent<FightingStateMachineVariables>().weaponInstance.transform;
    }

    public void GenerateTrajectoryOut(out Vector3[] points)
    {
        points = new Vector3[linePoints];
        GenerateTrajectory(ref points);
    }

    public void GenerateTrajectory(ref Vector3[] points)
    {
        var crossHairPos = _crossHair.position;
        crossHairPos.y = Mathf.Clamp(crossHairPos.y, -Mathf.Infinity, _upperPointYPos.position.y);
      
        var middle = Vector3.Lerp(crossHairPos, _granadeInstance.position,0.5f);
        middle.y += Vector3.Distance(crossHairPos,_granadeInstance.position) * heightMul;
        GenerateTrajectory(_granadeInstance.position,in middle,in crossHairPos,ref points);
    }
    private void GenerateTrajectory(in Vector3 lineStart,in Vector3 lineMidUp,in Vector3 lineEnd,ref Vector3[] points)
    {
        float t = 0f;
        for (int i = 0; i < linePoints; i++)
        {
            points[i] = (1 - t) * (1 - t) * lineStart + 2 * (1 - t) * t * lineMidUp + t * t * lineEnd;
            t += 1 / (float)linePoints;
        }
    }
}