// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;

namespace Improbable.Util.Injection
{
    public class InjectionCache
    {
        // For each type, holds the list of members that contain at least one of the attributes passed.
        private readonly IDictionary<Type, IList<IMemberAdapter>> injectableMembersCache = new Dictionary<Type, IList<IMemberAdapter>>();

        // For each type, contains a mapping from an injectee type to a member that should be injected to. Null if the dictionary would have been empty.
        private readonly IDictionary<Type, IDictionary<Type, IMemberAdapter>> memberInjectionCache = new Dictionary<Type, IDictionary<Type, IMemberAdapter>>();
        private readonly HashSet<Type> erroneousTypes = new HashSet<Type>();
        private readonly MemberReflectionUtil memberReflectionUtil;

        /// <param name="injectionAttributeTypes">Attributes that mark injectable members.</param>
        public InjectionCache(params Type[] injectionAttributeTypes)
        {
            memberReflectionUtil = new MemberReflectionUtil(injectionAttributeTypes);
        }

        public void RegisterType(Type classType)
        {
            if (classType == null || injectableMembersCache.ContainsKey(classType))
            {
                return;
            }

            RegisterType(classType.BaseType);
            if (erroneousTypes.Contains(classType.BaseType))
            {
                CacheUninjectableType(classType);
                throw new ArgumentException("Type {0} cannot be registered as one of its base classes registered a failure.", classType.FullName);
            }

            var membersToInject = memberReflectionUtil.GetMembersWithMatchingAttributes(classType);
            ConcatLists(ref membersToInject, classType.BaseType == null ? null : injectableMembersCache[classType.BaseType]);

            var cache = CreateMemberInjectionCache(classType, membersToInject);
            injectableMembersCache.Add(classType, membersToInject);
            memberInjectionCache.Add(classType, cache);
        }

        public void Inject(object targetOfInjection, Type typeOfInjectee, object injectee)
        {
            if (!TryInject(targetOfInjection, typeOfInjectee, injectee))
            {
                throw new ArgumentException(
                                            string.Format(
                                                          "Could not find a member in '{0}' to inject '{1}' (of type '{2}') into.",
                                                          targetOfInjection.GetType(),
                                                          injectee,
                                                          injectee != null ? injectee.GetType().ToString() : "null"
                                                         )
                                           );
            }
        }

        public bool TryInject(object targetOfInjection, Type typeOfInjectee, object injectee)
        {
            var memberToInjectInto = GetAdapterForType(targetOfInjection.GetType(), typeOfInjectee);
            if (memberToInjectInto != null)
            {
                memberToInjectInto.SetValue(targetOfInjection, injectee);
            }

            return memberToInjectInto != null;
        }

        public IMemberAdapter GetAdapterForType(Type targetType, Type typeOfInjectee)
        {
            RegisterType(targetType);
            var cache = memberInjectionCache[targetType];
            IMemberAdapter memberToInjectInto = null;
            if (cache == null || cache.TryGetValue(typeOfInjectee, out memberToInjectInto))
            {
                return memberToInjectInto;
            }

            var injectableMembers = injectableMembersCache[targetType];
            for (int i = 0; i < injectableMembers.Count; i++)
            {
                if (injectableMembers[i].TypeOfMember.IsAssignableFrom(typeOfInjectee))
                {
                    memberToInjectInto = injectableMembers[i];
                    break;
                }
            }

            cache.Add(typeOfInjectee, memberToInjectInto);
            return memberToInjectInto;
        }

        public IList<IMemberAdapter> GetAdapters(object injectionTarget)
        {
            return GetAdaptersForType(injectionTarget.GetType());
        }

        public IList<IMemberAdapter> GetAdaptersForType(Type injectionTarget)
        {
            RegisterType(injectionTarget);
            return injectableMembersCache[injectionTarget];
        }

        private void CacheUninjectableType(Type targetType)
        {
            injectableMembersCache.Add(targetType, null);
            memberInjectionCache.Add(targetType, null);
            erroneousTypes.Add(targetType);
        }

        private IDictionary<Type, IMemberAdapter> CreateMemberInjectionCache(Type classType, IList<IMemberAdapter> injectableMembers)
        {
            if (injectableMembers == null)
            {
                return null;
            }

            var result = new Dictionary<Type, IMemberAdapter>();
            for (int i = 0; i < injectableMembers.Count; ++i)
            {
                var typeToInject = injectableMembers[i].TypeOfMember;
                if (!result.ContainsKey(typeToInject))
                {
                    result.Add(typeToInject, injectableMembers[i]);
                }
                else
                {
                    CacheUninjectableType(classType);
                    throw new ArgumentException(string.Format("More than one member in '{0}' inject the type '{1}'.", classType, typeToInject));
                }
            }

            return result;
        }

        private static void ConcatLists<T>(ref IList<T> listA, IList<T> listB)
        {
            if (listA == null)
            {
                listA = listB;
                return;
            }

            if (listB == null)
            {
                return;
            }

            for (int i = 0; i < listB.Count; ++i)
            {
                listA.Add(listB[i]);
            }
        }
    }
}
