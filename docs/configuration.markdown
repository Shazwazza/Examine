---
# Feel free to add content and custom Front Matter to this file.
# To modify the layout, see https://jekyllrb.com/docs/themes/#overriding-theme-defaults

layout: default
---

_[...Back to home](index)_

Configuration
===

An index can be configured in many ways including different configurations per field such as how those values are analyzed, indexed, tokenized ... basically how the data is stored and retrieved. 

_**Note**: This documentation refers to using Lucene based indexes in Examine (the default index type shipped in Examine)._

The `LuceneIndex` constructor has several **optional** parameters that can be supplied to configure the index:

* __fieldDefinitions__ _`FieldDefinitionCollection`_ - Manages the mappings between a field name and it's index value type
* __analyzer__ _`Analyzer`_ - The default Lucene Analyzer to use for each field (default = `StandardAnalyzer`)
* __validator__ _`IValueSetValidator`_ - Used to validate a value set to be indexed, if validation fails it will not be indexed
* __indexValueTypesFactory__ _`IReadOnlyDictionary<string, IFieldValueTypeFactory>`_ - Allows you to define custom data types, basically how a field value is stored and retrieved. 

## Default value types

These are the default value types provided:

* `FieldDefinitionTypes.FullText` - Default. The field will be indexed with the index's default Analyzer without any fancy type storage or sortability. Generally this is fine for normal text searching.
* `FieldDefinitionTypes.FullTextSortable` - Will be indexed with FullText but also enable sorting on this field for search results.
* `FieldDefinitionTypes.Integer` - Stored as a numerical structure.
* `FieldDefinitionTypes.Float` - Stored as a numerical structure.
* `FieldDefinitionTypes.Double` - Stored as a numerical structure.
* `FieldDefinitionTypes.Long` - Stored as a numerical structure.
* `FieldDefinitionTypes.DateTime` - Stored as a DateTime, represented by a numerical structure.
* `FieldDefinitionTypes.DateYear` - Just like DateTime but with precision only to the year.
* `FieldDefinitionTypes.DateMonth` - Just like DateTime but with precision only to the month.
* `FieldDefinitionTypes.DateDay` - Just like DateTime but with precision only to the day.
* `FieldDefinitionTypes.DateHour` - Just like DateTime but with precision only to the hour.
* `FieldDefinitionTypes.DateMinute` - Just like DateTime but with precision only to the minute.
* `FieldDefinitionTypes.EmailAddress` - Uses custom analyzers for dealing with email address searching.
* `FieldDefinitionTypes.InvariantCultureIgnoreCase` - Uses custom analyzers for dealing with text so it can be searched on regardless of the culture/casing.
* `FieldDefinitionTypes.Raw` - Will be indexed without analysis, searching will only match with an exact value.

## Custom field definitions

By default any field that is indexed that is not mapped to an explicit definition will be mapped to the default: `FieldDefinitionTypes.FullText`. If you need to map a field to a specific value type, you can specify this via the constructor, or you can modify the definitions at runtime.

### Via constructor

```cs
// Create and add a new index to the manager
var myIndex = examineManager.AddIndex(
    new LuceneIndex(            
        "MyIndex",              
        new SimpleFSDirectory(new DirectoryInfo("C:\\TestIndexes")),
        // Pass in a custom field definition collection
        new FieldDefinitionCollection(            
            // Set the "Price" field to map to the 'Double' value type.
            // All values for this field will be stored numerically in the index (not as strings).
            new FieldDefinition("Price", FieldDefinitionTypes.Double))));
```

### At runtime

You can modify the field definitions for an index at runtime by using any of the following methods:

* `myIndex.FieldDefinitionCollection.TryAdd`
* `myIndex.FieldDefinitionCollection.AddOrUpdate`
* `myIndex.FieldDefinitionCollection.GetOrAdd`

## Custom field value types

A field value type is defined by `IIndexFieldValueType`

_**Tip**: There are many implementations of IIndexFieldValueType in the source code to use as examples/reference._

Field value types are responsible for:

* Defining a field name and if the field should be sortable, the field to store the sortable data
* Adding a field value to an index document
* Configuring how the value will be stored in the index
* Configuring the analyzer for the field
* Generating the Query for the field

A common base class that can be used for field value types is: `IndexFieldValueTypeBase`.

A common implementation that can be used for field value types for custom Analyzers is: `GenericAnalyzerFieldValueType`.

### Example - Phone Number

A phone number stored in Lucene could require a custom analyzer to index and search it properly. So the best way to set this up in Examine would be to have a custom field value type for it. Since this field value type doesn't need to do anything more fancy than to provide a custom analyzer, we can create it with the `GenericAnalyzerFieldValueType`.

```cs
// Create a writeable dictionary based off of the 
// Examine default field value types
var fieldValueTypes = ValueTypeFactoryCollection.DefaultValueTypes
    .ToDictionary(x => x.Key, x => x.Value);

// Add a new phone number field value type
fieldValueTypes.Add(
    "phonenumber",  // Each field value type needs a unique name
    new DelegateFieldValueTypeFactory(name =>
        new GenericAnalyzerFieldValueType(
            name, 
            new PhoneNumberAnalyzer()))); // Pass in a custom analyzer

// Create the index with the customized dictionary
var myIndex = new LuceneIndex(
    "MyIndex",
    new SimpleFSDirectory(new DirectoryInfo("C:\\TestIndexes")),
    // Pass in a custom field definition collection
    new FieldDefinitionCollection(            
        // Set the "Phone" field to map to the 'phonenumber' value type.
        new FieldDefinition("Phone", "phonenumber"))
    // Pass in the custom field value type dictionary with the phonenumber type
    indexValueTypesFactory: fieldValueTypes);
```

The above creates a custom field value type using a custom analyzer and maps the "Phone" field to use this value type.

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

## ValueSet validators

An `IValueSetValidator` is a simple interface: 

```cs
public interface IValueSetValidator
{
    ValueSetValidationResult Validate(ValueSet valueSet);
}
```

That returns an enum `ValueSetValidationResult` of values: 

* `Valid` - The ValueSet is valid and will be indexed
* `Failed` - The ValueSet was invalid and will not be indexed
* `Filtered` - The ValueSet has been filtered/modified by the validator and will be indexed

Examine only has one implementation: `ValueSetValidatorDelegate` which can be used by developers as a simple way to create a validator based on a callback, else developers can implement this interface if required. By default, no ValueSet validation is done with Examine.
