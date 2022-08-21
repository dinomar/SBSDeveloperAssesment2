using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.Diagnostics;
using System.Collections.ObjectModel;
using SBSDeveloperAssesment2.Models;

namespace SBSDeveloperAssesment2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<FileBuildInfo> _files = new List<FileBuildInfo>();
        private readonly IList<FileBuildInfo> _filteredFiles = new ObservableCollection<FileBuildInfo>();
        private readonly IList<string> _filterList = new ObservableCollection<string>()
        {
            "System",
            "Microsoft"
        };

        public MainWindow()
        {
            InitializeComponent();
            dataGrid.ItemsSource = _filteredFiles;
            filterListbox.ItemsSource = _filterList;
        }

        private void btnOpenDirectory_Click(object sender, RoutedEventArgs e)
        {
            _files.Clear();

            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                var result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    if (!Directory.Exists(dialog.SelectedPath))
                    {
                        MessageBox.Show("Selected directory doesn't exist!");
                    }

                    List<string> files = new List<string>();
                    populateFilesList(files, dialog.SelectedPath);

                    var filteredFiles = files.Where(f => f.ToLower().EndsWith("dll") || f.ToLower().EndsWith("exe"));

                    foreach (var file in filteredFiles)
                    {
                        var versionInfo = FileVersionInfo.GetVersionInfo(file);
                        if (!string.IsNullOrEmpty(versionInfo.OriginalFilename))
                        {
                            _files.Add(new FileBuildInfo
                            {
                                FileName = versionInfo.OriginalFilename,
                                MajorVersion = versionInfo.FileMajorPart,
                                MinorVersion = versionInfo.FileMinorPart,
                                BuildVersion = versionInfo.FileBuildPart
                            });
                        }
                    }
                }
            }

            applyGridFilter();
        }

        private void applyGridFilter()
        {
            _filteredFiles.Clear();

            // Filter
            foreach (FileBuildInfo info in _files)
            {
                bool exclude = false;
                foreach (string filter in _filterList)
                {
                    if (info.FileName.Contains(filter))
                    {
                        exclude = true;
                        break;
                    }
                }

                if (!exclude)
                {
                    _filteredFiles.Add(info);
                }
            }

            // Sort
            var sorted = _filteredFiles.OrderBy(i => i.BuildVersion).ToList();
            _filteredFiles.Clear();
            sorted.ForEach(i => _filteredFiles.Add(i));
        }

        private void populateFilesList(List<string> filesList, string directory)
        {
            foreach (string dir in Directory.GetDirectories(directory))
            {
                populateFilesList(filesList, dir);
            }

            filesList.AddRange(Directory.GetFiles(directory));
        }

        private void btnRemoveFilter_Click(object sender, RoutedEventArgs e)
        {
            if (filterListbox.SelectedIndex >= 0)
            {
                string selectedItem = filterListbox.Items[filterListbox.SelectedIndex].ToString();
                _filterList.Remove(selectedItem);
            }

            applyGridFilter();
        }

        private void btnAddFilter_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtBoxFilter.Text))
            {
                _filterList.Add(txtBoxFilter.Text);
            }

            applyGridFilter();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
