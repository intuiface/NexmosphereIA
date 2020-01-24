using System;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.ComponentModel;

namespace Nexmosphere
{


    public class Nexmosphere : INotifyPropertyChanged, IDisposable
    {

        #region INotifyPropertyChanged

        /// <summary>
        /// Event fired when a property is changed
        /// </summary>
        /// <remarks>Defined in the INotifyPropertyChanged interface</remarks>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Helper function raising a PropertyChanged event
        /// </summary>
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion

        #region attributes

        /// <summary>
        /// The main control for communicating through the RS-232 port
        /// </summary>
        private SerialPort m_refComPort = new SerialPort();

        /// <summary>
        /// Internal serial buffer
        /// </summary>
        private string m_refSerialBuffer = "";

        /// <summary>
        /// Timer to schedule connection check
        /// </summary>
        private System.Timers.Timer m_checkConnectTimer;


        #endregion

        #region Properties

        #region CheckConnectionInterval

        /// <summary>
        /// Internal reference on the CheckConnectionInterval property
        /// </summary>
        protected int m_refCheckConnectionInterval = 1000;

        /// <summary>
        /// Time interval, in milliseconds, to check the serial port connection status
        /// </summary>
        public int CheckConnectionInterval
        {
            get { return m_refCheckConnectionInterval; }
            set
            {
                if (m_refCheckConnectionInterval != value)
                {
                    m_refCheckConnectionInterval = value;
                    NotifyPropertyChanged("CheckConnectionInterval");
                }
            }
        }

        #endregion

        #region BaudRate

        /// <summary>
        /// Internal reference on the BaudRate property
        /// </summary>
        protected int m_refBaudRate = 115200;

        /// <summary>
        /// Baud-Rate of the serial port (currently 115200 for all Nexmosphere devices)
        /// </summary>
        public int BaudRate
        {
            get { return m_refBaudRate; }
            set
            {
                if (m_refBaudRate != value)
                {
                    m_refBaudRate = value;
                    NotifyPropertyChanged("BaudRate");
                }
            }
        }

        #endregion

        #region PortName

        /// <summary>
        /// Internal reference on the PortName property
        /// </summary>
        protected string m_refPortName = "COM1";

        /// <summary>
        /// Gets or sets the PortName property.
        /// </summary>
        public string PortName
        {
            get { return m_refPortName; }
            set
            {
                if (m_refPortName != value)
                {
                    m_refPortName = value;
                    NotifyPropertyChanged("PortName");
                }
            }
        }

        #endregion

        #region PortNameOtherPlatforms

        /// <summary>
        /// Internal reference on the PortNameOtherPlatforms property
        /// </summary>
        protected string m_refPortNameOtherPlatforms = "/usb/001";

        /// <summary>
        /// Gets or sets the PortNameOtherPlatforms property.
        /// </summary>
        public string PortNameOtherPlatforms
        {
            get { return m_refPortNameOtherPlatforms; }
            set
            {
                if (m_refPortNameOtherPlatforms != value)
                {
                    m_refPortNameOtherPlatforms = value;
                    NotifyPropertyChanged("PortNameOtherPlatforms");
                }
            }
        }

        #endregion

        #region IsConnected

        /// <summary>
        /// Internal reference on the IsConnected property
        /// </summary>
        private bool m_bIsConnected = false;

        /// <summary>
        /// Connection status with the Nexmosphere board
        /// </summary>
        public bool IsConnected
        {
            get { return m_bIsConnected; }
            set
            {               
                if (m_bIsConnected != value)
                {
                    m_bIsConnected = value;
                    NotifyPropertyChanged("IsConnected");
                    if (value)
                    {                 
                        AddOutputLog("Port " + m_refComPort.PortName + " opened");
                    }
                    else
                    {
                        AddOutputLog("Port " + m_refComPort.PortName + " closed");
                    }
                }
            }
        }

        #endregion

        #region AutoReconnect

        /// <summary>
        /// Internal reference on the AutoReconnect property
        /// </summary>
        private bool m_bAutoReconnect = true;

        /// <summary>
        /// Auto reconnect status
        /// </summary>
        public bool AutoReconnect
        {
            get { return m_bAutoReconnect; }
            set
            {
                if (m_bAutoReconnect != value)
                {
                    m_bAutoReconnect = value;
                    NotifyPropertyChanged("AutoReconnect");
                }
            }
        }

        #endregion

        #region OutputLog

        /// <summary>
        /// Internal reference on the OutputLog property
        /// </summary>
        protected string m_refOutputLog = "";

        /// <summary>
        /// Gets or sets the OutputLog property.
        /// </summary>
        public string OutputLog
        {
            get { return m_refOutputLog; }
            set
            {
                if (m_refOutputLog != value)
                {
                    m_refOutputLog = value;
                    string[] Log = m_refOutputLog.Split('\n');
                    NotifyPropertyChanged("OutputLog");
                }
            }
        }

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Event raised when a X-Script command is sent by a Nexmosphere device
        /// </summary>
        public event XScriptCommandEventArgsHandler XScriptCommand;

        /// <summary>
        /// Raises the RaiseXScriptCommand event
        /// </summary>
        /// <param name="xScriptCommand">The X-Script command to send to IntuiFace Player</param>
        private void RaiseXScriptCommand(XScriptCommand xScriptCommand)
        {
            if (XScriptCommand != null)
            {
                XScriptCommandEventArgs args = new XScriptCommandEventArgs() { FullLine = xScriptCommand.XScript, TYPE = xScriptCommand.TYPE, ADDRESS = xScriptCommand.ADDRESS, FORMAT = xScriptCommand.FORMAT, COMMAND = xScriptCommand.COMMAND };
                XScriptCommand(this, args);
            }
        }
        
        #endregion

        #region Constructors

        /// <summary>
        /// Constructor of the Nexmosphere class
        /// </summary>
        public Nexmosphere()
        {
            AddOutputLog("Start Nexmosphere Interface Asset");
            // Suscribe to when data is received through the serial port
            m_refComPort.DataReceived += new SerialDataReceivedEventHandler(SerialDataReceived);
            // Suscribe to when an error is received through the serial port
            m_refComPort.ErrorReceived += new SerialErrorReceivedEventHandler(SerialErrorReceived);

            // Create a Timer, used to check connection status periodically 
            m_checkConnectTimer = new System.Timers.Timer(CheckConnectionInterval);  
            m_checkConnectTimer.Elapsed += new System.Timers.ElapsedEventHandler(CheckConnectTick);

        }

        #endregion

        #region Public methods (visible in IntuiFace)

        /// <summary>
        /// Configure and open serial port connection
        /// </summary>
        public void OpenSerialPort()
        {
            AddOutputLog("Try to open Serial Port " + this.PortName);

            // If the port connection is open, close it.
            if (m_refComPort.IsOpen)
            {
                m_refComPort.Close();
            }

            // Set the port's settings
            m_refComPort.BaudRate = this.BaudRate;
            m_refComPort.PortName = this.PortName;
            m_refComPort.Parity = Parity.None;
            m_refComPort.DataBits = 8;
            m_refComPort.StopBits = StopBits.One;
            m_refComPort.NewLine = "\r\n";

            // Clean the Serial Buffer
            m_refSerialBuffer = "";

            // Open SerialPort connection
            OpenSerialPortConnection();
            

            if (m_refComPort.IsOpen)
            {
                // Set the IsConnected status to true
                IsConnected = true;
            }
            else
            {
                // Set the IsConnected status to false
                IsConnected = false;
            }

            // Enable the connection check Timer 
            m_checkConnectTimer.Enabled = true;
        }


        /// <summary>
        /// Close SerialPort connection
        /// </summary>
        public void CloseSerialPort()
        {
            AddOutputLog("Try to close Serial Port " + this.PortName);
            // If the port is open, close it.
            if (m_refComPort.IsOpen)
            {
                // Close the port connection
                m_refComPort.Close();
                // Set the IsConnected status to false
                IsConnected = false;
            }
            // Disable the connection check Timer
            m_checkConnectTimer.Enabled = false;
        }

        
        /// <summary>
        /// Send X-Script Command line
        /// </summary>
        /// <param name="XScriptCommand">The a full line X-Script command</param>
        public void SendXScriptCommand_FullLine(string XScriptCommand)
        {
            XScriptCommand xScriptCmd = new XScriptCommand(XScriptCommand);

            // Check if the X-Script command is valid
            if (xScriptCmd.IsValid)
            {
                // Send the X-Script command to the serial port
                WriteBytes(StringToBytes(xScriptCmd.XScript + "\r\n"));
                AddOutputLog("Send X-Script command: " + xScriptCmd.XScript);
            }
            else
            {
                AddOutputLog("Failed to send X-Script command: " + xScriptCmd.XScript + " is not valid!");
            }
        }

        /// <summary>
        /// Send X-Script Command
        /// </summary>
        /// <param name="TYPE">Type of command: X= X-Talk command, G = Generic control command, S = System command, D = Diagnostic command</param>
        /// <param name="ADDRESS">Channel address to which the command is sent: value between 0-999</param>
        /// <param name="FORMAT">Format of the command: A = short command (number between 0-65535), B = long command (string of 0-30 characters), S = setting command (custom format for each setting)</param>
        /// <param name="COMMAND">The Command to send</param>
        public void SendXScriptCommand(string TYPE, string ADDRESS, string FORMAT, string COMMAND)
        {
            // Send X-Script Command line
            SendXScriptCommand_FullLine(TYPE + ADDRESS + FORMAT + "[" + COMMAND + "]");
        }

        #endregion

        #region Private methods & callbacks

        /// <summary>
        /// Open SerialPort connection
        /// </summary>
        private void OpenSerialPortConnection()
        {
            try
            {
                // Open the serial port connection
                m_refComPort.Open();
            }
            catch (Exception ex)
            {
                AddOutputLog("Could not open the COM port. Most likely it is already in use, has been removed, or is unavailable");
                AddOutputLog("Exception message: " + ex.Message);
            }
        }

        /// <summary>
        /// Add new data to the serial buffer
        /// </summary>
        /// <param name="data">Data to add into the buffer</param>
        private void addToSerialBuffer(string data)
        {
            // Add data to the serial buffer
            m_refSerialBuffer += data;
            // Parse the serial buffer
            parseSerialBuffer();
        }

        /// <summary>
        /// Parse serial buffer, to find any complete command line on the internal serial buffer 
        /// </summary>
        private void parseSerialBuffer()
        {
            lock (m_refSerialBuffer)
            {
                // Find index of the first End Of Line
                int indexEOL = m_refSerialBuffer.IndexOf("\r\n");
                // If any End Of Line is present, we can extract this complete serial command
                while (indexEOL != -1)
                {
                    // Get a new serial command line
                    string newSerialCommand = m_refSerialBuffer.Substring(0, indexEOL);

                    // Update serial buffer, by removing this last serial command line
                    m_refSerialBuffer = m_refSerialBuffer.Remove(0, indexEOL + 2);
                    indexEOL = m_refSerialBuffer.IndexOf("\r\n");

                    // Process the new serial command line
                    processNewSerialCommand(newSerialCommand);
                }
            }
        }

        /// <summary>
        /// Process a new serial command line
        /// </summary>
        /// <param name="newLine"></param>
        private void processNewSerialCommand(string newLine)
        {

            XScriptCommand xScriptCmd = new XScriptCommand(newLine);

            if (xScriptCmd.IsValid){
                RaiseXScriptCommand(xScriptCmd);
                AddOutputLog("Received X-Script command: " + xScriptCmd.XScript);
            }
            else
            {
                AddOutputLog("Received an invalid X-Script command: " + newLine);
            }
            
        }

        /// <summary>
        /// Encodes all the characters in the specified input string, to a sequence of bytes
        /// </summary>
        /// <param name="input">The string containing the characters to encode</param>
        /// <returns>A byte array containing the results of encoding the input set of characters</returns>
        private static byte[] StringToBytes(string input)
        {
            return Encoding.ASCII.GetBytes(input);
        }

        /// <summary>
        /// Writes the specified byte array to the serial port
        /// </summary>
        /// <param name="array">Byte array to write</param>
        private void WriteBytes(byte[] array)
        {
            m_refComPort.Write(array, 0, array.Length);
        }

        /// <summary>
        /// Add a new line on the OutputLog
        /// </summary>
        /// <param name="log">The new line to add</param>
        private void AddOutputLog(string log)
        {
            // Format the new line and add it on OutputLog
            OutputLog += DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + " -> " + log + Environment.NewLine;

            // Keep only the 20 last lines in the OutputLog
            int maxLine = 20;
            string[] outputLogList = OutputLog.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            int length = outputLogList.Count();
            if (length > maxLine)
            {
                outputLogList = outputLogList.Reverse().Take(maxLine).Reverse().ToArray();
                OutputLog = String.Join(Environment.NewLine, outputLogList) + Environment.NewLine;
            }
        }

        #region callbacks

        /// <summary>
        /// Callback called when the connection check Timer is reached
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Elapsed event args</param>
        private void CheckConnectTick(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (m_refComPort.IsOpen)
            {
                // Set the IsConnected status to true
                IsConnected = true;
            }
            else
            {
                // Set the IsConnected status to false
                IsConnected = false;
                // If AutoReconnect option is enabled
                if (AutoReconnect)
                {
                    // Open SerialPort connection
                    OpenSerialPortConnection();
                }
            }
        }

        /// <summary>
        /// Callback called when data is received through the SerialPort
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event args</param>
        private void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // If the com port has been closed, do nothing
            if (!m_refComPort.IsOpen) return;

            // This method will be called when there is data waiting in the port's buffer

            // Read all the data waiting in the buffer
            string data = m_refComPort.ReadExisting();

            // Add data to the serial buffer
            addToSerialBuffer(data);

        }

        /// <summary>
        /// Callback called when an error is received through the SerialPort
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event args</param>
        void SerialErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            AddOutputLog("Serial port error: " + e.EventType);
        }

        #endregion

        #region Converters

        /// <summary>
        /// Convert a string of hex digits (ex: E4 CA B2) to a byte array.
        /// </summary>
        /// <param name="str"> The string containing the hex digits (with or without spaces). </param>
        /// <returns> Returns an array of bytes. </returns>
        private byte[] HexStringToByteArray(string str)
        {
            str = str.Replace(" ", "");
            byte[] buffer = new byte[str.Length / 2];
            for (int i = 0; i < str.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(str.Substring(i, 2), 16);
            return buffer;
        }

        /// <summary>
        /// Converts an array of bytes into a formatted string of hex digits (ex: E4 CA B2)
        /// </summary>
        /// <param name="data"> The array of bytes to be translated into a string of hex digits. </param>
        /// <returns> Returns a well formatted string of hex digits with spacing. </returns>
        private string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
            return sb.ToString().ToUpper();
        }

        #endregion

        #endregion

        #region Dispose

        /// <summary>
        /// Frees the resources
        /// </summary>
        public void Dispose()
        {
            // If the SerialPort is open, close it.
            if (m_refComPort.IsOpen) m_refComPort.Close();

            // Unsubscribe from the event handler on Timer
            m_checkConnectTimer.Elapsed -= new System.Timers.ElapsedEventHandler(CheckConnectTick);
            // Dispose the Timer use for the AutoReconnect option 
            m_checkConnectTimer.Close();
            m_checkConnectTimer.Dispose();

            // Unsubscribe from the event handlers on SerialPort
            m_refComPort.DataReceived -= new SerialDataReceivedEventHandler(SerialDataReceived);
            m_refComPort.ErrorReceived -= new SerialErrorReceivedEventHandler(SerialErrorReceived);
        }

        #endregion

    }
}
