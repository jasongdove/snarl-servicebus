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

        public int Initialize()
        {
            _snarl = new SnarlInterface();
            _snarl.Register("jasongdove/" + ApplicationName, ApplicationName, Guid.NewGuid().ToString("N"));
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
            _snarl.Notify("", "", ApplicationName, "Snarl test");
        }

        public void Stop()
        {
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
            info.SupportEmail = "jason@jasongdove.com";
            info.URL = "http://jasongdove.com";
            info.Version = SnarlApiVersion;
        }

        public void LastError(ref string description)
        {
        }
    }
}