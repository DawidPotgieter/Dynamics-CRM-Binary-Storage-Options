This plugin allows you to move email attachments and note attachments from the CRM database to an external provider, with both Azure Blob storage and Azure File storage providers included. Once installed and configured, the process is transparent to users and the organizationservice.
Also comes with a migration tool for existing binaries.

Here are some features :

* Runs in sandbox mode. Tested with CRM2015 on premise, and I know it works with CRM online (2016). Might also work down to CRM2011 if you recompile it against the 2011 sdk.
* Supports either Azure Blob storage or Azure File storage. Easy to add more providers
* Supports client side (crm server) compression and encryption (v4.1+).
* Works seamlessly with existing attachments/annotations. If the binary data is available in CRM, that is used
* Nobody will know, UI/api works exactly as before :)

See Wiki on how to configure setup the different versions and why you'd use said version.

The old project with previous versions is still avialable while CodePlex allows here :

[https://dynamicscrmbinarystorageoptions.codeplex.com/]
