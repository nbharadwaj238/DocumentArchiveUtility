# DocumentArchiveUtility
Document Archive utility can automatically archive in-active customer documents from on premise server to cloud to create effortless capacity expansion & to secure customer data. This solution will shift developer / admin focus from Infrastructure management (capacity purchase) to meeting business needs. 

Document Archive Utility used Azure blob storage solution to store customer documents on cloud, Azure storage services runs in a high-availability environment (data centers), patched and supported by Microsoft.

Document Archive Utility will compressed and encrypt the files(documents) before uploading to Azure Blob Storage, Also will decrypt and un-compressed files while downloading from Azure Storage.

Once a document uploaded successfully, DAU will delete this document from physic disk (on premise server).

#Technologies & Services
1. .NET CORE 5
2. Azure Key Vault
3. Azure Blob Storage
4. Azure Managed Identity
5. Azure Active Directory

# Best Practice
1.	To save money on azure blob storage costs, Use Azure Storage reserved capacity.
2.	Enable Azure roll back access over storage account.
3.	Enable authentication using Azure managed identity.
4.	Use Azure Keyvalult to store secretes, key and certificates.
5.	Try to avoid, storage account access using access keys – if still required use azure keyvalut to store access keys.
6.	To share access with 3rd party always use, Azure AD based OAuth 2.0.

# Other Point to Consider – 
1.	Enable - Soft delete for blobs enables you to recover blob data after it has been deleted. 
2.	Enable - Soft delete for containers enables you to recover a container after it has been deleted
3.	Lock storage account - to prevents the account itself from being deleted
4.	Configure - Configure time-based retention policies to store blob data in a WORM (Write Once, Read Many) state. 
5.	Access - Over HTTPS protocol only - Reject http.
6.	Limit storage account access using firewall rules. (Can configure IP based rules).
7.	Enable Azure Storage logging 
8.	Configure log alerts to evaluate resources logs 
9.	Enable auditing for Azure Storage Account. 

 # Note –
1.	Azure storage pricing is not based on number of account, so we can create multiple account as per our app service plan limit. Azure storage pricing is based on region, type, performance tier, storage account type, and redundancy and access tier. 
2.	Archive tier will take less than 15 hours to provide data from azure storage.


 
