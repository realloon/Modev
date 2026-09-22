using System.Xml.Linq;

namespace Modev.Utility;

public static class XmlCommentStripper {
    public static void StripDirectory(DirectoryInfo dir) {
        foreach (var file in dir.EnumerateFiles("*.xml", SearchOption.AllDirectories)) {
            StripFile(file.FullName);
        }
    }

    private static void StripFile(string path) {
        var doc = XDocument.Load(path, LoadOptions.PreserveWhitespace);
        var comments = doc.DescendantNodes().OfType<XComment>().ToList();
        if (comments.Count == 0) return;

        comments.ForEach(comment => comment.Remove());
        doc.Save(path, SaveOptions.DisableFormatting);
    }
}
