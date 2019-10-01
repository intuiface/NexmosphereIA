using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Nexmosphere
{
   
    public class XScriptCommand
    {
        #region Static

        /// <summary>
        /// Regex pattern for generic X-Script command
        /// </summary>
        private static string XScriptCommandPattern = @"^(X|G|S|D)([0-9]{3})(A|B|S)\[(\S*)\]$";

        /// <summary>
        /// Regex pattern for specific XR ANTENNA DRIVER X-Script command
        /// </summary>
        private static string XrXScriptCommandPattern = @"^(XR)\[(\S*)\]$";

        #endregion

        #region Properties

        #region TYPE

        /// <summary>
        /// Internal reference on the TYPE property
        /// </summary>
        private string m_sTYPE = "";

        /// <summary>
        /// TYPE part string
        /// </summary>
        public string TYPE
        {
            get { return m_sTYPE; }
            set
            {
                if (m_sTYPE != value)
                {
                    m_sTYPE = value;
                    parseXScriptCmd(XScript);
                }
            }
        }

        #endregion

        #region ADDRESS

        /// <summary>
        /// Internal reference on the ADDRESS property
        /// </summary>
        private string m_sADDRESS = "";

        /// <summary>
        /// ADDRESS part string
        /// </summary>
        public string ADDRESS
        {
            get { return m_sADDRESS; }
            set
            {
                if (m_sADDRESS != value)
                {
                    m_sADDRESS = value;
                    parseXScriptCmd(XScript);
                }
            }
        }

        #endregion

        #region FORMAT

        /// <summary>
        /// Internal reference on the FORMAT property
        /// </summary>
        private string m_sFORMAT = "";

        /// <summary>
        /// FORMAT part string
        /// </summary>
        public string FORMAT
        {
            get { return m_sFORMAT; }
            set
            {
                if (m_sFORMAT != value)
                {
                    m_sFORMAT = value;
                    parseXScriptCmd(XScript);
                }
            }
        }

        #endregion

        #region COMMAND

        /// <summary>
        /// Internal reference on the COMMAND property
        /// </summary>
        private string m_sCOMMAND = "";

        /// <summary>
        /// COMMAND part string
        /// </summary>
        public string COMMAND
        {
            get { return m_sCOMMAND; }
            set
            {
                if (m_sCOMMAND != value)
                {
                    m_sCOMMAND = value;
                    parseXScriptCmd(XScript);
                }
            }
        }

        #endregion

        #region XScript

        /// <summary>
        /// X-Script command string
        /// </summary>
        public string XScript
        {
            get { return formatXScriptCmd(TYPE, ADDRESS, FORMAT, COMMAND); }
            set
            {
                string m_sXScript = formatXScriptCmd(TYPE, ADDRESS, FORMAT, COMMAND);
                if (m_sXScript != value)
                {
                    parseXScriptCmd(value);
                }
            }
        }

        #endregion

        #region IsValid

        /// <summary>
        /// Internal reference on the IsValid property
        /// </summary>
        private bool m_bIsValid = false;

        /// <summary>
        /// Indicate if the XScriptCommand is valid
        /// </summary>
        public bool IsValid
        {
            get { return m_bIsValid; }
            set
            {
                if (m_bIsValid != value)
                {
                    m_bIsValid = value;
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor of XScriptCommand
        /// </summary>
        /// <param name="strXScript">The a full line X-Script command</param>
        public XScriptCommand(string strXScript)
        {
            parseXScriptCmd(strXScript);
        }

        /// <summary>
        /// Constructor of XScriptCommand
        /// </summary>
        /// <param name="strTYPE">Type of command: X= X-Talk command, G = Generic control command, S = System command, D = Diagnostic command</param>
        /// <param name="strADDRESS">Channel address to which the command is sent: value between 0-999</param>
        /// <param name="strFORMAT">Format of the command: A = short command (number between 0-65535), B = long command (string of 0-30 characters), S = setting command (custom format for each setting)</param>
        /// <param name="strCOMMAND">The Command to send</param>
        public XScriptCommand(string strTYPE, string strADDRESS, string strFORMAT, string strCOMMAND)
        {
            parseXScriptCmd(formatXScriptCmd(strTYPE, strADDRESS, strFORMAT, strCOMMAND));            
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Constructor of XScriptCommand
        /// </summary>
        /// <param name="strTYPE">Type of command: X= X-Talk command, G = Generic control command, S = System command, D = Diagnostic command</param>
        /// <param name="strADDRESS">Channel address to which the command is sent: value between 0-999</param>
        /// <param name="strFORMAT">Format of the command: A = short command (number between 0-65535), B = long command (string of 0-30 characters), S = setting command (custom format for each setting)</param>
        /// <param name="strCOMMAND">The Command to send</param>
        /// <returns>A full line X-Script command</returns>
        private string formatXScriptCmd(string strTYPE, string strADDRESS, string strFORMAT, string strCOMMAND)
        {
            return strTYPE + strADDRESS + strFORMAT + '[' + strCOMMAND + ']';
        }

        /// <summary>
        /// Constructor of XScriptCommand
        /// </summary>
        /// <param name="strData">A string representing a full line X-Script command</param>
        /// <returns>True if XScriptCommand structure is valid, false otherwise</returns>
        private bool parseXScriptCmd(string strData)
        {
            // For generic S-Script command partern
            Regex regexPattern = new Regex(XScriptCommandPattern);
            bool patternIsValid = regexPattern.IsMatch(strData);

            // For specific X-Script command XR ANTENNA DRIVER partern
            Regex XrRegexPattern = new Regex(XrXScriptCommandPattern);
            bool XrPatternIsValid = XrRegexPattern.IsMatch(strData);

            // For generic S-Script command partern
            if (patternIsValid)
            {
                MatchCollection matches = regexPattern.Matches(strData);
                GroupCollection groups = matches[0].Groups;
                List<string> parts = new List<string>();
                for (int i = 1; i < groups.Count; i++)
                {
                    parts.Add(groups[i].Value);
                }

                TYPE = parts[0];
                ADDRESS = parts[1];
                FORMAT = parts[2];
                COMMAND = parts[3];
            }
            // For specific S-Script command XR ANTENNA DRIVER partern
            else if (XrPatternIsValid)
            {
                MatchCollection matches = XrRegexPattern.Matches(strData);
                GroupCollection groups = matches[0].Groups;
                List<string> parts = new List<string>();
                for (int i = 1; i < groups.Count; i++)
                {
                    parts.Add(groups[i].Value);
                }

                TYPE = parts[0];
                ADDRESS = "";
                FORMAT = "";
                COMMAND = parts[1];
            }

            // Update IsValid boolean 
            IsValid = patternIsValid || XrPatternIsValid;
            return IsValid;
        }

        #endregion
    }   

}
