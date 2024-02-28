using MTPS.Core;
using MTPS.Shooter.FightingStateMachine;
using ThirdPersonController.Input;
using UnityEngine;

namespace MTPS.Shooter.WeaponsSystem.Granade
{
    [RequireComponent(typeof(LineRenderer),typeof(BallisticTrajectoryGenerator))]
    public class BallisticTrajectoryPreview : MonoBehaviour
    {
        private IFightingStateMachineVariables _variables; 
        private LineRenderer _lineRenderer;

        private BallisticTrajectoryGenerator _generator;
        private Vector3[] _generatedPoints;

        private IShooterInput _inputReader;

        private void Awake()
        {
            _generator = GetComponent<BallisticTrajectoryGenerator>();
            _inputReader = GetComponentInParent<ReferenceResolver>().GetComponent<IShooterInput>();
            _generatedPoints = new Vector3[_generator.linePoints];

            _variables = GetComponent<IFightingStateMachineVariables>();
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.positionCount = _generator.linePoints;
        }

        private void OnEnable() => _lineRenderer.enabled = true;
        private void OnDisable() => _lineRenderer.enabled = false;

        // Update is called once per frame
        private void Update()
        {
            _lineRenderer.forceRenderingOff = _variables.isCooldown || !(_inputReader.IsAim || _inputReader.IsAttack);
            _generator.GenerateTrajectory(ref _generatedPoints);
            _lineRenderer.SetPositions(_generatedPoints);
        }
    }
}