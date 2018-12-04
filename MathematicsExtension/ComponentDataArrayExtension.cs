using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using System.Collections.Generic;

namespace E7.ECS
{
    public static class ComponentDataArrayExtension
    {
        /// <summary>
        /// Like `EntityArray.ToArray` but for CDA.
        /// </summary>
        public static List<T> CopyToList<T>(this ComponentDataArray<T> cda) where T : struct, IComponentData
        {
            using (var na = new NativeArray<T>(cda.Length, Allocator.Temp))
            {
                cda.CopyTo(na);
                List<T> list = new List<T>(na);
                return list;
            }
        }

        /// <summary>
        /// Like `EntityArray.ToArray` but for CDA.
        /// </summary>
        public static List<T> CopyToList<T>(this ComponentDataArray<T> cda, IComparer<T> sorting) where T : struct, IComponentData
        {
            var list = CopyToList<T>(cda);
            list.Sort(sorting);
            return list;
        }
    }
}