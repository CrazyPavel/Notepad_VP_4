using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using NotePad.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using static NotePad.Models.FileTypes;
using static System.Net.Mime.MediaTypeNames;
using Path = System.IO.Path;

namespace NotePad.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ObservableCollection<FileItem> fileList = new();
        public ObservableCollection<FileItem> FileList { get => fileList; }

        private string cur_dir = "";

        private void LoadDisks()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            string system = Environment.GetFolderPath(Environment.SpecialFolder.System);
            string? sys_d = Path.GetPathRoot(system);
            fileList.Clear();
            foreach (DriveInfo drive in drives)
                fileList.Add(new FileItem(drive.Name == sys_d ? SysDrive : Drive, drive.Name));
        }
        private void LoadDir()
        {
            DirectoryInfo directory = new(cur_dir);
            Sravn nc = new();

            List<string> dirs = new();
            foreach (var file in directory.GetDirectories()) dirs.Add(file.Name);
            dirs.Sort(nc);

            List<string> files = new();
            foreach (var file in directory.GetFiles()) files.Add(file.Name);
            files.Sort(nc);

            fileList.Clear();
            fileList.Add(new FileItem(BackFolder, ".."));
            foreach (var name in dirs) fileList.Add(new FileItem(Folder, name));
            foreach (var name in files) fileList.Add(new FileItem(FILE, name));
        }
        private void Loader(bool start = false)
        {
            FileBox = "";
            if (start) cur_dir = Directory.GetCurrentDirectory();
            if (cur_dir == "") LoadDisks();
            else LoadDir();
        }
        private void UpdButtonMode()
        {
            if (openMode) return;

            string path = Path.Combine(cur_dir, fileBox);
            if (!File.Exists(path))
            {
                ButtonMode = "Открыть";
                return;
            }

            var attrs = File.GetAttributes(path);
            bool file_rpov = (attrs & FileAttributes.Archive) != 0;
            ButtonMode = file_rpov ? "Сохранить" : "Открыть";
        }

        private bool explorerMode = false;
        private bool openMode = false;
        private string buttonMode = "";
        public bool ExplorerMode { get => explorerMode; set => this.RaiseAndSetIfChanged(ref explorerMode, value); }
        public string ButtonMode { get => buttonMode; set => this.RaiseAndSetIfChanged(ref buttonMode, value); }

        private void FuncOpen()
        {
            if (explorerMode) return;
            ExplorerMode = true;
            Loader(true);
            ButtonMode = "Открыть";
            openMode = true;
        }
        private void FuncSave()
        {
            if (explorerMode) return;
            ExplorerMode = true;
            Loader(true);
            ButtonMode = "Открыть";
            openMode = false;
        }

        private void FuncOk()
        {
            DoubleTap();
        }

        private void FuncCancel()
        {
            if (!explorerMode) return;
            ExplorerMode = false;
        }

        private void SelectItem(FileItem item)
        {
            if (item == null) return;
            FileBox = item.Name;
            UpdButtonMode();
        }

        private void Message(string msg)
        {
            if (!fileBox.StartsWith(msg)) FileBox = msg + fileBox;
        }

        public void DoubleTap()
        {
            if (!explorerMode) return;

            if (fileBox == "..")
            {
                var parentDir = Directory.GetParent(cur_dir);
                cur_dir = parentDir == null ? "" : parentDir.FullName;
                Loader();
                return;
            }

            if (cur_dir == "")
            {
                if (Directory.Exists(fileBox))
                {
                    cur_dir = fileBox;
                    Loader();
                }
                else Message("Нет такого диска: ");
                return;
            }

            string path = Path.Combine(cur_dir, fileBox);
            FileAttributes attrs;

            try
            {
                attrs = File.GetAttributes(path);
            }
            catch (IOException)
            {
                if (openMode) Message("Нет такой папки/файла: ");
                else
                { // saveMode
                    File.WriteAllText(path, contentBox);
                    ExplorerMode = false;
                }
                return;
            }

            bool Dir = (attrs & FileAttributes.Directory) != 0;
            bool file_rpov = (attrs & FileAttributes.Archive) != 0;

            if (Dir)
            {
                cur_dir = path;
                Loader();
            }
            else if (file_rpov)
            {
                if (openMode)
                {
                    ContentBox = File.ReadAllText(path);
                    ExplorerMode = false;
                }
                else
                {
                    File.WriteAllText(path, contentBox);
                    ExplorerMode = false;
                }
            }
        }

        string contentBox = "";
        string fileBox = "";
        FileItem selectedItem = new(FILE, "?");

        public string ContentBox { get => contentBox; set => this.RaiseAndSetIfChanged(ref contentBox, value); }
        public string FileBox { get => fileBox; set => this.RaiseAndSetIfChanged(ref fileBox, value); }
        public FileItem SelectedItem { get => selectedItem; set { selectedItem = value; SelectItem(value); } }

        public MainWindowViewModel()
        {
            Open = ReactiveCommand.Create<Unit, Unit>(_ => { FuncOpen(); return new Unit(); });
            Save = ReactiveCommand.Create<Unit, Unit>(_ => { FuncSave(); return new Unit(); });
            Ok = ReactiveCommand.Create<Unit, Unit>(_ => { FuncOk(); return new Unit(); });
            Cancel = ReactiveCommand.Create<Unit, Unit>(_ => { FuncCancel(); return new Unit(); });
        }

        public ReactiveCommand<Unit, Unit> Open { get; }
        public ReactiveCommand<Unit, Unit> Save { get; }
        public ReactiveCommand<Unit, Unit> Ok { get; }
        public ReactiveCommand<Unit, Unit> Cancel { get; }
    }
}