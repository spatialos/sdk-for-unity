// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

namespace Improbable.Unity.Entity
{
    // #SNIPPET_START enum
    /// <summary>
    ///     Enumeration of modifications that can be made to the calculated component interest.
    /// </summary>
    /// <remarks>See <seealso cref="IWorkerComponentInterestOverrider" /> for more information about component interest.</remarks>
    public enum WorkerComponentInterest
    {
        /// <summary>
        ///     No override - use the calculated interest.
        /// </summary>
        Default,

        /// <summary>
        ///     Always be interested in a component, even if it hasn't been calculated.
        /// </summary>
        Always,

        /// <summary>
        ///     Never be interested in a component, even if it's been calculated.
        /// </summary>
        Never
    }

    // #SNIPPET_END enum
}
