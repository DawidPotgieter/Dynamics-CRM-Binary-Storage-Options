This plugin allows you to move email attachments and note attachments from the CRM database to an external provider, with both Azure Blob storage and Azure File storage providers included. Once installed and configured, the process is transparent to users and the organizationservice.
Also comes with a migration tool for existing binaries.

Here are some features :

* Runs in sandbox mode. Tested with CRM 2011,2013 (if you recompile with older SDK),2015 on premise, Dynamics 365.
* Supports either Azure Blob storage or Azure File storage. Easy to add more providers.  For instance, the (currently) undocumented "Proxy" provider also provides a FileShare option.
* Supports client side (crm server) compression and encryption (v4.1+), in addition to all the server side encryption schemes.
* Automatic failover if using RAGRS blob storage
* Works seamlessly with existing attachments/annotations. If the binary data is available in CRM, that is used as is. If not, it returns the external version.
* Nobody will know, UI/api works exactly as before :)
* Includes a handy migration tool to move existing annotation/attachments out of your CRM db

See Wiki on how to configure setup the different versions and why you'd use said version.
