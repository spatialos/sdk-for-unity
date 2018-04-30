// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using Improbable.Collections;
using Improbable.Worker;

namespace Improbable.Unity.Metrics
{
    /// <summary>
    ///     This class is responsible for all of the metrics tracked by the application.
    /// </summary>
    /// <remarks>
    ///     It is not safe to access any members of this class from any thread other than the main thread.
    /// </remarks>
    public class WorkerMetrics
    {
        private readonly Worker.Metrics instance = new Worker.Metrics();

        /// <summary>
        ///     Provides access to the underlying metrics object.
        /// </summary>
        public Worker.Metrics RawMetrics
        {
            get { return instance; }
        }

        /// <summary>
        ///     Provides access to user-defined gauges.
        /// </summary>
        public Map<string, double> Gauges
        {
            get { return instance.GaugeMetrics; }
        }

        /// <summary>
        ///     Provides access to user-defined histograms.
        /// </summary>
        public Map<string, HistogramMetric> Histograms
        {
            get { return instance.HistogramMetrics; }
        }

        /// <summary>
        ///     Sets the current worker load metric.
        /// </summary>
        /// <param name="load"></param>
        public void SetLoad(double load)
        {
            instance.Load = load;
        }

        /// <summary>
        ///     Increments the specified gauge by 1.
        /// </summary>
        /// <remarks>
        ///     If a gauge does not already exist, a new zeroed gauge will be created and then incremented.
        /// </remarks>
        public void IncrementGauge(string gaugeName)
        {
            IncrementGaugeBy(gaugeName, 1);
        }

        /// <summary>
        ///     Decrements the specified gauge by 1.
        /// </summary>
        /// <remarks>
        ///     If a gauge does not already exist, a new zeroed gauge will be created and then incremented.
        /// </remarks>
        public void DecrementGauge(string gaugeName)
        {
            IncrementGaugeBy(gaugeName, -1);
        }

        /// <summary>
        ///     Sets the specified gauge to the value.
        /// </summary>
        /// <remarks>
        ///     If a gauge does not already exist, a new zeroed gauge will be created.
        /// </remarks>
        public void SetGauge(string gaugeName, double newValue)
        {
            instance.GaugeMetrics[gaugeName] = newValue;
        }

        /// <summary>
        ///     Increments the specified gauge by the specified amount.
        /// </summary>
        /// <remarks>
        ///     If a gauge does not already exist, a new zeroed gauge will be created and then incremented.
        /// </remarks>
        public void IncrementGaugeBy(string gaugeName, int amount)
        {
            if (instance.GaugeMetrics.ContainsKey(gaugeName))
            {
                instance.GaugeMetrics[gaugeName] += amount;
            }
            else
            {
                instance.GaugeMetrics[gaugeName] = amount;
            }
        }
    }
}
