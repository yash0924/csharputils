﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CSharpUtils.VirtualFileSystem
{
	public class ProxyReadOnlyFilesystem : ProxyFileSystem
	{
		public ProxyReadOnlyFilesystem(FileSystem ParentFileSystem) : base(ParentFileSystem)
		{
		}

		internal override FileSystemFileStream ImplOpenFile(string FileName, System.IO.FileMode FileMode)
		{
			switch (FileMode)
			{
				case FileMode.Open:
					return base.ImplOpenFile(FileName, FileMode);
				default:
					throw(new NotImplementedException());
			}
		}

		sealed internal override void ImplDeleteFile(string Path)
		{
			throw(new NotImplementedException());
		}

		sealed internal override void ImplMoveFile(string ExistingFileName, string NewFileName, bool ReplaceExisiting)
		{
			throw (new NotImplementedException());
		}

		sealed internal override void ImplWriteFile(FileSystemFileStream FileStream, byte[] Buffer, int Offset, int Count)
		{
			throw (new NotImplementedException());
		}
	}
}
