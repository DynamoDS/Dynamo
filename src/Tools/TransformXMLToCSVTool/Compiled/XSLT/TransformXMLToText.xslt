<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="text" indent="yes"/>
  <xsl:variable name='newline'>
    <xsl:text>
  </xsl:text>
  </xsl:variable>

  <xsl:template match="/">
    
    <xsl:variable name='dllName'>
      <xsl:call-template name="substring-after-last">
        <xsl:with-param name="string" select="/test-results/test-suite/results/test-suite/@name"/>
        <xsl:with-param name="delimiter" select="'\'"/>
      </xsl:call-template>
    </xsl:variable>
    
    <xsl:for-each select="//test-suite[@type='TestFixture' or @type='ParameterizedTest'  and @executed='True']">

      <xsl:if test="contains(../../../../../../@name,'.dll')">
        <xsl:value-of select="concat(@name,',', results/test-case/@name,',',../../../../../../@name)" />
      </xsl:if>

      <xsl:if test="contains(../../../../@name,'.dll')">
        <xsl:value-of select="concat(@name,',', results/test-case/@name,',',../../../../@name)" />
      </xsl:if>

      <xsl:if test="contains(../../../../../../../../@name,'.dll')">
        <xsl:value-of select="concat(@name,',', results/test-case/@name,',',../../../../../../../../@name)" />
      </xsl:if>
      
      <xsl:if test="contains(../../../../../../../../../../@name,'.dll')">
        <xsl:value-of select="concat(@name,',', results/test-case/@name,',',../../../../../../../../../../@name)" />
      </xsl:if>
                             
      <xsl:text>&#13;</xsl:text>
    </xsl:for-each>
  </xsl:template>


  <xsl:template name="substring-after-last">
    <xsl:param name="string" />
    <xsl:param name="delimiter" />
    <xsl:choose>
      <xsl:when test="contains($string, $delimiter)">
        <xsl:call-template name="substring-after-last">
          <xsl:with-param name="string"
            select="substring-after($string, $delimiter)" />
          <xsl:with-param name="delimiter" select="$delimiter" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$string" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="substring-before-last">
    <xsl:param name="list"/>
    <xsl:param name="delimiter"/>
    <xsl:choose>
      <xsl:when test="contains($list, $delimiter)">

        <xsl:value-of select="substring-before($list,$delimiter)"/>
        <xsl:choose>
          <xsl:when test="contains(substring-after($list,$delimiter),$delimiter)">
            <xsl:value-of select="$delimiter"/>
          </xsl:when>
        </xsl:choose>
        <xsl:call-template name="substring-before-last">
          <xsl:with-param name="list" select="substring-after($list,$delimiter)"/>
          <xsl:with-param name="delimiter" select="$delimiter"/>
        </xsl:call-template>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>
