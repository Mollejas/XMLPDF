<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:cfdi="http://www.sat.gob.mx/cfd/4">
    <xsl:output method="text" omit-xml-declaration="yes" version="1.0" />

    <xsl:template match="/">
        <xsl:text>||</xsl:text>
        <xsl:apply-templates select="//*[local-name()='Comprobante']" />
    </xsl:template>

    <xsl:template match="cfdi:Comprobante|*[local-name()='Comprobante']">
        <xsl:value-of select="@Version" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@Serie" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@Folio" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@Fecha" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@NoCertificado" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@SubTotal" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@Moneda" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@Total" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@TipoDeComprobante" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@MetodoPago" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@FormaPago" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@Exportacion" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@LugarExpedicion" />
        <xsl:text>|</xsl:text>
        <xsl:apply-templates select="cfdi:Emisor|*[local-name()='Emisor']" />
        <xsl:apply-templates select="cfdi:Receptor|*[local-name()='Receptor']" />
        <xsl:apply-templates select="cfdi:Conceptos/cfdi:Concepto|*[local-name()='Conceptos']/*[local-name()='Concepto']" />
        <xsl:apply-templates select="cfdi:Impuestos/cfdi:Traslados/cfdi:Traslado|*[local-name()='Impuestos']/*[local-name()='Traslados']/*[local-name()='Traslado']" mode="impuestos" />
        <xsl:text>|</xsl:text>
    </xsl:template>

    <xsl:template match="cfdi:Emisor|*[local-name()='Emisor']">
        <xsl:value-of select="@Rfc" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@Nombre" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@RegimenFiscal" />
        <xsl:text>|</xsl:text>
    </xsl:template>

    <xsl:template match="cfdi:Receptor|*[local-name()='Receptor']">
        <xsl:value-of select="@Rfc" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@Nombre" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@DomicilioFiscalReceptor" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@RegimenFiscalReceptor" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@UsoCFDI" />
        <xsl:text>|</xsl:text>
    </xsl:template>

    <xsl:template match="cfdi:Concepto|*[local-name()='Concepto']">
        <xsl:value-of select="@ClaveProdServ" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@NoIdentificacion" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@Cantidad" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@ClaveUnidad" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@Unidad" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@Descripcion" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@ValorUnitario" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@Importe" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@ObjetoImp" />
        <xsl:text>|</xsl:text>
        <xsl:apply-templates select="cfdi:Impuestos/cfdi:Traslados/cfdi:Traslado|*[local-name()='Impuestos']/*[local-name()='Traslados']/*[local-name()='Traslado']" />
    </xsl:template>

    <xsl:template match="cfdi:Traslado|*[local-name()='Traslado']">
        <xsl:value-of select="@Base" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@Impuesto" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@TipoFactor" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@TasaOCuota" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@Importe" />
        <xsl:text>|</xsl:text>
    </xsl:template>

    <xsl:template match="cfdi:Traslado|*[local-name()='Traslado']" mode="impuestos">
        <xsl:value-of select="@Base" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@Impuesto" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@TipoFactor" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@TasaOCuota" />
        <xsl:text>|</xsl:text>
        <xsl:value-of select="@Importe" />
        <xsl:text>|</xsl:text>
    </xsl:template>
</xsl:stylesheet>
