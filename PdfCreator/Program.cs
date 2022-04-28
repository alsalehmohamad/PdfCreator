using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Backend.Services.CMSService;
using BackEnd.Types.Entities;
using Microsoft.Extensions.DependencyInjection;
//using BackEnd.Data;
using SimpleTypes.Models;
using BackEnd.Data;
using IronBarCode;
using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using OpenHtmlToPdf;

namespace Cronjob
{
    public class HtmlTemplate
    {
        private string _html;

        public HtmlTemplate(string templatePath)
        {
            using (var reader = new StreamReader(@"HTML/HTMLPage.html"))
                _html = reader.ReadToEnd();



        }

        public string Render(object values)
        {
            string output = _html;
            foreach (var p in values.GetType().GetProperties())
                output = output.Replace("[" + p.Name + "]", (p.GetValue(values, null) as string) ?? string.Empty);
            return output;

        }
    }

    public class Program
    {
        public class AppSettings
        {
            public bool Seed;
            public bool SeedLogin;
        }
        public static async Task Main(string[] args)
        {
            
            IHost host = CreateHostBuilder(args).Build();
            AppSettings appSettings;
            using (
                StreamReader r = new StreamReader("appsettings.json"))
            {
                string json = r.ReadToEnd();
                appSettings = JsonConvert.DeserializeObject<AppSettings>(json);
            }
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                BackEnd.Data.ApplicationDbContext context = services.GetRequiredService<ApplicationDbContext>();
                IBookingService bookingService = services.GetRequiredService<BookingService>();
                // context.Database.EnsureCreated();
                List<Booking> bookings = await bookingService.GetAllWorkshopBooking();

                //PdfDocument doc = new PdfDocument();

                foreach (Booking booking in bookings)
                {
                    booking.DeserializeData();

                    string Werkstattname = "AUTOFIT Absolute";
                    string Kundennummer = booking.CustomerNumber;
                    string StrassePlzOrt = "Jungfernstieg 49, 20354 Hamburg";
                    string Kundenname = "Tim Thaler";
                    string EMail = "";




                    foreach (EventAttendeeDto ead in booking.Attendees)
                    {


                        Kundenname = ead.Lastname;
                        EMail = ead.EMail;
                        Console.WriteLine($"{Werkstattname};{Kundennummer};{Kundenname};{EMail}");

                        //string dateiname = Kundennummer + "-" + Kundenname + ".png";
                       // string pfad = Kundenname + Kundennummer + "-" + ".png";
                       string pfad = Kundennummer + Kundenname + "-" + ".png";
                        string Ticketname = Kundenname + ".pdf";


                        GeneratedBarcode barcode = IronBarCode.BarcodeWriter.CreateBarcode($"{ead.Lastname}", BarcodeEncoding.QRCode);
                        // GeneratedBarcode barcode = IronBarCode.BarcodeWriter.CreateBarcode($"{Werkstattname};{Kundennummer};{Kundenname};{EMail}", BarcodeEncoding.QRCode);


                        //for (int i = 0; i < booking.Attendees.Count; i++)
                        //{

                        var template = new HtmlTemplate(@"HTMLPage1.html");
                        var output = template.Render(new
                        {
                            Kundenname = Kundenname,
                            Kundennumber = Kundennummer,
                            qrcode = pfad
                        });
                        Console.WriteLine(output);

                        //ead.QR = ("Qrcode" + dateiname + ".png");
                        barcode.SaveAsPng(@"C:\Users\Alsaleh\Desktop\Aufgabe\LkqMesseAnmeldungOmarMohamad\LkqMesseAnmeldungOmarMohamad\Cronjob\PdfCreator\QrCode\" + pfad);
                            string html1 = File.ReadAllText(@"HTML\HTMLPage.html");
                            PdfDocument pdf1 = PdfGenerator.GeneratePdf(output, PageSize.A4);
                            pdf1.Save(@"C:\Users\Alsaleh\Desktop\Aufgabe\LkqMesseAnmeldungOmarMohamad\LkqMesseAnmeldungOmarMohamad\Cronjob\PdfCreator\QrCodePdf\" + Ticketname);
                            //html1 = html1.Replace("<Kundenname/>", Kundenname);

                            

                            /*
                            var combinedPdf = new PdfDocument();
                         

                           
                            PdfDocument pdf1ForImport = ImportPdfDocument(pdf1);
                            combinedPdf.Pages.Add(pdf1ForImport.Pages[0]);
                            combinedPdf.Save(@"C:\Users\Alsaleh\Desktop\Aufgabe\LkqMesseAnmeldungOmarMohamad\LkqMesseAnmeldungOmarMohamad\Cronjob\PdfCreator\QrCodePdf\Tickets.pdf");
                            */


                        //}
                    }
                }
            }
            host.Run();
        }

      
        //private static PdfDocument ImportPdfDocument(PdfDocument pdf1)
        //{
        //    using (var stream = new MemoryStream())
        //    {
        //        pdf1.Save(stream, false);
        //        stream.Position = 500;
        //        var result = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
        //        return result;
        //    }
        //}

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }


}
