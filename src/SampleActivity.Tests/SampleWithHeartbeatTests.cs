using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using SampleActivity.Settings;
using STG.Common.DTO;
using STG.RT.API.Document;
using STG.RT.API.Document.Factories;

namespace SampleActivity.Tests
{
    //every test creates its own activity and document and the fixture holds no state, so the tests can run
    //concurrently instead of adding up their waiting times
    [TestFixture, Parallelizable(ParallelScope.All)]
    public class SampleWithHeartbeatTests
    {
        // Short processing times keep the tests fast; a real configuration uses more than 60 seconds
        static SampleWithHeartbeat CreateActivity(int processingDurationSeconds, int heartbeatIntervalSeconds)
        {
            return new SampleWithHeartbeat
            {
                ActivityConfiguration = new SampleWithHeartbeatSettings
                {
                    ProcessingDurationSeconds = processingDurationSeconds,
                    HeartbeatIntervalSeconds = heartbeatIntervalSeconds,
                    HeartbeatMessage = "Still processing"
                }
            };
        }

        static STGDocument CreateDocument(DtoWorkItemData workItem)
        {
            // Create document API factory for local work (without attempting to access the database)
            var documentApiFactory = new DocumentApiFactory(new DocumentFactoryOptions { WorkLocally = true });
            return documentApiFactory.CreateDocumentFactory().CreateSTGDoc(workItem);
        }

        [Test]
        public void SampleWithHeartbeat_ReportsHeartbeatWhileProcessing()
        {
            var activity = CreateActivity(processingDurationSeconds: 3, heartbeatIntervalSeconds: 1);

            var workItem = new DtoWorkItemData();
            var document = CreateDocument(workItem);

            var reported = new List<string>();
            activity.SetPlatformHeartbeatDelegate((wi, message) =>
            {
                lock (reported)
                {
                    Assert.That(wi, Is.SameAs(workItem));
                    reported.Add(message);
                }
            });

            activity.Process(workItem, document);

            lock (reported)
            {
                // the third heartbeat may not make it before processing ends, so only check the lower bound
                Assert.That(reported.Count, Is.GreaterThanOrEqualTo(2));
                Assert.That(reported[0], Does.StartWith("Still processing"));
            }

            Assert.That(document.LoadCustomValue("ReportedHeartbeats"), Is.Not.Null);
        }

        [Test]
        public void SampleWithHeartbeat_WithoutHeartbeatDelegate_ProcessesWorkItem()
        {
            // a platform without heartbeat support never calls SetPlatformHeartbeatDelegate
            var activity = CreateActivity(processingDurationSeconds: 1, heartbeatIntervalSeconds: 1);

            var workItem = new DtoWorkItemData();
            var document = CreateDocument(workItem);

            Assert.DoesNotThrow(() => activity.Process(workItem, document));
            Assert.That(document.LoadCustomValue("SampleWithHeartbeat"), Is.Not.Null);
        }

        [Test]
        public void SampleWithHeartbeat_Terminate_StopsProcessingAndLeavesWorkItemUnprocessed()
        {
            var activity = CreateActivity(processingDurationSeconds: 120, heartbeatIntervalSeconds: 1);

            var workItem = new DtoWorkItemData();
            var document = CreateDocument(workItem);

            // the platform calls Terminate from another thread than the one running Process
            var terminator = new Timer(_ => activity.Terminate(), null, TimeSpan.FromSeconds(1), Timeout.InfiniteTimeSpan);

            var stopwatch = Stopwatch.StartNew();
            using (terminator)
            {
                Assert.Throws<OperationCanceledException>(() => activity.Process(workItem, document));
            }
            stopwatch.Stop();

            Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(30)));

            // the document must not be marked as processed, so the work item is processed again later
            Assert.That(document.LoadCustomValue("SampleWithHeartbeat"), Is.Null);
        }

        [Test]
        public void SampleWithHeartbeat_AfterProcessing_NoHeartbeatIsReported()
        {
            var activity = CreateActivity(processingDurationSeconds: 1, heartbeatIntervalSeconds: 1);

            var workItem = new DtoWorkItemData();
            var document = CreateDocument(workItem);

            var reported = 0;
            activity.SetPlatformHeartbeatDelegate((wi, message) => Interlocked.Increment(ref reported));

            activity.Process(workItem, document);

            var reportedWhenProcessingEnded = Volatile.Read(ref reported);

            // Process disposes the timer, so nothing may be reported after it returns. Waiting longer than one
            // heartbeat interval is enough: a timer that was still alive would have fired within that interval
            Thread.Sleep(TimeSpan.FromMilliseconds(1500));

            Assert.That(Volatile.Read(ref reported), Is.EqualTo(reportedWhenProcessingEnded));
        }

        [Test]
        public void SampleWithHeartbeat_HeartbeatIntervalOutOfRange_FallsBackToTheDefaultInterval()
        {
            // the settings cannot restrict the configured value, so the activity has to handle it on its own
            var activity = CreateActivity(processingDurationSeconds: 3, heartbeatIntervalSeconds: 0);

            var workItem = new DtoWorkItemData();
            var document = CreateDocument(workItem);

            var reported = 0;
            activity.SetPlatformHeartbeatDelegate((wi, message) => Interlocked.Increment(ref reported));

            Assert.DoesNotThrow(() => activity.Process(workItem, document));

            // the default interval is longer than the processing time, so no heartbeat is reported at all
            Assert.That(Volatile.Read(ref reported), Is.Zero);
            Assert.That(document.LoadCustomValue("SampleWithHeartbeat"), Is.Not.Null);
        }

        [Test]
        public void SampleWithHeartbeat_WithoutHeartbeatMessage_FallsBackToTheDefaultMessage()
        {
            var activity = CreateActivity(processingDurationSeconds: 3, heartbeatIntervalSeconds: 1);
            activity.ActivityConfiguration.HeartbeatMessage = string.Empty;

            var workItem = new DtoWorkItemData();
            var document = CreateDocument(workItem);

            var reported = new List<string>();
            activity.SetPlatformHeartbeatDelegate((wi, message) =>
            {
                lock (reported)
                {
                    reported.Add(message);
                }
            });

            activity.Process(workItem, document);

            lock (reported)
            {
                Assert.That(reported, Is.Not.Empty);
                Assert.That(reported[0], Does.StartWith("Still processing"));
            }
        }
    }
}
