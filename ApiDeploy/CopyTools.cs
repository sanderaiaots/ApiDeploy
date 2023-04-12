using System.Diagnostics;
using System.IO.Compression;

namespace ApiDeploy;

public class CopyTools {
	private HashSet<string> Ignore;

	public bool IsVerbrose = false;
	public int CopyFileCount = 0;
	public long CopyFileSize = 0;
	public int DeleteFileCount = 0;
	public long DeleteFileSize = 0;

	public CopyTools(string ignore) {
		Ignore = new HashSet<string>(ignore.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);
	}

	public void CleanFolder(string sourceDirectory) {
		DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
		DeleteAll(diSource, 0);
	}

	public void Copy(string sourceDirectory, string targetDirectory) {
		if (sourceDirectory.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) {
			//zip file.. lets extract
			using (ZipArchive archive = ZipFile.OpenRead(sourceDirectory))

				foreach (ZipArchiveEntry entry in archive.Entries) {
					if (!Ignore.Contains(entry.Name)) {
						bool isDir = string.IsNullOrEmpty(entry.Name) && entry.FullName.EndsWith("/");
						// Gets the full path to ensure that relative segments are removed.
						string destinationPath = Path.GetFullPath(Path.Combine(targetDirectory, entry.FullName));
						if (IsVerbrose) {
							Console.WriteLine(destinationPath);
						}

						if (isDir) {
							DirectoryInfo dir = new DirectoryInfo(destinationPath);
							dir.Create();
						}
						else {
							// Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
							// are case-insensitive.
							entry.ExtractToFile(destinationPath);
							CopyFileSize += entry.Length;
							CopyFileCount++;
						}
					}
				}
		}
		else {
			DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
			DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);
			CopyAll(diSource, diTarget, 0);
		}
	}

	public void CopyAll(DirectoryInfo source, DirectoryInfo target, int level) {
		Directory.CreateDirectory(target.FullName);

		// Copy each file into the new directory.
		foreach (FileInfo fi in source.GetFiles()) {
			if (Ignore.Contains(fi.Name)) {
				//Ignore
			}
			else {
				if (IsVerbrose) {
					Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
				}

				fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
				CopyFileCount++;
				CopyFileSize += fi.Length;
			}
		}

		// Copy each subdirectory using recursion.
		foreach (DirectoryInfo diSourceSubDir in source.GetDirectories()) {
			if (!Ignore.Contains(diSourceSubDir.Name)) {
				DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
				CopyAll(diSourceSubDir, nextTargetSubDir, level + 1);
			}
		}
	}

	public void DeleteAll(DirectoryInfo source, int level) {
		// Copy each file into the new directory.
		foreach (FileInfo fi in source.GetFiles()) {
			if (Ignore.Contains(fi.Name)) {
				//Ignore
			}
			else {
				if (IsVerbrose) {
					Console.WriteLine(@"Delete {0}", fi.FullName);
				}

				DeleteFileSize += fi.Length;

				fi.Delete();
				DeleteFileCount++;
			}
		}

		// Copy each subdirectory using recursion.
		foreach (DirectoryInfo diSourceSubDir in source.GetDirectories()) {
			if (!Ignore.Contains(diSourceSubDir.Name)) {
				DeleteAll(diSourceSubDir, level + 1);
				diSourceSubDir.Delete();
			}
		}
	}

	public static void Backup(string backupFolder, string folderToBackup, string appName) {
		Stopwatch sw = Stopwatch.StartNew();
		DirectoryInfo dir = new DirectoryInfo(GetBackupFolder(backupFolder));
		if (!dir.Exists) {
			dir.Create();
			Console.WriteLine("Created backupFolder " + dir.FullName);
		}
		
		string backupFileName = CreateBackupFile(backupFolder, appName, null);
		int i = 0;
		while (File.Exists(backupFileName)) {
			backupFileName = CreateBackupFile(backupFolder, appName, ++i);
			if (i > 100) {
				Console.WriteLine("Too many backup files. Abort. Last try: " + backupFileName);
				return;
			}
		}
		ZipFile.CreateFromDirectory(folderToBackup, backupFileName);
		Console.WriteLine($"BackupDone {folderToBackup}->{backupFileName} elapsed={sw.ElapsedMilliseconds}ms");
	}

	private static string GetBackupFolder(string backupFolder) {
		return $"{backupFolder}{Path.DirectorySeparatorChar}{DateTime.Now:yyyyMMdd}";
	}
	
	private static string CreateBackupFile(string backupFolder, string appName, int? idx) {
		string sidx = "";
		if (idx != null) {
			sidx = "_" + idx;
		}
		string backupFileName = $"{GetBackupFolder(backupFolder)}{Path.DirectorySeparatorChar}{appName}_backup_{DateTime.Now:yyyyMMdd}{sidx}.zip";
		return backupFileName;
	}
}