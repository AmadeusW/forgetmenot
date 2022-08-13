namespace forgetmenot.Model;
using forgetmenot.Data;

public class ChecklistParser
{
    internal Task<string> LoadAsync(string path)
    {
        return Task.FromResult(SampleData);
    }

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
            }
            else if (line.Equals("## Metadata"))
            {
                section = Section.Metadata;
            }
            else if (line.Equals("## List"))
            {
                section = Section.List;
            }
            else
            {
                if (section == Section.Metadata)
                {
                    if (Matches(line, "- Created:", out var dateCreatedCandidate))
                    {
                        dateCreated = DateTime.Parse(dateCreatedCandidate);
                    }
                    else if (Matches(line, "- Author:", out var authorCandidate))
                    {
                        author = authorCandidate;
                    }
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
        }

        return Task.FromResult(new Checklist()
        {
            Title = title,
            Summary = summary,
            Author = author,
            DateCreated = dateCreated,
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

    private enum Section {
        None,
        Header,
        Metadata,
        List
    }

    private const string SampleData = @"
# Leaving on vacation

Checklist for leaving the house

## Metadata

- Created: 2022-4-12
- Author: Amadeusz
- Schema: 1

## List

- Power off
  - Upstairs office power strip
  - Downstairs office power strip
  - TV power strip
  - Shut off fireplace
  - Check stove
  - Check outdoor faucets
- Plants
  - Water the plants
  - Move plants
    - (condition) if person comes to water them
- Cat
  - Dry food
  - Wet food: 1 can per 2 days
  - Litter and litter box
  - Bowls
  - Brush
  - Collar
  - Bring to friend

        ";
}