<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns="http://www.w3.org/1999/xhtml"
    xmlns:tei="http://www.tei-c.org/ns/1.0"
    xpath-default-namespace="http://www.tei-c.org/ns/1.0"
    version="2.0">
    <xsl:template match="/">
        <html>
            <head>
                <title>
                    <xsl:apply-templates select="/TEI/teiHeader/fileDesc/titleStmt/title" />
                       
                    
                </title>
            </head>
            <body dir="rtl">
<!--              In the next line, as value of "select" enter xpath:
-->                <xsl:apply-templates select="//body//p"/>
                
                
            </body>
        </html>
    </xsl:template>
    
</xsl:stylesheet>