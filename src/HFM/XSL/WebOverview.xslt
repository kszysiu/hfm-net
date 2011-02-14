<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
   <xsl:output method="html" encoding="utf-8" doctype-public="-//W3C//DTD HTML 4.01 Transitional//EN" doctype-system="http://www.w3.org/TR/html4/loose.dtd" />
   <xsl:template match="Overview">
      <html>
         <head>
            <title>Folding Client Overview</title>
            <meta http-equiv="Pragma" content="no-cache" />
            <meta http-equiv="Cache-Control" content="no-cache" />
            <link rel="stylesheet" type="text/css" href="$CSSFILE" />
         </head>
         <body>
            <table class="Overview" width="32%">
               <tr>
                  <td class="Heading" width="50%">Overview</td>
                  <td class="Plain" width="50%">
                     <a href="summary.html">
                        Summary<xsl:text disable-output-escaping="yes">&amp;nbsp;</xsl:text>Page
                     </a>
                  </td>
               </tr>
               <tr>
                  <td class="LeftCol">
                     Total<xsl:text disable-output-escaping="yes">&amp;nbsp;</xsl:text>Folding<xsl:text disable-output-escaping="yes">&amp;nbsp;</xsl:text>Clients
                  </td>
                  <td class="RightCol">
                     <xsl:value-of select="TotalHosts"/>
                  </td>
               </tr>
               <tr>
                  <td class="AltLeftCol">
                     Working<xsl:text disable-output-escaping="yes">&amp;nbsp;</xsl:text>Clients
                  </td>
                  <td class="AltRightCol">
                     <xsl:value-of select="GoodHosts"/>
                  </td>
               </tr>
               <tr>
                  <td class="LeftCol">
                     Non-Working<xsl:text disable-output-escaping="yes">&amp;nbsp;</xsl:text>Clients
                  </td>
                  <td class="RightCol">
                     <xsl:value-of select="BadHosts"/>
                  </td>
               </tr>
               <tr>
                  <td class="AltLeftCol">
                     Total<xsl:text disable-output-escaping="yes">&amp;nbsp;</xsl:text>PPD
                  </td>
                  <td class="AltRightCol">
                     <xsl:value-of select="EstPPD"/>
                  </td>
               </tr>
               <tr>
                  <td class="LeftCol">
                     Total<xsl:text disable-output-escaping="yes">&amp;nbsp;</xsl:text>PPW
                  </td>
                  <td class="RightCol">
                     <xsl:value-of select="EstPPW"/>
                  </td>
               </tr>
               <tr>
                  <td class="AltLeftCol">
                     Total<xsl:text disable-output-escaping="yes">&amp;nbsp;</xsl:text>UPD
                  </td>
                  <td class="AltRightCol">
                     <xsl:value-of select="EstUPD"/>
                  </td>
               </tr>
               <tr>
                  <td class="LeftCol">
                     Total<xsl:text disable-output-escaping="yes">&amp;nbsp;</xsl:text>UPW
                  </td>
                  <td class="RightCol">
                     <xsl:value-of select="EstUPW"/>
                  </td>
               </tr>
               <tr>
                  <td class="AltLeftCol">
                     Average<xsl:text disable-output-escaping="yes">&amp;nbsp;</xsl:text>PPD
                  </td>
                  <td class="AltRightCol">
                     <xsl:value-of select="AvEstPPD"/>
                  </td>
               </tr>
               <tr>
                  <td class="LeftCol">
                     Average<xsl:text disable-output-escaping="yes">&amp;nbsp;</xsl:text>PPW
                  </td>
                  <td class="RightCol">
                     <xsl:value-of select="AvEstPPW"/>
                  </td>
               </tr>
               <tr>
                  <td class="AltLeftCol">
                     Average<xsl:text disable-output-escaping="yes">&amp;nbsp;</xsl:text>UPD
                  </td>
                  <td class="AltRightCol">
                     <xsl:value-of select="AvEstUPD"/>
                  </td>
               </tr>
               <tr>
                  <td class="LeftCol">
                     Average<xsl:text disable-output-escaping="yes">&amp;nbsp;</xsl:text>UPW
                  </td>
                  <td class="RightCol">
                     <xsl:value-of select="AvEstUPW"/>
                  </td>
               </tr>
               <tr>
                  <td class="AltLeftCol">Completed Units</td>
                  <td class="AltRightCol">
                     <xsl:value-of select="Completed"/>
                  </td>
               </tr>
               <tr>
                  <td class="LeftCol">Failed Units</td>
                  <td class="RightCol">
                     <xsl:value-of select="Failed"/>
                  </td>
               </tr>
               <tr>
                  <td class="AltLeftCol">Total Completed Units</td>
                  <td class="AltRightCol">
                     <xsl:value-of select="TotalCompleted"/>
                  </td>
               </tr>
               <tr>
                  <td class="Plain" colspan="2" align="center">
                     <a href="mobile.html">
                        Mobile<xsl:text disable-output-escaping="yes">&amp;nbsp;</xsl:text>Version
                     </a>
                  </td>
               </tr>
               <tr>
                  <td class="Plain" colspan="2" align="center">
                  </td>
               </tr>
               <tr>
                  <td class="Plain" colspan="2" align="center">
                     Page rendered by <a href="http://code.google.com/p/hfm-net/">HFM.NET</a><xsl:text disable-output-escaping="yes">&amp;nbsp;</xsl:text><xsl:value-of select="HFMVersion"/> on <xsl:value-of select="LastUpdatedDate"/>
                     at <xsl:value-of select="LastUpdatedTime"/>
                  </td>
               </tr>
            </table>
         </body>
      </html>
   </xsl:template>
</xsl:stylesheet>
