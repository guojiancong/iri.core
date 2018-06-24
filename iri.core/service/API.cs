using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using iri.core.controllers;
using iri.core.hash;
using iri.core.model;
using iri.core.service.dto;
using iri.core.utils;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NLog;
using JsonSerializer = iri.core.utils.JsonSerializer;

namespace iri.core.service
{
    public class API
    {

        private const int HashSize = 81;
        private const int TrytesSize = 2673;
        private const long MaxTimestampValue = (3 ^ 27 - 1) / 2;

        private const string InvalidParams = "Invalid parameters";
        private const string OverMaxErrorMessage = "Could not complete request";

        private const char ZeroLengthAllowed = 'Y';
        private const char ZeroLengthNotAllowed = 'N';

        // ReSharper disable NotAccessedField.Local
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static TimeSpan _ellapsedTimePoW = TimeSpan.Zero;
        private static int _counterPoW = 0;

        private readonly object _syncRoot = new object();

        private Iota _instance;

        private IXI _ixi;
        // ReSharper restore NotAccessedField.Local

        private static int _counter;
        private readonly Regex _trytesPattern;
        private volatile PearlDiver _pearlDiver = new PearlDiver();

        public API(Iota iota, IXI ixi)
        {
            _instance = iota;
            _ixi = ixi;
            _trytesPattern = new Regex("[9A-Z]*");

            MaxBodyLength = 1000000;
            MaxRequestList = 1000;
        }

        public int MaxBodyLength { get; set; }
        public int MaxRequestList { get; set; }

        public void Init()
        {
            ReadPreviousEpochsSpentAddresses();

        }

        private void ReadPreviousEpochsSpentAddresses()
        {
            //TODO(gjc):add code here
        }


        public Task ProcessRequest(HttpContext context)
        {
            var beginTime = DateTime.Now;

            context.Response.ContentType = "application/json";

            StreamReader streamReader = new StreamReader(context.Request.Body);
            string body = streamReader.ReadToEnd();
            streamReader.Close();

            AbstractResponse response;
            // TODO(gjc): other method???
            if (!context.Request.Headers.Keys.Contains("X-IOTA-API-Version")
                && !context.Request.Headers.Keys.Contains("X-IOTA-API-Version".ToLower()))
                response = ErrorResponse.Create("Invalid API Version");
            else if (body.Length > MaxBodyLength)
                response = ErrorResponse.Create("Request too long");
            else
                response = Process(body, context.Connection.RemoteIpAddress);

            return SendFileResponseExtensions(context, response, beginTime);
        }

        private Task SendFileResponseExtensions(
            HttpContext context,
            AbstractResponse response,
            DateTime beginTime)
        {
            response.Duration = (int) (DateTime.Now - beginTime).TotalMilliseconds;
            string responseBody = new JsonSerializer().Serialize(response);

            if (response is ErrorResponse)
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            else if (response is AccessLimitedResponse)
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            else if (response is ExceptionResponse)
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Add("Keep-Alive", "timeout=500, max=100");

            context.Response.ContentLength = responseBody.Length;
            return context.Response.WriteAsync(responseBody);
        }

        // ReSharper disable once UnusedParameter.Local
        private AbstractResponse Process(string requestString, IPAddress remoteIpAddress)
        {
            try
            {
                Dictionary<string, object> request =
                    JsonConvert.DeserializeObject<Dictionary<string, object>>(requestString);
                if (request == null)
                {
                    return ExceptionResponse.Create($"Invalid request payload: '{requestString}'");
                }

                var result = request.TryGetValue("command", out var valueObject);
                if (!result)
                    return ErrorResponse.Create("COMMAND parameter has not been specified in the request.");

                string command = (string) valueObject;

                //TODO(gjc): add remote limit api

                Log.Debug($"# {Interlocked.Increment(ref _counter)} -> Requesting command '{command}'");

                switch (command)
                {
                    case "attachToTangle":
                        Hash trunkTransaction =
                            new Hash(GetParameterAsStringAndValidate(request, "trunkTransaction", HashSize));
                        Hash branchTransaction =
                            new Hash(GetParameterAsStringAndValidate(request, "branchTransaction", HashSize));
                        int minWeightMagnitude = GetParameterAsInt(request, "minWeightMagnitude");
                        List<string> trytes = GetParameterAsList(request, "trytes", TrytesSize);

                        List<string> elements = AttachToTangleStatement(trunkTransaction, branchTransaction,
                            minWeightMagnitude, trytes);
                        return AttachToTangleResponse.Create(elements);

                    case "getNodeInfo":
                        
                        return GetNodeInfoResponse.Create(
                            Program.MainnetName,Program.Version,
                            Environment.ProcessorCount,
                            975883432, "1.8.0_151", 5726797824, 3193962496,
                            new Hash("TWRPOZZSMGMUMG9LLARESH9CYOZYSZXSNXMJZUC9B9YSFR9VEDYMQQAJJHBWCITVJVSKHYSHWMHP99999"),
                            400101,
                            new Hash("TWRPOZZSMGMUMG9LLARESH9CYOZYSZXSNXMJZUC9B9YSFR9VEDYMQQAJJHBWCITVJVSKHYSHWMHP99999"), 
                            400101,
                            0,0,
                            TimeStamp.Now(),
                            5862,346);

                    //String name = instance.configuration.booling(Configuration.DefaultConfSettings.TESTNET) ? IRI.TESTNET_NAME : IRI.MAINNET_NAME;
                    //return GetNodeInfoResponse.create(name, IRI.VERSION, Runtime.getRuntime().availableProcessors(),
                    //    Runtime.getRuntime().freeMemory(), System.getProperty("java.version"), Runtime.getRuntime().maxMemory(),
                    //    Runtime.getRuntime().totalMemory(), instance.milestone.latestMilestone, instance.milestone.latestMilestoneIndex,
                    //    instance.milestone.latestSolidSubtangleMilestone, instance.milestone.latestSolidSubtangleMilestoneIndex,
                    //    instance.node.howManyNeighbors(), instance.node.queuedTransactionsSize(),
                    //    System.currentTimeMillis(), instance.tipsViewModel.size(),
                    //    instance.transactionRequester.numberOfTransactionsToRequest());

                    case "getNeighbors":
                        return GetNeighborsStatement();

                    default:
                    {
                        //TODO(gjc):add ixi process
                        return ErrorResponse.Create($"Command [{command}] is unknown");
                    }
                }

            }
            catch (ValidationException e)
            {
                Log.Info(e, "API Validation failed");
                return ErrorResponse.Create(e.Message);
            }
            catch (Exception e)
            {
                Log.Error(e, "API Exception");
                return ErrorResponse.Create(e.Message);
            }
        }

        private List<string> AttachToTangleStatement(
            Hash trunkTransaction, Hash branchTransaction,
            int minWeightMagnitude, List<string> trytes)
        {
            lock (_syncRoot)
            {
                List<TransactionViewModel> transactionViewModels = new List<TransactionViewModel>();

                Hash prevTransaction = null;
                _pearlDiver = new PearlDiver();

                int[] transactionTrits = Converter.AllocateTritsForTrytes(TrytesSize);

                foreach (string tryte in trytes)
                {
                    var startTime = DateTime.Now;
                    long timestamp = TimeStamp.Now();
                    try
                    {
                        Converter.Trits(tryte, transactionTrits, 0);
                        //branch and trunk
                        Array.Copy((prevTransaction ?? trunkTransaction).Trits(), 0,
                            transactionTrits, TransactionViewModel.TrunkTransactionTrinaryOffset,
                            TransactionViewModel.TrunkTransactionTrinarySize);
                        Array.Copy((prevTransaction == null ? branchTransaction : trunkTransaction).Trits(), 0,
                            transactionTrits, TransactionViewModel.BranchTransactionTrinaryOffset,
                            TransactionViewModel.BranchTransactionTrinarySize);

                        //attachment fields: tag and timestamps
                        //tag - copy the obsolete tag to the attachment tag field only if tag isn't set.
                        var tagTrits = ArrayUtils.SubArray(transactionTrits,
                            TransactionViewModel.TagTrinaryOffset,
                            TransactionViewModel.TagTrinarySize);
                        if (Array.TrueForAll(tagTrits, s => s == 0))
                        {
                            Array.Copy(transactionTrits, TransactionViewModel.ObsoleteTagTrinaryOffset,
                                transactionTrits, TransactionViewModel.TagTrinaryOffset,
                                TransactionViewModel.TagTrinarySize);
                        }

                        Converter.CopyTrits(timestamp, transactionTrits,
                            TransactionViewModel.AttachmentTimestampTrinaryOffset,
                            TransactionViewModel.AttachmentTimestampTrinarySize);
                        Converter.CopyTrits(0, transactionTrits,
                            TransactionViewModel.AttachmentTimestampLowerBoundTrinaryOffset,
                            TransactionViewModel.AttachmentTimestampLowerBoundTrinarySize);
                        Converter.CopyTrits(MaxTimestampValue, transactionTrits,
                            TransactionViewModel.AttachmentTimestampUpperBoundTrinaryOffset,
                            TransactionViewModel.AttachmentTimestampUpperBoundTrinarySize);

                        if (!_pearlDiver.Search(transactionTrits, minWeightMagnitude, 0))
                        {
                            transactionViewModels.Clear();
                            break;
                        }

                        //validate PoW - throws exception if invalid
                        TransactionViewModel transactionViewModel =
                            TransactionValidator.Validate(transactionTrits,
                                _instance.TransactionValidator.MinWeightMagnitude);

                        transactionViewModels.Add(transactionViewModel);
                        prevTransaction = transactionViewModel.Hash;
                    }
                    finally
                    {
                        IncreaseEllapsedTimePoW(DateTime.Now - startTime);
                        IncreaseCounterPoW();
                        if ((GetCounterPoW() % 100) == 0)
                        {
                            string sb = $"Last 100 PoW consumed {GetEllapsedTimePoW().TotalSeconds:F3} seconds processing time.";
                            
                            Log.Info(sb);
                            ResetCounterPow();
                            ResetEllapsedTimePoW();
                        }
                    }

                }

                List<string> elements = new List<string>();
                for (int i = transactionViewModels.Count; i-- > 0;)
                {
                    elements.Add(Converter.Trytes(transactionViewModels[i].Trits()));
                }

                return elements;

            }

        }

        private AbstractResponse GetNeighborsStatement()
        {
            // TODO(gjc): just for test
            return GetNeighborsResponse.CreateForTest();
            //return GetNeighborsResponse.Create(_instance.Node.GetNeighbors());
        }

        #region GetParameter

        private List<string> GetParameterAsList(Dictionary<string, object> request, string paramName, int size)
        {
            ValidateParamExists(request, paramName);
            Newtonsoft.Json.Linq.JArray paramArray = (Newtonsoft.Json.Linq.JArray) request[paramName];
            List<string> paramList = paramArray.ToObject<List<string>>();
            if (paramList.Count > MaxRequestList)
            {
                throw new ValidationException(OverMaxErrorMessage);
            }

            if (size > 0)
            {
                //validate
                foreach (string param in paramList)
                {
                    ValidateTrytes(paramName, size, param);
                }

            }

            return paramList;

        }

        private int GetParameterAsInt(Dictionary<string, object> request, string paramName)
        {
            ValidateParamExists(request, paramName);
            int result;
            try
            {
                result = Convert.ToInt32(request[paramName]);
            }
            catch (Exception)
            {
                throw new ValidationException($"Invalid {paramName} input");
            }

            return result;
        }

        private String GetParameterAsStringAndValidate(Dictionary<string, object> request, string paramName, int size)
        {
            ValidateParamExists(request, paramName);
            string result = (string) request[paramName];
            ValidateTrytes(paramName, size, result);
            return result;
        }

        #endregion

        #region Validate

        private void ValidateTrytes(string paramName, int size, string result)
        {
            if (!ValidTrytes(result, size, ZeroLengthNotAllowed))
            {
                throw new ValidationException($"Invalid {paramName} input");
            }

        }

        private bool ValidTrytes(string trytes, int length, char zeroAllowed)
        {
            if (trytes.Length == 0 && zeroAllowed == ZeroLengthAllowed)
            {
                return true;
            }

            return trytes.Length == length && _trytesPattern.IsMatch(trytes);
        }

        private void ValidateParamExists(Dictionary<string, object> request, string paramName)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (paramName == null) throw new ArgumentNullException(nameof(paramName));

            if (!request.ContainsKey(paramName))
            {
                throw new ValidationException(InvalidParams);
            }
        }

        #endregion

        public static void IncreaseEllapsedTimePoW(TimeSpan ellapsedTime)
        {
            _ellapsedTimePoW += ellapsedTime;
        }

        public static TimeSpan GetEllapsedTimePoW()
        {
            return _ellapsedTimePoW;
        }

        public static void ResetEllapsedTimePoW()
        {
            _ellapsedTimePoW = TimeSpan.Zero;
        }

        public static void IncreaseCounterPoW()
        {
            _counterPoW++;
        }
        public static int GetCounterPoW()
        {
            return _counterPoW;
        }

        public static void ResetCounterPow()
        {
            _counterPoW = 0;
        }
    }
}
