using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using System.Collections.Generic;
using UnityEngine;

namespace E7.ECS
{
    public static class ActiveWorld
    {
        public static void Issue<ReactiveComponent,ReactiveGroup>()
        where ReactiveComponent : struct, IMessage
        where ReactiveGroup : struct, IMessageGroup
        => World.Active.GetExistingManager<EntityManager>().Message<ReactiveComponent, ReactiveGroup>();
    }
}