// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using Improbable.Unity.Core;
using Improbable.Worker;

namespace UnityEngine
{
    public static class GameObjectAuthorityExtensions
    {
        /// <summary>
        ///     Returns true if the GameObject has authority over the specified component.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     If the GameObject is not a SpatialOS entity.
        /// </exception>
        [Obsolete("Deprecated: Please use \"GetAuthority<T>() == Improbable.Worker.Authority.Authoritative || GetAuthority<T>() == Improbable.Worker.Authority.AuthorityLossImminent\" instead.")]
        public static bool HasAuthority<T>(this GameObject obj) where T : IComponentMetaclass
        {
            var authority = obj.GetAuthority<T>();
            return authority == Authority.Authoritative || authority == Authority.AuthorityLossImminent;
        }

        /// <summary>
        ///     Returns true if the GameObject has authority over the specified component.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     If the GameObject is not a SpatialOS entity.
        /// </exception>
        [Obsolete("Deprecated: Please use \"GetAuthority(componentId) == Improbable.Worker.Authority.Authoritative || GetAuthority(componentId) == Improbable.Worker.Authority.AuthorityLossImminent\" instead.")]
        public static bool HasAuthority(this GameObject obj, uint componentId)
        {
            var authority = obj.GetAuthority(componentId);
            return authority == Authority.Authoritative || authority == Authority.AuthorityLossImminent;
        }

        /// <summary>
        ///     Returns the authority state of the GameObject over the specified component.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     If the GameObject is not a SpatialOS entity, or the authority for this entity could not be accessed.
        /// </exception>
        public static Authority GetAuthority<T>(this GameObject obj) where T : IComponentMetaclass
        {
            return obj.GetAuthority(Dynamic.GetComponentId<T>());
        }

        /// <summary>
        ///     Returns the authority state of the GameObject over the specified component.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     If the GameObject is not a SpatialOS entity, or the authority for this entity could not be accessed.
        /// </exception>
        public static Authority GetAuthority(this GameObject obj, uint componentId)
        {
            var entityObject = obj.GetSpatialOsEntity();
            if (entityObject == null)
            {
                throw new InvalidOperationException(string.Format("{0} is not a SpatialOS-related entity.", obj));
            }

            Improbable.Collections.Map<uint, Authority> authorityForComponentsOfEntity;

            if (!SpatialOS.Dispatcher.Authority.TryGetValue(entityObject.EntityId, out authorityForComponentsOfEntity))
            {
                throw new InvalidOperationException(
                                                    string.Format("Authority information for entity {0} could not be accessed.", obj));
            }

            Authority authorityForComponent;

            if (!authorityForComponentsOfEntity.TryGetValue(componentId, out authorityForComponent))
            {
                // It is possible that this worker cannot even see the component.
                // That means it certainly is not authoritative over it.
                authorityForComponent = Authority.NotAuthoritative;
            }

            return authorityForComponent;
        }
    }
}
