Imports System
Imports System.IO
Imports System.Web.UI

Namespace FACT
    Public Partial Class Certificados
        Inherits Page

        Private ReadOnly certDir As String = HttpContext.Current.Server.MapPath("~/App_Data/Certificados")

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                lblEstadoActual.Text = ObtenerEstadoActual()
            End If
        End Sub

        Private Function ObtenerEstadoActual() As String
            Dim cerPath = Path.Combine(certDir, "certificado.cer")
            Dim keyPath = Path.Combine(certDir, "llave.key")
            Dim pwdPath = Path.Combine(certDir, "password.txt")

            If File.Exists(cerPath) AndAlso File.Exists(keyPath) AndAlso File.Exists(pwdPath) Then
                Dim fecha = File.GetLastWriteTime(cerPath)
                Return $"Sellos cargados el {fecha:dd/MM/yyyy HH:mm}" & " - sin timbrar"
            End If

            Return "No hay sellos cargados."
        End Function

        Protected Sub btnGuardar_Click(sender As Object, e As EventArgs) Handles btnGuardar.Click
            lblMensaje.Text = String.Empty
            lblMensaje.CssClass = "text-info"

            If Not fuCer.HasFile OrElse Not fuKey.HasFile Then
                lblMensaje.Text = "Selecciona el archivo .cer y el archivo .key."
                lblMensaje.CssClass = "text-danger"
                Return
            End If

            If String.IsNullOrWhiteSpace(txtPassword.Text) Then
                lblMensaje.Text = "Captura la contraseña de la llave privada (.key)."
                lblMensaje.CssClass = "text-danger"
                Return
            End If

            Directory.CreateDirectory(certDir)

            Dim cerPath = Path.Combine(certDir, "certificado.cer")
            Dim keyPath = Path.Combine(certDir, "llave.key")
            Dim pwdPath = Path.Combine(certDir, "password.txt")

            fuCer.SaveAs(cerPath)
            fuKey.SaveAs(keyPath)
            File.WriteAllText(pwdPath, txtPassword.Text.Trim())

            lblMensaje.Text = "Sellos guardados correctamente. Ya puedes generar el CFDI con sello, certificado y número de certificado."
            lblMensaje.CssClass = "text-success"
            lblEstadoActual.Text = ObtenerEstadoActual()
        End Sub
    End Class
End Namespace
