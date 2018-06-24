namespace iri.core.service.dto
{
    public class GetNeighborsResponse: AbstractResponse
    {
        public static AbstractResponse CreateForTest()
        {
            //"address": "IOTAserver.raganius.com:14600",
            //"numberOfAllTransactions": 1537080,
            //"numberOfRandomTransactionRequests": 104364,
            //"numberOfNewTransactions": 488,
            //"numberOfInvalidTransactions": 0,
            //"numberOfSentTransactions": 3398709,
            //"connectionType": "udp"
            Neighbor neighbor = new Neighbor
            {
                Address = "IOTAserver.raganius.com:14600",
                NumberOfAllTransactions = 1537080,
                NumberOfRandomTransactionRequests = 104364,
                NumberOfNewTransactions = 488,
                NumberOfInvalidTransactions = 0,
                NumberOfSentTransactions = 3398709,
                ConnectionType = "udp"
            };


            GetNeighborsResponse res = new GetNeighborsResponse {Neighbors = new Neighbor[1]};
            res.Neighbors[0] = neighbor;

            return res;
        }

        public Neighbor[] Neighbors { get; private set; }

        public class Neighbor
        {
            public string Address { get; set; }
            public long NumberOfAllTransactions { get; set; }
            public long NumberOfRandomTransactionRequests { get; set; }
            public long NumberOfNewTransactions { get; set; }
            public long NumberOfInvalidTransactions { get; set; }
            public long NumberOfSentTransactions { get; set; }
            public string ConnectionType { get; set; }
        }
    }
}
