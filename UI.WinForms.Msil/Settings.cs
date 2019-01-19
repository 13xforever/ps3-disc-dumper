using System.Configuration;

namespace UI.WinForms.Msil
{
    internal sealed class Settings : ApplicationSettingsBase
    {
        [UserScopedSetting, DefaultSettingValue(@".\")]
        public string OutputDir { get => (string)this[nameof(OutputDir)]; set => this[nameof(OutputDir)] = value; }

        [UserScopedSetting, DefaultSettingValue(@".\ird")]
        public string IrdDir { get => (string)this[nameof(IrdDir)]; set => this[nameof(IrdDir)] = value; }

        [UserScopedSetting, DefaultSettingValue("[%product_code_letters% %product_code_numbers%] %title%")]
        public string DumpNameTemplate { get => (string)this[nameof(DumpNameTemplate)]; set => this[nameof(DumpNameTemplate)] = value; }

        [UserScopedSetting, DefaultSettingValue("false")]
        public bool Configured { get => (bool)this[nameof(Configured)]; set => this[nameof(Configured)] = value; }
    }
}
