/**
 * Constructor of XScriptCommand
 *
 * @param {String} xScript - The a full line X-Script command
 */
function XScriptCommand(xScript) {
    // Regex pattern for generic X-Script command
    this.xScriptCommandPattern = /^(X|G|S|D)([0-9]{3})(A|B|S)\[(\S*)\]$/;
    // Regex pattern for specific XR ANTENNA DRIVER X-Script command
    this.xrXScriptCommandPattern = /^(XR)\[(\S*)\]$/;

    // X-Script command string
    this.xScript = xScript;
    // TYPE part string
    this.type = '';
    // ADDRESS part string
    this.address = '';
    // FORMAT part string
    this.format = '';
    // COMMAND part string
    this.command = '';

    // Indicate if the XScriptCommand is valid
    this.isValid = this._parse();
}

/**
 * Check if the xScript command is valid
 *
 * @return {boolean} True if XScriptCommand structure is valid, false otherwise
 */
XScriptCommand.prototype._parse = function () {
    var matches;

    // For generic S-Script command partern
    if ((matches = this.xScript.matchAll(this.xScriptCommandPattern)) != null) {
        this.type = matches[0][1];
        this.address = matches[0][2];
        this.format = matches[0][3];
        this.command = matches[0][4];
    }
    // For specific X-Script command XR ANTENNA DRIVER partern
    else if ((matches = this.xScript.matchAll(this.xrXScriptCommandPattern)) != null)
    {
        this.type = matches[0][1];
        this.command = matches[0][2];
    }
    else
    {
        return false;
    }

    return true;
};
