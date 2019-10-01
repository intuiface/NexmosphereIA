using System;

namespace Nexmosphere
{
    /// <summary>
    /// Create the Event handler to notify a XScriptCommand from Nexmosphere device
    /// </summary>
    /// <param name="sender">Event source</param>
    /// <param name="e">XScriptCommand EventArgs</param>
    public delegate void XScriptCommandEventArgsHandler(object sender, XScriptCommandEventArgs e);

    public class XScriptCommandEventArgs : EventArgs
    {
        public string FullLine { get; set; }
        public string TYPE { get; set; }
        public string ADDRESS { get; set; }
        public string FORMAT { get; set; }
        public string COMMAND { get; set; }
    }
}
