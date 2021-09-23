using System;
using System.Collections.Generic;
using System.Text;

namespace ApkaBotLibrary.Utils.Settings
{
    public class GlobalConfig
    {
        public ISettingsConnection Connection { get; set; } = new FileConnector();
    }
}
