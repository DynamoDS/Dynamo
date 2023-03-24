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
      <!--NOTE! THESE excludes do not affect the harvest folder-->

    <!--Exclude *.xml-->
    <xsl:key name="xml-search" match="wix:Component[contains(@Directory, 'INSTALLDIR') and (contains(wix:File/@Source, '.xml') or contains(wix:File/@Source, '.XML')) and not(contains(wix:File/@Source, '_DynamoCustomization.xml') or contains(wix:File/@Source, '.Migrations.xml'))]" use="@Id"/>
    <xsl:template match="wix:Component[key('xml-search', @Id)]" />
  
    <!--Exclude *.pdb-->
    <xsl:key name="pdb-search" match="wix:Component[contains(wix:File/@Source, '.pdb')]" use="@Id"/>
    <xsl:template match="wix:Component[key('pdb-search', @Id)]" />

    <!--Exclude Test*.dll/exe-->
    <xsl:key name="testdll-search" match="wix:Component[contains(wix:File/@Source, 'Test') and (contains(wix:File/@Source, '.dll') or contains(wix:File/@Source, '.exe'))]" use="@Id"/>
    <xsl:template match="wix:Component[key('testdll-search', @Id)]" />

    <!--Exclude FFITarget.dll-->
    <xsl:key name="ffitarget-search" match="wix:Component[contains(wix:File/@Source, 'FFITarget.dll')]" use="@Id"/>
    <xsl:template match="wix:Component[key('ffitarget-search', @Id)]" />

    <!--Exclude binariestosign.txt-->
    <xsl:key name="binariestosign-search" match="wix:Component[contains(wix:File/@Source, 'binariestosign.txt')]" use="@Id"/>
    <xsl:template match="wix:Component[key('binariestosign-search', @Id)]" />
	
    <!--Exclude RevitAddinUtility.dll-->
    <xsl:key name="RevitAddinUtility-search" match="wix:Component[contains(wix:File/@Source, 'RevitAddinUtility.dll')]" use="@Id"/>
    <xsl:template match="wix:Component[key('RevitAddinUtility-search', @Id)]" />
	
    <!--Exclude DynamoAddInGenerator.exe-->
    <xsl:key name="DynamoAddInGenerator-search" match="wix:Component[contains(wix:File/@Source, 'DynamoAddInGenerator.exe')]" use="@Id"/>
    <xsl:template match="wix:Component[key('DynamoAddInGenerator-search', @Id)]" />

    <!--Exclude Moq.dll-->
    <xsl:key name="moq-search" match="wix:Component[contains(wix:File/@Source, 'Moq')]" use="@Id"/>
    <xsl:template match="wix:Component[key('moq-search', @Id)]" />

    <!--Exclude nunit*.dll-->
    <xsl:key name="nunit-search" match="wix:Component[contains(wix:File/@Source, 'nunit')]" use="@Id"/>
    <xsl:template match="wix:Component[key('nunit-search', @Id)]" />
	
    <!--Exclude *.vshost.exe*-->
    <xsl:key name="vshost-search" match="wix:Component[contains(wix:File/@Source, 'vshost.exe')]" use="@Id"/>
    <xsl:template match="wix:Component[key('vshost-search', @Id)]" />

    <!--Exclude TestResult.xml-->
    <xsl:key name="TestResult-search" match="wix:Component[contains(wix:File/@Source, 'TestResult.xml')]" use="@Id"/>
    <xsl:template match="wix:Component[key('TestResult-search', @Id)]" />

    <!--Exclude 'revit_2015' folders-->
    <xsl:key name="revit_2015-search" match="wix:Component[contains(wix:File/@Source, '\Revit_2015\')]" use="@Id"/>
    <xsl:template match="wix:Directory[@Name = 'Revit_2015']" />
    <xsl:template match="wix:Component[key('revit_2015-search', @Id)]" />
  
    <!--Exclude 'revit_2016' folders-->
    <xsl:key name="revit_2016-search" match="wix:Component[contains(wix:File/@Source, '\Revit_2016\')]" use="@Id"/>
    <xsl:template match="wix:Directory[@Name = 'Revit_2016']" />
    <xsl:template match="wix:Component[key('revit_2016-search', @Id)]" />
  
    <!--Exclude 'revit_2017' folders-->
    <xsl:key name="revit_2017-search" match="wix:Component[contains(wix:File/@Source, '\Revit_2017\')]" use="@Id"/>
    <xsl:template match="wix:Directory[@Name = 'Revit_2017']" />
    <xsl:template match="wix:Component[key('revit_2017-search', @Id)]" />

    <!--Exclude 'revit_2018' folders-->
    <xsl:key name="revit_2018-search" match="wix:Component[contains(wix:File/@Source, '\Revit_2018\')]" use="@Id"/>
    <xsl:template match="wix:Directory[@Name = 'Revit_2018']" />
    <xsl:template match="wix:Component[key('revit_2018-search', @Id)]" />

    <!--Exclude 'int' folders-->
    <xsl:template match="wix:Directory[@Name = 'int']" />
    <xsl:key name="int-search" match="wix:Component[contains(wix:File/@Source, '\int\')]" use="@Id"/>
    <xsl:template match="wix:Component[key('int-search', @Id)]" />
  
    <!--Exclude 'samples' folders-->
    <xsl:template match="wix:Directory[@Name = 'samples']" />
    <xsl:key name="samples-search" match="wix:Component[contains(wix:File/@Source, '\samples\')]" use="@Id"/>
    <xsl:template match="wix:Component[key('samples-search', @Id)]" />
</xsl:stylesheet>
