<xsl:stylesheet version="1.0"
            xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
            xmlns:msxsl="urn:schemas-microsoft-com:xslt"
            exclude-result-prefixes="msxsl"
            xmlns:wix="http://schemas.microsoft.com/wix/2006/wi"
            xmlns:my="my:my">

    <xsl:output method="xml" indent="yes" />

    <xsl:strip-space elements="*"/>

    <xsl:template match="@*|node()">
        <xsl:copy>
            <xsl:apply-templates select="@*|node()"/>
        </xsl:copy>
    </xsl:template>

    <xsl:template match="wix:DirectoryRef[@Id='DYNAMOREVIT_INSTALLDIR']/wix:Directory/@Name['Release']">
        <xsl:attribute name="Name">
            <xsl:value-of select="'Dynamo Revit'"/>
        </xsl:attribute>
    </xsl:template>

</xsl:stylesheet>