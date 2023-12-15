using System.Linq;
using Fighting.Pushing;
using ThirdPersonController.Code.AnimatedStateMachine;
using ThirdPersonController.Core.DI;
using UnityEngine;

namespace Weapons
{
    public class WeaponOffset : MonoBehaviour
    {
        public Vector3 localPosition;
        [QuaternionAsEuler] public Quaternion localRotation;

        public string applyWhen;
        private void Awake()
        {
            if (applyWhen == string.Empty)
            {
                Apply();
            }
        }

        public void Apply() => transform.SetLocalPositionAndRotation(localPosition,localRotation);
        public void SetCustomPose(Pose pose) => transform.SetLocalPositionAndRotation(pose.position,pose.rotation);

#if UNITY_EDITOR
        [ContextMenu("CopyVariables")]
        public void CopyVariables() => transform.GetLocalPositionAndRotation(out localPosition,out localRotation);

        [ContextMenu("PasteVariables")]
        public void PasteVariables() => transform.SetLocalPositionAndRotation(localPosition,localRotation);
#endif
    }

    public class WeaponOffsetApplyFeature : BaseFeatureWithAwaiters
    {
        public string applyWhenName;
        public bool waitForStateWeight1;

        private IFightingStateMachineVariables _variables;
        private AnimationLayer _animationLayer;
        
        private WeaponOffset targetOffset;
        private Pose _previousPose;
        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _variables = variables as IFightingStateMachineVariables;
            _animationLayer = (_variables as FightingStateMachineVariables).GetComponent<AnimationLayer>();
        }

        public override async void OnEnterState()
        {
            base.OnEnterState();
            targetOffset ??= 
                _variables.weaponInstance.GetComponents<WeaponOffset>().First(o => o.applyWhen == applyWhenName);
            if (waitForStateWeight1)
            {
                await _animationLayer.WaitForNextState();
                if(!IsRunning) return;
            }

            targetOffset.transform.GetLocalPositionAndRotation(out _previousPose.position,out _previousPose.rotation);
            targetOffset.Apply();
        }

        public override void OnExitState()
        {
            base.OnExitState();
            targetOffset.SetCustomPose(_previousPose);
        }
    }
}