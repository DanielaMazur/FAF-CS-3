using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace CS
{
     /// <summary>
     /// Interaction logic for MainWindow.xaml
     /// </summary>
     public partial class MainWindow : Window
     {
          private string _importedFileContents;
          private List<Dictionary<string, string>> CutsomItemsDictList { get; set; }
          public MainWindow()
          {
               InitializeComponent();
               Resources["CutsomItemsDictList"] = new List<Dictionary<string, string>>();
          }

          private void OpenAuditFile(object sender, RoutedEventArgs e)
          {
               OpenFileDialog fileDialog = new();
               fileDialog.Filter = "Audit files|*.audit";
               var dialogResult = fileDialog.ShowDialog();

               if (dialogResult.HasValue && dialogResult.Value)
               {
                    try
                    {
                         using var sr = new StreamReader(fileDialog.FileName);
                         _importedFileContents = sr.ReadToEnd();
                         CutsomItemsDictList = this.ConvertToDict(_importedFileContents);
                         Resources["CutsomItemsDictList"] = CutsomItemsDictList;
                    }
                    catch (IOException error)
                    {
                         MessageBox.Show("The file could not be read: " + error.Message, "Error");
                    }
               }
          }

          private void SaveAuditFile(object sender, RoutedEventArgs e)
          {
               SaveFileDialog saveFileDialog = new();
               saveFileDialog.DefaultExt = ".audit";
               if (saveFileDialog.ShowDialog() == true)
               {
                    File.WriteAllText(saveFileDialog.FileName, _importedFileContents);
               }
          }
          private void SaveJSONFile(object sender, RoutedEventArgs e)
          {
               SaveFileDialog saveFileDialog = new();
               var JSONFileData = Newtonsoft.Json.JsonConvert.SerializeObject(CutsomItemsDictList);
               if (saveFileDialog.ShowDialog() == true)
               {
                    File.WriteAllText(saveFileDialog.FileName + ".json", JSONFileData);
               }
          }

          private List<Dictionary<string, string>> ConvertToDict(string fileData)
          {
               var customItemPattern = @"<custom_item>([\S\s]*?)</custom_item>";
               var customItemKeyValuePattern = @"^[\s]+([a-z_]+)\s+:\s+((""[\S\s\n]*?"")|([^\n]+))";

               Regex regexCustomItemPattern = new(customItemPattern);
               Regex regexCustomItemKeyValuePattern = new(customItemKeyValuePattern, RegexOptions.Multiline);

               var customItems = regexCustomItemPattern.Matches(fileData);

               var parsedCustomItems = new List<Dictionary<string, string>>();

               foreach (var customItem in customItems)
               {
                    var keyValuePairs = regexCustomItemKeyValuePattern.Matches(customItem.ToString());
                    var keysValuesDict = new Dictionary<string, string>();

                    foreach (var keyValuePair in keyValuePairs)
                    {
                         var keyValueArray = keyValuePair.ToString().Split(":", 2);
                         var key = keyValueArray[0].Trim();
                         var value = keyValueArray[1].Trim();

                         keysValuesDict.Add(key, value);
                    }
                    parsedCustomItems.Add(keysValuesDict);
               }

               return parsedCustomItems;
          }
     }
}
