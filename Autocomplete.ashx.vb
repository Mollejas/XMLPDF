Imports System
Imports System.Web
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization
Imports System.Text

Public Class Autocomplete
    Implements IHttpHandler

    Public Sub ProcessRequest(context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        Dim term = context.Request.QueryString("term")
        If term Is Nothing Then term = String.Empty
        term = term.Trim()

        If String.IsNullOrEmpty(term) Then
            context.Response.Write("[]")
            Return
        End If

        ' Separa el término en palabras (ignorando espacios extra)
        Dim terms() = term.Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)

        ' Construye el WHERE dinámico: cada palabra debe aparecer en ArtCLAVE o ArtDescri
        Dim whereSb As New StringBuilder("1=1")
        For i As Integer = 0 To terms.Length - 1
            whereSb.Append(" AND (ArtCLAVE LIKE @t").Append(i).Append(" OR ArtDescri LIKE @t").Append(i).Append(")")
        Next

        Dim sql As String = "SELECT ArtCLAVE, ArtDescri " &
                            "FROM FCAART1 " &
                            "WHERE " & whereSb.ToString() & " " &
                            "ORDER BY ArtCLAVE;"

        Dim results As New List(Of Object)
        Dim connString As String = "Data Source=MOYRUBENS\SQLEXPRESS;Initial Catalog=VENTA;Integrated Security=True"

        Using conn As New SqlConnection(connString)
            Using cmd As New SqlCommand(sql, conn)
                For i As Integer = 0 To terms.Length - 1
                    cmd.Parameters.AddWithValue("@t" & i, "%" & terms(i) & "%")
                Next

                conn.Open()
                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()
                        results.Add(New With {
                            .clave = rdr("ArtCLAVE").ToString(),
                            .descripcion = rdr("ArtDescri").ToString(),
                            .label = rdr("ArtCLAVE").ToString() & " - " & rdr("ArtDescri").ToString(),
                            .value = rdr("ArtCLAVE").ToString()
                        })
                    End While
                End Using
            End Using
        End Using

        Dim js = New JavaScriptSerializer()
        context.Response.Write(js.Serialize(results))
    End Sub

    Public ReadOnly Property IsReusable As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class
