<%@ Page Title="Sellos digitales" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="Certificados.aspx.vb" Inherits="FACT.Certificados" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2 class="page-header">Sellos digitales</h2>
    <p class="text-muted">Carga tu certificado (.cer), la llave privada (.key) y la contraseña para que el CFDI se firme automáticamente.</p>

    <asp:Label ID="lblEstadoActual" runat="server" CssClass="label label-info" />
    <div class="clearfix" style="margin-bottom:12px;"></div>

    <div class="form-group">
        <label for="fuCer">Certificado (.cer)</label>
        <asp:FileUpload ID="fuCer" runat="server" CssClass="form-control" />
    </div>

    <div class="form-group">
        <label for="fuKey">Llave privada (.key)</label>
        <asp:FileUpload ID="fuKey" runat="server" CssClass="form-control" />
    </div>

    <div class="form-group">
        <label for="txtPassword">Contraseña de la llave</label>
        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" />
    </div>

    <asp:Button ID="btnGuardar" runat="server" CssClass="btn btn-primary" Text="Guardar sellos" />
    <asp:Label ID="lblMensaje" runat="server" CssClass="text-info" Style="margin-left:10px;" />
</asp:Content>
