// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections;
using Improbable.Worker;
using UnityEngine;

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     This class is responsible for managing the connection to SpatialOS.
    /// </summary>
    /// <remarks>
    ///     All methods and properties must be called or accessed from the game thread.
    /// </remarks>
    public class WorkerConnection
    {
        private const int ReturnImmediatelyMillis = 0;

        /// <summary>
        ///     True if a valid connection exists to SpatialOS.
        /// </summary>
        [Obsolete("Use SpatialOS.IsConnected instead.")]
        public static bool IsConnected
        {
            get { return SpatialOS.IsConnected; }
        }

        /// <summary>
        ///     The connection to SpatialOS.
        /// </summary>
        /// <remarks>
        ///     It is invalid to access this property if Connect has not been called first.
        /// </remarks>
        [Obsolete("Use SpatialOS.Connection instead.")]
        public static Connection Connection
        {
            get { return SpatialOS.Connection; }
        }

        /// <summary>
        ///     Provides callbacks for events related to the connection and updates.
        /// </summary>
        /// <remarks>
        ///     It is invalid to access this property if Connect has not been called first.
        /// </remarks>
        [Obsolete("Use SpatialOS.Dispatcher instead.")]
        public static Dispatcher Dispatcher
        {
            get { return SpatialOS.Dispatcher; }
        }

        /// <summary>
        ///     Returns the deployment selected for connection, or null if no deployment was selected.
        /// </summary>
        /// <remarks>
        ///     It is invalid to access this property if Connect has not been called first.
        /// </remarks>
        [Obsolete("Use SpatialOS.Deployment instead.")]
        public static Deployment? Deployment
        {
            get { return SpatialOS.Deployment; }
        }

        /// <summary>
        ///     Processes messages from the SpatialOS connection and triggers callbacks.
        ///     This should be called periodically, typically every frame.
        /// </summary>
        [Obsolete("Use SpatialOS.ProcessEvents instead.")]
        public static void ProcessEvents()
        {
            SpatialOS.ProcessEvents();
        }

        /// <summary>
        ///     Call this to initiate a connection to SpatialOS, using settings specified in <c>WorkerConfiguration</c>.
        /// </summary>
        /// <remarks>
        ///     Is it invalid to call this method if IsConnected is true.
        /// </remarks>
        public static IEnumerator ConnectAsync(WorkerConnectionParameters parameters, Deployment? deployment, Action<Connection> onConnection)
        {
            if (SpatialOS.Connection != null)
            {
                throw new InvalidOperationException("ConnectAsync called while already connected");
            }

            IEnumerator connect;

            if (deployment.HasValue)
            {
                connect = ConnectToLocatorAsync(parameters, deployment.Value, onConnection);
            }
            else
            {
                connect = ConnectToReceptionistAsync(parameters, onConnection);
            }

            yield return connect;
        }

        /// <summary>
        ///     Whether connected or not, this method indicates whether any resources can be disposed.
        /// </summary>
        [Obsolete]
        public static bool CanDispose()
        {
            return false;
        }

        /// <summary>
        ///     Call this to free all resources created by a call to Connect.
        ///     Every call to Connect should always have a matching call to Dispose.
        /// </summary>
        /// <remarks>
        ///     It is invalid to call this method if Connect has not been called first.
        ///     Calling this method invalidates access to some properties on this class.
        /// </remarks>
        [Obsolete]
        public static void Dispose() { }

        /// <summary>
        ///     Returns a list of deployments associated with the current ProjectName.
        /// </summary>
        /// <returns>
        ///     A list of available deployments.
        /// </returns>
        /// <remarks>
        ///     Is it invalid to call this method again before the listReceived callback is invoked.
        /// </remarks>
        public static IEnumerator GetDeploymentListAsync(WorkerConnectionParameters parameters, Action<DeploymentList, Action<Deployment>> listReceived, Action<Deployment> handleDeploymentChosen)
        {
            var locator = CreateLocator(parameters);
            var future = locator.GetDeploymentListAsync();

            // ReSharper disable once AccessToDisposedClosure
            var wait = new WaitUntil(() => future.Get(ReturnImmediatelyMillis).HasValue);
            yield return wait;

            listReceived(future.Get(), handleDeploymentChosen);

            future.Dispose();
            locator.Dispose();
        }

        private static bool OnQueueStatusThunk(QueueStatus status)
        {
            if (SpatialOS.OnQueueStatus != null)
            {
                SpatialOS.OnQueueStatus(status);
            }

            return !SpatialOS.Disconnecting;
        }

        private static Locator CreateLocator(WorkerConnectionParameters parameters)
        {
            return new Locator(parameters.LocatorHost, parameters.LocatorParameters);
        }

        private static IEnumerator ConnectToLocatorAsync(WorkerConnectionParameters parameters, Deployment deployment, Action<Connection> onConnection)
        {
            var locator = CreateLocator(parameters);
            var connectionFuture = locator.ConnectAsync(deployment.DeploymentName, parameters.ConnectionParameters, OnQueueStatusThunk);

            // ReSharper disable once AccessToDisposedClosure
            var wait = new WaitUntil(() => connectionFuture.Get(ReturnImmediatelyMillis).HasValue);
            yield return wait;

            onConnection(connectionFuture.Get());

            connectionFuture.Dispose();
            locator.Dispose();
        }

        private static IEnumerator ConnectToReceptionistAsync(WorkerConnectionParameters parameters, Action<Connection> onConnection)
        {
            var connectionFuture = Worker.Connection.ConnectAsync(parameters.ReceptionistHost, parameters.ReceptionistPort, parameters.WorkerId, parameters.ConnectionParameters);

            // ReSharper disable once AccessToDisposedClosure
            var wait = new WaitUntil(() => connectionFuture.Get(ReturnImmediatelyMillis).HasValue);
            yield return wait;

            onConnection(connectionFuture.Get());
            connectionFuture.Dispose();
        }
    }
}
