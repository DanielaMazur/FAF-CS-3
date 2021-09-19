﻿using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace CS
{
     /// <summary>
     /// Interaction logic for MainWindow.xaml
     /// </summary>
     public partial class MainWindow : Window
     {
          private string JSONFileData { get; set; }
          public MainWindow()
          {
               InitializeComponent();
          }

          private void ButtonUpload_Click(object sender, RoutedEventArgs e)
          {


               OpenFileDialog fileDialog = new();
               fileDialog.DefaultExt = ".audit";
               var dialogResult = fileDialog.ShowDialog();

               if (dialogResult.HasValue && dialogResult.Value)
               {
                    try
                    {
                         using var sr = new StreamReader(fileDialog.FileName);
                         FileTextBlock.Text = sr.ReadToEnd();
                         ConvertToJson.IsEnabled = true;
                         //Clear TreeView
                         TreeView.Items.Clear();
                         ExportJsonFile.IsEnabled = false;
                    }
                    catch (IOException error)
                    {
                         MessageBox.Show("The file could not be read: " + error.Message, "Error");
                    }
               }
          }

          private void ButtonConvertToJson_Click(object sender, RoutedEventArgs e)
          {
               var textToBeConverted = FileTextBlock.Text;

               var auditDict = ConvertAuditFileDataToDict(textToBeConverted);

               ExportJsonFile.IsEnabled = true;
              
               JSONFileData = Newtonsoft.Json.JsonConvert.SerializeObject(auditDict);
          }

          private Dictionary<string, object> ConvertAuditFileDataToDict(string fileData)
          {
               var custom_items = new List<Dictionary<string, string>>();

               bool isCustmItemParsingStarted = false;
               KeyValuePair<string, string> multilineStringKeyValuePair = new(null, null);

               Dictionary<string, string> activeCutsomItem = new();

               TreeViewItem activeTreeKey = new();

               TreeViewItem DefaultTreeViewItem = new();
               DefaultTreeViewItem.Header = "custom_items";
               TreeView.Items.Add(DefaultTreeViewItem);

               foreach (var textLine in fileData.Split("\n"))
               {
                    if (textLine.StartsWith("#")) continue;

                    if (textLine.Contains("<custom_item>"))
                    {
                         //create a new key which is the index of the item in the collection
                         activeTreeKey = new();
                         activeTreeKey.Header = custom_items.Count;
                         DefaultTreeViewItem.Items.Add(activeTreeKey);

                         isCustmItemParsingStarted = true;
                         continue;
                    }

                    if (textLine.Contains("</custom_item>"))
                    {
                         custom_items.Add(activeCutsomItem);

                         activeCutsomItem = new();
                         isCustmItemParsingStarted = false;
                         continue;
                    }

                    if (multilineStringKeyValuePair.Key != null)
                    {
                         multilineStringKeyValuePair = new(multilineStringKeyValuePair.Key, multilineStringKeyValuePair.Value + textLine);
                         bool isLastLineInStirng = textLine.Trim().EndsWith('"');

                         if (isLastLineInStirng)
                         {
                              activeCutsomItem.Add(multilineStringKeyValuePair.Key, multilineStringKeyValuePair.Value);

                              //set key value pair for the created collection keys
                              TreeViewItem newTreeValue = new();
                              newTreeValue.Header = $"{multilineStringKeyValuePair.Key} : { multilineStringKeyValuePair.Value}";
                              activeTreeKey.Items.Add(newTreeValue);

                              multilineStringKeyValuePair = new(null, null);
                         }
                         continue;
                    }

                    if (isCustmItemParsingStarted)
                    {
                         var keyValuePair = textLine.Split(':', 2);
                         var key = keyValuePair[0].Trim();
                         var value = keyValuePair[1].Trim();

                         if (value.StartsWith('"') && value.Split('"').Length - 1 == 1 && !value.EndsWith('"'))
                         {
                              multilineStringKeyValuePair = new KeyValuePair<string, string>(key, keyValuePair[1]);
                              continue;
                         }
                         activeCutsomItem.Add(key, value);

                         //set key value pair for the created collection keys
                         TreeViewItem newTreeValue = new();
                         newTreeValue.Header = $"{key} : {value}";
                         activeTreeKey.Items.Add(newTreeValue);
                    }
               }

               var dictionaryWithMainKey = new Dictionary<string, object>();
               dictionaryWithMainKey.Add("custom_items", custom_items);

               return dictionaryWithMainKey;
          }

          private void ButtonExportJsonFile_Click(object sender, RoutedEventArgs e)
          {
               this.SaveFile();
          }

          private void SaveFile()
          {
               SaveFileDialog saveFileDialog = new SaveFileDialog();
               if (saveFileDialog.ShowDialog() == true)
               {
                   File.WriteAllText(saveFileDialog.FileName + ".json", JSONFileData);
               }
          }
     }
}
