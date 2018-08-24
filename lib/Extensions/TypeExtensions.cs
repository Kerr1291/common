using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System;

namespace nv
{
    public static class TypeExtensions
    {
        public static IEnumerable<MemberInfo> GetPublicMembers(this Type t)
        {
            var publicMembers = t.GetMembers(BindingFlags.Public | BindingFlags.Instance).Select(x => x);
            return publicMembers;
        }

        public static IEnumerable<MemberInfo> GetNonpublicMembers(this Type t)
        {
            var nonpublicMembers = t.GetMembers(BindingFlags.NonPublic | BindingFlags.Instance).Select(x => x);
            return nonpublicMembers;
        }

        public static IEnumerable<MemberInfo> GetInstanceMembers(this Type t, bool filterObsolete = true)
        {
            var publicMembers = t.GetPublicMembers();
            var nonpublicMembers = t.GetNonpublicMembers();
            var allMembers = publicMembers.Concat(nonpublicMembers);

            //filter out generic methods and generic special property methods
            allMembers = allMembers.Where(x => ((x as MethodInfo) == null) || (((x as MethodInfo) != null) && !((x as MethodInfo).IsSpecialName) && !((x as MethodInfo).ContainsGenericParameters)));

            if(filterObsolete)
            {
#if NET_4_6
                allMembers = allMembers.Where(x => x.GetCustomAttribute(typeof(ObsoleteAttribute)) == null).ToList();
#else
                allMembers = allMembers.Where(x =>
                {
                    var customAttributes = x.GetCustomAttributes();
                    bool hasObsolete = customAttributes.Where(y => y.GetType().IsAssignableFrom(typeof(ObsoleteAttribute))).Any();
                    return !hasObsolete;
                });
#endif
            }

            return allMembers;
        }
    }
}