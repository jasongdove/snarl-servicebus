using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IniParser;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using libSnarlExtn;
using Snarl.V44;
using System;

namespace SnarlExtensions
{
    [System.Runtime.InteropServices.ProgId("ServiceBus.extension")]
    public class ServiceBus : ISnarlExtension
    {
        private const string ApplicationName = "ServiceBus";
        private const string Release = "1.0";
        private const int Revision = 1;
        private const int SnarlApiVersion = 44;

        private readonly string _versionDate = DateTime.Parse("2013-01-05").ToLongDateString();

        private SnarlInterface _snarl;
        private CancellationTokenSource _cts;

        public int Initialize()
        {
            _snarl = new SnarlInterface();
            _snarl.Register("jasongdove/" + ApplicationName, ApplicationName, Guid.NewGuid().ToString("N"));

            _cts = new CancellationTokenSource();

            return (int)SnarlStatus.Success;
        }

        public void TidyUp()
        {
            if (_snarl != null)
            {
                _snarl.Unregister();
            }
        }

        public void Start()
        {
            Task.Factory.StartNew(ReceiveMessages, _cts.Token);
        }

        public void Stop()
        {
            _cts.Cancel();
        }

        public int GetConfigWindow()
        {
            return 0;
        }

        public void Pulse()
        {
        }

        public void GetInfo(ref extension_info info)
        {
            info.Author = "Jason G Dove";
            info.Copyright = "Copyright © Jason G Dove 2013";
            info.Date = _versionDate;
            info.Description = "Subscribes to Azure Service Bus topics for notifications";
            info.Name = ApplicationName;
            info.Path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            info.Release = Release;
            info.Revision = Revision;
            info.Version = SnarlApiVersion;
            //info.Flags = SNARL_EXTENSION_FLAGS.SNARL_EXTN_IS_CONFIGURABLE;
        }

        public void LastError(ref string description)
        {
        }

        private void ReceiveMessages()
        {
            var app = new libsnarl26.SnarlApp();
            string configPath = Path.Combine(app.GetEtcPath(), ".servicebus");
            var parser = new FileIniDataParser();
            IniData data = parser.LoadFile(configPath);
            var section = data["servicebus"];

            var builder = new ServiceBusConnectionStringBuilder();
            builder.Endpoints.Add(new Uri(section["endpoint"]));
            builder.SharedSecretIssuerName = section["issuer"];
            builder.SharedSecretIssuerSecret = section["access_key"];
            string connectionString = builder.ToString();
            string topic = data["servicebus"]["topic"];
            string subscription = data["servicebus"]["subscription"];
            var client = SubscriptionClient.CreateFromConnectionString(connectionString, topic, subscription);

            while (!_cts.IsCancellationRequested)
            {
                var message = client.Receive(TimeSpan.FromHours(1));
                if (message != null)
                {
                    try
                    {
                        if (message.DeliveryCount > 2)
                        {
                            message.DeadLetter();
                        }

                        string title = message.Properties.ContainsKey("title")
                            ? message.Properties["title"] as string
                            : ApplicationName;

                        string image = message.Properties.ContainsKey("image")
                            ? message.Properties["image"] as string
                            : null;

                        SnarlMessagePriority priority = message.Properties.ContainsKey("priority")
                            ? (SnarlMessagePriority)(int)message.Properties["priority"]
                            : SnarlMessagePriority.Normal;

                        string body;
                        using (var reader = new StreamReader(message.GetBody<Stream>()))
                        {
                            body = reader.ReadToEnd();
                        }

                        _snarl.Notify(null, null, title, body, null, null, image, null, null, null, priority);

                        message.Complete();
                    }
                    catch (Exception ex)
                    {
                        _snarl.Notify(null, null, ApplicationName, ex.Message);
                        message.Abandon();
                    }
                }
            }
        }
    }
}