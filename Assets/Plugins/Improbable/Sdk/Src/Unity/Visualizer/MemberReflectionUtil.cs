// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Improbable.Util.Injection
{
    class MemberReflectionUtil
    {
        private readonly Type[] attributeTypes;

        private const BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.NonPublic |
                                                BindingFlags.Public | BindingFlags.DeclaredOnly;

        private const BindingFlags PropertyFlags =
            FieldFlags | BindingFlags.SetProperty | BindingFlags.GetProperty;

        internal MemberReflectionUtil(Type[] attributeTypes)
        {
            this.attributeTypes = attributeTypes;
        }

        /// <returns>
        ///     A MemberAdapter list of properties and fields declared in given type that match at least one of given attributes.
        /// </returns>
        internal IList<IMemberAdapter> GetMembersWithMatchingAttributes(Type targetType)
        {
            IList<IMemberAdapter> result = null;
            ProcessMembers(ref result, targetType.GetProperties(PropertyFlags), CreatePropertyAdapter);
            ProcessMembers(ref result, targetType.GetFields(FieldFlags), CreateFieldAdapter);
            return result;
        }

        private void ProcessMembers<T>(ref IList<IMemberAdapter> resultList, IList<T> members, Func<T, IMemberAdapter> adapterFactory) where T : MemberInfo
        {
            for (int i = 0; i < members.Count; i++)
            {
                if (MemberHasAttribute(members[i]))
                {
                    if (resultList == null)
                    {
                        resultList = new List<IMemberAdapter>();
                    }

                    resultList.Add(adapterFactory(members[i]));
                }
            }
        }

        private bool MemberHasAttribute(MemberInfo member)
        {
            for (int i = 0; i < attributeTypes.Length; i++)
            {
                if (Attribute.IsDefined(member, attributeTypes[i], false))
                {
                    return true;
                }
            }

            return false;
        }

        private static FieldInfoAdapter CreateFieldAdapter(FieldInfo member)
        {
            return new FieldInfoAdapter(member);
        }

        private static PropertyInfoAdapter CreatePropertyAdapter(PropertyInfo member)
        {
            return new PropertyInfoAdapter(member);
        }
    }
}
