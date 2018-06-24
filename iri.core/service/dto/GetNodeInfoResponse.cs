using iri.core.model;

namespace iri.core.service.dto
{
    public class GetNodeInfoResponse: AbstractResponse
    {
        public static AbstractResponse Create(
            string appName, string appVersion, int jreAvailableProcessors,
            long jreFreeMemory,
            string jreVersion, long maxMemory, long totalMemory, 
            Hash latestMilestone, int latestMilestoneIndex,
            Hash latestSolidSubtangleMilestone, int latestSolidSubtangleMilestoneIndex,
            int neighbors, int packetsQueueSize,
            long currentTimeMillis, int tips, int numberOfTransactionsToRequest)
        {
            GetNodeInfoResponse res = new GetNodeInfoResponse
            {
                AppName = appName,
                AppVersion = appVersion,
                JreAvailableProcessors = jreAvailableProcessors,
                JreFreeMemory = jreFreeMemory,
                JreVersion = jreVersion,

                JreMaxMemory = maxMemory,
                JreTotalMemory = totalMemory,
                LatestMilestone = latestMilestone.ToString(),
                LatestMilestoneIndex = latestMilestoneIndex,

                LatestSolidSubtangleMilestone = latestSolidSubtangleMilestone.ToString(),
                LatestSolidSubtangleMilestoneIndex = latestSolidSubtangleMilestoneIndex,

                Neighbors = neighbors,
                PacketsQueueSize = packetsQueueSize,
                Time = currentTimeMillis,
                Tips = tips,
                TransactionsToRequest = numberOfTransactionsToRequest

            };

            return res;
        }

        public string AppName { get; private set; }
        public string AppVersion { get; private set; }
        public int JreAvailableProcessors { get; private set; }
        public long JreFreeMemory { get; private set; }
        public string JreVersion { get; private set; }

        public long JreMaxMemory { get; private set; }
        public long JreTotalMemory { get; private set; }
        public string LatestMilestone { get; private set; }
        public int LatestMilestoneIndex { get; private set; }

        public string LatestSolidSubtangleMilestone { get; private set; }
        public int LatestSolidSubtangleMilestoneIndex { get; private set; }

        public int Neighbors { get; private set; }
        public int PacketsQueueSize { get; private set; }
        public long Time { get; private set; }
        public int Tips { get; private set; }
        public int TransactionsToRequest { get; private set; }
    }
}
