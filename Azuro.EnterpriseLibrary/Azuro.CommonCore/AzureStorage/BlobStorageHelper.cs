using Azure.Identity;
using Azure.Storage.Blobs;
using Azuro.Common.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Azuro.Common.AzureStorage
{
	/// <summary>
	/// A wrapper class for working with Azure Blobs
	/// </summary>
	/// <typeparam name="TConfig">Pass in a configuration object for this blob container.</typeparam>
	public class BlobStorageHelper<TConfig> where TConfig : class, IBlobStorageOptions
	{
		private readonly TConfig _options;
		private readonly ILogger<BlobStorageHelper<TConfig>> _logger;
		private BlobContainerClient _containerClient;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="options">The IOptions wrapper for the Blob Configuration.</param>
		/// <param name="logger">A reference to the logger.</param>
		public BlobStorageHelper(IOptions<TConfig> options, ILogger<BlobStorageHelper<TConfig>> logger)
		{
			_options = options?.Value;
			_logger = logger;
		}

		/// <summary>
		/// Upload a base64 encoded string to the path specified.
		/// </summary>
		/// <param name="path">The path to the blob file.</param>
		/// <param name="content">The base64 encoded content to upload as a string.</param>
		/// <returns></returns>
		public async Task UploadBase64Async(string path, string content, bool overwrite = true)
		{
			await UploadAsync(path, Convert.FromBase64String(content.EnsureBase64()), overwrite);
		}

		/// <summary>
		/// Upload a byte array to the path specified.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="fileBytes"></param>
		/// <returns></returns>
		public async Task UploadAsync(string path, byte[] fileBytes, bool overwrite = true)
		{
			using var stream = new MemoryStream(fileBytes, writable: false);
			await UploadAsync(path, stream);
		}

		/// <summary>
		/// Upload a Stream to the path specified.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="stream"></param>
		/// <returns></returns>
		public async Task UploadAsync(string path, Stream stream, bool overwrite = true)
		{
			try
			{
				var client = await CreateBlobContainerClientAsync();
				if (overwrite)
					_ = await client.DeleteBlobIfExistsAsync(path);    //	TODO: Snapshot Options should be made available in config
				await client.UploadBlobAsync(path, stream);
			}
			catch (Exception ex)
			{
				LogError($"Exception [{nameof(BlobStorageHelper<TConfig>)}]: {ex.Message}");
				throw;
			}
		}

		/// <summary>
		/// Return an IAsyncEnumerable list of string paths to blobs in the container.
		/// </summary>
		/// <returns></returns>
		public async IAsyncEnumerable<string> ListBlobsAsync()
		{
			var client = await CreateBlobContainerClientAsync();
			//	TODO: Check implementation of AsyncPageable for certainty
			await foreach (var b in client.GetBlobsAsync())
				yield return b.Name;
		}

		/// <summary>
		/// Return the Blob as a Stream.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public async Task<Stream> GetBlobStreamAsync(string path)
		{
			try
			{
				var blobClient = await CreateBlobClientAsync(path);
				return await blobClient.OpenReadAsync();
			}
			catch (Exception ex)
			{
				LogError($"Exception [{nameof(BlobStorageHelper<TConfig>)}]: {ex.Message}");
				return null;
			}
		}

		/// <summary>
		/// Return the blob as a Base64 encoded string.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public async Task<string> GetBlobBase64Async(string path)
		{
			return await (await GetBlobBytesAsync(path)).ToBase64Async();
		}

		/// <summary>
		/// Return the blob as a Base64 encoded image string
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public async Task<string> GetBlobBase64ImageAsync(string path)
		{
			try
			{
				var imageExtension = Path.GetExtension(path).Replace(".", "").ToLower();
				return $"data:image/{imageExtension};base64,{await GetBlobBase64Async(path)}";
			}
			catch (Exception ex)
			{
				LogError($"Exception [{nameof(BlobStorageHelper<TConfig>)}]: {ex.Message}");
				return null;
			}
		}

		/// <summary>
		/// Get the blob as an array of bytes.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public async Task<byte[]> GetBlobBytesAsync(string path)
		{
			using var ms = new MemoryStream();
			await (await GetBlobStreamAsync(path)).CopyToAsync(ms);
			return ms.ToArray();
		}


		public async Task<bool> CopyFileAsync(string fromPath, string toPath)
		{
			try
			{
				var blobSource = await CreateBlobClientAsync(fromPath);
				if (await blobSource.ExistsAsync())
				{
					var blobTarget = await CreateBlobClientAsync(toPath);
					_ = await blobTarget.StartCopyFromUriAsync(blobSource.Uri);
					return true;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"Exception [{nameof(BlobStorageHelper<TConfig>)}.{nameof(CopyFileAsync)}]: {ex.Message}");
			}

			return false;
		}

		private async Task<BlobContainerClient> CreateBlobContainerClientAsync()
		{
			//	Re-use the client if there are multiple calls in the same scope.
			if (_containerClient == null)
			{
				//	AzureCredentials implies using a ServiceAccount assigned to the caller, else use embedded SAS token
				BlobServiceClient serviceClient = _options.UseDevelopment
					? (new("UseDevelopmentStorage=true"))
					: _options.AzureCredentials
					? new(new Uri(_options.Connection), new DefaultAzureCredential())
					: new(_options.Connection);

				//	Get the container client and return it if valid
				var containerClient = serviceClient.GetBlobContainerClient(_options.ContainerName);
				//	Create container if it doesn't already exist
				_ = await containerClient.CreateIfNotExistsAsync(_options.IsPublic ? Azure.Storage.Blobs.Models.PublicAccessType.BlobContainer : Azure.Storage.Blobs.Models.PublicAccessType.None);

				//	Set the client up if the blob container exists
				_containerClient = await containerClient.ExistsAsync()
					? containerClient
					: throw new AzuroStorageException($"Unable to connect to Blob: {(_options.UseDevelopment ? "Development" : _options.Connection)}/{_options.ContainerName}");
			}

			return _containerClient;
		}

		private async Task<BlobClient> CreateBlobClientAsync(string path)
		{
			var client = await CreateBlobContainerClientAsync();
			return client.GetBlobClient(path);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "Using string interpolation for message formatting")]
		private void LogError(string message)
		{
			_logger.LogError(message);
		}
	}
}