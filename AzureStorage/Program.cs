using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types


namespace AzureStorage
{
    class Program
    {
        static void Main(string[] args)
        {
            // connect();
            //list_blobs();
            //download_blobs();
            //delete_blob();
        }
        private static void appenblob()
        {
            //Parse the connection string for the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                Microsoft.Azure.CloudConfigurationManager.GetSetting("StorageConnectionString"));

            //Create service client for credentialed access to the Blob service.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            //Get a reference to a container.
            CloudBlobContainer container = blobClient.GetContainerReference("my-append-blobs");

            //Create the container if it does not already exist.
            container.CreateIfNotExists();

            //Get a reference to an append blob.
            CloudAppendBlob appendBlob = container.GetAppendBlobReference("append-blob.log");

            //Create the append blob. Note that if the blob already exists, the CreateOrReplace() method will overwrite it.
            //You can check whether the blob exists to avoid overwriting it by using CloudAppendBlob.Exists().
            appendBlob.CreateOrReplace();

            int numBlocks = 10;

            //Generate an array of random bytes.
            Random rnd = new Random();
            byte[] bytes = new byte[numBlocks];
            rnd.NextBytes(bytes);

            //Simulate a logging operation by writing text data and byte data to the end of the append blob.
            for (int i = 0; i < numBlocks; i++)
            {
                appendBlob.AppendText(String.Format("Timestamp: {0:u} \tLog Entry: {1}{2}",
                    DateTime.UtcNow, bytes[i], Environment.NewLine));
            }

            //Read the append blob to the console window.
            Console.WriteLine(appendBlob.DownloadText());

        }
        async public static Task ListBlobsSegmentedInFlatListing(CloudBlobContainer container)
        {
            //List blobs to the console window, with paging.
            Console.WriteLine("List blobs in pages:");

            int i = 0;
            BlobContinuationToken continuationToken = null;
            BlobResultSegment resultSegment = null;

            //Call ListBlobsSegmentedAsync and enumerate the result segment returned, while the continuation token is non-null.
            //When the continuation token is null, the last page has been returned and execution can exit the loop.
            
            {
                //This overload allows control of the page size. You can return all remaining results by passing null for the maxResults parameter,
                //or by calling a different overload.
                resultSegment = await container.ListBlobsSegmentedAsync("", true, BlobListingDetails.All, 10, continuationToken, null, null);
                if (resultSegment.Results.Count<IListBlobItem>() > 0) { Console.WriteLine("Page {0}:", ++i); }
                foreach (var blobItem in resultSegment.Results)
                {
                    Console.WriteLine("\t{0}", blobItem.StorageUri.PrimaryUri);
                }
                Console.WriteLine();

                //Get the continuation token.
                continuationToken = resultSegment.ContinuationToken;
            }
            while (continuationToken != null);
        }
        private static void delete_blob()
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("azurecontainer1");

            // Retrieve reference to a blob named "myblob.txt".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("cms_administrationguide.pdf");

            // Delete the blob.
            blockBlob.Delete();

        }
        private static void list_blobs()
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("azurecontainer1");

            // Loop over items within the container and output the length and URI.
            foreach (IListBlobItem item in container.ListBlobs(null, false))
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob blob = (CloudBlockBlob)item;

                    Console.WriteLine("Block blob of length {0}: {1}", blob.Properties.Length, blob.Uri);

                }
                else if (item.GetType() == typeof(CloudPageBlob))
                {
                    CloudPageBlob pageBlob = (CloudPageBlob)item;

                    Console.WriteLine("Page blob of length {0}: {1}", pageBlob.Properties.Length, pageBlob.Uri);

                }
                else if (item.GetType() == typeof(CloudBlobDirectory))
                {
                    CloudBlobDirectory directory = (CloudBlobDirectory)item;

                    Console.WriteLine("Directory: {0}", directory.Uri);
                }
            }

        }
        private static void connect()
        {// Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("azurecontainer1");

            // Retrieve reference to a blob named "myblob".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("azureblob");

            // Create or overwrite the "myblob" blob with contents from a local file.
            using (var fileStream = System.IO.File.OpenRead(@"AzureTextFile.txt"))
            {
                blockBlob.UploadFromStream(fileStream);
            }

            container.CreateIfNotExists();
        }

        private static void download_blobs()
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("azurecontainer1");

            // Retrieve reference to a blob named "cms_administrationguide.pdf".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("cms_administrationguide.pdf");

            // Save blob contents to a file.
            using (var fileStream = System.IO.File.OpenWrite(@"vishal.pdf"))
            {
                blockBlob.DownloadToStream(fileStream);
            }

        }
    }
}
