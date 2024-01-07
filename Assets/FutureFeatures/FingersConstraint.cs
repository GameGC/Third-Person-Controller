using System.Collections.Generic;
using GameGC.Collections;
using Unity.Collections;

namespace UnityEngine.Animations.Rigging
{
    public interface ISourceTransforms
    {
        public IList<Transform> sourceObjects { get; }
        
        public Quaternion[] targetRotations { get; }
    }
    [System.Serializable]
    public struct FingersRotationConstraintData : IAnimationJobData,ISourceTransforms
    {
        [SyncSceneToStream, SerializeField] private SKeyValueList<Transform, Quaternion> m_SourceObjects;
        public bool IsValid()
        {
            return m_SourceObjects.Count > 0;
        }

        public void SetDefaultValues()
        {
            m_SourceObjects = new SKeyValueList<Transform, Quaternion>();
        }

        public IList<Transform> sourceObjects => m_SourceObjects.KeysArray;
        public Quaternion[] targetRotations => m_SourceObjects.ValuesArray.ToArray();
    }

    [Unity.Burst.BurstCompile]
    public struct FingersRotationJob : IWeightedAnimationJob
    {
        public NativeArray<ReadWriteTransformHandle> sourceTransforms;
        public NativeArray<Quaternion> targetRotations;
        
        public void ProcessAnimation(AnimationStream stream)
        {
            float w = jobWeight.Get(stream);
            if (w > 0f)
            {
                for (int i = 0; i < sourceTransforms.Length; i++)
                {
                    sourceTransforms[i].SetLocalRotation(stream,targetRotations[i]);
                }
            }
        }

        public void ProcessRootMotion(AnimationStream stream)
        {
        }

        public FloatProperty jobWeight { get; set; }
    }

    public class FingersRotationConstraintJobBinder<T> : AnimationJobBinder<FingersRotationJob, T>
        where T : struct, IAnimationJobData,ISourceTransforms
    {
        public override FingersRotationJob Create(Animator animator, ref T data, Component component)
        {
            var job = new FingersRotationJob();
            
            BindReadWriteTransforms(animator,component,data.sourceObjects, out job.sourceTransforms);
            job.targetRotations = new NativeArray<Quaternion>(data.targetRotations, Allocator.Persistent);

            return job;
        }

        private static void BindReadWriteTransforms(Animator animator, Component component, IList<Transform> transformArray, out NativeArray<ReadWriteTransformHandle> transforms)
        {
            transforms = new NativeArray<ReadWriteTransformHandle>(transformArray.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            for (int index = 0; index < transformArray.Count; ++index)
            {
                transforms[index] = ReadWriteTransformHandle.Bind(animator, transformArray[index].transform);
            }
        }

        public override void Destroy(FingersRotationJob job)
        {
            job.sourceTransforms.Dispose();
            job.targetRotations.Dispose();
        }
    }
    
    public class FingersConstraint : RigConstraint<FingersRotationJob,FingersRotationConstraintData,FingersRotationConstraintJobBinder<FingersRotationConstraintData>>
    {
    }
}