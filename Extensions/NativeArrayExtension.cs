using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using System.Collections.Generic;

namespace E7.ECS
{
    public static class NativeArrayExtension
    {
        public static List<T> CopyToList<T>(this NativeArray<T> cda) where T : struct, IComponentData
        {
            using (var na = new NativeArray<T>(cda.Length, Allocator.Temp))
            {
                cda.CopyTo(na);
                List<T> list = new List<T>(na);
                return list;
            }
        }

        public static List<T> CopyToList<T>(this NativeArray<T> cda, IComparer<T> sorting) where T : struct, IComponentData
        {
            var list = CopyToList<T>(cda);
            list.Sort(sorting);
            return list;
        }
    }
}