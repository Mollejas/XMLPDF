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
    ' GENERAR PDF CON FORMATO DE FACTURA (SIN MOSTRAR XML)
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
                    Dim sectionFont As New iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.BOLD)
                    Dim normalFont As New iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL)
                    Dim boldFont As New iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD)

                    ' Cargar XML
                    Dim xml As New XmlDocument()
                    xml.Load(xmlPath)

                    Dim nsmgr As New XmlNamespaceManager(xml.NameTable)
                    nsmgr.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/4")

                    Dim compNode As XmlNode = xml.SelectSingleNode("//cfdi:Comprobante", nsmgr)
                    Dim emisorNode As XmlNode = xml.SelectSingleNode("//cfdi:Emisor", nsmgr)
                    Dim receptorNode As XmlNode = xml.SelectSingleNode("//cfdi:Receptor", nsmgr)

                    Dim serie As String = If(compNode?.Attributes("Serie") IsNot Nothing, compNode.Attributes("Serie").Value, "")
                    Dim folio As String = If(compNode?.Attributes("Folio") IsNot Nothing, compNode.Attributes("Folio").Value, "")
                    Dim fecha As String = If(compNode?.Attributes("Fecha") IsNot Nothing, compNode.Attributes("Fecha").Value, "")
                    Dim subtotalStr As String = If(compNode?.Attributes("SubTotal") IsNot Nothing, compNode.Attributes("SubTotal").Value, "")
                    Dim totalStr As String = If(compNode?.Attributes("Total") IsNot Nothing, compNode.Attributes("Total").Value, "")
                    Dim ivaStr As String = ""
                    Dim trasladoGlobal As XmlNode = xml.SelectSingleNode("//cfdi:Impuestos/cfdi:Traslados/cfdi:Traslado", nsmgr)
                    If trasladoGlobal IsNot Nothing Then
                        ivaStr = If(trasladoGlobal.Attributes("Importe") IsNot Nothing, trasladoGlobal.Attributes("Importe").Value, "")
                    End If

                    doc.Add(New Paragraph("FACTURA CFDI 4.0 (NO TIMBRADA)", tituloFont))
                    doc.Add(New Paragraph("LA CASA DEL AJUSTE DE MOTOR", normalFont))
                    doc.Add(Chunk.NEWLINE)

                    ' Información general de la factura
                    Dim infoTable As New PdfPTable(2)
                    infoTable.WidthPercentage = 100
                    infoTable.SetWidths(New Single() {1.5F, 1.0F})
                    infoTable.SpacingAfter = 10

                    infoTable.AddCell(New PdfPCell(New Phrase("Serie: " & serie, normalFont)) With {.Border = Rectangle.NO_BORDER})
                    infoTable.AddCell(New PdfPCell(New Phrase("Fecha: " & fecha, normalFont)) With {.Border = Rectangle.NO_BORDER, .HorizontalAlignment = Element.ALIGN_RIGHT})
                    infoTable.AddCell(New PdfPCell(New Phrase("Folio: " & folio, normalFont)) With {.Border = Rectangle.NO_BORDER})
                    infoTable.AddCell(New PdfPCell(New Phrase("Método/forma de pago: " & If(compNode?.Attributes("MetodoPago")?.Value, "") & " / " & If(compNode?.Attributes("FormaPago")?.Value, ""), normalFont)) With {
                        .Border = Rectangle.NO_BORDER,
                        .HorizontalAlignment = Element.ALIGN_RIGHT
                    })

                    doc.Add(infoTable)

                    ' Datos del emisor y receptor
                    doc.Add(New Paragraph("Datos fiscales", sectionFont))
                    doc.Add(Chunk.NEWLINE)

                    Dim fiscalTable As New PdfPTable(2)
                    fiscalTable.WidthPercentage = 100
                    fiscalTable.SetWidths(New Single() {1.0F, 1.0F})
                    fiscalTable.SpacingAfter = 10

                    Dim emisorNombre As String = If(emisorNode?.Attributes("Nombre") IsNot Nothing, emisorNode.Attributes("Nombre").Value, "")
                    Dim emisorRfc As String = If(emisorNode?.Attributes("Rfc") IsNot Nothing, emisorNode.Attributes("Rfc").Value, "")
                    Dim receptorNombre As String = If(receptorNode?.Attributes("Nombre") IsNot Nothing, receptorNode.Attributes("Nombre").Value, "")
                    Dim receptorRfc As String = If(receptorNode?.Attributes("Rfc") IsNot Nothing, receptorNode.Attributes("Rfc").Value, "")

                    fiscalTable.AddCell(New PdfPCell(New Phrase("Emisor:" & vbCrLf & emisorNombre & vbCrLf & "RFC: " & emisorRfc, normalFont)) With {.Border = Rectangle.BOX})
                    fiscalTable.AddCell(New PdfPCell(New Phrase("Receptor:" & vbCrLf & receptorNombre & vbCrLf & "RFC: " & receptorRfc, normalFont)) With {.Border = Rectangle.BOX})

                    doc.Add(fiscalTable)

                    ' Tabla de conceptos con estilo de factura
                    Dim conceptosNodes As XmlNodeList = xml.SelectNodes("//*[local-name()='Conceptos']/*[local-name()='Concepto']")
                    Dim getAttr = Function(node As XmlNode, attrName As String) As String
                                       If node Is Nothing OrElse node.Attributes Is Nothing Then Return String.Empty

                                       Dim attr = node.Attributes.Cast(Of XmlAttribute)() _
                                           .FirstOrDefault(Function(a) String.Equals(a.LocalName, attrName, StringComparison.OrdinalIgnoreCase) _
                                                                     OrElse String.Equals(a.Name, attrName, StringComparison.OrdinalIgnoreCase))

                                       Return If(attr Is Nothing, String.Empty, attr.Value)
                                   End Function
                    If conceptosNodes IsNot Nothing AndAlso conceptosNodes.Count > 0 Then
                        doc.Add(New Paragraph("Conceptos", sectionFont))

                        Dim table As New PdfPTable(5)
                        table.WidthPercentage = 100
                        table.SetWidths(New Single() {0.8F, 2.8F, 0.7F, 0.9F, 1.0F})
                        table.SpacingBefore = 5
                        table.SpacingAfter = 10

                        Dim headerBg = New BaseColor(230, 230, 230)
                        Dim headers() As String = {"Clave", "Descripción", "Cant.", "Precio", "Importe"}
                        For Each h In headers
                            table.AddCell(New PdfPCell(New Phrase(h, boldFont)) With {.BackgroundColor = headerBg})
                        Next

                        For Each concepto As XmlNode In conceptosNodes
                            table.AddCell(New Phrase(getAttr(concepto, "ClaveProdServ"), normalFont))
                            table.AddCell(New Phrase(getAttr(concepto, "Descripcion"), normalFont))
                            table.AddCell(New PdfPCell(New Phrase(getAttr(concepto, "Cantidad"), normalFont)) With {.HorizontalAlignment = Element.ALIGN_RIGHT})
                            table.AddCell(New PdfPCell(New Phrase(getAttr(concepto, "ValorUnitario"), normalFont)) With {.HorizontalAlignment = Element.ALIGN_RIGHT})
                            table.AddCell(New PdfPCell(New Phrase(getAttr(concepto, "Importe"), normalFont)) With {.HorizontalAlignment = Element.ALIGN_RIGHT})
                        Next

                        doc.Add(table)
                    End If

                    ' Totales estilo factura
                    doc.Add(New Paragraph("Resumen", sectionFont))

                    Dim totalsTable As New PdfPTable(2)
                    totalsTable.WidthPercentage = 40
                    totalsTable.HorizontalAlignment = Element.ALIGN_RIGHT
                    totalsTable.SetWidths(New Single() {1.0F, 1.0F})

                    totalsTable.AddCell(New PdfPCell(New Phrase("Subtotal", boldFont)) With {.HorizontalAlignment = Element.ALIGN_RIGHT})
                    totalsTable.AddCell(New PdfPCell(New Phrase(subtotalStr, normalFont)) With {.HorizontalAlignment = Element.ALIGN_RIGHT})

                    totalsTable.AddCell(New PdfPCell(New Phrase("IVA", boldFont)) With {.HorizontalAlignment = Element.ALIGN_RIGHT})
                    totalsTable.AddCell(New PdfPCell(New Phrase(ivaStr, normalFont)) With {.HorizontalAlignment = Element.ALIGN_RIGHT})

                    totalsTable.AddCell(New PdfPCell(New Phrase("Total", boldFont)) With {
                        .HorizontalAlignment = Element.ALIGN_RIGHT,
                        .BackgroundColor = New BaseColor(230, 230, 230)
                    })
                    totalsTable.AddCell(New PdfPCell(New Phrase(totalStr, boldFont)) With {
                        .HorizontalAlignment = Element.ALIGN_RIGHT,
                        .BackgroundColor = New BaseColor(230, 230, 230)
                    })

                    totalsTable.SpacingBefore = 5

                    doc.Add(totalsTable)
                    doc.Add(Chunk.NEWLINE)
                    doc.Add(New Paragraph("Documento generado automáticamente para revisión previa al timbrado.", normalFont))
                End Using
            End Using
        Catch ex As Exception
            Throw New Exception("Error al generar PDF: " & ex.Message, ex)
        End Try
    End Sub


End Class
