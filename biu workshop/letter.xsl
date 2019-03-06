<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns="http://www.w3.org/1999/xhtml"
    xmlns:tei="http://www.tei-c.org/ns/1.0"
    xpath-default-namespace="http://www.tei-c.org/ns/1.0"
    version="2.0">
    <!--<xsl:import href="file:/Users/yaeln/Downloads/oxygenframeworks/xml/tei/stylesheet/html/html.xsl"/>-->
    <xsl:template match="/">
        <html>
            <head>
                <title>
                    <xsl:apply-templates select="/TEI/teiHeader/fileDesc/titleStmt/title" />
                       
                    
                </title>
            </head>
            <body dir="rtl">
                <h2>
                    <xsl:apply-templates select="/TEI/teiHeader/fileDesc/titleStmt/title" />
                    
                </h2>
                <xsl:apply-templates select="//teiHeader"/>
                <hr/>
                
                <xsl:apply-templates select="//body"/>
                <hr/>
                <h2>אישים מוזכרים</h2>
                <ul>
                    <xsl:apply-templates select="//text//persName" mode="toc">
                        <xsl:sort select="."/>
                    </xsl:apply-templates>
                </ul>
            </body>
        </html>
    </xsl:template>
    
    <xsl:template match="opener">
        <p><em><xsl:apply-templates/></em></p>
        
    </xsl:template>
    
    <xsl:template match="closer">
        <p ><em><xsl:apply-templates/></em></p>
        
    </xsl:template>
    
      
    
    <xsl:template match="dateline">
        <span style="font:em; text-align:left"><xsl:apply-templates/></span>
    </xsl:template>
    <xsl:template match="lb">
        <br/>
    </xsl:template>
    
    <xsl:template match="p">
        <p><xsl:apply-templates/></p>
    </xsl:template>
    <xsl:template match="placeName">
        <span class="place" style="background-color: offwhite"><xsl:apply-templates/></span>
    </xsl:template>
    <xsl:template match="persName">
        <span class="person" style="background-color:grey"><xsl:apply-templates/></span>
    </xsl:template>
    <xsl:template match="note">
        <br/>
        <p style="font-size:70%; "><xsl:apply-templates/></p>
        
    </xsl:template>
    <xsl:template match="signed">
        <p style="font:em; text-align:center"><xsl:apply-templates/></p>
        
    </xsl:template>
    
    <xsl:template match="//text//persName" mode="toc">
        <li>
            <xsl:apply-templates/>
        </li>
    </xsl:template>  
    
</xsl:stylesheet>