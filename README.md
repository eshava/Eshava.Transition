# transition
A library to transform text files into c# objects and vice versa

Supported file formats
* xml
* json
* csv
* edi

## common functions
* Doublets for import
* Consider culture for data conversion
* Configure value translation
* Supported property data type
    * primitive data types
    * classes
    * enumerables (classes)
* Set configured value into property (import)

## xml functions
* Split export per data record
* Add additional xml node attributes to property configuration (independent of the data record) 
* Supported property data type
    * enumerables (primitive data types)
* Write/Read xml nodes and xml node attributes
* Conditional mapping execution for export 
    * same class as property to map
    * child properties from currenct class

## json functions
* Split export per data record
* Supported property data type
    * enumerables (primitive data types)
* Conditional mapping execution for export 
    * same class as property to map
    * child properties from currenct class
* Force export property value as string

## csv functions
* mark first line as column names
* index or column name based configuration
* support quotation mark surrounded values

## edi functions
* Repeat data property configuration for enumerables
