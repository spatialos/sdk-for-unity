// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     An interface to the old style of sending commands from workers.
    /// </summary>
    public interface IWorkerCommandSender : IDescriptorCommander, IWorldCommander { }
}
