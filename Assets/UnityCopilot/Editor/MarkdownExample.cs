using UnityEngine;

namespace YourCompany.UnityCopilot.Editor
{
    /// <summary>
    /// Example demonstrating markdown to Unity rich text conversion.
    /// </summary>
    public static class MarkdownExample
    {
        public static void TestConversion()
        {
            var markdown = @"# Header 1
## Header 2
### Header 3

This is **bold text** and this is *italic text*.

Here's some `inline code` in a sentence.

```
// Code block example
public void HelloWorld()
{
    Debug.Log(""Hello!"");
}
```

- Bullet point 1
- Bullet point 2
  - Nested bullet

1. First item
2. Second item
3. Third item";

            var richText = MarkdownToRichText.Convert(markdown);
            Debug.Log("Original Markdown:\n" + markdown);
            Debug.Log("\nConverted to Rich Text:\n" + richText);
        }
    }
}

