﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BibliotecaMusical.Models;

namespace BibliotecaMusical.Services {
	public class LibraryService {
		private const string CONTAINER_NAME = "bibliotecamusical";
		private const string DELETE_QUEUE_NAME = "bibliotecamusicaldeletelog";

		public static void SaveFile(string fileName, Stream stream, string userEmail) {
			AzureService.SaveBlob(CONTAINER_NAME, fileName, stream);

			var userActionModel = new UserActionModel {
				Email = userEmail,
				Description = $"User [{userEmail}] added the file [{fileName}] to the Library."
			};
			UserActionService.SaveUserAction(userActionModel);
		}

		public static List<BlobModel> GetFileList() {
			var fileList = AzureService.GetBlobList(CONTAINER_NAME)
				.Select(b => new BlobModel {
					Name = b.Name,
					Size = Convert.ToDecimal((double)b.Properties.Length / 1024 / 1024).ToString("#,##0.00")
				})
				.ToList();

			return fileList;
		}

		public static byte[] GetFileData(string fileName) {
			byte[] fileData = null;
			var blob = AzureService.GetBlob(CONTAINER_NAME, fileName);
			blob.DownloadToByteArray(fileData, 0);

			return fileData;
		}

		public static void DeleteFile(string fileName, string userEmail) {
			//AzureService.DeleteBlob(CONTAINER_NAME, fileName);
			AzureService.SaveMessage(DELETE_QUEUE_NAME, fileName);

			var userActionModel = new UserActionModel {
				Email = userEmail,
				Description = $"User [{userEmail}] deleted the file [{fileName}] to the Library."
			};
			UserActionService.SaveUserAction(userActionModel);
		}
	}
}