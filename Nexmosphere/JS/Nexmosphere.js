Nexmosphere.prototype = new EventEmitter();
Nexmosphere.prototype.constructor = Nexmosphere;

/**
 * Constructor of Nexmosphere
 */
function Nexmosphere() {
    // intuiface object is a service that is automatically loaded when launching XP
    // usbSerialService is the service used for usb serial communication, and included the following functions:
    // list, open, write, close, registerReadCallback, isConnected
    this._usbSerialService = intuiface.get('usbSerialService', this);

    /* Properties from IFD */
    // PortName property (for windows)
    this.portName = 'COM1';
    // PortNameOtherPlatforms property (for other platforms)
    this.portNameOtherPlatforms = '/usb/001';
    // OutputLog property
    this.outputLog = '';
    // OutputLog property
    this.autoReconnect = true;
    // IsConnected property
    this.isConnected = false;

    /* Configuration of serial port */
    // Nexmosphere USB to Serial controller Product ID (for android)
    this.PID = 8963;
    // Nexmosphere USB to Serial controller Vendor ID (for android)
    this.VID = 1659;
    // Nexmosphere USB to Serial controller Driver name (for android)
    this.driver = 'ProlificSerialDriver';
    // Transmission speed
    this.baudRate = 115200;
    // Number of data bits in each character
    this.dataBits = 8;
    // Separate each unit of data
    this.stopBits = 1;
    // Method of detecting errors in transmission
    this.parity = 0;
    // Data Terminal Ready
    this.DTR = false;
    // Request To Send
    this.RTS = false;
    // Resume serial port once the app is brought back to foreground
    this.sleepOnPause = true;

    /* Internal properties */
    // Serial buffer for commands
    this.serialBuffer = '';
    // End of line of command
    this.EOL = '\r\n';
    // Reconnection counter
    this.autoReconnectCount = 0;
    // ID of timer
    this.autoReconnectTimeoutID = null;
    // Check connection status
    this.isCheckConnection = false;
    // ID of timer
    this.checkConnectionTimeoutID = null;

    this._addOutputLog('Start Nexmosphere Interface Asset');
}

/**
 * Configure and open serial port connection
 */
Nexmosphere.prototype.OpenSerialPort = function () {
    const self = this;

    this._addOutputLog('Try to open Serial Port ' + this.portNameOtherPlatforms);

    // Open the serial port
    this._open(function (error) {
        if (error) {
            self._addOutputLog('Could not open the path port. Most likely it is already in use, has been removed, or is unavailable.');
            self._addOutputLog('Exception message: ' + error);
            return;
        }
        // Register a callback to read the received message
        self._registerCallback(function (error) {
            if (error) {
                self._addOutputLog('Failed to register callback.');
                self._addOutputLog('Exception message: ' + error);
            }
        });
    });
};

/**
 * Close serial port connection
 */
Nexmosphere.prototype.CloseSerialPort = function () {
    const self = this;

    this._addOutputLog('Try to close Serial Port ' + this.portNameOtherPlatforms);

    // Disallow to check connection status
    this.isCheckConnection = false;

    // Closing the serial port
    self._usbSerialService.close(
        function (error, result) {
            // Handle error
            if (error) {
                return self._addOutputLog('Failed to close the path port: ' + error);
            }

            self.isConnected = false;
            // Send an event to refresh view property
            self.emit('isConnectedChanged', [self.isConnected])
        }
    );
};

/**
 * Send X-Script Command line
 *
 * @param {String} command - The a full line X-Script command
 */
Nexmosphere.prototype.SendXScriptCommand_FullLine = function (command) {
    const self = this;

    // Create a xScriptCommand object
    const xScriptCommand = new XScriptCommand(command);

    // Verify if X-Script is valid before to send it
    if (!xScriptCommand.isValid) {
        return this._addOutputLog('Failed to send X-Script command: ' + command + ' is not valid!');
    }

    // Sending the X-Script command to the serial port
    self._usbSerialService.write(
        command + self.EOL,
        function (error) {
            // Handle error
            if (error) {
                return self._addOutputLog('Failed to send X-Script command: ' + error);
            }

            self._addOutputLog('Send X-Script command: ' + xScriptCommand.xScript);
        }
    );
};

/**
 * Send X-Script Command
 *
 * @param {String} type - Type of command: X = X-Talk command, G = Generic control command, S = System command, D = Diagnostic command
 * @param {String} address - Channel address to which the command is sent: value between 0-999
 * @param {String} format - Format of the command: A = short command (number between 0-65535), B = long command (string of 0-30 characters), S = setting command (custom format for each setting)
 * @param {String} command - The Command to send
 */
Nexmosphere.prototype.SendXScriptCommand = function (type, address, format, command) {
    this.SendXScriptCommand_FullLine(type + address + format + '[' + command + ']');
};

/**
 * Set property portName
 * @param {String} portName
 */
Nexmosphere.prototype.setPortName = function (portName) {
    this.portName = portName;
    // Send an event to refresh view property
    this.emit('portNameChanged', [this.portName]);
};

/**
 * Set property portNameOtherPlatforms
 * @param {String} portNameOtherPlatforms
 */
Nexmosphere.prototype.setPortNameOtherPlatforms = function (portNameOtherPlatforms) {
    this.portNameOtherPlatforms = portNameOtherPlatforms;
    // Send an event to refresh view property
    this.emit('portNameOtherPlatformsChanged', [this.portNameOtherPlatforms]);
};

/**
 * Set property autoReconnect
 * @param {String} autoReconnect
 */
Nexmosphere.prototype.setAutoReconnect = function (autoReconnect) {
    this.autoReconnect = autoReconnect;
    // Send an event to refresh view property
    this.emit('autoReconnectChanged', [this.autoReconnect]);
};

/**
 * Add a new line on the OutputLog
 *
 * @param {String} log - The new line to add
 */
Nexmosphere.prototype._addOutputLog = function (log) {
    // Separate each log line in an array
    const outputLogList = this.outputLog.split('\n');

    // Formatting the new log line
    log = moment().format('LLLL') + ' -> ' + log;

    // Push the new log line to the array of logs
    outputLogList.push(log);

    // Keep only the 20 last lines in the outputLog
    if (outputLogList.length > 20) {
        outputLogList.shift();
    }

    // Concatenates all log lines
    this.outputLog = outputLogList.join('\n');

    // Send an event to refresh view property
    this.emit('outputLogChanged', [this.outputLog]);
};

/**
 * Handle the auto reconnect
 * If autoReconnect is checked and serial port loss connection, every 3 seconds we try to reconnect
 * A limit of 10 tries before cancellation
 */
Nexmosphere.prototype._autoReconnect = function () {
    const self = this;

    if (this.autoReconnectTimeoutID || !this.autoReconnect || this.autoReconnectCount > 10 || this.isConnected) {
        return;
    }

    self.OpenSerialPort();
    self.autoReconnectCount++;

    // Initialise a timeout of 3s to recall this same function
    self.autoReconnectTimeoutID = setTimeout(function () {
        self.autoReconnectTimeoutID = null;
        self._autoReconnect();
    }, 3000);
};

/**
 * Handle the connection status
 * Every second we ask to the usb serial service the status of connexion
 */
Nexmosphere.prototype._checkConnection = function () {
    const self = this;

    if (!this.isCheckConnection) {
        return;
    }

    // Checking the status of connection of the serial port
    self._usbSerialService.isConnected(
        function (error, isConnected) {
            // Only emit if new state if different to the older
            if (isConnected !== self.isConnected) {
                self.isConnected = isConnected;
                // Send an event to refresh view property
                self.emit('isConnectedChanged', [self.isConnected]);
            }

            self._autoReconnect();
        }
    );

    // Initialise a timeout of 1s to recall this same function
    self.checkConnectionTimeoutID = setTimeout(function () {
        self.checkConnectionTimeoutID = null;
        self._checkConnection();
    }, 1000);
};

/**
 * Open serial port connection
 */
Nexmosphere.prototype._open = function (callback) {
    const self = this;

    // Opening serial port
    self._usbSerialService.open(
        {
            port: self.portNameOtherPlatforms,
            pid: self.PID,
            vid: self.VID,
            driver: self.driver,
            baudRate: self.baudRate,
            dataBits: self.dataBits,
            stopBits: self.stopBits,
            parity: self.parity,
            dtr: self.DTR,
            rts: self.RTS,
            sleepOnPause: self.sleepOnPause
        },
        function (error, result) {
            // Handle error
            if (error) {
                return callback(error);
            }

            self._addOutputLog('Port ' + self.portNameOtherPlatforms + ' opened');

            self.isConnected = false;
            // Send an event to refresh view property
            self.emit('isConnectedChanged', [self.isConnected]);

            // Reset counter of auto reconnect
            self.autoReconnectCount = 0;
            // Allow to check connection status
            self.isCheckConnection = true;
            // Start checking connection status
            self._checkConnection();

            callback(null, result);
        }
    );
};

/**
 * Registering a callback to be able to receive messages
 */
Nexmosphere.prototype._registerCallback = function (callback) {
    const self = this;

    // Registering a callback to the usb serial service to be automatically notified of new messages
    self._usbSerialService.registerReadCallback(
        function (message) {
            // Add the received message to the serial buffer
            self.serialBuffer += message;

            // var instead let, because for Interface Asset, the Intuiface Player don't use the last Javascript engine
            var indexEOL;
            // If any end of line is present, we can extract this complete serial command
            while ((indexEOL = self.serialBuffer.indexOf(self.EOL)) !== -1) {
                // Get a new serial command line
                const command = self.serialBuffer.slice(0, indexEOL);
                // Update serial buffer, by removing this last serial command line
                self.serialBuffer = self.serialBuffer.slice(indexEOL + self.EOL.length);

                // Create a xScriptCommand object
                const xScriptCommand = new XScriptCommand(command);

                // Check if is a xScriptCommand valid
                if (xScriptCommand.isValid) {
                    self._addOutputLog('Received X-Script command: ' + xScriptCommand.xScript);
                    // Send an event to refresh view property
                    self.emit('XScriptCommand', [xScriptCommand.xScript, xScriptCommand.type, xScriptCommand.address, xScriptCommand.format, xScriptCommand.command]);
                } else {
                    self._addOutputLog('Received an invalid X-Script command: ' + command);
                }
            }
        },
        function (error, result) {
            if (error) {
                return callback(error);
            }
            callback(null, result);
        }
    );
};
