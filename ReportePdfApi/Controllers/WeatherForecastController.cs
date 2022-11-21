using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.qrcode;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Drawing.Imaging;
using QRCoder;
using System.Reflection.Metadata;
using Document = iTextSharp.text.Document;

namespace ReportePdfApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
       
        public WeatherForecastController()
        {

        }
        /*Pdf buscar por nombre*/
        [HttpGet("BusquedaDelPdf/{pdfBuscar}")]
        public ActionResult Get(string pdfBuscar)
        { 
            try
            {
                var stream = new FileStream(@"C:\Users\VIAMATICA\Desktop\"+pdfBuscar+".pdf", FileMode.Open);
                return File(stream, "application/pdf", "FileDownloadName.ext");
            }
            catch
            {
                return NotFound("No se encontro el pdf");
            }
        }
        /*Pdf buscar por nombre y presentar imagen*/
        [HttpGet("BusquedaDelPdfConImagen/{pdfBuscar}")]
        public ActionResult GetPdfConImagen(string pdfBuscar)
        {
            try
            {
                string pdfUser = "C:\\Users\\VIAMATICA\\Desktop\\"+ pdfBuscar + ".pdf";
                string tempPdf = "C:\\Users\\VIAMATICA\\Desktop\\TemporalPdf"+ pdfBuscar + ".pdf";

                using (Stream inputPdf = new FileStream(pdfUser, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (Stream outputPdf = new FileStream(tempPdf, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        var reader = new PdfReader(inputPdf);
                        var stamper = new PdfStamper(reader, outputPdf);
                        iTextSharp.text.Rectangle rect =
                         new iTextSharp.text.Rectangle(100, 100, 500, 500);
                        iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance("C:\\Users\\VIAMATICA\\Desktop\\QrImagen.png");
                        img.ScaleAbsolute(rect.Width, rect.Height);
                        img.SetAbsolutePosition(rect.Left, rect.Bottom);
                        int numeroDePaginas = stamper.Reader.NumberOfPages;
                        stamper.InsertPage(numeroDePaginas+1, new iTextSharp.text.Rectangle(20,28,20,28));
                        stamper.GetOverContent(numeroDePaginas+1).AddImage(img);
                        stamper.Close();
                    }
                }
                using (Stream outputPdf = new FileStream(tempPdf, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (Stream inputPdf = new FileStream(pdfUser, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        var reader = new PdfReader(outputPdf);
                        var stamper = new PdfStamper(reader, inputPdf);
                        stamper.Close();
                    }
                }
                System.IO.File.Delete(tempPdf);
                FileStream stream = new FileStream(@"C:\Users\VIAMATICA\Desktop\" + pdfBuscar + ".pdf", FileMode.Open);
                return File(stream, "application/pdf", "FileDownloadName.ext");
            }
            catch(Exception ex)
            {
                return NotFound("No se encontro el pdf");
            }
        }
        /*Pdf crearlo con html*/
        [HttpPost("CrarElPdfConHtml")]
        public ActionResult CrearPdfConHtml(Html html)
        {
            try
            {

                    StringWriter sw = new StringWriter();
                    sw.WriteLine(html.codigoHtml.ToString());
                    StringReader sr = new StringReader(sw.ToString());
                iTextSharp.text.Document pdfDoc = new Document();
                    HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
                    PdfWriter.GetInstance(pdfDoc, new FileStream("C:\\Users\\VIAMATICA\\Desktop\\HtmlPdfiu.pdf", FileMode.Create));
                    pdfDoc.Open();
                    htmlparser.Parse(sr);
                    iTextSharp.text.Image image1 = iTextSharp.text.Image.GetInstance("C:\\Users\\VIAMATICA\\Desktop\\QrImagen.png");
                    image1.ScaleAbsoluteWidth(100);
                    image1.ScaleAbsoluteHeight(100);
                    image1.Alignment = Element.ALIGN_RIGHT;
                    //image1.SetAbsolutePosition(50, 50);
               // float positionx = image1.AbsoluteX;
              //  float positiony = image1.AbsoluteY;
              //  positionx = positionx + 10;
              //  Console.WriteLine("Position x:" + positionx + " Position Y : " + positiony);
                pdfDoc.Add(image1);
                    pdfDoc.Close();
                    FileStream stream = new FileStream("C:\\Users\\VIAMATICA\\Desktop\\HtmlPdfiu.pdf", FileMode.Open);
                    return File(stream, "application/pdf", "FileDownloadName.ext");
            }
            catch(Exception ex)
            {
                return NotFound("No se encontro el pdf => "+ex.Message);
            }
        }
        /*Pdf crearlo y codigo Qr*/
        [HttpPost("CrarElPdfConHtmlYPersonaQr")]
        public ActionResult CrearPdfConHtmlYPersonaQr(Html html)
        {
            try
            {
                /*Creacion del pdf por código del html*/
                StringReader stringReader = ParseHtml(html);
                iTextSharp.text.Document pdfNuevo = new iTextSharp.text.Document();
                HTMLWorker htmlWorker = new HTMLWorker(pdfNuevo);
                // Agregamos la ruta donde se ca a guardar el pdf
                PdfWriter writer = PdfWriter.GetInstance(pdfNuevo, new FileStream("C:\\Users\\Kevin Arevalo\\Desktop\\1.pdf", FileMode.Create));
                pdfNuevo.Open();
                htmlWorker.Parse(stringReader);
                //Obtenemos la imagen de la persona por el metodo
                iTextSharp.text.Image imagenQr = imagenPersona().GetImage();
                imagenQr.ScaleAbsoluteWidth(80);
                imagenQr.ScaleAbsoluteHeight(80);
                imagenQr.Alignment = Element.ALIGN_RIGHT;
                /*Creacion de la tabla*/
                PdfPTable tabla = new PdfPTable(new float[] { 50f, 40f }) { WidthPercentage = 40 };
                PdfPCell elementoImagen = new PdfPCell();
                elementoImagen.Padding = 0;
                elementoImagen.Border = 0;
                elementoImagen.AddElement(imagenQr);
                tabla.AddCell(elementoImagen);
                //Creación de las celdas y sus contenidos
                PdfPCell elemntoTexto = new PdfPCell();
                Paragraph textoFirma = new Paragraph("Firmado por");
                textoFirma.Alignment = Element.ALIGN_CENTER;
                textoFirma.PaddingTop = 150f;
                Paragraph textoPersona = new Paragraph(nombreCliente()); ;
                textoPersona.Alignment = Element.ALIGN_CENTER;
                elemntoTexto.Padding = 0;
                elemntoTexto.Border = 0;
                elemntoTexto.HorizontalAlignment = Element.ALIGN_CENTER;
                elemntoTexto.AddElement(textoFirma);
                elemntoTexto.AddElement(textoPersona);
                elemntoTexto.PaddingTop = 17;
                tabla.AddCell(elemntoTexto);
                tabla.HorizontalAlignment = Element.ALIGN_RIGHT;
                pdfNuevo.Add(tabla);
                pdfNuevo.Close();
                FileStream stream = new FileStream("C:\\Users\\Kevin Arevalo\\Desktop\\1.pdf", FileMode.Open);
                return File(stream, "application/pdf", "FileDownloadName.ext");//
            }
            catch (Exception ex)
            {
                return NotFound("Error al crear el pdf => " + ex.Message);
            }
        }
        /*No funciona aun*/
        [HttpGet("BusquedaDeTodosLosPdfs")]
        public ActionResult GetTodos()
        {
            try
            {
                List<String> listaPdf = new List<String>();
                string[] files = Directory.GetFiles(@"C:\Users\VIAMATICA\Desktop", "*.pdf");
                foreach (string file in files)
                {
                    listaPdf.Add(file);
                }
                if(listaPdf.Count > 0)
                {
                    //FileInfo[] file = new FileInfo[]();
                    return Ok(files);
                }
                else{
                    return NotFound("No se encontro el pdf");
                }
                
            }
            catch
            {
                return NotFound("No se encontro el pdf");
            }
        }/*
        public HttpResponseMessage Get(string docId)
        {
            byte[] response = FileProxy.GetDocumentStream(docId);

            if (response == null) return new HttpResponseMessage(HttpStatusCode.BadRequest);

            MemoryStream ms = new MemoryStream(response);
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new ByteArrayContent(ms.ToArray());
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            return result;
        }*/
        private BarcodeQRCode imagenPersona()
        {
            try
            {
                Persona persona = new Persona
                {
                    nombre = "Kevin",
                    apellido = "Arevalo",
                    cedula = "0302527742"
                };
                var paramQR = new Dictionary<EncodeHintType, object>();
                paramQR.Add(EncodeHintType.CHARACTER_SET, CharacterSetECI.GetCharacterSetECIByName("UTF-8"));
                BarcodeQRCode qrCodigo = new BarcodeQRCode("Nombre: " + persona.nombre + " Apellido: " + persona.apellido + " Cedula: " + persona.cedula,
                    150, 150, paramQR);

                return qrCodigo;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private StringReader ParseHtml(Html html)
        {
            try
            {
                StringWriter stringWriter = new StringWriter();
                stringWriter.WriteLine(html.codigoHtml.ToString());
                StringReader stringReader = new StringReader(stringWriter.ToString());
                return stringReader;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }
        private String nombreCliente()
        {
            Persona persona = new Persona
            {
                nombre = "Kevin",
                apellido = "Arevalo",
                cedula = "Saldaña"
            };
            return persona.nombre + " " + persona.apellido + " " + persona.cedula.Substring(0, 1) + ".";
        }
    }
}