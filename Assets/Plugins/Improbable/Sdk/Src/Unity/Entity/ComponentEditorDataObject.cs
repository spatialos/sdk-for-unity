// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Text;
using Improbable.Unity.Util;

namespace Improbable.Unity.CodeGeneration
{
    /// <summary>
    ///     Class for bridging SpatialOS component classes with component editors in Unity Editor.
    /// </summary>
    /// <typeparam name="TComponent"></typeparam>
    public class ComponentEditorDataObject<TComponent> : IComponentEditorDataObject<TComponent>
        where TComponent : class, ICanAttachEditorDataObject
    {
        private const int EditorLogSize = 20;
        private const int EditorSpeedBuckets = 10;

        private const string UpdateLogFormat = "{0:HH:mm:ss}:{1}";
        private const string ComponentUpdateFormat = " {0}={1};";
        private const string CommandRequestFormat = "{0:HH:mm:ss}: {1}: {2}";

        protected TComponent Component;

        private string[] componentUpdateLogArray = new string[EditorLogSize];
        private string[] commandRequestLogArray = new string[EditorLogSize];

        private readonly CircularBuffer<string> componentUpdateLog = new CircularBuffer<string>(EditorLogSize);
        private readonly CircularBuffer<string> commandRequestLog = new CircularBuffer<string>(EditorLogSize);

        private readonly CircularIntBuffer commandRequestsPerSecond = new CircularIntBuffer(EditorSpeedBuckets);
        private readonly CircularIntBuffer componentUpdatesPerSecond = new CircularIntBuffer(EditorSpeedBuckets);

        private int componentUpdatesSinceLastUpdate;
        private int commandRequestsSinceLastUpdate;

        internal double averageComponentUpdatesPerSecond;
        internal double averageCommandRequestsPerSecond;

        private readonly StringBuilder fieldLogs = new StringBuilder();
        private bool updatePending;

        private DateTime lastUpdated = DateTime.MinValue;
        private readonly TimeSpan updatePeriod = TimeSpan.FromSeconds(1);

        #region Unity Editor foldout status

#pragma warning disable 649
        public bool ShowUpdates;
        public bool ShowCommands;
#pragma warning restore 649

        #endregion

        /// <summary>
        ///     Attaches SpatialOS generated component to this ComponentEditorDataObject.
        /// </summary>
        public void AttachComponent(TComponent component)
        {
            Component = component;
            Component.AttachEditorDataObject(this);
        }

        /// <summary>
        ///     Releases the reference to the component.
        /// </summary>
        public void DetachComponent()
        {
            Component.RemoveEditorDataObject();
            Component = null;
        }

        /// <summary>
        ///     Called regularly to calculate wall time - based stats.
        /// </summary>
        public void UpdateEditorLogs()
        {
            var now = DateTime.UtcNow;
            var timeSinceLastUpdate = now - lastUpdated;
            if (now - lastUpdated < updatePeriod)
            {
                return;
            }

            componentUpdatesPerSecond.Add((int) System.Math.Round(componentUpdatesSinceLastUpdate / timeSinceLastUpdate.TotalSeconds));
            componentUpdatesSinceLastUpdate = 0;
            averageComponentUpdatesPerSecond = componentUpdatesPerSecond.GetAverage();

            commandRequestsPerSecond.Add((int) System.Math.Round(commandRequestsSinceLastUpdate / timeSinceLastUpdate.TotalSeconds));
            commandRequestsSinceLastUpdate = 0;
            averageCommandRequestsPerSecond = commandRequestsPerSecond.GetAverage();

            lastUpdated = now;
        }

        /// <summary>
        ///     Logs component update.
        /// </summary>
        /// <remarks>
        ///     Component update will only propagate to the log when SendUpdateLog() is called.
        /// </remarks>
        public void LogComponentUpdate(string componentName, object componentValue)
        {
            updatePending = true;

            fieldLogs.AppendFormat(ComponentUpdateFormat, componentName, componentValue);
        }

        /// <summary>
        ///     Logs command request.
        /// </summary>
        /// <remarks>
        ///     Component update will only propagate to the log when SendUpdateLog() is called.
        /// </remarks>
        public void LogCommandRequest(DateTime dateTime, string commandName, object payload)
        {
            updatePending = true;

            var requestLog = string.Format(CommandRequestFormat, dateTime, commandName, payload);
            commandRequestLog.Add(requestLog);
            commandRequestLog.GetItemsInMostRecentOrder(ref commandRequestLogArray);
            ++commandRequestsSinceLastUpdate;
        }

        /// <summary>
        ///     Writes the combined update message to the log.
        /// </summary>
        public void SendUpdateLog()
        {
            if (!updatePending)
            {
                return;
            }

            var updateLog = string.Format(UpdateLogFormat, DateTime.Now, fieldLogs);
            componentUpdateLog.Add(updateLog);
            componentUpdateLog.GetItemsInMostRecentOrder(ref componentUpdateLogArray);
            ++componentUpdatesSinceLastUpdate;

            // Reuse fieldLogs.
            fieldLogs.Length = 0;
            updatePending = false;
        }

        /// <summary>
        ///     Component update logs.
        /// </summary>
        public string[] ComponentUpdateLogArray
        {
            get { return componentUpdateLogArray; }
        }

        /// <summary>
        ///     Command request logs.
        /// </summary>
        public string[] CommandRequestLogArray
        {
            get { return commandRequestLogArray; }
        }

        /// <summary>
        ///     Average component updates per second.
        /// </summary>
        public double AverageComponentUpdatesPerSecond
        {
            get { return averageComponentUpdatesPerSecond; }
        }

        /// <summary>
        ///     Average command requests per second.
        /// </summary>
        public double AverageCommandRequestsPerSecond
        {
            get { return averageCommandRequestsPerSecond; }
        }
    }
}
