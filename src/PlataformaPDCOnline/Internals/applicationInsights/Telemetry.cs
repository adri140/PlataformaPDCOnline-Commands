using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace PlataformaPDCOnline.Internals.applicationInsights
{
    class Telemetry //esto no me gusta
    {
        private static Telemetry ApplicationTelemetry;

        private static Telemetry Singelton()
        {
            if (ApplicationTelemetry == null) ApplicationTelemetry = new Telemetry();
            return ApplicationTelemetry;
        }

        private string TelemetryKey = "the key of men";

        private TelemetryClient client;

        private Telemetry()
        {
            client = GetAppTelemetryClient();
        }

        private TelemetryClient GetAppTelemetryClient()
        {
            var config = new TelemetryConfiguration();

            config.InstrumentationKey = TelemetryKey;
            config.TelemetryChannel = new Microsoft.ApplicationInsights.Channel.InMemoryChannel();
            config.TelemetryChannel.DeveloperMode = Debugger.IsAttached;

#if DEBUG
            config.TelemetryChannel.DeveloperMode = true;
#endif

            TelemetryClient client = new TelemetryClient(config);
            client.Context.Component.Version = Assembly.GetEntryAssembly().GetName().Version.ToString();
            client.Context.User.Id = Guid.NewGuid().ToString();
            client.Context.User.Id = (Environment.UserName + Environment.MachineName).GetHashCode().ToString();
            client.Context.Device.OperatingSystem = Environment.OSVersion.ToString();

            return client;
        }

        public void TrackEvent(String eventMessage)
        {
            client.TrackEvent(eventMessage);
        }
    }
}
