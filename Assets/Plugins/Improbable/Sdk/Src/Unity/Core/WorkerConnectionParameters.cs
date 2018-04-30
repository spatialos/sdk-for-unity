// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using Improbable.Worker;

namespace Improbable.Unity.Core
{
    public class WorkerConnectionParameters
    {
        public ConnectionParameters ConnectionParameters;
        public LocatorParameters LocatorParameters;
        public string LocatorHost;
        public string ReceptionistHost;
        public ushort ReceptionistPort;
        public string WorkerId;
    }
}
