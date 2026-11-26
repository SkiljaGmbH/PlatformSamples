using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using SampleActivity.Settings;
using STG.Common.DTO;
using STG.Common.Interfaces;
using STG.Common.Interfaces.Activities;
using STG.RT.API.Activity;
using STG.RT.API.Document;

namespace SampleActivity
{
    public class SampleWithHeartbeat : STGUnattendedAbstract<SampleWithHeartbeatSettings>, ISTGActivityNotification
    {
        //Terminate is called from another thread than Process, and the simulated work can wait on the event
        readonly ManualResetEventSlim _terminateSignal = new ManualResetEventSlim(false);
        PlatformHeartbeatDelegate _platformHeartbeat;
        int _heartbeatCount;

        public void SetPlatformHeartbeatDelegate(PlatformHeartbeatDelegate platformHeartbeatDelegate)
        {
            _platformHeartbeat = platformHeartbeatDelegate;
        }

        public void Terminate()
        {
            //must return quickly, so only raise the signal and let Process stop on its own
            _terminateSignal.Set();
        }

        public override void Process(DtoWorkItemData workItemInProgress, STGDocument document)
        {
            _heartbeatCount = 0;

            //the settings cannot restrict the configured values, so they are validated before they are used
            var processingTime = TimeSpan.FromSeconds(
                GetValidatedSeconds(
                    nameof(SampleWithHeartbeatSettings.ProcessingDurationSeconds),
                    ActivityConfiguration.ProcessingDurationSeconds,
                    1,
                    3600,
                    180)
                );

            var heartbeatInterval = TimeSpan.FromSeconds(
                GetValidatedSeconds(
                    nameof(SampleWithHeartbeatSettings.HeartbeatIntervalSeconds),
                    ActivityConfiguration.HeartbeatIntervalSeconds,
                    1,
                    59,
                    20)
                );

            if (_platformHeartbeat == null)
            {
                //the host does not support the heartbeat
                Log?.Warn("No platform heartbeat delegate was set; the work item may be released while it is still being processed.");
            }

            var sw = Stopwatch.StartNew();

            //real long running work is usually a single blocking call that occupies this thread, so the heartbeat is
            //reported from a timer callback on a thread pool thread instead
            var heartbeatTimer = new Timer(_ => ReportHeartbeat(workItemInProgress), null, heartbeatInterval, heartbeatInterval);

            try
            {
                SimulateBlockingWork(processingTime);
            }
            finally
            {
                //a plain Dispose() returns before a callback that is already queued on the thread pool has run, and
                //that callback would report a heartbeat for a work item that is no longer being processed
                using (var allCallbacksDone = new ManualResetEvent(false))
                {
                    //false means the timer was already disposed, and then the handle is never signalled
                    if (heartbeatTimer.Dispose(allCallbacksDone))
                    {
                        allCallbacksDone.WaitOne();
                    }
                }
            }

            sw.Stop();

            if (_terminateSignal.IsSet)
            {
                //throwing lets the platform put the work item back so the next host processes it from the beginning
                throw new OperationCanceledException("Processing was stopped because the host is shutting down. The work item is left unprocessed.");
            }

            document.AddCustomValue("SampleWithHeartbeat", DateTime.Now.ToString("yyyyMMddHHmmss"), true);
            document.AddCustomValue("ProcessingTime", sw.Elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture), false);
            document.AddCustomValue("ReportedHeartbeats", _heartbeatCount.ToString(), false);
        }

        void ReportHeartbeat(DtoWorkItemData workItemInProgress)
        {
            try
            {
                var heartbeat = _platformHeartbeat;
                if (heartbeat == null)
                {
                    return;
                }

                var count = Interlocked.Increment(ref _heartbeatCount);
                heartbeat(workItemInProgress, $"{HeartbeatMessage} ({count})");
            }
            catch (Exception ex)
            {
                //an exception leaving a timer callback terminates the host process
                Log?.Warn("Failed to report the heartbeat to the platform", ex);
            }
        }

        //A Range attribute on the settings is not supported, so a value outside the supported range reaches the
        //activity unchanged. The activity has to handle it instead of failing, and falls back to the default
        int GetValidatedSeconds(string settingName, int configuredValue, int minimum, int maximum, int defaultValue)
        {
            if (configuredValue >= minimum && configuredValue <= maximum)
            {
                return configuredValue;
            }
            else
            {
                Log?.Warn($"{settingName} is configured as {configuredValue} seconds, which is outside the supported range of {minimum} to {maximum} seconds. The default of {defaultValue} seconds is used instead.");
                return defaultValue;
            }
        }

        void SimulateBlockingWork(TimeSpan processingTime)
        {
            //a real blocking call cannot be interrupted like this; there the activity would check the terminate
            //signal after the call returns, or use the cancellation the called component offers
            _terminateSignal.Wait(processingTime);
        }

        string HeartbeatMessage =>
            string.IsNullOrWhiteSpace(ActivityConfiguration.HeartbeatMessage)
            ? SampleWithHeartbeatSettings.DefaultHeartbeatMessage
            : ActivityConfiguration.HeartbeatMessage;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _terminateSignal.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
