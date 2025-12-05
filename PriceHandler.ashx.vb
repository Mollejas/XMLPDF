Imports System
Imports System.Web
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization

Public Class PriceHandler : Implements IHttpHandler
    Public Sub ProcessRequest(context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        Dim clave = context.Request.QueryString("clave")
        Dim lista = context.Request.QueryString("lista")
        Dim precio As Decimal = 0D

        Dim connString As String =
          "Data Source=MOYRUBENS\SQLEXPRESS;Initial Catalog=VENTA;Integrated Security=True"
        Using conn As New SqlConnection(connString)
            Using cmd As New SqlCommand(
              "SELECT APRPRC
         FROM FCUAPR1
         WHERE APRCLAVE = @clave
           AND APRLISTA = @lista", conn)
                cmd.Parameters.AddWithValue("@clave", clave)
                cmd.Parameters.AddWithValue("@lista", lista)
                conn.Open()
                Dim result = cmd.ExecuteScalar()
                If result IsNot Nothing AndAlso Not Convert.IsDBNull(result) Then
                    precio = Convert.ToDecimal(result)
                End If
            End Using
        End Using

        ' Devuelve JSON { "precio": 123.45 }
        Dim js = New JavaScriptSerializer()
        context.Response.Write(js.Serialize(New With {.precio = precio}))
    End Sub

    Public ReadOnly Property IsReusable As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
End Class