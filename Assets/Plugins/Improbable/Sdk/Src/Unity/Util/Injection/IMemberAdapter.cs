// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Reflection;

namespace Improbable.Util.Injection
{
    public interface IMemberAdapter
    {
        void SetValue(object obj, object value);
        object GetValue(object obj);
        Type TypeOfMember { get; }
        MemberInfo Member { get; }
    }
}
