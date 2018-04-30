// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using Improbable.Entity.Component;
using Improbable.Worker;
using UnityEngine;

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     Implementation of the Entity Pipeline.
    /// </summary>
    public class EntityPipeline : IEntityPipelineInternal
    {
        private static readonly EntityPipeline PipelineInstance = new EntityPipeline();

        internal static IEntityPipelineInternal Internal
        {
            get { return PipelineInstance; }
        }

        public static IEntityPipeline Instance
        {
            get { return PipelineInstance; }
        }

        private static readonly EmptyEntityBlock FinalBlock = new EmptyEntityBlock();

        private IEntityPipelineBlock firstBlock;
        private IEntityPipelineBlock lastBlock;

        private bool started = false;

        private ISpatialCommunicator spatialCommunicator;
        private Connection connection;
        private Dispatcher dispatcher;
        private IList<ulong> registeredDispatcherCallbacks = new List<ulong>();

        public IEntityPipeline AddBlock(IEntityPipelineBlock block)
        {
            if (started)
            {
                throw new InvalidOperationException("Cannot add blocks after the pipeline has been started.");
            }

            if (firstBlock == null)
            {
                firstBlock = block;
            }

            if (lastBlock != null)
            {
                lastBlock.NextEntityBlock = block;
            }

            lastBlock = block;
            return this;
        }

        /// <inheritdoc />
        public void Start(ISpatialCommunicator spatialCommunicator)
        {
            if (started)
            {
                throw new InvalidOperationException("The pipeline has already been started.");
            }

            this.spatialCommunicator = spatialCommunicator;
            RegisterDispatcherCallbacks();

            // Set the empty block as the last block to prevent NREs being thrown
            // when the last block uses 'NextEntityBlock' property.
            lastBlock.NextEntityBlock = FinalBlock;

            started = true;
        }

        private void RegisterDispatcherCallbacks()
        {
            registeredDispatcherCallbacks.Add(spatialCommunicator.RegisterAddEntity(addEntityOp => firstBlock.AddEntity(new AddEntityPipelineOp { DispatcherOp = addEntityOp })));
            registeredDispatcherCallbacks.Add(spatialCommunicator.RegisterRemoveEntity(removeEntityOp => firstBlock.RemoveEntity(new RemoveEntityPipelineOp { DispatcherOp = removeEntityOp })));
            registeredDispatcherCallbacks.Add(spatialCommunicator.RegisterCriticalSection(criticalSectionOp => firstBlock.CriticalSection(new CriticalSectionPipelineOp { DispatcherOp = criticalSectionOp })));
        }

        private void UnRegisterDispatcherCallbacks()
        {
            if (spatialCommunicator == null)
            {
                return;
            }

            for (int i = 0; i < registeredDispatcherCallbacks.Count; i++)
            {
                spatialCommunicator.Remove(registeredDispatcherCallbacks[i]);
            }

            registeredDispatcherCallbacks.Clear();
        }

        /// <inheritdoc />
        public void ProcessOps()
        {
            var block = firstBlock;
            while (block != null)
            {
                try
                {
                    block.ProcessOps();
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("Exception was thrown while processing ops in block {0}.", block.GetType().Name);
                    Debug.LogException(e);
                }

                block = block.NextEntityBlock;
            }
        }

        private class RegisterHandler : Dynamic.Handler
        {
            EntityPipeline impl;

            public RegisterHandler(EntityPipeline impl)
            {
                this.impl = impl;
            }

            public void Accept<C>(C metaclass) where C : IComponentMetaclass
            {
                var factory = metaclass as IComponentFactory;
                if (factory != null)
                {
                    factory.RegisterWithConnection(impl.connection, impl.dispatcher,
                                                   impl.firstBlock.ToComponentFactoryCallbacks());
                }
                else
                {
                    Debug.LogErrorFormat("Could not register {0} as a {1}", metaclass, typeof(IComponentFactory));
                }
            }
        }

        private class UnregisterHandler : Dynamic.Handler
        {
            EntityPipeline impl;

            public UnregisterHandler(EntityPipeline impl)
            {
                this.impl = impl;
            }

            public void Accept<C>(C metaclass) where C : IComponentMetaclass
            {
                var factory = metaclass as IComponentFactory;
                if (factory != null)
                {
                    factory.UnregisterWithConnection(impl.connection, impl.dispatcher);
                }
                else
                {
                    Debug.LogErrorFormat("Could not unregister {0} as a {1}", metaclass, typeof(IComponentFactory));
                }
            }
        }

        /// <inheritdoc />
        public void RegisterComponentFactories(Connection connection, Dispatcher dispatcher)
        {
            this.connection = connection;
            this.dispatcher = dispatcher;

            if (started)
            {
                throw new InvalidOperationException("Cannot register component factories after the pipeline has been started.");
            }

            Dynamic.ForEachComponent(new RegisterHandler(this));
        }

        /// <inheritdoc />
        public void UnregisterComponentFactories()
        {
            if (connection == null || dispatcher == null)
            {
                return;
            }

            Dynamic.ForEachComponent(new UnregisterHandler(this));
        }

        public void Dispose()
        {
            UnRegisterDispatcherCallbacks();
            UnregisterComponentFactories();

            var block = firstBlock;
            while (block != null)
            {
                var disposable = block as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }

                block = block.NextEntityBlock;
            }

            firstBlock = null;
            lastBlock = null;
            started = false;

            spatialCommunicator = null;
            connection = null;
            dispatcher = null;
        }

        /// <inheritdoc />
        public bool IsEmpty
        {
            get { return firstBlock == null; }
        }
    }
}
