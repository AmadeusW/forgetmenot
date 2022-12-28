using forgetmenot.Data;

namespace forgetmenot.test
{
    public class ChecklistParserTest
    {
        [Fact]
        public async Task CompleteParseAsync()
        {
            var rawData = @"
# Checklist title

Checklist summary

- Item A
- Item B
- Item C
- Item D

";
            var parser = new SimpleChecklistParser();
            var checklist = await parser.ParseAsync(rawData);

            Assert.Equal("Checklist title", checklist.Title);
            Assert.Equal("Checklist summary", checklist.Summary);
            Assert.NotNull(checklist.Items.Single(n => n.Name == "Item A"));
            Assert.NotNull(checklist.Items.Single(n => n.Name == "Item B"));
            Assert.NotNull(checklist.Items.Single(n => n.Name == "Item C"));
            Assert.NotNull(checklist.Items.Single(n => n.Name == "Item D"));
        }
    }
}