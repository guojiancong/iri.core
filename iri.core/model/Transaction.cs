using System;
using System.Collections.Generic;
using System.Text;
using iri.core.controllers;

namespace iri.core.model
{
    public class Transaction
    {
        public byte[] Bytes { get; set; }

        public Hash Address { get; set; }
        public Hash Bundle { get; set; }
        public Hash Trunk { get; set; }
        public Hash Branch { get; set; }
        public Hash ObsoleteTag { get; set; }
        public long Value { get; set; }
        public long CurrentIndex { get; set; }
        public long LastIndex { get; set; }
        public long Timestamp { get; set; }

        public Hash Tag { get; set; }
        public long AttachmentTimestamp { get; set; }
        public long AttachmentTimestampLowerBound { get; set; }
        public long AttachmentTimestampUpperBound { get; set; }

        public int Validity { get; set; }
        public int Type { get; set; } = TransactionViewModel.PrefilledSlot;
        public long ArrivalTime { get; set; }
    }
}
