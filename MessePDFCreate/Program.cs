using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessePDFCreate
{
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

                        string dateiname = Kundennummer + "-" + Kundenname + ".png";

                        GeneratedBarcode barcode = IronBarCode.BarcodeWriter.CreateBarcode($"{ead.Lastname}", BarcodeEncoding.QRCode);

                        for (int i = 0; i < booking.Attendees.Count; i++)
                        {
                            //ead.QR = ("Qrcode" + dateiname + ".png");
                            barcode.SaveAsPng(@"C:\Users\Alsaleh\Desktop\Aufgabe\LkqMesseAnmeldungOmarMohamad\LkqMesseAnmeldungOmarMohamad\Cronjob\PdfCreator\QrCode\" + dateiname);

                        }
                    }
                }
            }

            host.Run();
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
