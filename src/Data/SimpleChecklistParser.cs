using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace forgetmenot.Data;
public class SimpleChecklistParser : IChecklistParser
{
    public Task<Checklist> ParseAsync(string serialized)
    {
        List<ChecklistItem> items = new List<ChecklistItem>();
        Dictionary<int, ChecklistItem> lastItemAtIndent = new Dictionary<int, ChecklistItem>();
        string title = string.Empty;
        string summary = string.Empty;
        string identifier = string.Empty;
        string modifiedBy = string.Empty;
        bool isPrototype = false;
        DateTime modifiedDate = default;
        int version = 0;
        ParentChecklistItem? currentNestedItem = null;


        var section = Section.None;
        foreach (var rawLine in serialized.Split('\n'))
        {
            var line = rawLine.TrimEnd(); // don't trim start so that we can find out line indentation
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }
            else if (Matches(line, "# ", out var titleCandidate))
            {
                section = Section.Header;
                title = titleCandidate;
                continue;
            }
            else if (Matches(line, "-", out _))
            {
                section = Section.List;
            }
            else if (Matches(line, "|", out var keyValuePair))
            {
                var parts = keyValuePair.Split('|');
                var key = parts[0].Trim();
                var value = parts[1].Trim();
                switch (key) {
                    case "moniker": identifier = value; break;
                    case "version": version = Int32.Parse(value); break;
                    case "modifiedBy": modifiedBy = value; break;
                    case "modifiedDate": modifiedDate = DateTime.Parse(value); break;
                }
            }
            else if (Matches(line, "$", out var identifierData))
            {
                var parts = identifierData.Split('@');
                identifier = parts[0];
                version = Int32.Parse(parts[1]);
                continue;
            }
            else if (Matches(line, "!", out var modifiedData))
            {
                var parts = modifiedData.Split('@');
                modifiedBy = parts[0];
                modifiedDate = DateTime.Parse(parts[1]);
                continue;
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
                bool done = false;
                if (itemName.StartsWith("[x]"))
                {
                    done = true;
                    itemName = itemName.Substring(3).TrimStart();
                }
                else if (itemName.StartsWith("[ ]"))
                {
                    itemName = itemName.Substring(3).TrimStart();
                }
                else if (itemName.StartsWith('+'))
                {
                    var embeddedId = itemName.Substring(1).TrimStart();
                    var parts = embeddedId.Split('@');
                    int embeddedVersion = -1;
                    var topicId = parts[0];
                    if (parts.Length > 0)
                    {
                        embeddedVersion = Int32.Parse(parts[1]);
                    }
                    var newEmbeddedItem = new EmbeddedChecklistItem()
                    {
                        ChecklistId = new ChecklistId()
                        {
                            TopicId = topicId,
                            Version = embeddedVersion,
                        },
                    };
                    items.Add(newEmbeddedItem);
                    continue;
                }

                ChecklistItem newItem;
                if (itemName.EndsWith(':'))
                {
                    var newNestedItem = new ParentChecklistItem()
                    {
                        Name = itemName,
                        Done = done,
                        Items = new List<ChecklistItem>(),
                    };
                    currentNestedItem = newNestedItem;
                    newItem = newNestedItem;
                }
                else
                {
                    newItem = new ChecklistItem()
                    {
                        Name = itemName,
                        Done = done
                    };

                    if (indentSize == 0)
                    {
                        currentNestedItem = null;
                    }
                    else
                    {
                        Debug.Assert(currentNestedItem is not null, "Indented item has no parent that ends with ':'");
                        if (currentNestedItem is not null)
                        {
                            currentNestedItem.Items.Add(newItem);
                            continue;
                        }
                    }
                }

                items.Add(newItem);
            }
        }

        return Task.FromResult(new Checklist()
        {
            Items = items,
            Title = title,
            Summary = summary,
            Id = new ChecklistId()
            {
                TopicId = title,
                Version = 1,
            },
        });
    }

    public string Serialize(Checklist checklist)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {checklist.Title}");
        sb.AppendLine(String.Empty);
        sb.AppendLine(checklist.Summary);
        sb.AppendLine(String.Empty);
        foreach (var item in checklist.Items)
        {
            if (item is EmbeddedChecklistItem embeddedItem)
            {
                sb.AppendLine($"- +{embeddedItem.ChecklistId.TopicId}@{embeddedItem.ChecklistId.Version}");
                continue;
            }
            var box = item.Done ? "[x]" : "[ ]";
            //sb.AppendLine($"{new string(' ', item.IndentSize)}- {box} {item.Name}");
            sb.AppendLine($"- {box} {item.Name}");
        }
        sb.AppendLine(String.Empty);
        sb.AppendLine($"${checklist.Id.TopicId}@{checklist.Id.Version}");
        sb.AppendLine($"!{checklist.ModifiedBy}@{checklist.ModifiedDate}");
        return sb.ToString();
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