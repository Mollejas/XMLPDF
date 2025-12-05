Imports System
Imports System.IO
Imports System.Globalization
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Xml
Imports System.Data.SqlClient

Imports Newtonsoft.Json

Imports iTextSharp.text
Imports iTextSharp.text.pdf



Public Class FACTURA
    Inherits System.Web.UI.Page



    ' MISMA CADENA QUE USAS EN LOS HANDLERS
    Private ReadOnly connString As String = "Data Source=MOYRUBENS\SQLEXPRESS;Initial Catalog=VENTA;Integrated Security=True"

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            ' Inicialización si hace falta
        End If
    End Sub

    ' ===================================================================
    ' CLICK DEL BOTÓN: GENERAR XML CFDI 4.0 + PDF
    ' ===================================================================
    Protected Sub btnGenerarXMLPDF_Click(sender As Object, e As EventArgs)
        Dim facturaJson As String = hf_JSON_FACTURA.Value

        If String.IsNullOrEmpty(facturaJson) Then
            ClientScript.RegisterStartupScript(Me.GetType(), "alert", "alert('No hay datos para generar CFDI');", True)
            Return
        End If

        Dim factura As FacturaModel = Nothing

        Try
            factura = JsonConvert.DeserializeObject(Of FacturaModel)(facturaJson)
        Catch ex As Exception
            ClientScript.RegisterStartupScript(Me.GetType(), "alert", "alert('Error al leer datos de factura: " &
                                               ex.Message.Replace("'", "") & "');", True)
            Return
        End Try

        If factura Is Nothing OrElse factura.conceptos Is Nothing OrElse factura.conceptos.Count = 0 Then
            ClientScript.RegisterStartupScript(Me.GetType(), "alert", "alert('No hay conceptos en la factura.');", True)
            Return
        End If

        ' Carpeta de salida
        Dim carpetaCFDI As String = Server.MapPath("~/CFDI")
        If Not Directory.Exists(carpetaCFDI) Then
            Directory.CreateDirectory(carpetaCFDI)
        End If

        Dim nombreBase As String = "Factura_" &
                                       (If(String.IsNullOrWhiteSpace(factura.clienteClave), "SINCLAVE", factura.clienteClave)) &
                                       "_" & DateTime.Now.ToString("yyyyMMddHHmmss")

        Dim xmlPath As String = Path.Combine(carpetaCFDI, nombreBase & ".xml")
        Dim pdfPath As String = Path.Combine(carpetaCFDI, nombreBase & ".pdf")

        Try
            GenerarCFDI40_XML(factura, xmlPath)
            GenerarPDF(xmlPath, pdfPath)

            Dim msg As String = "XML y PDF generados correctamente:\n" &
                                    xmlPath.Replace("\", "\\") & "\n" &
                                    pdfPath.Replace("\", "\\")
            ClientScript.RegisterStartupScript(Me.GetType(), "alert", "alert('" & msg & "');", True)
        Catch ex As Exception
            ClientScript.RegisterStartupScript(Me.GetType(), "alert", "alert('Error al generar archivos: " &
                                               ex.Message.Replace("'", "") & "');", True)
        End Try
    End Sub

    ' ===================================================================
    ' MODELOS PARA DESERIALIZAR JSON
    ' ===================================================================
    Public Class FacturaModel
        Public Property clienteClave As String
        Public Property clienteNombre As String
        Public Property lista As String
        Public Property vendedor As String
        Public Property usoCFDI As String
        Public Property metodoPago As String
        Public Property formaPago As String
        Public Property obs As String
        Public Property obs1 As String
        Public Property obs2 As String
        Public Property obs3 As String
        Public Property conceptos As List(Of Concepto)
    End Class

    Public Class Concepto
        Public Property clave As String
        Public Property descripcion As String
        Public Property cantidad As Decimal
        Public Property precio As Decimal
        Public Property importe As Decimal
    End Class

    ' Datos de cliente obtenidos de FCACLI1
    Public Class ClienteBD
        Public Property Clave As String
        Public Property Nombre As String
        Public Property RFC As String
    End Class

    ' ===================================================================
    ' OBTENER DATOS DEL CLIENTE (CLIRFC, NOMBRE) DESDE FCACLI1
    ' ===================================================================
    Private Function ObtenerClientePorClave(clave As String) As ClienteBD
        If String.IsNullOrWhiteSpace(clave) Then
            Return Nothing
        End If

        Dim cli As ClienteBD = Nothing

        Using conn As New SqlConnection(connString)
            Dim sql As String = "SELECT CLICLAVE, CLINOMBRE, CLIRFC FROM FCACLI1 WHERE CLICLAVE = @clave"
            Using cmd As New SqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@clave", clave.Trim())
                conn.Open()
                Using rdr = cmd.ExecuteReader()
                    If rdr.Read() Then
                        cli = New ClienteBD() With {
                                .Clave = rdr("CLICLAVE").ToString().Trim(),
                                .Nombre = rdr("CLINOMBRE").ToString().Trim(),
                                .RFC = rdr("CLIRFC").ToString().Trim()
                            }
                    End If
                End Using
            End Using
        End Using

        Return cli
    End Function

    ' ===================================================================
    ' GENERAR XML CFDI 4.0 (SIN TIMBRAR, PREVIO PARA PAC)
    '  - AQUÍ SE OBTIENE EL RFC DEL CLIENTE DESDE FCACLI1.CLIRFC
    ' ===================================================================
    Private Sub GenerarCFDI40_XML(f As FacturaModel, xmlPath As String)
        Dim xml As New XmlDocument()

        Dim xmlDecl = xml.CreateXmlDeclaration("1.0", "UTF-8", Nothing)
        xml.AppendChild(xmlDecl)

        Dim root = xml.CreateElement("cfdi:Comprobante")
        xml.AppendChild(root)

        ' Namespaces obligatorios
        root.SetAttribute("xmlns:cfdi", "http://www.sat.gob.mx/cfd/4")
        root.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance")
        root.SetAttribute("xsi:schemaLocation",
                              "http://www.sat.gob.mx/cfd/4 http://www.sat.gob.mx/sitio_internet/cfd/4/cfdv40.xsd")

        ' Cálculo de totales
        Dim subTotal As Decimal = f.conceptos.Sum(Function(c) c.importe)
        Dim iva As Decimal = Math.Round(subTotal * 0.16D, 2)
        Dim total As Decimal = subTotal + iva

        Dim ci As CultureInfo = CultureInfo.InvariantCulture

        ' Atributos del Comprobante
        root.SetAttribute("Version", "4.0")
        root.SetAttribute("Serie", "A")                 ' Cambia a tu serie real
        root.SetAttribute("Folio", "1")                 ' Cambia a folio real
        root.SetAttribute("Fecha", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"))
        root.SetAttribute("SubTotal", subTotal.ToString("0.00", ci))
        root.SetAttribute("Moneda", "MXN")
        root.SetAttribute("Total", total.ToString("0.00", ci))
        root.SetAttribute("TipoDeComprobante", "I")     ' Ingreso
        root.SetAttribute("MetodoPago", If(String.IsNullOrWhiteSpace(f.metodoPago), "PUE", f.metodoPago))
        root.SetAttribute("FormaPago", "01")            ' Ejemplo: 01 Efectivo (ajusta según tu catálogo)
        root.SetAttribute("Exportacion", "01")          ' No aplica
        root.SetAttribute("LugarExpedicion", "64000")   ' CP del emisor

        ' Emisor (DEBES PONER TUS DATOS REALES)
        Dim emisor = xml.CreateElement("cfdi:Emisor")
        emisor.SetAttribute("Rfc", "AAA010101AAA")          ' RFC emisor real
        emisor.SetAttribute("Nombre", "LA CASA DEL AJUSTE DE MOTOR")
        emisor.SetAttribute("RegimenFiscal", "601")         ' Ejemplo: 601 General de Ley Personas Morales
        root.AppendChild(emisor)

        ' Receptor (se obtiene RFC desde FCACLI1.CLIRFC)
        Dim datosCli As ClienteBD = ObtenerClientePorClave(f.clienteClave)

        Dim receptorRFC As String = "XAXX010101000"
        Dim receptorNombre As String = f.clienteNombre
        If datosCli IsNot Nothing Then
            If Not String.IsNullOrWhiteSpace(datosCli.RFC) Then
                receptorRFC = datosCli.RFC
            End If
            If Not String.IsNullOrWhiteSpace(datosCli.Nombre) Then
                receptorNombre = datosCli.Nombre
            End If
        End If

        If String.IsNullOrWhiteSpace(receptorNombre) Then
            receptorNombre = "PUBLICO EN GENERAL"
        End If

        Dim receptor = xml.CreateElement("cfdi:Receptor")
        receptor.SetAttribute("Rfc", receptorRFC)
        receptor.SetAttribute("Nombre", receptorNombre)
        receptor.SetAttribute("UsoCFDI", If(String.IsNullOrWhiteSpace(f.usoCFDI), "G03", f.usoCFDI))
        receptor.SetAttribute("DomicilioFiscalReceptor", "64000")
        receptor.SetAttribute("RegimenFiscalReceptor", "616")   ' Ejemplo: 616 - Sin obligaciones fiscales
        root.AppendChild(receptor)

        ' Conceptos
        Dim conceptosNode = xml.CreateElement("cfdi:Conceptos")
        root.AppendChild(conceptosNode)

        For Each c In f.conceptos
            If c.cantidad <= 0 OrElse c.importe <= 0 Then Continue For

            Dim con = xml.CreateElement("cfdi:Concepto")
            con.SetAttribute("ClaveProdServ", If(String.IsNullOrWhiteSpace(c.clave), "01010101", c.clave))
            con.SetAttribute("NoIdentificacion", c.clave)
            con.SetAttribute("Cantidad", c.cantidad.ToString("0.######", ci))
            con.SetAttribute("ClaveUnidad", "H87")  ' Pza
            con.SetAttribute("Unidad", "PZA")
            con.SetAttribute("Descripcion", c.descripcion)
            con.SetAttribute("ValorUnitario", c.precio.ToString("0.00", ci))
            con.SetAttribute("Importe", c.importe.ToString("0.00", ci))
            con.SetAttribute("ObjetoImp", "02")     ' Sí objeto de impuesto

            ' Impuesto por concepto (16%)
            Dim impuestosC = xml.CreateElement("cfdi:Impuestos")
            Dim trasladosC = xml.CreateElement("cfdi:Traslados")
            Dim trasladoC = xml.CreateElement("cfdi:Traslado")

            Dim baseC As Decimal = c.importe
            Dim ivaC As Decimal = Math.Round(baseC * 0.16D, 2)

            trasladoC.SetAttribute("Base", baseC.ToString("0.00", ci))
            trasladoC.SetAttribute("Impuesto", "002")        ' IVA
            trasladoC.SetAttribute("TipoFactor", "Tasa")
            trasladoC.SetAttribute("TasaOCuota", "0.160000")
            trasladoC.SetAttribute("Importe", ivaC.ToString("0.00", ci))

            trasladosC.AppendChild(trasladoC)
            impuestosC.AppendChild(trasladosC)
            con.AppendChild(impuestosC)

            conceptosNode.AppendChild(con)
        Next

        ' Impuestos globales
        Dim impGlobal = xml.CreateElement("cfdi:Impuestos")
        impGlobal.SetAttribute("TotalImpuestosTrasladados", iva.ToString("0.00", ci))

        Dim trasladosGlobal = xml.CreateElement("cfdi:Traslados")
        Dim trasladoGlobal = xml.CreateElement("cfdi:Traslado")
        trasladoGlobal.SetAttribute("Base", subTotal.ToString("0.00", ci))
        trasladoGlobal.SetAttribute("Impuesto", "002")
        trasladoGlobal.SetAttribute("TipoFactor", "Tasa")
        trasladoGlobal.SetAttribute("TasaOCuota", "0.160000")
        trasladoGlobal.SetAttribute("Importe", iva.ToString("0.00", ci))

        trasladosGlobal.AppendChild(trasladoGlobal)
        impGlobal.AppendChild(trasladosGlobal)
        root.AppendChild(impGlobal)

        xml.Save(xmlPath)
    End Sub

    ' ===================================================================
    ' GENERAR PDF SIMPLE BASADO EN EL XML
    ' ===================================================================
    ' ===================================================================
    ' GENERAR PDF SIMPLE BASADO EN EL XML
    ' ===================================================================
    Private Sub GenerarPDF(xmlPath As String, pdfPath As String)
        If String.IsNullOrEmpty(xmlPath) OrElse Not File.Exists(xmlPath) Then
            Throw New Exception("El archivo XML no existe: " & xmlPath)
        End If

        If String.IsNullOrEmpty(pdfPath) Then
            Throw New Exception("La ruta del PDF está vacía.")
        End If

        Dim pdfDir As String = Path.GetDirectoryName(pdfPath)
        If Not Directory.Exists(pdfDir) Then
            Directory.CreateDirectory(pdfDir)
        End If

        Try
            Using fs As New FileStream(pdfPath, FileMode.Create, FileAccess.Write, FileShare.None)
                Using doc As New iTextSharp.text.Document(iTextSharp.text.PageSize.LETTER, 36, 36, 36, 36)
                    iTextSharp.text.pdf.PdfWriter.GetInstance(doc, fs)

                    doc.Open()

                    ' Fuentes
                    Dim tituloFont As New iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 16, iTextSharp.text.Font.BOLD)
                    Dim normalFont As New iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL)
                    Dim smallFont As New iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.COURIER, 7)

                    ' Encabezado
                    doc.Add(New iTextSharp.text.Paragraph("FACTURA CFDI 4.0 (NO TIMBRADA)", tituloFont))
                    doc.Add(New iTextSharp.text.Paragraph("LA CASA DEL AJUSTE DE MOTOR", normalFont))
                    doc.Add(New iTextSharp.text.Paragraph(" ", normalFont))

                    ' Cargar XML
                    Dim xml As New XmlDocument()
                    xml.Load(xmlPath)

                    Dim nsmgr As New XmlNamespaceManager(xml.NameTable)
                    nsmgr.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/4")

                    Dim compNode As XmlNode = xml.SelectSingleNode("//cfdi:Comprobante", nsmgr)

                    If compNode IsNot Nothing Then
                        Dim serie As String = If(compNode.Attributes("Serie") IsNot Nothing, compNode.Attributes("Serie").Value, "")
                        Dim folio As String = If(compNode.Attributes("Folio") IsNot Nothing, compNode.Attributes("Folio").Value, "")
                        Dim fecha As String = If(compNode.Attributes("Fecha") IsNot Nothing, compNode.Attributes("Fecha").Value, "")
                        Dim total As String = If(compNode.Attributes("Total") IsNot Nothing, compNode.Attributes("Total").Value, "")

                        doc.Add(New iTextSharp.text.Paragraph("Serie: " & serie, normalFont))
                        doc.Add(New iTextSharp.text.Paragraph("Folio: " & folio, normalFont))
                        doc.Add(New iTextSharp.text.Paragraph("Fecha: " & fecha, normalFont))
                        doc.Add(New iTextSharp.text.Paragraph("Total: " & total, normalFont))
                        doc.Add(New iTextSharp.text.Paragraph(" ", normalFont))
                    End If

                    ' Receptor
                    Dim receptorNode As XmlNode = xml.SelectSingleNode("//cfdi:Receptor", nsmgr)
                    If receptorNode IsNot Nothing Then
                        doc.Add(New iTextSharp.text.Paragraph("Receptor: " & If(receptorNode.Attributes("Nombre")?.Value, ""), normalFont))
                        doc.Add(New iTextSharp.text.Paragraph("RFC: " & If(receptorNode.Attributes("Rfc")?.Value, ""), normalFont))
                        doc.Add(New iTextSharp.text.Paragraph(" ", normalFont))
                    End If

                    ' Tabla de conceptos
                    Dim conceptosNodes As XmlNodeList = xml.SelectNodes("//cfdi:Conceptos/cfdi:Concepto", nsmgr)
                    If conceptosNodes IsNot Nothing AndAlso conceptosNodes.Count > 0 Then
                        Dim table As New iTextSharp.text.pdf.PdfPTable(4)
                        table.WidthPercentage = 100
                        table.SetWidths(New Single() {3.0F, 1.0F, 1.0F, 1.0F})

                        table.AddCell(New Phrase("Descripción", normalFont))
                        table.AddCell(New Phrase("Cantidad", normalFont))
                        table.AddCell(New Phrase("Precio", normalFont))
                        table.AddCell(New Phrase("Importe", normalFont))

                        For Each concepto As XmlNode In conceptosNodes
                            table.AddCell(New Phrase(If(concepto.Attributes("Descripcion")?.Value, ""), normalFont))
                            table.AddCell(New Phrase(If(concepto.Attributes("Cantidad")?.Value, ""), normalFont))
                            table.AddCell(New Phrase(If(concepto.Attributes("ValorUnitario")?.Value, ""), normalFont))
                            table.AddCell(New Phrase(If(concepto.Attributes("Importe")?.Value, ""), normalFont))
                        Next

                        doc.Add(table)
                        doc.Add(New iTextSharp.text.Paragraph(" ", normalFont))
                    End If

                    ' Resumen del XML completo para depuración
                    doc.Add(New iTextSharp.text.Paragraph("XML generado (vista rápida):", normalFont))
                    doc.Add(New iTextSharp.text.Paragraph(" ", normalFont))

                    Dim xmlTexto As String = File.ReadAllText(xmlPath)
                    doc.Add(New iTextSharp.text.Paragraph(xmlTexto, smallFont))
                End Using
            End Using
        Catch ex As Exception
            Throw New Exception("Error al generar PDF: " & ex.Message, ex)
        End Try
    End Sub

End Class