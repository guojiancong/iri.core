using System;
using iri.core.conf;
using iri.core.service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NLog;

namespace iri.core
{
    internal class Program
    {
        public static string MainnetName = "IRI";
        public static string TestnetName = "IRI Testnet";
        public static string Version = "1.4.2.2";

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static Iota _iota;
        private static API _api;
        private static IXI _ixi;
        private static Configuration _configuration;

        private static IWebHost _webHost;

        private static int Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

            _configuration = new Configuration();
            try
            {
                _configuration.ValidateParams(args);
            }
            catch (Exception e)
            {
                Log.Error(e);
                return -1;
            }


            Log.Info($"Welcome to {MainnetName} {Version}");

            _iota = new Iota(_configuration);
            _ixi = new IXI(_iota);
            _api = new API(_iota, _ixi);

            try
            {
                Log.Info("Process Start");

                _iota.Init();
                _api.Init();


                var apiPort = 14265;
                //Log.Debug($"Binding JSON-REST API Kestrel server on {IPAddress.Loopback}:{apiPort}");

                _webHost = new WebHostBuilder()
                    .UseUrls($"http://*:{apiPort}")
                    .UseStartup<Program>()
                    .UseKestrel(options =>
                    {
                        //options.Listen(IPAddress.Loopback, apiPort);
                        //TODO(gjc): add limit

                    }).Build();

                _webHost.Run();
            }
            catch (Exception e)
            {
                Log.Error(e, "Exception during IOTA node initialisation");
                return -1;
            }

            return 0;
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            Log.Info("Process Exit");
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            //var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();

            app.Run(async context =>
            {
                var method = context.Request.Method;
                if (string.Equals(method, "OPTIONS"))
                {
                    string allowedMethods = "GET,HEAD,POST,PUT,DELETE,TRACE,OPTIONS,CONNECT,PATCH";

                    context.Response.StatusCode = StatusCodes.Status200OK;
                    context.Response.ContentType = "text/plain";
                    context.Response.ContentLength = 0;

                    context.Response.Headers.Add("Allow", allowedMethods);
                    context.Response.Headers.Add("Connection", "Keep-Alive");
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    context.Response.Headers.Add("Access-Control-Allow-Headers",
                        "Origin, X-Requested-With, Content-Type, Accept, X-IOTA-API-Version");

                    return;
                }

                await _api.ProcessRequest(context);
            });
        }
    }
}