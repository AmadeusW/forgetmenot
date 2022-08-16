namespace forgetmenot.Data;
public class ChecklistParser
{
    internal Task<Checklist> ParseAsync(string serialized)
    {
        string title = string.Empty;
        string author = string.Empty;
        DateTime dateCreated = default;
        string summary = string.Empty;
        List<ChecklistItem> items = new List<ChecklistItem>();
        Dictionary<int, ChecklistItem> lastItemAtIndent = new Dictionary<int, ChecklistItem>();

        var section = Section.None;
        foreach (var rawLine in serialized.Split('\n'))
        {
            var line = rawLine.TrimEnd(); // don't trim start so that we can find out line indentation
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }
            if (Matches(line, "# ", out var titleCandidate))
            {
                section = Section.Header;
                title = titleCandidate;
                continue;
            }

            if (Matches(line, "-", out _))
            {
                section = Section.List;
            }

            if (section == Section.Header)
            {
                summary += line;
                continue;
            }
            else if (section == Section.List)
            {
                var indentSize = line.IndexOf('-');
                var itemName = line.Substring(indentSize + 1).TrimStart();
                var newItem = new ChecklistItem()
                {
                    Name = itemName,
                    IndentSize = indentSize,
                };

                if (indentSize > 0)
                {
                    for (int previousIndentSize = indentSize - 1; previousIndentSize >= 0; previousIndentSize--)
                    {
                        if (lastItemAtIndent.TryGetValue(previousIndentSize, out var previousItem))
                        {
                            newItem.ParentItem = previousItem;
                            previousItem.HasChildItems = true;
                            break;
                        }
                    }
                }

                lastItemAtIndent[indentSize] = newItem;
                items.Add(newItem);
            }
        }

        return Task.FromResult(new Checklist()
        {
            Title = title,
            Summary = summary,
            Items = items,
        });
    }

    private bool Matches(string haystack, string needle, out string value)
    {
        if (haystack.StartsWith(needle))
        {
            value = haystack.Substring(needle.Length).TrimStart();
            Console.WriteLine($"Found {value} matching {needle} in {haystack}");
            return true;
        }
        value = string.Empty;
        return false;
    }

    private enum Section
    {
        None,
        Header,
        List
    }
}