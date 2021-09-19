using Microsoft.Win32;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace CS
{
     /// <summary>
     /// Interaction logic for MainWindow.xaml
     /// </summary>
     public partial class MainWindow : Window
     {
          private List<CustomItem> CustomItemsList;
          private List<CustomItem> FilteredCustomItemsList = new();

          public MainWindow()
          {
               InitializeComponent();
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
                         CustomItemsList = this.ConvertToDict(sr.ReadToEnd());
                         FilteredCustomItemsList = CustomItemsList;
                         Resources["CustomItemsDictList"] = FilteredCustomItemsList;
                         if (CustomItemsList.Count > 0)
                         {
                              BodyContainer.Visibility = Visibility.Visible;
                              EmptyViewText.Visibility = Visibility.Collapsed;
                         }
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
                    StringBuilder sb = new();
                    foreach (var item in FilteredCustomItemsList)
                    {
                         if (item.IsChecked == true) sb.Append(item.ToString());
                    }
                    File.WriteAllText(saveFileDialog.FileName, sb.ToString());
               }
          }
          private void SaveJSONFile(object sender, RoutedEventArgs e)
          {
               SaveFileDialog saveFileDialog = new();
               var JSONFileData = JsonConvert.SerializeObject(FilteredCustomItemsList.Where(item => item.IsChecked == true).Select(item => item.Properties));
               if (saveFileDialog.ShowDialog() == true)
               {
                    File.WriteAllText(saveFileDialog.FileName + ".json", JSONFileData);
               }
          }

          private void HandleAllItemsCheck(object sender, RoutedEventArgs e)
          {
               foreach (var customItem in FilteredCustomItemsList)
               {
                    customItem.IsChecked = true;
               }
          }

          private void HandleAllItemsUnchecked(object sender, RoutedEventArgs e)
          {
               foreach (var customItem in FilteredCustomItemsList)
               {
                    customItem.IsChecked = false;
               }
          }

          private void HandleCheckOneItem(object sender, RoutedEventArgs e)
          {
               var itemToCheck = CustomItemsList.Single(item => item.Id.ToString() == ((CheckBox)sender).Uid);
               itemToCheck.IsChecked = true;
          }

          private void HandleUnCheckOneItem(object sender, RoutedEventArgs e)
          {
               var itemToUnCheck = CustomItemsList.Single(item => item.Id.ToString() == ((CheckBox)sender).Uid);
               itemToUnCheck.IsChecked = false;
          }

          private List<CustomItem> ConvertToDict(string fileData)
          {
               var customItemPattern = @"<custom_item>([\S\s]*?)</custom_item>";
               var customItemKeyValuePattern = @"^[\s]+([a-z_]+)\s+:\s+((""[\S\s\n]*?"")|([^\n]+))";

               Regex regexCustomItemPattern = new(customItemPattern);
               Regex regexCustomItemKeyValuePattern = new(customItemKeyValuePattern, RegexOptions.Multiline);

               var customItems = regexCustomItemPattern.Matches(fileData);

               var parsedCustomItems = new List<CustomItem>();

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
                    var newCustomItem = new CustomItem(parsedCustomItems.Count, keysValuesDict);
                    parsedCustomItems.Add(newCustomItem);
               }

               return parsedCustomItems;
          }

          private void Search_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
          {
               FilteredCustomItemsList = CustomItemsList.Where(item =>
                                          item.Properties.Keys.Select(key => key.ToLower()).Any(key => key.Contains(Search.Text.ToLower())) ||
                                          item.Properties.Values.Select(value => value.ToLower()).Any(value => value.Contains(Search.Text.ToLower()))).ToList();
               NoResultsFound.Visibility = FilteredCustomItemsList.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
               Resources["CustomItemsDictList"] = FilteredCustomItemsList;
          }

          private class CustomItem : INotifyPropertyChanged
          {
               public int Id { get; private set; }
               private bool? _isChecked = true;
               public bool? IsChecked
               {
                    get
                    {
                         return _isChecked;
                    }
                    set
                    {
                         _isChecked = value;
                         NotifyPropertyChanged();
                    }
               }
               public Dictionary<string, string> Properties { get; private set; }
               public CustomItem(int id, Dictionary<string, string> properties)
               {
                    Id = id;
                    Properties = properties;
               }

               public event PropertyChangedEventHandler PropertyChanged;
               private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
               {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
               }
               public override string ToString()
               {
                    StringBuilder sb = new();
                    sb.Append("<custom_item>\n");
                    foreach (var key in Properties.Keys)
                    {
                         sb.Append(key + ":" + Properties[key] + "\n");
                    }
                    sb.Append("</custom_item>\n");
                    return sb.ToString();
               }
          }
     }
}
