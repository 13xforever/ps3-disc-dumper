namespace UI.WinForms.Msil.Utils
{
    internal static class DriveLetterToIdConverter
    {
        public static int ToDriveId(this char driveLetter)
        {
            driveLetter = char.ToUpperInvariant(driveLetter);
            if (driveLetter < 'A' || driveLetter > 'Z')
                return 0;

            return 1 << (driveLetter - 'A');
        }
    }
}
