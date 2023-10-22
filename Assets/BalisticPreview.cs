using Fighting.Pushing;
using ThirdPersonController.Core.DI;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(LineRenderer))]
public class BalisticPreview : MonoBehaviour
{
    [FormerlySerializedAs("LinePoints")] [SerializeField,Range(10, 100)] private int linePoints = 25;
    [SerializeField] private float heightMul = 1;
    
    private LineRenderer _lineRenderer;

    private Transform _crossHair;
    private Transform _upperPointYPos;
    private Transform _granadeInstance;


    private Vector3[] _generatedPoints;
    private void Awake()
    {
        _generatedPoints = new Vector3[linePoints];
        
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = linePoints;
    }

    // Start is called before the first frame update
    void Start()
    {
        var resolver = GetComponentInParent<ReferenceResolver>();
        
        _crossHair = resolver.GetNamedComponent<Transform>("TargetLook");
        _upperPointYPos = resolver.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head).transform.GetChild(0);

        _granadeInstance = GetComponent<FightingStateMachineVariables>().weaponInstance.transform;
    }

    // Update is called once per frame
    private void Update()
    {
        if (_lineRenderer.enabled)
        {
            GenerateTrajectory(ref _generatedPoints);
            _lineRenderer.SetPositions(_generatedPoints);
        }
    }

    public void GenerateTrajectoryOut(out Vector3[] points)
    {
        points = new Vector3[linePoints];
        GenerateTrajectory(ref points);
    }

    private void GenerateTrajectory(ref Vector3[] points)
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