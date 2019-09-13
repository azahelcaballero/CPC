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

namespace cpc
{
    public class Program
    {
        public static void Main(string[] args)
        {


            var sauce = @"C:\papercut-print-log-2019-05-03.csv";
            if (!File.Exists(sauce)) throw new Exception("Error. File not found.");

            var file = new List<List<string>>();
            List<string> fileHeader;

            #region Attempt Read from Txt File
            //log("Intentando leer archivo: " + ev.FullPath);
            file = null;

            file = (from l in File.ReadLines(sauce, Encoding.GetEncoding("iso-8859-1"))
                    let x = l.Split(new[] { ',' }).ToList()
                    select x).ToList();
            //log("Archivo leído correctamente.");

            if (file.Count() <= 1)
            {
                //log("Archivo vacio.");
                throw new Exception("Error. Empty File.");
            }

            #endregion

            //log("Intentando leer encabezado");
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

            #endregion
            /*
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls(Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://localhost:5000/")
                .Build();


            host.Run();
            */
        }
    }

    public class Startup : ModularStartup
    {
        public Startup(IConfiguration configuration) : base(configuration){}

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
            : base("cpc", typeof(MyServices).Assembly) { }

        public override void Configure(Container container)
        {
        }
    }
}