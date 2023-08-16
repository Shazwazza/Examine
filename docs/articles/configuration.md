---
title: Configuration
permalink: /configuration
uid: configuration
order: 1
---

Configuration
===

An index can be configured in many ways including different configurations per field such as how those values are analyzed, indexed, tokenized ... basically how the data is stored and retrieved. This is done via Examine ["Value Types"](#value-types). 

_**Note**: This documentation refers to using Lucene based indexes in Examine (the default index type shipped in Examine)._

## Field definitions

A Field Definition is a mapping of a field name to a ["Value Types"](#value-types). By default all fields are mapped to the default Value Type: [`FieldDefinitionTypes.FullText`](xref:Examine.FieldDefinitionTypes#Examine_FieldDefinitionTypes_FullText).

You can map a field to any value type when configuring the index.

### IConfigureNamedOptions

Configuration of Examine indexes is done with [.NET's Options pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0). For Examine, this is done with named options: [`IConfigureNamedOptions`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.options.iconfigurenamedoptions-1).

There are several options that can be configured, the most common ones are:

* __FieldDefinitions__ _[`FieldDefinitionCollection`](xref:Examine.FieldDefinitionCollection)_ - Manages the mappings between a field name and it's index value type
* __Analyzer__ _`Analyzer`_ - The default Lucene Analyzer to use for each field (default = [`StandardAnalyzer`](https://lucenenet.apache.org/docs/4.8.0-beta00016/api/analysis-common/Lucene.Net.Analysis.Standard.StandardAnalyzer.html))
* __Validator__ _[`IValueSetValidator`]([`IValueSetValidator`](xref:Examine.IValueSetValidator))_ - Used to validate a value set to be indexed, if validation fails it will not be indexed
* __[IndexValueTypesFactory](xref:Examine.Lucene.IFieldValueTypeFactory)__ _`IReadOnlyDictionary<string, IFieldValueTypeFactory>`_ - Allows you to define custom Value Types

```cs
/// <summary>
/// Configure Examine indexes using .NET IOptions
/// </summary>
public sealed class ConfigureIndexOptions : IConfigureNamedOptions<LuceneDirectoryIndexOptions>
{
    public void Configure(string name, LuceneDirectoryIndexOptions options)
    {
        switch (name)
        {
            case "MyIndex":
                // Set the "Price" field to map to the 'Double' value type.
                options.FieldDefinitions.AddOrUpdate(
                    new FieldDefinition("Price", FieldDefinitionTypes.Double));
                break;
        }
    }

    public void Configure(LuceneDirectoryIndexOptions options) 
        => Configure(string.Empty, options);
}
```

### After construction

You can modify the field definitions [FieldDefinitionCollection](xref:Examine.FieldDefinitionCollection) for an index after it is constructed by using any of the following methods:

* `myIndex.FieldDefinitionCollection.TryAdd`
* `myIndex.FieldDefinitionCollection.AddOrUpdate`
* `myIndex.FieldDefinitionCollection.GetOrAdd`

These modifications __must__ be done before any indexing or searching is executed.

### Add a field value type after construction

It is possible to add custom field value types after the construction of the index, but this must be done before the index is used. Some people may prefer this method of adding custom field value types. Generally, these should be modified directly after the construction of the index.

```cs
// Create the index with all of the defaults
var myIndex = new LuceneIndex(
    "MyIndex",
    new SimpleFSDirectory(new DirectoryInfo("C:\\TestIndexes")));

// Add a custom field value type
myIndex.FieldValueTypeCollection.ValueTypeFactories
    .TryAdd(
        "phonenumber", 
        name => new GenericAnalyzerFieldValueType(
            name, 
            new PhoneNumberAnalyzer()));

// Map a field to use the custom field value type
myIndex.FieldDefinitionCollection.TryAdd(
    new FieldDefinition("Phone", "phonenumber"));
```

## Value types

Value types are responsible for:

* Defining a field name and if the field should be sortable, the field to store the sortable data
* Adding a field value to an index document
* Configuring how the value will be stored in the index
* Configuring the analyzer for the field
* Generating the Query for the field

These are the default field value types provided with Examine. Each value type can be resolved from the static class [`Examine.FieldDefinitionTypes`](xref:Examine.FieldDefinitionTypes) (i.e. [`Examine.FieldDefinitionTypes.FullText`](xref:Examine.FieldDefinitionTypes#Examine_FieldDefinitionTypes_FullText)).

| Value Type                 | Description                                                                                                                                                                          | Sortable |
|----------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------|
| FullText                   | __Default__.<br />The field will be indexed with the index's <br />default Analyzer without any sortability. <br />Generally this is fine for normal text searching.                                  | ❌           |
| FullTextSortable           | Will be indexed with FullText but also <br />enable sorting on this field for search results. <br />_FullText sortability adds additional overhead <br />since it requires an additional index field._ | ✅           |
| Integer                    | Stored as a numerical structure.                                                                                                                                                     | ✅           |
| Float                      | Stored as a numerical structure.                                                                                                                                                     | ✅           |
| Double                     | Stored as a numerical structure.                                                                                                                                                     | ✅           |
| Long                       | Stored as a numerical structure.                                                                                                                                                     | ✅           |
| DateTime                   | Stored as a DateTime, <br />represented by a numerical structure.                                                                                                                          | ✅           |
| DateYear                   | Just like DateTime but with <br />precision only to the year.                                                                                                                              | ✅           |
| DateMonth                  | Just like DateTime but with <br />precision only to the month.                                                                                                                             | ✅           |
| DateDay                    | Just like DateTime but with <br />precision only to the day.                                                                                                                               | ✅           |
| DateHour                   | Just like DateTime but with <br />precision only to the hour.                                                                                                                              | ✅           |
| DateMinute                 | Just like DateTime but with <br />precision only to the minute.                                                                                                                            | ✅           |
| EmailAddress               | Uses custom analyzers for dealing <br />with email address searching.                                                                                                                      | ❌           |
| InvariantCultureIgnoreCase | Uses custom analyzers for dealing with text so it<br /> can be searched on regardless of the culture/casing.                                                                               | ❌           |
| Raw                        | Will be indexed without analysis, searching will<br /> only match with an exact value.                                                                                                     | ❌           |

### Custom field value types

A field value type is defined by [`IIndexFieldValueType`](xref:Examine.Lucene.Indexing.IIndexFieldValueType)

_**Tip**: There are many implementations of IIndexFieldValueType in the source code to use as examples/reference._

A common base class that can be used for field value types is: [`IndexFieldValueTypeBase`](xref:Examine.Lucene.Indexing.IndexFieldValueTypeBase).

A common implementation that can be used for field value types for custom Analyzers is: [`GenericAnalyzerFieldValueType`](xref:Examine.Lucene.Indexing.GenericAnalyzerFieldValueType).

#### Example - Phone Number

A phone number stored in Lucene could require a custom analyzer to index and search it properly. So the best way to set this up in Examine would be to have a custom field value type for it. Since this field value type doesn't need to do anything more fancy than to provide a custom analyzer, we can create it with the [`GenericAnalyzerFieldValueType`](xref:Examine.Lucene.Indexing.GenericAnalyzerFieldValueType).

```cs
/// <summary>
/// Configure Examine indexes using .NET IOptions
/// </summary>
public sealed class ConfigureIndexOptions : IConfigureNamedOptions<LuceneDirectoryIndexOptions>
{
    private readonly ILoggerFactory _loggerFactory;

    public ConfigureIndexOptions(ILoggerFactory loggerFactory)
        => _loggerFactory = loggerFactory;

    public void Configure(string name, LuceneDirectoryIndexOptions options)
    {
        switch (name)
        {
            case "MyIndex":
                // Create a dictionary for custom value types.
                // They keys are the value type names.
                options.IndexValueTypesFactory = new Dictionary<string, IFieldValueTypeFactory>
                {
                    // Create a phone number value type using the GenericAnalyzerFieldValueType
                    // to pass in a custom analyzer. As an example, it could use Examine's
                    // PatternAnalyzer to pass in a phone number pattern to match.
                    ["phone"] = new DelegateFieldValueTypeFactory(name =>
                                    new GenericAnalyzerFieldValueType(
                                        name,
                                        _loggerFactory,
                                        new PatternAnalyzer(@"\d{3}\s\d{3}\s\d{4}", 0)))
                };

                // Add the field definition for a field called "phone" which maps
                // to a Value Type called "phone" defined above.
                options.FieldDefinitions.AddOrUpdate(new FieldDefinition("phone", "phone"));
                break;
        }
    }

    public void Configure(LuceneDirectoryIndexOptions options)
        => throw new NotImplementedException("This is never called and is just part of the interface");
}
```

The above creates a custom field value type using a custom analyzer and maps the "Phone" field to use this value type.

## ValueSet validators

An [`IValueSetValidator`](xref:Examine.IValueSetValidator) is a simple interface:

```cs
public interface IValueSetValidator
{
    ValueSetValidationResult Validate(ValueSet valueSet);
}
```

That returns an result [`ValueSetValidationResult`](xref:Examine.ValueSetValidationResult) with an enum of [`ValueSetValidationStatus`](xref:Examine.ValueSetValidationStatus) values:

* `Valid` - The ValueSet is valid and will be indexed
* `Failed` - The ValueSet was invalid and will not be indexed
* `Filtered` - The ValueSet has been filtered/modified by the validator and will be indexed

Examine only has one implementation: [`ValueSetValidatorDelegate`](xref:Examine.Lucene.Providers.ValueSetValidatorDelegate) which can be used by developers as a simple way to create a validator based on a callback, else developers can implement this interface if required. By default, no ValueSet validation is done with Examine.


## Luke
Lucene.NET 4.8 is compatible with Java Luke 4.8.0, a useful tool for working with Lucene Indexes. Newer versions of Luke can't be used with Lucene.NET 4.8 indexes. 
[Luke 4.8.0](https://github.com/DmitryKey/luke/releases/download/4.8.0/luke-with-deps.jar)
