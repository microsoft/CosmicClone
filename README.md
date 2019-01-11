# Cosmic Clone
1. [Overview](#overview)
1. [Prerequisites](#prerequisites)
1. [Deployment Steps](#deployment-steps)
1. [Screens](#screens)
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

## Prerequisites
- Azure Cosmos SQL API DB Source Collection with Data(read connection keys)
- Azure Cosmos Destination account(read write connection keys)


## Deployment Steps
1. Just Compile and Run the Code.
2. For Best performance you can run the compiled code in an Azure VM that is in the same region as the source and destination Cosmos Colleciton.


## Screens

**Initial screen**

![screen1](/docs/images/sinitial.png)

**Enter Source connection details**

![screen2](/docs/images/sinitialDetails.png)


**Set migration options**

![screen3](/docs/images/soptions.png)


**Configure anonymization rules**

![screen4](/docs/images/sAnonymize.png)

**Sample rule1**

![screen5](/docs/images/sRule1.png)

**Sample rule2**

![screen6](/docs/images/sRule2.png)


Note there are options to validate, save and load these rules

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


## Contributing
 [Contribution guidelines for this project](docs/CONTRIBUTING.md)
 
License
----

MIT
