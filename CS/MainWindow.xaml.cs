using Fare;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
                         if (item.IsChecked) sb.Append(item.ToString());
                    }
                    File.WriteAllText(saveFileDialog.FileName, sb.ToString());
               }
          }
          private void SaveJSONFile(object sender, RoutedEventArgs e)
          {
               SaveFileDialog saveFileDialog = new();
               var JSONFileData = JsonConvert.SerializeObject(FilteredCustomItemsList.Where(item => item.IsChecked).Select(item => item.Properties));
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

          private void AuditCheckedItems(object sender, RoutedEventArgs e)
          {
               var checkedCustomItems = CustomItemsList.Where((item) => item.IsChecked);
               foreach (var customItem in checkedCustomItems)
               {
                    if (customItem.Properties.TryGetValue("type", out string customItemType) && customItemType == "REGISTRY_SETTING")
                    {
                         if (customItem.Properties.TryGetValue("reg_key", out string registryKey))
                         {
                              var registryPath = registryKey.Replace("HKLM\\", "").Replace("\"", "");
                              var key = Registry.LocalMachine.OpenSubKey(registryPath);
                              if (key == null)
                              {
                                   customItem.AuditStatus = AuditStatusEnum.Fail;
                                   continue;
                              }
                              if (customItem.Properties.TryGetValue("reg_item", out string registryItem))
                              {
                                   var value = Convert.ToString(key.GetValue(registryItem.Replace("\"", "")));
                                   if (value == null)
                                   {
                                        customItem.AuditStatus = AuditStatusEnum.Fail;
                                        continue;
                                   };

                                   if (customItem.Properties.TryGetValue("check_type", out string checkType) && checkType == "CHECK_REGEX")
                                   {
                                        customItem.Properties.TryGetValue("value_data", out string pattern);
                                        Regex regexPattern = new(pattern.Replace("\"", ""));
                                        var isMatching = regexPattern.IsMatch(value);
                                        customItem.AuditStatus = isMatching ? AuditStatusEnum.Success : AuditStatusEnum.Fail;
                                        continue;
                                   }

                                   if (customItem.Properties.TryGetValue("reg_option", out string regOption))
                                   {
                                        switch (regOption.Replace("\"", ""))
                                        {
                                             case "CAN_NOT_BE_NULL":
                                                  customItem.AuditStatus = value != null ? AuditStatusEnum.Success : AuditStatusEnum.Fail;
                                                  break;
                                             case "MUST_NOT_EXIST":
                                                  customItem.AuditStatus = value == null ? AuditStatusEnum.Success : AuditStatusEnum.Fail;
                                                  break;
                                             case "CAN_BE_NULL":
                                                  customItem.AuditStatus = AuditStatusEnum.Success;
                                                  break;
                                        }
                                        continue;
                                   }

                                   customItem.Properties.TryGetValue("value_data", out string valueData);
                                   customItem.AuditStatus = valueData == value ? AuditStatusEnum.Success : AuditStatusEnum.Fail;
                              }
                         }
                    }
               }
          }

          public void EnforceFailedItems(object sender, RoutedEventArgs e)
          {
               var failedCustomItems = CustomItemsList.Where((item) => item.IsChecked && item.AuditStatus == AuditStatusEnum.Fail);
               foreach (var customItem in failedCustomItems)
               {
                    if (customItem.Properties.TryGetValue("type", out string customItemType) && customItemType == "REGISTRY_SETTING")
                    {
                         if (customItem.Properties.TryGetValue("reg_key", out string registryKey))
                         {
                              if (customItem.Properties.TryGetValue("reg_item", out string registryItem))
                              {
                                   customItem.Properties.TryGetValue("value_data", out string expectedValueRegex);
                                   customItem.InitialValue = Convert.ToString(Registry.LocalMachine.OpenSubKey(registryKey).GetValue(registryItem.Replace("\"", "")));

                                   var xeger = new Xeger(expectedValueRegex);
                                   var generatedString = xeger.Generate();

                                   Registry.LocalMachine.SetValue(registryKey, generatedString);
                                   customItem.AuditStatus = AuditStatusEnum.Changed;
                              }
                         }
                    }
               }
          }

          public void Rollback(object sender, RoutedEventArgs e)
          {
               var changedCustomItems = CustomItemsList.Where((item) => item.IsChecked && item.AuditStatus == AuditStatusEnum.Changed);
               foreach (var customItem in changedCustomItems)
               {
                    if (customItem.Properties.TryGetValue("type", out string customItemType) && customItemType == "REGISTRY_SETTING")
                    {
                         if (customItem.Properties.TryGetValue("reg_key", out string registryKey))
                         {
                              Registry.LocalMachine.SetValue(registryKey, customItem.InitialValue);
                              customItem.AuditStatus = AuditStatusEnum.Unprocessed;
                         }
                    }
               }
          }
     }
}