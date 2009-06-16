/*
 * HFM.NET - String (Regex) Helper Class
 * Copyright (C) 2009 Ryan Harlamert (harlam357)
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; version 2
 * of the License. See the included file GPLv2.TXT.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System.Text;
using System.Text.RegularExpressions;

namespace HFM.Helpers
{
   public static class StringOps
   {
      #region Constants
      private const string ValidNameFirst =  "[a-zA-Z0-9\\+=\\-_\\$&^\\[\\]]";
      private const string ValidNameMiddle = "[a-zA-Z0-9\\+=\\-_\\$&^\\[\\] \\.]";
      private const string ValidNameLast =   "[a-zA-Z0-9\\+=\\-_\\$&^\\[\\]]";
      
      private const string ValidFileName = @"[^\\/:*?""<>|\r\n]*";

      private const string ValidWinPath = @"(?:\b[a-z]:|\\\\[a-z0-9.$_-]+\\[a-z0-9.`~!@#$%^&()_-]+)\\(?:[^\\/:*?""<>|\r\n]+\\)*";
      private const string ValidUnixPath = @"^(?:/[a-z0-9\-._~%!$&'()*+,;=:@/]*/)*$";
      
      private const string ValidFtpServer = @"^[a-z0-9\-._%]+$";
      
      private const string ValidHttpURL = "(https?|file|smb)://[-A-Z0-9+&@#/%?=~_|$!:,.;]*/";

      private const string ValidMatchHttpOrFtpUrl =       @"\b(?<protocol>https?|ftp)://(?<domain>[-A-Z0-9.]+)(?<file>/[-A-Z0-9+&@#/%=~_|!:,.;]*)";
      private const string ValidMatchFtpWithUserPassUrl = @"\b(?<protocol>ftp)://(?<username>[A-Z0-9+&@#/%=~_|!:,.;]+):(?<password>[A-Z0-9+&@#/%=~_|!:,.;]+)@(?<domain>[-A-Z0-9.]+)(?<file>/[-A-Z0-9+&@#/%=~_|!:,.;]*)";
      #endregion
   
      #region Methods
      /// <summary>
      /// Validate Instance Name
      /// </summary>
      /// <param name="val">String to validate</param>
      public static bool ValidateInstanceName(string val)
      {
         Regex rValidName = new Regex(string.Format("^{0}{1}+{2}$", ValidNameFirst, ValidNameMiddle, ValidNameLast), RegexOptions.Singleline);
         return rValidName.IsMatch(val);
      }

      /// <summary>
      /// Clean Instance Name
      /// </summary>
      /// <param name="val">String to clean</param>
      /// <returns>Cleaned Instance Name</returns>
      public static string CleanInstanceName(string val)
      {
         Regex rValidFirst = new Regex(ValidNameFirst, RegexOptions.Singleline);
         Regex rValidMiddle = new Regex(ValidNameMiddle, RegexOptions.Singleline);
         Regex rValidLast = new Regex(ValidNameLast, RegexOptions.Singleline);
         
         StringBuilder sbldr = new StringBuilder(val.Length);
         for (int i = 0; i < val.Length; i++)
         {
            if (i == 0)
            {
               if (rValidFirst.IsMatch(val[i].ToString()))
               {
                  sbldr.Append(val[i]);
               }
            }
            else if (i == val.Length - 1)
            {
               if (rValidLast.IsMatch(val[i].ToString()))
               {
                  sbldr.Append(val[i]);
               }
            }
            else
            {
               if (rValidMiddle.IsMatch(val[i].ToString()))
               {
                  sbldr.Append(val[i]);
               }
            }
         }
         
         return sbldr.ToString();
      }

      /// <summary>
      /// Validate File Name
      /// </summary>
      /// <param name="val">String to validate</param>
      public static bool ValidateFileName(string val)
      {
         Regex rValidFileName = new Regex(ValidFileName, RegexOptions.Singleline | RegexOptions.IgnoreCase);
         
         return rValidFileName.IsMatch(val);
      }

      /// <summary>
      /// Validate PathInstance Path
      /// </summary>
      /// <param name="val">String to validate</param>
      public static bool ValidatePathInstancePath(string val)
      {
         System.Diagnostics.Debug.WriteLine(System.Environment.OSVersion.VersionString);
         
         Regex rValidPathWin = new Regex(ValidWinPath, RegexOptions.Singleline | RegexOptions.IgnoreCase);
         Regex rValidPathUnix = new Regex(ValidUnixPath, RegexOptions.Singleline | RegexOptions.IgnoreCase);

         return (rValidPathWin.IsMatch(val) || rValidPathUnix.IsMatch(val));
      }

      /// <summary>
      /// Validate FTP Server Name
      /// </summary>
      /// <param name="val">String to validate</param>
      public static bool ValidateFtpServerName(string val)
      {
         Regex rValidFtpServer = new Regex(ValidFtpServer, RegexOptions.Singleline | RegexOptions.IgnoreCase);
         
         return rValidFtpServer.IsMatch(val);
      }

      /// <summary>
      /// Validate FTPInstance Path
      /// </summary>
      /// <param name="val">String to validate</param>
      public static bool ValidateFtpPath(string val)
      {
         Regex rValidPathUnix = new Regex(ValidUnixPath, RegexOptions.Singleline | RegexOptions.IgnoreCase); 
         
         return rValidPathUnix.IsMatch(val);
      }

      /// <summary>
      /// Validate HTTPInstance URL
      /// </summary>
      /// <param name="val">String to validate</param>
      public static bool ValidateHttpURL(string val)
      {
         Regex rValidHttpURL = new Regex(ValidHttpURL, RegexOptions.Singleline | RegexOptions.IgnoreCase);

         return rValidHttpURL.IsMatch(val);
      }
      
      /// <summary>
      /// Match HTTP or FTP URL String
      /// </summary>
      /// <param name="val"></param>
      public static Match MatchHttpOrFtpUrl(string val)
      {
         Regex rValidHttpOrFtpUrl = new Regex(ValidMatchHttpOrFtpUrl, RegexOptions.Singleline | RegexOptions.IgnoreCase);
         
         return rValidHttpOrFtpUrl.Match(val);
      }

      /// <summary>
      /// Validate FTP URL String with Username and Password
      /// </summary>
      /// <param name="val">String to validate</param>
      public static bool ValidateFtpWithUserPassUrl(string val)
      {
         return MatchFtpWithUserPassUrl(val).Success;
      }

      /// <summary>
      /// Match FTP URL String with Username and Password
      /// </summary>
      /// <param name="val"></param>
      public static Match MatchFtpWithUserPassUrl(string val)
      {
         Regex rValidFtpWithUserPassUrl = new Regex(ValidMatchFtpWithUserPassUrl, RegexOptions.Singleline | RegexOptions.IgnoreCase);

         return rValidFtpWithUserPassUrl.Match(val);
      }
      #endregion
   }
}