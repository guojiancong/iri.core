using System;
using iri.core.model;
using iri.core.utils;

namespace iri.core.controllers
{
    public class TransactionViewModel
    {
        #region Const Value
        public const  int Size = 1604;
        private const  int TagSize = 27;


        public const int SignatureMessageFragmentTrinaryOffset = 0, SignatureMessageFragmentTrinarySize = 6561;

        public const int AddressTrinaryOffset =
                SignatureMessageFragmentTrinaryOffset + SignatureMessageFragmentTrinarySize,
            AddressTrinarySize = 243;

        public const int ValueTrinaryOffset = AddressTrinaryOffset + AddressTrinarySize,
            ValueTrinarySize = 81,
            ValueUsableTrinarySize = 33;

        public const int ObsoleteTagTrinaryOffset = ValueTrinaryOffset + ValueTrinarySize, ObsoleteTagTrinarySize = 81;

        public const int TimestampTrinaryOffset = ObsoleteTagTrinaryOffset + ObsoleteTagTrinarySize,
            TimestampTrinarySize = 27;

        public const int CurrentIndexTrinaryOffset = TimestampTrinaryOffset + TimestampTrinarySize,
            CurrentIndexTrinarySize = 27;

        public const int LastIndexTrinaryOffset = CurrentIndexTrinaryOffset + CurrentIndexTrinarySize,
            LastIndexTrinarySize = 27;

        public const int BundleTrinaryOffset = LastIndexTrinaryOffset + LastIndexTrinarySize, BundleTrinarySize = 243;

        public const int TrunkTransactionTrinaryOffset = BundleTrinaryOffset + BundleTrinarySize,
            TrunkTransactionTrinarySize = 243;

        public const int BranchTransactionTrinaryOffset = TrunkTransactionTrinaryOffset + TrunkTransactionTrinarySize,
            BranchTransactionTrinarySize = 243;

        public const int TagTrinaryOffset = BranchTransactionTrinaryOffset + BranchTransactionTrinarySize,
            TagTrinarySize = 81;

        public const int AttachmentTimestampTrinaryOffset = TagTrinaryOffset + TagTrinarySize,
            AttachmentTimestampTrinarySize = 27;

        public const int AttachmentTimestampLowerBoundTrinaryOffset =
                AttachmentTimestampTrinaryOffset + AttachmentTimestampTrinarySize,
            AttachmentTimestampLowerBoundTrinarySize = 27;

        public const int AttachmentTimestampUpperBoundTrinaryOffset =
                AttachmentTimestampLowerBoundTrinaryOffset + AttachmentTimestampLowerBoundTrinarySize,
            AttachmentTimestampUpperBoundTrinarySize = 27;

        private const int NonceTrinaryOffset =
                AttachmentTimestampUpperBoundTrinaryOffset + AttachmentTimestampUpperBoundTrinarySize,
            NonceTrinarySize = 81;

        public const int TrinarySize = NonceTrinaryOffset + NonceTrinarySize;


        public const int
            Group = 0; // transactions GROUP means that's it's a non-leaf node (leafs store transaction value)

        public const int
            PrefilledSlot =
                1; // means that we know only hash of the tx, the rest is unknown yet: only another tx references that hash

        public const int FilledSlot = -1; //  knows the hash only coz another tx references that hash

        #endregion

        private readonly object _syncRoot = new object();

        private Transaction _transaction;
        private int[] _trits;
        private Hash _hash;

        public TransactionViewModel(int[] trits, Hash hash)
        {
            _transaction = new Transaction();
            _trits = new int[trits.Length];
            Array.Copy(trits, 0, _trits, 0, trits.Length);
            _transaction.Bytes = Converter.AllocateBytesForTrits(trits.Length);
            Converter.Bytes(trits, 0, _transaction.Bytes, 0, trits.Length);
            _hash = hash;

            _transaction.Type = FilledSlot;

            WeightMagnitude = _hash.TrailingZeros();
            _transaction.Validity = 0;
            _transaction.ArrivalTime = 0;
        }

        public int WeightMagnitude { get; set; }

        public Hash Hash => _hash;

        public static int[] Trits(byte[] transactionBytes)
        {
            int[] trits = new int[TrinarySize];
            if (transactionBytes != null)
            {
                Converter.GetTrits(transactionBytes, trits);
            }

            return trits;
        }

        public int[] Trits()
        {
            lock (_syncRoot)
            {
                return _trits ?? (_trits = Trits(_transaction.Bytes));
            }


        }

        public void SetMetadata()
        {
            _transaction.Value = Converter.LongValue(Trits(), ValueTrinaryOffset, ValueUsableTrinarySize);
            _transaction.Timestamp = Converter.LongValue(Trits(), TimestampTrinaryOffset, TimestampTrinarySize);
            //if (transaction.timestamp > 1262304000000L ) transaction.timestamp /= 1000L;  // if > 01.01.2010 in milliseconds
            _transaction.CurrentIndex =
                Converter.LongValue(Trits(), CurrentIndexTrinaryOffset, CurrentIndexTrinarySize);
            _transaction.LastIndex = Converter.LongValue(Trits(), LastIndexTrinaryOffset, LastIndexTrinarySize);
            _transaction.Type = _transaction.Bytes == null ? PrefilledSlot : FilledSlot;
        }

        public void SetAttachmentData()
        {
            GetTagValue();
            _transaction.AttachmentTimestamp = Converter.LongValue(Trits(),
                AttachmentTimestampTrinaryOffset,
                AttachmentTimestampTrinarySize);
            _transaction.AttachmentTimestampLowerBound = Converter.LongValue(Trits(),
                AttachmentTimestampLowerBoundTrinaryOffset,
                AttachmentTimestampLowerBoundTrinarySize);
            _transaction.AttachmentTimestampUpperBound = Converter.LongValue(Trits(),
                AttachmentTimestampUpperBoundTrinaryOffset,
                AttachmentTimestampUpperBoundTrinarySize);

        }
        public Hash GetTagValue()
        {
            if (_transaction.Tag == null)
            {
                byte[] tagBytes = Converter.AllocateBytesForTrits(TagTrinarySize);
                Converter.Bytes(Trits(), TagTrinaryOffset, tagBytes, 0,TagTrinarySize);
                _transaction.Tag = new Hash(tagBytes, 0, TagSize);
            }
            return _transaction.Tag;
        }
        public long GetAttachmentTimestamp() { return _transaction.AttachmentTimestamp; }
        public long Value()
        {
            return _transaction.Value;
        }
        public Hash GetAddressHash()
        {
            return _transaction.Address ?? (_transaction.Address = new Hash(Trits(), AddressTrinaryOffset));
        }
        public long GetTimestamp()
        {
            return _transaction.Timestamp;
        }
        public Hash GetHash()
        {
            return _hash;
        }
    }
}
