using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Entities;
using UnityEngine;

//An idea that does not work : inheriting from GameObjectEntity as LinkingGameObjectEntity
//1. On `OnEnable` it is not guaranteed that child's `GameObjectEntity` will have been enabled and created its own `Entity` to be linked.
//   The linking step has to be done after all Enable.
//2. We have to reflection to scan for all gather compatible component data.
//Solution : The simplest is to make you call `Gather` by yourself with a generic.

/// <summary>
/// Gather `Entity` generated from other `GameObjectEntity` as this `GameObjectEntity`'s `Entity` field of its component data.
/// </summary>
public class EntityGatherer : SharedComponentDataWrapper<EntityGathererData>
{
    /// <summary>
    /// Gather entities on active world. Call while you are sure all related `GameObjectEntity` are active. (So that the `Entity` exists in the world)
    /// 1. Attach `EntityGatherer` component on game object with `GameObjectEntity` and your gather target `ComponentDataWrapper`
    /// 2. Link up multiple child `GameObjectEntity` as you like in the Inspector.
    /// 3. In your `IComponentData` definition add [GatherTarget(index)] to any `Entity` field or `struct` field with `Entity` inside, which needs [GatherTarget] to mark the spot.
    /// 4. Call this static method.
    /// </summary>
    public static void Gather<T>() where T : struct, IComponentData
    {
        World.Active.GetOrCreateManager<EntityGathererSystem>().Gather<T>();
    }

    /// <summary>
    /// Gather entities on any world. Call while you are sure all related `GameObjectEntity` are active. (So that the `Entity` exists in the world)
    /// 1. Attach `EntityGatherer` component on game object with `GameObjectEntity` and your gather target `ComponentDataWrapper`
    /// 2. Link up multiple child `GameObjectEntity` as you like in the Inspector.
    /// 3. In your `IComponentData` definition add [GatherTarget(index)] to any `Entity` field or `struct` field with `Entity` inside, which needs [GatherTarget] to mark the spot.
    /// 4. Call this static method.
    /// </summary>
    public static void Gather<T>(World w) where T : struct, IComponentData
    {
        w.GetOrCreateManager<EntityGathererSystem>().Gather<T>();
    }
}

[System.Serializable]
public struct EntityGathererData : ISharedComponentData
{
    public GameObjectEntity parent;
    public GameObjectEntity[] childs;
}

[System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class EntityGatherTargetAttribute : System.Attribute
{
    public int ChildIndex { get; }
    public EntityGatherTargetAttribute(int childIndex = -1)
    {
        this.ChildIndex = childIndex;
    }
}

[DisableAutoCreation]
public class EntityGathererSystem : ComponentSystem
{
    ComponentType[] gathererType = new ComponentType[]{ ComponentType.Create<EntityGathererData>() };
    ComponentGroup gathererGroup;
    protected override void OnCreateManager()
    {
        gathererGroup = GetComponentGroup(gathererType);
        this.Enabled = false;
    }

    protected override void OnUpdate() { }

    /// <summary>
    /// Parent must have the component T
    /// </summary>
    public void Gather<T>() where T : struct, IComponentData
    {
        var scda = gathererGroup.GetSharedComponentDataArray<EntityGathererData>();
        for (int i = 0; i < scda.Length; i++)
        {
            GameObjectEntity parent = scda[i].parent;
            //Debug.Log($"Parent {parent.Entity}");
            var data = EntityManager.GetComponentData<T>(parent.Entity);
            System.Type tType = typeof(T);
            foreach (var field in tType.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                var gatherTarget = (EntityGatherTargetAttribute)field.GetCustomAttributes(typeof(EntityGatherTargetAttribute), false).FirstOrDefault();
                if (gatherTarget != null)
                {
                    Type fieldType = field.FieldType;
                    Entity toSet = scda[i].childs[gatherTarget.ChildIndex].Entity;
                    if (fieldType == typeof(Entity))
                    {
                        object dataBox = data;
                        field.SetValue(dataBox, toSet);
                        data = (T)dataBox;
                        //Debug.Log($"Successfully gather entity for parent GO {parent.name} at index {gatherTarget.ChildIndex} -> {tType.Name}:{field.Name} ({(Entity)toSet})");
                    }
                    else
                    {
                        //Find the first Entity type within only 1 level with the attribute.
                        var innerValue = field.GetValue(data);
                        foreach (var innerField in fieldType.GetFields(BindingFlags.Instance | BindingFlags.Public))
                        {
                            if (innerField.FieldType == typeof(Entity) && Attribute.IsDefined(innerField, typeof(EntityGatherTargetAttribute)))
                            {
                                innerField.SetValue(innerValue, toSet);
                                object dataBox = data;
                                field.SetValue(dataBox, innerValue);
                                data = (T)dataBox;
                                //Debug.Log($"Successfully gather entity for parent GO {parent.name} at index {gatherTarget.ChildIndex} -> {tType.Name}:{field.Name}.{innerField.Name} ({toSet})");
                            }
                        }
                    }
                }
            }
            EntityManager.SetComponentData<T>(parent.Entity, data);
        }
    }
}