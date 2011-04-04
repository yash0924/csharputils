﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tamir.SharpSsh.jsch;
using System.Windows.Forms;
using CSharpUtils.VirtualFileSystem.Local;
using System.Threading;

namespace CSharpUtils.VirtualFileSystem.Ssh
{
	public class SftpFileSystem : RemoteFileSystem
	{
		JSch jsch = null;
		Session session = null;
		ChannelSftp csftp = null;
		String RootPath = "";

		override public void Connect(string Host, int Port, string Username, string Password, int timeout = 10000)
		{
			jsch = new JSch();
			//session.setConfig();
			session = jsch.getSession(Username, Host, Port);
			UserInfo ui = new DirectPasswordUserInfo(Password);
			session.setUserInfo(ui);
			session.connect();

			csftp = (ChannelSftp)session.openChannel("sftp");
			csftp.connect();

			//RootPath = csftp.getHome();
			RootPath = "";
		}

		override protected String RealPath(String Path)
		{
			return RootPath + "/" + Path;
		}

		public override void Shutdown()
		{
			if (csftp != null)
			{
				csftp.disconnect();
				csftp = null;
			}

			if (session != null)
			{
				session.disconnect();
				session = null;
			}

			if (jsch != null)
			{
				jsch = null;
			}
		}

		override protected FileSystemEntry.FileTime ImplGetFileTime(String Path)
		{
			FileSystemEntry.FileTime Time = new FileSystemEntry.FileTime();
			var stat = csftp.lstat(RealPath(Path));
			Time.LastAccessTime = stat.getATime();
			Time.CreationTime = stat.getMTime();
			Time.LastWriteTime = stat.getMTime();
			return Time;
		}

		override protected LinkedList<FileSystemEntry> ImplFindFiles(String Path)
		{
			var Items = new LinkedList<FileSystemEntry>();

			foreach (var i in csftp.ls(RealPath(Path)))
			{
				var LsEntry = (Tamir.SharpSsh.jsch.ChannelSftp.LsEntry)i;
				var FileSystemEntry = new FileSystemEntry(this, Path + "/" + LsEntry.getFilename());
				FileSystemEntry.Size = LsEntry.getAttrs().getSize();
				FileSystemEntry.GroupId = LsEntry.getAttrs().getGId();
				FileSystemEntry.UserId = LsEntry.getAttrs().getUId();
				if (LsEntry.getAttrs().isDir()) {
					FileSystemEntry.Type = VirtualFileSystem.FileSystemEntry.EntryType.Directory;
				} else if (LsEntry.getAttrs().isLink()) {
					FileSystemEntry.Type = VirtualFileSystem.FileSystemEntry.EntryType.Link;
				} else {
					FileSystemEntry.Type = VirtualFileSystem.FileSystemEntry.EntryType.File;
				}
				Items.AddLast(FileSystemEntry);
			}

			return Items;
		}

		override public String DownloadFile(String RemoteFile, String LocalFile = null)
		{
			try
			{
				if (LocalFile == null) LocalFile = GetTempFile();
				csftp.get(RemoteFile, LocalFile);
				return LocalFile;
			}
			catch (Exception e)
			{
				throw (new Exception("Can't download sftp file '" + RemoteFile + "' : " + e.Message, e));
			}
		}

		override public void UploadFile(String RemoteFile, String LocalFile)
		{
			try
			{
				csftp.put(LocalFile, RemoteFile);
			}
			catch (Exception e)
			{
				throw (new Exception("Can't upload sftp file '" + RemoteFile + "' : " + e.Message, e));
			}
		}
	}

	class DirectPasswordUserInfo : UserInfo
	{
		String Password;

		public DirectPasswordUserInfo(String Password) { this.Password = Password; }
		public String getPassword() { return Password; }
		public bool promptYesNo(String str) { return true; }
		public String getPassphrase() { return null; }
		public bool promptPassphrase(String message) { return true; }
		public bool promptPassword(String message) { return true; }
		public void showMessage(String message) { MessageBox.Show(message, "SharpSSH", MessageBoxButtons.OK, MessageBoxIcon.Asterisk); }
	}

}