# Cosmic Clone
1. [Overview](#overview)
1. [Deployment Steps](#deployment-steps)
1. [Create backup copy of a collection](#Create-backup-copy-of-a-collection)
1. [Anonymize data of a cosmos collection](#Anonymize-data-of-a-cosmos-collection)
1. [Todos](#todos)
1. [References](#references)
1. [Contributing](#contributing)


## Overview
Cosmic Clone is a tool to clone\backup\restore and anonymize data in an azure Cosmos Collection.
As more applications begin to use Cosmos database, self serve capabilities such as backup, restore collection have become more essential.
Cosmos Clone is an attempt to create a simple utility that allows to clone a Cosmos Collection.
The utility helps in below
*	Clone collections for QA, testing and other non production env.
*	Backup data of a collection.
*	Create collections with similar settings(indexes, partition, TTL etc)
*	Anonymize data through scrubbing or shuffling of sensitive data in documents.

![screen91](/docs/images/prodcloneLogo.png)

## Deployment Steps
1. Just Compile and Run the Code.
2. Or Download a pre compiled binary from the releases section and run the “CosmicCloneUI.exe” file.
3. For Best performance you can run the compiled code in an Azure VM that is in the same region as the source and destination Cosmos Collection.

As a prerequisite the tool needs the below

*	Install Microsoft .Net Framework 4.6.1 or higher
*	Source Cosmos collection and read only keys to its account
*	Destination Cosmos Account and its read write keys
*	If firewall is enabled for the Cosmos Account, ensure the IP address of the machine running the tool is allowed.

## Create backup copy of a collection

**Initial screen**

![screen1](/docs/images/sinitial.png)

**Enter Source and Target connection details**

![screen2](/docs/images/sinitialDetails2.png)

If validation of the entered details fails an appropriate message is displayed below the Test Connection button.

If the access validation succeeds then the next screen shows various options for cloning of a collection.

**Set migration options**

![screen3](/docs/images/soptions.png)

All the options are checked by default but allow you to configure to optout of any.

For example: If you want to retain all the partition keys and indexes then you can keep the indexing policies and Partition keys check boxes checked. Uncheck these boxes if you do not want them to be copied.

If you do not want any of the documents to be copied but just a shell of the collection with similar settings, you can uncheck the Documents check box.

As you can observe the other check boxes for Stored procedures, User defined functions and Triggers all deal with copying code segments from collection to collection.

In the next page we move onto the Anonymization process. We will leave the anonymization discussion to the next section. For now, you can click next and initiate the cloning of the collection.

![screen7](/docs/images/sprogress1.png)

![screen8](/docs/images/scomplete.png)

Explore the cosmos portal and one can observe the new collection created with the required settings.

## Anonymize data of a cosmos collection

Post selection of cloning options as seen in the previous section, we see the below page 

![screen4](/docs/images/sAnonymize.png)

Here we can enter the rules and attribute details that need to be masked or sanitized. 

To add a rule, click on the “Add Rule” button, a mini form to enter details is displayed.

A rule is an encapsulation of an attribute and the anonymization to be performed on it. A rule tells the Cosmic Clone tool what attribute to scrub and how.

The ‘Attribute to scrub’ represents the field that needs to scrubbed\anonymized. 

The ‘Filter Query’ represents the where condition based on which this rule will be applied to various documents. If this rule must be applied to all documents, then leave this field as blank.

The ‘Scrub Type’ field provides options such as 
*	Single value: Replace the attribute value with a fixed value
*	Null Value: Empty the attribute content.
*	Shuffle: Random shuffle the attribute values across all documents of the collection.


**Sample rule1**

![screen5](/docs/images/sRule1.png)

This shuffles the Full name attribute value between all documents. 

**Sample rule2**

![screen6](/docs/images/sRule2.png)

To update key values of the Nested Entities you can configure an anonymization rule as above. Note the Filter Query that tells the tool to perform this operation only if the EntityType attribute of the document is an “Individual”. 

Note there are options on the anonymization screen to validate, save and load these rules

**Migration screen**

![screen7](/docs/images/sprogress1.png)

**Completion notification**

![screen8](/docs/images/scomplete.png)


**Before and After anonymization**

![screen9](/docs/images/BeforeAfter.JPG)

As can be inferred from above, documents will be sanitized based on rules.

### Todos

 - Adapt to other Cosmos API like Graph and Cassandra apart from SQL API
 - Parellelize read and write to improve efficiency
 - Add anonymization option to mask with random values (predefined patterns and regular expressions)
 - Refactor some of the UI and utility code to improve maintainability
 - Write more tests

## References
**Static data masking**
https://docs.microsoft.com/en-us/sql/relational-databases/security/static-data-masking?view=sql-server-2017

**Cosmos Data Import tool** 
https://docs.microsoft.com/en-us/azure/cosmos-db/import-data 

**Cosmos Bulk executor tool**
https://docs.microsoft.com/en-us/azure/cosmos-db/bulk-executor-overview 


## Contributing
 [Contribution guidelines for this project](docs/CONTRIBUTING.md)
 
License
----

MIT
