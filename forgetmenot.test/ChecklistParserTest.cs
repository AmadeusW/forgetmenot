using forgetmenot.Data;

namespace forgetmenot.test
{
    public class ChecklistParserTest
    {
        [Fact]
        public async Task CompleteParseAsync()
        {
            var rawData = @"
===
Author: author name
Last modified: 2022-11-09
===

# Checklist title
Checklist summary

- Item A
- Item B
- Item C
  - Item C1
  - Item C2
- Item D

";
            var parser = new ChecklistParser();
            var checklist = await parser.ParseAsync(rawData);

            Assert.Equal("Checklist title", checklist.Title);
            Assert.Equal("Checklist summary", checklist.Summary);
            Assert.Equal(DateTime.Parse("2022-11-09"), checklist.ModifiedDate);
            Assert.Equal("author name", checklist.ModifiedBy);
        }
    }
}