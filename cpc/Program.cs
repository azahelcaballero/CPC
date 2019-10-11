using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Funq;
using Microsoft.Extensions.Configuration;
using ServiceStack;
using cpc.ServiceInterface;
using cpc.ServiceModel;
using ServiceStack.Configuration;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ServiceStack.OrmLite;
using cpc.ServiceModel.Types;
using ServiceStack.Text;

namespace cpc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // using (var cpc_Context = new x())

            var yesterday = DateTime.Now.AddDays(-1);
            var sauce = $@"\\appsrvr\c$\Program Files (x86)\PaperCut Print Logger\logs\csv\daily\papercut-print-log-{yesterday.Year}-{yesterday.ToString("MM")}-{yesterday.ToString("dd")}.csv";
            //var sauce = $@"\\appsrvr\c$\Program Files (x86)\PaperCut Print Logger\logs\csv\papercut-print-log-all-time - Copy.csv";
            if (!File.Exists(sauce)) throw new Exception("Error. File not found.");

            var file = new List<List<string>>();
            List<string> fileHeader;


            try
            {


                #region Attempt Read from Txt File
                //log("Intentando leer archivo: " + ev.FullPath);
                file = null;
                /*
                file = (from l in File.ReadLines(sauce, Encoding.GetEncoding("iso-8859-1"))
                        let x = l.Split(new[] { ',' }).ToList()
                        select x).ToList();


                */


                file = File.ReadAllText(sauce).FromCsv<List<List<string>>>();
                //log("Archivo leído correctamente.");

                if (file.Count() <= 1)
                {
                    //log("Archivo vacio.");
                    throw new Exception("Error. Empty File.");
                }

                #endregion

                #region Find Header Columns

                var startLineNumber = file.FindIndex(l => l[0].Contains("Time"));
                fileHeader = file[startLineNumber];
                if (fileHeader == null)
                {
                    //log("No se ha podido leer encabezado.");
                    throw new Exception("Error. Could not read Header.");
                }


                //log("Leyendo ubicación de columnas en encabezado");
                Dictionary<string, int> header = new Dictionary<string, int>();
                for (int i = 0; i < fileHeader.Count; i++)
                {
                    //Omit repeated column titles.
                    try
                    {
                        header.Add(fileHeader[i].ToLower(), i);
                    }
                    catch
                    {
                        //log("Se omitió columna repetida: [" + fileHeader[i] + "]");
                    }
                }

                Console.WriteLine(file[3][header["user"]]);
                // Console.WriteLine(file[3][2]); 


                file = file.Skip(startLineNumber + 1).ToList();

                #endregion

                #region Insert Rows into Log


                var logs = new List<PrintingLog>();

                foreach (var row in file)
                {
                    var log = new PrintingLog();



                    logs.Add(new PrintingLog
                    {

                        Client = row[header["client"]],
                        User = row[header["user"]],
                        Size = row[header["size"]],
                        Time = DateTime.Parse(row[header["time"]]),
                        duplex = row[header["duplex"]] == "DUPLEX" ? true : false,
                        Height = row[header["height"]] != "" ? Convert.ToDouble(row[header["height"]]) : 0,
                        grayscale = row[header["grayscale"]] == "GRAYSCALE" ? true : false,
                        DocumentName = row[header["document name"]],
                        Language = row[header["language"]],
                        Pages = Convert.ToDouble(row[header["pages"]]),
                        Width = row[header["width"]] != "" ? Convert.ToDouble(row[header["width"]]) : 0,


                    }); ;



                }

                var dbFactory = new OrmLiteConnectionFactory(
                "data source=RSSERVER;initial catalog=PaperCut;Persist Security Info=True;User ID=IQS_APP;Password=capsonic;MultipleActiveResultSets=True", SqlServer2008Dialect.Provider);

                using (var db = dbFactory.Open())
                {
                    db.CreateTableIfNotExists<PrintingLog>();
                    db.InsertAll(logs);
                }

                //renaming file after import


                #endregion

                System.IO.File.Move(sauce, sauce + " - Imported.csv");
            }catch  (Exception) {


                System.IO.File.Move(sauce, sauce + " - Failed.csv");


                throw;
                }
        }
    }

    public class Startup : ModularStartup
    {
        public Startup(IConfiguration configuration) : base(configuration) { }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public new void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseServiceStack(new AppHost());

            app.Run(context =>
            {
                context.Response.Redirect("/metadata");
                return Task.FromResult(0);
            });
        }
    }

    public class AppHost : AppHostBase
    {
        public AppHost()
            : base("cpc", typeof(PrintingLogService).Assembly) { }

        public override void Configure(Container container)
        {
        }
    }
}