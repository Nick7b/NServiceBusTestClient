using HyFlo.Service.Messages.Commands.Info;
using HyFlo.Service.Messages.InternalMessages.Info;
using NServiceBus;
using NServiceBus.Features;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NServiceBusTestClient
{
    public class Program
    {

        public static IEndpointInstance Bus;

        static void Main(string[] args)
        {

            AsyncPump.Run(async delegate
                {
                    Bus = await InitialiseBus();
                });

            CheckVersion();
        }

        #region InitialiseBus
        private static async Task<IEndpointInstance> InitialiseBus()
        {
            // bus initialisation & startup
            var route = $"WorkflowUI.Worklist.hydr0175.{Environment.MachineName}";
            var endpointConfiguration = new EndpointConfiguration(route);
            //endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);
            endpointConfiguration.PurgeOnStartup(true);
            endpointConfiguration.UseDataBus<FileShareDataBus>().BasePath(@"\\HIDDEN\NServicebusDataBus");

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString("host=HIDDEN;username=HIDDEN;password=HIDDEN");
            transport.UseConventionalRoutingTopology();

            //var routing = transport.Routing();

            //var hyfloMessagesAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName.StartsWith("HyFlo.Service.Messages"));
            //routing.RouteToEndpoint(hyfloMessagesAssembly, route);

            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.EnableDurableMessages();


            endpointConfiguration.DisableFeature<Scheduler>();
            //endpointConfiguration.DisableFeature<StorageDrivenPublishing>();
            //endpointConfiguration.DisableFeature<MessageDrivenSubscriptions>();
            endpointConfiguration.DisableFeature<TimeoutManager>();
            //endpointConfiguration.DisableFeature<TimeoutManagerBasedDeferral>();
            //endpointConfiguration.DisableFeature<SLAMonitoring>();
            //endpointConfiguration.DisableFeature<CriticalTimeMonitoring>();
            //endpointConfiguration.DisableFeature<ForwardReceivedMessages>();
            //endpointConfiguration.DisableFeature<BinarySerialization>();
            //endpointConfiguration.DisableFeature<BsonSerialization>();
            //endpointConfiguration.DisableFeature<JsonSerialization>();
            endpointConfiguration.DisableFeature<Sagas>();
            //endpointConfiguration.DisableFeature<Encryptor>();
            endpointConfiguration.DisableFeature<Audit>();
            //endpointConfiguration.DisableFeature<SecondLevelRetries>();

            endpointConfiguration.ExecuteTheseHandlersFirst(
                            typeof(NServiceBusHelper<,>).MakeGenericType(typeof(GetVersionCommand), typeof(GetVersionReply))
                            );

            endpointConfiguration.EnableInstallers();

            var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);
            Console.WriteLine("endpointInstance initialized.");
            return endpointInstance;
        }
        #endregion

        // compare client version with backend version
        #region GetVersionCommand
        public static async void CheckVersion()
        {
            //LogWriter.Debug($"AppMain.GetVersion::Context.Send<GetVersionCommand>({token.User} {correlationId})");

            var helper = new NServiceBusHelper<GetVersionCommand, GetVersionReply>();
            var options = new SendOptions();
            options.SetHeader("correlationId", "00001");
            var message = new GetVersionCommand();


            GetVersionReply result =
                await helper.GetMessage(Bus, message, options, TimeSpan.FromMinutes(1)).ConfigureAwait(false);

            if (result != null)
            {
                GetVersionReplyHandler(result);
            }
            else
            {
                return;
            }

            //token.Context.Send<GetVersionCommand>(c =>
            //         {
            //             if (!correlationId.Equals(Guid.Empty))
            //             {
            //                 token.Context.SetMessageHeader(c, CorrelationIdHeaderString, correlationId.ToString());
            //             }
            //         }).Register(r =>
            //         {
            //             if (token.IsCancelled)
            //             {
            //                 return;
            //             }
            //             if (r?.Messages != null && r.Messages.Any())
            //             {
            //                 GetVersionReplyHandler(r.Messages[0] as GetVersionReply);
            //             }
            //             else
            //             {
            //                 throw new Exception("HyFlo.GetVersionCommand()\r\nVersion Reply was empty! HyFlo/HSB probably timed out! Please report to your administrator.");
            //             }
            //         });
        }
        #endregion

        #region GetVersionReplyHandler
        private static void GetVersionReplyHandler(GetVersionReply asyncResult)
        {
            if (asyncResult == null || asyncResult.Failure)
            {
                //LogWriter.Error($"MainWindow.GetVersionReplyHandler::GetVersionReply\r\nError Code: {asyncResult?.ErrorCode ?? 0L} (no error code? then version Reply was empty!)\r\n");
                throw new Exception($"HyFlo.GetVersionReplyHandler()\r\nError Code: {(asyncResult == null ? "(no error code); Reply was empty! HyFlo/HSB probably timed out! Please report to your administrator." : $"{asyncResult.ErrorCode}")}\r\n");
            }

            //empty sessionFileString
            if (String.IsNullOrEmpty(asyncResult.Version.Trim()))
            {
                //LogWriter.Error("MainWindow.GetVersionReplyHandler::GetVersionReply; Empty Version reply!");
                return;
            }

            var ClientMajorVersion = Assembly.GetExecutingAssembly().GetName().Version.Major.ToString();
            var ClientMinorVersion = Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString();

            var BackendMajorVersion = asyncResult.Version.Trim().Split('.')[0];
            var BackendMinorVersion = asyncResult.Version.Trim().Split('.')[1];

            if (!ClientMajorVersion.Equals(BackendMajorVersion) || !ClientMinorVersion.Equals(BackendMinorVersion))
            {
                //LogWriter.Error("This client is outdated! It needs to be updated!");

                //Application.Current.Dispatcher.Invoke(delegate
                //{
                //    var wnd = new ExitWindow();
                //    wnd.ShowDialog();
                //});
            }
            //LogWriter.Debug($"MainWindow.GetVersionReplyHandler::GetVersionReply; No failure. Version:\r\n{asyncResult.Version}\r\n ^-- Save as .wrk file if you need to check this sessionfile manually.");

        }
        #endregion
    }

}
