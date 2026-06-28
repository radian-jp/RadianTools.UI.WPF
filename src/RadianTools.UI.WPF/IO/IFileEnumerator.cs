namespace RadianTools.UI.WPF.IO;

public interface IFileEnumerator
{
    IEnumerable<IFileEntry> Enumerate();
}
