using System;
using iri.core.controllers;
using iri.core.exception;
using iri.core.hash;
using iri.core.model;
using iri.core.utils;

namespace iri.core
{
    public class TransactionValidator
    {
        private const long MinTimestamp = 1517180400;
        private const long MinTimestampMs = MinTimestamp * 1000;
        private const long MaxTimestampFuture = 2 * 60 * 60;
        private const long MaxTimestampFutureMs = MaxTimestampFuture * 1000;

        public int MinWeightMagnitude { get; }

        public TransactionValidator()
        {
            MinWeightMagnitude = 14;
        }

        public static TransactionViewModel Validate(int[] trits, int minWeightMagnitude)
        {
            TransactionViewModel transactionViewModel = new TransactionViewModel(trits,
                Hash.Calculate(trits, 0, trits.Length, SpongeFactory.Create(SpongeFactory.Mode.CurlP81)));
            RunValidation(transactionViewModel, minWeightMagnitude);
            return transactionViewModel;
        }

        public static void RunValidation(TransactionViewModel transactionViewModel, int minWeightMagnitude)
        {
            transactionViewModel.SetMetadata();
            transactionViewModel.SetAttachmentData();
            if (HasInvalidTimestamp(transactionViewModel))
            {
                throw new StaleTimestampException("Invalid transaction timestamp.");
            }

            for (int i = TransactionViewModel.ValueTrinaryOffset + TransactionViewModel.ValueUsableTrinarySize;
                i < TransactionViewModel.ValueTrinaryOffset + TransactionViewModel.ValueTrinarySize;
                i++)
            {
                if (transactionViewModel.Trits()[i] != 0)
                {
                    throw new Exception("Invalid transaction value");
                }
            }

            int weightMagnitude = transactionViewModel.WeightMagnitude;
            if (weightMagnitude < minWeightMagnitude)
            {
                throw new Exception("Invalid transaction hash");
            }

            if (transactionViewModel.Value() != 0 &&
                transactionViewModel.GetAddressHash().Trits()[Sponge.HashLength - 1] != 0)
            {
                throw new Exception("Invalid transaction address");
            }
        }

        private static bool HasInvalidTimestamp(TransactionViewModel transactionViewModel)
        {
            if (transactionViewModel.GetAttachmentTimestamp() == 0)
            {
                return transactionViewModel.GetTimestamp() < MinTimestamp &&
                       transactionViewModel.GetHash() != Hash.NullHash
                       || transactionViewModel.GetTimestamp() >
                       (TimeStamp.Now() / 1000) + MaxTimestampFuture;
            }

            return transactionViewModel.GetAttachmentTimestamp() < MinTimestampMs
                   || transactionViewModel.GetAttachmentTimestamp() > TimeStamp.Now() + MaxTimestampFutureMs;
        }
    }
}
