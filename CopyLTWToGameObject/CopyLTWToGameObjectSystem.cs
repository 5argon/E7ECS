using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace E7.ECS.AttachGameObject
{
    /// <summary>
    /// CopyTransformToGameObjectSystem will copy PRS (not LTW, actually scale not supported too) 
    /// which is a local transform in the case that it has any attaches. The correct copy should be global transform.
    /// 
    /// This system allows the CALCULATED LTW from TransformSystem to be applied to game object.
    /// It runs after transform system to allow leaf object to calculate its world pos first before copying.
    /// 
    /// The matrix is decoded very simply, you must not have projections encoded in the matrix.
    /// TODO : Currently decode only position and scale..
    /// </summary>
    [UpdateAfter(typeof(EndFrameTransformSystem))]
    [ExecuteInEditMode]
    public class CopyLTWToGameObjectSystem : JobComponentSystem
    {
        [BurstCompile]
        struct CopyTransforms : IJobParallelForTransform
        {
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> ltw;

            [ReadOnly]
            public EntityArray entities;

            public void Execute(int index, TransformAccess transform)
            {
                var entity = entities[index];
                float4x4 matrix = ltw[entity].Value;

                //Decode the matrix
                var translation = matrix.c3;
                transform.position = new Vector3(translation.x, translation.y, translation.z);

                var scale = new Vector3(math.length(matrix.c0), math.length(matrix.c1), math.length(matrix.c2));
                transform.localScale = scale;
            }
        }

        ComponentGroup m_TransformGroup;

        protected override void OnCreateManager()
        {
            m_TransformGroup = GetComponentGroup(ComponentType.ReadOnly(typeof(CopyLTWToGameObject)), typeof(UnityEngine.Transform));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var transforms = m_TransformGroup.GetTransformAccessArray();
            var entities = m_TransformGroup.GetEntityArray();

            var copyTransformsJob = new CopyTransforms
            {
                ltw = GetComponentDataFromEntity<LocalToWorld>(isReadOnly: true),
                entities = entities
            };

            return copyTransformsJob.Schedule(transforms, inputDeps);
        }
    }
}
