<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  
  <xsl:output method="text"/>
  <xsl:template match="/">{
    <xsl:apply-templates select="*"/>}
  </xsl:template>
  
  <!-- Object or Element Property-->
    <xsl:template match="*">
        "<xsl:value-of select="name()"/>" : <xsl:call-template name="Properties"/>
    </xsl:template>

    <!-- Array Element -->
    <xsl:template match="*" mode="ArrayElement">
        <xsl:call-template name="Properties"/>
    </xsl:template>

    <!-- Object Properties -->
    <xsl:template name="Properties">
        <xsl:variable name="childName" select="name(*[1])"/>
        <xsl:choose>
            <xsl:when test="not(*|@*)">"<xsl:value-of select="."/>"</xsl:when>
            <xsl:when test="count(*[name()=$childName]) > 1">{ "<xsl:value-of select="$childName"/>" :[<xsl:apply-templates select="*" mode="ArrayElement"/>] }</xsl:when>
            <xsl:otherwise>{
                <xsl:apply-templates select="@*"/>
                <xsl:apply-templates select="*"/>
    }</xsl:otherwise>
        </xsl:choose>
        <xsl:if test="following-sibling::*">,</xsl:if>
    </xsl:template>

    <!-- Attribute Property-->
    <xsl:template match="@*">
      <xsl:choose>
         <xsl:when test="position()=last()">
          <xsl:text>&#10;</xsl:text>
         "<xsl:value-of select="name()"/>" : "<xsl:value-of select="."/>"
        </xsl:when>
        <xsl:otherwise>
          "<xsl:value-of select="name()"/>" : "<xsl:value-of select="."/>",
        </xsl:otherwise>
     </xsl:choose>
      
    
    </xsl:template> 


</xsl:stylesheet>