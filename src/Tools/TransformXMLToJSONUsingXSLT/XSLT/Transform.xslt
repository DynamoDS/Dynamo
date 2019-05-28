<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="text"/>
  <xsl:template match="/">
    {
    <xsl:text>"Result":</xsl:text> [
    <xsl:apply-templates select="/" mode="DLL-Content" />
    
    <xsl:call-template name="CounterNext">
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type/DeclarationDiffs[count(CustomAttribute)>0]"/>
    </xsl:call-template>
    
    <xsl:call-template name="GeneralTemplate">
      <xsl:with-param  name="prmDiffObject" select="'Class'"/>
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type/DeclarationDiffs/CustomAttribute"/>
    </xsl:call-template>
    
    <xsl:call-template name="CounterNext">
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type/DeclarationDiffs[count(DiffItem)>0]"/>
    </xsl:call-template>
    
     <xsl:call-template name="GeneralTemplate">
      <xsl:with-param  name="prmDiffObject" select="'Class'"/>
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type/DeclarationDiffs/DiffItem"/>
    </xsl:call-template>
    
    

    <!--<xsl:call-template name="CounterNext">
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type[count(DeclarationDiffs)=0]"/>
    </xsl:call-template>

    <xsl:call-template name="GeneralTemplateNoDeclarationDiffs">
      <xsl:with-param  name="prmDiffObject" select="'Class'"/>
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type[count(DeclarationDiffs)=0]"/>
    </xsl:call-template>

    <xsl:call-template name="CounterNext">
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type/Field[count(DeclarationDiffs)=0]"/>
    </xsl:call-template>

    <xsl:call-template name="GeneralTemplateNoDeclarationDiffs">
      <xsl:with-param  name="prmDiffObject" select="'Field'"/>
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type/Field[count(DeclarationDiffs)=0]"/>
    </xsl:call-template>

    <xsl:call-template name="CounterNext">
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type/Property[count(DeclarationDiffs)=0]"/>
    </xsl:call-template>

    <xsl:call-template name="GeneralTemplateNoDeclarationDiffs">
      <xsl:with-param  name="prmDiffObject" select="'Property'"/>
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type/Property[count(DeclarationDiffs)=0]"/>
    </xsl:call-template>

    <xsl:call-template name="CounterNext">
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type/Method[count(DeclarationDiffs)=0]"/>
    </xsl:call-template>

    <xsl:call-template name="GeneralTemplateNoDeclarationDiffs">
      <xsl:with-param  name="prmDiffObject" select="'Method'"/>
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type/Method[count(DeclarationDiffs)=0]"/>
    </xsl:call-template>

    <xsl:call-template name="CounterNext">
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type/DeclarationDiffs[count(CustomAttribute)>0]"/>
    </xsl:call-template>

    <xsl:call-template name="GeneralTemplateWithDeclarationDiffsCustomAttribute">
      <xsl:with-param  name="prmDiffObject" select="'Class'"/>
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type/DeclarationDiffs/CustomAttribute"/>
    </xsl:call-template>

    <xsl:call-template name="CounterNext">
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type/DeclarationDiffs[count(DiffItem)>0]"/>
    </xsl:call-template>

    <xsl:call-template name="GeneralTemplateWithDeclarationDiffsDiffItem">
      <xsl:with-param  name="prmDiffObject" select="'Class'"/>
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type/DeclarationDiffs/DiffItem"/>
    </xsl:call-template>

    <xsl:call-template name="CounterNext">
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type/Field/DeclarationDiffs[count(CustomAttribute)>0]"/>
    </xsl:call-template>

    <xsl:call-template name="GeneralTemplateWithDeclarationDiffsCustomAttribute">
      <xsl:with-param  name="prmDiffObject" select="'Field'"/>
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type/Field/DeclarationDiffs/CustomAttribute"/>
    </xsl:call-template>

    <xsl:call-template name="CounterNext">
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type/Field/DeclarationDiffs[count(DiffItem)>0]"/>
    </xsl:call-template>

    <xsl:call-template name="GeneralTemplateWithDeclarationDiffsDiffItem">
      <xsl:with-param  name="prmDiffObject" select="'Field'"/>
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type/Field/DeclarationDiffs/DiffItem"/>
    </xsl:call-template>

    <xsl:call-template name="CounterNext">
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type/Property/DeclarationDiffs[count(CustomAttribute)>0]"/>
    </xsl:call-template>

    <xsl:call-template name="GeneralTemplateWithDeclarationDiffsCustomAttribute">
      <xsl:with-param  name="prmDiffObject" select="'Property'"/>
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type/Property/DeclarationDiffs/CustomAttribute"/>
    </xsl:call-template>

    <xsl:call-template name="CounterNext">
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type/Property/DeclarationDiffs[count(DiffItem)>0]"/>
    </xsl:call-template>

    <xsl:call-template name="GeneralTemplateWithDeclarationDiffsDiffItem">
      <xsl:with-param  name="prmDiffObject" select="'Property'"/>
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type/Property/DeclarationDiffs/DiffItem"/>
    </xsl:call-template>

    <xsl:call-template name="CounterNext">
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type/Method/DeclarationDiffs[count(CustomAttribute)>0]"/>
    </xsl:call-template>

    <xsl:call-template name="GeneralTemplateWithDeclarationDiffsCustomAttribute">
      <xsl:with-param  name="prmDiffObject" select="'Method'"/>
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type/Method/DeclarationDiffs/CustomAttribute"/>
    </xsl:call-template>

    <xsl:call-template name="CounterNext">
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type/Method/DeclarationDiffs[count(DiffItem)>0]"/>
    </xsl:call-template>

    <xsl:call-template name="GeneralTemplateWithDeclarationDiffsDiffItem">
      <xsl:with-param  name="prmDiffObject" select="'Method'"/>
      <xsl:with-param  name="prmLongPath" select="/Assembly/Module/Type/Method/DeclarationDiffs/DiffItem"/>
    </xsl:call-template>

    -->]

    }
  </xsl:template>

  <xsl:template name="GeneralTemplateNoDeclarationDiffs">
    <xsl:param name="prmDiffObject" select="''"/>
    <xsl:param name="prmLongPath" select="''"/>
    <xsl:for-each select="$prmLongPath">
      {
      <xsl:text>"DiffObject": </xsl:text>"<xsl:value-of select="$prmDiffObject"/>",
      <xsl:text>"DiffType":   </xsl:text>"<xsl:value-of select="@DiffType"/>",
      <xsl:text>"Trace":      </xsl:text><xsl:choose>
        <xsl:when test="$prmDiffObject = 'Class'">
          "<xsl:value-of select="../@Name"/>"
        </xsl:when>
        <xsl:otherwise>
          "<xsl:value-of select="../../@Name"/>-<xsl:value-of select="../@Name"/>"
        </xsl:otherwise>
      </xsl:choose>,
      <xsl:text>"Name":       </xsl:text>"<xsl:value-of select="@Name"/>",
      <xsl:text>"Description":</xsl:text>{}
      <xsl:choose>
        <xsl:when test="position()=last()">}</xsl:when>
        <xsl:otherwise>},</xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="GeneralTemplateWithDeclarationDiffsCustomAttribute">
    <xsl:param name="prmDiffObject" select="''"/>
    <xsl:param name="prmLongPath" select="''"/>
    <xsl:for-each select="$prmLongPath">

      {
      <xsl:text>"DiffObject": </xsl:text>"<xsl:value-of select="$prmDiffObject"/>",
      <xsl:text>"DiffType":   </xsl:text>"<xsl:value-of select="@DiffType"/>",
      <xsl:text>"Trace":      </xsl:text><xsl:choose>
        <xsl:when test="$prmDiffObject = 'Class'">
          "<xsl:value-of select="../../../@Name"/>"
        </xsl:when>
        <xsl:otherwise>
          "<xsl:value-of select="../../../../@Name"/>-<xsl:value-of select="../../../@Name"/>"
        </xsl:otherwise>
      </xsl:choose>,
      <xsl:text>"Name":       </xsl:text><xsl:choose>
        <xsl:when test="$prmDiffObject = 'Class'">
          "<xsl:value-of select="../../@Name"/>"
        </xsl:when>
        <xsl:otherwise>
          "<xsl:value-of select="../../@Name"/>"
        </xsl:otherwise>
      </xsl:choose>,
      <xsl:text>"Description":</xsl:text>
      {
      <xsl:text>"DiffType":   </xsl:text>"<xsl:value-of select="@DiffType"/>",
      <xsl:text>"Text":       </xsl:text>"<xsl:value-of select="@Name"/>"
      }
      <xsl:choose>
        <xsl:when test="position()=last()">}</xsl:when>
        <xsl:otherwise>},</xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="GeneralTemplateWithDeclarationDiffsDiffItem">
    <xsl:param name="prmDiffObject" select="''"/>
    <xsl:param name="prmLongPath" select="''"/>
    <xsl:for-each select="$prmLongPath">
      {
      <xsl:text>"DiffObject": </xsl:text>"<xsl:value-of select="$prmDiffObject"/>",
      <xsl:text>"DiffType":   </xsl:text>"<xsl:value-of select="@DiffType"/>",
      <xsl:text>"Trace":      </xsl:text><xsl:choose>
        <xsl:when test="$prmDiffObject = 'Class'">
          "<xsl:value-of select="../../../@Name"/>"
        </xsl:when>
        <xsl:otherwise>
          "<xsl:value-of select="../../../../@Name"/>-<xsl:value-of select="../../../@Name"/>"
        </xsl:otherwise>
      </xsl:choose>,
      <xsl:text>"Name":       </xsl:text><xsl:choose>
        <xsl:when test="$prmDiffObject = 'Class'">
          "<xsl:value-of select="../../@Name"/>"
        </xsl:when>
        <xsl:otherwise>
          "<xsl:value-of select="../../@Name"/>"
        </xsl:otherwise>
      </xsl:choose>,
      <xsl:text>"Description":</xsl:text>
      {
      <xsl:text>"DiffType":   </xsl:text>"<xsl:value-of select="@DiffType"/>",
      <xsl:text>"Text":       </xsl:text>"<xsl:value-of select="text()"/>"
      }
      <xsl:choose>
        <xsl:when test="position()=last()">}</xsl:when>
        <xsl:otherwise>},</xsl:otherwise>
      </xsl:choose>

    </xsl:for-each>
  </xsl:template>
  
  <xsl:template name="GeneralTemplate">
    <xsl:param name="prmDiffObject" select="''"/>
    <xsl:param name="prmLongPath" select="''"/>
     <xsl:for-each select="$prmLongPath">
      {
      <xsl:text>"DiffObject": </xsl:text>"<xsl:value-of select="$prmDiffObject"/>",
      <xsl:text>"DiffType":   </xsl:text>"<xsl:value-of select="@DiffType"/>",
      <xsl:text>"Trace":      </xsl:text><xsl:choose>
        <xsl:when test="$prmDiffObject = 'Class'">
          "<xsl:value-of select="../../../@Name"/>"
        </xsl:when>
        <xsl:otherwise>
          "<xsl:value-of select="../../../../@Name"/>-<xsl:value-of select="../../../@Name"/>"
        </xsl:otherwise>
      </xsl:choose>,
      <xsl:text>"Name":       </xsl:text><xsl:choose>
        <xsl:when test="$prmDiffObject = 'Class'">
          "<xsl:value-of select="../../@Name"/>"
        </xsl:when>
        <xsl:otherwise>
          "<xsl:value-of select="../../@Name"/>"
        </xsl:otherwise>
      </xsl:choose>,
      <xsl:text>"Description":</xsl:text>
      {
      <xsl:text>"DiffType":   </xsl:text>"<xsl:value-of select="@DiffType"/>",
      <xsl:variable name="testx" select="text()"/>
      <xsl:choose>
        
        <xsl:when test="$testx != ''">
          <xsl:text>"Text DiffItem":</xsl:text>"<xsl:value-of select="text()"/>"        
        </xsl:when>
        <xsl:otherwise>
         <xsl:text>"Text Custom": </xsl:text>"<xsl:value-of select="@Name"/>"
        </xsl:otherwise>
      </xsl:choose>
      
      }
      <xsl:choose>
        <xsl:when test="position()=last()">}</xsl:when>
        <xsl:otherwise>},</xsl:otherwise>
      </xsl:choose>

    </xsl:for-each>
  
  </xsl:template>

  <xsl:template match="*" mode="DLL-Content">

    <xsl:variable name="c" select="count(DeclarationDiffs)"/>
    <xsl:variable name="DiffObject" select="'Assembly'"/>
    <xsl:variable name="NamePath" select="/Assembly/@Name"/>
    <xsl:choose>
      <xsl:when test="$c &gt; 0">

        <xsl:for-each select="DeclarationDiffs/DiffItem">
          {
          <xsl:text>"DiffObject": </xsl:text>"<xsl:value-of select="$DiffObject"/>",
          <xsl:text>"DiffType":   </xsl:text>"<xsl:value-of select="/Assembly/@DiffType"/>",
          <xsl:text>"Trace":      </xsl:text>"",
          <xsl:text>"Name":       </xsl:text>"<xsl:value-of select="$NamePath"/>",
          <xsl:text>"Description":</xsl:text>
          {
          <xsl:text>"DiffType":   </xsl:text>"<xsl:value-of select="@DiffType"/>",
          <xsl:text>"Text":       </xsl:text>"<xsl:value-of select="text()"/>"
          }
          <xsl:choose>
            <xsl:when test="position()=last()">}</xsl:when>
            <xsl:otherwise>},</xsl:otherwise>
          </xsl:choose>
        </xsl:for-each>

        <xsl:variable name="c2" select="count(DeclarationDiffs/CustomAttribute)"/>
        <xsl:choose>
          <xsl:when test="$c2 &gt; 0">
            <xsl:text>,</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text></xsl:text>
          </xsl:otherwise>
        </xsl:choose>


        <xsl:for-each select="DeclarationDiffs/CustomAttribute">
          {
          <xsl:text>"DiffObject": </xsl:text>"<xsl:value-of select="$DiffObject"/>",
          <xsl:text>"DiffType":   </xsl:text>"<xsl:value-of select="/Assembly/@DiffType"/>",
          <xsl:text>"Trace":      </xsl:text>"",
          <xsl:text>"Name":       </xsl:text>"<xsl:value-of select="/Assembly/@Name"/>",
          <xsl:text>"Description":</xsl:text>
          {
          <xsl:text>"DiffType":   </xsl:text>"<xsl:value-of select="@DiffType"/>",
          <xsl:text>"Text":       </xsl:text>"<xsl:value-of select="@Name"/>"
          }
          <xsl:choose>
            <xsl:when test="position()=last()">}</xsl:when>
            <xsl:otherwise>},</xsl:otherwise>
          </xsl:choose>
        </xsl:for-each>


      </xsl:when>
      <xsl:otherwise>
        {
        <xsl:text>"DiffObject": </xsl:text>"<xsl:value-of select="$DiffObject"/>",
        <xsl:text>"DiffType":   </xsl:text>"<xsl:value-of select="/Assembly/@DiffType"/>",
        <xsl:text>"Trace":      </xsl:text>"",
        <xsl:text>"Name":       </xsl:text>"<xsl:value-of select="/Assembly/@Name"/>",
        <xsl:text>"Description":</xsl:text>{}
        <xsl:choose>
          <xsl:when test="position()=last()">}</xsl:when>
          <xsl:otherwise>},</xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="CounterNext">
    <xsl:param name="prmLongPath" select="''"/>
    <xsl:choose>
      <xsl:when test="count($prmLongPath)  &gt; 0">,</xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>

  </xsl:template>



</xsl:stylesheet>