# .NET Training

## Input Parser & Summarizer

A C#/.NET console application developed as part of my .NET training to practice core C# and .NET concepts.

### Objectives

Build a console application that accepts user input, identifies it as numeric or text, and generates a summary of the provided data.

### Features

* **Main Menu:** Analyze Input, Help, and Exit
* **Numeric Analysis:**

  * Count, sum, average, minimum, and maximum
  * Positive, negative, and zero values
  * Whole, even, and odd numbers
  * Unique and duplicate values
* **Text Analysis:**

  * Word and character count
  * Characters excluding spaces
  * Unique words
  * Average word length
  * Longest and shortest words

### Concepts Practiced

* C# fundamentals and data types
* Conditions, loops, and switch statements
* Methods and arrays
* Strings and string manipulation
* Input validation and `TryParse`
* LINQ
* Console I/O and formatting
* Basic error handling

### Day 1 Outcome

Successfully developed a functional console application demonstrating core C# programming and basic data-processing concepts.

---

## Day 2 – OOP & Collections+LINQ Refactor

### What I Did

Refactored the Day 1 application — originally one large procedural `Program.cs` — into an object-oriented design, without changing what the app does for the user.

* Extracted the analysis logic behind an `ISummarizer` interface, implemented by `NumberSummarizer` and `TextSummarizer`
* Introduced `InputProcessor`, which owns a summarizer for each input type (composition, "has-a") and picks the right one at runtime, instead of using inheritance
* Modeled parsed input as an immutable `InputData` record (`Value`, `Type`) backed by an `InputType` enum
* Slimmed `Program.cs` down to menu handling and console I/O only, delegating all parsing and summarizing to the new services

### Concepts Practiced

* Interfaces and polymorphism (`ISummarizer`)
* Composition over inheritance ("has-a" vs. "is-a")
* Records, immutability, and enums
* Separation of concerns (UI vs. business logic)
* Continued application of LINQ (`Where`, `Select`, `GroupBy`, `Distinct`, aggregates) within the extracted services

### Day 2 Outcome

Same numeric and text analysis features as Day 1, now organized into a maintainable, testable OOP structure instead of one large procedural file.

**Training Progress:** Day 2 completed



