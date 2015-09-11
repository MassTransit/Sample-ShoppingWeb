namespace TrackingService
{
    using System.IO;
    using System.Text;
    using log4net.Config;
    using MassTransit.Log4NetIntegration.Logging;
    using Topshelf;
    using Topshelf.Logging;


    class Program
    {
        static int Main(string[] args)
        {
            ConfigureLogger();

            // Topshelf to use Log4Net
            Log4NetLogWriterFactory.Use();

            // MassTransit to use Log4Net
            Log4NetLogger.Use();

            return (int)HostFactory.Run(x => x.Service<TrackingService>());
        }

        static void ConfigureLogger()
        {
            const string logConfig = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<log4net>
  <root>
    <level value=""INFO"" />
    <appender-ref ref=""console"" />
  </root>
  <logger name=""NHibernate"">
    <level value=""ERROR"" />
  </logger>
  <appender name=""console"" type=""log4net.Appender.ColoredConsoleAppender"">
    <layout type=""log4net.Layout.PatternLayout"">
      <conversionPattern value=""%m%n"" />
    </layout>
  </appender>
</log4net>";

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(logConfig)))
            {
                XmlConfigurator.Configure(stream);
            }
        }
    }
}