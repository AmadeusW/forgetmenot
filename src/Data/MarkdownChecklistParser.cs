using System.Text;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace forgetmenot.Data;
public class MarkdownChecklistParser : IChecklistParser
{
    private readonly MarkdownPipeline parsePipeline;

    public MarkdownChecklistParser()
    {
        this.parsePipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            //.UsePragmaLines()
            .UsePreciseSourceLocation()
            .UseYamlFrontMatter()
            .UseEmphasisExtras()
            .Build();
    }

    public Task<Checklist> ParseAsync(string serialized)
    {
        string title = string.Empty;
        string author = string.Empty;
        DateTime dateCreated = default;
        string summary = string.Empty;

        var parsed = Markdown.Parse(serialized, parsePipeline);
        
        foreach (var descendant in parsed.Descendants())
        {
            if (descendant is ListBlock listBlock)
            {
                var descendants = listBlock.Descendants().Select(n => GetText(n.Span)).ToList();
                var children = listBlock.Select(n => GetText(n.Span)).ToList();
                foreach (var child in listBlock)
                {
                    //root.Children.Add(AddToTree(root, child));
                }
            }
            else if (descendant is HeadingBlock headingBlock)
            {
                var descendants = headingBlock.Descendants().Select(n => GetText(n.Span)).ToList();

                var text = GetText(headingBlock.Span);
                var content = headingBlock.Inline.FirstChild;
                title = text;
            }
            else if (descendant is ParagraphBlock paragraphBlock)
            {
                var text = GetText(paragraphBlock.Span);
                summary = text;
            }
            else if (descendant is ListItemBlock listItemBlock)
            {
                var text = GetText(listItemBlock.Span);
            }
        }
/*
        ChecklistNode AddToTree(ChecklistNode parent, Block block)
        {
            var listItemBlock = (ListItemBlock)block;
            var node = new ChecklistNode()
            {
                Children = new List<ChecklistNode>(),
                Parent = parent,
            };

            foreach (var child in listItemBlock)
            {
                if (child is ParagraphBlock paragraphBlock)
                {
                    node.Item = new ChecklistItem()
                    {
                        Name = GetText(paragraphBlock.Span),
                    };
                }
                else if (child is ListBlock listBlock)
                {
                    foreach (var listChild in listBlock)
                    {
                        var childNode = AddToTree(node, listChild);
                        node.Children.Add(childNode);
                    }
                }
            }

            return node;
        }
*/
        string GetText(SourceSpan span)
        {
            return serialized.Substring(span.Start, span.Length);
        }

        //List<ChecklistItem> items = new List<ChecklistItem>();
        //Dictionary<int, ChecklistItem> lastItemAtIndent = new Dictionary<int, ChecklistItem>();
        /*
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

                var newItem = new ChecklistItem()
                {
                    Name = itemName,
                    IndentSize = indentSize,
                    Done = done
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
        */
        return Task.FromResult(new Checklist()
        {
            Title = title,
            Summary = summary,
            //Items = root.Children,
        });
    }

    public string Serialize(Checklist checklist)
    {
        var sb = new StringBuilder();
        //sb.AppendLine($"# {checklist.Title}");
        //sb.AppendLine(String.Empty);
        //sb.AppendLine(checklist.Summary);
        //sb.AppendLine(String.Empty);
        //foreach (var item in checklist.Items)
        //{
        //    var box = item.Done ? "[x]" : "[ ]";
        //    sb.AppendLine($"{new string(' ', item.IndentSize)}- {box} {item.Name}");
        //}
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