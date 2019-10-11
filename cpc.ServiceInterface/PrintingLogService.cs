using System;
using ServiceStack;
using cpc.ServiceModel;
using ServiceStack.Configuration;

namespace cpc.ServiceInterface
{
    public class PrintingLogService : Service
    {
        public object Any()
        {

            IAppSettings appSettings = new AppSettings();
            var SourceFile = appSettings.Get<string>("SourceFile");



            return new HelloResponse { Result = $"Hello!" };
        }




    }
}
